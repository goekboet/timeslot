using System;
using System.Collections.Generic;
using System.Linq;

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
        Adjacent,
        ProperSubset,
        Partial,
        None
    }

    public static class TimeSlotFunctions
    {
        public static TimeSpan Minutes(int m) => TimeSpan.FromMinutes(m);

        public static TimeSpan Hours(int h) => TimeSpan.FromHours(h);
        public static TimeSpan Zero => TimeSpan.Zero;
        public static string Show(
            (TimeSpan o, TimeSpan d) span)
        {
            return $"{span.o.ToString()}{span.d.ToString()}";
        }

        public static TimeSpan End(
            (TimeSpan o, TimeSpan d) span)
        {
            return span.o.Add(span.d);
        }

        public static (TimeSpan o, TimeSpan d) TileForward(
            (TimeSpan o, TimeSpan d) slot)
        {
            return MoveForward(slot, slot.d);
        }

        public static (TimeSpan o, TimeSpan d) MoveForward(
            (TimeSpan o, TimeSpan d) slot,
            TimeSpan span)
        {
            return (slot.o.Add(span), slot.d);
        }

        public static (TimeSpan o, TimeSpan d) MoveBack(
            (TimeSpan o, TimeSpan d) slot,
            TimeSpan span)
        {
            return (slot.o.Subtract(span), slot.d);
        }

        public static Overlap ClassifyOverlap(
            (TimeSpan open, TimeSpan dur) fst,
            (TimeSpan open, TimeSpan dur) snd)
        {
            if (End(fst) < snd.open ||
                End(snd) < fst.open)
                return Overlap.None;
            if (End(fst) == snd.open ||
                End(snd) == fst.open)
                return Overlap.Adjacent;
            if (fst.open <= snd.open && End(fst) < End(snd) ||
                snd.open <= fst.open && End(snd) < End(fst))
                return Overlap.Partial;
            if (fst.open == snd.open && fst.dur == snd.dur)
                return Overlap.Equal;

            return Overlap.ProperSubset;
        }

        public static (TimeSpan open, TimeSpan dur)[] Split(
            (TimeSpan open, TimeSpan dur) dividend,
            int divisor)
        {
            IEnumerable<(TimeSpan o, TimeSpan d)> FillDividend(
                IEnumerable<(TimeSpan o, TimeSpan d)> fill,
                (TimeSpan o, TimeSpan d) next)
            {
                if (End(next) > End(dividend)) return fill;
                return FillDividend(fill.Concat(new[] { next }), TileForward(next));
            }

            var quotient = new TimeSpan(dividend.dur.Ticks / divisor);

            return FillDividend(
                Enumerable.Empty<(TimeSpan, TimeSpan)>(),
                (dividend.open, quotient)).ToArray();
        }

        public static (TimeSpan o, TimeSpan d)[] Minus(
            (TimeSpan o, TimeSpan d) min,
            (TimeSpan o, TimeSpan d) sub)
        {
            var overlap = ClassifyOverlap(min, sub);

            if (new[] { Overlap.Adjacent, Overlap.None }.Contains(overlap))
                return new[] { min };
            if (overlap == Overlap.Partial)
                return min.o < sub.o
                    ? new[] { (min.o, sub.o.Subtract(min.o)) }
                    : new[] { (End(sub), End(min).Subtract(End(sub))) };
            if (overlap == Overlap.ProperSubset)
                return sub.o < min.o
                    ? new[] { (Zero, Zero) }
                    : new[]
                    {
                        (min.o, sub.o.Subtract(min.o)),
                        (End(sub), End(min).Subtract(End(sub)))
                    };

            return new[] { (Zero, Zero) };
        }

    }

    public static class TimeSlotFactories
    {
        public static Func<int, IEnumerable<(TimeSpan o, TimeSpan d)>> Weekdays(
            TimeSpan open,
            TimeSpan @for)
        {
            return weekday =>
            {
                if ((weekday % 7) > 4) return Enumerable.Empty<(TimeSpan o, TimeSpan d)>();

                return new [] { (open, @for) };
            };
        }

        public static IEnumerable<IEnumerable<(TimeSpan o, TimeSpan d)>> BusinessWeek(
            Func<int, IEnumerable<(TimeSpan open, TimeSpan close)>> businessHours,
            Func<(TimeSpan o, TimeSpan d), IEnumerable<(TimeSpan o, TimeSpan d)>> workShifts,
            Func<int, (TimeSpan open, TimeSpan close)[]> breaks)
        {
            return Enumerable.Empty<IEnumerable<(TimeSpan o, TimeSpan d)>>();
        }

    }
}