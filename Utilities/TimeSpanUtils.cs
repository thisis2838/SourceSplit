using System;
using static System.TimeSpan;

namespace LiveSplit.SourceSplit.Utilities
{
    public static class TimeSpanUtils
    {
        public static TimeSpan TimeFromTicks(long ticks, double timePerTick)
        {
            float f = (float)timePerTick;
            return FromTicks((long)(ticks * (f * TimeSpan.TicksPerSecond)));
        }

        public static string ToStringCustom(this TimeSpan span, int precision = 6)
        {
            long hours = span.Ticks / TicksPerHour;
            long minutes = (span.Ticks - hours * TicksPerHour) / TicksPerMinute;
            long seconds = (span.Ticks - hours * TicksPerHour - minutes * TicksPerMinute) / TicksPerSecond;

            long ms = (span.Ticks - hours * TicksPerHour - minutes * TicksPerMinute - seconds * TicksPerSecond);

            return $"{hours:D2}:{minutes:D2}:{seconds:D2}." + ms.ToString("D7").Substring(0, precision > 7 ? 7 : precision);
        }
    }
}
