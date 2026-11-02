namespace SerialPortListener
{
    partial class FPrintDailyReport3
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
            this.rvDailyReport = new Microsoft.Reporting.WinForms.ReportViewer();
            this.SuspendLayout();
            // 
            // rvDailyReport
            // 
            this.rvDailyReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rvDailyReport.LocalReport.ReportEmbeddedResource = "SerialPortListener.DailyReport3.rdlc";
            this.rvDailyReport.Location = new System.Drawing.Point(0, 0);
            this.rvDailyReport.Name = "rvDailyReport";
            this.rvDailyReport.ServerReport.BearerToken = null;
            this.rvDailyReport.Size = new System.Drawing.Size(800, 450);
            this.rvDailyReport.TabIndex = 0;
            // 
            // FPrintDailyReport3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.rvDailyReport);
            this.Name = "FPrintDailyReport3";
            this.Text = "รายงานการชั่งสินค้าประจำวัน 3";
            this.Load += new System.EventHandler(this.FPrintDailyReport3_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Reporting.WinForms.ReportViewer rvDailyReport;
    }
}