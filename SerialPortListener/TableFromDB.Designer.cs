namespace SerialPortListener
{
    partial class TableFromDB
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lbTotal = new System.Windows.Forms.Label();
            this.dateFrom = new System.Windows.Forms.DateTimePicker();
            this.dateTo = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cbbSearchWeight = new System.Windows.Forms.ComboBox();
            this.tableDataFromDB = new System.Windows.Forms.DataGridView();
            this.วันที่ = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.เลขที่เอกสาร = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.รหัสลูกค้า = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ลูกค้า = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.หน้างาน = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ทะเบียนรถ = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.จังหวัด = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.คนขับ = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ทีม = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.น้ำหนักรถ = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.น้ำหนักรวม = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.น้ำหนักสินค้า = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.คิว = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.โรงโม่ = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ชนิดหิน = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ประเภทหิน = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ราคาตัน = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.จำนวณเงิน = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.vat = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.จำนวนเงินสุทธิ = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.จ่ายเงิน = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.วันที่ชั่งเข้า = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.เวลาชั่งเข้า = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.วันที่ชั่งออก = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.เวลาชั่งออก = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.รหัสผู้ชั่ง = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ชื่อผู้ชั่ง = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.รหัสผู้ตัก = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ชื่อผู้ตัก = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ค่าขนส่ง = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.weight_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ชนิดvat = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ล้าง = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ขนส่ง = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.หมายเหตุ = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.oil_content = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.weightBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.truckDataSet = new SerialPortListener.truckDataSet();
            this.label3 = new System.Windows.Forms.Label();
            this.btSearch = new System.Windows.Forms.Button();
            this.btClose = new System.Windows.Forms.Button();
            this.btDelete = new System.Windows.Forms.Button();
            this.btUpdate = new System.Windows.Forms.Button();
            this.btAdd = new System.Windows.Forms.Button();
            this.truckDataSetBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.weightTableAdapter = new SerialPortListener.truckDataSetTableAdapters.weightTableAdapter();
            this.btExport = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.tableDataFromDB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.weightBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.truckDataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.truckDataSetBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // lbTotal
            // 
            this.lbTotal.AutoSize = true;
            this.lbTotal.Location = new System.Drawing.Point(0, 0);
            this.lbTotal.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lbTotal.Name = "lbTotal";
            this.lbTotal.Size = new System.Drawing.Size(101, 20);
            this.lbTotal.TabIndex = 3;
            this.lbTotal.Text = "Total rows : 0";
            this.lbTotal.Visible = false;
            // 
            // dateFrom
            // 
            this.dateFrom.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.dateFrom.CalendarFont = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateFrom.CustomFormat = "dd-MM-yyyy";
            this.dateFrom.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateFrom.Location = new System.Drawing.Point(731, 13);
            this.dateFrom.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dateFrom.Name = "dateFrom";
            this.dateFrom.Size = new System.Drawing.Size(127, 27);
            this.dateFrom.TabIndex = 9;
            // 
            // dateTo
            // 
            this.dateTo.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.dateTo.CalendarFont = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateTo.CustomFormat = "dd-MM-yyyy";
            this.dateTo.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTo.Location = new System.Drawing.Point(924, 13);
            this.dateTo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dateTo.Name = "dateTo";
            this.dateTo.Size = new System.Drawing.Size(127, 27);
            this.dateTo.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(661, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 21);
            this.label1.TabIndex = 11;
            this.label1.Text = "From :";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(877, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 21);
            this.label2.TabIndex = 12;
            this.label2.Text = "To :";
            // 
            // cbbSearchWeight
            // 
            this.cbbSearchWeight.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cbbSearchWeight.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbbSearchWeight.FormattingEnabled = true;
            this.cbbSearchWeight.Items.AddRange(new object[] {
            "ทั้งหมด",
            "ยังไม่ได้ชั่งออก"});
            this.cbbSearchWeight.Location = new System.Drawing.Point(484, 13);
            this.cbbSearchWeight.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbbSearchWeight.Name = "cbbSearchWeight";
            this.cbbSearchWeight.Size = new System.Drawing.Size(151, 29);
            this.cbbSearchWeight.TabIndex = 14;
            // 
            // tableDataFromDB
            // 
            this.tableDataFromDB.AutoGenerateColumns = false;
            this.tableDataFromDB.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.tableDataFromDB.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tableDataFromDB.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.วันที่,
            this.เลขที่เอกสาร,
            this.รหัสลูกค้า,
            this.ลูกค้า,
            this.หน้างาน,
            this.ทะเบียนรถ,
            this.จังหวัด,
            this.คนขับ,
            this.ทีม,
            this.น้ำหนักรถ,
            this.น้ำหนักรวม,
            this.น้ำหนักสินค้า,
            this.คิว,
            this.โรงโม่,
            this.ชนิดหิน,
            this.ประเภทหิน,
            this.ราคาตัน,
            this.จำนวณเงิน,
            this.vat,
            this.จำนวนเงินสุทธิ,
            this.จ่ายเงิน,
            this.วันที่ชั่งเข้า,
            this.เวลาชั่งเข้า,
            this.วันที่ชั่งออก,
            this.เวลาชั่งออก,
            this.รหัสผู้ชั่ง,
            this.ชื่อผู้ชั่ง,
            this.รหัสผู้ตัก,
            this.ชื่อผู้ตัก,
            this.ค่าขนส่ง,
            this.weight_id,
            this.ชนิดvat,
            this.ล้าง,
            this.ขนส่ง,
            this.หมายเหตุ,
            this.oil_content});
            this.tableDataFromDB.DataSource = this.weightBindingSource;
            this.tableDataFromDB.Location = new System.Drawing.Point(12, 60);
            this.tableDataFromDB.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tableDataFromDB.Name = "tableDataFromDB";
            this.tableDataFromDB.ReadOnly = true;
            this.tableDataFromDB.Size = new System.Drawing.Size(1153, 499);
            this.tableDataFromDB.TabIndex = 15;
            this.tableDataFromDB.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.tableDataFromDB_CellDoubleClick);
            // 
            // วันที่
            // 
            this.วันที่.DataPropertyName = "วันที่";
            this.วันที่.FillWeight = 279.2793F;
            this.วันที่.HeaderText = "วันที่";
            this.วันที่.Name = "วันที่";
            this.วันที่.ReadOnly = true;
            this.วันที่.Width = 59;
            // 
            // เลขที่เอกสาร
            // 
            this.เลขที่เอกสาร.DataPropertyName = "เลขที่เอกสาร";
            this.เลขที่เอกสาร.FillWeight = 254.849F;
            this.เลขที่เอกสาร.HeaderText = "เลขที่การชั่ง";
            this.เลขที่เอกสาร.Name = "เลขที่เอกสาร";
            this.เลขที่เอกสาร.ReadOnly = true;
            this.เลขที่เอกสาร.Width = 101;
            // 
            // รหัสลูกค้า
            // 
            this.รหัสลูกค้า.DataPropertyName = "รหัสลูกค้า";
            this.รหัสลูกค้า.FillWeight = 232.6197F;
            this.รหัสลูกค้า.HeaderText = "รหัสลูกค้า";
            this.รหัสลูกค้า.Name = "รหัสลูกค้า";
            this.รหัสลูกค้า.ReadOnly = true;
            this.รหัสลูกค้า.Width = 90;
            // 
            // ลูกค้า
            // 
            this.ลูกค้า.DataPropertyName = "ลูกค้า";
            this.ลูกค้า.FillWeight = 212.393F;
            this.ลูกค้า.HeaderText = "ลูกค้า";
            this.ลูกค้า.Name = "ลูกค้า";
            this.ลูกค้า.ReadOnly = true;
            this.ลูกค้า.Width = 67;
            // 
            // หน้างาน
            // 
            this.หน้างาน.DataPropertyName = "หน้างาน";
            this.หน้างาน.FillWeight = 87.5784F;
            this.หน้างาน.HeaderText = "หน้างาน";
            this.หน้างาน.Name = "หน้างาน";
            this.หน้างาน.ReadOnly = true;
            this.หน้างาน.Width = 81;
            // 
            // ทะเบียนรถ
            // 
            this.ทะเบียนรถ.DataPropertyName = "ทะเบียนรถ";
            this.ทะเบียนรถ.FillWeight = 193.9885F;
            this.ทะเบียนรถ.HeaderText = "ทะเบียนรถ";
            this.ทะเบียนรถ.Name = "ทะเบียนรถ";
            this.ทะเบียนรถ.ReadOnly = true;
            this.ทะเบียนรถ.Width = 93;
            // 
            // จังหวัด
            // 
            this.จังหวัด.DataPropertyName = "จังหวัด";
            this.จังหวัด.FillWeight = 177.2422F;
            this.จังหวัด.HeaderText = "จังหวัด";
            this.จังหวัด.Name = "จังหวัด";
            this.จังหวัด.ReadOnly = true;
            this.จังหวัด.Width = 73;
            // 
            // คนขับ
            // 
            this.คนขับ.DataPropertyName = "คนขับ";
            this.คนขับ.FillWeight = 162.0045F;
            this.คนขับ.HeaderText = "คนขับ";
            this.คนขับ.Name = "คนขับ";
            this.คนขับ.ReadOnly = true;
            this.คนขับ.Width = 70;
            // 
            // ทีม
            // 
            this.ทีม.DataPropertyName = "ทีม";
            this.ทีม.FillWeight = 148.1395F;
            this.ทีม.HeaderText = "ทีม";
            this.ทีม.Name = "ทีม";
            this.ทีม.ReadOnly = true;
            this.ทีม.Width = 51;
            // 
            // น้ำหนักรถ
            // 
            this.น้ำหนักรถ.DataPropertyName = "น้ำหนักรถ";
            this.น้ำหนักรถ.FillWeight = 135.5236F;
            this.น้ำหนักรถ.HeaderText = "น้ำหนักเข้า";
            this.น้ำหนักรถ.Name = "น้ำหนักรถ";
            this.น้ำหนักรถ.ReadOnly = true;
            this.น้ำหนักรถ.Width = 97;
            // 
            // น้ำหนักรวม
            // 
            this.น้ำหนักรวม.DataPropertyName = "น้ำหนักรวม";
            this.น้ำหนักรวม.FillWeight = 124.0443F;
            this.น้ำหนักรวม.HeaderText = "น้ำหนักออก";
            this.น้ำหนักรวม.Name = "น้ำหนักรวม";
            this.น้ำหนักรวม.ReadOnly = true;
            this.น้ำหนักรวม.Width = 102;
            // 
            // น้ำหนักสินค้า
            // 
            this.น้ำหนักสินค้า.DataPropertyName = "น้ำหนักสินค้า";
            this.น้ำหนักสินค้า.FillWeight = 113.5992F;
            this.น้ำหนักสินค้า.HeaderText = "น้ำหนักสุทธิ";
            this.น้ำหนักสินค้า.Name = "น้ำหนักสินค้า";
            this.น้ำหนักสินค้า.ReadOnly = true;
            this.น้ำหนักสินค้า.Width = 102;
            // 
            // คิว
            // 
            this.คิว.DataPropertyName = "คิว";
            this.คิว.FillWeight = 104.0951F;
            this.คิว.HeaderText = "น้ำหนักคิว";
            this.คิว.Name = "คิว";
            this.คิว.ReadOnly = true;
            this.คิว.Width = 93;
            // 
            // โรงโม่
            // 
            this.โรงโม่.DataPropertyName = "โรงโม่";
            this.โรงโม่.FillWeight = 95.44719F;
            this.โรงโม่.HeaderText = "โรงโม่";
            this.โรงโม่.Name = "โรงโม่";
            this.โรงโม่.ReadOnly = true;
            this.โรงโม่.Width = 64;
            // 
            // ชนิดหิน
            // 
            this.ชนิดหิน.DataPropertyName = "ชนิดหิน";
            this.ชนิดหิน.FillWeight = 80.41848F;
            this.ชนิดหิน.HeaderText = "ชนิดหิน";
            this.ชนิดหิน.Name = "ชนิดหิน";
            this.ชนิดหิน.ReadOnly = true;
            this.ชนิดหิน.Width = 79;
            // 
            // ประเภทหิน
            // 
            this.ประเภทหิน.DataPropertyName = "ประเภทหิน";
            this.ประเภทหิน.FillWeight = 73.90363F;
            this.ประเภทหิน.HeaderText = "ประเภทหิน";
            this.ประเภทหิน.Name = "ประเภทหิน";
            this.ประเภทหิน.ReadOnly = true;
            this.ประเภทหิน.Width = 94;
            // 
            // ราคาตัน
            // 
            this.ราคาตัน.DataPropertyName = "ราคาตัน";
            this.ราคาตัน.FillWeight = 67.97569F;
            this.ราคาตัน.HeaderText = "ราคา/ตัน";
            this.ราคาตัน.Name = "ราคาตัน";
            this.ราคาตัน.ReadOnly = true;
            this.ราคาตัน.Width = 87;
            // 
            // จำนวณเงิน
            // 
            this.จำนวณเงิน.DataPropertyName = "จำนวณเงิน";
            this.จำนวณเงิน.FillWeight = 62.58178F;
            this.จำนวณเงิน.HeaderText = "จำนวนเงิน";
            this.จำนวณเงิน.Name = "จำนวณเงิน";
            this.จำนวณเงิน.ReadOnly = true;
            this.จำนวณเงิน.Width = 93;
            // 
            // vat
            // 
            this.vat.DataPropertyName = "vat";
            this.vat.FillWeight = 57.67384F;
            this.vat.HeaderText = "ภาษี";
            this.vat.Name = "vat";
            this.vat.ReadOnly = true;
            this.vat.Width = 59;
            // 
            // จำนวนเงินสุทธิ
            // 
            this.จำนวนเงินสุทธิ.DataPropertyName = "จำนวนเงินสุทธิ";
            this.จำนวนเงินสุทธิ.FillWeight = 53.20803F;
            this.จำนวนเงินสุทธิ.HeaderText = "จำนวนเงินรวมภาษี";
            this.จำนวนเงินสุทธิ.Name = "จำนวนเงินสุทธิ";
            this.จำนวนเงินสุทธิ.ReadOnly = true;
            this.จำนวนเงินสุทธิ.Width = 139;
            // 
            // จ่ายเงิน
            // 
            this.จ่ายเงิน.DataPropertyName = "จ่ายเงิน";
            this.จ่ายเงิน.FillWeight = 49.14456F;
            this.จ่ายเงิน.HeaderText = "จ่ายเงิน";
            this.จ่ายเงิน.Name = "จ่ายเงิน";
            this.จ่ายเงิน.ReadOnly = true;
            this.จ่ายเงิน.Width = 76;
            // 
            // วันที่ชั่งเข้า
            // 
            this.วันที่ชั่งเข้า.DataPropertyName = "วันที่ชั่งเข้า";
            this.วันที่ชั่งเข้า.FillWeight = 45.44717F;
            this.วันที่ชั่งเข้า.HeaderText = "วันที่ชั่งเข้า";
            this.วันที่ชั่งเข้า.Name = "วันที่ชั่งเข้า";
            this.วันที่ชั่งเข้า.ReadOnly = true;
            this.วันที่ชั่งเข้า.Width = 94;
            // 
            // เวลาชั่งเข้า
            // 
            this.เวลาชั่งเข้า.DataPropertyName = "เวลาชั่งเข้า";
            this.เวลาชั่งเข้า.FillWeight = 42.08287F;
            this.เวลาชั่งเข้า.HeaderText = "เวลาชั่งเข้า";
            this.เวลาชั่งเข้า.Name = "เวลาชั่งเข้า";
            this.เวลาชั่งเข้า.ReadOnly = true;
            this.เวลาชั่งเข้า.Width = 95;
            // 
            // วันที่ชั่งออก
            // 
            this.วันที่ชั่งออก.DataPropertyName = "วันที่ชั่งออก";
            this.วันที่ชั่งออก.FillWeight = 39.02167F;
            this.วันที่ชั่งออก.HeaderText = "วันที่ชั่งออก";
            this.วันที่ชั่งออก.Name = "วันที่ชั่งออก";
            this.วันที่ชั่งออก.ReadOnly = true;
            this.วันที่ชั่งออก.Width = 99;
            // 
            // เวลาชั่งออก
            // 
            this.เวลาชั่งออก.DataPropertyName = "เวลาชั่งออก";
            this.เวลาชั่งออก.FillWeight = 36.23623F;
            this.เวลาชั่งออก.HeaderText = "เวลาชั่งออก";
            this.เวลาชั่งออก.Name = "เวลาชั่งออก";
            this.เวลาชั่งออก.ReadOnly = true;
            // 
            // รหัสผู้ชั่ง
            // 
            this.รหัสผู้ชั่ง.DataPropertyName = "รหัสผู้ชั่ง";
            this.รหัสผู้ชั่ง.FillWeight = 33.70175F;
            this.รหัสผู้ชั่ง.HeaderText = "รหัสผู้ชั่ง";
            this.รหัสผู้ชั่ง.Name = "รหัสผู้ชั่ง";
            this.รหัสผู้ชั่ง.ReadOnly = true;
            this.รหัสผู้ชั่ง.Width = 81;
            // 
            // ชื่อผู้ชั่ง
            // 
            this.ชื่อผู้ชั่ง.DataPropertyName = "ชื่อผู้ชั่ง";
            this.ชื่อผู้ชั่ง.FillWeight = 31.3956F;
            this.ชื่อผู้ชั่ง.HeaderText = "ชื่อผู้ชั่ง";
            this.ชื่อผู้ชั่ง.Name = "ชื่อผู้ชั่ง";
            this.ชื่อผู้ชั่ง.ReadOnly = true;
            this.ชื่อผู้ชั่ง.Width = 75;
            // 
            // รหัสผู้ตัก
            // 
            this.รหัสผู้ตัก.DataPropertyName = "รหัสผู้ตัก";
            this.รหัสผู้ตัก.FillWeight = 29.29721F;
            this.รหัสผู้ตัก.HeaderText = "รหัสผู้ตัก";
            this.รหัสผู้ตัก.Name = "รหัสผู้ตัก";
            this.รหัสผู้ตัก.ReadOnly = true;
            this.รหัสผู้ตัก.Width = 84;
            // 
            // ชื่อผู้ตัก
            // 
            this.ชื่อผู้ตัก.DataPropertyName = "ชื่อผู้ตัก";
            this.ชื่อผู้ตัก.FillWeight = 27.38786F;
            this.ชื่อผู้ตัก.HeaderText = "ชื่อผู้ตัก";
            this.ชื่อผู้ตัก.Name = "ชื่อผู้ตัก";
            this.ชื่อผู้ตัก.ReadOnly = true;
            this.ชื่อผู้ตัก.Width = 78;
            // 
            // ค่าขนส่ง
            // 
            this.ค่าขนส่ง.DataPropertyName = "ค่าขนส่ง";
            this.ค่าขนส่ง.HeaderText = "ค่าขนส่ง";
            this.ค่าขนส่ง.Name = "ค่าขนส่ง";
            this.ค่าขนส่ง.ReadOnly = true;
            this.ค่าขนส่ง.Visible = false;
            this.ค่าขนส่ง.Width = 82;
            // 
            // weight_id
            // 
            this.weight_id.DataPropertyName = "weight_id";
            this.weight_id.HeaderText = "weight_id";
            this.weight_id.Name = "weight_id";
            this.weight_id.ReadOnly = true;
            this.weight_id.Visible = false;
            this.weight_id.Width = 106;
            // 
            // ชนิดvat
            // 
            this.ชนิดvat.DataPropertyName = "ชนิดvat";
            this.ชนิดvat.HeaderText = "ชนิดvat";
            this.ชนิดvat.Name = "ชนิดvat";
            this.ชนิดvat.ReadOnly = true;
            this.ชนิดvat.Visible = false;
            this.ชนิดvat.Width = 86;
            // 
            // ล้าง
            // 
            this.ล้าง.DataPropertyName = "ล้าง";
            this.ล้าง.HeaderText = "ล้าง";
            this.ล้าง.Name = "ล้าง";
            this.ล้าง.ReadOnly = true;
            this.ล้าง.Width = 55;
            // 
            // ขนส่ง
            // 
            this.ขนส่ง.DataPropertyName = "ขนส่ง";
            this.ขนส่ง.HeaderText = "ขนส่ง";
            this.ขนส่ง.Name = "ขนส่ง";
            this.ขนส่ง.ReadOnly = true;
            this.ขนส่ง.Width = 66;
            // 
            // หมายเหตุ
            // 
            this.หมายเหตุ.DataPropertyName = "หมายเหตุ";
            this.หมายเหตุ.HeaderText = "หมายเหตุ";
            this.หมายเหตุ.Name = "หมายเหตุ";
            this.หมายเหตุ.ReadOnly = true;
            this.หมายเหตุ.Width = 88;
            // 
            // oil_content
            // 
            this.oil_content.DataPropertyName = "oil_content";
            this.oil_content.HeaderText = "ปริมาณน้ำมัน";
            this.oil_content.Name = "oil_content";
            this.oil_content.ReadOnly = true;
            this.oil_content.Width = 111;
            // 
            // weightBindingSource
            // 
            this.weightBindingSource.DataMember = "weight";
            this.weightBindingSource.DataSource = this.truckDataSet;
            // 
            // truckDataSet
            // 
            this.truckDataSet.DataSetName = "truckDataSet";
            this.truckDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(414, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 21);
            this.label3.TabIndex = 16;
            this.label3.Text = "น้ำหนัก:";
            // 
            // btSearch
            // 
            this.btSearch.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btSearch.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btSearch.Image = global::SerialPortListener.Properties.Resources.search_32px;
            this.btSearch.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btSearch.Location = new System.Drawing.Point(1077, 10);
            this.btSearch.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btSearch.Name = "btSearch";
            this.btSearch.Size = new System.Drawing.Size(87, 32);
            this.btSearch.TabIndex = 13;
            this.btSearch.Text = "ค้นหา";
            this.btSearch.UseVisualStyleBackColor = true;
            this.btSearch.Click += new System.EventHandler(this.btSearch_Click);
            // 
            // btClose
            // 
            this.btClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btClose.BackColor = System.Drawing.Color.Gray;
            this.btClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btClose.Font = new System.Drawing.Font("Century Gothic", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btClose.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btClose.Image = global::SerialPortListener.Properties.Resources.exit_24px;
            this.btClose.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btClose.Location = new System.Drawing.Point(1081, 577);
            this.btClose.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btClose.Name = "btClose";
            this.btClose.Size = new System.Drawing.Size(90, 39);
            this.btClose.TabIndex = 8;
            this.btClose.Text = "ออก";
            this.btClose.UseVisualStyleBackColor = false;
            this.btClose.Click += new System.EventHandler(this.btClose_Click);
            // 
            // btDelete
            // 
            this.btDelete.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btDelete.BackColor = System.Drawing.Color.IndianRed;
            this.btDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btDelete.Font = new System.Drawing.Font("Century Gothic", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btDelete.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btDelete.Image = global::SerialPortListener.Properties.Resources.delete_bin_24px;
            this.btDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btDelete.Location = new System.Drawing.Point(252, 577);
            this.btDelete.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btDelete.Name = "btDelete";
            this.btDelete.Size = new System.Drawing.Size(90, 39);
            this.btDelete.TabIndex = 7;
            this.btDelete.Text = "ลบ";
            this.btDelete.UseVisualStyleBackColor = false;
            this.btDelete.Visible = false;
            this.btDelete.Click += new System.EventHandler(this.btDelete_Click);
            // 
            // btUpdate
            // 
            this.btUpdate.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btUpdate.BackColor = System.Drawing.Color.LightSkyBlue;
            this.btUpdate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btUpdate.Font = new System.Drawing.Font("Century Gothic", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btUpdate.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btUpdate.Image = global::SerialPortListener.Properties.Resources.edit_file_24px;
            this.btUpdate.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btUpdate.Location = new System.Drawing.Point(133, 577);
            this.btUpdate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btUpdate.Name = "btUpdate";
            this.btUpdate.Size = new System.Drawing.Size(90, 39);
            this.btUpdate.TabIndex = 6;
            this.btUpdate.Text = "แก้ไข";
            this.btUpdate.UseVisualStyleBackColor = false;
            this.btUpdate.Click += new System.EventHandler(this.btUpdate_Click);
            // 
            // btAdd
            // 
            this.btAdd.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btAdd.BackColor = System.Drawing.Color.MediumAquamarine;
            this.btAdd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btAdd.Font = new System.Drawing.Font("Century Gothic", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btAdd.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btAdd.Image = global::SerialPortListener.Properties.Resources.add_24px;
            this.btAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btAdd.Location = new System.Drawing.Point(11, 577);
            this.btAdd.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btAdd.Name = "btAdd";
            this.btAdd.Size = new System.Drawing.Size(90, 39);
            this.btAdd.TabIndex = 5;
            this.btAdd.Text = "เพิ่ม";
            this.btAdd.UseVisualStyleBackColor = false;
            this.btAdd.Click += new System.EventHandler(this.btAdd_Click);
            // 
            // truckDataSetBindingSource
            // 
            this.truckDataSetBindingSource.DataSource = this.truckDataSet;
            this.truckDataSetBindingSource.Position = 0;
            // 
            // weightTableAdapter
            // 
            this.weightTableAdapter.ClearBeforeFill = true;
            // 
            // btExport
            // 
            this.btExport.Image = global::SerialPortListener.Properties.Resources.microsoft_excel_32px;
            this.btExport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btExport.Location = new System.Drawing.Point(997, 582);
            this.btExport.Name = "btExport";
            this.btExport.Size = new System.Drawing.Size(78, 33);
            this.btExport.TabIndex = 17;
            this.btExport.Text = "Export";
            this.btExport.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btExport.UseVisualStyleBackColor = true;
            this.btExport.Click += new System.EventHandler(this.btExport_Click);
            // 
            // TableFromDB
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Linen;
            this.ClientSize = new System.Drawing.Size(1176, 634);
            this.Controls.Add(this.btExport);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tableDataFromDB);
            this.Controls.Add(this.cbbSearchWeight);
            this.Controls.Add(this.btSearch);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dateTo);
            this.Controls.Add(this.dateFrom);
            this.Controls.Add(this.btClose);
            this.Controls.Add(this.btDelete);
            this.Controls.Add(this.btUpdate);
            this.Controls.Add(this.btAdd);
            this.Controls.Add(this.lbTotal);
            this.Font = new System.Drawing.Font("Century Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.MaximizeBox = false;
            this.Name = "TableFromDB";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "รายการชั่งน้ำหนัก";
            this.Load += new System.EventHandler(this.TableFromDB_Load);
            ((System.ComponentModel.ISupportInitialize)(this.tableDataFromDB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.weightBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.truckDataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.truckDataSetBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.BindingSource truckDataSetBindingSource;
        private truckDataSet truckDataSet;
        private System.Windows.Forms.BindingSource weightBindingSource;
        private truckDataSetTableAdapters.weightTableAdapter weightTableAdapter;
        private System.Windows.Forms.Label lbTotal;
        private System.Windows.Forms.Button btAdd;
        private System.Windows.Forms.Button btUpdate;
        private System.Windows.Forms.Button btDelete;
        private System.Windows.Forms.Button btClose;
        private System.Windows.Forms.DateTimePicker dateFrom;
        private System.Windows.Forms.DateTimePicker dateTo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btSearch;
        private System.Windows.Forms.ComboBox cbbSearchWeight;
        private System.Windows.Forms.DataGridView tableDataFromDB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btExport;
        private System.Windows.Forms.DataGridViewTextBoxColumn วันที่;
        private System.Windows.Forms.DataGridViewTextBoxColumn เลขที่เอกสาร;
        private System.Windows.Forms.DataGridViewTextBoxColumn รหัสลูกค้า;
        private System.Windows.Forms.DataGridViewTextBoxColumn ลูกค้า;
        private System.Windows.Forms.DataGridViewTextBoxColumn หน้างาน;
        private System.Windows.Forms.DataGridViewTextBoxColumn ทะเบียนรถ;
        private System.Windows.Forms.DataGridViewTextBoxColumn จังหวัด;
        private System.Windows.Forms.DataGridViewTextBoxColumn คนขับ;
        private System.Windows.Forms.DataGridViewTextBoxColumn ทีม;
        private System.Windows.Forms.DataGridViewTextBoxColumn น้ำหนักรถ;
        private System.Windows.Forms.DataGridViewTextBoxColumn น้ำหนักรวม;
        private System.Windows.Forms.DataGridViewTextBoxColumn น้ำหนักสินค้า;
        private System.Windows.Forms.DataGridViewTextBoxColumn คิว;
        private System.Windows.Forms.DataGridViewTextBoxColumn โรงโม่;
        private System.Windows.Forms.DataGridViewTextBoxColumn ชนิดหิน;
        private System.Windows.Forms.DataGridViewTextBoxColumn ประเภทหิน;
        private System.Windows.Forms.DataGridViewTextBoxColumn ราคาตัน;
        private System.Windows.Forms.DataGridViewTextBoxColumn จำนวณเงิน;
        private System.Windows.Forms.DataGridViewTextBoxColumn vat;
        private System.Windows.Forms.DataGridViewTextBoxColumn จำนวนเงินสุทธิ;
        private System.Windows.Forms.DataGridViewTextBoxColumn จ่ายเงิน;
        private System.Windows.Forms.DataGridViewTextBoxColumn วันที่ชั่งเข้า;
        private System.Windows.Forms.DataGridViewTextBoxColumn เวลาชั่งเข้า;
        private System.Windows.Forms.DataGridViewTextBoxColumn วันที่ชั่งออก;
        private System.Windows.Forms.DataGridViewTextBoxColumn เวลาชั่งออก;
        private System.Windows.Forms.DataGridViewTextBoxColumn รหัสผู้ชั่ง;
        private System.Windows.Forms.DataGridViewTextBoxColumn ชื่อผู้ชั่ง;
        private System.Windows.Forms.DataGridViewTextBoxColumn รหัสผู้ตัก;
        private System.Windows.Forms.DataGridViewTextBoxColumn ชื่อผู้ตัก;
        private System.Windows.Forms.DataGridViewTextBoxColumn ค่าขนส่ง;
        private System.Windows.Forms.DataGridViewTextBoxColumn weight_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn ชนิดvat;
        private System.Windows.Forms.DataGridViewTextBoxColumn ล้าง;
        private System.Windows.Forms.DataGridViewTextBoxColumn ขนส่ง;
        private System.Windows.Forms.DataGridViewTextBoxColumn หมายเหตุ;
        private System.Windows.Forms.DataGridViewTextBoxColumn oil_content;
    }
}