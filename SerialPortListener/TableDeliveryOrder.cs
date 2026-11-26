using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Odbc;

namespace SerialPortListener
{
    public partial class TableDeliveryOrder : Form
    {
        MainForm mainForm;
        Datalayer dl;

        public class DataDO
        {
            public String do_id;
            public String docNo;
            public String deliveryDate;
            public String carCompany;
            public String carCustomer;
            public String customerId;
            public String customerName;
            public String siteId;
            public String siteName;
            public String stoneTypeId;
            public String stoneTypeName;
            public String qty;
            public String unitName;
            public String creditType; // "cash" = เงินสด, "credit-day" = เงินเชื่อ
        }

        // แจ้งเตือนใบส่งของที่เลือกไม่ได้ (ไม่พบรหัสลูกค้า / รหัสหินในฐานข้อมูลเครื่องนี้) หน้าตาเดียวกับหน้า editWeightSell ใน web app
        // สร้างใน code ไม่แก้ Designer เพื่อให้ Blue / Pink ใช้ไฟล์เดียวกันได้
        private static readonly Color MissText = Color.FromArgb(192, 57, 43);
        private static readonly Color MissRowBack = Color.FromArgb(253, 236, 234);
        // ใบส่งของที่ k2m ไม่ระบุเหมือง (delivery_order.mine_unknown = 1): เลือกได้ แต่ไฮไลต์เหลือง + ถามยืนยัน
        private static readonly Color MineRowBack = Color.FromArgb(255, 244, 179);
        private DoWarnPanel pnlDoWarn;
        private OfflineBanner pnlOffline;
        private MineUnknownBanner pnlMineUnknown;
        private Font mineBoldFont; // คอลัมน์เหมือง "ไม่ระบุ" (สร้างครั้งเดียว ไม่สร้างใหม่ทุกครั้งที่วาดเซลล์)
        private int gridTop, gridHeight;

        // ไม่ว่าง = ต่อ API ใบส่งของ (base_api id = 2) ไม่ได้ ตารางนี้แสดงจาก delivery_order ในเครื่อง (ตั้งก่อน ShowDialog)
        public string OfflineReason { get; set; }
        // false = อัปเดตยอดเที่ยว/ยอดหินจาก web app ไม่สำเร็จ
        public bool SummaryUpdated { get; set; } = true;

        public TableDeliveryOrder(MainForm parent)
        {
            InitializeComponent();
            dl = new Datalayer();
            mainForm = parent;
            buildDoWarnPanel();
            Disposed += (s, e) => { if (mineBoldFont != null) mineBoldFont.Dispose(); };
        }

        private void buildDoWarnPanel()
        {
            // คอลัมน์เหมือง (= delivery_order.comp_code = k2m mine_code) ต่อจากเลขที่ใบส่งของ
            // แถวที่ k2m ไม่ระบุเหมืองแสดง "ไม่ระบุ" (ดู dgvDO_CellFormatting) สร้างใน code เพื่อให้ Blue / Pink ใช้ไฟล์เดียวกันได้
            dgvDO.Columns.Insert(dgvDO.Columns["doc_no"].Index + 1, new DataGridViewTextBoxColumn
            {
                Name = "mine_code",
                DataPropertyName = "comp_code",
                HeaderText = "เหมือง",
                Width = 70,
                ReadOnly = true
            });

            gridTop = dgvDO.Top;
            gridHeight = dgvDO.Height;

            pnlDoWarn = new DoWarnPanel
            {
                Location = new Point(dgvDO.Left, gridTop),
                Width = dgvDO.Width,
                BackColor = BackColor,
                Visible = false
            };
            Controls.Add(pnlDoWarn);
            pnlDoWarn.BringToFront();

            pnlOffline = new OfflineBanner
            {
                Location = new Point(dgvDO.Left, gridTop),
                Width = dgvDO.Width,
                Height = OfflineBanner.PreferredHeight,
                BackColor = BackColor,
                Visible = false
            };
            Controls.Add(pnlOffline);
            pnlOffline.BringToFront();

            pnlMineUnknown = new MineUnknownBanner
            {
                Location = new Point(dgvDO.Left, gridTop),
                Width = dgvDO.Width,
                Height = MineUnknownBanner.PreferredHeight,
                BackColor = BackColor,
                Visible = false
            };
            Controls.Add(pnlMineUnknown);
            pnlMineUnknown.BringToFront();
        }

        // จัดตำแหน่ง: แถบออฟไลน์ -> กล่องรหัสที่ไม่พบ -> ตาราง (ใช้พื้นที่เดิมของตารางใน Designer)
        private void layoutDoPanels()
        {
            int y = gridTop;
            int bottom = gridTop + gridHeight;

            pnlOffline.Visible = !string.IsNullOrEmpty(OfflineReason);
            if (pnlOffline.Visible)
            {
                pnlOffline.Reason = OfflineReason;
                pnlOffline.SummaryUpdated = SummaryUpdated;
                pnlOffline.Top = y;
                pnlOffline.Invalidate();
                y += pnlOffline.Height + 6;
            }

            pnlMineUnknown.Visible = pnlMineUnknown.Docs.Count > 0;
            if (pnlMineUnknown.Visible)
            {
                pnlMineUnknown.Top = y;
                pnlMineUnknown.Invalidate();
                y += pnlMineUnknown.Height + 6;
            }

            if (pnlDoWarn.Groups.Count > 0)
            {
                // ตารางต้องเหลือที่อย่างน้อยครึ่งหนึ่งของพื้นที่ที่เหลือ
                int height = Math.Min(pnlDoWarn.MeasureHeight(), (bottom - y) / 2);
                pnlDoWarn.Top = y;
                pnlDoWarn.Height = height;
                pnlDoWarn.Visible = true;
                pnlDoWarn.Invalidate();
                y += height + 6;
            }
            else
            {
                pnlDoWarn.Visible = false;
            }

            dgvDO.Top = y;
            dgvDO.Height = bottom - y;
        }

