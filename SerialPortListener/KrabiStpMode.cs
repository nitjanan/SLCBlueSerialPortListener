using System;
using System.Collections.Generic;
using System.IO;

namespace SerialPortListener
{
    // Persists the Krabi STP mode flag using the same flat Key=Value config-file
    // pattern as ReportMainTemplate's config_reportmain.txt. See
    // docs/superpowers/specs/2026-09-29-krabi-stp-mode-design.md.
    static class KrabiStpMode
    {
        private const string FileName = "config_krabistp.txt";
        private const string Key = "IsKrabiSTPVersion";

        // Test-only seam: when set, config lives here instead of Utils.AppDataDir.
        // Only ever assigned via test code/reflection, so the compiler can't see it
        // being written from production paths - CS0649 here is expected and harmless.
#pragma warning disable 649
        internal static string ConfigDirOverride;
#pragma warning restore 649

        private static string ConfigDir
        {
            get { return ConfigDirOverride ?? Utils.AppDataDir; }
        }

        private static string ConfigPath
        {
            get { return Path.Combine(ConfigDir, FileName); }
        }

        private static bool? _cachedValue;

        public static bool IsEnabled
        {
            get
            {
                if (_cachedValue.HasValue)
                    return _cachedValue.Value;

                _cachedValue = ComputeIsEnabled();
                return _cachedValue.Value;
            }
        }

        private static bool ComputeIsEnabled()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                    return false;

                foreach (string line in File.ReadAllLines(ConfigPath))
                {
                    int idx = line.IndexOf('=');
                    if (idx <= 0)
                        continue;

                    string key = line.Substring(0, idx).Trim();
                    if (!string.Equals(key, Key, StringComparison.OrdinalIgnoreCase))
                        continue;

                    bool value;
                    if (bool.TryParse(line.Substring(idx + 1).Trim(), out value))
                        return value;

                    return false; // key present but value unparseable
                }
            }
            catch (Exception)
            {
                // Missing permissions, locked file, etc. - never block startup.
            }

            return false;
        }

        public static bool Save(bool enabled)
        {
            try
            {
                if (!Directory.Exists(ConfigDir))
                    Directory.CreateDirectory(ConfigDir);

                var lines = new List<string>();
                bool replaced = false;

                if (File.Exists(ConfigPath))
                {
                    foreach (string line in File.ReadAllLines(ConfigPath))
                    {
                        int idx = line.IndexOf('=');
                        string key = idx > 0 ? line.Substring(0, idx).Trim() : null;
                        if (key != null && string.Equals(key, Key, StringComparison.OrdinalIgnoreCase))
                        {
                            lines.Add(Key + "=" + enabled);
                            replaced = true;
                        }
                        else
                        {
                            lines.Add(line);
                        }
                    }
                }

                if (!replaced)
                    lines.Add(Key + "=" + enabled);

                File.WriteAllLines(ConfigPath, lines.ToArray());
                _cachedValue = enabled;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Test-only seam: clears the cached value so tests can exercise ComputeIsEnabled again
        // after swapping ConfigDirOverride.
        internal static void ResetCacheForTests()
        {
            _cachedValue = null;
        }
    }
}
