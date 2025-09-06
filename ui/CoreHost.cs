#nullable enable

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MirrorAudio.UI
{
public sealed class CoreHost : IDisposable
{
    private readonly string _coreExePath;
    private Process? _proc;
    private IntPtr _job = IntPtr.Zero;
    private CancellationTokenSource? _heartbeatCts;
    public bool AutoRestart { get; set; } = true;
    public event EventHandler? CoreRestarted;
    public event EventHandler<Exception>? CoreCrashed;

    private const string PipeName = "MirrorAudioSettings";
    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(3);
    private static readonly Encoding Wire = Encoding.Unicode;

    public CoreHost(string coreExePath) { _coreExePath = coreExePath; }

    public bool IsRunning => _proc is { HasExited: false };

    public async Task StartAsync(bool autoStartAudio = false, CancellationToken ct = default)
    {
        if (IsRunning) return;
        if (!File.Exists(_coreExePath))
            throw new FileNotFoundException("未找到内核", _coreExePath);

        var psi = new ProcessStartInfo
        {
            FileName = _coreExePath,
            Arguments = autoStartAudio ? "--auto-start" : "",
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            WorkingDirectory = Path.GetDirectoryName(_coreExePath)!
        };
        _proc = Process.Start(psi) ?? throw new InvalidOperationException("内核启动失败");

        if (_job == IntPtr.Zero)
        {
            _job = CreateJobObject(IntPtr.Zero, null);
            var info = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
            {
                BasicLimitInformation = new JOBOBJECT_BASIC_LIMIT_INFORMATION
                {
                    LimitFlags = 0x00002000 // JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
                }
            };
            int len = Marshal.SizeOf<JOBOBJECT_EXTENDED_LIMIT_INFORMATION>();
            IntPtr p = Marshal.AllocHGlobal(len);
            try
            {
                Marshal.StructureToPtr(info, p, false);
                if (!SetInformationJobObject(_job, 9, p, (uint)len))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "SetInformationJobObject 失败");
            }
            finally { Marshal.FreeHGlobal(p); }
        }
        if (!AssignProcessToJobObject(_job, _proc.Handle))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "AssignProcessToJobObject 失败");

        var t0 = DateTime.UtcNow;
        while ((DateTime.UtcNow - t0) < ConnectTimeout)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(500);
                await client.ConnectAsync(cts.Token);
                break;
            }
            catch
            {
                if (_proc.HasExited) throw new InvalidOperationException("内核异常退出");
                await Task.Delay(120, ct);
            }
        }
        StartHeartbeat();
    }

    private void StartHeartbeat()
    {
        StopHeartbeat();
        _heartbeatCts = new CancellationTokenSource();
        _ = Task.Run(async () =>
        {
            while (!_heartbeatCts.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(2000, _heartbeatCts.Token);
                    var _ = await SendAsync("PING", _heartbeatCts.Token);
                }
                catch (Exception ex)
                {
                    CoreCrashed?.Invoke(this, ex);
                    if (AutoRestart)
                    {
                        try { await RestartAsync(); CoreRestarted?.Invoke(this, EventArgs.Empty); }
                        catch { }
                    }
                }
            }
        });
    }

    private void StopHeartbeat()
    {
        _heartbeatCts?.Cancel();
        _heartbeatCts?.Dispose();
        _heartbeatCts = null;
    }

    public async Task<string> SendAsync(string text, CancellationToken ct = default)
    {
        using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await client.ConnectAsync(ct);
        var bytes = Wire.GetBytes(text + "\0");
        await client.WriteAsync(bytes, 0, bytes.Length, ct);
        await client.FlushAsync(ct);

        using var ms = new MemoryStream();
        var buf = new byte[2048];
        int n;
        do {
            n = await client.ReadAsync(buf, 0, buf.Length, ct);
            if (n > 0) ms.Write(buf, 0, n);
        } while (n == buf.Length);
        return Wire.GetString(ms.ToArray()).TrimEnd('\0');
    }

    public Task<string> SendJsonAsync(object payload, CancellationToken ct = default)
        => SendAsync(JsonSerializer.Serialize(payload), ct);

    public Task EnableLogAsync(bool enable, CancellationToken ct = default)
        => SendJsonAsync(new { log = enable }, ct);

    public async Task StopAsync(CancellationToken ct = default)
    {
        StopHeartbeat();
        try { await SendAsync("STOP", ct); } catch { }
        if (_proc is { HasExited: false })
        {
            try { if (!_proc.WaitForExit(1500)) _proc.Kill(true); } catch { }
        }
        _proc?.Dispose(); _proc = null;
    }

    public async Task RestartAsync(CancellationToken ct = default)
    {
        await StopAsync(ct);
        await StartAsync(false, ct);
    }

    public void Dispose()
    {
        try { StopAsync().GetAwaiter().GetResult(); } catch { }
        if (_job != IntPtr.Zero) { CloseHandle(_job); _job = IntPtr.Zero; }
    }

    #region P/Invoke JobObject
    [StructLayout(LayoutKind.Sequential)]
    struct JOBOBJECT_BASIC_LIMIT_INFORMATION
    {
        public long PerProcessUserTimeLimit;
        public long PerJobUserTimeLimit;
        public int LimitFlags;
        public UIntPtr MinimumWorkingSetSize;
        public UIntPtr MaximumWorkingSetSize;
        public int ActiveProcessLimit;
        public long Affinity;
        public int PriorityClass;
        public int SchedulingClass;
    }
    [StructLayout(LayoutKind.Sequential)]
    struct IO_COUNTERS
    {
        public ulong ReadOperationCount;
        public ulong WriteOperationCount;
        public ulong OtherOperationCount;
        public ulong ReadTransferCount;
        public ulong WriteTransferCount;
        public ulong OtherTransferCount;
    }
    [StructLayout(LayoutKind.Sequential)]
    struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
    {
        public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
        public IO_COUNTERS IoInfo;
        public UIntPtr ProcessMemoryLimit;
        public UIntPtr JobMemoryLimit;
        public UIntPtr PeakProcessMemoryUsed;
        public UIntPtr PeakJobMemoryUsed;
    }
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string? name);
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool SetInformationJobObject(IntPtr hJob, int infoClass, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool CloseHandle(IntPtr hObject);
    #endregion
}
}
