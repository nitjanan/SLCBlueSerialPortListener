using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SerialPortListener;

namespace SerialPortListener.Tests
{
    [TestClass]
    public class LineTypeWorkflowTests
    {
        [TestMethod]
        public void LongLine_AlwaysReturnsZero_RegardlessOfLookup()
        {
            decimal result = LineTypeWorkflow.ResolveWeightIn(
                "1กก-1234", isShortLine: false,
                lookupTodayWeightIn: (lic, date) => 15000m);

            Assert.AreEqual(0m, result);
        }

        [TestMethod]
        public void ShortLine_MatchFound_ReturnsLookedUpValue()
        {
            decimal result = LineTypeWorkflow.ResolveWeightIn(
                "1กก-1234", isShortLine: true,
                lookupTodayWeightIn: (lic, date) => 15000m);

            Assert.AreEqual(15000m, result);
        }

        [TestMethod]
        public void ShortLine_NoMatchToday_ReturnsZero_NotStaleValue()
        {
            decimal result = LineTypeWorkflow.ResolveWeightIn(
                "ไม่เคยชั่ง-999", isShortLine: true,
                lookupTodayWeightIn: (lic, date) => null);

            Assert.AreEqual(0m, result);
        }

        [TestMethod]
        public void ShortLine_LookupThrows_ReturnsZero_DoesNotPropagate()
        {
            decimal result = LineTypeWorkflow.ResolveWeightIn(
                "1กก-1234", isShortLine: true,
                lookupTodayWeightIn: (lic, date) => { throw new InvalidOperationException("db down"); });

            Assert.AreEqual(0m, result);
        }

        [TestMethod]
        public void ShortLine_PassesTodaysDateToLookup()
        {
            DateTime? capturedDate = null;
            LineTypeWorkflow.ResolveWeightIn(
                "1กก-1234", isShortLine: true,
                lookupTodayWeightIn: (lic, date) => { capturedDate = date; return null; });

            Assert.AreEqual(DateTime.Today, capturedDate.Value.Date);
        }
    }
}
