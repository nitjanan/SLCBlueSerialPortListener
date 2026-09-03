using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using Microsoft.Reporting.WinForms;

namespace SerialPortListener
{
    public class ReportPrintHelper : IDisposable
    {
        private int m_currentPageIndex;
        private IList<Stream> m_streams = new List<Stream>();

        private Stream CreateStream(string name, string fileNameExtension, Encoding encoding, string mimeType, bool willSeek)
        {
            Stream stream = new MemoryStream();
            m_streams.Add(stream);
            return stream;
        }

        public void Export(LocalReport report, double widthInches, double heightInches, double marginLeft, double marginRight, double marginTop, double marginBottom)
        {
            string deviceInfo = string.Format(
                "<DeviceInfo>" +
                "  <OutputFormat>EMF</OutputFormat>" +
                "  <PageWidth>{0}in</PageWidth>" +
                "  <PageHeight>{1}in</PageHeight>" +
                "  <MarginTop>{2}in</MarginTop>" +
                "  <MarginLeft>{3}in</MarginLeft>" +
                "  <MarginRight>{4}in</MarginRight>" +
                "  <MarginBottom>{5}in</MarginBottom>" +
                "</DeviceInfo>",
                widthInches, heightInches, marginTop, marginLeft, marginRight, marginBottom);

            Warning[] warnings;
            m_streams.Clear();
            report.Render("Image", deviceInfo, CreateStream, out warnings);
            foreach (Stream stream in m_streams)
            {
                stream.Position = 0;
            }
        }

        private void PrintPage(object sender, PrintPageEventArgs ev)
        {
            if (m_currentPageIndex >= m_streams.Count)
            {
                ev.HasMorePages = false;
                return;
            }

            // Read the EMF stream as a Metafile image
            using (Metafile pageImage = new Metafile(m_streams[m_currentPageIndex]))
            {
                // Adjust layout rectangle to fit page settings
                Rectangle adjustedRect = new Rectangle(
                    ev.PageBounds.Left - (int)ev.PageSettings.HardMarginX,
                    ev.PageBounds.Top - (int)ev.PageSettings.HardMarginY,
                    ev.PageBounds.Width,
                    ev.PageBounds.Height);

                // Fill background with white to avoid transparent EMF printing as black on some printers
                ev.Graphics.FillRectangle(Brushes.White, adjustedRect);
                ev.Graphics.DrawImage(pageImage, adjustedRect);
            }

            m_currentPageIndex++;
            ev.HasMorePages = (m_currentPageIndex < m_streams.Count);
        }

        public void Print(string printerName = null)
        {
            if (m_streams == null || m_streams.Count == 0)
                throw new Exception("Error: no streams to print.");

            using (PrintDocument printDoc = new PrintDocument())
            {
                if (!string.IsNullOrEmpty(printerName))
                {
                    printDoc.PrinterSettings.PrinterName = printerName;
                }

                printDoc.PrintPage += new PrintPageEventHandler(PrintPage);
                m_currentPageIndex = 0;
                printDoc.Print();
            }
        }

        public void Dispose()
        {
            if (m_streams != null)
            {
                foreach (Stream stream in m_streams)
                {
                    stream.Close();
                    stream.Dispose();
                }
                m_streams = null;
            }
        }
    }
}
