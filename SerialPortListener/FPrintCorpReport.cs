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
    public partial class FPrintCorpReport : Form
    {
        Microsoft.Reporting.WinForms.ReportDataSource _rs = new Microsoft.Reporting.WinForms.ReportDataSource();
        public FPrintCorpReport(Microsoft.Reporting.WinForms.ReportDataSource rs)
        {
            InitializeComponent();
            _rs = rs;
        }

        private void FPrintCorpReport_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'truckDataSet.weight' table. You can move, or remove it, as needed.
            Microsoft.Reporting.WinForms.ReportParameter[] p = new Microsoft.Reporting.WinForms.ReportParameter[] {
                new Microsoft.Reporting.WinForms.ReportParameter("PDateFrom",WeightTempReport.DateFrom),
                new Microsoft.Reporting.WinForms.ReportParameter("PDateTo",WeightTempReport.DateTo),
                new Microsoft.Reporting.WinForms.ReportParameter("PMainComp",WeightTempReport.MainComp), //set หัวกระดาษรายงาน 17-11-2025
            };

            this.rvCorpReport.SetDisplayMode(Microsoft.Reporting.WinForms.DisplayMode.PrintLayout);
            this.rvCorpReport.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.Percent;
            this.rvCorpReport.LocalReport.DataSources.Clear();
            this.rvCorpReport.LocalReport.DataSources.Add(_rs);
            this.rvCorpReport.LocalReport.SetParameters(p);
            this.rvCorpReport.LocalReport.DisplayName = "รายงานตามบริษัทประจำวันที่ " + WeightTempReport.DateFrom.Replace('/', '-') + " ถึง " + WeightTempReport.DateTo.Replace('/', '-');
            this.rvCorpReport.RefreshReport();
        }
    }
}
