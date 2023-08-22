using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SerialPortListener
{
    public partial class FPrintCustomerReport : Form
    {
        Microsoft.Reporting.WinForms.ReportDataSource _rs = new Microsoft.Reporting.WinForms.ReportDataSource();
        public FPrintCustomerReport(Microsoft.Reporting.WinForms.ReportDataSource rs)
        {
            InitializeComponent();
            _rs = rs;
        }

        private void FPrintCustomerReport_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'weightScoopDataSet.weight' table. You can move, or remove it, as needed.
            //this.weightTableAdapter.Fill(this.weightScoopDataSet.weight);

            Microsoft.Reporting.WinForms.ReportParameter[] p = new Microsoft.Reporting.WinForms.ReportParameter[] {
                new Microsoft.Reporting.WinForms.ReportParameter("PDateFrom",WeightTempReport.DateFrom),
                new Microsoft.Reporting.WinForms.ReportParameter("PDateTo",WeightTempReport.DateTo),
            };

            this.rvCustomerReport.SetDisplayMode(Microsoft.Reporting.WinForms.DisplayMode.PrintLayout);
            this.rvCustomerReport.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.Percent;
            this.rvCustomerReport.LocalReport.DataSources.Clear();
            this.rvCustomerReport.LocalReport.DataSources.Add(_rs);
            this.rvCustomerReport.LocalReport.SetParameters(p);
            this.rvCustomerReport.LocalReport.DisplayName = "รายงานการชั่งสินค้าตามลูกค้าประจำวันที่ " + WeightTempReport.DateFrom.Replace('/', '-') + " ถึง " + WeightTempReport.DateTo.Replace('/', '-');
            this.rvCustomerReport.RefreshReport();
        }
    }
}
