
namespace SerialPortListener
{
    partial class FPrintCCReport
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
            this.rvCCReport = new Microsoft.Reporting.WinForms.ReportViewer();
            this.SuspendLayout();
            // 
            // rvCCReport
            // 
            this.rvCCReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rvCCReport.LocalReport.ReportEmbeddedResource = "SerialPortListener.CustomerCompanyReport.rdlc";
            this.rvCCReport.Location = new System.Drawing.Point(0, 0);
            this.rvCCReport.Name = "rvCCReport";
            this.rvCCReport.ServerReport.BearerToken = null;
            this.rvCCReport.Size = new System.Drawing.Size(800, 450);
            this.rvCCReport.TabIndex = 0;
            // 
            // FPrintCCReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.rvCCReport);
            this.Name = "FPrintCCReport";
            this.Text = "รายงานตามบริษัทและการขนส่ง";
            this.Load += new System.EventHandler(this.FPrintCCReport_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Reporting.WinForms.ReportViewer rvCCReport;
    }
}