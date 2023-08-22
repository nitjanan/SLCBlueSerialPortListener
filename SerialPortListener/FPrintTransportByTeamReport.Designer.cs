namespace SerialPortListener
{
    partial class FPrintTransportByTeamReport
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
            this.rvTransportByTeamReport = new Microsoft.Reporting.WinForms.ReportViewer();
            this.SuspendLayout();
            // 
            // rvTransportByTeamReport
            // 
            this.rvTransportByTeamReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rvTransportByTeamReport.LocalReport.ReportEmbeddedResource = "SerialPortListener.TransportByTeamReport.rdlc";
            this.rvTransportByTeamReport.Location = new System.Drawing.Point(0, 0);
            this.rvTransportByTeamReport.Name = "rvTransportByTeamReport";
            this.rvTransportByTeamReport.ServerReport.BearerToken = null;
            this.rvTransportByTeamReport.Size = new System.Drawing.Size(800, 450);
            this.rvTransportByTeamReport.TabIndex = 0;
            // 
            // FPrintTransportByTeamReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.rvTransportByTeamReport);
            this.Name = "FPrintTransportByTeamReport";
            this.Text = "รายงานขนส่งตามทีม";
            this.Load += new System.EventHandler(this.FPrintTransportByTeamReport_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Reporting.WinForms.ReportViewer rvTransportByTeamReport;
    }
}