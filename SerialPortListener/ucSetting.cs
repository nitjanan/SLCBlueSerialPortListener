using System;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Devart.Data.PostgreSql;
using System.Data.Odbc;
using System.Diagnostics;

namespace SerialPortListener
{
    public partial class ucSetting : UserControl
    {
        private static ucSetting _instance;
        Datalayer dl = null;
        //scale
        OdbcDataAdapter adtScale;
        DataTable dtScale;
        OdbcCommandBuilder cmbScale;
        //scoop
        OdbcDataAdapter adtScoop;
        DataTable dtScoop;
        OdbcCommandBuilder cmbScoop;
        //stone type
        OdbcDataAdapter adtStoneType;
        DataTable dtStoneType;
        OdbcCommandBuilder cmbStoneType;
        //approve
        OdbcDataAdapter adtApprove;
        DataTable dtApprove;
        OdbcCommandBuilder cmbApprove;
        //customer
        OdbcDataAdapter adtCustomer;
        DataTable dtCustomer;
        OdbcCommandBuilder cmbCustomer;
        //car city
        OdbcDataAdapter adtCarCity;
        DataTable dtCarCity;
        OdbcCommandBuilder cmbCarCity;
        //users
        OdbcDataAdapter adtUsers;
        DataTable dtUsers;
        OdbcCommandBuilder cmbUsers;
        //car team
        OdbcDataAdapter adtCarTeam;
        DataTable dtCarTeam;
        OdbcCommandBuilder cmbCarTeam;
        //car
        OdbcDataAdapter adtCar;
        DataTable dtCar;
        OdbcCommandBuilder cmbCar;
        //mill
        OdbcDataAdapter adtMill;
        DataTable dtMill;
        OdbcCommandBuilder cmbMill;

        //site
        OdbcDataAdapter adtSite;
        DataTable dtSite;
        OdbcCommandBuilder cmbSite;

        //site
        OdbcDataAdapter adtJobType;
        DataTable dtJobType;
        OdbcCommandBuilder cmbJobType;

        // Bind default keywords
        List<string> listOriginalCustomerNameSetting = new List<string>();
        // save new keywords
        List<string> listNewCustomerNameSetting = new List<string>();

        private static ucSetting Instance
        {
            set 
            {
                if (_instance == null)
                    _instance = new ucSetting();
            }
            get
            {
                if (_instance == null)
                    _instance = new ucSetting();
                return _instance;
            }
        }
        public ucSetting()
        {
            dl = new Datalayer();
            InitializeComponent();
            //set ค่าให้ช่อง combobox ลูกค้า
            setautoComplete(cbbCustomerSiteName, "รหัสลูกค้า", "ชื่อลูกค้า", "base_customer", listOriginalCustomerNameSetting);
        
            // ส่วนตั้งค่าที่ย้ายมาจาก ucBackup / ucHelp
            LoadBackupConfig();
            InitAutoBackupTimer();
            LoadReportTemplateSetting();
        }

        private void ucSetting_Load(object sender, EventArgs e)
        {
            //this.base_scaleTableAdapter.Fill(this.baseScaleDataSet.base_scale);
            setDataSouceForDGVScale();
            if (!Globals.isPermissionTop() && !Globals.isPermissionAddSetting())
            {
                //แถบรหัสพนักงาน
                tcSetting.TabPages.Remove(tabPage1);
                //แถบuser
                tcSetting.TabPages.Remove(tabPage7);
            }
        }

        private void setautoComplete(ComboBox cb, string fieldId, string fieldName, string tableName, List<string> listOriginal)
        {
            listOriginal.Clear();
            cb.Items.Clear();

            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT DISTINCT " + fieldName + " , " + fieldId + " FROM public." + tableName + " WHERE base_job_type_id IS NOT NULL AND base_vat_type_id  IS NOT NULL ORDER BY " + fieldId;
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string rdStr = reader[fieldId].ToString() +" : " + reader[fieldName].ToString();
                    listOriginal.Add(rdStr);

                }
            }
            catch (Exception)
            {
            }

            dl.close();
            cb.Items.AddRange(listOriginal.ToArray());
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void tcSetting_SelectedIndexChanged(object sender, EventArgs e)
        {
            resetDTGV();
        }

