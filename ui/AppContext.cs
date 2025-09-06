
#nullable enable
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MirrorAudio.UI
{
    public sealed class AppContext : ApplicationContext
    {
        private readonly NotifyIcon _tray;
        private readonly CoreHost _core;
        private SettingsForm? _settings;   // single instance
        private bool _logEnabled;

        public AppContext()
        {
            string exe = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mirroraudio_core.exe");
            _core = new CoreHost(exe) { AutoRestart = true };
            _core.CoreRestarted += (_, __) => ShowBalloon("内核已自动重启");
            _core.CoreCrashed +=   (_, ex) => ShowBalloon("内核异常：" + ex.Message);

            var menu = new ContextMenuStrip();

            var mStart = new ToolStripMenuItem("启动(&S)", null, async (_, __) => await StartCore());
            var mStop  = new ToolStripMenuItem("停止(&T)", null, async (_, __) => await _core.StopAsync());
            var mApply = new ToolStripMenuItem("应用参数(&A)", null, async (_, __) => {
                await _core.SendJsonAsync(new {
                    cmd = "apply",
                    exclusive = true,
                    raw = true,
                    force_passthrough = false,
                    period_us = 3000,
                    sample_rate = 192000,
                    bits = 24,
                    channels = 2
                });
            });

            // Declare mLog BEFORE adding to menu or using inside AddRange
            var mLog = new ToolStripMenuItem("启用日志(&L)");
            mLog.CheckOnClick = true;
            mLog.Click += async (_, __) => {
                _logEnabled = !_logEnabled;
                mLog.Checked = _logEnabled;
                await _core.EnableLogAsync(_logEnabled);
            };

            var mSettings = new ToolStripMenuItem("设置(&O)…", null, (_, __) => ShowSettings());
            var mExit  = new ToolStripMenuItem("退出(&X)", null, async (_, __) => await CleanupAndExit());

            menu.Items.Add(mStart);
            menu.Items.Add(mStop);
            menu.Items.Add(mApply);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(mLog);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(mSettings);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(mExit);

            _tray = new NotifyIcon {
                Icon = System.Drawing.SystemIcons.Application,
                Text = "MirrorAudio",
                Visible = true,
                ContextMenuStrip = menu
            };
            _tray.DoubleClick += (_, __) => ShowSettings();
        }

        private async System.Threading.Tasks.Task StartCore()
        {
            await _core.StartAsync(false);
            if (_logEnabled) await _core.EnableLogAsync(true);
            await _core.SendAsync("START");
            ShowBalloon("内核已启动");
        }

        private void ShowSettings()
        {
            if (_settings == null || _settings.IsDisposed)
            {
                _settings = new SettingsForm(_core);
                _settings.FormClosing += (s, e) => {
                    if (e.CloseReason == CloseReason.UserClosing)
                    {
                        e.Cancel = true;
                        _settings!.Hide();
                    }
                };
            }
            if (!_settings.Visible) _settings.Show();
            if (_settings.WindowState == FormWindowState.Minimized) _settings.WindowState = FormWindowState.Normal;
            _settings.Activate();
        }

        private void ShowBalloon(string msg) =>
            _tray?.ShowBalloonTip(2000, "MirrorAudio", msg, ToolTipIcon.Info);

        private async System.Threading.Tasks.Task CleanupAndExit()
        {
            try { await _core.StopAsync(); } catch { }
            _tray.Visible = false;
            _tray.Dispose();
            if (_settings != null && !_settings.IsDisposed) _settings.Dispose();
            ExitThread();
        }

        protected override async void ExitThreadCore()
        {
            await CleanupAndExit();
            base.ExitThreadCore();
        }
    }
}
