namespace SerialPortListener
{
    partial class ucHelp
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_spManager != null)
                {
                    _spManager.NewSerialDataRecieved -= _spManager_NewSerialDataRecieved;
                }
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.gbConnection = new System.Windows.Forms.GroupBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.cboPort = new System.Windows.Forms.ComboBox();
            this.lblBaud = new System.Windows.Forms.Label();
            this.cboBaud = new System.Windows.Forms.ComboBox();
            this.lblParity = new System.Windows.Forms.Label();
            this.cboParity = new System.Windows.Forms.ComboBox();
            this.lblDataBits = new System.Windows.Forms.Label();
            this.cboDataBits = new System.Windows.Forms.ComboBox();
            this.lblStopBits = new System.Windows.Forms.Label();
            this.cboStopBits = new System.Windows.Forms.ComboBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnSavePort = new System.Windows.Forms.Button();
            this.gbRawData = new System.Windows.Forms.GroupBox();
            this.tbRx = new System.Windows.Forms.TextBox();
            this.gbScale = new System.Windows.Forms.GroupBox();
            this.lblSerialHandler = new System.Windows.Forms.Label();
            this.cboSerialHandler = new System.Windows.Forms.ComboBox();
            this.lblWeightPreview = new System.Windows.Forms.Label();
            this.tbWeightPreview = new System.Windows.Forms.TextBox();
            this.timerRx = new System.Windows.Forms.Timer(this.components);
            this.gbConnection.SuspendLayout();
            this.gbRawData.SuspendLayout();
            this.gbScale.SuspendLayout();
            this.SuspendLayout();
            //
            // gbConnection
            //
            this.gbConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.gbConnection.Controls.Add(this.lblPort);
            this.gbConnection.Controls.Add(this.cboPort);
            this.gbConnection.Controls.Add(this.lblBaud);
            this.gbConnection.Controls.Add(this.cboBaud);
            this.gbConnection.Controls.Add(this.lblParity);
            this.gbConnection.Controls.Add(this.cboParity);
            this.gbConnection.Controls.Add(this.lblDataBits);
            this.gbConnection.Controls.Add(this.cboDataBits);
            this.gbConnection.Controls.Add(this.lblStopBits);
            this.gbConnection.Controls.Add(this.cboStopBits);
            this.gbConnection.Controls.Add(this.btnStart);
            this.gbConnection.Controls.Add(this.btnStop);
            this.gbConnection.Controls.Add(this.btnSavePort);
            this.gbConnection.Font = new System.Drawing.Font("Century Gothic", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbConnection.Location = new System.Drawing.Point(16, 16);
            this.gbConnection.Name = "gbConnection";
            this.gbConnection.Size = new System.Drawing.Size(340, 352);
            this.gbConnection.TabIndex = 0;
            this.gbConnection.TabStop = false;
            this.gbConnection.Text = "การเชื่อมต่อพอร์ต";
            //
            // lblPort
            //
            this.lblPort.AutoSize = true;
            this.lblPort.Font = new System.Drawing.Font("Century Gothic", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPort.Location = new System.Drawing.Point(20, 34);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(46, 22);
            this.lblPort.TabIndex = 2;
            this.lblPort.Text = "Port";
            //
            // cboPort
            //
            this.cboPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPort.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboPort.FormattingEnabled = true;
            this.cboPort.Location = new System.Drawing.Point(120, 30);
            this.cboPort.Name = "cboPort";
            this.cboPort.Size = new System.Drawing.Size(200, 29);
            this.cboPort.TabIndex = 3;
            //
            // lblBaud
            //
            this.lblBaud.AutoSize = true;
            this.lblBaud.Font = new System.Drawing.Font("Century Gothic", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBaud.Location = new System.Drawing.Point(20, 78);
            this.lblBaud.Name = "lblBaud";
            this.lblBaud.Size = new System.Drawing.Size(59, 22);
            this.lblBaud.TabIndex = 4;
            this.lblBaud.Text = "Baud";
            //
            // cboBaud
            //
            this.cboBaud.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBaud.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboBaud.FormattingEnabled = true;
            this.cboBaud.Location = new System.Drawing.Point(120, 74);
            this.cboBaud.Name = "cboBaud";
            this.cboBaud.Size = new System.Drawing.Size(200, 29);
            this.cboBaud.TabIndex = 5;
            //
            // lblParity
            //
            this.lblParity.AutoSize = true;
            this.lblParity.Font = new System.Drawing.Font("Century Gothic", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblParity.Location = new System.Drawing.Point(20, 122);
            this.lblParity.Name = "lblParity";
            this.lblParity.Size = new System.Drawing.Size(61, 22);
            this.lblParity.TabIndex = 6;
            this.lblParity.Text = "Parity";
            //
            // cboParity
            //
            this.cboParity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboParity.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboParity.FormattingEnabled = true;
            this.cboParity.Location = new System.Drawing.Point(120, 118);
            this.cboParity.Name = "cboParity";
            this.cboParity.Size = new System.Drawing.Size(200, 29);
            this.cboParity.TabIndex = 7;
            //
            // lblDataBits
            //
            this.lblDataBits.AutoSize = true;
            this.lblDataBits.Font = new System.Drawing.Font("Century Gothic", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDataBits.Location = new System.Drawing.Point(20, 166);
            this.lblDataBits.Name = "lblDataBits";
            this.lblDataBits.Size = new System.Drawing.Size(85, 22);
            this.lblDataBits.TabIndex = 8;
            this.lblDataBits.Text = "DataBits";
            //
            // cboDataBits
            //
            this.cboDataBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDataBits.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboDataBits.FormattingEnabled = true;
            this.cboDataBits.Location = new System.Drawing.Point(120, 162);
            this.cboDataBits.Name = "cboDataBits";
            this.cboDataBits.Size = new System.Drawing.Size(200, 29);
            this.cboDataBits.TabIndex = 9;
            //
            // lblStopBits
            //
            this.lblStopBits.AutoSize = true;
            this.lblStopBits.Font = new System.Drawing.Font("Century Gothic", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStopBits.Location = new System.Drawing.Point(20, 210);
            this.lblStopBits.Name = "lblStopBits";
            this.lblStopBits.Size = new System.Drawing.Size(79, 22);
            this.lblStopBits.TabIndex = 10;
            this.lblStopBits.Text = "StopBits";
            //
            // cboStopBits
            //
            this.cboStopBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStopBits.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboStopBits.FormattingEnabled = true;
            this.cboStopBits.Location = new System.Drawing.Point(120, 206);
            this.cboStopBits.Name = "cboStopBits";
            this.cboStopBits.Size = new System.Drawing.Size(200, 29);
            this.cboStopBits.TabIndex = 11;
            //
            // btnStart
            //
            this.btnStart.BackColor = System.Drawing.Color.White;
            this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStart.Font = new System.Drawing.Font("Century Gothic", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStart.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnStart.Location = new System.Drawing.Point(20, 254);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(146, 38);
            this.btnStart.TabIndex = 12;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            //
            // btnStop
            //
            this.btnStop.BackColor = System.Drawing.Color.White;
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStop.Font = new System.Drawing.Font("Century Gothic", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStop.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.btnStop.Location = new System.Drawing.Point(174, 254);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(146, 38);
            this.btnStop.TabIndex = 13;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            //
            // btnSavePort
            //
            this.btnSavePort.BackColor = System.Drawing.Color.White;
            this.btnSavePort.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSavePort.Font = new System.Drawing.Font("Century Gothic", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSavePort.ForeColor = System.Drawing.Color.Green;
            this.btnSavePort.Location = new System.Drawing.Point(20, 300);
            this.btnSavePort.Name = "btnSavePort";
            this.btnSavePort.Size = new System.Drawing.Size(300, 32);
            this.btnSavePort.TabIndex = 15;
            this.btnSavePort.Text = "save config ";
            this.btnSavePort.UseVisualStyleBackColor = false;
            this.btnSavePort.Click += new System.EventHandler(this.btnSavePort_Click);
            //
            // gbRawData
            //
            this.gbRawData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbRawData.Controls.Add(this.tbRx);
            this.gbRawData.Font = new System.Drawing.Font("Century Gothic", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbRawData.Location = new System.Drawing.Point(372, 16);
            this.gbRawData.Name = "gbRawData";
            this.gbRawData.Size = new System.Drawing.Size(456, 352);
            this.gbRawData.TabIndex = 1;
            this.gbRawData.TabStop = false;
            this.gbRawData.Text = "ข้อมูลดิบที่ได้รับ (Rx)";
            //
            // tbRx
            //
            this.tbRx.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbRx.BackColor = System.Drawing.Color.White;
            this.tbRx.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbRx.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbRx.Location = new System.Drawing.Point(20, 32);
            this.tbRx.Multiline = true;
            this.tbRx.Name = "tbRx";
            this.tbRx.ReadOnly = true;
            this.tbRx.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbRx.Size = new System.Drawing.Size(416, 300);
            this.tbRx.TabIndex = 14;
            //
            // gbScale
            //
            this.gbScale.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbScale.Controls.Add(this.lblSerialHandler);
            this.gbScale.Controls.Add(this.cboSerialHandler);
            this.gbScale.Controls.Add(this.lblWeightPreview);
            this.gbScale.Controls.Add(this.tbWeightPreview);
            this.gbScale.Font = new System.Drawing.Font("Century Gothic", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbScale.Location = new System.Drawing.Point(16, 384);
            this.gbScale.Name = "gbScale";
            this.gbScale.Size = new System.Drawing.Size(812, 130);
            this.gbScale.TabIndex = 16;
            this.gbScale.TabStop = false;
            this.gbScale.Text = "รูปแบบตาชั่ง";
            //
            // lblSerialHandler
            //
            this.lblSerialHandler.AutoSize = true;
            this.lblSerialHandler.Font = new System.Drawing.Font("Century Gothic", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSerialHandler.Location = new System.Drawing.Point(16, 36);
            this.lblSerialHandler.Name = "lblSerialHandler";
            this.lblSerialHandler.Size = new System.Drawing.Size(84, 22);
            this.lblSerialHandler.TabIndex = 17;
            this.lblSerialHandler.Text = "รูปแบบตาชั่ง";
            //
            // cboSerialHandler
            //
            this.cboSerialHandler.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSerialHandler.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSerialHandler.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboSerialHandler.FormattingEnabled = true;
            this.cboSerialHandler.Location = new System.Drawing.Point(176, 32);
            this.cboSerialHandler.Name = "cboSerialHandler";
            this.cboSerialHandler.Size = new System.Drawing.Size(604, 29);
            this.cboSerialHandler.TabIndex = 18;
            this.cboSerialHandler.SelectedIndexChanged += new System.EventHandler(this.cboSerialHandler_SelectedIndexChanged);
            //
            // lblWeightPreview
            //
            this.lblWeightPreview.AutoSize = true;
            this.lblWeightPreview.Font = new System.Drawing.Font("Century Gothic", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWeightPreview.Location = new System.Drawing.Point(16, 75);
            this.lblWeightPreview.Name = "lblWeightPreview";
            this.lblWeightPreview.Size = new System.Drawing.Size(104, 22);
            this.lblWeightPreview.TabIndex = 19;
            this.lblWeightPreview.Text = "น้ำหนักที่อ่านได้";
            //
            // tbWeightPreview
            //
            this.tbWeightPreview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbWeightPreview.BackColor = System.Drawing.Color.Black;
            this.tbWeightPreview.Font = new System.Drawing.Font("Century Gothic", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbWeightPreview.ForeColor = System.Drawing.Color.LightGreen;
            this.tbWeightPreview.Location = new System.Drawing.Point(176, 67);
            this.tbWeightPreview.Name = "tbWeightPreview";
            this.tbWeightPreview.ReadOnly = true;
            this.tbWeightPreview.Size = new System.Drawing.Size(604, 47);
            this.tbWeightPreview.TabIndex = 20;
            this.tbWeightPreview.TabStop = false;
            this.tbWeightPreview.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            //
            // timerRx
            //
            this.timerRx.Interval = 200;
            this.timerRx.Tick += new System.EventHandler(this.timerRx_Tick);
            //
            // ucHelp
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbScale);
            this.Controls.Add(this.gbRawData);
            this.Controls.Add(this.gbConnection);
            this.Name = "ucHelp";
            this.Size = new System.Drawing.Size(844, 530);
            this.Load += new System.EventHandler(this.ucHelp_Load);
            this.gbConnection.ResumeLayout(false);
            this.gbConnection.PerformLayout();
            this.gbRawData.ResumeLayout(false);
            this.gbRawData.PerformLayout();
            this.gbScale.ResumeLayout(false);
            this.gbScale.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox gbConnection;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.ComboBox cboPort;
        private System.Windows.Forms.Label lblBaud;
        private System.Windows.Forms.ComboBox cboBaud;
        private System.Windows.Forms.Label lblParity;
        private System.Windows.Forms.ComboBox cboParity;
        private System.Windows.Forms.Label lblDataBits;
        private System.Windows.Forms.ComboBox cboDataBits;
        private System.Windows.Forms.Label lblStopBits;
        private System.Windows.Forms.ComboBox cboStopBits;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnSavePort;
        private System.Windows.Forms.GroupBox gbRawData;
        private System.Windows.Forms.TextBox tbRx;
        private System.Windows.Forms.Timer timerRx;
        private System.Windows.Forms.GroupBox gbScale;
        private System.Windows.Forms.Label lblSerialHandler;
        private System.Windows.Forms.ComboBox cboSerialHandler;
        private System.Windows.Forms.Label lblWeightPreview;
        private System.Windows.Forms.TextBox tbWeightPreview;
    }
}
