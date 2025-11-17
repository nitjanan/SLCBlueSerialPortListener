using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialPortListener
{
    class WeightTempReport
    {
        private static string _dateFrom;
        private static string _dateTo;
        private static string _mainComp;

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

        public static string MainComp
        {
            get
            {
                return _mainComp;
            }
            set
            {
                _mainComp = value;
            }
        }
    }
}
