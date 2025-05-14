
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
            this.components = new System.ComponentModel.Container();
            Microsoft.Reporting.WinForms.ReportDataSource reportDataSource1 = new Microsoft.Reporting.WinForms.ReportDataSource();
            this.rvCCReport = new Microsoft.Reporting.WinForms.ReportViewer();
            this.ReportDataSet = new SerialPortListener.ReportDataSet();
            this.weightBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.weightTableAdapter = new SerialPortListener.ReportDataSetTableAdapters.weightTableAdapter();
            ((System.ComponentModel.ISupportInitialize)(this.ReportDataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.weightBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // rvCCReport
            // 
            this.rvCCReport.Dock = System.Windows.Forms.DockStyle.Fill;
            reportDataSource1.Name = "customerCompanyDataSet";
            reportDataSource1.Value = this.weightBindingSource;
            this.rvCCReport.LocalReport.DataSources.Add(reportDataSource1);
            this.rvCCReport.LocalReport.ReportEmbeddedResource = "SerialPortListener.CustomerCompanyReport.rdlc";
            this.rvCCReport.Location = new System.Drawing.Point(0, 0);
            this.rvCCReport.Name = "rvCCReport";
            this.rvCCReport.ServerReport.BearerToken = null;
            this.rvCCReport.Size = new System.Drawing.Size(800, 450);
            this.rvCCReport.TabIndex = 0;
            // 
            // ReportDataSet
            // 
            this.ReportDataSet.DataSetName = "ReportDataSet";
            this.ReportDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // weightBindingSource
            // 
            this.weightBindingSource.DataMember = "weight";
            this.weightBindingSource.DataSource = this.ReportDataSet;
            // 
            // weightTableAdapter
            // 
            this.weightTableAdapter.ClearBeforeFill = true;
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
            ((System.ComponentModel.ISupportInitialize)(this.ReportDataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.weightBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Reporting.WinForms.ReportViewer rvCCReport;
        private System.Windows.Forms.BindingSource weightBindingSource;
        private ReportDataSet ReportDataSet;
        private ReportDataSetTableAdapters.weightTableAdapter weightTableAdapter;
    }
}