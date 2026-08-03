using System;
using System.Drawing;
using System.Windows.Forms;

namespace SerialPortListener
{
    // หน้าต่างแสดงสถานะการดาวน์โหลด/อัพเดทข้อมูล เทียบกับ print() ใน auto_setting_sell.py
    public class frmDownloadProgress : Form
    {
        private TextBox tbLog;
        private Button btClose;

        public frmDownloadProgress()
        {
            Text = "กำลังดาวน์โหลดการตั้งค่า";
            Width = 560;
            Height = 420;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ControlBox = false;

            tbLog = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Top,
                Height = 340,
                Font = new Font("Consolas", 9F)
            };

            btClose = new Button
            {
                Text = "ปิด",
                Dock = DockStyle.Bottom,
                Height = 32,
                Enabled = false
            };
            btClose.Click += (s, e) => Close();

            Controls.Add(tbLog);
            Controls.Add(btClose);
        }

        public void Log(string message)
        {
            tbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
        }

        public void AllowClose()
        {
            btClose.Enabled = true;
            ControlBox = true;
        }
    }
}
