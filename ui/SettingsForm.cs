
using System;
using System.Windows.Forms;

namespace MirrorAudio.UI
{
    public partial class SettingsForm : Form
    {
        private CoreHost? _core;
        public SettingsForm()
        {
            InitializeComponent();
            string exe = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mirroraudio_core.exe");
            _core = new CoreHost(exe) { AutoRestart = true };
            _core.CoreRestarted += (_, __) => this.BeginInvoke((Action)(() => Text = "Core restarted"));
            _core.CoreCrashed += (_, ex) => this.BeginInvoke((Action)(() => Text = "Core crashed: " + ex.Message));
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            await _core!.StartAsync(false);
            await _core.EnableLogAsync(chkLog.Checked);
            await _core.SendJsonAsync(new {
                cmd = "apply",
                exclusive = chkExclusive.Checked,
                raw = chkRaw.Checked,
                force_passthrough = chkForce.Checked,
                period_us = (int)numPeriodUs.Value,
                sample_rate = (int)numRate.Value,
                bits = (int)numBits.Value,
                channels = (int)numCh.Value
            });
            await _core.SendAsync("START");
        }

        private async void btnStop_Click(object sender, EventArgs e)
        {
            await _core!.StopAsync();
        }

        private async void chkLog_CheckedChanged(object sender, EventArgs e)
        {
            await _core!.EnableLogAsync(chkLog.Checked);
        }

        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            try { await _core!.StopAsync(); } catch { }
            base.OnFormClosing(e);
        }
    }
}
