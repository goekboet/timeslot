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
            Func<(TimeSpan o, TimeSpan), bool> fullyAppliedBy(
                (TimeSpan o, TimeSpan) _s) => _r => End(_s) > End(_r);

            if (fst == null || !fst.Any()) return Empty;
            if (snd == null || !snd.Any()) return fst;

            var s = snd.First();
            var before = fst.TakeWhile(x => End(x) < s.o).ToArray();
            var applicable = fst
                .Skip(before.Count())
                .TakeWhile(x => Overlap(x, s) != None);
            var r = applicable.SelectMany(x => Difference(x, s));

            var computed = r.TakeWhile(fullyAppliedBy(s));
            var remainder = r.SkipWhile(fullyAppliedBy(s));

            return before
                .Concat(computed)
                .Concat(Difference(
                    fst: remainder.Concat(fst.Skip(before.Count() + applicable.Count())),
                    snd: snd.Skip(1)));
        }

        public static IEnumerable<(TimeSpan o, TimeSpan d)> Union(
            IEnumerable<(TimeSpan o, TimeSpan d)> fst,
            IEnumerable<(TimeSpan o, TimeSpan d)> snd
        )
        {
            Func<(TimeSpan o, TimeSpan), bool> fullyAppliedBy(
                (TimeSpan o, TimeSpan) _s) => _r => End(_s) > End(_r);

            if (fst == null || !fst.Any()) return snd;
            if (snd == null || !snd.Any()) return fst;

            var s = snd.First();
            var before = fst.TakeWhile(x => End(x) < s.o).ToArray();

            var applicable = fst
                .Skip(before.Count())
                .TakeWhile(x => Overlap(x, s) != None);

            var r = applicable.Any()
                ? new[] { applicable.Aggregate(s, (acc, next) => Union(acc, next).Single()) }
                : new[] { s };

            var computed = r.TakeWhile(fullyAppliedBy(s));
            var remainder = r.SkipWhile(fullyAppliedBy(s));

            return before
                .Concat(computed)
                .Concat(
                    Union(
                        fst: remainder.Concat(fst.Skip(before.Count() + applicable.Count())),
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
            (TimeSpan o, TimeSpan d) fst,
            (TimeSpan o, TimeSpan d) snd)
        {
            switch (Overlap(fst, snd))
            {
                case Equal:
                    return Empty;
                case None:
                    return new[] { fst };
                case Intersect:
                    return fst.o < snd.o
                        ? new[] { (fst.o, snd.o.Subtract(fst.o)) }
                        : new[] { (End(snd), End(fst).Subtract(End(snd))) };
                case ProperSubset:
                    return snd.o < fst.o
                        ? Empty
                        : new[]
                        {
                            (fst.o, snd.o.Subtract(fst.o)),
                            (End(snd), End(fst).Subtract(End(snd)))
                        }.Where(x => x.Item2 > Zero).ToArray();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static IEnumerable<(TimeSpan o, TimeSpan d)> Union(
            (TimeSpan o, TimeSpan d) fst,
            (TimeSpan o, TimeSpan d) snd)
        {
            switch (Overlap(fst, snd))
            {
                case Equal:
                    return new[] { fst };
                case None:
                    return new[] { fst, snd }.OrderBy(x => x.o);
                case Intersect:
                    return new[] { (Min(fst.o, snd.o), Max(End(fst), End(snd)).Subtract(Min(fst.o, snd.o))) };
                case ProperSubset:
                    return new[] { (Min(fst.o, snd.o), Max(fst.d, snd.d)) };
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static (TimeSpan o, TimeSpan d)[] Intersection(
            (TimeSpan o, TimeSpan d) fst,
            (TimeSpan o, TimeSpan d) snd)
        {
            (TimeSpan, TimeSpan)[] WithoutZero((TimeSpan, TimeSpan) [] spans) => 
                spans.Where(IsNonZero).ToArray();

            switch (Overlap(fst, snd))
            {
                case Equal:
                    return new[] { fst };
                case None:
                    return Empty;
                case Intersect:
                    return End(fst) > End(snd)
                        ? WithoutZero(new [] { (fst.o, End(snd).Subtract(fst.o))})
                        : WithoutZero(new [] { (snd.o, End(fst).Subtract(snd.o))});
                case ProperSubset:
                    return fst.d < snd.d 
                        ? new [] { fst }
                        : new [] { snd }; 
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}