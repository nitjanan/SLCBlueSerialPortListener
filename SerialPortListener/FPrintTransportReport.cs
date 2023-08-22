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
    public partial class FPrintTransportReport : Form
    {
        Microsoft.Reporting.WinForms.ReportDataSource _rs = new Microsoft.Reporting.WinForms.ReportDataSource();
        public FPrintTransportReport(Microsoft.Reporting.WinForms.ReportDataSource rs)
        {
            InitializeComponent();
            _rs = rs;
        }

        private void FPrintTransportReport_Load(object sender, EventArgs e)
        {
            Microsoft.Reporting.WinForms.ReportParameter[] p = new Microsoft.Reporting.WinForms.ReportParameter[] {
                new Microsoft.Reporting.WinForms.ReportParameter("PDateFrom",WeightTempReport.DateFrom),
                new Microsoft.Reporting.WinForms.ReportParameter("PDateTo",WeightTempReport.DateTo),
            };

            this.rvTransportReport.SetDisplayMode(Microsoft.Reporting.WinForms.DisplayMode.PrintLayout);
            this.rvTransportReport.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.Percent;
            this.rvTransportReport.LocalReport.DataSources.Clear();
            this.rvTransportReport.LocalReport.DataSources.Add(_rs);
            this.rvTransportReport.LocalReport.SetParameters(p);
            this.rvTransportReport.LocalReport.DisplayName = "รายงานขนส่งประจำวันที่ " + WeightTempReport.DateFrom.Replace('/', '-') + " ถึง " + WeightTempReport.DateTo.Replace('/', '-');
            this.rvTransportReport.RefreshReport();
        }
    }
}
