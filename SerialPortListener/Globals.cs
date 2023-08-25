using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SerialPortListener
{
    class Globals
    {
        private static string _userId;
        private static string _username;
        private static string _password;
        private static string _firstname;
        private static string _permission;
        private static string _modeWeight;

        public static string UserId
        {
            get
            {
                // Reads are usually simple
                return _userId;
            }
            set
            {
                // You can add logic here for race conditions,
                // or other measurements
                _userId = value;
            }
        }

        public static string Username
        {
            get
            {
                // Reads are usually simple
                return _username;
            }
            set
            {
                // You can add logic here for race conditions,
                // or other measurements
                _username = value;
            }
        }

        public static string Password
        {
            get
            {
                // Reads are usually simple
                return _password;
            }
            set
            {
                // You can add logic here for race conditions,
                // or other measurements
                _password = value;
            }
        }

        public static string Firstname
        {
            get
            {
                // Reads are usually simple
                return _firstname;
            }
            set
            {
                // You can add logic here for race conditions,
                // or other measurements
                _firstname = value;
            }
        }

        public static string Permission
        {
            get
            {
                // Reads are usually simple
                return _permission;
            }
            set
            {
                // You can add logic here for race conditions,
                // or other measurements
                _permission = value;
            }
        }
        public static string ModeWeight
        {
            // A = Add (เพิ่ม)
            // U = Update (แก้ไข)
            // F = Finish (ข้อมูลเรียบร้อย)
            get
            {
                return _modeWeight;
            }
            set
            {
                _modeWeight = value;
            }
        }

        public static Boolean isPermissionTop() {
            if (Globals.Permission == Utils.hashPassword("admin"))
                return true;
            else
                return false;
        }

        public static Boolean isPermissionEditWeight()
        {
            if (Globals.Permission == Utils.hashPassword("edit_weight"))
                return true;
            else
                return false;
        }

        public static Boolean isPermissionSales()
        {
            if (Globals.Permission == Utils.hashPassword("sale"))
                return true;
            else
                return false;
        }

    }
}
