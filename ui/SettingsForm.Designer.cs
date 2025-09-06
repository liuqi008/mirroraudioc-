
namespace MirrorAudio.UI
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.chkExclusive = new System.Windows.Forms.CheckBox();
            this.chkRaw = new System.Windows.Forms.CheckBox();
            this.chkForce = new System.Windows.Forms.CheckBox();
            this.chkLog = new System.Windows.Forms.CheckBox();
            this.numPeriodUs = new System.Windows.Forms.NumericUpDown();
            this.numRate = new System.Windows.Forms.NumericUpDown();
            this.numBits = new System.Windows.Forms.NumericUpDown();
            this.numCh = new System.Windows.Forms.NumericUpDown();
            this.lblPeriod = new System.Windows.Forms.Label();
            this.lblRate = new System.Windows.Forms.Label();
            this.lblBits = new System.Windows.Forms.Label();
            this.lblCh = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numPeriodUs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBits)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCh)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(24, 24);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(90, 30);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "启动";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(132, 24);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(90, 30);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "停止";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(240, 24);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(90, 30);
            this.btnApply.TabIndex = 2;
            this.btnApply.Text = "应用";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += async (s,e) => await _core!.SendJsonAsync(new {
                cmd = "apply",
                exclusive = chkExclusive.Checked,
                raw = chkRaw.Checked,
                force_passthrough = chkForce.Checked,
                period_us = (int)numPeriodUs.Value,
                sample_rate = (int)numRate.Value,
                bits = (int)numBits.Value,
                channels = (int)numCh.Value
            });
            // 
            // chkExclusive
            // 
            this.chkExclusive.AutoSize = true;
            this.chkExclusive.Checked = true;
            this.chkExclusive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExclusive.Location = new System.Drawing.Point(24, 80);
            this.chkExclusive.Name = "chkExclusive";
            this.chkExclusive.Size = new System.Drawing.Size(75, 21);
            this.chkExclusive.TabIndex = 3;
            this.chkExclusive.Text = "独占模式";
            // 
            // chkRaw
            // 
            this.chkRaw.AutoSize = true;
            this.chkRaw.Checked = true;
            this.chkRaw.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRaw.Location = new System.Drawing.Point(120, 80);
            this.chkRaw.Name = "chkRaw";
            this.chkRaw.Size = new System.Drawing.Size(66, 21);
            this.chkRaw.TabIndex = 4;
            this.chkRaw.Text = "RAW直通";
            // 
            // chkForce
            // 
            this.chkForce.AutoSize = true;
            this.chkForce.Location = new System.Drawing.Point(204, 80);
            this.chkForce.Name = "chkForce";
            this.chkForce.Size = new System.Drawing.Size(99, 21);
            this.chkForce.TabIndex = 5;
            this.chkForce.Text = "强制直通(拒降级)";
            // 
            // chkLog
            // 
            this.chkLog.AutoSize = true;
            this.chkLog.Location = new System.Drawing.Point(324, 80);
            this.chkLog.Name = "chkLog";
            this.chkLog.Size = new System.Drawing.Size(75, 21);
            this.chkLog.TabIndex = 6;
            this.chkLog.Text = "启用日志";
            this.chkLog.CheckedChanged += new System.EventHandler(this.chkLog_CheckedChanged);
            // 
            // numPeriodUs
            // 
            this.numPeriodUs.Location = new System.Drawing.Point(96, 120);
            this.numPeriodUs.Maximum = new decimal(new int[] { 20000, 0, 0, 0 });
            this.numPeriodUs.Minimum = new decimal(new int[] { 500, 0, 0, 0 });
            this.numPeriodUs.Name = "numPeriodUs";
            this.numPeriodUs.Size = new System.Drawing.Size(80, 23);
            this.numPeriodUs.TabIndex = 7;
            this.numPeriodUs.Value = new decimal(new int[] { 3000, 0, 0, 0 });
            // 
            // numRate
            // 
            this.numRate.Location = new System.Drawing.Point(96, 152);
            this.numRate.Maximum = new decimal(new int[] { 384000, 0, 0, 0 });
            this.numRate.Minimum = new decimal(new int[] { 44100, 0, 0, 0 });
            this.numRate.Name = "numRate";
            this.numRate.Size = new System.Drawing.Size(80, 23);
            this.numRate.TabIndex = 8;
            this.numRate.Value = new decimal(new int[] { 192000, 0, 0, 0 });
            // 
            // numBits
            // 
            this.numBits.Location = new System.Drawing.Point(96, 184);
            this.numBits.Maximum = new decimal(new int[] { 32, 0, 0, 0 });
            this.numBits.Minimum = new decimal(new int[] { 16, 0, 0, 0 });
            this.numBits.Name = "numBits";
            this.numBits.Size = new System.Drawing.Size(80, 23);
            this.numBits.TabIndex = 9;
            this.numBits.Value = new decimal(new int[] { 24, 0, 0, 0 });
            // 
            // numCh
            // 
            this.numCh.Location = new System.Drawing.Point(96, 216);
            this.numCh.Maximum = new decimal(new int[] { 8, 0, 0, 0 });
            this.numCh.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            this.numCh.Name = "numCh";
            this.numCh.Size = new System.Drawing.Size(80, 23);
            this.numCh.TabIndex = 10;
            this.numCh.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // labels
            this.lblPeriod.AutoSize = true; this.lblPeriod.Location = new System.Drawing.Point(24, 122);
            this.lblPeriod.Text = "周期(us)";
            this.lblRate.AutoSize = true; this.lblRate.Location = new System.Drawing.Point(24, 154);
            this.lblRate.Text = "采样率";
            this.lblBits.AutoSize = true; this.lblBits.Location = new System.Drawing.Point(24, 186);
            this.lblBits.Text = "位深";
            this.lblCh.AutoSize = true; this.lblCh.Location = new System.Drawing.Point(24, 218);
            this.lblCh.Text = "通道数";

            // 
            // SettingsForm
            // 
            this.ClientSize = new System.Drawing.Size(440, 270);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.chkExclusive);
            this.Controls.Add(this.chkRaw);
            this.Controls.Add(this.chkForce);
            this.Controls.Add(this.chkLog);
            this.Controls.Add(this.numPeriodUs);
            this.Controls.Add(this.numRate);
            this.Controls.Add(this.numBits);
            this.Controls.Add(this.numCh);
            this.Controls.Add(this.lblPeriod);
            this.Controls.Add(this.lblRate);
            this.Controls.Add(this.lblBits);
            this.Controls.Add(this.lblCh);
            this.Text = "MirrorAudio 设置";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.CheckBox chkExclusive;
        private System.Windows.Forms.CheckBox chkRaw;
        private System.Windows.Forms.CheckBox chkForce;
        private System.Windows.Forms.CheckBox chkLog;
        private System.Windows.Forms.NumericUpDown numPeriodUs;
        private System.Windows.Forms.NumericUpDown numRate;
        private System.Windows.Forms.NumericUpDown numBits;
        private System.Windows.Forms.NumericUpDown numCh;
        private System.Windows.Forms.Label lblPeriod;
        private System.Windows.Forms.Label lblRate;
        private System.Windows.Forms.Label lblBits;
        private System.Windows.Forms.Label lblCh;
    }
}
