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
    public partial class FPrintTransportByTeamReport : Form
    {
        Microsoft.Reporting.WinForms.ReportDataSource _rs = new Microsoft.Reporting.WinForms.ReportDataSource();
        public FPrintTransportByTeamReport(Microsoft.Reporting.WinForms.ReportDataSource rs)
        {
            InitializeComponent();
            _rs = rs;
        }

        private void FPrintTransportByTeamReport_Load(object sender, EventArgs e)
        {
            Microsoft.Reporting.WinForms.ReportParameter[] p = new Microsoft.Reporting.WinForms.ReportParameter[] {
                new Microsoft.Reporting.WinForms.ReportParameter("PDateFrom",WeightTempReport.DateFrom),
                new Microsoft.Reporting.WinForms.ReportParameter("PDateTo",WeightTempReport.DateTo),
            };

            this.rvTransportByTeamReport.SetDisplayMode(Microsoft.Reporting.WinForms.DisplayMode.PrintLayout);
            this.rvTransportByTeamReport.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.Percent;
            this.rvTransportByTeamReport.LocalReport.DataSources.Clear();
            this.rvTransportByTeamReport.LocalReport.DataSources.Add(_rs);
            this.rvTransportByTeamReport.LocalReport.SetParameters(p);
            this.rvTransportByTeamReport.LocalReport.DisplayName = "รายงานขนส่งตามทีมประจำวันที่ " + WeightTempReport.DateFrom.Replace('/', '-') + " ถึง " + WeightTempReport.DateTo.Replace('/', '-');

            this.rvTransportByTeamReport.RefreshReport();
        }
    }
}
