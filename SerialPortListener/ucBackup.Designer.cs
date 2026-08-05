namespace SerialPortListener
{
    partial class ucBackup
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
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btDLSetting = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btDLWeight = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tbdateULWeight = new System.Windows.Forms.DateTimePicker();
            this.btULWeight = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btnCheckUpdate = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.lbPgDumpPath = new System.Windows.Forms.Label();
            this.tbPgDumpPath = new System.Windows.Forms.TextBox();
            this.btnBrowsePgDump = new System.Windows.Forms.Button();
            this.lbBackupDir = new System.Windows.Forms.Label();
            this.tbBackupDir = new System.Windows.Forms.TextBox();
            this.btnBrowseBackupDir = new System.Windows.Forms.Button();
            this.btnSaveBackupConfig = new System.Windows.Forms.Button();
            this.chkAutoBackup = new System.Windows.Forms.CheckBox();
            this.lbLastAutoBackup = new System.Windows.Forms.Label();
            this.btnBackup = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btDLSetting);
            this.groupBox1.Location = new System.Drawing.Point(74, 21);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(830, 89);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "ดาว์นโหลดการตั้งค่า";
            // 
            // btDLSetting
            // 
            this.btDLSetting.Location = new System.Drawing.Point(332, 38);
            this.btDLSetting.Name = "btDLSetting";
            this.btDLSetting.Size = new System.Drawing.Size(159, 27);
            this.btDLSetting.TabIndex = 3;
            this.btDLSetting.Text = "Downloade";
            this.btDLSetting.UseVisualStyleBackColor = true;
            this.btDLSetting.Click += new System.EventHandler(this.btDLSetting_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btDLWeight);
            this.groupBox2.Location = new System.Drawing.Point(74, 116);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(830, 96);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "ดาว์นโหลดรายการชั่งที่แก้ไข";
            // 
            // btDLWeight
            // 
            this.btDLWeight.Location = new System.Drawing.Point(332, 40);
            this.btDLWeight.Name = "btDLWeight";
            this.btDLWeight.Size = new System.Drawing.Size(159, 27);
            this.btDLWeight.TabIndex = 3;
            this.btDLWeight.Text = "Downloade";
            this.btDLWeight.UseVisualStyleBackColor = true;
            this.btDLWeight.Click += new System.EventHandler(this.btDLWeight_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tbdateULWeight);
            this.groupBox3.Controls.Add(this.btULWeight);
            this.groupBox3.Location = new System.Drawing.Point(74, 218);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(830, 100);
            this.groupBox3.TabIndex = 13;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Upload to WebApp";
            // 
            // tbdateULWeight
            // 
            this.tbdateULWeight.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.tbdateULWeight.Location = new System.Drawing.Point(230, 41);
            this.tbdateULWeight.Name = "tbdateULWeight";
            this.tbdateULWeight.Size = new System.Drawing.Size(304, 27);
            this.tbdateULWeight.TabIndex = 8;
            // 
            // btULWeight
            // 
            this.btULWeight.Enabled = false;
            this.btULWeight.Location = new System.Drawing.Point(540, 41);
            this.btULWeight.Name = "btULWeight";
            this.btULWeight.Size = new System.Drawing.Size(126, 27);
            this.btULWeight.TabIndex = 3;
            this.btULWeight.Text = "Upload";
            this.btULWeight.UseVisualStyleBackColor = true;
            this.btULWeight.Click += new System.EventHandler(this.btULWeight_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btnCheckUpdate);
            this.groupBox4.Location = new System.Drawing.Point(74, 324);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(830, 97);
            this.groupBox4.TabIndex = 14;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "ตรวจสอบอัพเดทโปรแกรม";
            // 
            // btnCheckUpdate
            // 
            this.btnCheckUpdate.Location = new System.Drawing.Point(332, 36);
            this.btnCheckUpdate.Name = "btnCheckUpdate";
            this.btnCheckUpdate.Size = new System.Drawing.Size(159, 27);
            this.btnCheckUpdate.TabIndex = 3;
            this.btnCheckUpdate.Text = "ตรวจสอบอัพเดท";
            this.btnCheckUpdate.UseVisualStyleBackColor = true;
            this.btnCheckUpdate.Click += new System.EventHandler(this.btnCheckUpdate_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.lbPgDumpPath);
            this.groupBox5.Controls.Add(this.tbPgDumpPath);
            this.groupBox5.Controls.Add(this.btnBrowsePgDump);
            this.groupBox5.Controls.Add(this.lbBackupDir);
            this.groupBox5.Controls.Add(this.tbBackupDir);
            this.groupBox5.Controls.Add(this.btnBrowseBackupDir);
            this.groupBox5.Controls.Add(this.btnSaveBackupConfig);
            this.groupBox5.Controls.Add(this.chkAutoBackup);
            this.groupBox5.Controls.Add(this.lbLastAutoBackup);
            this.groupBox5.Controls.Add(this.btnBackup);
            this.groupBox5.Location = new System.Drawing.Point(74, 427);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(830, 200);
            this.groupBox5.TabIndex = 15;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Setting Backup";
            // 
            // lbPgDumpPath
            // 
            this.lbPgDumpPath.AutoSize = true;
            this.lbPgDumpPath.Location = new System.Drawing.Point(33, 40);
            this.lbPgDumpPath.Name = "lbPgDumpPath";
            this.lbPgDumpPath.Size = new System.Drawing.Size(127, 21);
            this.lbPgDumpPath.TabIndex = 0;
            this.lbPgDumpPath.Text = "pg_dump.exe :";
            // 
            // tbPgDumpPath
            // 
            this.tbPgDumpPath.Location = new System.Drawing.Point(166, 37);
            this.tbPgDumpPath.Name = "tbPgDumpPath";
            this.tbPgDumpPath.Size = new System.Drawing.Size(500, 27);
            this.tbPgDumpPath.TabIndex = 1;
            // 
            // btnBrowsePgDump
            // 
            this.btnBrowsePgDump.Location = new System.Drawing.Point(676, 36);
            this.btnBrowsePgDump.Name = "btnBrowsePgDump";
            this.btnBrowsePgDump.Size = new System.Drawing.Size(90, 27);
            this.btnBrowsePgDump.TabIndex = 2;
            this.btnBrowsePgDump.Text = "Browse...";
            this.btnBrowsePgDump.UseVisualStyleBackColor = true;
            this.btnBrowsePgDump.Click += new System.EventHandler(this.btnBrowsePgDump_Click);
            // 
            // lbBackupDir
            // 
            this.lbBackupDir.AutoSize = true;
            this.lbBackupDir.Location = new System.Drawing.Point(33, 80);
            this.lbBackupDir.Name = "lbBackupDir";
            this.lbBackupDir.Size = new System.Drawing.Size(128, 21);
            this.lbBackupDir.TabIndex = 3;
            this.lbBackupDir.Text = "Backup Folder :";
            // 
            // tbBackupDir
            // 
            this.tbBackupDir.Location = new System.Drawing.Point(166, 77);
            this.tbBackupDir.Name = "tbBackupDir";
            this.tbBackupDir.Size = new System.Drawing.Size(500, 27);
            this.tbBackupDir.TabIndex = 4;
            // 
            // btnBrowseBackupDir
            // 
            this.btnBrowseBackupDir.Location = new System.Drawing.Point(676, 76);
            this.btnBrowseBackupDir.Name = "btnBrowseBackupDir";
            this.btnBrowseBackupDir.Size = new System.Drawing.Size(90, 27);
            this.btnBrowseBackupDir.TabIndex = 5;
            this.btnBrowseBackupDir.Text = "Browse...";
            this.btnBrowseBackupDir.UseVisualStyleBackColor = true;
            this.btnBrowseBackupDir.Click += new System.EventHandler(this.btnBrowseBackupDir_Click);
            // 
            // btnSaveBackupConfig
            // 
            this.btnSaveBackupConfig.Location = new System.Drawing.Point(284, 118);
            this.btnSaveBackupConfig.Name = "btnSaveBackupConfig";
            this.btnSaveBackupConfig.Size = new System.Drawing.Size(159, 27);
            this.btnSaveBackupConfig.TabIndex = 6;
            this.btnSaveBackupConfig.Text = "Save Config";
            this.btnSaveBackupConfig.UseVisualStyleBackColor = true;
            this.btnSaveBackupConfig.Click += new System.EventHandler(this.btnSaveBackupConfig_Click);
            // 
            // chkAutoBackup
            // 
            this.chkAutoBackup.AutoSize = true;
            this.chkAutoBackup.Checked = true;
            this.chkAutoBackup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoBackup.Location = new System.Drawing.Point(526, 155);
            this.chkAutoBackup.Name = "chkAutoBackup";
            this.chkAutoBackup.Size = new System.Drawing.Size(299, 25);
            this.chkAutoBackup.TabIndex = 7;
            this.chkAutoBackup.Text = "Auto Backup ทุก 2 ชม. (09:00 - 17:00)";
            this.chkAutoBackup.UseVisualStyleBackColor = true;
            this.chkAutoBackup.Visible = false;
            // 
            // lbLastAutoBackup
            // 
            this.lbLastAutoBackup.AutoSize = true;
            this.lbLastAutoBackup.Location = new System.Drawing.Point(33, 156);
            this.lbLastAutoBackup.Name = "lbLastAutoBackup";
            this.lbLastAutoBackup.Size = new System.Drawing.Size(120, 21);
            this.lbLastAutoBackup.TabIndex = 8;
            this.lbLastAutoBackup.Text = "Backup ล่าสุด: -";
            // 
            // btnBackup
            // 
            this.btnBackup.Location = new System.Drawing.Point(449, 118);
            this.btnBackup.Name = "btnBackup";
            this.btnBackup.Size = new System.Drawing.Size(159, 27);
            this.btnBackup.TabIndex = 3;
            this.btnBackup.Text = "backup";
            this.btnBackup.UseVisualStyleBackColor = true;
            this.btnBackup.Click += new System.EventHandler(this.btnBackup_Click);
            // 
            // ucBackup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "ucBackup";
            this.Size = new System.Drawing.Size(965, 685);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btDLSetting;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btDLWeight;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btULWeight;
        private System.Windows.Forms.DateTimePicker tbdateULWeight;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button btnCheckUpdate;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label lbPgDumpPath;
        private System.Windows.Forms.TextBox tbPgDumpPath;
        private System.Windows.Forms.Button btnBrowsePgDump;
        private System.Windows.Forms.Label lbBackupDir;
        private System.Windows.Forms.TextBox tbBackupDir;
        private System.Windows.Forms.Button btnBrowseBackupDir;
        private System.Windows.Forms.Button btnSaveBackupConfig;
        private System.Windows.Forms.CheckBox chkAutoBackup;
        private System.Windows.Forms.Label lbLastAutoBackup;
        private System.Windows.Forms.Button btnBackup;
    }
}
