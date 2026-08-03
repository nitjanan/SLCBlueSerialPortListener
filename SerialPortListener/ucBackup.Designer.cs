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
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btDLSetting);
            this.groupBox1.Location = new System.Drawing.Point(82, 49);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(830, 115);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "ดาว์นโหลดการตั้งค่า";
            // 
            // btDLSetting
            // 
            this.btDLSetting.Location = new System.Drawing.Point(332, 51);
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
            this.groupBox2.Location = new System.Drawing.Point(82, 183);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(830, 115);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "ดาว์นโหลดรายการชั่งที่แก้ไข";
            // 
            // btDLWeight
            // 
            this.btDLWeight.Location = new System.Drawing.Point(332, 51);
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
            this.groupBox3.Location = new System.Drawing.Point(82, 331);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(830, 115);
            this.groupBox3.TabIndex = 13;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Upload to WebApp";
            // 
            // tbdateULWeight
            // 
            this.tbdateULWeight.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.tbdateULWeight.Location = new System.Drawing.Point(230, 40);
            this.tbdateULWeight.Name = "tbdateULWeight";
            this.tbdateULWeight.Size = new System.Drawing.Size(304, 27);
            this.tbdateULWeight.TabIndex = 8;
            // 
            // btULWeight
            // 
            this.btULWeight.Enabled = false;
            this.btULWeight.Location = new System.Drawing.Point(540, 40);
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
            this.groupBox4.Location = new System.Drawing.Point(82, 452);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(830, 115);
            this.groupBox4.TabIndex = 14;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "ตรวจสอบอัพเดทโปรแกรม";
            // 
            // btnCheckUpdate
            // 
            this.btnCheckUpdate.Location = new System.Drawing.Point(332, 47);
            this.btnCheckUpdate.Name = "btnCheckUpdate";
            this.btnCheckUpdate.Size = new System.Drawing.Size(159, 27);
            this.btnCheckUpdate.TabIndex = 3;
            this.btnCheckUpdate.Text = "ตรวจสอบอัพเดท";
            this.btnCheckUpdate.UseVisualStyleBackColor = true;
            this.btnCheckUpdate.Click += new System.EventHandler(this.btnCheckUpdate_Click);
            // 
            // ucBackup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "ucBackup";
            this.Size = new System.Drawing.Size(965, 620);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
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
    }
}