        private static bool rowFlag(DataRowView row, string column)
        {
            if (row == null || !row.Row.Table.Columns.Contains(column) || row[column] == DBNull.Value)
                return true; // ไม่มีข้อมูล flag = ไม่บล็อก (เหมือนเดิม)
            return Convert.ToInt32(row[column]) == 1;
        }

        // k2m ไม่ระบุเหมืองของใบส่งของนี้ (ไม่มีคอลัมน์ / ค่าว่าง = ระบุแล้ว)
        private static bool isMineUnknown(DataRowView row)
        {
            if (row == null || !row.Row.Table.Columns.Contains("mine_unknown") || row["mine_unknown"] == DBNull.Value)
                return false;
            return Convert.ToInt32(row["mine_unknown"]) == 1;
        }

        private const string MineUnknownTip = "k2m ไม่ได้ระบุเหมืองของใบส่งของนี้ ตรวจสอบก่อนใช้";

        // ข้อความว่าใบส่งของนี้เลือกไม่ได้เพราะอะไร (ว่าง = เลือกได้)
        private static string doMissingText(DataRowView row)
        {
            List<string> msgs = new List<string>();
            if (!rowFlag(row, "has_customer"))
                msgs.Add("ไม่พบรหัสลูกค้า " + (row["customer_code"] ?? "").ToString().Trim() + " ในระบบ");
            if (!rowFlag(row, "has_stone"))
                msgs.Add("ไม่พบรหัสหิน " + (row["product_code"] ?? "").ToString().Trim() + " ในระบบ");
            return string.Join(", ", msgs);
        }

        // สรุปรหัสลูกค้า / รหัสหิน ที่ไม่มีในเครื่องเหนือตาราง ให้ user รู้ว่าตัวแดงคืออะไรและต้องทำอะไร
        private void showDoMissingWarn(DataTable dt)
        {
            var groups = new List<DoMissGroup>();
            var byKey = new Dictionary<string, DoMissGroup>();
            var badDocs = new HashSet<string>();
            var mineDocs = new List<string>();

            foreach (DataRowView row in dt.DefaultView)
            {
                string doc = (row["doc_no"] ?? "").ToString();
                if (isMineUnknown(row))
                    mineDocs.Add(doc);
                if (!rowFlag(row, "has_customer"))
                    addMissing("ลูกค้า", row["customer_code"], row["customer_name"], doc, groups, byKey, badDocs);
                if (!rowFlag(row, "has_stone"))
                    addMissing("ชนิดหิน", row["product_code"], row["product_name"], doc, groups, byKey, badDocs);
            }

            pnlDoWarn.Groups = groups;
            pnlDoWarn.DocCount = badDocs.Count;
            pnlMineUnknown.Docs = mineDocs;
            layoutDoPanels();
        }

        private static void addMissing(string kind, object code, object name, string doc,
            List<DoMissGroup> groups, Dictionary<string, DoMissGroup> byKey, HashSet<string> badDocs)
        {
            string c = (code ?? "").ToString().Trim();
            if (c == "") c = "(ว่าง)";
            string key = kind + "|" + c;
            DoMissGroup g;
            if (!byKey.TryGetValue(key, out g))
            {
                g = new DoMissGroup { Kind = kind, Code = c, Name = (name ?? "").ToString().Trim() };
                byKey[key] = g;
                groups.Add(g);
            }
            g.Docs.Add(doc);
            badDocs.Add(doc);
        }

        private class DoMissGroup
        {
            public string Kind, Code, Name;
            public List<string> Docs = new List<string>();
        }

        // กล่องแจ้งเตือนวาดเอง: การ์ดสีครีม แถบส้มซ้าย ไอคอน "!" หัวข้อ + คำอธิบาย
        // แต่ละรหัสเป็นกรอบ มีป้ายประเภท รหัส/ชื่อสีแดง และเลขที่ใบส่งของเป็นป้ายเล็ก
        private class DoWarnPanel : Panel
        {
            private static readonly Color Back = Color.FromArgb(255, 248, 232);
            private static readonly Color Border = Color.FromArgb(240, 220, 174);
            private static readonly Color Accent = Color.FromArgb(242, 181, 68);
            private static readonly Color Text = Color.FromArgb(91, 69, 16);
            private static readonly Color Muted = Color.FromArgb(138, 109, 43);
            private static readonly Color Code = Color.FromArgb(192, 57, 43);
            private static readonly Color ItemBack = Color.FromArgb(255, 252, 244);

            public List<DoMissGroup> Groups = new List<DoMissGroup>();
            public int DocCount;

            private readonly Font fTitle, fSub, fBadge, fCode, fName, fChip, fHint, fIcon;
            private const TextFormatFlags Tf = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;

