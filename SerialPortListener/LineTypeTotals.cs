using System;

namespace SerialPortListener
{
    // Line-type segmented daily totals for Krabi STP mode.
    // See docs/superpowers/specs/2026-09-29-krabi-stp-mode-design.md, item 2.
    static class LineTypeTotals
    {
        public struct Snapshot
        {
            public readonly int ShortCount;
            public readonly decimal ShortWeight;
            public readonly int LongCount;
            public readonly decimal LongWeight;

            public Snapshot(int shortCount, decimal shortWeight, int longCount, decimal longWeight)
            {
                ShortCount = shortCount;
                ShortWeight = shortWeight;
                LongCount = longCount;
                LongWeight = longWeight;
            }
        }

        public static bool IsWithinCutoff(DateTime? cutoffDate, TimeSpan? cutoffTime, DateTime now)
        {
            if (cutoffDate == null || cutoffTime == null)
                return false; // not fully configured yet - treat as "no cutoff," never throw

            DateTime cutoff = cutoffDate.Value.Date + cutoffTime.Value;
            return now >= cutoff;
        }

        public static Snapshot Accumulate(Snapshot current, string lineType, decimal weight)
        {
            if (string.Equals(lineType, "short", StringComparison.OrdinalIgnoreCase))
            {
                return new Snapshot(current.ShortCount + 1, current.ShortWeight + weight,
                                     current.LongCount, current.LongWeight);
            }

            if (string.Equals(lineType, "long", StringComparison.OrdinalIgnoreCase))
            {
                return new Snapshot(current.ShortCount, current.ShortWeight,
                                     current.LongCount + 1, current.LongWeight + weight);
            }

            return current; // unrecognized line_type value - don't silently miscount either bucket
        }
    }
}
