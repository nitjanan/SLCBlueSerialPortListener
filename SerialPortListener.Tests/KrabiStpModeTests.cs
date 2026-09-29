using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SerialPortListener;

namespace SerialPortListener.Tests
{
    [TestClass]
    public class KrabiStpModeTests
    {
        private string _tempDir;

        [TestInitialize]
        public void Setup()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "KrabiStpModeTests_" + Guid.NewGuid());
            Directory.CreateDirectory(_tempDir);
            KrabiStpMode.ConfigDirOverride = _tempDir;
        }

        [TestCleanup]
        public void Cleanup()
        {
            KrabiStpMode.ConfigDirOverride = null;
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        [TestMethod]
        public void IsEnabled_MissingFile_ReturnsFalse()
        {
            Assert.IsFalse(KrabiStpMode.IsEnabled);
        }

        [TestMethod]
        public void IsEnabled_MissingDirectory_ReturnsFalse()
        {
            Directory.Delete(_tempDir, true);
            Assert.IsFalse(KrabiStpMode.IsEnabled);
        }

        [TestMethod]
        public void IsEnabled_MalformedValue_ReturnsFalse()
        {
            File.WriteAllText(Path.Combine(_tempDir, "config_krabistp.txt"), "IsKrabiSTPVersion=notabool\r\n");
            Assert.IsFalse(KrabiStpMode.IsEnabled);
        }

        [TestMethod]
        public void Save_True_ThenIsEnabled_ReturnsTrue()
        {
            bool saved = KrabiStpMode.Save(true);
            Assert.IsTrue(saved);
            Assert.IsTrue(KrabiStpMode.IsEnabled);
        }

        [TestMethod]
        public void Save_FalseAfterTrue_ReturnsFalse()
        {
            KrabiStpMode.Save(true);
            KrabiStpMode.Save(false);
            Assert.IsFalse(KrabiStpMode.IsEnabled);
        }

        [TestMethod]
        public void Save_PreservesOtherKeysInFile()
        {
            File.WriteAllText(Path.Combine(_tempDir, "config_krabistp.txt"), "SomeOtherKey=hello\r\n");
            KrabiStpMode.Save(true);
            string content = File.ReadAllText(Path.Combine(_tempDir, "config_krabistp.txt"));
            StringAssert.Contains(content, "SomeOtherKey=hello");
            Assert.IsTrue(KrabiStpMode.IsEnabled);
        }
    }
}
