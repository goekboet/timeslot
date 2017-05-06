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
            var first = _stack(fst);
            var second = _stack(snd);
            var result = _stack(Empty);

            while (first.Count > 0)
            {
                var f = first.Pop();

                if (second.Count == 0)
                    result.Push(f);
                else
                {
                    var s = second.Pop();
                    var r = Difference(f, s);
                    if (s.o < f.o) second.Push(s);

                    var computed = r.Where(x => x.o >= End(s));
                    var remainder = r.Where(x => End(x) <= s.o);

                    if (computed.Any()) result.Push(computed.Single());
                    if (remainder.Any()) first.Push(remainder.Single());
                }
            }

            return result;
        }

        private static Stack<T> _stack<T>(IEnumerable<T> items) where T : struct => new Stack<T>(items ?? new T[] { });

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
            var first = _stack(fst);
            var second = _stack(snd);
            var result = _stack(Empty);

            while (first.Any())
            {
                var f = first.Pop();

                if (second.Any())
                {
                    var s = second.Pop();
                    if (End(s) < f.o)
                    {
                        result.Push(f);
                        second.Push(s);
                    }
                    else if (End(f) < s.o)
                    {
                        result.Push(s);
                        first.Push(f);
                    }
                    else
                    {
                        var r = Union(f, s).Single();
                        if (r.o == f.o) first.Push(r);
                        else second.Push(r);
                    }
                }
                else
                    result.Push(f);
            }

            while (second.Any())
                result.Push(second.Pop());

            return result;
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
            var first = _stack(fst);
            var second = _stack(snd);
            var result = _stack(Empty);

            while (first.Any())
            {
                var f = first.Pop();
                if (second.Any())
                {
                    var s = second.Pop();
                    if (s.o < f.o) second.Push(s);
                    if (f.o < s.o) first.Push(f);
                    var r = Intersection(f, s);
                    if (r.Any())
                    {
                        var i = r.Single();
                        result.Push(i);
                    }
                }
            }

            return result;
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
            (TimeSpan, TimeSpan)[] WithoutZero((TimeSpan, TimeSpan)[] spans) =>
                spans.Where(IsNonZero).ToArray();

            switch (Overlap(fst, snd))
            {
                case Equal:
                    return new[] { fst };
                case None:
                    return Empty;
                case Intersect:
                    return End(fst) > End(snd)
                        ? WithoutZero(new[] { (fst.o, End(snd).Subtract(fst.o)) })
                        : WithoutZero(new[] { (snd.o, End(fst).Subtract(snd.o)) });
                case ProperSubset:
                    return fst.d < snd.d
                        ? new[] { fst }
                        : new[] { snd };
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}