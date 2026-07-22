
namespace SerialPortListener
{
    partial class FPrintCorpReport
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
            this.rvCorpReport = new Microsoft.Reporting.WinForms.ReportViewer();
            this.SuspendLayout();
            // 
            // rvCorpReport
            // 
            this.rvCorpReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rvCorpReport.LocalReport.ReportEmbeddedResource = "SerialPortListener.CustomerCorporationReport.rdlc";
            this.rvCorpReport.Location = new System.Drawing.Point(0, 0);
            this.rvCorpReport.Name = "rvCorpReport";
            this.rvCorpReport.ServerReport.BearerToken = null;
            this.rvCorpReport.Size = new System.Drawing.Size(800, 450);
            this.rvCorpReport.TabIndex = 0;
            // 
            // FPrintCorpReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.rvCorpReport);
            this.Name = "FPrintCorpReport";
            this.Text = "รายงานตามบริษัท";
            this.Load += new System.EventHandler(this.FPrintCorpReport_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Reporting.WinForms.ReportViewer rvCorpReport;
    }
}