            public DoWarnPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
                string ui = fontExists("Leelawadee UI") ? "Leelawadee UI" : "Tahoma";
                string mono = fontExists("Consolas") ? "Consolas" : "Courier New";
                fTitle = new Font(ui, 11F, FontStyle.Bold);
                fSub = new Font(ui, 9F);
                fBadge = new Font(ui, 8.25F, FontStyle.Bold);
                fCode = new Font(mono, 10F, FontStyle.Bold);
                fName = new Font(ui, 10F);
                fChip = new Font(mono, 8.25F);
                fHint = new Font(ui, 9F);
                fIcon = new Font(ui, 10F, FontStyle.Bold);
            }

            internal static bool fontExists(string name)
            {
                using (Font f = new Font(name, 9F))
                    return string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase);
            }

            public int MeasureHeight()
            {
                using (Graphics g = CreateGraphics())
                    return render(g, false);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                render(e.Graphics, true);
            }

            internal static GraphicsPath rounded(Rectangle r, int radius)
            {
                int d = radius * 2;
                GraphicsPath p = new GraphicsPath();
                p.AddArc(r.X, r.Y, d, d, 180, 90);
                p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                p.CloseFigure();
                return p;
            }

            private static Size measure(Graphics g, string s, Font f)
            {
                return TextRenderer.MeasureText(g, s, f, new Size(int.MaxValue, int.MaxValue), Tf);
            }

            private int render(Graphics g, bool draw)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int w = ClientSize.Width;

                if (draw)
                {
                    Rectangle card = new Rectangle(0, 0, w - 1, ClientSize.Height - 1);
                    using (GraphicsPath p = rounded(card, 6))
                    using (SolidBrush b = new SolidBrush(Back))
                    using (Pen pen = new Pen(Border))
                    {
                        g.FillPath(b, p);
                        g.DrawPath(pen, p);
                    }
                    using (GraphicsPath p = rounded(new Rectangle(0, 0, 5, ClientSize.Height - 1), 2))
                    using (SolidBrush b = new SolidBrush(Accent))
                        g.FillPath(b, p);
                }

                int x = 18, y = 8;

                // ไอคอน "!"
                if (draw)
                {
                    Rectangle icon = new Rectangle(x, y, 22, 22);
                    using (SolidBrush b = new SolidBrush(Accent))
                        g.FillEllipse(b, icon);
                    TextRenderer.DrawText(g, "!", fIcon, icon, Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }

                int tx = x + 32;
                string title = "มี " + DocCount + " ใบส่งของที่ยังเลือกไม่ได้";
                Size ts = measure(g, title, fTitle);
                // คำอธิบายสีจางต่อท้ายหัวข้อในบรรทัดเดียวกัน (ประหยัดความสูง ให้ตารางเหลือที่มากขึ้น)
                string sub = "รหัสในใบส่งของไม่มีในระบบ (แสดงเป็นตัวหนังสือสีแดงในตาราง)";
                Size ss = measure(g, sub, fSub);
                int titleH = Math.Max(22, ts.Height);
                if (draw)
                {
                    TextRenderer.DrawText(g, title, fTitle, new Point(tx, y + (titleH - ts.Height) / 2), Text, Tf);
                    TextRenderer.DrawText(g, sub, fSub, new Point(tx + ts.Width + 12, y + (titleH - ss.Height) / 2 + 1), Muted, Tf);
                }
                y += titleH + 6;

                int itemW = w - tx - 14;
                foreach (DoMissGroup grp in Groups)
                {
                    int h = renderItem(g, grp, tx, y, itemW, false);
                    if (draw)
                    {
                        using (GraphicsPath p = rounded(new Rectangle(tx, y, itemW, h), 5))
                        using (SolidBrush b = new SolidBrush(ItemBack))
                        using (Pen pen = new Pen(Border))
                        {
                            g.FillPath(b, p);
                            g.DrawPath(pen, p);
                        }
                        renderItem(g, grp, tx, y, itemW, true);
                    }
                    y += h + 4;
                }

                string hint = "เพิ่มข้อมูลลูกค้า / ชนิดหิน ให้ตรงกับรหัสก่อน แล้วกด \"ค้นหา\" ใหม่";
                Size hs = measure(g, hint, fHint);
                y += 2;
                if (draw) TextRenderer.DrawText(g, hint, fHint, new Point(tx, y), Muted, Tf);
                y += hs.Height + 8;
                return y;
            }

