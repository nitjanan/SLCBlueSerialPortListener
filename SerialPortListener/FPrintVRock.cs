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
    public partial class FPrintVRock : Form
    {
        public FPrintVRock()
        {
            InitializeComponent();
        }

        private void FPrintVRock_Load(object sender, EventArgs e)
        {
            Microsoft.Reporting.WinForms.ReportParameter[] p = new Microsoft.Reporting.WinForms.ReportParameter[] {

                new Microsoft.Reporting.WinForms.ReportParameter("PDocNum",Weight.DocNum),
                new Microsoft.Reporting.WinForms.ReportParameter("PCustomerId",Weight.CustomerId),
                new Microsoft.Reporting.WinForms.ReportParameter("PCustomerName",Weight.CustomerName),
                new Microsoft.Reporting.WinForms.ReportParameter("PStoneType",Weight.StoneType),
                new Microsoft.Reporting.WinForms.ReportParameter("PStoneTypeId",Weight.StoneTypeId),
                new Microsoft.Reporting.WinForms.ReportParameter("PCar",Weight.CarLicense),
                new Microsoft.Reporting.WinForms.ReportParameter("PDateIn",Weight.DateIn),
                new Microsoft.Reporting.WinForms.ReportParameter("PDateOut",Weight.DateOut),
                new Microsoft.Reporting.WinForms.ReportParameter("PTimeIn",Weight.TimeIn),
                new Microsoft.Reporting.WinForms.ReportParameter("PTimeOut",Weight.TimeOut),
                new Microsoft.Reporting.WinForms.ReportParameter("PWeightIn",Weight.WeightIn),
                new Microsoft.Reporting.WinForms.ReportParameter("PWeightOut",Weight.WeightOut),
                new Microsoft.Reporting.WinForms.ReportParameter("PWeightTotal",Weight.WeightTotal),
                new Microsoft.Reporting.WinForms.ReportParameter("PPrice",Weight.Price),
                new Microsoft.Reporting.WinForms.ReportParameter("PAmountVat",Weight.AmountVat),
                new Microsoft.Reporting.WinForms.ReportParameter("PNote",Weight.Note),

            };
            //this.reportViewer1.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.PageWidth;
            System.Drawing.Printing.PageSettings ps = new System.Drawing.Printing.PageSettings();
            //ps.Landscape = false;

            /* กระดาษต่อเนื่อง
            ps.Margins = new System.Drawing.Printing.Margins(35, 35, 25, 25);
            ps.PaperSize = new System.Drawing.Printing.PaperSize("CustomType", 827, 550);
              กระดาษต่อเนื่อง */
            //ps.PaperSize.RawKind = (int)System.Drawing.Printing.PaperKind.Standard9x11;

            //กระดาษ A4 ps.Margins = new System.Drawing.Printing.Margins(46, 46, 60, 30);
            ps.Margins = new System.Drawing.Printing.Margins(35, 35, 15, 25);
            ps.PaperSize = new System.Drawing.Printing.PaperSize("CustomType", 827, 550);
            this.reportViewer1.SetPageSettings(ps);


            this.reportViewer1.SetDisplayMode(Microsoft.Reporting.WinForms.DisplayMode.PrintLayout);
            this.reportViewer1.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.Percent;
            this.reportViewer1.Clear();
            this.reportViewer1.LocalReport.SetParameters(p);
            this.reportViewer1.LocalReport.DisplayName = "ใบชั่งน้ำหนักสินค้า";
            this.reportViewer1.RefreshReport();

        }
    }
}
