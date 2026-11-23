using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SerialPortListener
{
    // หน้าต่างเลือกวันที่เริ่มต้นและสำเนา ก่อนกู้รายการชั่ง (WeightMirrorRestore)
    // ดึงรายการชั่งที่อัพโหลดแล้วบน server ก่อนเสมอ แล้วจึงเติมจากสำเนาที่เลือก
    // ตาชั่งหลายตัวใช้ comp_code เดียวกัน จึงให้ผู้ใช้เลือกสำเนาที่ถูกต้องเอง (เลือกของเครื่องนี้ไว้ให้ก่อน)
    public class RestoreMirrorForm : Form
    {
        private const string NoMirror = "(ไม่ใช้สำเนา ดึงเฉพาะรายการชั่งที่อัพโหลดแล้ว)";

        private readonly ComboBox cbStation = new ComboBox();
        private readonly DateTimePicker dtFrom = new DateTimePicker();

        // null = ไม่ใช้สำเนา
        public WeightMirrorRestore.StationInfo SelectedStation => cbStation.SelectedItem as WeightMirrorRestore.StationInfo;
        public DateTime DateFrom => dtFrom.Value.Date;

        public RestoreMirrorForm(WeightMirrorRestore.RestoreSource source)
        {
            List<WeightMirrorRestore.StationInfo> stations = source.Mirror;

            Text = "กู้รายการชั่งจากเซิร์ฟเวอร์";
            Font = new Font("Century Gothic", 11F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(680, 360);

            var lbWarn = new Label
            {
                Location = new Point(20, 16),
                Size = new Size(640, 90),
                ForeColor = Color.Firebrick,
                Text = "ใช้เมื่อฐานข้อมูลในเครื่องเสีย ควรกู้ก่อนเริ่มชั่งรายการแรก\r\n" +
                       "1. ดึงรายการชั่งที่อัพโหลดแล้วบนเซิร์ฟเวอร์ของตาชั่ง " + source.StationCode + " ก่อน\r\n" +
                       "2. เติมรายการที่ยังขาดจากสำเนา (รายการที่ยังไม่ได้อัพโหลดตอนเย็น)\r\n" +
                       "เติมเฉพาะรายการที่ไม่มีในเครื่อง ไม่ทับรายการเดิม และตั้งเลข weight_id ให้นับต่อจากเลขสูงสุด",
            };

            var lbFrom = new Label { Location = new Point(20, 120), Size = new Size(140, 28), Text = "ตั้งแต่วันที่ :", TextAlign = ContentAlignment.MiddleRight };
            dtFrom.Location = new Point(170, 120);
            dtFrom.Size = new Size(200, 28);
            dtFrom.Format = DateTimePickerFormat.Short;
            dtFrom.Value = DateTime.Today.AddDays(-7);

            var lbStation = new Label { Location = new Point(20, 165), Size = new Size(140, 28), Text = "สำเนา :", TextAlign = ContentAlignment.MiddleRight };
            cbStation.Location = new Point(170, 165);
            cbStation.Size = new Size(490, 28);
            cbStation.DropDownStyle = ComboBoxStyle.DropDownList;
            // ลำดับ : ไม่ใช้สำเนา ก่อน แล้วตามด้วยสำเนา (ของเครื่องนี้ขึ้นก่อน)
            cbStation.Items.Add(NoMirror);
            cbStation.Items.AddRange(stations.OrderByDescending(s => s.IsThisStation).Cast<object>().ToArray());
            // ค่าเริ่มต้น : ไม่ใช้สำเนา (วันที่เริ่มจะถูกตั้งเป็นวันแรกที่มีข้อมูลบน server ด้านล่าง)
            cbStation.SelectedItem = NoMirror;

            var lbHint = new Label
            {
                Location = new Point(170, 198),
                Size = new Size(490, 70),
                ForeColor = Color.DimGray,
                Text = (stations.Count == 0 ? "ไม่พบสำเนาบนเซิร์ฟเวอร์ จะดึงเฉพาะรายการชั่งที่อัพโหลดแล้ว\r\n" : "") +
                       "สำเนาเก็บเฉพาะรายการไม่เกิน 7 วันก่อนส่ง ; เลือก \"ไม่ใช้สำเนา\" จะตั้งวันที่เริ่มเป็นวันแรกที่มีข้อมูลบนเซิร์ฟเวอร์" +
                       (source.WeightFirstDate.HasValue ? " (" + source.WeightFirstDate.Value.ToString("dd/MM/yyyy") + ")" : ""),
            };

            var btOk = new Button { Text = "กู้ข้อมูล", Location = new Point(400, 305), Size = new Size(125, 36), DialogResult = DialogResult.OK, BackColor = Color.White, ForeColor = Color.Firebrick, FlatStyle = FlatStyle.Flat };
            var btCancel = new Button { Text = "ยกเลิก", Location = new Point(535, 305), Size = new Size(125, 36), DialogResult = DialogResult.Cancel, BackColor = Color.White, FlatStyle = FlatStyle.Flat };
            btOk.Click += (s, e) =>
            {
                if (cbStation.SelectedItem == null)
                {
                    MessageBox.Show(this, "กรุณาเลือกสำเนา", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                }
            };

            // ไม่ใช้สำเนา = กู้จากรายการที่อัพโหลดแล้วอย่างเดียว ตั้งวันที่เริ่มเป็นวันแรกที่มีข้อมูลบน server ให้อัตโนมัติ
            // เลือกสำเนา = กลับไปค่าเริ่มต้น 7 วันก่อน (สำเนาเก็บไม่เกิน 7 วัน)
            EventHandler applyDate = (s, e) =>
            {
                if (ReferenceEquals(cbStation.SelectedItem, NoMirror))
                {
                    if (source.WeightFirstDate.HasValue)
                        dtFrom.Value = source.WeightFirstDate.Value;
                }
                else if (cbStation.SelectedItem != null)
                {
                    dtFrom.Value = DateTime.Today.AddDays(-7);
                }
            };
            cbStation.SelectedIndexChanged += applyDate;
            applyDate(this, EventArgs.Empty);

            Controls.AddRange(new Control[] { lbWarn, lbFrom, dtFrom, lbStation, cbStation, lbHint, btOk, btCancel });
            AcceptButton = btOk;
            CancelButton = btCancel;
        }
    }
}
