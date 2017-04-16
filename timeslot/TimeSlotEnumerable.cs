using System;
using System.Collections.Generic;
using System.Linq;
using static timeslot.TimeSlot;

namespace timeslot
{
    public static class TimeSlotEnumerable
    {
        public static (TimeSpan o, TimeSpan d)[] Empty => new(TimeSpan, TimeSpan)[] { };

        public static string ShowSlots(IEnumerable<(TimeSpan, TimeSpan)> slots)
        {
            return string.Join("\n", slots.Select(x => Show(x)));
        }
        /// <summary>
        /// Split a timeslot into equal parts.
        /// </summary>
        /// <param name="open"></param>
        /// <param name="dur"></param>
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
        /// <summary>
        /// Applies minus to a list of timeslots and another list of timeslots. The result
        /// is the set difference of the time that is open between the first list and the second.
        /// </summary>
        /// <param name="o">open</param>
        /// <param name="d">duration</param>
        public static IEnumerable<(TimeSpan o, TimeSpan d)> Minus(
            IEnumerable<(TimeSpan o, TimeSpan d)> min,
            IEnumerable<(TimeSpan o, TimeSpan d)> sub)
        {
            Func<(TimeSpan o, TimeSpan), bool> before(
                (TimeSpan o, TimeSpan) fst) => snd => fst.o < snd.o;

            if (min == null || !min.Any()) return Empty;
            if (sub == null || !sub.Any()) return min;

            var s = sub.First();
            var m = min.TakeWhile(x => x.o < End(s));
            var r = m.SelectMany(x => Minus(x ,s));

            var computed = r.TakeWhile(before(s));
            var remainder = r.SkipWhile(before(s));

            return computed
                .Concat(Minus(
                    min: remainder.Concat(min.Skip(m.Count())), 
                    sub: sub.Skip(1))); 
        }
        /// <summary>
        /// Applies minus to one timeslot and another. The result is the set difference 
        /// of the time considered open in the two timeslots. The result will be a list of
        /// timeslots of zero to two elements.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="d"></param>
        public static (TimeSpan o, TimeSpan d)[] Minus(
            (TimeSpan o, TimeSpan d) min,
            (TimeSpan o, TimeSpan d) sub)
        {
            var overlap = ClassifyOverlap(min, sub);

            if (overlap == Overlap.None)
                return new[] { min };
            if (overlap == Overlap.Intersect)
                return min.o < sub.o
                    ? new[] { (min.o, sub.o.Subtract(min.o)) }
                    : new[] { (End(sub), End(min).Subtract(End(sub))) };
            if (overlap == Overlap.ProperSubset)
                return sub.o < min.o
                    ? Empty
                    : new[]
                    {
                        (End(sub), End(min).Subtract(End(sub))),
                        (min.o, sub.o.Subtract(min.o))
                    }.Where(x => x.Item2 > Zero).ToArray();

            return Empty;
        }
    }
}