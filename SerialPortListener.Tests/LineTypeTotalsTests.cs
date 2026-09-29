using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SerialPortListener;

namespace SerialPortListener.Tests
{
    [TestClass]
    public class LineTypeTotalsTests
    {
        [TestMethod]
        public void IsWithinCutoff_BothNull_ReturnsFalse_TreatedAsNotConfigured()
        {
            Assert.IsFalse(LineTypeTotals.IsWithinCutoff(null, null, DateTime.Now));
        }

        [TestMethod]
        public void IsWithinCutoff_DateSetTimeNull_ReturnsFalse_DoesNotThrow()
        {
            Assert.IsFalse(LineTypeTotals.IsWithinCutoff(DateTime.Today, null, DateTime.Now));
        }

        [TestMethod]
        public void IsWithinCutoff_TimeSetDateNull_ReturnsFalse_DoesNotThrow()
        {
            Assert.IsFalse(LineTypeTotals.IsWithinCutoff(null, TimeSpan.FromHours(6), DateTime.Now));
        }

        [TestMethod]
        public void IsWithinCutoff_BothSet_NowAfterCutoff_ReturnsTrue()
        {
            var cutoffDate = DateTime.Today.AddDays(-1);
            var cutoffTime = TimeSpan.FromHours(6);
            var now = DateTime.Today.AddHours(7);

            Assert.IsTrue(LineTypeTotals.IsWithinCutoff(cutoffDate, cutoffTime, now));
        }

        [TestMethod]
        public void IsWithinCutoff_BothSet_NowBeforeCutoff_ReturnsFalse()
        {
            var cutoffDate = DateTime.Today;
            var cutoffTime = TimeSpan.FromHours(20);
            var now = DateTime.Today.AddHours(7);

            Assert.IsFalse(LineTypeTotals.IsWithinCutoff(cutoffDate, cutoffTime, now));
        }

        [TestMethod]
        public void Accumulate_ShortLine_AddsToShortTotalOnly()
        {
            var start = new LineTypeTotals.Snapshot(0, 0m, 0, 0m);
            var result = LineTypeTotals.Accumulate(start, "short", 1500m);

            Assert.AreEqual(1, result.ShortCount);
            Assert.AreEqual(1500m, result.ShortWeight);
            Assert.AreEqual(0, result.LongCount);
            Assert.AreEqual(0m, result.LongWeight);
        }

        [TestMethod]
        public void Accumulate_LongLine_AddsToLongTotalOnly()
        {
            var start = new LineTypeTotals.Snapshot(0, 0m, 0, 0m);
            var result = LineTypeTotals.Accumulate(start, "long", 2000m);

            Assert.AreEqual(0, result.ShortCount);
            Assert.AreEqual(0m, result.ShortWeight);
            Assert.AreEqual(1, result.LongCount);
            Assert.AreEqual(2000m, result.LongWeight);
        }

        [TestMethod]
        public void Accumulate_UnknownLineType_LeavesSnapshotUnchanged()
        {
            var start = new LineTypeTotals.Snapshot(1, 100m, 1, 200m);
            var result = LineTypeTotals.Accumulate(start, "unknown", 999m);

            Assert.AreEqual(start.ShortCount, result.ShortCount);
            Assert.AreEqual(start.ShortWeight, result.ShortWeight);
            Assert.AreEqual(start.LongCount, result.LongCount);
            Assert.AreEqual(start.LongWeight, result.LongWeight);
        }
    }
}
