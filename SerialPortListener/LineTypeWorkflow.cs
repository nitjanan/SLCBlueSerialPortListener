using System;

namespace SerialPortListener
{
    // Decides what weight-in value to apply for Krabi's short/long line workflow.
    // See docs/superpowers/specs/2026-09-29-krabi-stp-mode-design.md, item 1.
    static class LineTypeWorkflow
    {
        public static decimal ResolveWeightIn(
            string carLicense,
            bool isShortLine,
            Func<string, DateTime, decimal?> lookupTodayWeightIn)
        {
            if (!isShortLine)
                return 0m;

            try
            {
                decimal? found = lookupTodayWeightIn(carLicense, DateTime.Today);
                return found ?? 0m;
            }
            catch (Exception)
            {
                // No valid reading available - behave like "nothing found," not a crash.
                return 0m;
            }
        }
    }
}
