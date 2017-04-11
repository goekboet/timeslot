using System;

namespace bookings.core
{
    public struct TimeSlot
    {
        public TimeSlot(
            TimeSpan start,
            TimeSpan duration)
        {
            Start = start;
            Duration = duration;
        }

        public TimeSpan Start { get; }
        public TimeSpan Duration { get; }
    }

    public enum Overlap
    {
        Equal,
        Complete,
        Partial,
        None
    }

    public static class TimeSlotFunctions
    {
        public static string Show((TimeSpan, TimeSpan) slot)
        {
            var (open, close) = slot;
            return $"{open.ToString()}{close.ToString()}";
        }

        public static Overlap GetOverlap(
            (TimeSpan open, TimeSpan dur) fst, 
            (TimeSpan open, TimeSpan dur) snd)
        {
            if (fst.open.Add(fst.dur) <= snd.open || 
                snd.open.Add(snd.dur) <= fst.open)
                return Overlap.None;
            if (fst.open < snd.open && fst.open.Add(fst.dur) > snd.open ||
                snd.open < fst.open && snd.open.Add(snd.dur) > fst.open)
                return Overlap.Partial;
            if (fst.open == snd.open && fst.dur == snd.dur)
                return Overlap.Equal;

            return Overlap.Complete;
        } 
        
        public static (TimeSpan, TimeSpan)[] Split((TimeSpan, TimeSpan) open, (TimeSpan, TimeSpan) closed)
        {
            return new [] {(new TimeSpan(), new TimeSpan())};
        }

    }

    public static class TimeSlotFactories
    {
        public static Func<int, (TimeSpan open, TimeSpan close)[]> Weekdays(
            TimeSpan open,
            TimeSpan close)
        {
            return weekday => {
                if (weekday > 4 ) return new (TimeSpan, TimeSpan)[] {};

                return new (TimeSpan, TimeSpan)[] { (open, close) };
            };
        }

        public static TimeSlot[] FillBusinessHours(
            Func<int, (TimeSpan open, TimeSpan close)[]> businessHours,
            Func<TimeSpan, TimeSpan> durationStrategy,
            Func<TimeSpan, TimeSpan, bool> onBusinessHourOverFlowStrategy)
        {
            return new TimeSlot[] { };
        }

    }
}