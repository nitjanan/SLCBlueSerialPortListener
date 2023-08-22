namespace SerialPortListener
{
    partial class FPrintMillReport
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
            this.truckDataSet = new SerialPortListener.truckDataSet();
            this.rvMillReport = new Microsoft.Reporting.WinForms.ReportViewer();
            this.weightTableAdapter = new SerialPortListener.truckDataSetTableAdapters.weightTableAdapter();
            ((System.ComponentModel.ISupportInitialize)(this.weightBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.truckDataSet)).BeginInit();
            this.SuspendLayout();
            // 
            // weightBindingSource
            // 
            this.weightBindingSource.DataMember = "weight";
            this.weightBindingSource.DataSource = this.truckDataSet;
            // 
            // truckDataSet
            // 
            this.truckDataSet.DataSetName = "truckDataSet";
            this.truckDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // rvMillReport
            // 
            this.rvMillReport.Dock = System.Windows.Forms.DockStyle.Fill;
            reportDataSource1.Name = "millDataSet";
            reportDataSource1.Value = this.weightBindingSource;
            this.rvMillReport.LocalReport.DataSources.Add(reportDataSource1);
            this.rvMillReport.LocalReport.ReportEmbeddedResource = "SerialPortListener.MillReport.rdlc";
            this.rvMillReport.Location = new System.Drawing.Point(0, 0);
            this.rvMillReport.Name = "rvMillReport";
            this.rvMillReport.ServerReport.BearerToken = null;
            this.rvMillReport.Size = new System.Drawing.Size(800, 450);
            this.rvMillReport.TabIndex = 0;
            // 
            // weightTableAdapter
            // 
            this.weightTableAdapter.ClearBeforeFill = true;
            // 
            // FPrintMillReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.rvMillReport);
            this.Name = "FPrintMillReport";
            this.Text = "รายงานตามโรงโม่";
            this.Load += new System.EventHandler(this.FPrintMillReport_Load);
            ((System.ComponentModel.ISupportInitialize)(this.weightBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.truckDataSet)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Reporting.WinForms.ReportViewer rvMillReport;
        private System.Windows.Forms.BindingSource weightBindingSource;
        private truckDataSet truckDataSet;
        private truckDataSetTableAdapters.weightTableAdapter weightTableAdapter;
    }
}