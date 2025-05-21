
namespace SerialPortListener
{
    partial class FPrintSumLineReport
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
            this.rvSumLineReport = new Microsoft.Reporting.WinForms.ReportViewer();
            this.SuspendLayout();
            // 
            // rvSumLineReport
            // 
            this.rvSumLineReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rvSumLineReport.LocalReport.ReportEmbeddedResource = "SerialPortListener.SumLineReport.rdlc";
            this.rvSumLineReport.Location = new System.Drawing.Point(0, 0);
            this.rvSumLineReport.Name = "rvSumLineReport";
            this.rvSumLineReport.ServerReport.BearerToken = null;
            this.rvSumLineReport.Size = new System.Drawing.Size(800, 450);
            this.rvSumLineReport.TabIndex = 0;
            // 
            // FPrintSumLineReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.rvSumLineReport);
            this.Name = "FPrintSumLineReport";
            this.Text = "รายงานสรุป";
            this.Load += new System.EventHandler(this.FPrintSumLineReport_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Reporting.WinForms.ReportViewer rvSumLineReport;
    }
}