namespace SerialPortListener
{
    partial class FPrintCleanReport
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
            Microsoft.Reporting.WinForms.ReportDataSource reportDataSource1 = new Microsoft.Reporting.WinForms.ReportDataSource();
            this.rvCleanReport = new Microsoft.Reporting.WinForms.ReportViewer();
            this.cleanDataSet = new SerialPortListener.cleanDataSet();
            this.weightBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.weightTableAdapter = new SerialPortListener.cleanDataSetTableAdapters.weightTableAdapter();
            ((System.ComponentModel.ISupportInitialize)(this.cleanDataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.weightBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // rvCleanReport
            // 
            this.rvCleanReport.Dock = System.Windows.Forms.DockStyle.Fill;
            reportDataSource1.Name = "CleanDataSet";
            reportDataSource1.Value = this.weightBindingSource;
            this.rvCleanReport.LocalReport.DataSources.Add(reportDataSource1);
            this.rvCleanReport.LocalReport.ReportEmbeddedResource = "SerialPortListener.CleanReport.rdlc";
            this.rvCleanReport.Location = new System.Drawing.Point(0, 0);
            this.rvCleanReport.Name = "rvCleanReport";
            this.rvCleanReport.ServerReport.BearerToken = null;
            this.rvCleanReport.Size = new System.Drawing.Size(800, 450);
            this.rvCleanReport.TabIndex = 0;
            // 
            // cleanDataSet
            // 
            this.cleanDataSet.DataSetName = "cleanDataSet";
            this.cleanDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // weightBindingSource
            // 
            this.weightBindingSource.DataMember = "weight";
            this.weightBindingSource.DataSource = this.cleanDataSet;
            // 
            // weightTableAdapter
            // 
            this.weightTableAdapter.ClearBeforeFill = true;
            // 
            // FPrintCleanReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.rvCleanReport);
            this.Name = "FPrintCleanReport";
            this.Text = "รายงานล้างสเปร์ย";
            this.Load += new System.EventHandler(this.FPrintCleanReport_Load);
            ((System.ComponentModel.ISupportInitialize)(this.cleanDataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.weightBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Reporting.WinForms.ReportViewer rvCleanReport;
        private System.Windows.Forms.BindingSource weightBindingSource;
        private cleanDataSet cleanDataSet;
        private cleanDataSetTableAdapters.weightTableAdapter weightTableAdapter;
    }
}