using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialPortListener
{
    class Company
    {
        private static string _companyName;
        private static string _address;
        private static string _telephone;
        private static string _email;
        private static string _Ttelephone;
        private static string _Temail;
        private static string _TdocName;
        private static string _Tlogo;
        private static string _code;
        public static string CompanyName
        {
            get
            {
                // Reads are usually simple
                return _companyName;
            }
            set
            {
                // You can add logic here for race conditions,
                // or other measurements
                _companyName = value;
            }
        }
        public static string Address
        {
            get
            {
                // Reads are usually simple
                return _address;
            }
            set
            {
                // You can add logic here for race conditions,
                // or other measurements
                _address = value;
            }
        }

        public static string Telephone
        {
            get
            {
                // Reads are usually simple
                return _telephone;
            }
            set
            {
                // You can add logic here for race conditions,
                // or other measurements
                _telephone = value;
            }
        }

        public static string Email
        {
            get
            {
                // Reads are usually simple
                return _email;
            }
            set
            {
                // You can add logic here for race conditions,
                // or other measurements
                _email = value;
            }
        }

        public static string TTelephone
        {
            get
            {
                // Reads are usually simple
                return _Ttelephone;
            }
            set
            {
                // You can add logic here for race conditions,
                // or other measurements
                _Ttelephone = value;
            }
        }

        public static string TEmail
        {
            get
            {
                // Reads are usually simple
                return _Temail;
            }
            set
            {
                // You can add logic here for race conditions,
                // or other measurements
                _Temail = value;
            }
        }

        public static string TDocName
        {
            get
            {
                // Reads are usually simple
                return _TdocName;
            }
            set
            {
                // You can add logic here for race conditions,
                // or other measurements
                _TdocName = value;
            }
        }

        public static string TLogo
        {
            get
            {
                // Reads are usually simple
                return _Tlogo;
            }
            set
            {
                // You can add logic here for race conditions,
                // or other measurements
                _Tlogo = value;
            }
        }

        public static string Code
        {
            get
            {
                // Reads are usually simple
                return _code;
            }
            set
            {
                // You can add logic here for race conditions,
                // or other measurements
                _code = value;
            }
        }
    }
}