            // บรรทัดเดียว: [ป้ายประเภท] รหัส ชื่อ   [เลขที่ใบส่งของ] [..]  (ป้ายเลขที่ขึ้นบรรทัดใหม่เมื่อไม่พอ)
            private int renderItem(Graphics g, DoMissGroup grp, int x, int y, int w, bool draw)
            {
                int cx = x + 10, top = y + 5;

                Size bs = measure(g, grp.Kind, fBadge);
                Rectangle badge = new Rectangle(cx, top, bs.Width + 16, bs.Height + 4);
                Size cs = measure(g, grp.Code, fCode);
                Size ns = measure(g, grp.Name, fName);
                int lineH = Math.Max(badge.Height, Math.Max(cs.Height, ns.Height));

                if (draw)
                {
                    using (GraphicsPath p = rounded(badge, badge.Height / 2))
                    using (SolidBrush b = new SolidBrush(Accent))
                        g.FillPath(b, p);
                    TextRenderer.DrawText(g, grp.Kind, fBadge, badge, Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                    int codeX = badge.Right + 8;
                    TextRenderer.DrawText(g, grp.Code, fCode, new Point(codeX, top + (lineH - cs.Height) / 2), Code, Tf);
                    TextRenderer.DrawText(g, grp.Name, fName, new Point(codeX + cs.Width + 8, top + (lineH - ns.Height) / 2), Code, Tf);
                }

                // ป้ายเลขที่ใบส่งของเริ่มต่อท้ายชื่อ ถ้าไม่พอขึ้นบรรทัดใหม่ใต้ป้ายประเภท
                int nameRight = badge.Right + 8 + cs.Width + 8 + ns.Width;
                int chipX = nameRight + 16, chipH = 0;
                int chipY = top;
                foreach (string doc in grp.Docs)
                {
                    Size ds = measure(g, doc, fChip);
                    Rectangle chip = new Rectangle(chipX, chipY, ds.Width + 12, ds.Height + 6);
                    if (chipY == top)
                        chip.Y = top + (lineH - chip.Height) / 2; // จัดกึ่งกลางแนวตั้งกับบรรทัดแรก
                    if (chip.Right > x + w - 10)
                    {
                        chipX = cx;
                        chipY = (chipY == top ? top + lineH : chipY + chip.Height) + 4;
                        chip.Location = new Point(chipX, chipY);
                    }
                    if (draw)
                    {
                        using (GraphicsPath p = rounded(chip, 3))
                        using (Pen pen = new Pen(Border))
                        {
                            g.FillPath(Brushes.White, p);
                            g.DrawPath(pen, p);
                        }
                        TextRenderer.DrawText(g, doc, fChip, chip, Text,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                    }
                    chipX = chip.Right + 5;
                    chipH = chip.Height;
                }
                int bottom = chipY == top ? top + lineH : chipY + chipH;
                return (bottom + 5) - y;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    fTitle.Dispose(); fSub.Dispose(); fBadge.Dispose(); fCode.Dispose();
                    fName.Dispose(); fChip.Dispose(); fHint.Dispose(); fIcon.Dispose();
                }
                base.Dispose(disposing);
            }
        }

        // แถบแจ้งใบส่งของที่ k2m ไม่ระบุเหมือง สไตล์เดียวกับ DoWarnPanel แต่โทนเหลือง (ตรงกับสีแถวในตาราง)
        //  (!)  มี 2 ใบส่งของที่ k2m ไม่ระบุเหมือง   แถวสีเหลืองในตาราง · เลือกได้ แต่ตรวจสอบให้แน่ใจว่าเป็นของเหมืองนี้
        //       [DO2611-000478] [DO2611-000487]
        private class MineUnknownBanner : Panel
        {
            private static readonly Color Back = Color.FromArgb(255, 251, 224);
            private static readonly Color Border = Color.FromArgb(235, 214, 120);
            private static readonly Color Accent = Color.FromArgb(222, 178, 20);
            private static readonly Color Text = Color.FromArgb(102, 80, 0);
            private static readonly Color Muted = Color.FromArgb(138, 115, 40);

            public List<string> Docs = new List<string>();

            private readonly Font fTitle, fSub, fChip, fIcon;
            private const TextFormatFlags Tf = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;
            private const TextFormatFlags Center = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;

            public const int PreferredHeight = 62;

            public MineUnknownBanner()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
                string ui = DoWarnPanel.fontExists("Leelawadee UI") ? "Leelawadee UI" : "Tahoma";
                string mono = DoWarnPanel.fontExists("Consolas") ? "Consolas" : "Courier New";
                fTitle = new Font(ui, 11F, FontStyle.Bold);
                fSub = new Font(ui, 9F);
                fChip = new Font(mono, 8.25F);
                fIcon = new Font(ui, 10F, FontStyle.Bold);
            }

