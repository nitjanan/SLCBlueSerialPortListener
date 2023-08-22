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
    public partial class FPrintDailyReport : Form
    {
        List<WeightDailyReport> _list = new List<WeightDailyReport>();
        Microsoft.Reporting.WinForms.ReportDataSource _rs = new Microsoft.Reporting.WinForms.ReportDataSource();
        public FPrintDailyReport(List<WeightDailyReport> list, Microsoft.Reporting.WinForms.ReportDataSource rs)
        {
            _list = list;
            _rs = rs;
            InitializeComponent();
        }

        private void FPrintDailyReport_Load(object sender, EventArgs e)
        {
            Microsoft.Reporting.WinForms.ReportParameter[] p = new Microsoft.Reporting.WinForms.ReportParameter[] {
                new Microsoft.Reporting.WinForms.ReportParameter("PDateFrom",WeightDailyReport.DateFrom),
                new Microsoft.Reporting.WinForms.ReportParameter("PDateTo",WeightDailyReport.DateTo),
            };
            this.rvDailyReport.SetDisplayMode(Microsoft.Reporting.WinForms.DisplayMode.PrintLayout);
            this.rvDailyReport.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.Percent;
            this.rvDailyReport.LocalReport.DataSources.Clear();
            this.rvDailyReport.LocalReport.DataSources.Add(_rs);
            this.rvDailyReport.LocalReport.SetParameters(p);
            this.rvDailyReport.LocalReport.DisplayName = "รายงานการชั่งสินค้าประจำวันที่ "+ WeightDailyReport.DateFrom.Replace('/', '-') +" ถึง "+ WeightDailyReport.DateTo.Replace('/', '-');
            this.rvDailyReport.RefreshReport();
        }
    }
}
