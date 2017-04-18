using System;
using System.Collections.Generic;
using System.Linq;
using static timeslot.TimeSlot;
using static timeslot.Overlap;

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
        /// Applies Difference to a list of timeslots and another list of timeslots. The result
        /// is the set difference of the time that is open between the first list and the second.
        /// </summary>
        /// <param name="o">open</param>
        /// <param name="d">duration</param>
        public static IEnumerable<(TimeSpan o, TimeSpan d)> Difference(
            IEnumerable<(TimeSpan o, TimeSpan d)> fst,
            IEnumerable<(TimeSpan o, TimeSpan d)> snd)
        {
            Func<(TimeSpan o, TimeSpan), bool> before(
                (TimeSpan o, TimeSpan) a) => b => a.o < b.o;

            if (fst == null || !fst.Any()) return Empty;
            if (snd == null || !snd.Any()) return fst;

            var s = snd.First();
            var m = fst.TakeWhile(x => x.o < End(s));
            var r = m.SelectMany(x => Difference(x ,s));

            var computed = r.TakeWhile(before(s));
            var remainder = r.SkipWhile(before(s));

            return computed
                .Concat(Difference(
                    fst: remainder.Concat(fst.Skip(m.Count())), 
                    snd: snd.Skip(1))); 
        }
        /// <summary>
        /// The difference between the first timeslot and a second. 
        /// The result is the time considered open in first but not the second. 
        /// The result will be a list of timeslots of zero to two elements.
        /// </summary>
        /// <param name="o">open</param>
        /// <param name="d">duration</param>
        /// <param name="m"></param>
        /// <param name="d"></param>
        /// </summary>
       
        public static (TimeSpan o, TimeSpan d)[] Difference(
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

        public static IEnumerable<(TimeSpan o, TimeSpan d)> Union(
            (TimeSpan o, TimeSpan d) fst,
            (TimeSpan o, TimeSpan d) snd)
        {
            switch (ClassifyOverlap(fst, snd))
            {
                case None:
                    return new [] { fst, snd }.OrderByDescending(x => x.o);
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
    }
}