            private static Size measure(Graphics g, string s, Font f)
            {
                return TextRenderer.MeasureText(g, s, f, new Size(int.MaxValue, int.MaxValue), Tf);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int w = ClientSize.Width, h = ClientSize.Height;

                using (GraphicsPath p = DoWarnPanel.rounded(new Rectangle(0, 0, w - 1, h - 1), 6))
                using (SolidBrush b = new SolidBrush(Back))
                using (Pen pen = new Pen(Border))
                {
                    g.FillPath(b, p);
                    g.DrawPath(pen, p);
                }
                using (GraphicsPath p = DoWarnPanel.rounded(new Rectangle(0, 0, 5, h - 1), 2))
                using (SolidBrush b = new SolidBrush(Accent))
                    g.FillPath(b, p);

                int x = 18, y = 8, tx = 50;

                Rectangle icon = new Rectangle(x, y, 22, 22);
                using (SolidBrush b = new SolidBrush(Accent))
                    g.FillEllipse(b, icon);
                TextRenderer.DrawText(g, "!", fIcon, icon, Color.White, Center);

                string title = "มี " + Docs.Count + " ใบส่งของที่ k2m ไม่ระบุเหมือง";
                Size ts = measure(g, title, fTitle);
                TextRenderer.DrawText(g, title, fTitle, new Point(tx, y + (22 - ts.Height) / 2), Text, Tf);
                string sub = "แถวสีเหลืองในตาราง  ·  เลือกได้ แต่ตรวจสอบให้แน่ใจว่าเป็นของเหมืองนี้";
                Size ss = measure(g, sub, fSub);
                int subX = tx + ts.Width + 12;
                TextRenderer.DrawText(g, sub, fSub,
                    new Rectangle(subX, y + (22 - ss.Height) / 2 + 1, Math.Max(0, w - 14 - subX), ss.Height), Muted, Tf | TextFormatFlags.EndEllipsis);
                y += 28;

                // ป้ายเลขที่ใบส่งของ บรรทัดเดียว ถ้าไม่พอแสดง "+N"
                int cx = tx;
                for (int i = 0; i < Docs.Count; i++)
                {
                    Size ds = measure(g, Docs[i], fChip);
                    Rectangle chip = new Rectangle(cx, y, ds.Width + 12, ds.Height + 6);
                    string more = "+" + (Docs.Count - i);
                    int moreW = measure(g, more, fChip).Width + 12;
                    bool last = i == Docs.Count - 1;
                    if (chip.Right + (last ? 0 : moreW + 5) > w - 14)
                    {
                        Rectangle mr = new Rectangle(cx, y, moreW, chip.Height);
                        TextRenderer.DrawText(g, more, fChip, mr, Muted, Center);
                        break;
                    }
                    using (GraphicsPath p = DoWarnPanel.rounded(chip, 3))
                    using (Pen pen = new Pen(Border))
                    {
                        g.FillPath(Brushes.White, p);
                        g.DrawPath(pen, p);
                    }
                    TextRenderer.DrawText(g, Docs[i], fChip, chip, Text, Center);
                    cx = chip.Right + 5;
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    fTitle.Dispose(); fSub.Dispose(); fChip.Dispose(); fIcon.Dispose();
                }
                base.Dispose(disposing);
            }
        }

        // แถบแจ้งว่ากำลังใช้ใบส่งของในเครื่อง (ต่อ API ใบส่งของไม่ได้) สไตล์เดียวกับ DoWarnPanel แต่โทนแดง
        //  (!)  ใช้ข้อมูลใบส่งของในเครื่อง  [ออฟไลน์]
        //       ข้อมูลอาจไม่เป็นปัจจุบัน · ใบส่งของใหม่ที่ยังไม่เคยดึงลงเครื่องจะไม่แสดง
        //       [สาเหตุ] ข้อความที่อ่านง่าย   รายละเอียดทางเทคนิค (สีจาง)        [ยอดเที่ยว / ยอดหิน ยังไม่อัปเดต]
        private class OfflineBanner : Panel
        {
            private static readonly Color Back = Color.FromArgb(253, 240, 238);
            private static readonly Color Border = Color.FromArgb(241, 196, 190);
            private static readonly Color Accent = Color.FromArgb(214, 69, 65);
            private static readonly Color Text = Color.FromArgb(122, 31, 27);
            private static readonly Color Muted = Color.FromArgb(150, 98, 94);
            private static readonly Color Faint = Color.FromArgb(176, 140, 137);
            private static readonly Color AmberBack = Color.FromArgb(255, 243, 214);
            private static readonly Color AmberBorder = Color.FromArgb(240, 210, 140);
            private static readonly Color AmberText = Color.FromArgb(122, 86, 10);

            public string Reason = "";
            public bool SummaryUpdated = true;

            private readonly Font fTitle, fSub, fPill, fReason, fTech, fIcon;
            private const TextFormatFlags Tf = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;
            private const TextFormatFlags Center = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;

            public const int PreferredHeight = 80;

            public OfflineBanner()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
                string ui = DoWarnPanel.fontExists("Leelawadee UI") ? "Leelawadee UI" : "Tahoma";
                string mono = DoWarnPanel.fontExists("Consolas") ? "Consolas" : "Courier New";
                fTitle = new Font(ui, 11F, FontStyle.Bold);
                fSub = new Font(ui, 9F);
                fPill = new Font(ui, 8.25F, FontStyle.Bold);
                fReason = new Font(ui, 9.75F, FontStyle.Bold);
                fTech = new Font(mono, 8.25F);
                fIcon = new Font(ui, 10F, FontStyle.Bold);
            }

            // แปลง error จากระบบเป็นข้อความที่ user เข้าใจ ส่วนข้อความเดิมแสดงเป็นรายละเอียดสีจาง
            private static string friendly(string reason, out string tech)
            {
                tech = "";
                string r = reason ?? "";
                int colon = r.IndexOf(" : ", StringComparison.Ordinal);
                string detail = colon >= 0 ? r.Substring(colon + 3) : r;
                string lower = r.ToLowerInvariant();

                if (lower.Contains("could not be resolved") || lower.Contains("no such host"))
                {
                    int q1 = detail.IndexOf('\''), q2 = q1 >= 0 ? detail.IndexOf('\'', q1 + 1) : -1;
                    tech = q2 > q1 ? detail.Substring(q1 + 1, q2 - q1 - 1) : detail;
                    return "ไม่มี internet หรือหาชื่อ server ไม่เจอ";
                }
                if (lower.Contains("unable to connect") || lower.Contains("actively refused") || lower.Contains("connection"))
                {
                    tech = detail;
                    return "เชื่อมต่อ server ไม่ได้";
                }
                if (r.StartsWith("หมดเวลา"))
                    return "server ตอบช้าเกินไป (เกิน 30 วินาที)";
                if (r.StartsWith("API ตอบกลับ HTTP"))
                {
                    tech = r.Substring("API ตอบกลับ ".Length);
                    if (r.Contains("HTTP 401") || r.Contains("HTTP 403"))
                        return "API ไม่อนุญาต (token ใน base_api id = 2 ไม่ถูกต้อง)";
                    return "API ใบส่งของตอบกลับผิดพลาด";
                }
                tech = detail;
                return colon >= 0 ? r.Substring(0, colon) : "ดึงใบส่งของไม่สำเร็จ";
            }

            private static void pill(Graphics g, Rectangle r, string text, Font f, Color back, Color border, Color fore)
            {
                using (GraphicsPath p = DoWarnPanel.rounded(r, r.Height / 2))
                using (SolidBrush b = new SolidBrush(back))
                {
                    g.FillPath(b, p);
                    if (border != Color.Empty)
                        using (Pen pen = new Pen(border))
                            g.DrawPath(pen, p);
                }
                TextRenderer.DrawText(g, text, f, r, fore, Center);
            }

            private static Size measure(Graphics g, string s, Font f)
            {
                return TextRenderer.MeasureText(g, s, f, new Size(int.MaxValue, int.MaxValue), Tf);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int w = ClientSize.Width, h = ClientSize.Height;

                // การ์ด + แถบแดงซ้าย
                using (GraphicsPath p = DoWarnPanel.rounded(new Rectangle(0, 0, w - 1, h - 1), 6))
                using (SolidBrush b = new SolidBrush(Back))
                using (Pen pen = new Pen(Border))
                {
                    g.FillPath(b, p);
                    g.DrawPath(pen, p);
                }
                using (GraphicsPath p = DoWarnPanel.rounded(new Rectangle(0, 0, 5, h - 1), 2))
                using (SolidBrush b = new SolidBrush(Accent))
                    g.FillPath(b, p);

                int x = 18, y = 9, tx = 50;

                // แถว 1: ไอคอน + หัวข้อ + ป้าย "ออฟไลน์"
                Rectangle icon = new Rectangle(x, y, 22, 22);
                using (SolidBrush b = new SolidBrush(Accent))
                    g.FillEllipse(b, icon);
                TextRenderer.DrawText(g, "!", fIcon, icon, Color.White, Center);

                string title = "ใช้ข้อมูลใบส่งของในเครื่อง";
                Size ts = measure(g, title, fTitle);
                TextRenderer.DrawText(g, title, fTitle, new Point(tx, y + (22 - ts.Height) / 2), Text, Tf);
                Size ps = measure(g, "ออฟไลน์", fPill);
                pill(g, new Rectangle(tx + ts.Width + 10, y + 2, ps.Width + 16, 18), "ออฟไลน์", fPill, Accent, Color.Empty, Color.White);
                y += 26;

                // แถว 2: คำอธิบาย
                string sub = "เชื่อมต่อ API ใบส่งของไม่ได้   ·   ข้อมูลอาจไม่เป็นปัจจุบัน   ·   ใบส่งของใหม่ที่ยังไม่เคยดึงลงเครื่องจะไม่แสดง";
                Size ss = measure(g, sub, fSub);
                TextRenderer.DrawText(g, sub, fSub, new Point(tx, y), Muted, Tf);
                y += ss.Height + 7;

                // แถว 3: [สาเหตุ] ข้อความอ่านง่าย  รายละเอียด          [ยอดยังไม่อัปเดต] (ชิดขวา)
                string tech;
                string reason = friendly(Reason, out tech);
                Size ls = measure(g, "สาเหตุ", fPill);
                Rectangle lbl = new Rectangle(tx, y, ls.Width + 16, 20);
                pill(g, lbl, "สาเหตุ", fPill, Color.White, Border, Accent);

                int rightLimit = w - 14;
                if (!SummaryUpdated)
                {
                    string warn = "ยอดเที่ยว / ยอดหิน ยังไม่อัปเดตจาก web app";
                    Size wsz = measure(g, warn, fPill);
                    Rectangle wr = new Rectangle(w - 14 - (wsz.Width + 20), y, wsz.Width + 20, 20);
                    pill(g, wr, warn, fPill, AmberBack, AmberBorder, AmberText);
                    rightLimit = wr.Left - 12;
                }

                Size rs = measure(g, reason, fReason);
                int rx = lbl.Right + 8;
                TextRenderer.DrawText(g, reason, fReason,
                    new Rectangle(rx, y + (20 - rs.Height) / 2, Math.Max(0, rightLimit - rx), rs.Height), Text, Tf | TextFormatFlags.EndEllipsis);
                int techX = rx + rs.Width + 12;
                if (tech != "" && techX < rightLimit)
                {
                    Size tsz = measure(g, tech, fTech);
                    TextRenderer.DrawText(g, tech, fTech,
                        new Rectangle(techX, y + (20 - tsz.Height) / 2, rightLimit - techX, tsz.Height), Faint, Tf | TextFormatFlags.EndEllipsis);
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    fTitle.Dispose(); fSub.Dispose(); fPill.Dispose();
                    fReason.Dispose(); fTech.Dispose(); fIcon.Dispose();
                }
                base.Dispose(disposing);
            }
        }

        private void TableDeliveryOrder_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(OfflineReason))
                Text += "  (ข้อมูลในเครื่อง)";


            //dgvDO.Columns[3].DefaultCellStyle.BackColor = Color.DarkSeaGreen;
            //dgvDO.Columns[5].DefaultCellStyle.BackColor = Color.DarkSeaGreen;
            //dgvDO.Columns[10].DefaultCellStyle.BackColor = Color.DarkSeaGreen;

            // ถ้าค่า = 0 ให้ตัวอักษรเป็นสีแดง
            dgvDO.CellFormatting += dgvDO_CellFormatting;

            try
            {
                // เลือก "ยังไม่สำเร็จ"
                cbbSearchDO.SelectedItem = "ยังไม่สำเร็จ";

                // โหลดข้อมูล
                setSearchDataDO();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            // แสดงแถบ "ใช้ข้อมูลในเครื่อง" แม้ query ใบส่งของจะไม่สำเร็จ
            layoutDoPanels();
        }

        private void dgvDO_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // ใบส่งของที่เลือกไม่ได้: พื้นแถวแดงอ่อน + ช่องรหัส/ชื่อที่ไม่พบเป็นตัวแดง + tooltip บอกเหตุผล
            if (e.RowIndex >= 0)
            {
                DataGridViewRow gridRow = dgvDO.Rows[e.RowIndex];
                DataRowView row = gridRow.DataBoundItem as DataRowView;
                if (row != null)
                {
                    string missing = doMissingText(row);
                    bool mineUnknown = isMineUnknown(row);
                    // ไม่ระบุเหมือง: พื้นเหลือง (ถ้าเลือกไม่ได้ด้วย สีแดงด้านล่างทับ)
                    if (mineUnknown)
                    {
                        e.CellStyle.BackColor = MineRowBack;
                        gridRow.Cells[e.ColumnIndex].ToolTipText = MineUnknownTip;
                        if (dgvDO.Columns[e.ColumnIndex].Name == "mine_code")
                        {
                            e.Value = "ไม่ระบุ";
                            e.FormattingApplied = true;
                            e.CellStyle.ForeColor = Color.FromArgb(150, 110, 0);
                            if (mineBoldFont == null)
                                mineBoldFont = new Font(dgvDO.Font, FontStyle.Bold);
                            e.CellStyle.Font = mineBoldFont;
                        }
                    }
                    if (missing != "")
                    {
                        e.CellStyle.BackColor = MissRowBack;
                        string col = dgvDO.Columns[e.ColumnIndex].Name;
                        bool redCustomer = !rowFlag(row, "has_customer") && (col == "customer_code" || col == "customer_name");
                        bool redStone = !rowFlag(row, "has_stone") && (col == "product_code" || col == "product_name");
                        if (redCustomer || redStone)
                        {
                            e.CellStyle.ForeColor = MissText;
                            e.CellStyle.SelectionForeColor = Color.FromArgb(255, 205, 200);
                        }
                        gridRow.Cells[e.ColumnIndex].ToolTipText = missing + " ยังเลือกใบส่งของนี้ไม่ได้"
                            + (mineUnknown ? "\n" + MineUnknownTip : "");
                    }
                }
            }

            // ตรวจเฉพาะคอลัมน์เที่ยวคงเหลือ (อ้างด้วยชื่อ ไม่ใช้ index เพราะมีคอลัมน์ที่เพิ่มใน code)
            string colName = e.ColumnIndex >= 0 ? dgvDO.Columns[e.ColumnIndex].Name : "";
            if (colName == "car_customer_rem" || colName == "car_company_rem")
            {
                if (e.Value != null)
                {
                    decimal value;

                    if (decimal.TryParse(e.Value.ToString(), out value))
                    {
                        if (value == 0)
                        {
                            e.CellStyle.ForeColor = Color.Red;
                            e.CellStyle.SelectionForeColor = Color.Red;
                        }
                    }
                }
            }
        }

        private void prepareDataDOToMainForm()
        {
            // บล็อกใบส่งของที่ไม่พบรหัสลูกค้า / รหัสหินในเครื่อง (เหมือน selectDeliveryOrder ใน web app)
            DataRowView selected = dgvDO.CurrentRow != null ? dgvDO.CurrentRow.DataBoundItem as DataRowView : null;
            if (selected != null)
            {
                string missing = doMissingText(selected);
                if (missing != "")
                {
                    MessageBox.Show(missing + "\nไม่สามารถอ้างอิงใบส่งของ " + selected["doc_no"] + " ได้\n\nกรุณาเพิ่มข้อมูลลูกค้า / ชนิดหิน ให้ตรงกับรหัสก่อน แล้วกด \"ค้นหา\" ใหม่",
                        "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // k2m ไม่ระบุเหมือง: ให้เลือกได้ แต่ต้องยืนยันว่าเป็นของเหมืองนี้
                if (isMineUnknown(selected))
                {
                    string own = DeliveryOrderMine.GetOwnMineCode(dl);
                    DialogResult ok = MessageBox.Show(
                        "ใบส่งของ " + selected["doc_no"] + " k2m ไม่ได้ระบุเหมือง\n\nยืนยันว่าเป็นของเหมือง " + own + " ใช่หรือไม่ ?",
                        "ยืนยันเหมือง", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                    if (ok != DialogResult.Yes)
                        return;
                }
            }

            if (mainForm.isHaveDataOld()){
                DialogResult result = MessageBox.Show(
                    "มีข้อมูลค้างที่รายการชั่ง ต้องการแทนที่ข้อมูลเดิมหรือไม่ ?",
                    "ยืนยันการทำรายการ",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.No) {
                    return; // กด No ออกจาก method เลย
                } else if (result == DialogResult.Yes) {
                    setDatoDo();
                }
            }
            else {

                setDatoDo();
            }

        }

        private void setDatoDo() {
            //set old do id
            mainForm.setOldDOId();

            //reset old do id
            mainForm.resetFromDO();

            DataDO data_do = new DataDO();
            if (dgvDO.Rows.Count > 1)
            {
                data_do.do_id = dgvDO.CurrentRow.Cells["do_id2"].Value.ToString();
                data_do.docNo = dgvDO.CurrentRow.Cells["doc_no"].Value.ToString();

                data_do.deliveryDate = dgvDO.CurrentRow.Cells["delivery_date"].Value.ToString();
                data_do.carCompany = dgvDO.CurrentRow.Cells["car_company"].Value.ToString();

                data_do.carCustomer = dgvDO.CurrentRow.Cells["car_customer"].Value.ToString();
                // ส่งเฉพาะรหัสลูกค้า / รหัสหิน — ชื่อจะ get จาก database ในเครื่องที่ MainForm.setDataFromTableDo
                data_do.customerId = (dgvDO.CurrentRow.Cells["customer_code"].Value ?? "").ToString().Trim();
                data_do.siteId = dgvDO.CurrentRow.Cells["site_id"].Value.ToString();
                data_do.siteName = dgvDO.CurrentRow.Cells["site_name"].Value.ToString();
                data_do.stoneTypeId = (dgvDO.CurrentRow.Cells["product_code"].Value ?? "").ToString().Trim();

                data_do.qty = dgvDO.CurrentRow.Cells["qty2"].Value.ToString();

                data_do.unitName = dgvDO.CurrentRow.Cells["unit_name"].Value.ToString();

                // credit_type ไม่ได้แสดงใน grid: อ่านจากแถวข้อมูลที่ bind อยู่
                DataRowView rowView = dgvDO.CurrentRow.DataBoundItem as DataRowView;
                if (rowView != null && rowView.Row.Table.Columns.Contains("credit_type"))
                    data_do.creditType = (rowView["credit_type"] ?? "").ToString().Trim();

                mainForm.setDataFromTableDo(data_do);
                this.Hide();
            }
            else
            {
                MessageBox.Show("ไม่พบข้อมูล ใบส่งของที่ต้องการ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        private void setSearchDataDO()
        {
            DateTime dateMainForm = DateTime.Parse(mainForm.getdtDate());

            try
            {
                // แสดงเฉพาะ DO ของเหมืองตัวเอง: delivery_order.comp_code (= k2m mine_code = Django DeliveryOrder.comp_code)
                // เทียบกับ base_api id = 2 comp_code (ตารางเต็มดู DeliveryOrderMine.cs)
                DeliveryOrderMine.EnsureCompCodeColumn(dl);
                string ownMine = DeliveryOrderMine.GetOwnMineCode(dl);

                dl.connect();

                StringBuilder sql = new StringBuilder();
                // qty_rem = plan จำนวนหิน - จำนวนหินชั่งแล้ว
                // has_customer / has_stone = รหัสในใบส่งของมีในฐานข้อมูลเครื่องนี้ไหม (1/0) เงื่อนไขเดียวกับ MainForm.getCustomerNameFromDbById / getStoneTypeNameFromDbById
                sql.Append("SELECT d.*, COALESCE(d.qty, 0) - COALESCE(d.qty_tot, 0) AS qty_rem, ");
                sql.Append("CASE WHEN EXISTS (SELECT 1 FROM public.base_customer c WHERE c.รหัสลูกค้า = TRIM(d.customer_code)) THEN 1 ELSE 0 END AS has_customer, ");
                sql.Append("CASE WHEN EXISTS (SELECT 1 FROM public.base_stone_type s WHERE s.รหัสหิน = TRIM(d.product_code)) THEN 1 ELSE 0 END AS has_stone ");
                sql.Append("FROM public.delivery_order d WHERE ");

                if (cbbSearchDO.SelectedIndex == 1)
                {
                    // ยังไม่สำเร็จ
                    sql.Append("(car_company_rem > 0 OR car_customer_rem > 0) ");
                    sql.Append("AND status = 'open' AND ");
                }
                else if (cbbSearchDO.SelectedIndex == 2)
                {
                    // สำเร็จแล้ว
                    sql.Append("(car_company_rem <= 0 AND car_customer_rem <= 0) ");
                    sql.Append("AND status = 'open' AND ");
                }
                else if (cbbSearchDO.SelectedIndex == 3)
                {
                    // ยกเลิก
                    sql.Append("status = 'cancel' AND ");
                }

                // กรณี index = 0 (ทั้งหมด) จะไม่เพิ่มเงื่อนไข status

                sql.Append("delivery_date = ? ");
                sql.Append("AND comp_code = ? ");
                sql.Append("ORDER BY doc_no");

                using (OdbcCommand cmd = new OdbcCommand(sql.ToString(), dl.sqlConn()))
                {
                    cmd.Parameters.Add("delivery_date", OdbcType.Date).Value = dateMainForm;
                    cmd.Parameters.AddWithValue("comp_code", ownMine);

                    using (OdbcDataAdapter adt = new OdbcDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adt.Fill(dt);
                        dgvDO.DataSource = dt;
                        showDoMissingWarn(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                dl.close();
            }
        }

        private void dgvDO_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            prepareDataDOToMainForm();
        }

        private void btSelect_Click(object sender, EventArgs e)
        {
            prepareDataDOToMainForm();
        }

        private void btSearchDO_Click(object sender, EventArgs e)
        {
            setSearchDataDO();
        }
    }


}
