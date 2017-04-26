using System;

namespace timeslot
{
    /// <summary>
    /// Type of overlap
    /// </summary>
    public enum Overlap
    {
        /// <summary>
        /// one timeslot is equal to the other
        /// </summary>
        Equal,
        /// <summary>
        /// one timeslot is a proper subset of the other with regards to the time it is open
        /// </summary>
        ProperSubset,
        /// <summary>
        /// one timeslot occupies some of the same time as the other
        /// </summary>
        Intersect,
        /// <summary>
        /// the timeslots are disjunct
        /// </summary>
        None
    }

    public static class TimeSlot
    {
        /// <summary>
        /// Classify an overlap between two timeslots
        /// </summary>
        /// <param name="o">open</param>
        /// <param name="d">duration</param>
        /// <returns>A classification</returns>
        public static Overlap Overlap(
            (TimeSpan o, TimeSpan d) fst,
            (TimeSpan o, TimeSpan d) snd)
        {
            if (End(fst) < snd.o ||
                End(snd) < fst.o)
                return timeslot.Overlap.None;
            if (fst.o <= snd.o && End(fst) < End(snd) ||
                snd.o <= fst.o && End(snd) < End(fst))
                return timeslot.Overlap.Intersect;
            if (fst.o == snd.o && fst.d == snd.d)
                return timeslot.Overlap.Equal;

            return timeslot.Overlap.ProperSubset;
        }
        public static bool IsZero((TimeSpan o, TimeSpan d) slot) => slot.d.Equals(Zero);
        public static bool IsNonZero((TimeSpan, TimeSpan) slot) => !IsZero(slot);
        /// <summary>
        /// TimeSpan convenience
        /// </summary>
        public static TimeSpan Zero => TimeSpan.Zero;
        /// <summary>
        /// TimeSpan convenience
        /// </summary>
        public static TimeSpan End(
            (TimeSpan o, TimeSpan d) span)
        {
            return span.o.Add(span.d);
        }
        /// <summary>
        /// TimeSpan convenience
        /// </summary>
        public static TimeSpan Minutes(int m) => TimeSpan.FromMinutes(m);
        /// <summary>
        /// TimeSpan convenience
        /// </summary>
        public static TimeSpan Hours(int h) => TimeSpan.FromHours(h);
        /// <summary>
        /// Like toString
        /// </summary>
        /// <param name="o">open</param>
        /// <param name="d">duration</param>
        /// <returns>a string representation of the timeslot</returns>
        
        public static TimeSpan Min(TimeSpan fst, TimeSpan snd) => fst < snd ? fst : snd;

        public static TimeSpan Max(TimeSpan fst, TimeSpan snd) => fst > snd ? fst : snd;
        public static string Show(
            (TimeSpan o, TimeSpan d) span)
        {
            return $"o: {span.o.ToString()} d: {span.d.ToString()}";
        }
    }
}