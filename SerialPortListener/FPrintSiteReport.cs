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
    public partial class FPrintSiteReport : Form
    {
        Microsoft.Reporting.WinForms.ReportDataSource _rs = new Microsoft.Reporting.WinForms.ReportDataSource();
        public FPrintSiteReport(Microsoft.Reporting.WinForms.ReportDataSource rs)
        {
            InitializeComponent();
            _rs = rs;
        }

        private void FPrintSiteReport_Load(object sender, EventArgs e)
        {
            
            Microsoft.Reporting.WinForms.ReportParameter[] p = new Microsoft.Reporting.WinForms.ReportParameter[] {
                new Microsoft.Reporting.WinForms.ReportParameter("PDateFrom",WeightTempReport.DateFrom),
                new Microsoft.Reporting.WinForms.ReportParameter("PDateTo",WeightTempReport.DateTo),
            };
            

            this.rvSiteReport.SetDisplayMode(Microsoft.Reporting.WinForms.DisplayMode.PrintLayout);
            this.rvSiteReport.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.Percent;
            this.rvSiteReport.LocalReport.DataSources.Clear();
            this.rvSiteReport.LocalReport.DataSources.Add(_rs);
            this.rvSiteReport.LocalReport.SetParameters(p);
            this.rvSiteReport.LocalReport.DisplayName = "รายงานสรุปแยกหน้างานประจำวันที่ " + WeightTempReport.DateFrom.Replace('/', '-') + " ถึง " + WeightTempReport.DateTo.Replace('/', '-');
            this.rvSiteReport.RefreshReport();
        }
    }
}
