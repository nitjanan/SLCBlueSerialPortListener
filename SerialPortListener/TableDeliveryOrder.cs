using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
            public String stoneTypeId;
            public String stoneTypeName;
            public String qty;
            public String unitName;
        }

        public TableDeliveryOrder(MainForm parent)
        {
            InitializeComponent();
            dl = new Datalayer();
            mainForm = parent;
        }

        private void TableDeliveryOrder_Load(object sender, EventArgs e)
        {

            try
            {
                // TODO: This line of code loads data into the 'truckDataSet.weight' table. You can move, or remove it, as needed.
                setSearchDataDO();
            }
            catch (Exception ex)
            {
            }

        }

        private void prepareDataDOToMainForm()
        {
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
            mainForm.resetFromDO();
            DataDO data_do = new DataDO();
            if (dgvDO.Rows.Count > 1)
            {
                data_do.do_id = dgvDO.CurrentRow.Cells["do_id2"].Value.ToString();
                data_do.docNo = dgvDO.CurrentRow.Cells["doc_no"].Value.ToString();

                data_do.deliveryDate = dgvDO.CurrentRow.Cells["delivery_date"].Value.ToString();
                data_do.carCompany = dgvDO.CurrentRow.Cells["car_company"].Value.ToString();

                data_do.carCustomer = dgvDO.CurrentRow.Cells["car_customer"].Value.ToString();
                data_do.customerId = dgvDO.CurrentRow.Cells["customer_code"].Value.ToString();

                data_do.customerName = dgvDO.CurrentRow.Cells["customer_name"].Value.ToString();
                data_do.stoneTypeId = dgvDO.CurrentRow.Cells["product_code"].Value.ToString();

                data_do.stoneTypeName = dgvDO.CurrentRow.Cells["product_name"].Value.ToString();
                data_do.qty = dgvDO.CurrentRow.Cells["qty2"].Value.ToString();

                data_do.unitName = dgvDO.CurrentRow.Cells["unit_name"].Value.ToString();

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
            DateTime today = DateTime.Today;

            try
            {
                dl.connect();

                string sql = @"SELECT * 
                   FROM public.delivery_order 
                   WHERE delivery_date = ? ORDER BY doc_no";

                using (OdbcCommand cmd = new OdbcCommand(sql, dl.sqlConn()))
                {
                    // ใส่ parameter ตามลำดับ ?
                    cmd.Parameters.Add("delivery_date", OdbcType.Date).Value = today;

                    using (OdbcDataAdapter adt = new OdbcDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adt.Fill(dt);
                        dgvDO.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
    }


}
