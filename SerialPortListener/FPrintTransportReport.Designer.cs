namespace SerialPortListener
{
    partial class FPrintTransportReport
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
            this.rvTransportReport = new Microsoft.Reporting.WinForms.ReportViewer();
            this.SuspendLayout();
            // 
            // rvTransportReport
            // 
            this.rvTransportReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rvTransportReport.LocalReport.ReportEmbeddedResource = "SerialPortListener.TransportReport.rdlc";
            this.rvTransportReport.Location = new System.Drawing.Point(0, 0);
            this.rvTransportReport.Name = "rvTransportReport";
            this.rvTransportReport.ServerReport.BearerToken = null;
            this.rvTransportReport.Size = new System.Drawing.Size(800, 450);
            this.rvTransportReport.TabIndex = 0;
            // 
            // FPrintTransportReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.rvTransportReport);
            this.Name = "FPrintTransportReport";
            this.Text = "รายงานขนส่ง";
            this.Load += new System.EventHandler(this.FPrintTransportReport_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Reporting.WinForms.ReportViewer rvTransportReport;
    }
}