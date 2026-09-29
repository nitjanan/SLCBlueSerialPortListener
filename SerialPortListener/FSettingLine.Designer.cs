
namespace SerialPortListener
{
    partial class FSettingLine
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label2 = new System.Windows.Forms.Label();
            this.cbbSite = new System.Windows.Forms.ComboBox();
            this.btSaveLine = new System.Windows.Forms.Button();
            this.label37 = new System.Windows.Forms.Label();
            this.dtFromOut = new System.Windows.Forms.DateTimePicker();
            this.dtDateFrom = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(46, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 21);
            this.label2.TabIndex = 56;
            this.label2.Text = "ชื่อเรือ:";
            // 
            // cbbSite
            // 
            this.cbbSite.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cbbSite.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbbSite.DropDownHeight = 300;
            this.cbbSite.FormattingEnabled = true;
            this.cbbSite.IntegralHeight = false;
            this.cbbSite.Location = new System.Drawing.Point(122, 27);
            this.cbbSite.Name = "cbbSite";
            this.cbbSite.Size = new System.Drawing.Size(196, 29);
            this.cbbSite.TabIndex = 55;
            // 
            // btSaveLine
            // 
            this.btSaveLine.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btSaveLine.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btSaveLine.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btSaveLine.Image = global::SerialPortListener.Properties.Resources.save_32px;
            this.btSaveLine.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btSaveLine.Location = new System.Drawing.Point(161, 159);
            this.btSaveLine.Name = "btSaveLine";
            this.btSaveLine.Size = new System.Drawing.Size(105, 40);
            this.btSaveLine.TabIndex = 54;
            this.btSaveLine.Text = "บันทึก";
            this.btSaveLine.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btSaveLine.UseVisualStyleBackColor = true;
            this.btSaveLine.Click += new System.EventHandler(this.btSaveLine_Click);
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Location = new System.Drawing.Point(46, 116);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(65, 21);
            this.label37.TabIndex = 53;
            this.label37.Text = "เวลาออก:";
            // 
            // dtFromOut
            // 
            this.dtFromOut.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.dtFromOut.CustomFormat = "HH:mm";
            this.dtFromOut.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtFromOut.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtFromOut.Location = new System.Drawing.Point(123, 113);
            this.dtFromOut.Name = "dtFromOut";
            this.dtFromOut.ShowUpDown = true;
            this.dtFromOut.Size = new System.Drawing.Size(195, 27);
            this.dtFromOut.TabIndex = 52;
            // 
            // dtDateFrom
            // 
            this.dtDateFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtDateFrom.Location = new System.Drawing.Point(123, 68);
            this.dtDateFrom.Name = "dtDateFrom";
            this.dtDateFrom.Size = new System.Drawing.Size(195, 27);
            this.dtDateFrom.TabIndex = 51;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(46, 73);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 21);
            this.label1.TabIndex = 50;
            this.label1.Text = "วันที่เริ่ม:";
            // 
            // FSettingLine
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(364, 226);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbbSite);
            this.Controls.Add(this.btSaveLine);
            this.Controls.Add(this.label37);
            this.Controls.Add(this.dtFromOut);
            this.Controls.Add(this.dtDateFrom);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.Name = "FSettingLine";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ตั้งค่าวัน/เวลาน้ำหนักตามสาย";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbbSite;
        private System.Windows.Forms.Button btSaveLine;
        private System.Windows.Forms.Label label37;
        private System.Windows.Forms.DateTimePicker dtFromOut;
        private System.Windows.Forms.DateTimePicker dtDateFrom;
        private System.Windows.Forms.Label label1;
    }
}