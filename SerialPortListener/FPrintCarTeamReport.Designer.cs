namespace SerialPortListener
{
    partial class FPrintCarTeamReport
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
            this.rvCarTeamReport = new Microsoft.Reporting.WinForms.ReportViewer();
            this.SuspendLayout();
            // 
            // rvCarTeamReport
            // 
            this.rvCarTeamReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rvCarTeamReport.LocalReport.ReportEmbeddedResource = "SerialPortListener.CarTeamReport.rdlc";
            this.rvCarTeamReport.Location = new System.Drawing.Point(0, 0);
            this.rvCarTeamReport.Name = "rvCarTeamReport";
            this.rvCarTeamReport.ServerReport.BearerToken = null;
            this.rvCarTeamReport.Size = new System.Drawing.Size(800, 450);
            this.rvCarTeamReport.TabIndex = 0;
            // 
            // FPrintCarTeamReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.rvCarTeamReport);
            this.Name = "FPrintCarTeamReport";
            this.Text = "รายงานชื่อทีมรถร่วม";
            this.Load += new System.EventHandler(this.FPrintCarTeamReport_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Reporting.WinForms.ReportViewer rvCarTeamReport;
    }
}