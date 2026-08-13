using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace SerialPortListener
{
    class Utils
    {
        public static string hashPassword(string password)
        {
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
            byte[] password_bytes = Encoding.ASCII.GetBytes(password);
            byte[] encrypted_bytes = sha1.ComputeHash(password_bytes);
            return Convert.ToBase64String(encrypted_bytes);
        }

        // Each branch/build has its own AssemblyProduct (Blue = "SerialPortListenerM", Pink = "SerialPortListenerS", ...).
        // Use it to derive a per-build AppData folder so Blue and Pink never read/write each other's config/log files
        // when installed on the same machine. "SerialPortListenerM" keeps the original folder name so existing Blue
        // installs keep their saved configuration; any other product name gets its own dedicated folder.
        public static readonly string AppDataDir = GetAppDataDir();

        private static string GetAppDataDir()
        {
            string product = System.Windows.Forms.Application.ProductName;
            string folderName = (product == "SerialPortListenerM")
                ? "SLCBlueSerialPortListener"
                : "SLC" + product;
            return System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                folderName);
        }
    }
}
