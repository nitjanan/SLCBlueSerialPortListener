using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialPortListener
{
    public class WeightDailyReport
    {
        private static string _dateFrom;
        private static string _dateTo;
        private static string _sumToTal;
        private static string _sumQ;
        private static string _sumAmount;
        private static string _sumAmountVat;
        private static string _countRound;

        public string Id{ get; set; }
        public  string วันที่ { get; set; }

        public  string เลขที่เอกสาร { get; set; }

        public  string ทะเบียนรถ { get; set; }

        public  string Mill { get; set; }

        public  string DriverName { get; set; }

        public string รหัสลูกค้า { get; set; }

        public  string ลูกค้า { get; set; }

        public  string ชนิดหิน { get; set; }

        public  string จังหวัด { get; set; }
        public  string DateIn { get; set; }

        public  string DateOut { get; set; }
        public  string TimeIn { get; set; }

        public  string TimeOut { get; set; }

        public  string WeightIn { get; set; }

        public  string WeightOut { get; set; }

        public  string น้ำหนักสินค้า { get; set; }

        public  string ราคาตัน { get; set; }

        public  string จำนวนเงิน { get; set; }

        public  string Vat { get; set; }

        public  string จำนวนเงินสุทธิ { get; set; }

        public  string คิว { get; set; }

        public  string จ่ายเงิน { get; set; }
        public  string VatType { get; set; }

        public  string CustomerAddress { get; set; }

        public  string Site { get; set; }

        public  string StoneColor { get; set; }

        public  string ทีม { get; set; }

        public string ชื่อผู้ตัก { get; set; }

        public static string DateFrom
        {
            get
            {
                return _dateFrom;
            }
            set
            {
                _dateFrom = value;
            }
        }

        public static string DateTo
        {
            get
            {
                return _dateTo;
            }
            set
            {
                _dateTo = value;
            }
        }
        public static string SumToTal
        {
            get
            {
                return _sumToTal;
            }
            set
            {
                _sumToTal = value;
            }
        }

        public static string SumQ
        {
            get
            {
                return _sumQ;
            }
            set
            {
                _sumQ = value;
            }
        }

        public static string SumAmount
        {
            get
            {
                return _sumAmount;
            }
            set
            {
                _sumAmount = value;
            }
        }

        public static string SumAmountVat
        {
            get
            {
                return _sumAmountVat;
            }
            set
            {
                _sumAmountVat = value;
            }
        }
        public static string CountRount
        {
            get
            {
                return _countRound;
            }
            set
            {
                _countRound = value;
            }
        }

    }
}
