namespace SerialPortListener
{
    partial class FPrintSiteReport
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
            this.components = new System.ComponentModel.Container();
            Microsoft.Reporting.WinForms.ReportDataSource reportDataSource3 = new Microsoft.Reporting.WinForms.ReportDataSource();
            this.rvSiteReport = new Microsoft.Reporting.WinForms.ReportViewer();
            this.truckDataSet = new SerialPortListener.truckDataSet();
            this.weightBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.weightTableAdapter = new SerialPortListener.truckDataSetTableAdapters.weightTableAdapter();
            ((System.ComponentModel.ISupportInitialize)(this.truckDataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.weightBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // rvSiteReport
            // 
            this.rvSiteReport.Dock = System.Windows.Forms.DockStyle.Fill;
            reportDataSource3.Name = "siteDataSet";
            reportDataSource3.Value = this.weightBindingSource;
            this.rvSiteReport.LocalReport.DataSources.Add(reportDataSource3);
            this.rvSiteReport.LocalReport.ReportEmbeddedResource = "SerialPortListener.SiteReport.rdlc";
            this.rvSiteReport.Location = new System.Drawing.Point(0, 0);
            this.rvSiteReport.Name = "rvSiteReport";
            this.rvSiteReport.ServerReport.BearerToken = null;
            this.rvSiteReport.Size = new System.Drawing.Size(800, 450);
            this.rvSiteReport.TabIndex = 0;
            // 
            // truckDataSet
            // 
            this.truckDataSet.DataSetName = "truckDataSet";
            this.truckDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // weightBindingSource
            // 
            this.weightBindingSource.DataMember = "weight";
            this.weightBindingSource.DataSource = this.truckDataSet;
            // 
            // weightTableAdapter
            // 
            this.weightTableAdapter.ClearBeforeFill = true;
            // 
            // FPrintSiteReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.rvSiteReport);
            this.Name = "FPrintSiteReport";
            this.Text = "รายงานสรุปแยกหน้างาน";
            this.Load += new System.EventHandler(this.FPrintSiteReport_Load);
            ((System.ComponentModel.ISupportInitialize)(this.truckDataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.weightBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Reporting.WinForms.ReportViewer rvSiteReport;
        private System.Windows.Forms.BindingSource weightBindingSource;
        private truckDataSet truckDataSet;
        private truckDataSetTableAdapters.weightTableAdapter weightTableAdapter;
    }
}