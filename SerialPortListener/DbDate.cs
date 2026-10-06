using System;
using System.Data.Odbc;
using System.Globalization;

namespace SerialPortListener
{
    // Date handling that does not depend on the PostgreSQL server's DateStyle
    // (ISO/DMY vs ISO/MDY, set by the server locale) nor on the Windows culture.
    public static class DbDate
    {
        // Every connection uses the same DateStyle regardless of server config,
        // so string date literals like '06/10/2026' are always read as dd/MM.
        public const string SessionDateStyle = "ISO, DMY";

        // Thai systems send day first
        private static readonly string[] DayFirstFormats =
        {
            "d/M/yyyy",
            "d/M/yyyy H:mm:ss",
            "d/M/yyyy HH:mm:ss",
            "d-M-yyyy",
            "d.M.yyyy",
        };

        public static void ApplySessionDateStyle(OdbcConnection conn)
        {
            using (OdbcCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SET DateStyle TO '" + SessionDateStyle + "'";
                cmd.ExecuteNonQuery();
            }
        }

        // Parses a date string from an API or DB. docNo (e.g. "DO2610-000213" = yymm)
        // is used to fix day/month swaps when the source format is ambiguous.
        public static DateTime? Parse(string raw, string docNo = null)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            string s = raw.Trim();
            DateTime d;

            // ISO (yyyy-MM-dd[THH:mm:ss[+07:00]]): take the date part as written,
            // without timezone conversion that could shift it to another day
            string isoDate = s.Length >= 10 ? s.Substring(0, 10) : s;
            if (!DateTime.TryParseExact(isoDate, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out d)
                && !DateTime.TryParseExact(s, DayFirstFormats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out d))
            {
                return null;
            }

            d = d.Date;

            // Buddhist year (e.g. 2569) -> Gregorian
            if (d.Year > 2400)
                d = d.AddYears(-543);

            return FixSwapByDocNo(d, docNo);
        }

        public static string ToIso(DateTime d)
        {
            return d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private static DateTime FixSwapByDocNo(DateTime d, string docNo)
        {
            // docNo format: XXyymm-nnnnnn
            if (string.IsNullOrEmpty(docNo) || docNo.Length < 6)
                return d;

            int mm;
            if (!int.TryParse(docNo.Substring(4, 2), NumberStyles.None, CultureInfo.InvariantCulture, out mm))
                return d;

            if (mm < 1 || mm > 12 || d.Month == mm || d.Day != mm)
                return d;

            // Day and month swapped: day matches doc month, month doesn't
            try
            {
                return new DateTime(d.Year, d.Day, d.Month);
            }
            catch (ArgumentOutOfRangeException)
            {
                return d;
            }
        }
    }
}
