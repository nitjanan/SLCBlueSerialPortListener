
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
            this.components = new System.ComponentModel.Container();
            Microsoft.Reporting.WinForms.ReportDataSource reportDataSource1 = new Microsoft.Reporting.WinForms.ReportDataSource();
            this.weightBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.ReportDataSet = new SerialPortListener.ReportDataSet();
            this.rvCorpReport = new Microsoft.Reporting.WinForms.ReportViewer();
            this.weightTableAdapter = new SerialPortListener.ReportDataSetTableAdapters.weightTableAdapter();
            ((System.ComponentModel.ISupportInitialize)(this.weightBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ReportDataSet)).BeginInit();
            this.SuspendLayout();
            // 
            // weightBindingSource
            // 
            this.weightBindingSource.DataMember = "weight";
            this.weightBindingSource.DataSource = this.ReportDataSet;
            // 
            // ReportDataSet
            // 
            this.ReportDataSet.DataSetName = "ReportDataSet";
            this.ReportDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // rvCorpReport
            // 
            this.rvCorpReport.Dock = System.Windows.Forms.DockStyle.Fill;
            reportDataSource1.Name = "CorpDataSet";
            reportDataSource1.Value = this.weightBindingSource;
            this.rvCorpReport.LocalReport.DataSources.Add(reportDataSource1);
            this.rvCorpReport.LocalReport.ReportEmbeddedResource = "SerialPortListener.CustomerCorporationReport.rdlc";
            this.rvCorpReport.Location = new System.Drawing.Point(0, 0);
            this.rvCorpReport.Name = "rvCorpReport";
            this.rvCorpReport.ServerReport.BearerToken = null;
            this.rvCorpReport.Size = new System.Drawing.Size(800, 450);
            this.rvCorpReport.TabIndex = 0;
            // 
            // weightTableAdapter
            // 
            this.weightTableAdapter.ClearBeforeFill = true;
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
            ((System.ComponentModel.ISupportInitialize)(this.weightBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ReportDataSet)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Reporting.WinForms.ReportViewer rvCorpReport;
        private System.Windows.Forms.BindingSource weightBindingSource;
        private ReportDataSet ReportDataSet;
        private ReportDataSetTableAdapters.weightTableAdapter weightTableAdapter;
    }
}