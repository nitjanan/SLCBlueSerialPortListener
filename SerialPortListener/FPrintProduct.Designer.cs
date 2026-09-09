
namespace SerialPortListener
{
    partial class FPrintProduct
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
            this.rvProductReport = new Microsoft.Reporting.WinForms.ReportViewer();
            this.SuspendLayout();
            // 
            // rvProductReport
            // 
            this.rvProductReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rvProductReport.LocalReport.ReportEmbeddedResource = "SerialPortListener.ProductReport.rdlc";
            this.rvProductReport.Location = new System.Drawing.Point(0, 0);
            this.rvProductReport.Name = "rvProductReport";
            this.rvProductReport.ServerReport.BearerToken = null;
            this.rvProductReport.Size = new System.Drawing.Size(800, 450);
            this.rvProductReport.TabIndex = 0;
            // 
            // FPrintProduct
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.rvProductReport);
            this.Name = "FPrintProduct";
            this.Text = "รายงานแยกตามสินค้า";
            this.Load += new System.EventHandler(this.FPrintProduct_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Reporting.WinForms.ReportViewer rvProductReport;
    }
}