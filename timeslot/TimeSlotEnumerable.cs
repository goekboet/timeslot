using System;
using System.Collections.Generic;
using System.Linq;
using static timeslot.TimeSlot;
using static timeslot.Overlap;

namespace timeslot
{
    public static class TimeSlotEnumerable
    {
        /// <summary>
        /// No result
        /// </summary>
        public static (TimeSpan o, TimeSpan d)[] Empty => new(TimeSpan, TimeSpan)[] { };

        /// <summary>
        /// Applies Difference to a list of timeslots and another.
        /// </summary>
        /// <param name="o">open</param>
        /// <param name="d">duration</param>
        /// <param name="fst">the term that gets applied first</param>
        /// <param name="snd">the term that gets applied second</param>
        /// <returns>
        /// A list of timeslots that represents the time 
        /// that is open in the first term but not the second.
        /// </returns>
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
        /// <summary>
        /// Applies Union to a list of timeslots and another. 
        /// </summary>
        /// <param name="o">open</param>
        /// <param name="d">duration</param>
        /// <param name="fst">the term that gets applied first</param>
        /// <param name="snd">the term that gets applied second</param>
        /// <returns>
        /// A list of timeslots that represents the time 
        /// that is open in the first term or the second.
        /// </returns>
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
        /// Applies Intersection to a list of timeslots and another.
        /// </summary>
        /// <param name="o">open</param>
        /// <param name="d">duration</param>
        /// <param name="fst">the term that gets applied first</param>
        /// <param name="snd">the term that gets applied second</param>
        /// <returns>
        /// A list of timeslots that represents the time 
        /// that is open in the first term but not the second.
        /// </returns>
        public static IEnumerable<(TimeSpan o, TimeSpan d)> Intersection(
            IEnumerable<(TimeSpan o, TimeSpan d)> fst,
            IEnumerable<(TimeSpan o, TimeSpan d)> snd)
        {
            Func<(TimeSpan o, TimeSpan), bool> fullyAppliedBy(
                (TimeSpan o, TimeSpan) _s) => _r => End(_s) > End(_r);

            if (fst == null || !fst.Any()) return Empty;
            if (snd == null || !snd.Any()) return Empty;

            var s = snd.First();
            var before = fst.TakeWhile(x => End(x) < s.o).ToArray();
            var applicable = fst
                .Skip(before.Count())
                .TakeWhile(x => Overlap(x, s) != None);
            var r = applicable.SelectMany(x => Intersection(x, s));

            var computed = r.TakeWhile(fullyAppliedBy(s));

            return r
                .Concat(
                    Intersection(
                        fst: fst.Skip(before.Count() + computed.Count()),
                        snd: snd.Skip(1)));
        }

        /// <summary>
        /// The Difference of a timeslot and a another. 
        /// </summary>
        /// <param name="o">open</param>
        /// <param name="d">duration</param>
        /// <param name="fst">first term</param>
        /// <param name="snd">second term</param>
        /// <returns>
        /// A list of zero to two slots that represents the time 
        /// that is open in the first term but not the second.
        /// </returns>
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
        /// <summary>
        /// The Union of a timeslot and a another. 
        /// </summary>
        /// <param name="o">open</param>
        /// <param name="d">duration</param>
        /// <param name="fst">first term</param>
        /// <param name="snd">second term</param>
        /// <returns>
        /// A list of zero to two slots that represents the time 
        /// that is open in the first term or the second.
        /// </returns>
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
        /// <summary>
        /// The Intersection of a timeslot and a another. 
        /// </summary>
        /// <param name="o">open</param>
        /// <param name="d">duration</param>
        /// <param name="fst">first term</param>
        /// <param name="snd">second term</param>
        /// <returns>
        /// A list of zero to two slots that represents the time 
        /// that is open in the first term and also the second.
        /// </returns>
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