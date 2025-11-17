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
    public partial class FPrintCarryReport : Form
    {
        Microsoft.Reporting.WinForms.ReportDataSource _rs = new Microsoft.Reporting.WinForms.ReportDataSource();
        public FPrintCarryReport(Microsoft.Reporting.WinForms.ReportDataSource rs)
        {
            InitializeComponent();
            _rs = rs;
        }

        private void FPrintCarryReport_Load(object sender, EventArgs e)
        {
            Microsoft.Reporting.WinForms.ReportParameter[] p = new Microsoft.Reporting.WinForms.ReportParameter[] {
                new Microsoft.Reporting.WinForms.ReportParameter("PDateFrom",WeightTempReport.DateFrom),
                new Microsoft.Reporting.WinForms.ReportParameter("PDateTo",WeightTempReport.DateTo),
                new Microsoft.Reporting.WinForms.ReportParameter("PMainComp",WeightTempReport.MainComp), //set หัวกระดาษรายงาน 17-11-2025
            };

            this.rvCarry.SetDisplayMode(Microsoft.Reporting.WinForms.DisplayMode.PrintLayout);
            this.rvCarry.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.Percent;
            this.rvCarry.LocalReport.DataSources.Clear();
            this.rvCarry.LocalReport.DataSources.Add(_rs);
            this.rvCarry.LocalReport.SetParameters(p);
            this.rvCarry.LocalReport.DisplayName = "รายงานการขนส่งประจำวันที่ " + WeightTempReport.DateFrom.Replace('/', '-') + " ถึง " + WeightTempReport.DateTo.Replace('/', '-');
            this.rvCarry.RefreshReport();
        }
    }
}
