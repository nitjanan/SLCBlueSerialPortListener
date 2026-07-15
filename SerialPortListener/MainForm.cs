using System;
using Microsoft.Reporting.WinForms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SerialPortListener.Serial;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Remoting.Messaging;
using Devart.Data.PostgreSql;
using static SerialPortListener.TableFromDB;
using System.Data.Odbc;
using Microsoft.VisualBasic;
using static SerialPortListener.TableDeliveryOrder;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SerialPortListener
{

    public partial class MainForm : Form
    {
        SerialPortManager _spManager;
        Datalayer dl;
        DatalayerNew dln;
        String strCalQ = "1.00";
        AutoCompleteStringCollection collCarTeam = new AutoCompleteStringCollection();
        bool isCheckedCash = false;
        bool isCheckedTrans = false;
        bool isCheckedCredit = false;
        bool isCheckedMill1 = false;
        bool isCheckedMill2 = false;
        bool isCheckedMill3 = false;
        bool isCheckedMillNo = false;
        bool isCheckedCleanStone = false;
        bool isCheckedCleanWater = false;
        bool isCheckedCleanNo = false;
        bool isCheckedSelfPick = false;
        bool isCheckedSendTo = false;
        private string lastLimitExceededError = null;

        /*1 search anywhere customer */
        // Bind default keywords
        List<string> listOriginalCustomerName = new List<string>();
        // save new keywords
        List<string> listNewCustomerName = new List<string>();

        List<string> listCusDO = new List<string>();

        class ComboboxValue
        {
            public string Id { get; private set; }
            public string Name { get; private set; }

            public ComboboxValue(string id, string name)
            {
                Id = id;
                Name = name;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public class DeliveryOrder
        {
            // --- Fields from BASE_URL (Phase 1 download) ---
            public string doc_no { get; set; }
            public string delivery_date { get; set; }
            public string delivery_type { get; set; }
            public string car_company { get; set; }
            public string car_customer { get; set; }
            public string car_company_rem { get; set; }
            public string car_customer_rem { get; set; }
            public string customer_code { get; set; }
            public string customer_name { get; set; }
            public string customer_address { get; set; }
            public string site_id { get; set; }
            public string site_name { get; set; }
            public string product_code { get; set; }
            public string product_name { get; set; }
            public object qty { get; set; }
            public string unit_name { get; set; }
            public string sale_name { get; set; }
            public string note { get; set; }
            public string status { get; set; }

            // --- Fields from summary API (Phase 2 update) ---
            public object car_company_tot { get; set; }
            public object car_customer_tot { get; set; }
            public object qty_tot { get; set; }
        }

        public class CancelDeliveryOrder
        {
            // --- Fields from BASE_URL (Phase 1 download) ---
            public string doc_no { get; set; }
            public string delivery_date { get; set; }
            public string status { get; set; }
            public string comp_code { get; set; }
        }


        // ============================================================
        // API response wrapper  { "data": [...], ... }
        // ============================================================
        public class DeliveryOrderPageResponse
        {
            public List<DeliveryOrderApiItem> data { get; set; }
        }

        public class DRFPaginationResponse<T>
        {
            public int count { get; set; }
            public string next { get; set; }
            public string previous { get; set; }
            public List<T> results { get; set; }
            public List<T> data { get; set; }
        }

        // ============================================================
        // camelCase fields from BASE_URL
        // ============================================================
        public class DeliveryOrderApiItem
        {
            public string docNo { get; set; }
            public string deliveryDate { get; set; }
            public string deliveryType { get; set; }
            public string carCompany { get; set; }
            public string carCustomer { get; set; }
            public string customerCode { get; set; }
            public string customerName { get; set; }
            public string customerAddress { get; set; }
            private string _siteId;

            [JsonProperty("deliveryCode")]
            public string siteId
            {
                get
                {
                    if (string.IsNullOrEmpty(_siteId)) return "";
                    int idx = _siteId.LastIndexOf("___");
                    if (idx >= 0)
                    {
                        return _siteId.Substring(idx + 3);
                    }
                    return _siteId;
                }
                set
                {
                    _siteId = value;
                }
            }
            [JsonProperty("deliveryLocation")]
            public string siteName { get; set; }
            public string productCode { get; set; }
            public string productName { get; set; }
            public object qty { get; set; }
            public string unitName { get; set; }
            public string saleName { get; set; }
            public string note { get; set; }
            public string status { get; set; }
        }


        public class WeightDelivery
        {
            public int weight_id { get; set; }
            public string delivery_date { get; set; }
            public string bws { get; set; }
            public string comp_code { get; set; }
            public string do_doc_no { get; set; }
            public string carry_type_name { get; set; }
            public Boolean is_cancel { get; set; }
        }

        public class UpdateDeliveryOrderResult
        {
            public bool IsSuccess { get; set; }
            public bool IsValidationError { get; set; }
            public string ErrorMessage { get; set; }
        }

        public MainForm(string username, String firstname)
        {
            dl = new Datalayer();
            InitializeComponent();

            UserInitialization();

            setDefaultFromDB(username, firstname);

            getSettingDefault();

            // _spManager.StartListening();
        }

        public void getSettingDefault()
        {
            lbCompanyCode.Text = Company.Code;
            /* autoComplete ผู้ตัก */
            autoCompleteSettingCompany(tbScoopId, "รหัสผู้ตัก", "base_scoop");
            autoCompleteSettingCompany(tbScoopName, "ชื่อผู้ตัก", "base_scoop");

            /* autoComplete ผู้ชั่ง */
            autoCompleteSetting(tbScaleId, "username", "users");
            autoCompleteSetting(tbScaleName, "firstname", "users");

            /* autoComplete ผู้อนุมัติ */
            autoCompleteSetting(tbApproveId, "รหัสผู้อนุมัติจ่าย", "base_approve");
            autoCompleteSetting(tbApproveName, "ชื่อผู้อนุมัติจ่าย", "base_approve");

            /* autoComplete จังหวัด */
            autoCompleteSetting(tbCarCity, "ชื่อจังหวัด", "base_car_city");

            /* autoComplete ลูกค้า */
            //autoCompleteSetting(tbCustomerId, "รหัสลูกค้า", "base_customer");
            //autoCompleteSetting(tbCustomerName, "ชื่อลูกค้า", "base_customer");

            /* autoComplete โรงโม่ */
            //autoCompleteSettingWeightType(tbMillId, "รหัสโรงโม่", "base_mill");
            //autoCompleteSettingWeightType(tbMillName, "ชื่อโรงโม่", "base_mill");

            setautoCompleteCustomer("รหัสลูกค้า", "ชื่อลูกค้า", "base_customer");

            Weight.CustomerAddress = getPrintFromDB("base_customer", "ที่อยู่", "รหัสลูกค้า", tbCustomerId.Text);

            tbWeigtData.Enter += (s, e) => { tbWeigtData.Parent.Focus(); };

            tbScoopId.KeyDown += tbScoopId_KeyDown;
            tbScoopName.KeyDown += tbScoopName_KeyDown;

            // Load DirectPrint setting
            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\SerialPortListener"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("DirectPrint");
                        if (val != null)
                        {
                            chkDirectPrint.Checked = Convert.ToBoolean(val);
                        }
                    }
                }
            }
            catch {}
        }
        public void EnableWeightInAndOut()
        {
            btReadIn.Enabled = true;
            btReadOut.Enabled = true;
        }

        public void disableReadWeightIn()
        {
            btReadIn.Enabled = false;
        }

        public void disableReadWeightOut()
        {
            btReadOut.Enabled = false;
        }

        public void resetMainForm()
        {
            tbId.Text = "";
            tbDocNum.Text = "";
            rbMill1.Checked = false;
            rbMill2.Checked = false;
            rbMill3.Checked = false;
            rbMillNo.Checked = false;
            rbCash.Checked = false;
            rbCredit.Checked = false;
            rbTrans.Checked = false;
            rbVat.Checked = false;
            cbbStoneType.Text = "";
            cbbStoneColor.Text = "";
            cbbTransport.Text = "";
            tbRefNum.Text = "";
            tbCustomerId.Text = "";
            tbCustomerName.Text = "";
            cbbCustomerName.Text = "";
            tbCarLicense.Text = "";
            tbCarCity.Text = "";
            tbDriverName.Text = "";
            cbbMill.Text = "";
            //tbMillId.Text = "";
            //tbMillName.Text = "";

            // login admin ให้เปลี่ยน
            if (Globals.isPermissionEditWeight())
            {
                tbScaleId.Text = "003";
                tbScaleName.Text = "รุ่งฤดี";
            }
            else
            {
                tbScaleId.Text = Globals.Username;
                tbScaleName.Text = Globals.Firstname;
            }

            tbScoopId.Text = "";
            tbScoopName.Text = "";
            tbWeightIn.Text = "0.00";
            tbWeightOut.Text = "0.00";
            tbWeightTotal.Text = "0.00";
            tbPricePerTon.Text = "0.00";
            tbAmountVat.Text = "0.00";
            tbAmount.Text = "0.00";
            tbShipCost.Text = "0.00";
            tbAmount.Text = "0.00";
            tbVat.Text = "0.00";
            tbApproveId.Text = "";
            tbApproveName.Text = "";
            dtDate.Text = DateTime.Now.ToShortDateString();
            dtWeightInDate.Text = DateTime.Now.ToShortDateString();
            dtWeightOutDate.Text = DateTime.Now.ToShortDateString();
            dtWeightInTime.Text = DateTime.Now.ToShortTimeString();
            dtWeightOutTime.Text = DateTime.Now.ToShortTimeString();
            tbQ.Text = "0.00";
            rbbNonVat.Checked = false;
            rbbVat.Checked = true;
            rbCleanStone.Checked = false;
            rbCleanWater.Checked = false;
            rbCleanNo.Checked = false;
            cbbSite.Text = "";
            cbbCarTeam.Text = "";
            tbNote.Text = "";
            tbStoneDesc.Text = "";
            tbOilContent.Text = "0.00";

            //ใบส่งของ
            tbOldDoId.Text = "";

            tbDoId.Text = "";
            tbDoDocNo.Text = "";
            cbbStoneType.Enabled = true;
            cbS.Checked = false;
            cbbSite.Enabled = true;

            fillStoneCombo();
            fillTransportCombo();
            fillMillCombo();
            calculatenumQ();

            disableBtAfterRead(0);
            // version vat if (Globals.isPermissionTop())
            disableBtAfterRead(3);

            if (Globals.isPermissionEditWeight())
                disableBtAfterRead(999);

        }

        public void setOldDOId()
        {
            tbOldDoId.Text = tbDoId.Text;
        }

        public string getdtDate()
        {
            return dtDate.Text;
        }

        public void resetFromDO()
        {
            tbDoId.Text = "";
            tbDoDocNo.Text = "";
            tbCustomerId.Text = "";
            tbCustomerName.Text = "";
            cbbStoneType.Text = "";
            tbStoneDesc.Text = "";
            cbbSite.Enabled = true;
        }

        public void runningDocNumber()
        {
            Boolean IsnewYear = false;
            string todayYear = DateTime.Now.ToString("yyyy");

            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public.seq_doc_num where run_year = '" + todayYear + "' ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                if (reader.Read())
                {
                    int rdNum = Convert.ToInt32(reader["run_number"].ToString());
                    rdNum++;
                    int lengthRdNum = reader["run_number"].ToString().Length;
                    string format = "D" + lengthRdNum.ToString();
                    tbDocNum.Text = rdNum.ToString(format);
                }
                else
                {
                    IsnewYear = true;
                }
            }
            catch (Exception)
            {
            }
            dl.close();

            if (IsnewYear)
                generateNewSeqNumber();

        }

        private void generateNewSeqNumber()
        {
            string todayYear = DateTime.Now.ToString("yyyy");
            string runningNumber = "000000";
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "INSERT INTO public.seq_doc_num (run_number, run_year) " +
                    "VALUES ('" + runningNumber + "', '" + todayYear + "') ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
            }
            catch (Exception)
            {
            }
            dl.close();

            //แก้ใน form
            int rdNumNew = Convert.ToInt32(runningNumber);
            rdNumNew++;
            int lengthRdNum = runningNumber.ToString().Length;
            string format = "D" + lengthRdNum.ToString();
            tbDocNum.Text = rdNumNew.ToString(format);
        }

        public void checkDocNumEmty()
        {
            if (tbDocNum.Text == "")
            {
                tbDocNum.Enabled = true;
            }
        }

        private void fillStoneCombo()
        {
            //ล้างก่อน
            cbbStoneType.Items.Clear();
            //เพิ่ม combobox
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public.base_stone_type where inactive = false ORDER BY รหัสหิน";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string id = reader["รหัสหิน"].ToString();
                    string des = reader["ชื่อหิน"].ToString();
                    cbbStoneType.Items.Add(new ComboboxValue(id, des));
                }
            }
            catch (Exception)
            {

            }
            dl.close();
        }

        private void fillMillCombo()
        {
            //ล้างก่อน
            cbbMill.Items.Clear();
            //เพิ่ม combobox
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public.base_mill where weight_type = 1 or weight_type = 3 ORDER BY รหัสโรงโม่";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string id = reader["รหัสโรงโม่"].ToString();
                    string des = reader["ชื่อโรงโม่"].ToString();
                    cbbMill.Items.Add(new ComboboxValue(id, des));
                }
            }
            catch (Exception)
            {

            }
            dl.close();
        }

        private void fillTransportCombo()
        {
            //ล้างก่อน
            cbbTransport.Items.Clear();
            //เพิ่ม combobox
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT base_transport_name FROM public.base_transport ORDER BY base_transport_id";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string des = reader["base_transport_name"].ToString();
                    cbbTransport.Items.Add(des);
                }
            }
            catch (Exception)
            {

            }
            dl.close();
        }

        //เรียกจาก TableFromDB
        public void AfterGetDataFromTable()
        {
            ucTruck.Hide();
            ucReport.Hide();
            ucHelp.Hide();
            ucSetting.Hide();
        }

        public void setDataFromClassTableFromDB(DataToUpdate data)
        {

            tbId.Text = data.id;
            dtDate.Text = data.date;
            tbDocNum.Text = data.docNum;
            tbCarLicense.Text = data.carLicense;
            tbCarCity.Text = data.carCity;
            tbDriverName.Text = data.driverName;
            tbCustomerId.Text = data.customerId;
            tbCustomerName.Text = data.customerName;

            if (data.customerId != "" && data.customerName != "" && data.doId != "")
            {
                cbbCustomerName.Items.Clear();
                listCusDO.Clear();

                listCusDO.Add(data.customerId + " : " + data.customerName);
                listCusDO.Add("09-V-001" + " : " + "ยกเลิก");
                cbbCustomerName.Text = data.customerId + " : " + data.customerName;
                cbbCustomerName.Items.AddRange(listCusDO.ToArray());
            }
            else if (data.customerId != "" && data.customerName != "")
                cbbCustomerName.Text = data.customerId + " : " + data.customerName;

            tbWeightIn.Text = tonTokg(data.weightIn);
            tbWeightOut.Text = tonTokg(data.weightOut);
            tbWeightTotal.Text = tonTokg(data.weightTotal);
            tbRefNum.Text = data.refNum;
            tbScaleId.Text = data.scaleId;
            tbScaleName.Text = data.scaleName;
            tbScoopId.Text = data.scoopId;
            tbScoopName.Text = data.scoopName;
            tbPricePerTon.Text = numberFormat(data.pricePerTon, 2);
            tbAmountVat.Text = numberFormat(data.amountVat, 2);
            tbAmount.Text = numberFormat(data.amount, 2);
            tbVat.Text = numberFormat(data.vat, 2);
            tbShipCost.Text = data.shipCost;
            dtWeightInDate.Text = data.weightInDate;
            dtWeightInTime.Text = data.weightInTime;
            if (tbWeightOut.Text == "0.00")
            {
                dtWeightOutDate.Text = DateTime.Now.ToShortDateString();
                dtWeightOutTime.Text = DateTime.Now.ToShortTimeString();
                btReadOut.Enabled = true;

                if (!checkEmptyTB(tbCarLicense))
                {
                    tbCarLicense.Enabled = false;
                }

                if (!checkEmptyTB(tbCarCity))
                {
                    tbCarCity.Enabled = false;
                }
            }
            else
            {
                dtWeightOutDate.Text = data.weightOutDate;
                dtWeightOutTime.Text = data.weightOutTime;
                //disable after read out
                disableBtAfterRead(2);

                if (!checkEmptyTB(tbCarLicense))
                {
                    tbCarLicense.Enabled = false;
                }

                if (!checkEmptyTB(tbCarCity))
                {
                    tbCarCity.Enabled = false;
                }
            }
            cbbStoneType.Text = data.stoneType;//111111111111
            tbQ.Text = numberFormat(data.q, 2);
            tbApproveId.Text = data.approveId;
            tbApproveName.Text = data.approveName;
            cbbStoneColor.Text = data.stoneColor;
            cbbTransport.Text = data.transport;
            cbbCarTeam.Text = data.team;//111111111111
            //tbMillName.Text = data.mill;//111111111111
            //tbMillId.Text = data.millId;//111111111111
            cbbMill.Text = data.mill;
            tbNote.Text = data.note;
            tbStoneDesc.Text = data.stone_desc ?? "";
            tbOilContent.Text = numberFormat(data.oilContent, 2);

            //ใบส่งของ
            tbDoId.Text = data.doId;
            tbDoDocNo.Text = data.doDocNo;
            if (tbDoId.Text != "")
            {
                cbbStoneType.Enabled = false;
                //cbbSite.Enabled = false;

            }

            //set is_s
            setDataSToisS(data.isS);

            //setDataMillToRB(data.mill);
            setDataPayToRB(data.payType);
            setDataVatToRB(data.vatType);
            setDataCleanToRB(data.clean);
            //ดึงหน้างาน
            fillSiteCombo();
            cbbSite.Text = data.site;
            for (int i = 0; i < cbbSite.Items.Count; i++)
            {
                var item = cbbSite.Items[i] as ComboboxValue;
                if (item != null && (item.Name == data.site || item.Id == data.siteId))
                {
                    cbbSite.SelectedIndex = i;
                    break;
                }
            }

            if (cbbSite.Text != "" && tbDoDocNo.Text != "")
            {
                cbbSite.Enabled = false;
            }

            AfterGetDataFromTable();

            //disable after read in
            disableBtAfterRead(1);

            //รหัสยกเลิกให้ปิดช่องให้หมด
            disableCancelId();

            //รหัสแก้ไขน้ำหนักได้
            if (Globals.isPermissionEditWeight())
                disableBtAfterRead(999);

            //if user admin enable all
            /* 111111111
            if (Globals.isPermissionTop())
            {
                disableBtAfterRead(999);
            }
            */
        }

        public bool isHaveDataOld()
        {
            bool isHave = false;
            if (tbCustomerId.Text != "" || cbbStoneType.Text != "")
                isHave = true;
            return isHave;
        }

        public void setDataFromTableDo(DataDO data_do)
        {
            tbDoId.Text = data_do.do_id;
            tbDoDocNo.Text = data_do.docNo;
            tbCustomerId.Text = data_do.customerId;
            tbCustomerName.Text = data_do.customerName;

            if (data_do.customerId != "" && data_do.customerName != "")
            {
                cbbCustomerName.Items.Clear();
                listCusDO.Clear();

                listCusDO.Add(data_do.customerId + " : " + data_do.customerName);
                listCusDO.Add("09-V-001" + " : " + "ยกเลิก");
                cbbCustomerName.Text = data_do.customerId + " : " + data_do.customerName;
                cbbCustomerName.Items.AddRange(listCusDO.ToArray());
            }

            cbbStoneType.Text = data_do.stoneTypeName;
            rbCredit.Checked = true;

            //ดึงหน้างาน
            fillSiteCombo();
            cbbSite.Text = data_do.siteName;
            for (int i = 0; i < cbbSite.Items.Count; i++)
            {
                var item = cbbSite.Items[i] as ComboboxValue;
                if (item != null && (item.Name == data_do.siteName || item.Id == data_do.siteId))
                {
                    cbbSite.SelectedIndex = i;
                    break;
                }
            }

            cbbStoneType.Enabled = false;
            if (cbbSite.Text != "" && tbDoDocNo.Text != "")
            {
                cbbSite.Enabled = false;
            }
        }

        private string getComboboxSiteUpdate()
        {
            return getComboboxId(cbbSite);
        }

        private string getComboboxStoneTypeUpdate()
        {
            return getComboboxId(cbbStoneType);
        }

        private string getComboboxMillUpdate()
        {
            return getComboboxId(cbbMill);
        }

        private string getComboboxCarTeamUpdate()
        {
            return getComboboxId(cbbCarTeam);
        }

        private string tonTokg(string tonStr)
        {
            double tmp = Convert.ToDouble(tonStr);
            double deci = tmp * 1000;
            string str = deci.ToString("#,##0.00");
            return str;
        }

        private string numberFormat(string numStr, int format)
        {
            double deci = Convert.ToDouble(numStr);
            string str = "";
            if (format == 1)
                str = deci.ToString();
            else if (format == 2)
                str = deci.ToString("#,##0.00");
            return str;
        }

        private void setDataMillToRB(string dataMill)
        {
            cbbMill.Text = dataMill; //111111111111
            /*
            if (dataMill.Equals("โรงโม่ 1"))
                rbMill1.Checked = true;
            else if (dataMill.Equals("โรงโม่ 2"))
                rbMill2.Checked = true;
            else if (dataMill.Equals("โรงโม่ 3"))
                rbMill3.Checked = true;
            else if (dataMill.Equals("ไม่มี"))
                rbMillNo.Checked = true;
            */
        }

        private void setDataSToisS(string dataS)
        {
            if (dataS.Equals("1") || dataS.Equals("True") || dataS.ToLower() == "true")
                cbS.Checked = true;
            else
                cbS.Checked = false;
        }

        private void setDataPayToRB(string dataPay)
        {
            if (dataPay.Equals("เงินสด"))
                rbCash.Checked = true;
            else if (dataPay.Equals("เงินเชื่อ"))
                rbCredit.Checked = true;
            else if (dataPay.Equals("เงินโอน"))
                rbTrans.Checked = true;
            else if (dataPay.Equals("Vat"))
                rbVat.Checked = true;
        }

        private void setDataVatToRB(string dataVat)
        {
            if (dataVat.Equals("ไม่รวมภาษี"))
                rbbVat.Checked = true;
            else if (dataVat.Equals("รวมภาษี"))
                rbbNonVat.Checked = true;
        }

        private void setDataCleanToRB(string dataClean)
        {
            if (dataClean.Equals("ล้างหิน"))
                rbCleanStone.Checked = true;
            else if (dataClean.Equals("สเปรย์น้ำ"))
                rbCleanWater.Checked = true;
            else if (dataClean.Equals("ไม่มี"))
                rbCleanNo.Checked = true;
        }

        private void setDefaultFromDB(string username, String firstname)
        {
            btMenu2.BackColor = Color.LightPink;
            btMenu3.BackColor = Color.LightPink;
            btMenu4.BackColor = Color.LightPink;
            btMenu5.BackColor = Color.LightPink;
            ucTruck.Show();
            ucReport.Hide();
            ucHelp.Hide();
            ucSetting.Hide();
            ucBackup.Hide();
            ucTruck.BringToFront();

            tbScaleId.Text = username;
            tbScaleName.Text = firstname;

            if (Globals.isPermissionSales())
            {
                btMenu1.Enabled = false;
                btMenu3.Enabled = false;
            }

            if (!Globals.isPermissionAddSetting())
            {
                btMenu3.Enabled = false;


                btLoadCustomer.Enabled = false;
            }


        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void chkDirectPrint_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\SerialPortListener"))
                {
                    if (key != null)
                    {
                        key.SetValue("DirectPrint", chkDirectPrint.Checked);
                    }
                }
            }
            catch {}
        }

        private void UserInitialization()
        {
            //Serial Port
            _spManager = new SerialPortManager();
            ucHelp.SetSerialPortManager(_spManager);
            SerialSettings mySerialSettings = _spManager.CurrentSerialSettings;
            serialSettingsBindingSource.DataSource = mySerialSettings;
            /*
            portNameComboBox.DataSource = mySerialSettings.PortNameCollection;
            baudRateComboBox.DataSource = mySerialSettings.BaudRateCollection;
            dataBitsComboBox.DataSource = mySerialSettings.DataBitsCollection;
            parityComboBox.DataSource = Enum.GetValues(typeof(System.IO.Ports.Parity));
            stopBitsComboBox.DataSource = Enum.GetValues(typeof(System.IO.Ports.StopBits));
            */

            _spManager.NewSerialDataRecieved += new EventHandler<SerialDataEventArgs>(_spManager_NewSerialDataRecieved);
            this.FormClosing += new FormClosingEventHandler(MainForm_FormClosing);

        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_spManager != null)
            {
                _spManager.Dispose();
            }
        }

        void _spManager_NewSerialDataRecieved(object sender, SerialDataEventArgs e)
        {
            if (this.InvokeRequired)
            {
                // Using this.Invoke causes deadlock when closing serial port, and BeginInvoke is good practice anyway.
                this.BeginInvoke(new EventHandler<SerialDataEventArgs>(_spManager_NewSerialDataRecieved), new object[] { sender, e });
                return;
            }

            int maxTextLength = 1000; // maximum text length in text box
            if (tbData.TextLength > maxTextLength)
                tbData.Text = tbData.Text.Remove(0, tbData.TextLength - maxTextLength);

            // This application is connected to a GPS sending ASCCI characters, so data is converted to text
            string str = Encoding.ASCII.GetString(e.Data);
            tbData.AppendText(str);
            tbData.ScrollToCaret();

            try
            {
                //แสดงเลขน้ำหนักที่กำลังวิ่ง
                /* เครื่องพี่จ๋า */
                /*
                string newString = tbData.Text.Remove(tbData.Text.LastIndexOf("KG"));
                string remainingText = newString.Substring(newString.LastIndexOf("\r"));
                MatchCollection mc = Regex.Matches(remainingText, @"\d+");
                */
                /* เครื่องพี่รุ่ง */

                string newString = tbData.Text.Remove(tbData.Text.LastIndexOf("kg"));
                string remainingText = newString.Substring(newString.LastIndexOf("G") + 3);
                MatchCollection mc = Regex.Matches(remainingText, @"\d+");

                if (mc.Count > 0)
                {
                    if (String.Compare(tbWeigtData.Text, mc[0].Value) != 0)
                    {
                        tbWeigtData.Text = mc[0].Value.TrimStart('0').PadLeft(1, '0');
                        //tbWeigtData.ForeColor = Color.LightCoral;
                    }
                    else
                    {
                        tbWeigtData.ForeColor = Color.LightGreen;
                    }
                }
            }
            catch (Exception ex)
            {

            }


        }

        // Handles the "Start Listening"-buttom click event
        private void btnStart_Click(object sender, EventArgs e)
        {
            _spManager.StartListening();
        }

        // Handles the "Stop Listening"-buttom click event
        private void btnStop_Click(object sender, EventArgs e)
        {
            _spManager.StopListening();
        }

        private void btRead_Click(object sender, EventArgs e)
        {
            try
            {
                _spManager.StopListening();

                /*
                int length = tbData.Text.Length;
                string substring = tbData.Text.Substring(length - 15, 7);
                tbWeightIn.Text = Regex.Match(substring, @"\d+").Value;
                tbData.Text = "";
                */

                tbWeightIn.Text = numberFormat(tbWeigtData.Text, 2);

                calculateWeight();
                _spManager.StartListening();

                //disable after read in
                if (!Globals.isPermissionTop())
                    disableBtAfterRead(1);
            }
            catch (Exception)
            {
            }
        }

        /* 
         * mode 0 -> enable all
         * mode 1 -> disable after read in
         * mode 2 -> disable after read out
         */
        private void disableBtAfterRead(int mode)
        {
            if (mode.Equals(0))
            {
                tbCarLicense.Enabled = true;
                tbCarCity.Enabled = true;

                tbCarCity.Enabled = true;
                dtWeightInDate.Enabled = true;
                dtWeightInTime.Enabled = true;
                dtWeightOutDate.Enabled = true;
                dtWeightOutTime.Enabled = true;
            }
            else if (mode.Equals(1))// weight in
            {
                disableReadWeightIn();
                dtWeightInDate.Enabled = false;
                dtWeightInTime.Enabled = false;

                if (!checkZeroStr(tbWeightIn.Text))
                    tbWeightIn.Enabled = false;
                if (!checkEmptyTB(tbCarLicense))
                {
                    tbCarLicense.Enabled = false;
                }

                if (!checkEmptyTB(tbCarCity))
                {
                    tbCarCity.Enabled = false;
                }

            }
            else if (mode.Equals(2))// weight out
            {
                disableReadWeightOut();
                dtWeightOutDate.Enabled = false;
                dtWeightOutTime.Enabled = false;

                if (!checkZeroStr(tbWeightOut.Text))
                {
                    tbWeightOut.Enabled = false;
                    tbWeightTotal.Enabled = false;
                    tbQ.Enabled = false;
                }
            }
            else if (mode.Equals(3))//open all admin add
            {
                tbWeightIn.Enabled = true;
                tbWeightOut.Enabled = true;
                tbWeightTotal.Enabled = true;
                tbQ.Enabled = true;
            }
            else if (mode.Equals(4))//disable all
            {
                dtWeightInDate.Enabled = false;
                dtWeightInTime.Enabled = false;

                dtWeightOutDate.Enabled = false;
                dtWeightOutTime.Enabled = false;

                tbWeightIn.Enabled = false;
                tbWeightOut.Enabled = false;
                tbWeightTotal.Enabled = false;
                tbQ.Enabled = false;
            }
            else if (mode.Equals(999))
            {//open all edit weight
                tbWeightIn.Enabled = true;
                tbWeightOut.Enabled = true;
                tbWeightTotal.Enabled = true;
                tbQ.Enabled = true;

                tbCarLicense.Enabled = true;
                tbCarCity.Enabled = true;
            }
        }


        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btMenu1_Click(object sender, EventArgs e)
        {
            btMenu1.BackColor = Color.White;
            btMenu2.BackColor = Color.LightPink;
            btMenu3.BackColor = Color.LightPink;
            btMenu4.BackColor = Color.LightPink;
            btMenu5.BackColor = Color.LightPink;

            ucTruck.BringToFront();
            ucTruck.Show();
            ucReport.Hide();
            ucHelp.Hide();
            ucSetting.Hide();
            ucBackup.Hide();

            TableFromDB mf = new TableFromDB(this);
            mf.ShowDialog();
        }

        private void btMenu2_Click(object sender, EventArgs e)
        {
            btMenu2.BackColor = Color.White;
            btMenu1.BackColor = Color.LightPink;
            btMenu3.BackColor = Color.LightPink;
            btMenu4.BackColor = Color.LightPink;
            btMenu5.BackColor = Color.LightPink;

            ucReport.Show();
            ucTruck.Hide();
            ucHelp.Hide();
            ucSetting.Hide();
            ucBackup.Hide();
            ucReport.BringToFront();

        }
        private void btMenu3_Click(object sender, EventArgs e)
        {
            btMenu3.BackColor = Color.White;
            btMenu1.BackColor = Color.LightPink;
            btMenu2.BackColor = Color.LightPink;
            btMenu4.BackColor = Color.LightPink;
            btMenu5.BackColor = Color.LightPink;

            ucSetting.Show();
            ucReport.Hide();
            ucHelp.Hide();
            ucTruck.Hide();
            ucBackup.Hide();
            ucSetting.BringToFront();
        }
        private void btMenu4_Click(object sender, EventArgs e)
        {
            btMenu4.BackColor = Color.White;
            btMenu1.BackColor = Color.LightPink;
            btMenu2.BackColor = Color.LightPink;
            btMenu3.BackColor = Color.LightPink;
            btMenu5.BackColor = Color.LightPink;

            ucHelp.Show();
            ucTruck.Hide();
            ucReport.Hide();
            ucSetting.Hide();
            ucBackup.Hide();
            ucHelp.BringToFront();
        }

        private void btMenu5_Click(object sender, EventArgs e)
        {
            btMenu5.BackColor = Color.White;
            btMenu1.BackColor = Color.LightPink;
            btMenu2.BackColor = Color.LightPink;
            btMenu3.BackColor = Color.LightPink;
            btMenu4.BackColor = Color.LightPink;

            ucBackup.Show();
            ucHelp.Hide();
            ucTruck.Hide();
            ucReport.Hide();
            ucSetting.Hide();
            ucBackup.BringToFront();
        }



        private void pnHelp_Paint(object sender, PaintEventArgs e)
        {

        }

        private void ucHelp_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void btReadOut_Click(object sender, EventArgs e)
        {
            try
            {
                _spManager.StopListening();

                /*
                int length = tbData.Text.Length;
                string substring = tbData.Text.Substring(length - 15, 7);
                tbWeightOut.Text = Regex.Match(substring, @"\d+").Value;
                */
                tbWeightOut.Text = numberFormat(tbWeigtData.Text, 2);

                calculateWeight();
                _spManager.StartListening();

                //disable after read out
                if (!Globals.isPermissionTop())
                    disableBtAfterRead(2);
            }
            catch (Exception)
            {
            }
        }

        private void calculateWeight()
        {
            string weightIn = tbWeightIn.Text;
            string weightOut = tbWeightOut.Text;
            double numWeightIn = 0;
            double numWeightOut = 0;

            if (weightIn != "" && weightIn != null && weightOut != "" && weightOut != null)
            {
                try
                {

                    numWeightIn = Convert.ToDouble(weightIn);
                    numWeightOut = Convert.ToDouble(weightOut);
                    double numWeight = 0;
                    if (numWeightIn > numWeightOut)
                        numWeight = numWeightIn - numWeightOut;
                    else if (numWeightIn < numWeightOut)
                        numWeight = numWeightOut - numWeightIn;
                    tbWeightTotal.Text = numWeight.ToString("#,##0.00");

                }
                catch (Exception)
                {
                }
            }

        }

        private Boolean checkDuplicateRunningNumber()
        {
            Boolean isDuplicate = false;
            string todayYear = DateTime.Now.ToString("yyyy");
            string startDate = todayYear + "-01-01";
            string endDate = todayYear + "-12-31";

            //sql get weight id
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT เลขที่เอกสาร FROM weight WHERE เลขที่เอกสาร = '" + tbDocNum.Text + "' AND วันที่ BETWEEN '" + startDate + "' AND '" + endDate + "' ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    isDuplicate = true;
                    //MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception)
            {
            }
            dl.close();
            return isDuplicate;
        }


        /* old check by table delivery order
        private int checkDeliveryOrder()
        {
            if (tbDoDocNo.Text != "") {
                int car_company_rem = -1;
                int car_customer_rem = -1;

                try
                {
                    dl.connect();

                    OdbcCommand pgCommand =
                        (OdbcCommand)dl.sqlConn().CreateCommand();

                    pgCommand.CommandText =
                        @"SELECT car_company_rem, car_customer_rem
                      FROM delivery_order
                      WHERE doc_no = ?";

                    pgCommand.Parameters.AddWithValue("", tbDoDocNo.Text);

                    OdbcDataReader reader = pgCommand.ExecuteReader();

                    if (reader.Read())
                    {
                        car_company_rem =
                            Convert.ToInt32(reader["car_company_rem"]);

                        car_customer_rem =
                            Convert.ToInt32(reader["car_customer_rem"]);
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("checkDeliveryOrder" + ex.Message);
                    return -1;
                }
                finally
                {
                    dl.close();
                }

                string carryType = findcarryTypeByTransport();

                if (carryType == "รับเอง" && car_customer_rem <= 0)
                    return 1;

                if (carryType == "ส่งให้" && car_company_rem <= 0)
                    return 2;
            }
            return 0;
        }
        */


        private int checkDeliveryOrder()
        {
            int car_company = 0;
            int car_customer = 0;

            try
            {
                dl.connect();
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText =
                    "SELECT car_company, car_customer " +
                    "FROM delivery_order WHERE do_id = ?";
                pgCommand.Parameters.AddWithValue("do_id", tbDoId.Text);

                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    car_company = Convert.ToInt32(reader["car_company"]);
                    car_customer = Convert.ToInt32(reader["car_customer"]);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                return 0;
            }
            finally
            {
                dl.close();
            }

            string carryType = findcarryTypeByTransport();

            if (carryType == "รับเอง")
                return (getDeliveryNotDoId(carryType) + 1) > car_customer ? 1 : 0;
            else if (carryType == "ส่งให้")
                return (getDeliveryNotDoId(carryType) + 1) > car_company ? 2 : 0;

            return 0;
        }

        private int getDeliveryNotDoId(string carryType)
        {

            int count_id = 0;

            //sql find company
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            StringBuilder sql = new StringBuilder();
            sql.Append("select count(weight_id) as count_id from weight_delivery where ");
            sql.Append("do_doc_no = '" + tbDoDocNo.Text + "' and carry_type_name = '" + carryType + "' and is_cancel = false ");
            if (tbId.Text != "")
                sql.Append(" and weight_id != '" + tbId.Text + "' ");

            pgCommand.CommandText = sql.ToString();
            //MessageBox.Show("pgCommand.CommandText = " + pgCommand.CommandText );
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    count_id = Convert.ToInt32(reader["count_id"].ToString());
                }
            }
            catch (Exception)
            {

            }
            dl.close();

            //MessageBox.Show("count_id = " + count_id + ", carryType = " + carryType);
            return count_id;
        }


        private bool checkHistoricalDateConstraint()
        {
            if (Globals.isPermissionEditWeight())
            {
                return true;
            }

            if ((DateTime.Today - dtDate.Value.Date).TotalDays > 1)
            {
                MessageBox.Show("ไม่สามารถบันทึกข้อมูลย้อนหลังได้", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private async void btSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!checkHistoricalDateConstraint())
                {
                    return;
                }

                // ==========================================
                // 1. UPDATE DELIVERY ORDER FROM API BEFORE SAVE
                // ==========================================
                if (!string.IsNullOrEmpty(tbDoId.Text))
                {
                    // เรียกใช้ฟังก์ชันอัปเดต และตรวจสอบ HTTP Status หรือผลลัพธ์
                    var updateResult = await UpdateDeliveryOrderFromApi();

                    if (!updateResult.IsSuccess)
                    {
                        if (updateResult.IsValidationError)
                        {
                            MessageBox.Show(
                                "ไม่สามารถอัปเดตข้อมูล Delivery Order ได้เนื่องจากข้อมูลไม่ถูกต้องตามเงื่อนไข (422 Unprocessable Entity) ระบบจะไม่บันทึกข้อมูล",
                                "Validation Error (422)",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );
                        }
                        else
                        {
                            MessageBox.Show(
                                "ไม่สามารถอัปเดตข้อมูล Delivery Order ได้ ระบบจะไม่บันทึกข้อมูล กรุณาเชื่อมต่อ Internet!!!",
                                "API Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                        }

                        return; // ⛔ หยุดทำงานทันที ห้ามไปต่อ
                    }

                    // ==========================================
                    // 2. CU WEIGHT DELIVERY FROM API
                    // ==========================================
                    bool apiSuccess = await CUWeightDeliveryFromApi();

                    if (!apiSuccess)
                    {
                        MessageBox.Show(
                            "ไม่สามารถเชื่อมต่อ API ได้ ระบบจะไม่บันทึกข้อมูล กรุณาเชื่อมต่อ Internet",
                            "API Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );

                        return; // ⛔ หยุดทำงาน
                    }
                }

                // ==========================================
                // 3. SAVE (ถ้าผ่านเงื่อนไขด้านบนทั้งหมดแล้ว)
                // ==========================================
                await autoSave();

                //MessageBox.Show("บันทึกข้อมูลสำเร็จ", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "ERROR",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private async Task<bool> autoSave()
        {
            if (!checkHistoricalDateConstraint())
            {
                return false;
            }

            Boolean isPasswordCorrect = true;

            string tmpDoId = tbDoId.Text;
            string tmpOldDoId = tbOldDoId.Text;

            int checkResult = checkDeliveryOrder();

            // =========================================================
            // INSERT
            // =========================================================
            if (tbId.Text == "")
            {
                isPasswordCorrect = checkCancelAction();

                // เช็คค่าว่าง
                if (tbDocNum.Text == "")
                {
                    MessageBox.Show(
                        "เลขที่การชั่งเป็นค่าว่าง กรุณาใส่เลขที่การชั่ง",
                        "แจ้งเตือน",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return false;
                }

                // PASSWORD
                if (!isPasswordCorrect)
                {
                    MessageBox.Show(
                        "รหัสยกเลิกผิด ไม่สามารถบันทึกข้อมูลได้",
                        "แจ้งเตือน",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return false;
                }

                // CHECK DUPLICATE
                if (checkDuplicateRunningNumber())
                {
                    MessageBox.Show(
                        "เลขที่การชั่งนี้ใช้ไปแล้ว กรุณาเข้าหน้าต่างใหม่",
                        "แจ้งเตือน",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return false;
                }

                // TRANSPORT EMPTY
                if (tbDoId.Text != "" && cbbTransport.Text == "")
                {
                    cbbTransport.Select();

                    MessageBox.Show(
                        "ขนส่งเป็นค่าว่าง กรุณาเลือกขนส่ง ไม่สามารถบันทึกข้อมูลได้",
                        "แจ้งเตือน",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return false;
                }

                // CHECK PLAN
                if (tbDoId.Text != "" && checkResult != 0)
                {
                    string error = "";

                    if (checkResult == 1)
                        error = "รถลูกค้าเกินกว่าที่ plan ในใบส่งของแล้ว";

                    else if (checkResult == 2)
                        error = "รถบริษัทเกินกว่าที่ plan ในใบส่งของแล้ว";

                    MessageBox.Show(
                        error + " ระบบไม่สามารถบันทึกข้อมูลได้",
                        "แจ้งเตือน",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return false;
                }

                // =========================
                // HAS DO → CHECK API CONNECT FIRST
                // =========================
                if (!string.IsNullOrEmpty(tmpDoId) && tmpDoId != "0")
                {
                    bool canConnect = await CheckApiConnect();

                    if (!canConnect)
                    {
                        MessageBox.Show(
                            "ไม่สามารถเชื่อมต่อ API ได้ ระบบจะไม่บันทึกข้อมูล กรุณาเชื่อมต่อ Internet",
                            "API Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );

                        return false; // ← STOP
                    }
                }

                // =========================
                // SAVE DATABASE
                // =========================
                saveActionOnly();

                // =========================
                // SEND WEIGHT DELIVERY
                // =========================
                bool apiFailedWithLimitExceeded = false;
                bool weightSuccess = true;
                if (!string.IsNullOrEmpty(tmpDoId) && tmpDoId != "0")
                {
                    int newWeightId = Convert.ToInt32(tbId.Text);
                    lastLimitExceededError = null;

                    weightSuccess =
                        await prepareWeightDelivery(tmpDoId, tmpOldDoId, newWeightId);

                    if (!weightSuccess && lastLimitExceededError != null)
                    {
                        apiFailedWithLimitExceeded = true;
                    }
                }

                if (apiFailedWithLimitExceeded)
                {
                    int newWeightId = Convert.ToInt32(tbId.Text);
                    rollbackInsert(newWeightId);
                    return false;
                }

                if (!string.IsNullOrEmpty(tmpDoId) && tmpDoId != "0" && !weightSuccess)
                {
                    MessageBox.Show(
                        "บันทึกข้อมูลสำเร็จ แต่ส่ง Weight Delivery ไม่สำเร็จ",
                        "API Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }

                disableAfterSave();
                return true;
            }

            // =========================================================
            // UPDATE
            // =========================================================
            else
            {
                isPasswordCorrect = checkCancelAction();

                // TRANSPORT EMPTY
                if (tbDoId.Text != "" && cbbTransport.Text == "")
                {
                    cbbTransport.Select();

                    MessageBox.Show(
                        "ขนส่งเป็นค่าว่าง กรุณาเลือกขนส่ง ไม่สามารถบันทึกข้อมูลได้",
                        "แจ้งเตือน",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return false;
                }

                // CHECK PLAN
                if (tbDoId.Text != "" && checkResult != 0)
                {
                    string error = "";

                    if (checkResult == 1)
                        error = "รถลูกค้าเกินกว่าที่ plan ในใบส่งของแล้ว";

                    else if (checkResult == 2)
                        error = "รถบริษัทเกินกว่าที่ plan ในใบส่งของแล้ว";

                    MessageBox.Show(
                        error + " ระบบไม่สามารถบันทึกข้อมูลได้",
                        "แจ้งเตือน",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return false;
                }

                // PASSWORD
                if (!isPasswordCorrect)
                {
                    MessageBox.Show(
                        "รหัสยกเลิกผิด ไม่สามารถบันทึกข้อมูลได้",
                        "แจ้งเตือน",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return false;
                }

                // =========================
                // HAS DO → CHECK API CONNECT FIRST
                // =========================
                if (!string.IsNullOrEmpty(tmpDoId) && tmpDoId != "0")
                {
                    bool canConnect = await CheckApiConnect();

                    if (!canConnect)
                    {
                        MessageBox.Show(
                            "ไม่สามารถเชื่อมต่อ API ได้ ระบบจะไม่แก้ไขข้อมูล กรุณาเชื่อมต่อ Internet",
                            "API Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );

                        return false; // ← STOP
                    }
                }

                // =========================
                // SEND WEIGHT DELIVERY (BEFORE UPDATE)
                // =========================
                bool apiFailedWithLimitExceeded = false;
                bool weightSuccess = true;
                if (!string.IsNullOrEmpty(tmpDoId) && tmpDoId != "0")
                {
                    int currentWeightId = Convert.ToInt32(tbId.Text);
                    lastLimitExceededError = null;

                    weightSuccess =
                        await prepareWeightDelivery(tmpDoId, tmpOldDoId, currentWeightId);

                    if (!weightSuccess && lastLimitExceededError != null)
                    {
                        apiFailedWithLimitExceeded = true;
                    }
                }

                if (apiFailedWithLimitExceeded)
                {
                    return false;
                }

                // =========================
                // UPDATE DATABASE
                // =========================
                updateActionOnly();

                if (!string.IsNullOrEmpty(tmpDoId) && tmpDoId != "0" && !weightSuccess)
                {
                    MessageBox.Show(
                        "แก้ไขข้อมูลสำเร็จ แต่ส่ง Weight Delivery ไม่สำเร็จ",
                        "API Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }

                disableAfterSave();
                return true;
            }
        }

        // =========================
        // CHECK API CONNECT (JWT PING ONLY)
        // =========================
        private async Task<bool> CheckApiConnect()
        {
            try
            {
                string baseUrl = getBaseApi(1, 1);
                string username = getBaseApi(2, 1);
                string password = getBaseApi(3, 1);

                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);

                    string accessToken =
                        await GetJwtToken(client, baseUrl, username, password);

                    return accessToken != null;
                }
            }
            catch
            {
                return false;
            }
        }

        private void saveActionOnly()
        {
            Boolean isSuccess = false;
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();

            StringBuilder sql = new StringBuilder();
            sql.Append("INSERT INTO weight (วันที่, เลขที่เอกสาร, ทะเบียนรถ, จังหวัด, คนขับ, ลูกค้า, น้ำหนักรถ, น้ำหนักรวม, น้ำหนักสินค้า , เลขที่ใบตัก, โรงโม่, ชนิดหิน, จ่ายเงิน, รหัสผู้ชั่ง, รหัสผู้ตัก, ราคาตัน, จำนวณเงิน, ค่าขนส่ง, วันที่ชั่งเข้า, เวลาชั่งเข้า, วันที่ชั่งออก, เวลาชั่งออก, รหัสลูกค้า, ชื่อผู้ชั่ง, ชื่อผู้ตัก, vat, รหัสผู้อนุมัติจ่าย, ชื่อผู้อนุมัติจ่าย, คิว, ชนิดvat, จำนวนเงินสุทธิ, ประเภทหิน, หน้างาน, ทีม, ล้าง, ขนส่ง, หมายเหตุ, carry_type_name, base_weight_station_name, bws, ");
            sql.Append(" do_id, do_doc_no,");
            sql.Append(" oil_content, site_id, stone_type_id, mill_id, car_team_id, is_s, stone_desc)");

            sql.Append("VALUES ('" + dtDate.Value.ToString("yyyy-MM-dd") + "','" + tbDocNum.Text + "','" + tbCarLicense.Text.TrimEnd() + "','" + tbCarCity.Text + "','" + tbDriverName.Text + "','" + tbCustomerName.Text + "','" + kgToTon(tbWeightIn));
            sql.Append("','" + kgToTon(tbWeightOut) + "','" + kgToTon(tbWeightTotal) + "','" + tbRefNum.Text + "','" + cbbMill.Text + "','" + cbbStoneType.Text + "','" + getPayRadioValue() + "','" + tbScaleId.Text);
            sql.Append("','" + tbScoopId.Text + "','" + numberFormat(tbPricePerTon.Text, 1) + "','" + numberFormat(tbAmount.Text, 1) + "','" + tbShipCost.Text + "','" + dtWeightInDate.Value.ToString("yyyy-MM-dd") + "','" + dtWeightInTime.Text + "','" + dtWeightOutDate.Value.ToString("yyyy-MM-dd") + "','" + dtWeightOutTime.Text);
            sql.Append("','" + tbCustomerId.Text + "','" + tbScaleName.Text + "','" + tbScoopName.Text + "','" + numberFormat(tbVat.Text, 1) + "','" + tbApproveId.Text + "','" + tbApproveName.Text + "','" + numberFormat(tbQ.Text, 1) + "','" + getVatRadioValue() + "','" + numberFormat(tbAmountVat.Text, 1));
            sql.Append("','" + cbbStoneColor.Text + "','" + cbbSite.Text + "','" + cbbCarTeam.Text + "','" + getCleanRadioValue() + "','" + cbbTransport.Text + "','" + tbNote.Text + "','" + findcarryTypeByTransport() + "', (SELECT base_weight_station_name FROM base_weight_station WHERE base_weight_station_id = 1 ) , (SELECT code FROM base_weight_station WHERE base_weight_station_id = 1 )");
            sql.Append(" , " + CheckText(tbDoId.Text) + " ,'" + tbDoDocNo.Text + "'");
            sql.Append(" , '" + numberFormat(tbOilContent.Text, 1) + "','" + getComboboxId(cbbSite) + "','" + getComboboxId(cbbStoneType) + "','" + getComboboxId(cbbMill) + "','" + getComboboxId(cbbCarTeam) + "','" + cbS.Checked + "','" + tbStoneDesc.Text + "' )");

            pgCommand.CommandText = sql.ToString();

            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                isSuccess = runningDocNumberAfterSave();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            dl.close();

            //set WeightId
            if (isSuccess)
                setWeightId();
        }

        private void updateActionOnly()
        {
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            StringBuilder sql = new StringBuilder();
            sql.Append("UPDATE weight SET ทะเบียนรถ = '" + tbCarLicense.Text.TrimEnd() + "' , จังหวัด = '" + tbCarCity.Text + "' , คนขับ = '" + tbDriverName.Text + "', ลูกค้า = '" + tbCustomerName.Text + "' , น้ำหนักรถ = '" + kgToTon(tbWeightIn) + "' , น้ำหนักรวม = '" + kgToTon(tbWeightOut));
            sql.Append("' , น้ำหนักสินค้า = '" + kgToTon(tbWeightTotal) + "' , เลขที่ใบตัก = '" + tbRefNum.Text + "' , โรงโม่ = '" + cbbMill.Text + "' , ชนิดหิน = '" + cbbStoneType.Text + "' , จ่ายเงิน = '" + getPayRadioValue() + "' , รหัสผู้ชั่ง = '" + tbScaleId.Text);
            sql.Append("' , รหัสผู้ตัก = '" + tbScoopId.Text + "' , ราคาตัน = '" + numberFormat(tbPricePerTon.Text, 1) + "' , จำนวณเงิน = '" + numberFormat(tbAmount.Text, 1) + "' , ค่าขนส่ง = '" + tbShipCost.Text + "' , วันที่ชั่งเข้า = '" + dtWeightInDate.Value.ToString("yyyy-MM-dd") + "' , เวลาชั่งเข้า = '" + dtWeightInTime.Text);
            sql.Append("' , วันที่ชั่งออก = '" + dtWeightOutDate.Value.ToString("yyyy-MM-dd") + "' , เวลาชั่งออก = '" + dtWeightOutTime.Text + "'  , รหัสลูกค้า = '" + tbCustomerId.Text + "'  , ชื่อผู้ชั่ง = '" + tbScaleName.Text + "' , ชื่อผู้ตัก = '" + tbScoopName.Text + "' , vat = '" + numberFormat(tbVat.Text, 1));
            sql.Append("' , รหัสผู้อนุมัติจ่าย = '" + tbApproveId.Text + "' , ชื่อผู้อนุมัติจ่าย = '" + tbApproveName.Text + "' , คิว = '" + numberFormat(tbQ.Text, 1) + "' , ชนิดvat = '" + getVatRadioValue() + "' , จำนวนเงินสุทธิ = '" + numberFormat(tbAmountVat.Text, 1) + "' , ประเภทหิน = '" + cbbStoneColor.Text);
            sql.Append("' , หน้างาน = '" + cbbSite.Text + "' , ทีม = '" + cbbCarTeam.Text + "' , ล้าง = '" + getCleanRadioValue() + "' , ขนส่ง = '" + cbbTransport.Text + "' , carry_type_name = '" + findcarryTypeByTransport() + "' , หมายเหตุ = '" + tbNote.Text + "' , oil_content = '" + numberFormat(tbOilContent.Text, 1));
            sql.Append("' , site_id = '" + getComboboxSiteUpdate() + "' , stone_type_id = '" + getComboboxStoneTypeUpdate() + "' , mill_id = '" + getComboboxMillUpdate() + "', is_s = '" + cbS.Checked + "' , car_team_id = '" + getComboboxCarTeamUpdate());
            sql.Append("' , do_id = " + CheckText(tbDoId.Text) + " , do_doc_no = '" + tbDoDocNo.Text + "'");
            sql.Append(" , stone_desc = '" + tbStoneDesc.Text + "'");
            sql.Append(" WHERE วันที่ = '" + dtDate.Value.ToString("yyyy-MM-dd") + "' AND weight_id = " + tbId.Text + " ; ");

            pgCommand.CommandText = sql.ToString();

            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                //MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                while (reader.Read())
                {

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            dl.close();
        }

        private void rollbackInsert(int weightId)
        {
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "DELETE FROM weight WHERE weight_id = " + weightId;
            try
            {
                dl.connect();
                pgCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rollback weight record failed: " + ex.Message);
            }
            dl.close();

            pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "DELETE FROM weight_log WHERE weight_id = " + weightId;
            try
            {
                dl.connect();
                pgCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rollback weight log failed: " + ex.Message);
            }
            dl.close();

            tbId.Text = "";
        }

        private void saveAction()
        {
            Boolean isSuccess = false;
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();

            StringBuilder sql = new StringBuilder();
            sql.Append("INSERT INTO weight (วันที่, เลขที่เอกสาร, ทะเบียนรถ, จังหวัด, คนขับ, ลูกค้า, น้ำหนักรถ, น้ำหนักรวม, น้ำหนักสินค้า , เลขที่ใบตัก, โรงโม่, ชนิดหิน, จ่ายเงิน, รหัสผู้ชั่ง, รหัสผู้ตัก, ราคาตัน, จำนวณเงิน, ค่าขนส่ง, วันที่ชั่งเข้า, เวลาชั่งเข้า, วันที่ชั่งออก, เวลาชั่งออก, รหัสลูกค้า, ชื่อผู้ชั่ง, ชื่อผู้ตัก, vat, รหัสผู้อนุมัติจ่าย, ชื่อผู้อนุมัติจ่าย, คิว, ชนิดvat, จำนวนเงินสุทธิ, ประเภทหิน, หน้างาน, ทีม, ล้าง, ขนส่ง, หมายเหตุ, carry_type_name, base_weight_station_name, bws, ");
            sql.Append(" do_id, do_doc_no,");
            sql.Append(" oil_content, site_id, stone_type_id, mill_id, car_team_id, is_s, stone_desc)");

            sql.Append("VALUES ('" + dtDate.Value.ToString("yyyy-MM-dd") + "','" + tbDocNum.Text + "','" + tbCarLicense.Text.TrimEnd() + "','" + tbCarCity.Text + "','" + tbDriverName.Text + "','" + tbCustomerName.Text + "','" + kgToTon(tbWeightIn));
            sql.Append("','" + kgToTon(tbWeightOut) + "','" + kgToTon(tbWeightTotal) + "','" + tbRefNum.Text + "','" + cbbMill.Text + "','" + cbbStoneType.Text + "','" + getPayRadioValue() + "','" + tbScaleId.Text);
            sql.Append("','" + tbScoopId.Text + "','" + numberFormat(tbPricePerTon.Text, 1) + "','" + numberFormat(tbAmount.Text, 1) + "','" + tbShipCost.Text + "','" + dtWeightInDate.Value.ToString("yyyy-MM-dd") + "','" + dtWeightInTime.Text + "','" + dtWeightOutDate.Value.ToString("yyyy-MM-dd") + "','" + dtWeightOutTime.Text);
            sql.Append("','" + tbCustomerId.Text + "','" + tbScaleName.Text + "','" + tbScoopName.Text + "','" + numberFormat(tbVat.Text, 1) + "','" + tbApproveId.Text + "','" + tbApproveName.Text + "','" + numberFormat(tbQ.Text, 1) + "','" + getVatRadioValue() + "','" + numberFormat(tbAmountVat.Text, 1));
            sql.Append("','" + cbbStoneColor.Text + "','" + cbbSite.Text + "','" + cbbCarTeam.Text + "','" + getCleanRadioValue() + "','" + cbbTransport.Text + "','" + tbNote.Text + "','" + findcarryTypeByTransport() + "', (SELECT base_weight_station_name FROM base_weight_station WHERE base_weight_station_id = 1 ) , (SELECT code FROM base_weight_station WHERE base_weight_station_id = 1 )");
            sql.Append(" , " + CheckText(tbDoId.Text) + " ,'" + tbDoDocNo.Text + "'");
            sql.Append(" , '" + numberFormat(tbOilContent.Text, 1) + "','" + getComboboxId(cbbSite) + "','" + getComboboxId(cbbStoneType) + "','" + getComboboxId(cbbMill) + "','" + getComboboxId(cbbCarTeam) + "','" + cbS.Checked + "','" + tbStoneDesc.Text + "' )");

            pgCommand.CommandText = sql.ToString();

            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                isSuccess = runningDocNumberAfterSave();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            dl.close();

            //set WeightId
            if (isSuccess)
                setWeightId();

            //ปิดช่องหลัง save
            disableAfterSave();
        }

        private void saveWeightHistory()
        {

            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "INSERT INTO weight_log (weight_id, วันที่, เลขที่เอกสาร, ทะเบียนรถ, จังหวัด, คนขับ, ลูกค้า, น้ำหนักรถ, น้ำหนักรวม, น้ำหนักสินค้า , เลขที่ใบตัก, โรงโม่, ชนิดหิน, จ่ายเงิน, รหัสผู้ชั่ง, รหัสผู้ตัก, ราคาตัน, จำนวณเงิน, ค่าขนส่ง, วันที่ชั่งเข้า, เวลาชั่งเข้า, วันที่ชั่งออก, เวลาชั่งออก, รหัสลูกค้า, ชื่อผู้ชั่ง, ชื่อผู้ตัก, vat, รหัสผู้อนุมัติจ่าย, ชื่อผู้อนุมัติจ่าย, คิว, ชนิดvat, จำนวนเงินสุทธิ, ประเภทหิน, หน้างาน, ทีม, ล้าง, ขนส่ง, หมายเหตุ, carry_type_name, base_weight_station_name, oil_content, site_id, stone_type_id, mill_id, car_team_id, is_s, do_id, do_doc_no, stone_desc)" +
                                     "VALUES ('" + tbId.Text + "','" + dtDate.Value.ToString("yyyy-MM-dd") + "','" + tbDocNum.Text + "','" + tbCarLicense.Text.TrimEnd() + "','" + tbCarCity.Text + "','" + tbDriverName.Text + "','" + tbCustomerName.Text + "','" + kgToTon(tbWeightIn) + "'" + ",'"
                                     + kgToTon(tbWeightOut) + "','" + kgToTon(tbWeightTotal) + "','" + tbRefNum.Text + "','" + cbbMill.Text + "','" + cbbStoneType.Text + "','" + getPayRadioValue() + "','" + tbScaleId.Text + "','"
                                     + tbScoopId.Text + "','" + numberFormat(tbPricePerTon.Text, 1) + "','" + numberFormat(tbAmount.Text, 1) + "','" + tbShipCost.Text + "','" + dtWeightInDate.Value.ToString("yyyy-MM-dd") + "','" + dtWeightInTime.Text + "','" + dtWeightOutDate.Value.ToString("yyyy-MM-dd") + "','" + dtWeightOutTime.Text + "','"
                                     + tbCustomerId.Text + "','" + tbScaleName.Text + "','" + tbScoopName.Text + "','" + numberFormat(tbVat.Text, 1) + "','" + tbApproveId.Text + "','" + tbApproveName.Text + "','" + numberFormat(tbQ.Text, 1) + "','" + getVatRadioValue() + "','" + numberFormat(tbAmountVat.Text, 1) + "','"
                                     + cbbStoneColor.Text + "','" + cbbSite.Text + "','" + cbbCarTeam.Text + "','" + getCleanRadioValue() + "','" + cbbTransport.Text + "','" + tbNote.Text + "','" + findcarryTypeByTransport() + "', (SELECT base_weight_station_name FROM base_weight_station WHERE base_weight_station_id = 1 ) ,'"
                                     + numberFormat(tbOilContent.Text, 1) + "','" + getComboboxId(cbbSite) + "','" + getComboboxId(cbbStoneType) + "','" + getComboboxId(cbbMill) + "','" + getComboboxId(cbbCarTeam) + "','" + cbS.Checked + "', " + CheckText(tbDoId.Text) + " ,'" + tbDoDocNo.Text + "','" + tbStoneDesc.Text + "')";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            dl.close();

        }

        private void setWeightId()
        {
            //sql get weight id
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT weight_id FROM public.weight WHERE เลขที่เอกสาร = '" + tbDocNum.Text + "' AND วันที่ = '" + dtDate.Value.ToString("yyyy-MM-dd") + "' ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string rdStr = reader["weight_id"].ToString();
                    tbId.Text = rdStr;
                    //MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception)
            {
            }
            dl.close();
        }

        private void updateAction()
        {
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            StringBuilder sql = new StringBuilder();
            sql.Append("UPDATE weight SET ทะเบียนรถ = '" + tbCarLicense.Text.TrimEnd() + "' , จังหวัด = '" + tbCarCity.Text + "' , คนขับ = '" + tbDriverName.Text + "', ลูกค้า = '" + tbCustomerName.Text + "' , น้ำหนักรถ = '" + kgToTon(tbWeightIn) + "' , น้ำหนักรวม = '" + kgToTon(tbWeightOut));
            sql.Append("' , น้ำหนักสินค้า = '" + kgToTon(tbWeightTotal) + "' , เลขที่ใบตัก = '" + tbRefNum.Text + "' , โรงโม่ = '" + cbbMill.Text + "' , ชนิดหิน = '" + cbbStoneType.Text + "' , จ่ายเงิน = '" + getPayRadioValue() + "' , รหัสผู้ชั่ง = '" + tbScaleId.Text);
            sql.Append("' , รหัสผู้ตัก = '" + tbScoopId.Text + "' , ราคาตัน = '" + numberFormat(tbPricePerTon.Text, 1) + "' , จำนวณเงิน = '" + numberFormat(tbAmount.Text, 1) + "' , ค่าขนส่ง = '" + tbShipCost.Text + "' , วันที่ชั่งเข้า = '" + dtWeightInDate.Value.ToString("yyyy-MM-dd") + "' , เวลาชั่งเข้า = '" + dtWeightInTime.Text);
            sql.Append("' , วันที่ชั่งออก = '" + dtWeightOutDate.Value.ToString("yyyy-MM-dd") + "' , เวลาชั่งออก = '" + dtWeightOutTime.Text + "'  , รหัสลูกค้า = '" + tbCustomerId.Text + "'  , ชื่อผู้ชั่ง = '" + tbScaleName.Text + "' , ชื่อผู้ตัก = '" + tbScoopName.Text + "' , vat = '" + numberFormat(tbVat.Text, 1));
            sql.Append("' , รหัสผู้อนุมัติจ่าย = '" + tbApproveId.Text + "' , ชื่อผู้อนุมัติจ่าย = '" + tbApproveName.Text + "' , คิว = '" + numberFormat(tbQ.Text, 1) + "' , ชนิดvat = '" + getVatRadioValue() + "' , จำนวนเงินสุทธิ = '" + numberFormat(tbAmountVat.Text, 1) + "' , ประเภทหิน = '" + cbbStoneColor.Text);
            sql.Append("' , หน้างาน = '" + cbbSite.Text + "' , ทีม = '" + cbbCarTeam.Text + "' , ล้าง = '" + getCleanRadioValue() + "' , ขนส่ง = '" + cbbTransport.Text + "' , carry_type_name = '" + findcarryTypeByTransport() + "' , หมายเหตุ = '" + tbNote.Text + "' , oil_content = '" + numberFormat(tbOilContent.Text, 1));
            sql.Append("' , site_id = '" + getComboboxSiteUpdate() + "' , stone_type_id = '" + getComboboxStoneTypeUpdate()  + "' , mill_id = '" + getComboboxMillUpdate() + "', is_s = '" + cbS.Checked + "', car_team_id = '" + getComboboxCarTeamUpdate() );
            sql.Append("' , do_id = " + CheckText(tbDoId.Text) + " , do_doc_no = '" + tbDoDocNo.Text + "'");
            sql.Append(" , stone_desc = '" + tbStoneDesc.Text + "'");
            sql.Append(" WHERE วันที่ = '" + dtDate.Value.ToString("yyyy-MM-dd") + "' AND weight_id = " + tbId.Text + " ; ");

            pgCommand.CommandText = sql.ToString();

            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                //MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                while (reader.Read())
                {

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            dl.close();

            //ปิดช่องหลัง save
            disableAfterSave();
        }


        //get combobox id use to save or update
        private string getComboboxId(ComboBox cbb)
        {
            string tmp = "";

            try
            {
                if (cbb.SelectedIndex > -1)
                {
                    ComboboxValue tmpComboboxValue = (ComboboxValue)cbb.SelectedItem;
                    tmp = tmpComboboxValue.Id;
                }
                else if (!string.IsNullOrEmpty(cbb.Text))
                {
                    foreach (var item in cbb.Items)
                    {
                        if (item is ComboboxValue val && val.Name == cbb.Text)
                        {
                            tmp = val.Id;
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(tmp))
                    {
                        if (cbb == cbbSite)
                        {
                            tmp = getSiteIdFromDbByName(cbb.Text);
                        }
                        else if (cbb == cbbStoneType)
                        {
                            tmp = getStoneTypeIdFromDbByName(cbb.Text);
                        }
                        else if (cbb == cbbMill)
                        {
                            tmp = getMillIdFromDbByName(cbb.Text);
                        }
                        else if (cbb == cbbCarTeam)
                        {
                            tmp = getCarTeamIdFromDbByName(cbb.Text);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return tmp;
        }

        private string getSiteIdFromDbByName(string name)
        {
            string id = "";
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT base_site_id FROM public.base_site WHERE (weight_type = 1 or weight_type = 3) and base_site_name = ? LIMIT 1 ";
            pgCommand.Parameters.AddWithValue("", name);
            try
            {
                dl.connect();
                object val = pgCommand.ExecuteScalar();
                if (val != null && val != DBNull.Value)
                {
                    id = val.ToString();
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                dl.close();
            }
            return id;
        }

        private string getStoneTypeIdFromDbByName(string name)
        {
            string id = "";
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT รหัสหิน FROM public.base_stone_type WHERE inactive = false and ชื่อหิน = ? LIMIT 1";
            pgCommand.Parameters.AddWithValue("", name);
            try
            {
                dl.connect();
                object val = pgCommand.ExecuteScalar();
                if (val != null && val != DBNull.Value)
                {
                    id = val.ToString();
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                dl.close();
            }
            return id;
        }

        private string getMillIdFromDbByName(string name)
        {
            string id = "";
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT รหัสโรงโม่ FROM public.base_mill WHERE (weight_type = 1 or weight_type = 3) and ชื่อโรงโม่ = ? LIMIT 1";
            pgCommand.Parameters.AddWithValue("", name);
            try
            {
                dl.connect();
                object val = pgCommand.ExecuteScalar();
                if (val != null && val != DBNull.Value)
                {
                    id = val.ToString();
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                dl.close();
            }
            return id;
        }

        private string getCarTeamIdFromDbByName(string name)
        {
            string id = "";
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT รหัสทีม FROM public.base_car_team WHERE ชื่อทีม = ? LIMIT 1";
            pgCommand.Parameters.AddWithValue("", name);
            try
            {
                dl.connect();
                object val = pgCommand.ExecuteScalar();
                if (val != null && val != DBNull.Value)
                {
                    id = val.ToString();
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                dl.close();
            }
            return id;
        }

        private void disableAfterSave()
        {
            if (!Globals.isPermissionEditWeight())
            {
                if (!checkZeroStr(tbWeightIn.Text))
                    disableBtAfterRead(1);
                if (!checkZeroStr(tbWeightOut.Text))
                    disableBtAfterRead(2);
            }

            //รหัสยกเลิกให้ปิดช่องให้หมด
            disableCancelId();

            //19-09-2023 มาเก็บ weight history ตรงนี้นะ
            saveWeightHistory();

        }

        /* ใช้แบบใหม่แล้ว
        private void prepareUpdateDo(string tmpDoId, string tmpOldDoId)
        {

            //MessageBox.Show("tmpOldDoId = "+ tmpOldDoId + ", tmpDoId = " + tmpDoId);
            //20-02-2026 มาเก็บ Delivery Order ตรงนี้นะ
            if (tmpOldDoId != tmpDoId)
            {
                updateDeliveryOrder(tmpOldDoId);
                updateDeliveryOrder(tmpDoId);
            }
            else
            {
                updateDeliveryOrder(tmpDoId);
            }
        }
        */


        private async Task<bool> prepareWeightDelivery(
            string tmpDoId,
            string tmpOldDoId,
            int weightId
        )
        {
            try
            {
                // =========================
                // SKIP IF NO DO SELECTED
                // =========================
                if (string.IsNullOrEmpty(tmpDoId) || tmpDoId == "0")
                    return true;

                if (tmpOldDoId != tmpDoId)
                {
                    // only cancel old DO if it was actually set
                    if (!string.IsNullOrEmpty(tmpOldDoId) && tmpOldDoId != "0")
                    {
                        bool oldResult = await UCWeightDelivery(tmpOldDoId, true, weightId);
                        if (!oldResult) return false;
                    }

                    bool newResult = await UCWeightDelivery(tmpDoId, false, weightId);
                    if (!newResult) return false;
                }
                else
                {
                    bool result = await UCWeightDelivery(tmpDoId, false, weightId);
                    if (!result) return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "ERROR",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return false;
            }
        }


        private async Task<bool> UCWeightDelivery(string do_id, bool is_cancel, int weightId)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    // =========================
                    // JWT LOGIN
                    // =========================
                    string baseUrl = getBaseApi(1, 1);
                    string username = getBaseApi(2, 1);
                    string password = getBaseApi(3, 1);
                    string comp_code = getBaseApi(4, 1);

                    string accessToken =
                        await GetJwtToken(client, baseUrl, username, password);

                    if (accessToken == null)
                        return false;

                    // =========================
                    // SET TOKEN
                    // =========================
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(
                            "Bearer",
                            accessToken
                        );

                    // =========================
                    // API URL
                    // =========================
                    string apiUrl =
                        $"{baseUrl}/api/uc_weight_delivery/";

                    // =========================
                    // FETCH DELIVERY ORDER DATA
                    // =========================
                    string doc_no = "";
                    string delivery_date_str = "";
                    string unitName = "";
                    int car_company = 0;
                    int car_customer = 0;
                    string status = "";
                    double qty = 0;

                    using (OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand())
                    {
                        pgCommand.CommandText = "SELECT doc_no, delivery_date, unit_name, car_company, car_customer, status, qty FROM delivery_order where do_id = ?";
                        pgCommand.Parameters.AddWithValue("?", do_id);
                        try
                        {
                            dl.connect();
                            using (OdbcDataReader reader = pgCommand.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    doc_no = reader["doc_no"] != DBNull.Value ? reader["doc_no"].ToString() : "";
                                    delivery_date_str = reader["delivery_date"] != DBNull.Value ? reader["delivery_date"].ToString() : "";
                                    unitName = reader["unit_name"] != DBNull.Value ? reader["unit_name"].ToString() : "";
                                    int.TryParse(reader["car_company"] != DBNull.Value ? reader["car_company"].ToString() : "0", out car_company);
                                    int.TryParse(reader["car_customer"] != DBNull.Value ? reader["car_customer"].ToString() : "0", out car_customer);
                                    status = reader["status"] != DBNull.Value ? reader["status"].ToString() : "";
                                    double.TryParse(reader["qty"] != DBNull.Value ? reader["qty"].ToString() : "0", out qty);
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                        finally
                        {
                            dl.close();
                        }
                    }

                    DateTime deliveryDate = DateTime.Now;
                    if (!string.IsNullOrEmpty(delivery_date_str))
                    {
                        DateTime.TryParse(delivery_date_str, out deliveryDate);
                    }

                    bool real_is_cancel = is_cancel || isCancelDO();

                    // =========================
                    // API DATA
                    // =========================
                    var apiData = new
                    {
                        weight_id = weightId,

                        delivery_date =
                            deliveryDate.ToString("yyyy-MM-dd"),

                        bws = findBWS(),
                        comp_code = comp_code,

                        do_id =
                            Convert.ToInt32(do_id),

                        do_doc_no = doc_no,

                        carry_type_name =
                            findcarryTypeByTransport(),

                        weight_ton =
                            kgToTon(tbWeightTotal),

                        weight_q =
                            Convert.ToDouble(tbQ.Text),

                        unit_name = unitName,

                        car_company = car_company,

                        car_customer = car_customer,

                        is_cancel = real_is_cancel,

                        status = status,
                        qty = qty
                    };

                    string apiJson =
                        JsonConvert.SerializeObject(apiData);

                    var apiContent =
                        new StringContent(
                            apiJson,
                            Encoding.UTF8,
                            "application/json"
                        );

                    // =========================
                    // CALL API
                    // =========================
                    HttpResponseMessage apiResponse =
                        await client.PostAsync(apiUrl, apiContent);

                    string responseContent =
                        await apiResponse.Content.ReadAsStringAsync();

                    if (responseContent.Contains("car_customer limit exceeded"))
                    {
                        lastLimitExceededError = "car_customer";
                        MessageBox.Show(
                            "ไม่สามารถบันทึกข้อมูลได้เนื่องจากรถลูกค้าเกินจากที่วาง plan ไว้ กรุณาติดต่อพนักงานขาย",
                            "แจ้งเตือน",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                        return false;
                    }
                    else if (responseContent.Contains("car_company limit exceeded"))
                    {
                        lastLimitExceededError = "car_company";
                        MessageBox.Show(
                            "ไม่สามารถบันทึกข้อมูลได้เนื่องจากรถบริษัทเกินจากที่วาง plan ไว้ กรุณาติดต่อพนักงานขาย",
                            "แจ้งเตือน",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                        return false;
                    }

                    if (apiResponse.IsSuccessStatusCode)
                    {
                        //Console.WriteLine("SUCCESS : " + responseContent);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show(
                            "API ERROR : " + responseContent,
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );

                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "EXCEPTION",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return false;
            }
        }


        private async Task<string> GetJwtToken(HttpClient client, string baseUrl, string username, string password)
        {
            try
            {
                string jwtUrl = $"{baseUrl}/jwt/create/";

                var loginData = new
                {
                    username = username,
                    password = password
                };

                string loginJson =
                    JsonConvert.SerializeObject(loginData);

                var loginContent =
                    new StringContent(
                        loginJson,
                        Encoding.UTF8,
                        "application/json"
                    );

                HttpResponseMessage jwtResponse =
                    await client.PostAsync(jwtUrl, loginContent);

                if (!jwtResponse.IsSuccessStatusCode)
                {
                    string jwtError =
                        await jwtResponse.Content.ReadAsStringAsync();

                    /*
                    MessageBox.Show(
                        "JWT ERROR : " + jwtError,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    */

                    return null;
                }

                string jwtResult =
                    await jwtResponse.Content.ReadAsStringAsync();

                dynamic jwtObj =
                    JsonConvert.DeserializeObject(jwtResult);

                return jwtObj.access.ToString();
            }
            catch (Exception ex)
            {
                /*
                MessageBox.Show(
                    ex.ToString(),
                    "JWT EXCEPTION",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                */
                return null;
            }
        }


        private Boolean isCancelDO()
        {
            Boolean is_cancel = false;
            if (tbCustomerId.Text == "09-V-001")
            {
                is_cancel = true;
            }
            return is_cancel;
        }

        private void updateDeliveryOrder(string do_id)
        {
            if (do_id != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "UPDATE delivery_order SET car_company_tot = '" + calculateDOTotal(do_id, 1) + "' , car_customer_tot = '" + calculateDOTotal(do_id, 2) +
                                        "' , qty_tot = '" + calculateQtyTotal(do_id) + "'" +
                                        " WHERE delivery_date = '" + dtDate.Value.ToString("yyyy-MM-dd") + "' AND do_id = " + do_id + " ; ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    //MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    while (reader.Read())
                    {

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                dl.close();

            }
        }

        //ดึงจำนวนขนส่งที่ใช้จริง
        private int calculateDOTotal(string str_do_id, int mode)
        {
            if (mode == 1)
                return Convert.ToInt32(getDoFromSql(str_do_id, "ส่งให้"));
            else if (mode == 2)
                return Convert.ToInt32(getDoFromSql(str_do_id, "รับเอง"));
            else
                return 0;
        }


        private decimal calculateQtyTotal(string do_id)
        {
            string count_id = "";
            string ton_qty_total = "";
            string q_qty_total = "";
            string unit_name = "";

            //sql find company
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("COUNT(weight.do_id) AS count_id, SUM(weight.น้ำหนักสินค้า) AS ton_qty_total, SUM(weight.คิว) AS q_qty_total, MAX(delivery_order.unit_name) AS unit_name ");
            sql.Append("FROM weight JOIN delivery_order ON weight.do_id = delivery_order.do_id ");
            sql.Append("WHERE weight.do_id = '" + do_id + "' ;");
            pgCommand.CommandText = sql.ToString();
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    count_id = reader["count_id"].ToString();
                    ton_qty_total = reader["ton_qty_total"].ToString();
                    q_qty_total = reader["q_qty_total"].ToString();
                    unit_name = reader["unit_name"].ToString();
                }
            }
            catch (Exception)
            {
            }
            dl.close();

            if (unit_name == "ตัน")
                return Convert.ToDecimal(ton_qty_total);
            else if (unit_name == "คิว")
                return Convert.ToDecimal(q_qty_total);
            else
                return 0;
        }


        private string getDoFromSql(string do_id, string carry_type_name)
        {

            string count_id = "";

            //sql find company
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "select count(do_id) as count_id from weight where do_id = '" + do_id + "' and carry_type_name = '" + carry_type_name + "' ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    count_id = reader["count_id"].ToString();
                }
            }
            catch (Exception)
            {
            }
            dl.close();

            return count_id;
        }


        private void disableCancelId()
        {
            if (tbCustomerId.Text == "09-A-001" || tbCustomerId.Text == "09-V-001")
            {
                disableBtAfterRead(4);
                if (checkEmptyTB(tbNote))
                {
                    MessageBox.Show("กรุณาใส่เหตุผลในการยกเลิก", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tbNote.Select();
                }
                updateStatusCancel(true);
            }
            else
            {
                updateStatusCancel(false);
            }
        }

        private void updateStatusCancel(Boolean status)
        {
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "UPDATE weight SET is_cancel =  " + status + "  WHERE วันที่ = '" + dtDate.Value.ToString("yyyy-MM-dd") + "' AND weight_id = " + tbId.Text + " ; ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                //MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                while (reader.Read())
                {

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            dl.close();
        }

        private Boolean checkZeroStr(string str)
        {
            Double temp;
            Boolean isOk = Double.TryParse(str, out temp);
            Int32 value = isOk ? (Int32)temp : 0;

            return value == 0 ? true : false;
        }

        private Boolean checkEmptyTB(TextBox tb)
        {
            return string.IsNullOrEmpty(tb.Text) == true ? true : false;
        }


        private Boolean runningDocNumberAfterSave()
        {
            Boolean isSuccess = false;
            string todayYear = DateTime.Now.ToString("yyyy");
            //sql find
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public.seq_doc_num where run_year = '" + todayYear + "'";
            try
            {

                //sql update
                pgCommand.CommandText = "UPDATE public.seq_doc_num SET run_number = '" + tbDocNum.Text + "' where run_year = '" + todayYear + "'";
                OdbcDataReader reader = pgCommand.ExecuteReader();
                isSuccess = true;

            }
            catch (Exception)
            {
            }
            return isSuccess;
        }

        private string getMillRadioValue()
        {
            string value = "";
            if (rbMill1.Checked)
                value = rbMill1.Text;
            else if (rbMill2.Checked)
                value = rbMill2.Text;
            else if (rbMill3.Checked)
                value = rbMill3.Text;
            else if (rbMillNo.Checked)
                value = rbMillNo.Text;
            return value;
        }

        private string getCleanRadioValue()
        {
            string value = "";
            if (rbCleanStone.Checked)
                value = rbCleanStone.Text;
            else if (rbCleanWater.Checked)
                value = rbCleanWater.Text;
            else if (rbCleanNo.Checked)
                value = rbCleanNo.Text;
            return value;
        }
        private string getPayRadioValue()
        {
            string value = "";
            if (rbCash.Checked)
                value = rbCash.Text;
            else if (rbCredit.Checked)
                value = rbCredit.Text;
            if (rbTrans.Checked)
                value = rbTrans.Text;
            return value;
        }

        private string getVatRadioValue()
        {
            string value = null;
            if (rbbNonVat.Checked)
                value = rbbNonVat.Text;
            else if (rbbVat.Checked)
                value = rbbVat.Text;
            return value;
        }

        private string getVatRadioValuePrint()
        {
            string value = null;
            if (rbbNonVat.Checked)
            {
                value = "ใบส่งของ";
                Company.CompanyName = " ";
                Company.Address = " ";
                Company.Email = " ";
                Company.Telephone = " ";
                Company.TTelephone = " ";
                Company.TEmail = " ";
            }
            else if (rbbVat.Checked)
            {
                value = "ใบส่งสินค้า";
                getDefaultCompany();
            }
            return value;
        }


        private void getDefaultCompany()
        {
            //sql find company
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public.base_company where base_company_id = 1 ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    Company.CompanyName = reader["company_name"].ToString();
                    Company.Address = reader["address"].ToString();
                    Company.Telephone = reader["telephone"].ToString();
                    Company.Email = reader["email"].ToString();
                }
            }
            catch (Exception)
            {
            }
            dl.close();

        }


        /* autoComplete Setting */
        private void autoCompleteSetting(TextBox tb, string field, string tableName)
        {
            tb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            tb.AutoCompleteSource = AutoCompleteSource.CustomSource;
            AutoCompleteStringCollection coll = new AutoCompleteStringCollection();

            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public." + tableName;
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string rdStr = reader[field].ToString().Trim();
                    if (!string.IsNullOrEmpty(rdStr))
                    {
                        coll.Add(rdStr);
                    }
                }
            }
            catch (Exception)
            {
            }
            tb.AutoCompleteCustomSource = coll;
            dl.close();
        }

        /* autoComplete Setting */
        private void autoCompleteSettingCompany(TextBox tb, string field, string tableName)
        {
            tb.AutoCompleteMode = AutoCompleteMode.Suggest;
            tb.AutoCompleteSource = AutoCompleteSource.CustomSource;
            AutoCompleteStringCollection coll = new AutoCompleteStringCollection();

            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public." + tableName + " where company = '" + Company.Code + "' ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string rdStr = reader[field].ToString().Trim();
                    if (!string.IsNullOrEmpty(rdStr))
                    {
                        coll.Add(rdStr);
                    }
                }
            }
            catch (Exception)
            {
            }
            tb.AutoCompleteCustomSource = coll;
            dl.close();
        }

        /* autoComplete Setting Weight Type*/
        private void autoCompleteSettingWeightType(TextBox tb, string field, string tableName)
        {
            tb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            tb.AutoCompleteSource = AutoCompleteSource.CustomSource;
            AutoCompleteStringCollection coll = new AutoCompleteStringCollection();

            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public." + tableName + " WHERE weight_type = 1 or weight_type = 3 ";

            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string rdStr = reader[field].ToString().Trim();
                    if (!string.IsNullOrEmpty(rdStr))
                    {
                        coll.Add(rdStr);
                    }
                }
            }
            catch (Exception)
            {
            }
            tb.AutoCompleteCustomSource = coll;
            dl.close();
        }

        /*3 search anywhere customer */
        public void setautoCompleteCustomer(string fieldId, string fieldName, string tableName)
        {
            cbbCustomerName.Items.Clear();
            listOriginalCustomerName.Clear();

            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            //old -- pgCommand.CommandText = "SELECT " + fieldName + " , " + fieldId + " FROM public." + tableName + " WHERE base_job_type_id IS NOT NULL AND base_vat_type_id  IS NOT NULL ORDER BY " + fieldId;
            //20-09 not show inactive not confirm
            pgCommand.CommandText = "SELECT " + fieldName + " , " + fieldId + " FROM public." + tableName + " WHERE weight_type = 1 or weight_type = 3 ORDER BY " + fieldId;
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string rdStr = reader[fieldId].ToString() + " : " + reader[fieldName].ToString();
                    listOriginalCustomerName.Add(rdStr);

                }
            }
            catch (Exception)
            {
            }

            dl.close();


            if (tbDoId.Text == "")
                cbbCustomerName.Items.AddRange(listOriginalCustomerName.ToArray());
        }

        private void tbCustomerName_TextChanged(object sender, EventArgs e)
        {
            //customerNameTextChanged();
        }

        private void customerNameTextChanged()
        {
            try
            {
                tbCustomerId.Text = cbbCustomerName.Text.Substring(0, cbbCustomerName.Text.IndexOf(" : "));

                int start = cbbCustomerName.Text.IndexOf(" : ") + 3;
                int end = cbbCustomerName.Text.Length - 11;
                tbCustomerName.Text = cbbCustomerName.Text.Substring(start, end);

                Weight.CustomerAddress = getPrintFromDB("base_customer", "ที่อยู่", "รหัสลูกค้า", tbCustomerId.Text);

                Boolean isWrong = checkInputWrong(tbCustomerName, "ชื่อลูกค้า", "base_customer", tbCustomerId, "รหัสลูกค้า");
                if (isWrong)
                    cbbCustomerName.Text = "";
            }
            catch (Exception ex)
            {
                tbCustomerId.Text = "";
                tbCustomerName.Text = "";
                cbbCustomerName.Text = "";
            }

            /*
            Boolean isWrongId = checkInputWrong(tbCustomerId, "รหัสลูกค้า", "base_customer", tbCustomerName);
            Boolean isWrongName = checkInputWrong(tbCustomerName, "ชื่อลูกค้า", "base_customer", tbCustomerId);

            if (isWrongId || isWrongName)
               cbbCustomerName.Text = "";
            */


            /*
            if (cbbCustomerName.Text != null && cbbCustomerName.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_customer where ชื่อลูกค้า = '" + cbbCustomerName.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["รหัสลูกค้า"].ToString();
                        tbCustomerId.Text = rdStr;
                    }

                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        MessageBox.Show("ไม่มีชื่อลูกค้า " + cbbCustomerName.Text, "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        tbCustomerId.Text = "";
                        cbbCustomerName.Text = "";
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
                Weight.CustomerAddress = getPrintFromDB("base_customer", "ที่อยู่", "รหัสลูกค้า", tbCustomerId.Text);
            }
            else
            {
                tbCustomerId.Text = "";
                Weight.CustomerAddress = " ";
            }
            */
        }
        private Boolean checkInputWrong(TextBox tb, string field, string table, TextBox tbsecond, string fieldSecond)
        {
            Boolean isWrong = false;
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT " + field + " FROM public." + table + " where " + field + " = '" + tb.Text + "' AND " + fieldSecond + " = '" + tbsecond.Text + "' ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();

                //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                if (!reader.HasRows)
                {
                    //MessageBox.Show("ไม่มี " + tb.AccessibleName + " นี้ กรุณากรอกข้อมูลใหม่", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    isWrong = true;
                    tb.Text = "";
                    tbsecond.Text = "";
                }
            }
            catch (Exception)
            {
            }
            dl.close();

            return isWrong;
        }

        private void tbCustomerId_TextChanged(object sender, EventArgs e)
        {
            //customerIdTextChanged();

            //หาการล้าง,สเปรย์จากลูกค้าและชนิดหิน
            setDataCleanByCustomerAndStoneType();
        }

        private void customerIdTextChanged()
        {

            if (tbCustomerId != null && tbCustomerId.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_customer where รหัสลูกค้า = '" + tbCustomerId.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["ชื่อลูกค้า"].ToString();
                        cbbCustomerName.Text = rdStr;
                    }

                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        MessageBox.Show("ไม่มีรหัสลูกค้า " + tbCustomerId.Text, "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        tbCustomerId.Text = "";
                        cbbCustomerName.Text = "";
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
                Weight.CustomerAddress = getPrintFromDB("base_customer", "ที่อยู่", "รหัสลูกค้า", tbCustomerId.Text);
            }
            else
            {
                cbbCustomerName.Text = "";
                Weight.CustomerAddress = " ";
            }
        }


        private void tbScaleId_TextChanged(object sender, EventArgs e)
        {
            if (tbScaleId != null && tbScaleId.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.users where username = '" + tbScaleId.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["firstname"].ToString();
                        tbScaleName.Text = rdStr;
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
            else
            {
                tbScaleName.Text = "";
            }
        }

        private void tbScaleName_TextChanged(object sender, EventArgs e)
        {
            if (tbScaleName != null && tbScaleName.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.users where firstname = '" + tbScaleName.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["username"].ToString();
                        tbScaleId.Text = rdStr;
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
            else
            {
                tbScaleId.Text = "";
            }

        }

        private void tbPricePerTon_TextChanged(object sender, EventArgs e)
        {
            //calculateAmount();
            calculateVat();
        }
        private void tbWeightTotal_TextChanged(object sender, EventArgs e)
        {
            //calculateAmount();
            calculateVat();
            if (cbbStoneType.SelectedIndex != -1)
                calculatenumQ();
        }

        //ไม่ใช้แล้ว 03-09-2024 เนื่องจากมีการคำนวน vat (รวมภาษี) แบบใหม่
        private void calculateAmount()
        {
            try
            {
                double total = 0;
                total = Convert.ToDouble(tbWeightTotal.Text);
                double price = 0;
                price = Convert.ToDouble(tbPricePerTon.Text);
                double amount = 0;
                amount = (total / 1000) * price;
                tbAmountVat.Text = amount.ToString("#,##0.00");

                //set Temp
                tbAmount.Text = tbAmountVat.Text;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
        }

        private double getAmount()
        {
            double amount = 0;
            try
            {
                double total = 0;
                total = Convert.ToDouble(tbWeightTotal.Text);
                double price = 0;
                price = Convert.ToDouble(tbPricePerTon.Text);
                amount = (total / 1000) * price;
                tbAmount.Text = amount.ToString("#,##0.00");
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            return amount;
        }

        private void calculatenumQ()
        {
            try
            {
                if (!checkZeroStr(tbWeightIn.Text) && !checkZeroStr(tbWeightOut.Text) && !string.IsNullOrEmpty(strCalQ))
                {
                    double numCalQ = Convert.ToDouble(strCalQ);
                    double numWeightTotal = Convert.ToDouble(tbWeightTotal.Text);
                    double numQ = numWeightTotal / (numCalQ * 1000);
                    tbQ.Text = numQ.ToString("#,##0.00");
                }
                else
                {
                    tbQ.Text = "0.00";
                }

            }
            catch (Exception e)
            {

            }
        }

        private async void btPrintIn_Click(object sender, EventArgs e)
        {
            if (!checkHistoricalDateConstraint())
            {
                return;
            }

            //เช็คค่าว่าง
            showErrorWeightInEmty();

            //ปริ้น
            preparePrint(1);

            if (checkDuplicateRunningNumber() && tbId.Text == "")
            {
                //ไม่ต้องทำไร
            }
            else
            {
                //save อัตโนมัติ
                if (await autoSave())
                {
                    if (chkDirectPrint.Checked)
                    {
                        DirectPrintReportMain();
                    }
                    else
                    {
                        FPrint f = new FPrint();
                        f.ShowDialog();
                    }
                }
            }

        }

        private void preparePrint(int mode)
        {
            Company.TTelephone = "โทร";
            Company.TEmail = "E-mail";
            Weight.Date = dtDate.Text;
            Weight.DocNum = tbDocNum.Text;
            Weight.Mill = strNotEmty(cbbMill.Text);
            Weight.DriverName = strNotEmty(tbDriverName.Text);
            Weight.CustomerName = strNotEmty(tbCustomerName.Text);
            Weight.CustomerAddress = getPrintFromDB("base_customer", "ที่อยู่", "รหัสลูกค้า", tbCustomerId.Text);
            Weight.StoneType = strNotEmty(cbbStoneType.Text);
            Weight.StoneDesc = strNotEmty(tbStoneDesc.Text);
            Weight.CarLicense = strNotEmty(tbCarLicense.Text);
            Weight.CarCity = strNotEmty(tbCarCity.Text);
            Weight.DateIn = strNotEmty(dtWeightInDate.Text);
            Weight.TimeIn = strNotEmty(dtWeightInTime.Text);
            Weight.DateOut = strNotEmty(dtWeightOutDate.Text);
            Weight.TimeOut = strNotEmty(dtWeightOutTime.Text);
            Weight.WeightIn = kgToTon(tbWeightIn);
            Weight.WeightOut = kgToTon(tbWeightOut);
            Weight.WeightTotal = kgToTon(tbWeightTotal);
            Weight.Price = tbPricePerTon.Text;
            Weight.Amount = tbAmount.Text;
            Weight.Vat = tbVat.Text;
            Weight.AmountVat = tbAmountVat.Text;
            Weight.Q = tbQ.Text;
            Weight.Team = strNotEmty(cbbCarTeam.Text);
            Weight.StoneColor = strNotEmty(cbbStoneColor.Text);
            Weight.Site = strNotEmty(cbbSite.Text);
            Weight.ApproveName = strNotEmty(tbApproveName.Text);
            Weight.Pay = strNotEmty(getPayRadioValue());
            Weight.VatType = getVatRadioValuePrint();
            Weight.Clean = strNotEmty(getCleanRadioValue());
            Weight.Transport = strNotEmty(cbbTransport.Text);
            Weight.OilContent = zeroNotEmty(tbOilContent.Text);
            Weight.Id = tbId.Text;
            Weight.DoId = tbDoId.Text;


            if (mode.Equals(3))
            {
                //ปริ้นทั้ง IN และ OUT
                Company.TDocName = "เลขที่การชั่ง";
                Company.TLogo = "(Sandvik)";
            }
            else if (mode.Equals(2))
            {
                //ปริ้น OUT
                Weight.Pay = " ";
                Weight.DocNum = " ";
                Weight.DateIn = " ";
                Weight.TimeIn = " ";
                Weight.WeightIn = " ";
                Weight.CustomerName = " ";
                Weight.CustomerAddress = " ";
                Weight.Site = " ";
                Weight.StoneType = " ";
                Weight.StoneDesc = " ";
                Weight.CarLicense = " ";
                Weight.CarCity = " ";
                Weight.DriverName = " ";
                Weight.Team = " ";
                Weight.Transport = " ";
                Company.TDocName = " ";
                Company.TLogo = " ";
            }
            else if (mode.Equals(1))
            {
                //ปริ้น IN
                Weight.Mill = " ";
                Weight.StoneColor = " ";
                Weight.Clean = " ";
                Weight.ApproveName = " ";
                Weight.DateOut = " ";
                Weight.TimeOut = " ";
                Weight.WeightOut = " ";
                Weight.WeightTotal = " ";
                Weight.Q = " ";
                Weight.Price = " ";
                Weight.Amount = " ";
                Weight.Vat = " ";
                Weight.AmountVat = " ";
                Weight.OilContent = " ";
                Company.TDocName = "เลขที่การชั่ง";
                Company.TLogo = "(Sandvik)";
                Weight.DatePrintAndCopyNum = " ";

            }

        }


        public string CheckText(string text)
        {
            string doIdValue = string.IsNullOrWhiteSpace(text) ? "NULL" : text.Trim();
            return doIdValue;

        }
        private string strNotEmty(string str)
        {
            return str == "" ? " " : str;
        }

        private string zeroNotEmty(string str)
        {
            return str == "0.00" || str == "0" ? " " : str + " (L)";
        }

        private string getPrintFromDB(string database, string field, string fieldCondition, string condition)
        {
            //sql
            string rdStr = " ";
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT " + field + " FROM public." + database + " where " + fieldCondition + " = '" + condition + "' ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    rdStr = reader[field].ToString();
                }
            }
            catch (Exception)
            {
            }
            dl.close();


            if (rdStr == null || rdStr == "")
            {
                rdStr = " ";
            }

            return rdStr;

        }

        private string kgToTon(TextBox tb)
        {
            double tmp = Convert.ToDouble(tb.Text);
            double deci = tmp / 1000;
            string str = string.Format("{0:0.000}", deci);
            return str;
        }

        private void btLoadCustomer_Click(object sender, EventArgs e)
        {
            TableCustomer tc = new TableCustomer(this);
            tc.ShowDialog();
        }

        private void cbbStoneType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT ค่าคำนวณคิว FROM public.base_stone_type where ชื่อหิน = '" + cbbStoneType.Text + "' ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string rdStr = reader["ค่าคำนวณคิว"].ToString();
                    strCalQ = rdStr;

                }
            }
            catch (Exception)
            {
            }
            dl.close();

            //Weight.StoneColor = getPrintFromDB("base_stone_type", "ประเภทหิน", "ชื่อหิน", cbbStoneType.Text);
            //คำนวณค่าคิว
            calculatenumQ();

            //หาการล้าง,สเปรย์จากลูกค้าและชนิดหิน
            setDataCleanByCustomerAndStoneType();
        }
        private void textboxFormatDecimal(object sender, KeyPressEventArgs e, TextBox textBox)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if (e.KeyChar == '.' && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }

            if (!char.IsControl(e.KeyChar))
            {

                textBox = (TextBox)sender;

                if (textBox.Text.IndexOf('.') > -1 &&
                         textBox.Text.Substring(textBox.Text.IndexOf('.')).Length >= 3)
                {
                    e.Handled = true;
                }

            }

        }

        private void tbPricePerTon_Leave(object sender, EventArgs e)
        {
            convertFormatToDecimal(tbPricePerTon);
        }

        private void rbbNonVat_CheckedChanged(object sender, EventArgs e)
        {
            if (rbbNonVat.Checked)
            {
                try
                {
                    double tempAmount = getAmount();
                    double vat = tempAmount - (tempAmount / 107) * 100;
                    tbVat.Text = vat.ToString("#,##0.00");
                    double total = tempAmount - vat;
                    tbAmount.Text = total.ToString("#,##0.00");
                    tbAmountVat.Text = tempAmount.ToString("#,##0.00");
                }
                catch (Exception ec)
                {
                }
            }
        }

        private void rbbVat_CheckedChanged(object sender, EventArgs e)
        {
            if (rbbVat.Checked)
            {
                try
                {
                    double tempAmount = getAmount();
                    double vat = (tempAmount * 7.0) / 100;
                    tbVat.Text = vat.ToString("#,##0.00");
                    double total = tempAmount + vat;
                    tbAmount.Text = tempAmount.ToString("#,##0.00");
                    tbAmountVat.Text = total.ToString("#,##0.00");
                }
                catch (Exception ec)
                {

                }
            }
        }

        private void calculateVat()
        {
            double tempAmount = getAmount();
            if (rbbVat.Checked)
            {
                try
                {
                    double vat = (tempAmount * 7.0) / 100;
                    tbVat.Text = vat.ToString("#,##0.00");
                    double total = tempAmount + vat;
                    tbAmount.Text = tempAmount.ToString("#,##0.00");
                    tbAmountVat.Text = total.ToString("#,##0.00");
                }
                catch (Exception ec)
                {
                }
            }
            else if (rbbNonVat.Checked)
            {
                try
                {
                    double vat = tempAmount - (tempAmount / 107) * 100;
                    tbVat.Text = vat.ToString("#,##0.00");
                    double total = tempAmount - vat;
                    tbAmount.Text = total.ToString("#,##0.00");
                    tbAmountVat.Text = tempAmount.ToString("#,##0.00");
                }
                catch (Exception ec)
                {
                }
            }
        }

        private void tbApproveId_TextChanged(object sender, EventArgs e)
        {
            if (tbApproveId != null && tbApproveId.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_approve where รหัสผู้อนุมัติจ่าย = '" + tbApproveId.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["ชื่อผู้อนุมัติจ่าย"].ToString();
                        tbApproveName.Text = rdStr;
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
            else
            {
                tbApproveName.Text = "";
            }

        }

        private void tbApproveName_TextChanged(object sender, EventArgs e)
        {
            if (tbApproveName != null && tbApproveName.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_approve where ชื่อผู้อนุมัติจ่าย = '" + tbApproveName.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["รหัสผู้อนุมัติจ่าย"].ToString();
                        tbApproveId.Text = rdStr;
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
            else
            {
                tbApproveId.Text = "";
            }
        }

        private void label26_Click(object sender, EventArgs e)
        {

        }
        private void convertFormatToDecimal(TextBox tb)
        {
            try
            {
                double d = Convert.ToDouble(tb.Text);
                tb.Text = d.ToString("#,##0.00");
            }
            catch (Exception ex)
            {
                MessageBox.Show("ชนิดของข้อมูลผิด กรุณากรอกข้อมูลใหม่", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tb.Text = "0.00";
            }
        }

        private void tbWeightTotal_Leave(object sender, EventArgs e)
        {
            convertFormatToDecimal(tbWeightTotal);
        }

        private void tbAmount_Leave(object sender, EventArgs e)
        {
            convertFormatToDecimal(tbAmount);
        }

        private void tbVat_Leave(object sender, EventArgs e)
        {
            convertFormatToDecimal(tbVat);
        }

        private void tbAmountVat_Leave(object sender, EventArgs e)
        {
            convertFormatToDecimal(tbAmountVat);
        }

        private void tbQ_Leave(object sender, EventArgs e)
        {
            convertFormatToDecimal(tbQ);
        }

        private void tbWeightOut_TextChanged(object sender, EventArgs e)
        {
            calculateWeight();
        }

        private void tbWeightIn_TextChanged(object sender, EventArgs e)
        {
            calculateWeight();
        }

        private void tbCarLicense_TextChanged(object sender, EventArgs e)
        {
            fillCarTeamCombo();

            /*
            if (tbCarLicense != null && tbCarLicense.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                //pgCommand.CommandText = "SELECT รหัสทีม FROM public.base_car where ชื่อรถร่วม = '" + tbCarLicense.Text + "' ";
                pgCommand.CommandText = "SELECT base_car_team.ชื่อทีม FROM base_car INNER JOIN base_car_team ON base_car.รหัสทีม = base_car_team.รหัสทีม WHERE base_car.ชื่อรถร่วม = '" + tbCarLicense.Text + "' ";
                try
                {
                    collCarTeam.Clear();
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["ชื่อทีม"].ToString();
                        tbCarTeam.Text = rdStr;
                        collCarTeam.Add(rdStr);
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
            else
            {
                tbCarTeam.Text = "";
            }
            */

        }

        private void tbCarTeam_Click(object sender, EventArgs e)
        {
            tbCarTeam.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            tbCarTeam.AutoCompleteSource = AutoCompleteSource.CustomSource;
            tbCarTeam.AutoCompleteCustomSource = collCarTeam;
        }

        private void timerWeight_Tick(object sender, EventArgs e)
        {
            tbWeigtData.Text = tbWeigtData.Text;
        }

        private Boolean checkCancelAction()
        {
            if (tbCustomerId.Text == "09-A-001" || tbCustomerId.Text == "09-V-001")
            {
                using (var form = new FCancelPassword())
                {
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        string password = form.ReturnPassword;
                        if (password == "pdg]bd=yj'")
                        {
                            tbWeightIn.Text = "0.00";
                            tbWeightOut.Text = "0.00";
                            tbWeightTotal.Text = "0.00";
                            tbPricePerTon.Text = "0.00";
                            tbAmount.Text = "0.00";
                            tbAmountVat.Text = "0.00";
                            tbVat.Text = "0.00";
                            tbQ.Text = "0.00";
                            tbDoId.Text = "";
                            tbDoDocNo.Text = "";
                            tbOldDoId.Text = "";
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void checkResetWeight()
        {
            if (tbCustomerId.Text == "09-A-001" || tbCustomerId.Text == "09-V-001")
            {
                tbWeightIn.Text = "0.00";
                tbWeightOut.Text = "0.00";
                tbWeightTotal.Text = "0.00";
                tbPricePerTon.Text = "0.00";
                tbAmount.Text = "0.00";
                tbAmountVat.Text = "0.00";
                tbVat.Text = "0.00";
            }
        }

        private void tbCustomerId_Leave(object sender, EventArgs e)
        {
            /*
            checkResetWeight();
            customerIdTextChanged();
            */
        }

        private void tbCustomerName_Leave(object sender, EventArgs e)
        {
            checkResetWeight();
            customerNameTextChanged();
        }

        private void rbMill1_MouseClick(object sender, MouseEventArgs e)
        {
        }

        private void rbMill1_Click(object sender, EventArgs e)
        {

            RadioButton radio = (RadioButton)sender;
            if (radio.Checked)
            {
                radio.Checked = false;
            }


        }

        private void rbCash_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            isCheckedCash = radio.Checked;
        }

        private void rbCash_Click(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            if (radio.Checked && !isCheckedCash)
                radio.Checked = false;
            else
            {
                radio.Checked = true;
                isCheckedCash = false;
            }
        }

        private void rbTrans_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            isCheckedTrans = radio.Checked;
        }

        private void rbTrans_Click(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            if (radio.Checked && !isCheckedTrans)
                radio.Checked = false;
            else
            {
                radio.Checked = true;
                isCheckedTrans = false;
            }
        }

        private void rbCredit_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            isCheckedCredit = radio.Checked;
        }

        private void rbCredit_Click(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            if (radio.Checked && !isCheckedCredit)
                radio.Checked = false;
            else
            {
                radio.Checked = true;
                isCheckedCredit = false;
            }
        }

        private void rbMill1_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            isCheckedMill1 = radio.Checked;
        }

        private void rbMill1_Click_1(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            if (radio.Checked && !isCheckedMill1)
                radio.Checked = false;
            else
            {
                radio.Checked = true;
                isCheckedMill1 = false;
            }
        }

        private void rbMill2_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            isCheckedMill2 = radio.Checked;
        }

        private void rbMill2_Click(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            if (radio.Checked && !isCheckedMill2)
                radio.Checked = false;
            else
            {
                radio.Checked = true;
                isCheckedMill2 = false;
            }
        }

        private void rbMill3_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            isCheckedMill3 = radio.Checked;
        }

        private void rbMill3_Click(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            if (radio.Checked && !isCheckedMill3)
                radio.Checked = false;
            else
            {
                radio.Checked = true;
                isCheckedMill3 = false;
            }
        }

        private void rbCleanStone_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            isCheckedCleanStone = radio.Checked;
        }

        private void rbCleanStone_Click(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            if (radio.Checked && !isCheckedCleanStone)
                radio.Checked = false;
            else
            {
                radio.Checked = true;
                isCheckedCleanStone = false;
            }
        }

        private void rbCleanWater_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            isCheckedCleanWater = radio.Checked;
        }

        private void rbCleanWater_Click(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            if (radio.Checked && !isCheckedCleanWater)
                radio.Checked = false;
            else
            {
                radio.Checked = true;
                isCheckedCleanWater = false;
            }
        }

        private async void btPrintOut_Click(object sender, EventArgs e)
        {
            if (!checkHistoricalDateConstraint())
            {
                return;
            }

            //เช็คค่าว่าง
            showErrorWeightOutEmty();

            //ปริ้น
            preparePrint(2);
            if (checkDuplicateRunningNumber() && tbId.Text == "")
            {
                //ไม่ต้องทำไร
            }
            else
            {
                //save อัตโนมัติ
                if (await autoSave())
                {
                    HandleSuccessfulPrint();
                    //Print
                    if (chkDirectPrint.Checked)
                    {
                        DirectPrintReportMain();
                    }
                    else
                    {
                        FPrint f = new FPrint();
                        f.ShowDialog();
                    }
                }
            }

        }

        private async void btPrintAll_Click(object sender, EventArgs e)
        {
            if (!checkHistoricalDateConstraint())
            {
                return;
            }

            //เช็คค่าว่าง
            showErrorWeightInEmty();
            showErrorWeightOutEmty();

            //ปริ้น
            preparePrint(3);
            if (checkDuplicateRunningNumber() && tbId.Text == "")
            {
                //ไม่ต้องทำไร
            }
            else
            {
                //save อัตโนมัติ
                if (await autoSave())
                {
                    HandleSuccessfulPrint();
                    //Print
                    if (chkDirectPrint.Checked)
                    {
                        DirectPrintReportMain();
                    }
                    else
                    {
                        FPrint f = new FPrint();
                        f.ShowDialog();
                    }
                }
            }
        }

        private void HandleSuccessfulPrint()
        {
            int copy_num = findLastCopyByWeightId();
            copy_num++;

            Weight.DatePrint = DateTime.Now.ToString("yyyy-MM-dd");
            Weight.DatePrintAndCopyNum = DateTime.Now.ToString("dd/MM") + "#" + copy_num;
            Weight.TimePrint = DateTime.Now.ToString("HH:mm:ss");

            //save weight copy
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "INSERT INTO weight_copy (copy_num, date_print, time_print, user_print, weight_id )" +
                                     "VALUES ('" + copy_num + "','" + Weight.DatePrint + "','" + Weight.TimePrint + "','" + Globals.UserId + "','" + tbId.Text + "' )";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            dl.close();
        }

        private void DirectPrintReportMain()
        {
            try
            {
                using (LocalReport report = new LocalReport())
                {
                    report.ReportEmbeddedResource = "SerialPortListener.ReportMain.rdlc";

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

                    report.SetParameters(p);

                    using (ReportPrintHelper printer = new ReportPrintHelper())
                    {
                        // 8.27 x 11.69 inches is A4, margins in inches: Left=0.46, Right=0.46, Top=0.60, Bottom=0.30
                        printer.Export(report, 8.27, 11.69, 0.46, 0.46, 0.60, 0.30);
                        printer.Print();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาดในการพิมพ์: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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


        private int findLastCopyByWeightId()
        {
            int copy_num = 0;


            if (tbId.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "select copy_num from weight_copy where weight_id = '" + tbId.Text + "' ORDER BY weight_copy_id DESC LIMIT 1";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    if (reader.Read())
                    {
                        copy_num = Convert.ToInt32(reader["copy_num"].ToString());
                    }
                    else
                    {
                        copy_num = 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                dl.close();
            }

            return copy_num;
        }

        private void rbMillNo_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            isCheckedMillNo = radio.Checked;
        }

        private void rbMillNo_Click(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            if (radio.Checked && !isCheckedMillNo)
                radio.Checked = false;
            else
            {
                radio.Checked = true;
                isCheckedMillNo = false;
            }
        }

        private void rbCleanNo_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            isCheckedCleanNo = radio.Checked;
        }

        private void rbCleanNo_Click(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            if (radio.Checked && !isCheckedCleanNo)
                radio.Checked = false;
            else
            {
                radio.Checked = true;
                isCheckedCleanNo = false;
            }
        }

        private void showErrorEmtyTextBox(TextBox tb)
        {
            if (string.IsNullOrEmpty(tb.Text) || tb.Text == "0.00")
                MessageBox.Show("' " + tb.AccessibleName + "' เป็นค่าว่าง กรุณาใส่ข้อมูลให้ครบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void showErrorEmtyComboBox(ComboBox cbb)
        {
            if (string.IsNullOrEmpty(cbb.Text))
                MessageBox.Show("' " + cbb.AccessibleName + "' เป็นค่าว่าง กรุณาใส่ข้อมูลให้ครบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void showErrorEmtyRadioButton(GroupBox gb)
        {
            var rd = gb.Controls.OfType<RadioButton>()
                    .FirstOrDefault(n => n.Checked);
            if (rd == null)
                MessageBox.Show("' " + gb.AccessibleName + "' เป็นค่าว่าง กรุณาใส่ข้อมูลให้ครบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void showErrorWeightInEmty()
        {
            showErrorEmtyRadioButton(groupBox2);
            showErrorEmtyComboBox(cbbStoneType);
            showErrorEmtyComboBox(cbbTransport);
            showErrorEmtyTextBox(tbCarLicense);
            showErrorEmtyTextBox(tbCarCity);
            showErrorEmtyTextBox(tbWeightIn);
        }

        private void showErrorWeightOutEmty()
        {
            showErrorEmtyRadioButton(groupBox2);
            showErrorEmtyTextBox(tbScoopId);
            showErrorEmtyTextBox(tbScoopName);
            //showErrorEmtyRadioButton(groupBox1);
            //showErrorEmtyComboBox(cbbMill);
            showErrorEmtyComboBox(cbbMill);
            showErrorEmtyRadioButton(groupBox4);
            showErrorEmtyTextBox(tbQ);

        }

        /*4 search anywhere customer */
        private void cbbCustomerName_TextUpdate(object sender, EventArgs e)
        {
            setSearchAnywhereToCombobox(cbbCustomerName, listOriginalCustomerName, listNewCustomerName);
        }

        /*5 search anywhere customer */
        private void setSearchAnywhereToCombobox(ComboBox cb, List<string> listOriginal, List<string> listNew)
        {


            if (tbDoId.Text == "")
            {
                try
                {
                    //clear combobox
                    cb.Items.Clear();
                    //clear listNew
                    listNew.Clear();

                    foreach (var item in listOriginal)
                    {
                        // call ToLower() .. not case sensitive
                        if (item.ToLower().Contains(cb.Text))
                        {
                            //add to ListNew
                            listNew.Add(item);
                        }
                    }

                    if (listNew.Count > 0)
                    {
                        cb.Items.AddRange(listNew.ToArray());
                        cb.SelectionStart = cb.Text.Length;
                        Cursor = Cursors.Default;
                        // Automatically pop up drop-down
                        cb.DroppedDown = true;
                    }
                    else
                    {

                        cb.Items.AddRange(listOriginal.ToArray());
                        cb.DroppedDown = false;
                    }


                }
                catch (Exception)
                {
                }

            }

        }

        private void cbbCustomerName_Leave(object sender, EventArgs e)
        {
            checkResetWeight();
            customerNameTextChanged();
            fillSiteCombo();
        }

        private void fillSiteCombo()
        {
            //ล้างก่อน
            cbbSite.Items.Clear();
            //เพิ่ม combobox
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT base_site.base_site_id, base_site.base_site_name FROM public.base_customer_site INNER JOIN public.base_site ON base_customer_site.site_id = base_site.base_site_id where customer_id = '" + tbCustomerId.Text + "'";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string id = reader["base_site_id"].ToString();
                    string des = reader["base_site_name"].ToString();
                    //cbbSite.Items.Add(des);
                    cbbSite.Items.Add(new ComboboxValue(id, des));
                }
            }
            catch (Exception)
            {

            }
            dl.close();
        }

        private void fillCarTeamCombo()
        {

            //ล้างก่อน
            cbbCarTeam.Items.Clear();

            //เพิ่ม combobox
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT base_car_team.รหัสทีม , base_car_team.ชื่อทีม FROM base_car INNER JOIN base_car_team ON base_car.รหัสทีม = base_car_team.รหัสทีม WHERE base_car.ชื่อรถร่วม = '" + tbCarLicense.Text + "' order by base_car_team.รหัสทีม";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string id = reader["รหัสทีม"].ToString();
                    string des = reader["ชื่อทีม"].ToString();
                    //cbbSite.Items.Add(des);
                    cbbCarTeam.Items.Add(new ComboboxValue(id, des));
                    cbbCarTeam.SelectedIndex = 0;
                }
            }
            catch (Exception)
            {

            }
            dl.close();
            cbbCarTeam.Items.Add("");
        }

        private void cbbCustomerName_SelectedIndexChanged(object sender, EventArgs e)
        {
            cbbSite.Text = "";
        }

        private string findcarryTypeByTransport()
        {
            string carryTypeName = "";
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT base_carry_type.base_carry_type_name FROM base_carry_type INNER JOIN base_transport ON base_carry_type.base_carry_type_id = base_transport.base_carry_type_id where base_transport_name = '" + cbbTransport.Text + "'";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    carryTypeName = reader["base_carry_type_name"].ToString();
                }
            }
            catch (Exception)
            {

            }
            dl.close();

            return carryTypeName;
        }


        private string findBWS()
        {
            string code = "";
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT code FROM base_weight_station WHERE base_weight_station_id = 1";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    code = reader["code"].ToString();
                }
            }
            catch (Exception)
            {

            }
            dl.close();

            return code;
        }


        private string findValueByDO(string do_id, int mode)
        {
            string doc_no = "";
            string delivery_date = "";
            string unitName = "";
            string car_company = "";
            string car_customer = "";

            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT doc_no, delivery_date, unit_name, car_company, car_customer FROM delivery_order where do_id = '" + do_id + "'";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    doc_no = reader["doc_no"].ToString();
                    delivery_date = reader["delivery_date"].ToString();
                    unitName = reader["unit_name"].ToString();

                    car_company = reader["car_company"].ToString();
                    car_customer = reader["car_customer"].ToString();
                }
            }
            catch (Exception)
            {

            }
            dl.close();

            if (mode.Equals(1))
                return doc_no;
            else if (mode.Equals(2))
                return delivery_date;
            else if (mode.Equals(3))
                return unitName;
            else if (mode.Equals(4))
                return car_company;
            else if (mode.Equals(5))
                return car_customer;
            else
                return "";
        }

        private string getBaseApi(int mode , int base_api_id)
        {
            string url = "";
            string username = "";
            string password = "";
            string comp_code = "";
            string token = "";

            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT url, username, password, comp_code, token FROM base_api where id = " + base_api_id;
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    url = reader["url"].ToString();
                    username = reader["username"].ToString();
                    password = reader["password"].ToString();
                    comp_code = reader["comp_code"].ToString();
                    token = reader["token"].ToString();
                }
            }
            catch (Exception)
            {

            }
            dl.close();

            if (mode.Equals(1))
                return url;
            else if (mode.Equals(2))
                return username;
            else if (mode.Equals(3))
                return password;
            else if (mode.Equals(4))
                return comp_code;
            else if (mode.Equals(5))
                return token;
            else
                return "";
        }

        private void tbOilContent_Leave(object sender, EventArgs e)
        {
            convertFormatToDecimal(tbOilContent);
        }

        private void tbWeightIn_Leave(object sender, EventArgs e)
        {
            convertFormatToDecimal(tbWeightIn);
            checkNumWeightMany(tbWeightIn);
        }

        private void tbWeightOut_Leave(object sender, EventArgs e)
        {
            convertFormatToDecimal(tbWeightOut);
            checkNumWeightMany(tbWeightOut);
        }

        private void checkNumWeightMany(TextBox tb)
        {
            if (tb.Text.Length > 9)
            {
                MessageBox.Show("ช่อง " + tb.AccessibleName + "มีน้ำหนักเกิน กรุณากรอกข้อมูลใหม่", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tb.Text = "0.00";
            }

        }

        private void tbCarLicense_KeyUp(object sender, KeyEventArgs e)
        {
            findLastCityByCarLicense();
            findLastTransportByCarLicense();
        }

        private void findLastCityByCarLicense()
        {
            if (tbCarLicense != null && tbCarLicense.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                //pgCommand.CommandText = "SELECT รหัสทีม FROM public.base_car where ชื่อรถร่วม = '" + tbCarLicense.Text + "' ";
                pgCommand.CommandText = "SELECT จังหวัด from weight where ทะเบียนรถ = '" + tbCarLicense.Text + "' order by weight_id  desc LIMIT 1 ";
                try
                {
                    collCarTeam.Clear();
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["จังหวัด"].ToString();
                        tbCarCity.Text = rdStr;
                    }
                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        tbCarCity.Text = "";
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
            else
            {
                tbCarCity.Text = "";
            }
        }

        private void findLastTransportByCarLicense()
        {
            if (tbCarLicense != null && tbCarLicense.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT ขนส่ง from weight where ทะเบียนรถ = '" + tbCarLicense.Text + "' order by weight_id  desc LIMIT 1 ";
                try
                {
                    collCarTeam.Clear();
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["ขนส่ง"].ToString();
                        cbbTransport.Text = rdStr;
                    }
                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        cbbTransport.Text = "";
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
            else
            {
                cbbTransport.Text = "";
            }
        }

        private void setDataCleanByCustomerAndStoneType()
        {
            if (tbCustomerId != null && tbCustomerId.Text != "" && cbbStoneType != null && cbbStoneType.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT ล้าง from weight where รหัสลูกค้า = '" + tbCustomerId.Text + "' and ชนิดหิน = '" + cbbStoneType.Text + "' order by weight_id  desc LIMIT 1 ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["ล้าง"].ToString();
                        //set data clean
                        setDataCleanToRB(rdStr);
                    }

                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        setDataCleanToRB("ไม่มี");
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
        }

        private void LoadScoopById()
        {
            if (tbScoopId == null || tbScoopId.Text == "")
            {
                tbScoopName.Text = "";
                return;
            }

            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText =
                "SELECT รหัสผู้ตัก, ชื่อผู้ตัก FROM public.base_scoop " +
                "WHERE UPPER(TRIM(รหัสผู้ตัก)) = '" + tbScoopId.Text.Trim().ToUpper().Replace("'", "''") + "' " +
                "AND company = '" + Company.Code + "' " +
                "LIMIT 1";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                if (reader.Read())
                {
                    tbScoopId.Text = reader["รหัสผู้ตัก"].ToString().Trim();
                    tbScoopName.Text = reader["ชื่อผู้ตัก"].ToString().Trim();
                }
                else
                {
                    tbScoopId.Text = "";
                    tbScoopName.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                dl.close();
            }
        }

        private void LoadScoopByName()
        {
            if (tbScoopName == null || tbScoopName.Text == "")
            {
                tbScoopId.Text = "";
                return;
            }

            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = 
                "SELECT รหัสผู้ตัก, ชื่อผู้ตัก FROM public.base_scoop " +
                "WHERE UPPER(TRIM(ชื่อผู้ตัก)) = '" + tbScoopName.Text.Trim().ToUpper().Replace("'", "''") + "' " +
                "AND company = '" + Company.Code + "' " +
                "LIMIT 1";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                if (reader.Read())
                {
                    tbScoopId.Text = reader["รหัสผู้ตัก"].ToString().Trim();
                    tbScoopName.Text = reader["ชื่อผู้ตัก"].ToString().Trim();
                }
                else
                {
                    tbScoopId.Text = "";
                    tbScoopName.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                dl.close();
            }
        }

        private void tbScoopId_Leave(object sender, EventArgs e)
        {
            LoadScoopById();
        }

        private void tbScoopName_Leave(object sender, EventArgs e)
        {
            LoadScoopByName();
        }

        private void tbScoopId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // prevent beep
                LoadScoopById();
                this.SelectNextControl((Control)sender, true, true, true, true);
            }
        }

        private void tbScoopName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // prevent beep
                LoadScoopByName();
                this.SelectNextControl((Control)sender, true, true, true, true);
            }
        }

        private void btRefresh_Click(object sender, EventArgs e)
        {
            /* autoComplete ผู้ตัก */
            autoCompleteSettingCompany(tbScoopId, "รหัสผู้ตัก", "base_scoop");
            autoCompleteSettingCompany(tbScoopName, "ชื่อผู้ตัก", "base_scoop");

            /* autoComplete โรงโม่ */
            //autoCompleteSettingWeightType(tbMillId, "รหัสโรงโม่", "base_mill");
            //autoCompleteSettingWeightType(tbMillName, "ชื่อโรงโม่", "base_mill");

            setautoCompleteCustomer("รหัสลูกค้า", "ชื่อลูกค้า", "base_customer");

            Weight.CustomerAddress = getPrintFromDB("base_customer", "ที่อยู่", "รหัสลูกค้า", tbCustomerId.Text);

            fillStoneCombo();
            fillTransportCombo();
            fillMillCombo();
        }

        private void tbMillId_Leave(object sender, EventArgs e)
        {
            if (tbMillId != null && tbMillId.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_mill where (weight_type = 1 or weight_type = 3) and รหัสโรงโม่ = '" + tbMillId.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["ชื่อโรงโม่"].ToString();
                        tbMillName.Text = rdStr;
                    }
                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        tbMillId.Text = "";
                        tbMillName.Text = "";
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
            else
            {
                tbMillName.Text = "";
            }
        }

        private void tbMillName_Leave(object sender, EventArgs e)
        {
            if (tbMillName != null && tbMillName.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_mill where (weight_type = 1 or weight_type = 3) and ชื่อโรงโม่ = '" + tbMillName.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["รหัสโรงโม่"].ToString();
                        tbMillId.Text = rdStr;
                    }
                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        tbMillId.Text = "";
                        tbMillName.Text = "";
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
            else
            {
                tbMillId.Text = "";
            }
        }

        private void cbbStoneType_Leave(object sender, EventArgs e)
        {
            if (cbbStoneType != null && cbbStoneType.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_stone_type where inactive = false and ชื่อหิน = '" + cbbStoneType.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        cbbStoneType.Text = "";
                        MessageBox.Show("ไม่มีข้อมูลชนิดหินนี้ในระบบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
        }

        private void cbbMill_Leave(object sender, EventArgs e)
        {
            if (cbbMill != null && cbbMill.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_mill where (weight_type = 1 or weight_type = 3) and ชื่อโรงโม่ = '" + cbbMill.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        cbbMill.Text = "";
                        MessageBox.Show("ไม่มีข้อมูลต้นทางนี้ในระบบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
        }

        private void cbbSite_Leave(object sender, EventArgs e)
        {
            if (cbbSite != null && cbbSite.Text != "")
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_site where (weight_type = 1 or weight_type = 3) and base_site_name = '" + cbbSite.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        cbbSite.Text = "";
                        MessageBox.Show("ไม่มีข้อมูลปลายทางนี้ในระบบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else if (tbDoDocNo.Text != "")
                    {
                        cbbSite.Enabled = false;
                    }
                }
                catch (Exception)
                {
                }
                dl.close();
            }
        }

        private async void btLoadDO_Click(object sender, EventArgs e)
        {
            try
            {
                // =============================================
                // PHASE 1 : DOWNLOAD from BASE_URL → INSERT
                // =============================================
                bool downloadSuccess = await DownloadAndInsertDeliveryOrders();

                if (!downloadSuccess)
                {
                    MessageBox.Show(
                        "ไม่สามารถดาวน์โหลดข้อมูลจาก BASE_URL ได้",
                        "Download Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                // =============================================
                // PHASE 2 : UPDATE summary via JWT API
                // =============================================
                var apiResult = await UpdateDeliveryOrderFromApi();

                if (!apiResult.IsSuccess)
                {
                    if (apiResult.IsValidationError)
                    {
                        MessageBox.Show(
                            "ไม่สามารถอัปเดตข้อมูล Delivery Order ได้เนื่องจากข้อมูลไม่ถูกต้องตามเงื่อนไข (422 Unprocessable Entity)",
                            "Validation Error (422)",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                    }
                    else
                    {
                        MessageBox.Show(
                            "ไม่สามารถเชื่อมต่อ API ได้ กรุณาเชื่อมต่อ Internet!!!",
                            "API Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                    return;
                }

                // =============================================
                // PHASE 3 : OPEN WEBAPP FORM
                // =============================================
                TableDeliveryOrder td = new TableDeliveryOrder(this);
                td.ShowDialog();
            }
            catch (Exception ex)
            {
                /*
                MessageBox.Show(
                    ex.ToString(),
                    "ERROR",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                */
            }
            finally
            {
                await updateStatusCancelDO();
            }
        }

        private async Task updateStatusCancelDO()
        {
            string baseUrl = getBaseApi(1, 1);
            string username = getBaseApi(2, 1);
            string password = getBaseApi(3, 1);
            string comp_code = getBaseApi(4, 1);
            string apiUrl = $"{baseUrl}/api/uc_status_cancel_do/";

            List<CancelDeliveryOrder> cancelOrders = new List<CancelDeliveryOrder>();

            try
            {
                dl.connect();
                using (OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand())
                {
                    pgCommand.CommandText = @"
                        SELECT doc_no, delivery_date, status
                        FROM delivery_order 
                        WHERE delivery_date = '" + dtDate.Text + "' and status = 'cancel'";

                    using (OdbcDataReader reader = pgCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string formattedDate = "";
                            if (reader["delivery_date"] != DBNull.Value)
                            {
                                var dbVal = reader["delivery_date"];
                                if (dbVal is DateTime dtVal)
                                {
                                    formattedDate = dtVal.ToString("yyyy-MM-dd");
                                }
                                else
                                {
                                    string rawDate = dbVal.ToString();
                                    if (DateTime.TryParse(rawDate, out DateTime parsedDate))
                                    {
                                        formattedDate = parsedDate.ToString("yyyy-MM-dd");
                                    }
                                    else
                                    {
                                        formattedDate = rawDate;
                                    }
                                }
                            }

                            var order = new CancelDeliveryOrder
                            {
                                doc_no = reader["doc_no"] != DBNull.Value ? reader["doc_no"].ToString() : "",
                                delivery_date = formattedDate,
                                status = reader["status"] != DBNull.Value ? reader["status"].ToString() : "",
                                comp_code = comp_code
                            };
                            cancelOrders.Add(order);
                            string orderJson = JsonConvert.SerializeObject(order);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB Error in updateStatusCancelDO: " + ex.ToString());
                System.Diagnostics.Debug.WriteLine("DB Error in updateStatusCancelDO: " + ex.ToString());
            }
            finally
            {
                dl.close();
            }

            if (cancelOrders.Count == 0)
            {
                return;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    string accessToken = await GetJwtToken(client, baseUrl, username, password);

                    if (accessToken == null)
                        return;

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", accessToken);

                    string apiJson = JsonConvert.SerializeObject(cancelOrders);
                    var apiContent = new StringContent(apiJson, Encoding.UTF8, "application/json");

                    HttpResponseMessage apiResponse = await client.PostAsync(apiUrl, apiContent);
                    if (!apiResponse.IsSuccessStatusCode)
                    {
                        string apiError = await apiResponse.Content.ReadAsStringAsync();
                        Console.WriteLine("API Response Error: " + apiError);
                        System.Diagnostics.Debug.WriteLine("API Response Error: " + apiError);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("API Error in updateStatusCancelDO: " + ex.ToString());
                System.Diagnostics.Debug.WriteLine("API Error in updateStatusCancelDO: " + ex.ToString());
            }
        }


        // ----------------------------------------------------------
        // PHASE 1 : DOWNLOAD ALL PAGES from BASE_URL + INSERT
        // เทียบกับ fetch_all_pages() + main() ใน Python
        // ----------------------------------------------------------
        private async Task<bool> DownloadAndInsertDeliveryOrders()
        {
            try
            {
                btLoadDO.Enabled = false;

                string today = DateTime.Now.ToString("yyyy-MM-dd");
                string DOWNLOAD_BASE_URL = getBaseApi(1, 2);
                string DOWNLOAD_TOKEN = getBaseApi(5, 2);
                string compCode = getBaseApi(4, 2);

                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    // Static token (เหมือน TOKEN = "xxx" ใน Python)
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", DOWNLOAD_TOKEN);

                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")
                    );

                    int page = 1;
                    int totalRecords = 0;

                    // =============================================
                    // LOOP ทุก page (เทียบกับ while True ใน Python)
                    // =============================================
                    while (true)
                    {
                        string pagedUrl =
                            $"{DOWNLOAD_BASE_URL}" +
                            $"?company={compCode}" +
                            $"&deliveryDate={today}" +
                            $"&page={page}";

                        HttpResponseMessage response =
                            await client.GetAsync(pagedUrl);

                        if (!response.IsSuccessStatusCode)
                        {
                            string error =
                                await response.Content.ReadAsStringAsync();

                            MessageBox.Show(
                                $"DOWNLOAD ERROR (page {page}) : {error}",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                            return false;
                        }

                        string json =
                            await response.Content.ReadAsStringAsync();

                        // Parse  { "data": [...] }
                        DeliveryOrderPageResponse pageObj =
                            JsonConvert.DeserializeObject<DeliveryOrderPageResponse>(json);

                        // ไม่มีข้อมูลแล้ว → หยุด loop
                        if (pageObj?.data == null || pageObj.data.Count == 0)
                            break;

                        // =============================================
                        // INSERT INTO local DB
                        // ON CONFLICT (doc_no) DO NOTHING
                        // =============================================
                        dl.connect();

                        foreach (DeliveryOrderApiItem item in pageObj.data)
                        {
                            OdbcCommand cmd =
                                (OdbcCommand)dl.sqlConn().CreateCommand();

                            cmd.CommandText = @"
                            INSERT INTO delivery_order (
                                doc_no, delivery_date, delivery_type,
                                car_company, car_customer,
                                car_company_rem, car_customer_rem,
                                customer_code, customer_name, customer_address,
                                product_code, product_name, qty, unit_name,
                                sale_name, note, status, site_id, site_name
                            )
                            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
                            ON CONFLICT (doc_no) DO UPDATE SET
                                status = EXCLUDED.status;";

                            // --- VALUES (เทียบกับ convert_api_to_db() ใน Python) ---
                            cmd.Parameters.AddWithValue("", item.docNo ?? "");
                            cmd.Parameters.AddWithValue("", item.deliveryDate ?? "");
                            cmd.Parameters.AddWithValue("", item.deliveryType ?? "");
                            cmd.Parameters.AddWithValue("", item.carCompany ?? "");
                            cmd.Parameters.AddWithValue("", item.carCustomer ?? "");
                            cmd.Parameters.AddWithValue("", item.carCompany ?? ""); // car_company_rem
                            cmd.Parameters.AddWithValue("", item.carCustomer ?? ""); // car_customer_rem
                            cmd.Parameters.AddWithValue("", item.customerCode ?? "");
                            cmd.Parameters.AddWithValue("", item.customerName ?? "");
                            cmd.Parameters.AddWithValue("", item.customerAddress ?? "");
                            cmd.Parameters.AddWithValue("", item.productCode ?? "");
                            cmd.Parameters.AddWithValue("", item.productName ?? "");
                            cmd.Parameters.AddWithValue("",
                                item.qty != null ? Convert.ToDecimal(item.qty) : 0m);
                            cmd.Parameters.AddWithValue("", item.unitName ?? "");
                            cmd.Parameters.AddWithValue("", item.saleName ?? "");
                            cmd.Parameters.AddWithValue("", item.note ?? "");
                            // Resolve site_id from base_site if not found
                            string resolvedSiteId = item.siteId ?? "";
                            if (!string.IsNullOrEmpty(resolvedSiteId))
                            {
                                using (OdbcCommand checkCmd = (OdbcCommand)dl.sqlConn().CreateCommand())
                                {
                                    checkCmd.CommandText = "SELECT COUNT(*) FROM public.base_site WHERE base_site_id = ?";
                                    checkCmd.Parameters.AddWithValue("", resolvedSiteId);
                                    int count = 0;
                                    try
                                    {
                                        count = Convert.ToInt32(checkCmd.ExecuteScalar());
                                    }
                                    catch {}

                                    if (count == 0)
                                    {
                                        if (!string.IsNullOrEmpty(item.siteName))
                                        {
                                            using (OdbcCommand findCmd = (OdbcCommand)dl.sqlConn().CreateCommand())
                                            {
                                                findCmd.CommandText = "SELECT base_site_id FROM public.base_site WHERE base_site_name = ? LIMIT 1";
                                                findCmd.Parameters.AddWithValue("", item.siteName.Trim());
                                                object val = findCmd.ExecuteScalar();
                                                if (val != null && val != DBNull.Value)
                                                {
                                                    resolvedSiteId = val.ToString();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (!string.IsNullOrEmpty(item.siteName))
                            {
                                using (OdbcCommand findCmd = (OdbcCommand)dl.sqlConn().CreateCommand())
                                {
                                    findCmd.CommandText = "SELECT base_site_id FROM public.base_site WHERE base_site_name = ? LIMIT 1";
                                    findCmd.Parameters.AddWithValue("", item.siteName.Trim());
                                    object val = findCmd.ExecuteScalar();
                                    if (val != null && val != DBNull.Value)
                                    {
                                        resolvedSiteId = val.ToString();
                                    }
                                }
                            }

                            cmd.Parameters.AddWithValue("", item.status ?? "");
                            cmd.Parameters.AddWithValue("", resolvedSiteId);
                            cmd.Parameters.AddWithValue("", item.siteName ?? "");

                            // param สำหรับ WHERE NOT EXISTS


                            cmd.ExecuteNonQuery();
                            totalRecords++;
                        }
                                 
                        dl.close();

                        page++;
                    }

                    // จบ loop ทุก page สำเร็จ
                    return true;
                }
            }
            catch (Exception ex)
            {
                dl.close();

                MessageBox.Show(
                    "DOWNLOAD INSERT ERROR : " + ex.ToString(),
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return false;
            }
            finally
            {
                btLoadDO.Enabled = true;
            }
        }

        // ----------------------------------------------------------
        // PHASE 2 : UPDATE summary via JWT API (เดิม)
        // ----------------------------------------------------------
        private async Task<UpdateDeliveryOrderResult> UpdateDeliveryOrderFromApi()
        {
            string baseUrl = getBaseApi(1, 1);
            string username = getBaseApi(2, 1);
            string password = getBaseApi(3, 1);
            string compCode = getBaseApi(4, 1);

            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string jwtUrl = $"{baseUrl}/jwt/create/";

            try
            {
                btLoadDO.Enabled = false;

                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    // =============================================
                    // JWT LOGIN
                    // =============================================
                    var loginData = new { username = username, password = password };

                    string loginJson = JsonConvert.SerializeObject(loginData);

                    var loginContent =
                        new StringContent(loginJson, Encoding.UTF8, "application/json");

                    HttpResponseMessage jwtResponse =
                        await client.PostAsync(jwtUrl, loginContent);

                    if (!jwtResponse.IsSuccessStatusCode)
                    {
                        string jwtError = await jwtResponse.Content.ReadAsStringAsync();
                        return new UpdateDeliveryOrderResult 
                        { 
                            IsSuccess = false, 
                            ErrorMessage = "JWT Login failed: " + jwtError 
                        };
                    }

                    string jwtResult =
                        await jwtResponse.Content.ReadAsStringAsync();

                    dynamic jwtObj =
                        JsonConvert.DeserializeObject(jwtResult);

                    string accessToken = jwtObj.access.ToString();

                    // =============================================
                    // SET BEARER TOKEN
                    // =============================================
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", accessToken);

                    int page = 1;
                    bool hasMore = true;

                    // =============================================
                    // UPDATE local DB
                    // =============================================
                    dl.connect();

                    while (hasMore)
                    {
                        string summaryUrl =
                            $"{baseUrl}/deliveryorder/summary/api/by/comp/?comp_code={compCode}&date={today}&page={page}";

                        // =============================================
                        // GET SUMMARY API
                        // =============================================
                        HttpResponseMessage apiResponse =
                            await client.GetAsync(summaryUrl);

                        if (!apiResponse.IsSuccessStatusCode)
                        {
                            string apiError = await apiResponse.Content.ReadAsStringAsync();
                            bool is422 = apiResponse.StatusCode == (System.Net.HttpStatusCode)422;

                            dl.close();
                            return new UpdateDeliveryOrderResult 
                            { 
                                IsSuccess = false, 
                                IsValidationError = is422,
                                ErrorMessage = apiError 
                            };
                        }

                        string json =
                            await apiResponse.Content.ReadAsStringAsync();

                        List<DeliveryOrder> orders = null;
                        string nextUrl = null;

                        if (json.TrimStart().StartsWith("["))
                        {
                            orders = JsonConvert.DeserializeObject<List<DeliveryOrder>>(json);
                            hasMore = false; // Not paginated, single page
                        }
                        else
                        {
                            var pageObj = JsonConvert.DeserializeObject<DRFPaginationResponse<DeliveryOrder>>(json);
                            orders = pageObj?.results ?? pageObj?.data;
                            nextUrl = pageObj?.next;
                            hasMore = !string.IsNullOrEmpty(nextUrl) && orders != null && orders.Count > 0;
                        }

                        if (orders == null || orders.Count == 0)
                        {
                            break;
                        }

                        foreach (DeliveryOrder item in orders)
                        {
                            OdbcCommand pgCommand =
                                (OdbcCommand)dl.sqlConn().CreateCommand();

                            pgCommand.CommandText = @"
                            UPDATE delivery_order
                            SET
                                car_company_tot  = ?,
                                car_customer_tot = ?,
                                qty_tot          = ?,
                                car_company_rem  = ?,
                                car_customer_rem = ?
                            WHERE doc_no = ?
                            ";

                            pgCommand.Parameters.AddWithValue("", item.car_company_tot);
                            pgCommand.Parameters.AddWithValue("", item.car_customer_tot);
                            pgCommand.Parameters.AddWithValue("",
                                Convert.ToDecimal(item.qty_tot));
                            pgCommand.Parameters.AddWithValue("", item.car_company_rem);
                            pgCommand.Parameters.AddWithValue("", item.car_customer_rem);
                            pgCommand.Parameters.AddWithValue("", item.doc_no);

                            pgCommand.ExecuteNonQuery();
                        }

                        page++;
                    }

                    dl.close();

                    return new UpdateDeliveryOrderResult { IsSuccess = true };
                }
            }
            catch (Exception ex)
            {
                dl.close();
                return new UpdateDeliveryOrderResult 
                { 
                    IsSuccess = false, 
                    ErrorMessage = ex.Message 
                };
            }
            finally
            {
                btLoadDO.Enabled = true;
            }
        }


        private async Task<bool> CUWeightDeliveryFromApi()
        {
            string baseUrl = getBaseApi(1, 1);
            string username = getBaseApi(2, 1);
            string password = getBaseApi(3, 1);
            string compCode = getBaseApi(4, 1);

            string today = DateTime.Now.ToString("yyyy-MM-dd");

            string jwtUrl =
                $"{baseUrl}/jwt/create/";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    // =========================
                    // JWT LOGIN
                    // =========================
                    string accessToken = await GetJwtToken(client, baseUrl, username, password);

                    if (accessToken == null)
                        return false;

                    // =========================
                    // SET TOKEN
                    // =========================
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(
                            "Bearer",
                            accessToken
                        );

                    int page = 1;
                    bool hasMore = true;

                    // =========================
                    // UPDATE DATABASE
                    // =========================
                    dl.connect();

                    try
                    {
                        while (hasMore)
                        {
                            string apiUrl =
                                $"{baseUrl}/weightdelivery/summary/api/by/comp/?comp_code={compCode}&date={today}&page={page}";
                            // =========================
                            // GET API
                            // =========================
                            HttpResponseMessage apiResponse =
                                await client.GetAsync(apiUrl);

                            if (!apiResponse.IsSuccessStatusCode)
                            {
                                string apiError =
                                    await apiResponse.Content.ReadAsStringAsync();

                                MessageBox.Show(
                                    "API ERROR : " + apiError,
                                    "Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );

                                return false;
                            }

                            string json =
                                await apiResponse.Content.ReadAsStringAsync();

                            List<WeightDelivery> orders = null;
                            string nextUrl = null;

                            if (json.TrimStart().StartsWith("["))
                            {
                                orders = JsonConvert.DeserializeObject<List<WeightDelivery>>(json);
                                hasMore = false;
                            }
                            else
                            {
                                var pageObj = JsonConvert.DeserializeObject<DRFPaginationResponse<WeightDelivery>>(json);
                                orders = pageObj?.results ?? pageObj?.data;
                                nextUrl = pageObj?.next;
                                hasMore = !string.IsNullOrEmpty(nextUrl) && orders != null && orders.Count > 0;
                            }

                            if (orders == null || orders.Count == 0)
                            {
                                break;
                            }

                            foreach (var item in orders)
                            {
                                OdbcCommand pgCommand =
                                    (OdbcCommand)dl.sqlConn().CreateCommand();

                                pgCommand.CommandText = @"
                            INSERT INTO weight_delivery
                            (
                                weight_id,
                                delivery_date,
                                bws,
                                comp_code,
                                do_doc_no,
                                carry_type_name,
                                is_cancel
                            )
                            VALUES
                            (
                                ?, ?, ?, ?, ?, ?, ?
                            )
                            ON CONFLICT (weight_id)
                            DO UPDATE SET
                                delivery_date = EXCLUDED.delivery_date,
                                bws = EXCLUDED.bws,
                                comp_code = EXCLUDED.comp_code,
                                do_doc_no = EXCLUDED.do_doc_no,
                                carry_type_name = EXCLUDED.carry_type_name,
                                is_cancel = EXCLUDED.is_cancel
                        ";

                                pgCommand.Parameters.AddWithValue("", item.weight_id);
                                pgCommand.Parameters.AddWithValue("", Convert.ToDateTime(item.delivery_date));
                                pgCommand.Parameters.AddWithValue("", item.bws);
                                pgCommand.Parameters.AddWithValue("", item.comp_code);
                                pgCommand.Parameters.AddWithValue("", item.do_doc_no);
                                pgCommand.Parameters.AddWithValue("", item.carry_type_name);
                                pgCommand.Parameters.AddWithValue("", item.is_cancel);

                                pgCommand.ExecuteNonQuery();
                            }

                            page++;
                        }
                    }
                    finally
                    {
                        dl.close();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                dl.close();

                MessageBox.Show(
                    ex.ToString(),
                    "ERROR",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return false;
            }
        }

    }


}
