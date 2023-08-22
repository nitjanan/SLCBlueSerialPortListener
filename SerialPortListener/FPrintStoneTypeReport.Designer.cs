namespace SerialPortListener
{
    partial class FPrintStoneTypeReport
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
            this.rvStoneTypeReport = new Microsoft.Reporting.WinForms.ReportViewer();
            this.SuspendLayout();
            // 
            // rvStoneTypeReport
            // 
            this.rvStoneTypeReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rvStoneTypeReport.LocalReport.ReportEmbeddedResource = "SerialPortListener.StoneTypeReport.rdlc";
            this.rvStoneTypeReport.Location = new System.Drawing.Point(0, 0);
            this.rvStoneTypeReport.Name = "rvStoneTypeReport";
            this.rvStoneTypeReport.ServerReport.BearerToken = null;
            this.rvStoneTypeReport.Size = new System.Drawing.Size(800, 450);
            this.rvStoneTypeReport.TabIndex = 0;
            // 
            // FPrintStoneTypeReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.rvStoneTypeReport);
            this.Name = "FPrintStoneTypeReport";
            this.Text = "รายงานสรุปตามประเภทหิน";
            this.Load += new System.EventHandler(this.FPrintStoneTypeReport_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Reporting.WinForms.ReportViewer rvStoneTypeReport;
    }
}