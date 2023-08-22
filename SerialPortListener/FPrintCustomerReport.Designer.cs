namespace SerialPortListener
{
    partial class FPrintCustomerReport
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
            this.weightScoopDataSet = new SerialPortListener.weightScoopDataSet();
            this.rvCustomerReport = new Microsoft.Reporting.WinForms.ReportViewer();
            this.weightTableAdapter = new SerialPortListener.weightScoopDataSetTableAdapters.weightTableAdapter();
            ((System.ComponentModel.ISupportInitialize)(this.weightBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.weightScoopDataSet)).BeginInit();
            this.SuspendLayout();
            // 
            // weightBindingSource
            // 
            this.weightBindingSource.DataMember = "weight";
            this.weightBindingSource.DataSource = this.weightScoopDataSet;
            // 
            // weightScoopDataSet
            // 
            this.weightScoopDataSet.DataSetName = "weightScoopDataSet";
            this.weightScoopDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // rvCustomerReport
            // 
            this.rvCustomerReport.Dock = System.Windows.Forms.DockStyle.Fill;
            reportDataSource1.Name = "customerDataSet";
            reportDataSource1.Value = this.weightBindingSource;
            this.rvCustomerReport.LocalReport.DataSources.Add(reportDataSource1);
            this.rvCustomerReport.LocalReport.ReportEmbeddedResource = "SerialPortListener.CustomerReport.rdlc";
            this.rvCustomerReport.Location = new System.Drawing.Point(0, 0);
            this.rvCustomerReport.Name = "rvCustomerReport";
            this.rvCustomerReport.ServerReport.BearerToken = null;
            this.rvCustomerReport.Size = new System.Drawing.Size(800, 450);
            this.rvCustomerReport.TabIndex = 0;
            // 
            // weightTableAdapter
            // 
            this.weightTableAdapter.ClearBeforeFill = true;
            // 
            // FPrintCustomerReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.rvCustomerReport);
            this.Name = "FPrintCustomerReport";
            this.Text = "รายงานการชั่งสินค้าตามลูกค้าประจำวัน";
            this.Load += new System.EventHandler(this.FPrintCustomerReport_Load);
            ((System.ComponentModel.ISupportInitialize)(this.weightBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.weightScoopDataSet)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Reporting.WinForms.ReportViewer rvCustomerReport;
        private System.Windows.Forms.BindingSource weightBindingSource;
        private weightScoopDataSet weightScoopDataSet;
        private weightScoopDataSetTableAdapters.weightTableAdapter weightTableAdapter;
    }
}