        private void saveAndUpdateDTGV(OdbcCommandBuilder cmb, OdbcDataAdapter adt, DataTable dt, DataGridView dgv, String tableName)
        {
            try
            {
                dl.connect();
                cmb = new OdbcCommandBuilder(adt);
                adt.Update(dt);
                MessageBox.Show("บันทึกข้อมูลเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dl.close();
            }
            catch (System.InvalidOperationException exUpdate)
            {
                updateDTGV(dgv, tableName);
            }
            catch (OdbcException exDuplicate)
            {
                MessageBox.Show("มีรหัสที่ซ้ำกัน กรุณากรอกข้อมูลใหม่", "ผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("กรุณาลองใหม่อีกครั้ง", "ผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void resetDTGV() {
            if (tcSetting.SelectedTab == tabPage1)
            {
                //this.base_scaleTableAdapter.Fill(this.baseScaleDataSet.base_scale);
                setDataSouceForDGVScale();
            }
            else if (tcSetting.SelectedTab == tabPage2)
            {
                //this.base_stone_typeTableAdapter.Fill(this.baseStoneTypeDataSet.base_stone_type);
                setDataSouceForDGVStoneType();
            }
            else if (tcSetting.SelectedTab == tabPage3)
            {
                //this.base_scoopTableAdapter.Fill(this.baseScoopDataSet.base_scoop);
                setDataSouceForDGVScoop();
            }
            else if (tcSetting.SelectedTab == tabPage4)
            {
                //this.base_approveTableAdapter.Fill(this.baseApproveDataSet.base_approve);
                setDataSouceForDGVApprove();
            }
            else if (tcSetting.SelectedTab == tabPage5)
            {
                //this.base_customerTableAdapter.Fill(this.baseCustomerDataSet.base_customer);
                setDataSouceForDGVCustomer();
                // set cobbobox vat type
                fillComboAll(cbbVatType, "base_vat_type", "base_vat_type_name");
                // set cobbobox job type
                fillComboAll(cbbJobType, "base_job_type", "base_job_type_name");
            }
            else if (tcSetting.SelectedTab == tabPage6)
            {
                //this.base_car_cityTableAdapter .Fill(this.baseCarCityDataSet.base_car_city);
                setDataSouceForDGVCarCity();

            }
            else if (tcSetting.SelectedTab == tabPage7)
            {
                //this.usersTableAdapter.Fill(this.usersDataSet.users);
                setDataSouceForDGVUsers();
            }
            else if (tcSetting.SelectedTab == tabPage8)
            {
                //this.base_car_teamTableAdapter.Fill(this.baseCarTeamDataSet.base_car_team);
                setDataSouceForDGVCarTeam();
            }
            else if (tcSetting.SelectedTab == tabPage9)
            {
                fillCarTeamCombo();
                //this.base_carTableAdapter.Fill(this.baseCarDataSet.base_car);
                setDataSouceForDGVCar();
            }
            else if (tcSetting.SelectedTab == tabPage10)
            {
                setDataSouceForDGVMill();
            }
            else if (tcSetting.SelectedTab == tabPage11)
            {
                
            }
            else if (tcSetting.SelectedTab == tabPage12)
            {
                setDataSouceForDGVJobType();
            }
        }

        private void deleteDTGVOld(DataGridView dgv)
        {
            try
            {
                int rowIndex = dgv.CurrentCell.RowIndex;
                dgv.Rows.RemoveAt(rowIndex);
            }
            catch (Exception ex)
            {
            }
        }


        private void deleteDTGV(DataGridView dgv, String cellName, String tableName)
        {
            int rowIndex = dgv.CurrentCell.RowIndex;
            string id = dgv.CurrentRow.Cells[cellName].Value.ToString();

            //sql delete
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "DELETE FROM public."+ tableName + " where "+ cellName + " = '" + id + "' ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                dgv.Rows.RemoveAt(rowIndex);
                MessageBox.Show("ลบข้อมูลเรียบร้อย", "ลบข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("กรุณาลองใหม่อีกครั้ง", "ผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            dl.close();

            //reset ก่อน save ใหม่
            resetDTGV();
        }

        private void updateDTGV(DataGridView dgv, String tableName)
        {
            //dgv.CurrentRow.ErrorText = "";

            int numCol = dgv.Columns.Count;

            string idName = dgv.Columns[0].Name;
            string idValue = dgv.CurrentRow.Cells[idName].Value.ToString();

            string colOneName = dgv.Columns[1].Name;
            string colOneValue = dgv.CurrentRow.Cells[colOneName].Value.ToString();

            string colTwoName = null;
            string colTwoValue = null;

            if (numCol > 2)
            {
                colTwoName = dgv.Columns[2].Name;
                colTwoValue = dgv.CurrentRow.Cells[colTwoName].Value.ToString();
            }

            //sql update
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            StringBuilder sqlTxt = new StringBuilder();
            sqlTxt.Append("UPDATE public." + tableName + " SET " + colOneName + " = '" + colOneValue + "' ");
            if (numCol > 2)
                sqlTxt.Append(" , " + colTwoName + " = '" + colTwoValue + "' ");
            sqlTxt.Append(" WHERE " + idName + " = '" + idValue + "' ");
            pgCommand.CommandText = sqlTxt.ToString();
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                MessageBox.Show("อัพเดทข้อมูลเรียบร้อย", "ลบข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("กรุณาลองใหม่อีกครั้ง", "ผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            dl.close();
        }

        private void fillCarTeamCombo()
        {
            //ล้างก่อน
            cbbCarTeamName.Items.Clear();
            //เพิ่ม combobox
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT * FROM public.base_car_team";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string des = reader["ชื่อทีม"].ToString();
                    cbbCarTeamName.Items.Add(des);
                }
            }
            catch (Exception)
            {

            }
            dl.close();
            cbbCarTeamName.SelectedIndex = 0;
        }

        /*Base Scale*/
        private void setDataSouceForDGVScale()
        {
            try
            {
                dl.connect();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT *  FROM public.base_scale ");
                adtScale = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                dtScale = new DataTable();
                adtScale.Fill(dtScale);
                dgvScale.DataSource = dtScale;
            }
            catch (Exception)
            {
            }
            dl.close();
        }

        private void btSaveScale_Click(object sender, EventArgs e)
        {
            //saveActionScale();
            saveAndUpdateDTGV(cmbScale, adtScale, dtScale, dgvScale , "base_scale");
        }

        private void saveActionScale()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                basescaleBindingSource.EndEdit();
                base_scaleTableAdapter.Update(this.baseScaleDataSet.base_scale);
                MessageBox.Show("บันทึกข้อมูลเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.Default;
        }


        private void dgvScale_KeyDown(object sender, KeyEventArgs e)
        {
            /*
            if (e.KeyCode == Keys.Delete)
            {
                if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    basescaleBindingSource.RemoveCurrent();
                    saveActionScale();
                }
            }
            */
        }

        private void btDelScale_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //basescaleBindingSource.RemoveCurrent();
                //saveActionScale();
                deleteDTGV(dgvScale, "รหัสพนักงาน", "base_scale");
            }
        }

        /* Base Stone Type*/
        private void setDataSouceForDGVStoneType()
        {
            try
            {
                dl.connect();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT *  FROM public.base_stone_type ");
                adtStoneType = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                dtStoneType = new DataTable();
                adtStoneType.Fill(dtStoneType);
                dgvStoneType.DataSource = dtStoneType;
            }
            catch (Exception)
            {
            }
            dl.close();
        } 
        private void btSaveStoneType_Click(object sender, EventArgs e)
        {
            //saveActionStoneType();
            saveAndUpdateDTGV(cmbStoneType, adtStoneType, dtStoneType , dgvStoneType, "base_stone_type");
        }
        private void saveActionStoneType()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                basestonetypeBindingSource.EndEdit();
                base_stone_typeTableAdapter.Update(this.baseStoneTypeDataSet.base_stone_type);
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.Default;
        }

        private void dgvStoneType_KeyDown(object sender, KeyEventArgs e)
        {
            /*
            if (e.KeyCode == Keys.Delete)
            {
                if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    basestonetypeBindingSource.RemoveCurrent();
                    saveActionStoneType();
                }
            }
            */
        }

        private void btDelStoneType_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //basestonetypeBindingSource.RemoveCurrent();
                //saveActionStoneType();
                deleteDTGV(dgvStoneType, "รหัสหิน", "base_stone_type");
            }
        }

        /*Base Scoop*/
        private void setDataSouceForDGVScoop()
        {
            try
            {
                dl.connect();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT *  FROM public.base_scoop ");
                adtScoop = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                dtScoop = new DataTable();
                adtScoop.Fill(dtScoop);
                dgvScoop.DataSource = dtScoop;
            }
            catch (Exception)
            {
            }
            dl.close();
        }

        private void btSaveScoop_Click(object sender, EventArgs e)
        {
            //saveActionScoop();
            saveAndUpdateDTGV(cmbScoop, adtScoop, dtScoop, dgvScoop, "base_scoop");
        }
        private void saveActionScoop()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                basescoopBindingSource.EndEdit();
                base_scoopTableAdapter.Update(this.baseScoopDataSet.base_scoop);
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.Default;
        }

        private void dgvScoop_KeyDown(object sender, KeyEventArgs e)
        {
            /*
            if (e.KeyCode == Keys.Delete)
            {
                if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    basescoopBindingSource.RemoveCurrent();
                    saveActionScoop();
                }
            }
            */
        }

        private void btDelScoop_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //basescoopBindingSource.RemoveCurrent();
                //saveActionScoop();
                deleteDTGV(dgvScoop, "รหัสผู้ตัก", "base_scoop");
            }
        }

        /*Base Approve*/
        private void setDataSouceForDGVApprove()
        {
            try
            {
                dl.connect();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT *  FROM public.base_approve ");
                adtApprove = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                dtApprove = new DataTable();
                adtApprove.Fill(dtApprove);
                dgvApprove.DataSource = dtApprove;
            }
            catch (Exception)
            {
            }
            dl.close();
        }

        private void btSaveApprove_Click(object sender, EventArgs e)
        {
            //saveActionApprove();
            saveAndUpdateDTGV(cmbApprove, adtApprove, dtApprove, dgvApprove, "base_approve");
        }        
        private void saveActionApprove()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                baseapproveBindingSource.EndEdit();
                base_approveTableAdapter.Update(this.baseApproveDataSet.base_approve);
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.Default;
        }

        private void dgvApprove_KeyDown(object sender, KeyEventArgs e)
        {
            /*
            if (e.KeyCode == Keys.Delete)
            {
                if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    baseapproveBindingSource.RemoveCurrent();
                    saveActionApprove();
                }
            }
            */
        }

        private void btDelApprove_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //baseapproveBindingSource.RemoveCurrent();
                //saveActionApprove();

                deleteDTGV(dgvApprove, "รหัสผู้อนุมัติจ่าย", "base_approve");
            }
        }

        /*Base Customer*/
        private void setDataSouceForDGVCustomer()
        {
            if (cbbJobType.SelectedIndex != -1 && cbbVatType.SelectedIndex != -1)
            {
                try
                {
                    dl.connect();
                    StringBuilder sql = new StringBuilder();
                    sql.Append(" SELECT base_customer.*  FROM ((base_customer ");
                    sql.Append(" INNER JOIN base_job_type ON base_customer.base_job_type_id = base_job_type.base_job_type_id) ");
                    sql.Append(" INNER JOIN base_vat_type ON base_customer.base_vat_type_id = base_vat_type.base_vat_type_id) ");
                    sql.Append(" WHERE base_job_type_name = '" + cbbJobType.Text + "' AND base_vat_type_name = '" + cbbVatType.Text + "'  ORDER BY รหัสลูกค้า ");
                    adtCustomer = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                    dtCustomer = new DataTable();
                    adtCustomer.Fill(dtCustomer);
                    dgvCustomer.DataSource = dtCustomer;
                }
                catch (Exception)
                {
                }
                dl.close();
            }
        }

        private void saveActionCustomer()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                basecustomerBindingSource.EndEdit();
                base_customerTableAdapter.Update(this.baseCustomerDataSet.base_customer);
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.Default;
        }

        private void btSaveCustomer_Click(object sender, EventArgs e)
        {
            Boolean isUpdate = false;
            if (cbbJobType.SelectedIndex == -1 || cbbVatType.SelectedIndex == -1)
            {
                MessageBox.Show("ไม่สามารถบันทึกได้ กรุณาเลือกชนิดและประเภทลูกค้า", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (tbCustomerId.Text == "")
            {
                MessageBox.Show("ไม่สามารถบันทึกได้ กรุณาใส่รหัสลูกค้า", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (tbCustomerId.Text.Any(char.IsLower))
            {
                MessageBox.Show("ไม่สามารถบันทึกได้ เนื่องจากรหัสเป็นตัวพิมเล็ก", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                //หาว่า id ซ้ำหรือไม่ ถ้าซ้ำ update ถ้าไม่มี insert
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT รหัสลูกค้า FROM public.base_customer where รหัสลูกค้า = '" + tbCustomerId.Text + "'";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    isUpdate = reader.Read();
                }
                catch (Exception)
                {
                }
                dl.close();

                //update or save
                if (isUpdate && tbCustomerId.ReadOnly)
                    updateBaseCustomerAction();
                else
                    saveBaseCustomerAction();

                //set ค่าให้ช่อง combobox ลูกค้า
                setautoComplete(cbbCustomerSiteName, "รหัสลูกค้า", "ชื่อลูกค้า", "base_customer", listOriginalCustomerNameSetting);

            }
        }

        private void updateBaseCustomerAction()
        {
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "UPDATE base_customer SET ชื่อลูกค้า = '" + tbCustomerName.Text + "' , ที่อยู่ = '" + tbCustomerAddress.Text + "' WHERE รหัสลูกค้า = '" + tbCustomerId.Text + "' ; ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                /*กรองตาม ทีมรถ*/
                //this.base_carTableAdapter.Fill(this.baseCarDataSet.base_car);
                //this.basecarBindingSource.Filter = string.Format("รหัสทีม = '" + tbCarTeamId.Text + "'");
                setDataSouceForDGVCustomer();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            dl.close();
        }

        private void saveBaseCustomerAction()
        {
            if (tbCustomerId.Text.Length > 8)
            {
                MessageBox.Show("ไม่สามารถบันทึกได้ เนื่องจากรหัสลูกค้ามีมากกว่า 8 หลัก", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (tbCustomerId.Text.Contains(":") || tbCustomerName.Text.Contains(":"))
            {
                MessageBox.Show("ไม่สามารถบันทึกได้ เนื่องมีตัวอักษร : ในรหัสลูกค้าหรือชื่อลูกค้านี้", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "INSERT INTO base_customer (รหัสลูกค้า, ชื่อลูกค้า, ที่อยู่, base_job_type_id, base_vat_type_id)" +
                                         "VALUES ('" + tbCustomerId.Text + "','" + tbCustomerName.Text + "','" + tbCustomerAddress.Text + "', (SELECT base_job_type_id FROM base_job_type WHERE base_job_type_name = '" + cbbJobType.Text + "' ) , (SELECT base_vat_type_id FROM base_vat_type WHERE base_vat_type_name = '" + cbbVatType.Text + "' ) )";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    /*กรองตาม ทีมรถ*/
                    //this.base_carTableAdapter.Fill(this.baseCarDataSet.base_car);
                    //this.basecarBindingSource.Filter = string.Format("รหัสทีม = '" + tbCarTeamId.Text + "'");
                    setDataSouceForDGVCustomer();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("รหัสลูกค้านี้มีอยู่แล้ว กรุณาเปลี่ยนรหัสลูกค้าใหม่", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                dl.close();
            }
        }

        private void dgvCustomer_KeyDown(object sender, KeyEventArgs e)
        {
            /*
            if (e.KeyCode == Keys.Delete)
            {
                if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    basecustomerBindingSource.RemoveCurrent();
                    saveActionCustomer();
                }
            }
            */
        }
        private void btDelCustomer_Click(object sender, EventArgs e)
        {
            if (tbCustomerId.Text == "")
            {
                MessageBox.Show("กรุณาเลือกรายการที่ต้องการลบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    //ลบหน้างานที่ผูกกับลูกค้านั้นๆก่อน
                    deleteSiteBeforeCustomer();

                    //sql
                    OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                    pgCommand.CommandText = "DELETE FROM base_customer WHERE รหัสลูกค้า = '" + tbCustomerId.Text + "' ; ";
                    try
                    {
                        dl.connect();
                        OdbcDataReader reader = pgCommand.ExecuteReader();
                        MessageBox.Show("ลบรายการเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        /*กรองตาม ลูกค้า*/
                        setDataSouceForDGVCustomer();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    dl.close();
                }

                //set ค่าให้ช่อง combobox ลูกค้า
                setautoComplete(cbbCustomerSiteName, "รหัสลูกค้า", "ชื่อลูกค้า", "base_customer", listOriginalCustomerNameSetting);

                //clear ช่อง
                clearThreeTextbox(tbCustomerId, tbCustomerName, tbCustomerAddress);

            }
        }

        private void deleteSiteBeforeCustomer()
        {
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "DELETE FROM base_site WHERE base_customer_id = '" + tbCustomerId.Text + "' ; ";
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

        /*Base Car City*/
        private void setDataSouceForDGVCarCity()
        {
            try
            {
                dl.connect();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT *  FROM public.base_car_city ");
                adtCarCity = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                dtCarCity = new DataTable();
                adtCarCity.Fill(dtCarCity);
                dgvCarCity.DataSource = dtCarCity;
            }
            catch (Exception)
            {
            }
            dl.close();
        }

        private void btSaveCity_Click(object sender, EventArgs e)
        {
            //saveActionCarCity();
            saveAndUpdateDTGV(cmbCarCity, adtCarCity, dtCarCity, dgvCarCity, "base_car_city");
        }
        private void saveActionCarCity()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                basecarcityBindingSource.EndEdit();
                base_car_cityTableAdapter.Update(this.baseCarCityDataSet.base_car_city);
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.Default;
        }

        private void dgvCarCity_KeyDown(object sender, KeyEventArgs e)
        {
            /*
            if (e.KeyCode == Keys.Delete)
            {
                if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    basecarcityBindingSource.RemoveCurrent();
                    saveActionCarCity();
                }
            }
            */
        }

        private void btDelCity_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //basecarcityBindingSource.RemoveCurrent();
                //saveActionCarCity();
                deleteDTGV(dgvCarCity, "รหัสจังหวัด", "base_car_city");
            }
        }

        /*Users*/
        private void setDataSouceForDGVUsers()
        {
            try
            {
                dl.connect();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT *  FROM public.users ");
                adtUsers = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                dtUsers = new DataTable();
                adtUsers.Fill(dtUsers);
                dgvUsers.DataSource = dtUsers;
            }
            catch (Exception)
            {
            }
            dl.close();
        }

        private void saveActionUsers()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                usersBindingSource.EndEdit();
                usersTableAdapter.Update(this.usersDataSet.users);
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.Default;
        }

        private void btSaveUsers_Click(object sender, EventArgs e)
        {
            //saveActionUsers();
            saveAndUpdateDTGV(cmbUsers, adtUsers, dtUsers, dgvUsers, "users");
        }

        private void dgvUsers_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    //usersBindingSource.RemoveCurrent();
                    //saveActionUsers();
                    deleteDTGV(dgvUsers, "users_id", "users");
                }
            }
        }

        private void tbText_Leave(object sender, EventArgs e)
        {
            tbEncryption.Text = Utils.hashPassword(tbText.Text);
        }

        /*Base Car Team*/
        private void setDataSouceForDGVCarTeam()
        {
            try
            {
                dl.connect();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT *  FROM public.base_car_team ");
                adtCarTeam = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                dtCarTeam = new DataTable();
                adtCarTeam.Fill(dtCarTeam);
                dgvTeamCar.DataSource = dtCarTeam;
            }
            catch (Exception)
            {
            }
            dl.close();
        }

        private void btSaveCarTeam_Click(object sender, EventArgs e)
        {
            //saveActionCarTeam();
            saveAndUpdateDTGV(cmbCarTeam, adtCarTeam, dtCarTeam, dgvTeamCar, "base_car_team");
        }
        private void saveActionCarTeam()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                basecarteamBindingSource.EndEdit();
                base_car_teamTableAdapter.Update(this.baseCarTeamDataSet.base_car_team);
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.Default;
        }

        private void btDelCarTeam_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //basecarteamBindingSource.RemoveCurrent();
                //saveActionCarTeam();

                deleteDTGV(dgvTeamCar, "รหัสทีม", "base_car_team");
            }
        }

        private void dgvCar_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            /*set ค่าที่มาจาก Table base_car_team*/
            if(cbbCarTeamName.SelectedIndex != -1)
                setDataCarTeam();
        }

        private void setDataCarTeam()
        {
            if (dgvCar.Rows.Count > 1)
            {
                    tbCarId.Text = dgvCar.CurrentRow.Cells["รหัสรถร่วม"].Value.ToString();
                    tbCarName.Text = dgvCar.CurrentRow.Cells["ชื่อรถร่วม"].Value.ToString();

                    //เปิดปิดช่องเมื่อมีไม่มีค่า
                    if (tbCarId.Text != "")
                        tbCarId.ReadOnly = true;
                    else
                        tbCarId.ReadOnly = false;
            }
        }

        /*Base Car*/
        private void setDataSouceForDGVCar()
        {
            try
            {
                dl.connect();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT *  FROM public.base_car where รหัสทีม = '" + tbCarTeamId.Text + "'");
                adtCar = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                dtCar = new DataTable();
                adtCar.Fill(dtCar);
                dgvCar.DataSource = dtCar;
            }
            catch (Exception)
            {
            }
            dl.close();
        }

        private void btSaveCar_Click(object sender, EventArgs e)
        {
            Boolean isUpdate = false;
            if (tbCarTeamId.Text == "" || cbbCarTeamName.SelectedIndex == -1)
            {
                MessageBox.Show("กรุณาเลือกชื่อทีม", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else {
                //หาว่า id ซ้ำหรือไม่ ถ้าซ้ำ update ถ้าไม่มี insert
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT รหัสรถร่วม FROM public.base_car where รหัสรถร่วม = '" + tbCarId.Text + "' AND รหัสทีม  = '" + tbCarTeamId.Text + "'";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    isUpdate = reader.Read();
                }
                catch (Exception)
                {
                }
                dl.close();

                //update or save
                if (isUpdate && tbCarId.ReadOnly)
                    updateBaseCarAction();
                else
                    saveBaseCarAction();

            }
        }

        private void updateBaseCarAction() {
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "UPDATE base_car SET ชื่อรถร่วม = '" + tbCarName.Text + "' WHERE รหัสรถร่วม = '" + tbCarId.Text + "' ; ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                /*กรองตาม ทีมรถ*/
                //this.base_carTableAdapter.Fill(this.baseCarDataSet.base_car);
                //this.basecarBindingSource.Filter = string.Format("รหัสทีม = '" + tbCarTeamId.Text + "'");
                setDataSouceForDGVCar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            dl.close();
        }

        private void saveBaseCarAction(){
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "INSERT INTO base_car (รหัสรถร่วม, ชื่อรถร่วม, รหัสทีม)" +
                                     "VALUES ('" + tbCarId.Text + "','" + tbCarName.Text + "','" + tbCarTeamId.Text + "' )";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                /*กรองตาม ทีมรถ*/
                //this.base_carTableAdapter.Fill(this.baseCarDataSet.base_car);
                //this.basecarBindingSource.Filter = string.Format("รหัสทีม = '" + tbCarTeamId.Text + "'");
                setDataSouceForDGVCar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("รหัสรถร่วมนี้มีอยู่แล้ว กรุณาเปลี่ยนรหัสรถร่วมใหม่", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            dl.close();
        }

        private void btClearCar_Click(object sender, EventArgs e)
        {
            tbCarId.Text = "";
            tbCarId.ReadOnly = false;
            tbCarName.Text = "";
        }

        private void btDelCar_Click(object sender, EventArgs e)
        {
            if (tbCarId.Text == "") {
                MessageBox.Show("กรุณาเลือกรายการที่ต้องการลบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else {
                if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    //sql
                    OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                    pgCommand.CommandText = "DELETE FROM base_car WHERE รหัสรถร่วม = '" + tbCarId.Text + "' ; ";
                    try
                    {
                        dl.connect();
                        OdbcDataReader reader = pgCommand.ExecuteReader();
                        MessageBox.Show("ลบรายการเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        /*กรองตาม ทีมรถ*/
                        //this.base_carTableAdapter.Fill(this.baseCarDataSet.base_car);
                        //this.basecarBindingSource.Filter = string.Format("รหัสทีม = '" + tbCarTeamId.Text + "'");
                        setDataSouceForDGVCar();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    dl.close();

                    //clear ช่อง
                    clearTwoTextbox(tbCarId, tbCarName);
                }
            }
        }

        /*Base Mill*/
        private void setDataSouceForDGVMill()
        {
            try
            {
                dl.connect();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT *  FROM public.base_mill ");
                adtMill = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                dtMill = new DataTable();
                adtMill.Fill(dtMill);
                dgvMill.DataSource = dtMill;
            }
            catch (Exception)
            {
            }
            dl.close();
        }

        private void cbbCarTeamName_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbCarId.Text = "";
            tbCarName.Text = "";

            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT รหัสทีม FROM public.base_car_team where ชื่อทีม = '" + cbbCarTeamName.Text + "' ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string rdStr = reader["รหัสทีม"].ToString();
                    tbCarTeamId.Text = rdStr;
                }
            }
            catch (Exception)
            {
            }
            dl.close();

            /*กรองตาม ทีมรถ*/
            //this.basecarBindingSource.Filter = string.Format("รหัสทีม = '" + tbCarTeamId.Text + "'");
            setDataSouceForDGVCar();

            /*set ค่าที่มาจาก Table base_car_team*/
            setDataCarTeam();
        }

        private void btSaveMill_Click(object sender, EventArgs e)
        {
            saveAndUpdateDTGV(cmbMill, adtMill, dtMill, dgvMill, "base_mill");
        }

        private void btDelMill_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //baseapproveBindingSource.RemoveCurrent();
                //saveActionApprove();

                deleteDTGV(dgvMill, "รหัสโรงโม่", "base_mill");
            }
        }

        private void cbbCustomerSiteName_TextUpdate(object sender, EventArgs e)
        {
             setSearchAnywhereToCombobox(cbbCustomerSiteName, listOriginalCustomerNameSetting, listNewCustomerNameSetting);
        }

        private void setSearchAnywhereToCombobox(ComboBox cb, List<string> listOriginal, List<string> listNew)
        {
            //clear combobox
            cb.Items.Clear();
            //clear listNew
            listNew.Clear();

            try {
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
            catch (Exception) { 
            
            }

        }

        private void cbbCustomerSiteName_Leave(object sender, EventArgs e)
        {
            try {
                tbCustomerSiteId.Text = cbbCustomerSiteName.Text.Substring(0, cbbCustomerSiteName.Text.IndexOf(" : "));

                //ดึงเฉพาะหน้างานของลูกค้านั้นๆ
                setDataSouceForDGVSite();

                //set ค่าจากตารางใส่ในช่อง textbox
                setDataCustomerSite();  

            }
            catch (Exception)
            {
                tbCustomerSiteId.Text = "";
                cbbCustomerSiteName.Text = "";
            }

            /*
            if (cbbCustomerSiteName.Text != null && cbbCustomerSiteName.Text != "")
            {

                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT * FROM public.base_customer where ชื่อลูกค้า = '" + cbbCustomerSiteName.Text + "' ";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string rdStr = reader["รหัสลูกค้า"].ToString();
                        tbCustomerSiteId.Text = rdStr;
                    }

                    //sql รีเซตค่าหากหาข้อมูลไม่เจอ
                    if (!reader.HasRows)
                    {
                        MessageBox.Show("ไม่มีชื่อลูกค้า " + cbbCustomerSiteName.Text, "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        tbCustomerSiteId.Text = "";
                        cbbCustomerSiteName.Text = "";
                    }
                }
                catch (Exception)
                {
                }
                dl.close();

                //ดึงเฉพาะหน้างานของลูกค้านั้นๆ
                setDataSouceForDGVSite();

                //set ค่าจากตารางใส่ในช่อง textbox
                setDataCustomerSite();

            }
            else
            {
                tbCustomerSiteId.Text = "";
            }
            */

        }


        /*Base Site*/
        private void setDataSouceForDGVSite()
        {
            try
            {
                dl.connect();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT *  FROM public.base_site where base_customer_id = '" + tbCustomerSiteId.Text + "' ORDER BY base_site_id ");
                adtSite = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                dtSite = new DataTable();
                adtSite.Fill(dtSite);
                dgvSite.DataSource = dtSite;
            }
            catch (Exception)
            {
            }
            dl.close();
        }

        private void setDataCustomerSite()
        {
            if (dgvSite.Rows.Count > 1)
            {
                tbSiteId.Text = dgvSite.CurrentRow.Cells["base_site_id"].Value.ToString();
                tbSiteName.Text = dgvSite.CurrentRow.Cells["base_site_name"].Value.ToString();

                //เปิดปิดช่องเมื่อมีไม่มีค่า
                if (tbSiteId.Text != "")
                    tbSiteId.ReadOnly = true;
                else
                    tbSiteId.ReadOnly = false;
            }
            else {
                tbSiteId.Text = "";
                tbSiteName.Text = "";
                tbSiteId.ReadOnly = false;
            }
        }

        private void dgvSite_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            /*set ค่าที่มาจาก Table base_car_team*/
            if (cbbCustomerSiteName.SelectedIndex != -1)
                setDataCustomerSite();
        }

        private void btClearSite_Click(object sender, EventArgs e)
        {
            tbSiteId.Text = "";
            tbSiteId.ReadOnly = false;
            tbSiteName.Text = "";
        }

        private void btSaveSite_Click(object sender, EventArgs e)
        {
            Boolean isUpdate = false;
            if (tbCustomerSiteId.Text == "" || cbbCustomerSiteName.SelectedIndex == -1)
            {
                MessageBox.Show("กรุณาเลือกชื่อลูกค้า", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (tbSiteId.Text == "")
            {
                MessageBox.Show("กรุณาใส่รหัสหน้างาน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                //หาว่า id ซ้ำหรือไม่ ถ้าซ้ำ update ถ้าไม่มี insert
                //sql
                OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                pgCommand.CommandText = "SELECT base_site_id FROM public.base_site where base_site_id = '" + tbSiteId.Text + "' AND base_customer_id  = '" + tbCustomerSiteId.Text + "'";
                try
                {
                    dl.connect();
                    OdbcDataReader reader = pgCommand.ExecuteReader();
                    isUpdate = reader.Read();
                }
                catch (Exception)
                {
                }
                dl.close();

                //update or save
                if (isUpdate && tbSiteId.ReadOnly)
                    updateBaseSiteAction();
                else
                    saveBaseSiteAction();

            }
        }

        private void updateBaseSiteAction()
        {
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "UPDATE base_site SET base_site_name = '" + tbSiteName.Text + "' WHERE base_site_id = '" + tbSiteId.Text + "' ; ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                /*กรองตาม ทีมรถ*/
                //this.base_carTableAdapter.Fill(this.baseCarDataSet.base_car);
                //this.basecarBindingSource.Filter = string.Format("รหัสทีม = '" + tbCarTeamId.Text + "'");
                setDataSouceForDGVSite();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            dl.close();
        }

        private void saveBaseSiteAction()
        {
            //sql
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "INSERT INTO base_site (base_site_id, base_site_name, base_customer_id)" +
                                     "VALUES ('" + tbSiteId.Text + "','" + tbSiteName.Text + "','" + tbCustomerSiteId.Text + "' )";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                MessageBox.Show("บันทึกเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                /*กรองตาม ทีมรถ*/
                //this.base_carTableAdapter.Fill(this.baseCarDataSet.base_car);
                //this.basecarBindingSource.Filter = string.Format("รหัสทีม = '" + tbCarTeamId.Text + "'");
                setDataSouceForDGVSite();
            }
            catch (Exception ex)
            {
                MessageBox.Show("รหัสหน้างานนี้มีอยู่แล้ว กรุณาเปลี่ยนรหัสหน้างานใหม่", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            dl.close();
        }

        /*Base Job Type*/
        private void setDataSouceForDGVJobType()
        {
            try
            {
                dl.connect();
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT *  FROM public.base_job_type order by base_job_type_id ");
                adtJobType = new OdbcDataAdapter(sql.ToString(), dl.sqlConn());
                dtJobType = new DataTable();
                adtJobType.Fill(dtJobType);
                dgvJobType.DataSource = dtJobType;
            }
            catch (Exception)
            {
            }
            dl.close();
        }

        private void btDelSite_Click(object sender, EventArgs e)
        {
            if (tbSiteId.Text == "")
            {
                MessageBox.Show("กรุณาเลือกรายการที่ต้องการลบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    //sql
                    OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
                    pgCommand.CommandText = "DELETE FROM base_site WHERE base_site_id = '" + tbSiteId.Text + "' ; ";
                    try
                    {
                        dl.connect();
                        OdbcDataReader reader = pgCommand.ExecuteReader();
                        MessageBox.Show("ลบรายการเรียบร้อย", "บันทึก", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        /*กรองตาม ทีมรถ*/
                        //this.base_carTableAdapter.Fill(this.baseCarDataSet.base_car);
                        //this.basecarBindingSource.Filter = string.Format("รหัสทีม = '" + tbCarTeamId.Text + "'");
                        setDataSouceForDGVSite();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    dl.close();

                    //clear ช่อง
                    clearTwoTextbox(tbSiteId, tbSiteName);
                }
            }
        }

        private void btSaveJobType_Click(object sender, EventArgs e)
        {
            saveAndUpdateDTGV(cmbJobType, adtJobType, dtJobType, dgvJobType, "base_job_type");
        }

        private void btDelJobType_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("ต้องการลบรายการนี้ใช่หรือไม่", "แจ้งเตือน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //baseapproveBindingSource.RemoveCurrent();
                //saveActionApprove();

                deleteDTGV(dgvJobType, "base_job_type_id", "base_job_type");
            }
        }

        private void fillComboAll(ComboBox cbb, String tableName, String fieldName)
        {
            //ล้างก่อน
            cbb.Items.Clear();
            //เพิ่ม combobox
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT " + fieldName + " FROM public." + tableName + "";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    string des = reader[fieldName].ToString();
                    cbb.Items.Add(des);
                }
            }
            catch (Exception)
            {

            }
            dl.close();
        }

        private void cbbVatType_SelectedIndexChanged(object sender, EventArgs e)
        {
            setDataSouceForDGVCustomer();
            setDataCustomer();
        }

        private void cbbJobType_SelectedIndexChanged(object sender, EventArgs e)
        {
            setDataSouceForDGVCustomer();
            setDataCustomer();
            getJobTypeIdFromName();
        }

        private void setDataCustomer()
        {
            if (dgvCustomer.Rows.Count > 1)
            {
                tbCustomerId.Text = dgvCustomer.CurrentRow.Cells["รหัสลูกค้า"].Value.ToString();
                tbCustomerName.Text = dgvCustomer.CurrentRow.Cells["ชื่อลูกค้า"].Value.ToString();
                tbCustomerAddress.Text = dgvCustomer.CurrentRow.Cells["ที่อยู่"].Value.ToString();

                //เปิดปิดช่องเมื่อมีไม่มีค่า
                if (tbCustomerId.Text != "")
                    tbCustomerId.ReadOnly = true;
                else
                    tbCustomerId.ReadOnly = false;
            }else
            {
                tbCustomerId.Text = "";
                tbCustomerName.Text = "";
                tbCustomerAddress.Text = "";
                tbCustomerId.ReadOnly = false;
            }
        }

        private void getJobTypeIdFromName()
        {
            OdbcCommand pgCommand = (OdbcCommand)dl.sqlConn().CreateCommand();
            pgCommand.CommandText = "SELECT base_job_type_id FROM public.base_job_type WHERE base_job_type_name = '" + cbbJobType.Text + "' ";
            try
            {
                dl.connect();
                OdbcDataReader reader = pgCommand.ExecuteReader();
                while (reader.Read())
                {
                    tbJobId.Text = reader["base_job_type_id"].ToString();
                }
            }
            catch (Exception)
            {

            }
            dl.close();
        }

        private void dgvCustomer_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            /*set ค่าที่มาจาก Table base_customer*/
            if (cbbVatType.SelectedIndex != -1 && cbbJobType.SelectedIndex != -1)
                setDataCustomer();
        }

        private void btClearCustomer_Click(object sender, EventArgs e)
        {
            tbCustomerId.Text = "";
            tbCustomerId.ReadOnly = false;
            tbCustomerName.Text = "";
            tbCustomerAddress.Text = "";
        }

        /* start clearThreeTextbox */
        private void clearThreeTextbox(TextBox tbId, TextBox tbFirst, TextBox tbSecond)
        {
            tbId.Text = "";
            tbId.ReadOnly = false;
            tbFirst.Text = "";
            tbSecond.Text = "";
        }
        /* end clearThreeTextbox */

        /* start clearThreeTextbox */
        private void clearTwoTextbox(TextBox tbId, TextBox tbFirst)
        {
            tbId.Text = "";
            tbId.ReadOnly = false;
            tbFirst.Text = "";
        }
        /* end clearThreeTextbox */

        // ---- ย้ายมาจาก ucBackup และ ucHelp : ส่วนตั้งค่าทั้งหมดมารวมที่แท็บ Setting ----
        private const string DefaultPgDumpPath = @"C:\Program Files\PostgreSQL\9.5\bin\pg_dump.exe";
        private const string DefaultBackupDir = @"D:\backupSqlNew";
        // ตารางเวลา backup อัตโนมัติ: ทุก 2 ชั่วโมง เริ่ม/สิ้นสุดตามค่าที่ผู้มีสิทธิ์ isPermissionAddSetting ตั้งไว้ (ค่า default 09:00 - 17:00)
        private const int DefaultAutoBackupStartHour = 9;
        private const int DefaultAutoBackupEndHour = 17;
        // เทียบกับ processs_delivery_order() ใน AU_weight_to_local.py
        // ตั้งค่าตรงกับ backupSql_m.bat ใน C:\Users\Userpc\Documents\backupSqlNew\script\ แต่รันผ่าน pg_dump.exe โดยตรงจาก C# แทนการเรียก .bat
        // พารามิเตอร์การเชื่อมต่อ (host/port/database/user/password) อ่านจาก ODBC DSN "PostgreSQLM" เดียวกับที่ Datalayer ใช้ แทนการฝังค่าตายตัว
        private const string OdbcDsnName = "PostgreSQLM";
        // Program Files (ที่ติดตั้งโปรแกรม) เขียนไฟล์ไม่ได้ถ้าไม่ใช่ admin จึงเก็บ config/log ไว้ใน AppData ของผู้ใช้แทน
        // AppDataDir is per-build (see Utils.AppDataDir) so Blue and Pink never share the same config file.
        private static readonly string AppDataDir = Utils.AppDataDir;
        private static readonly string BackupConfigPath =
            System.IO.Path.Combine(AppDataDir, "configs_backup.txt");
        private System.Windows.Forms.Timer autoBackupTimer;
        private string lastAutoBackupKey = "";

        // เฉพาะ user ที่มีสิทธิ์ add_setting เท่านั้นที่แก้ไข/บันทึกการตั้งค่า backup ได้ user อื่นดูได้อย่างเดียว
        private void ApplyBackupConfigPermission()
        {
            bool canEdit = Globals.isPermissionAddSetting();

            tbPgDumpPath.ReadOnly = !canEdit;
            tbBackupDir.ReadOnly = !canEdit;
            btnBrowsePgDump.Enabled = canEdit;
            btnBrowseBackupDir.Enabled = canEdit;
            btnSaveBackupConfig.Enabled = canEdit;
            chkAutoBackup.Enabled = canEdit;
            dtpAutoBackupStart.Enabled = canEdit;
            dtpAutoBackupEnd.Enabled = canEdit;
        }
        // โหลดค่า PgDumpPath / BackupDir / AutoBackupEnabled จาก configs_backup.txt (key=value ต่อบรรทัด) ถ้าไม่มีไฟล์ใช้ค่า default
        private void LoadBackupConfig()
        {
            string pgDumpPath = DefaultPgDumpPath;
            string backupDir = DefaultBackupDir;
            bool autoBackupEnabled = true;
            int startHour = DefaultAutoBackupStartHour;
            int endHour = DefaultAutoBackupEndHour;

            if (System.IO.File.Exists(BackupConfigPath))
            {
                foreach (string line in System.IO.File.ReadAllLines(BackupConfigPath))
                {
                    int idx = line.IndexOf('=');
                    if (idx <= 0)
                        continue;

                    string key = line.Substring(0, idx).Trim();
                    string value = line.Substring(idx + 1).Trim();

                    if (key == "PgDumpPath")
                        pgDumpPath = value;
                    else if (key == "BackupDir")
                        backupDir = value;
                    else if (key == "AutoBackupEnabled")
                        bool.TryParse(value, out autoBackupEnabled);
                    else if (key == "AutoBackupStartHour")
                        int.TryParse(value, out startHour);
                    else if (key == "AutoBackupEndHour")
                        int.TryParse(value, out endHour);
                }
            }

            tbPgDumpPath.Text = pgDumpPath;
            tbBackupDir.Text = backupDir;
            chkAutoBackup.Checked = autoBackupEnabled;
            dtpAutoBackupStart.Value = DateTime.Today.AddHours(Clamp(startHour, 0, 23));
            dtpAutoBackupEnd.Value = DateTime.Today.AddHours(Clamp(endHour, 0, 23));
        }
        private void SaveBackupConfig()
        {
            string[] lines =
            {
                "PgDumpPath=" + tbPgDumpPath.Text.Trim(),
                "BackupDir=" + tbBackupDir.Text.Trim(),
                "AutoBackupEnabled=" + chkAutoBackup.Checked,
                "AutoBackupStartHour=" + dtpAutoBackupStart.Value.Hour,
                "AutoBackupEndHour=" + dtpAutoBackupEnd.Value.Hour,
            };
            if (!System.IO.Directory.Exists(AppDataDir))
                System.IO.Directory.CreateDirectory(AppDataDir);
            System.IO.File.WriteAllLines(BackupConfigPath, lines);
        }
        private void btnBrowsePgDump_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "pg_dump.exe|pg_dump.exe|Executable files (*.exe)|*.exe|All files (*.*)|*.*";
                dlg.FileName = "pg_dump.exe";
                if (System.IO.File.Exists(tbPgDumpPath.Text))
                    dlg.InitialDirectory = System.IO.Path.GetDirectoryName(tbPgDumpPath.Text);

                if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                    tbPgDumpPath.Text = dlg.FileName;
            }
        }
        private void btnBrowseBackupDir_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                if (System.IO.Directory.Exists(tbBackupDir.Text))
                    dlg.SelectedPath = tbBackupDir.Text;

                if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                    tbBackupDir.Text = dlg.SelectedPath;
            }
        }
        private void btnSaveBackupConfig_Click(object sender, EventArgs e)
        {
            if (!Globals.isPermissionAddSetting())
            {
                MessageBox.Show("คุณไม่มีสิทธิ์บันทึกการตั้งค่านี้", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(tbPgDumpPath.Text) || string.IsNullOrWhiteSpace(tbBackupDir.Text))
            {
                MessageBox.Show("กรุณาระบุ pg_dump.exe และ Backup Folder", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dtpAutoBackupStart.Value.TimeOfDay >= dtpAutoBackupEnd.Value.TimeOfDay)
            {
                MessageBox.Show("เวลาเริ่ม Auto Backup ต้องน้อยกว่าเวลาสิ้นสุด", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                SaveBackupConfig();
                MessageBox.Show("บันทึกการตั้งค่าสำเร็จ", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("บันทึกการตั้งค่าไม่สำเร็จ: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private IEnumerable<int> GetAutoBackupHours()
        {
            int startHour = dtpAutoBackupStart.Value.Hour;
            int endHour = dtpAutoBackupEnd.Value.Hour;
            for (int hour = startHour; hour <= endHour; hour += 2)
                yield return hour;
        }
        private void InitAutoBackupTimer()
        {
            autoBackupTimer = new System.Windows.Forms.Timer { Interval = 60000 };
            autoBackupTimer.Tick += AutoBackupTimer_Tick;
            autoBackupTimer.Start();

            this.Disposed += (s, e) =>
            {
                autoBackupTimer.Stop();
                autoBackupTimer.Dispose();
            };
        }
        private async void AutoBackupTimer_Tick(object sender, EventArgs e)
        {
            if (!chkAutoBackup.Checked)
                return;

            DateTime now = DateTime.Now;
            if (now.Minute != 0 || !GetAutoBackupHours().Contains(now.Hour))
                return;

            string key = now.ToString("yyyyMMdd_HH");
            if (key == lastAutoBackupKey)
                return;
            lastAutoBackupKey = key;

            if (!btnBackup.Enabled)
                return; // มีการ backup ทำงานอยู่แล้ว (ผู้ใช้กดเอง หรือรอบก่อนหน้ายังไม่เสร็จ)

            await DoBackupAsync(isAuto: true);
        }
        private async void btnBackup_Click(object sender, EventArgs e)
        {
            await DoBackupAsync(isAuto: false);
        }
        // ใช้ร่วมกันทั้งกดปุ่ม backup เองและ auto backup ตามตารางเวลา
        // isAuto = true จะทำงานเบื้องหลังทั้งหมด ไม่เปิดหน้าต่างและไม่เด้ง MessageBox ให้เห็น (log ลงไฟล์แทน)
        private async Task DoBackupAsync(bool isAuto)
        {
            btnBackup.Enabled = false;

            frmDownloadProgress progress = null;
            Action<string> log;
            if (isAuto)
            {
                log = LogToFile;
            }
            else
            {
                progress = new frmDownloadProgress();
                progress.Show(FindForm());
                log = progress.Log;
            }

            try
            {
                string pgDumpPath = tbPgDumpPath.Text.Trim();
                string backupDir = tbBackupDir.Text.Trim();

                if (string.IsNullOrEmpty(pgDumpPath) || !System.IO.File.Exists(pgDumpPath))
                {
                    log($"ไม่พบไฟล์ {pgDumpPath}");
                    if (!isAuto)
                        MessageBox.Show($"ไม่พบไฟล์ {pgDumpPath}", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(backupDir))
                {
                    log("กรุณาระบุ Backup Folder");
                    if (!isAuto)
                        MessageBox.Show("กรุณาระบุ Backup Folder", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                PgDsnInfo dsn = GetPgDsnInfo(OdbcDsnName);
                if (dsn == null || string.IsNullOrEmpty(dsn.Host) || string.IsNullOrEmpty(dsn.Database))
                {
                    log($"ไม่พบการตั้งค่า ODBC DSN \"{OdbcDsnName}\"");
                    if (!isAuto)
                        MessageBox.Show($"ไม่พบการตั้งค่า ODBC DSN \"{OdbcDsnName}\"", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!System.IO.Directory.Exists(backupDir))
                    System.IO.Directory.CreateDirectory(backupDir);

                string dateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFile = System.IO.Path.Combine(backupDir, $"{dsn.Database}_{dateTime}.backup");

                log("===========================================");
                log(isAuto ? "Auto Backup" : "Manual Backup");
                log($"Backup Database : {dsn.Database}");
                log($"Output File     : {backupFile}");
                log("===========================================");

                int exitCode = await RunPgDumpAsync(pgDumpPath, dsn, backupFile, log);

                if (exitCode == 0)
                {
                    log("===== Backup Success =====");
                    log(backupFile);
                    lbLastAutoBackup.Text = $"Backup ล่าสุด: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ({(isAuto ? "อัตโนมัติ" : "manual")}) สำเร็จ";
                    if (!isAuto)
                        MessageBox.Show("สำรองข้อมูลสำเร็จ\r\n" + backupFile, "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    log($"XXXXX Backup Failed (exit code {exitCode}) XXXXX");
                    lbLastAutoBackup.Text = $"Backup ล่าสุด: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ({(isAuto ? "อัตโนมัติ" : "manual")}) ไม่สำเร็จ";
                    if (!isAuto)
                        MessageBox.Show($"สำรองข้อมูลไม่สำเร็จ (exit code {exitCode})", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                log("Error: " + ex.Message);
                lbLastAutoBackup.Text = $"Backup ล่าสุด: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ({(isAuto ? "อัตโนมัติ" : "manual")}) เกิดข้อผิดพลาด";
                if (!isAuto)
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (progress != null)
                {
                    progress.AllowClose();
                    progress.Close();
                }
                btnBackup.Enabled = true;
            }
        }
        // เทียบกับคำสั่ง pg_dump ใน backupSql_m.bat: -h -p -U -F c -b -v -f <file> <database>
        private Task<int> RunPgDumpAsync(string pgDumpPath, PgDsnInfo dsn, string backupFile, Action<string> log)
        {
            var tcs = new TaskCompletionSource<int>();

            string port = string.IsNullOrEmpty(dsn.Port) ? "5432" : dsn.Port;

            var psi = new ProcessStartInfo
            {
                FileName = pgDumpPath,
                Arguments = $"-h {dsn.Host} -p {port} -U {dsn.Username} -F c -b -v -f \"{backupFile}\" {dsn.Database}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            psi.EnvironmentVariables["PGPASSWORD"] = dsn.Password ?? "";

            var process = new Process { StartInfo = psi, EnableRaisingEvents = true };

            process.OutputDataReceived += (s, ev) =>
            {
                if (!string.IsNullOrEmpty(ev.Data))
                    log(ev.Data);
            };
            process.ErrorDataReceived += (s, ev) =>
            {
                if (!string.IsNullOrEmpty(ev.Data))
                    log(ev.Data);
            };
            process.Exited += (s, ev) =>
            {
                tcs.TrySetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return tcs.Task;
        }
        private static PgDsnInfo GetPgDsnInfo(string dsnName)
        {
            string[] roots =
            {
                $@"SOFTWARE\ODBC\ODBC.INI\{dsnName}",
                $@"SOFTWARE\WOW6432Node\ODBC\ODBC.INI\{dsnName}",
            };

            foreach (var hive in new[] { Microsoft.Win32.Registry.CurrentUser, Microsoft.Win32.Registry.LocalMachine })
            {
                foreach (var subKey in roots)
                {
                    using (var key = hive.OpenSubKey(subKey))
                    {
                        if (key == null)
                            continue;

                        return new PgDsnInfo
                        {
                            Host = key.GetValue("Servername")?.ToString(),
                            Port = key.GetValue("Port")?.ToString(),
                            Database = key.GetValue("Database")?.ToString(),
                            Username = key.GetValue("Username")?.ToString() ?? key.GetValue("UID")?.ToString(),
                            Password = key.GetValue("Password")?.ToString(),
                        };
                    }
                }
            }

            return null;
        }
        // auto backup ไม่มีหน้าต่างให้เห็น เขียน log ลงไฟล์แทนไว้ตรวจสอบย้อนหลัง
        private static void LogToFile(string message)
        {
            try
            {
                if (!System.IO.Directory.Exists(AppDataDir))
                    System.IO.Directory.CreateDirectory(AppDataDir);
                System.IO.File.AppendAllText(AutoBackupLogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\r\n");
            }
            catch (Exception)
            {
                // ไม่ทำให้ auto backup ล้มเหลวเพียงเพราะเขียน log ไม่ได้
            }
        }
        private static int Clamp(int value, int min, int max)
        {
            return value < min ? min : (value > max ? max : value);
        }

        // เติมรายการแบบใบชั่งลงคอมโบ แล้วเลือกค่าที่บันทึกไว้ใน config_reportmain.txt
        // ถ้าไฟล์หายหรือค่าใช้ไม่ได้ ReportMainTemplate จะคืนค่าเริ่มต้น (Template 1) ให้เอง
        private void LoadReportTemplateSetting()
        {
            cboReportTemplate.Items.Clear();
            cboReportTemplate.Items.AddRange(ReportMainTemplate.All);

            int current = ReportMainTemplate.GetSelectedTemplateNumber();
            for (int i = 0; i < cboReportTemplate.Items.Count; i++)
            {
                ReportMainTemplate.TemplateInfo item =
                    cboReportTemplate.Items[i] as ReportMainTemplate.TemplateInfo;
                if (item != null && item.Number == current)
                {
                    cboReportTemplate.SelectedIndex = i;
                    return;
                }
            }

            if (cboReportTemplate.Items.Count > 0)
                cboReportTemplate.SelectedIndex = 0;
        }
        private void btnSaveReportTemplate_Click(object sender, EventArgs e)
        {
            // ใช้สิทธิ์ชุดเดียวกับการบันทึกตั้งค่าอื่นในหน้านี้
            if (!Globals.isPermissionAddSetting())
            {
                MessageBox.Show("คุณไม่มีสิทธิ์บันทึกการตั้งค่านี้", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ReportMainTemplate.TemplateInfo selected =
                cboReportTemplate.SelectedItem as ReportMainTemplate.TemplateInfo;
            if (selected == null)
            {
                MessageBox.Show("กรุณาเลือกแบบใบชั่ง", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (ReportMainTemplate.SaveSelectedTemplate(selected.Number))
            {
                MessageBox.Show("บันทึกแบบใบชั่งสำเร็จ: " + selected.DisplayName, "แบบใบชั่ง",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("บันทึกแบบใบชั่งไม่สำเร็จ กรุณาตรวจสอบสิทธิ์การเขียนไฟล์" + Environment.NewLine
                    + ReportMainTemplate.ConfigFilePath, "แบบใบชั่ง", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        // ตอน constructor ทำงานยังไม่ผ่าน Login จึงเช็คสิทธิ์ใหม่ทุกครั้งที่แสดงหน้านี้
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
                ApplyBackupConfigPermission();
        }
        private static readonly string AutoBackupLogPath =
            System.IO.Path.Combine(AppDataDir, "backup_auto.log");

        private class PgDsnInfo
        {
            public string Host;
            public string Port;
            public string Database;
            public string Username;
            public string Password;
        }


    }
}
