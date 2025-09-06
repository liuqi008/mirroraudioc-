
using System;
using System.Windows.Forms;

namespace MirrorAudio.UI
{
    public partial class SettingsForm : Form
    {
        private readonly CoreHost _core;

        public SettingsForm(CoreHost core)
        {
            InitializeComponent();
            _core = core;
            _core.CoreRestarted += (_, __) => this.BeginInvoke((Action)(() => Text = "Core restarted"));
            _core.CoreCrashed   += (_, ex) => this.BeginInvoke((Action)(() => Text = "Core crashed: " + ex.Message));
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            await _core.StartAsync(false);
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
            await _core.StopAsync();
        }

        private async void chkLog_CheckedChanged(object sender, EventArgs e)
        {
            await _core.EnableLogAsync(chkLog.Checked);
        }
    }
}
