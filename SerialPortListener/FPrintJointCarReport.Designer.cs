namespace SerialPortListener
{
    partial class FPrintJointCarReport
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
            this.rvJointCarReport = new Microsoft.Reporting.WinForms.ReportViewer();
            this.SuspendLayout();
            // 
            // rvJointCarReport
            // 
            this.rvJointCarReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rvJointCarReport.LocalReport.ReportEmbeddedResource = "SerialPortListener.JointCarReport.rdlc";
            this.rvJointCarReport.Location = new System.Drawing.Point(0, 0);
            this.rvJointCarReport.Name = "rvJointCarReport";
            this.rvJointCarReport.ServerReport.BearerToken = null;
            this.rvJointCarReport.Size = new System.Drawing.Size(800, 450);
            this.rvJointCarReport.TabIndex = 0;
            // 
            // FPrintJointCarReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.rvJointCarReport);
            this.Name = "FPrintJointCarReport";
            this.Text = "รายงานทะเบียนรถร่วม";
            this.Load += new System.EventHandler(this.FPrintJointCarReport_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Reporting.WinForms.ReportViewer rvJointCarReport;
    }
}