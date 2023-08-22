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
            this.btBackup = new System.Windows.Forms.Button();
            this.btBrowBackup = new System.Windows.Forms.Button();
            this.tbLocalBackup = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btRestore = new System.Windows.Forms.Button();
            this.tbLocalRestore = new System.Windows.Forms.TextBox();
            this.btBrowRestore = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btBackup);
            this.groupBox1.Controls.Add(this.btBrowBackup);
            this.groupBox1.Controls.Add(this.tbLocalBackup);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(74, 33);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(830, 194);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Backup Database";
            // 
            // btBackup
            // 
            this.btBackup.Enabled = false;
            this.btBackup.Location = new System.Drawing.Point(657, 116);
            this.btBackup.Name = "btBackup";
            this.btBackup.Size = new System.Drawing.Size(84, 27);
            this.btBackup.TabIndex = 3;
            this.btBackup.Text = "Backup";
            this.btBackup.UseVisualStyleBackColor = true;
            this.btBackup.Click += new System.EventHandler(this.btBackup_Click);
            // 
            // btBrowBackup
            // 
            this.btBrowBackup.Location = new System.Drawing.Point(657, 72);
            this.btBrowBackup.Name = "btBrowBackup";
            this.btBrowBackup.Size = new System.Drawing.Size(84, 27);
            this.btBrowBackup.TabIndex = 2;
            this.btBrowBackup.Text = "Browse";
            this.btBrowBackup.UseVisualStyleBackColor = true;
            this.btBrowBackup.Click += new System.EventHandler(this.btBrowBackup_Click);
            // 
            // tbLocalBackup
            // 
            this.tbLocalBackup.Location = new System.Drawing.Point(186, 72);
            this.tbLocalBackup.Name = "tbLocalBackup";
            this.tbLocalBackup.Size = new System.Drawing.Size(452, 27);
            this.tbLocalBackup.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(90, 72);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "Location";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btRestore);
            this.groupBox2.Controls.Add(this.tbLocalRestore);
            this.groupBox2.Controls.Add(this.btBrowRestore);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(74, 244);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(830, 194);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Restore Database";
            // 
            // btRestore
            // 
            this.btRestore.Enabled = false;
            this.btRestore.Location = new System.Drawing.Point(657, 115);
            this.btRestore.Name = "btRestore";
            this.btRestore.Size = new System.Drawing.Size(84, 27);
            this.btRestore.TabIndex = 7;
            this.btRestore.Text = "Restore";
            this.btRestore.UseVisualStyleBackColor = true;
            // 
            // tbLocalRestore
            // 
            this.tbLocalRestore.Location = new System.Drawing.Point(186, 71);
            this.tbLocalRestore.Name = "tbLocalRestore";
            this.tbLocalRestore.Size = new System.Drawing.Size(452, 27);
            this.tbLocalRestore.TabIndex = 5;
            // 
            // btBrowRestore
            // 
            this.btBrowRestore.Location = new System.Drawing.Point(657, 71);
            this.btBrowRestore.Name = "btBrowRestore";
            this.btBrowRestore.Size = new System.Drawing.Size(84, 27);
            this.btBrowRestore.TabIndex = 6;
            this.btBrowRestore.Text = "Browse";
            this.btBrowRestore.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(90, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 21);
            this.label2.TabIndex = 4;
            this.label2.Text = "Location";
            // 
            // ucBackup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "ucBackup";
            this.Size = new System.Drawing.Size(965, 514);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btBackup;
        private System.Windows.Forms.Button btBrowBackup;
        private System.Windows.Forms.TextBox tbLocalBackup;
        private System.Windows.Forms.Button btRestore;
        private System.Windows.Forms.TextBox tbLocalRestore;
        private System.Windows.Forms.Button btBrowRestore;
        private System.Windows.Forms.Label label2;
    }
}
