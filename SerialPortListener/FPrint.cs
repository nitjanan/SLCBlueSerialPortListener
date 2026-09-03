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
using Devart.Data.PostgreSql;

using System.Data.Odbc;

namespace SerialPortListener
{
    public partial class FPrint : Form
    {
        Datalayer dl;
        public FPrint()
        {
            dl = new Datalayer();
            InitializeComponent();
        }

        private void FPrint_Load(object sender, EventArgs e)
        {
            Microsoft.Reporting.WinForms.ReportParameter[] p = new Microsoft.Reporting.WinForms.ReportParameter[] {
                new Microsoft.Reporting.WinForms.ReportParameter("PCompanyName",Company.CompanyName),
                new Microsoft.Reporting.WinForms.ReportParameter("PAddress",GetReportAddress()),
                new Microsoft.Reporting.WinForms.ReportParameter("PTelephone",Company.Telephone),
                new Microsoft.Reporting.WinForms.ReportParameter("PEmail",Company.Email),
                new Microsoft.Reporting.WinForms.ReportParameter("PDocNum",Weight.DocNum),
                new Microsoft.Reporting.WinForms.ReportParameter("PMill",Weight.Mill),
                new Microsoft.Reporting.WinForms.ReportParameter("PDate",Weight.Date),
                new Microsoft.Reporting.WinForms.ReportParameter("PDriverName",Weight.DriverName),
                new Microsoft.Reporting.WinForms.ReportParameter("PCustomerName",Weight.CustomerName),
                new Microsoft.Reporting.WinForms.ReportParameter("PStoneType",Weight.StoneType),
                new Microsoft.Reporting.WinForms.ReportParameter("PStoneDesc",Weight.StoneDesc),
                new Microsoft.Reporting.WinForms.ReportParameter("PScoopName",Weight.ScoopName),
                new Microsoft.Reporting.WinForms.ReportParameter("PCar",Weight.CarLicense),
                new Microsoft.Reporting.WinForms.ReportParameter("PCity",Weight.CarCity),
                new Microsoft.Reporting.WinForms.ReportParameter("PDateIn",Weight.DateIn),
                new Microsoft.Reporting.WinForms.ReportParameter("PDateOut",Weight.DateOut),
                new Microsoft.Reporting.WinForms.ReportParameter("PTimeIn",Weight.TimeIn),
                new Microsoft.Reporting.WinForms.ReportParameter("PTimeOut",Weight.TimeOut),
                new Microsoft.Reporting.WinForms.ReportParameter("PWeightIn",Weight.WeightIn),
                new Microsoft.Reporting.WinForms.ReportParameter("PWeightOut",Weight.WeightOut),
                new Microsoft.Reporting.WinForms.ReportParameter("PWeightTotal",Weight.WeightTotal),
                new Microsoft.Reporting.WinForms.ReportParameter("PPrice",Weight.Price),
                new Microsoft.Reporting.WinForms.ReportParameter("PAmount",Weight.Amount),
                new Microsoft.Reporting.WinForms.ReportParameter("PVat",Weight.Vat),
                new Microsoft.Reporting.WinForms.ReportParameter("PAmountVat",Weight.AmountVat),
                new Microsoft.Reporting.WinForms.ReportParameter("PQ",Weight.Q),
                new Microsoft.Reporting.WinForms.ReportParameter("PPay",Weight.Pay),
                new Microsoft.Reporting.WinForms.ReportParameter("PVatType",Weight.VatType),
                new Microsoft.Reporting.WinForms.ReportParameter("PCustomerAddress",Weight.CustomerAddress),
                new Microsoft.Reporting.WinForms.ReportParameter("PCustomerSend",Weight.Site),
                new Microsoft.Reporting.WinForms.ReportParameter("PTeam",Weight.Team),
                new Microsoft.Reporting.WinForms.ReportParameter("PStoneColor",Weight.StoneColor),
                new Microsoft.Reporting.WinForms.ReportParameter("PApproveName",Weight.ApproveName),
                new Microsoft.Reporting.WinForms.ReportParameter("PClean",Weight.Clean),
                new Microsoft.Reporting.WinForms.ReportParameter("PTransport",Weight.Transport),
                new Microsoft.Reporting.WinForms.ReportParameter("POilContent",Weight.OilContent),
                new Microsoft.Reporting.WinForms.ReportParameter("TTelephone",Company.TTelephone),
                new Microsoft.Reporting.WinForms.ReportParameter("TEmail",Company.TEmail),
                new Microsoft.Reporting.WinForms.ReportParameter("TDocName",Company.TDocName),
                new Microsoft.Reporting.WinForms.ReportParameter("TLogo",Company.TLogo),
                new Microsoft.Reporting.WinForms.ReportParameter("PDatePrintAndCopyNum",Weight.DatePrintAndCopyNum),
            };
            //this.reportViewer1.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.PageWidth;
            System.Drawing.Printing.PageSettings ps = new System.Drawing.Printing.PageSettings();
            //ps.Landscape = false;

            /* กระดาษต่อเนื่อง
            ps.Margins = new System.Drawing.Printing.Margins(35, 35, 25, 25);
            ps.PaperSize = new System.Drawing.Printing.PaperSize("CustomType", 827, 550);
              กระดาษต่อเนื่อง */
            //ps.PaperSize.RawKind = (int)System.Drawing.Printing.PaperKind.Standard9x11;

            ps.Margins = new System.Drawing.Printing.Margins(46, 46, 60, 30);
            this.reportViewer1.SetPageSettings(ps);

            this.reportViewer1.SetDisplayMode(Microsoft.Reporting.WinForms.DisplayMode.PrintLayout);
            this.reportViewer1.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.Percent;
            this.reportViewer1.Clear();
            this.reportViewer1.LocalReport.SetParameters(p);
            this.reportViewer1.LocalReport.DisplayName = "ใบชั่งน้ำหนักสินค้า";
            this.reportViewer1.RefreshReport();
        }

        private string GetReportAddress()
        {
            string address = Company.Address;
            if (!string.IsNullOrEmpty(Weight.DoId) && 
                Weight.DoId.Trim() != "" && 
                !Weight.DoId.Equals("NULL", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    dl.connect();
                    using (OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand())
                    {
                        pgCommand.CommandText = "SELECT site_id, site_name FROM public.delivery_order WHERE do_id = ?";
                        pgCommand.Parameters.AddWithValue("do_id", Weight.DoId);
                        using (OdbcDataReader reader = pgCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string siteId = reader["site_id"].ToString();
                                string siteName = reader["site_name"].ToString();
                                if (siteId == "-")
                                {
                                    address = siteName;
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Fallback to default Company.Address
                }
                finally
                {
                    dl.close();
                }
            }
            return address;
        }
    }
}
