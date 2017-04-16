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
        /// one timeslot follows the other
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
        public static Overlap ClassifyOverlap(
            (TimeSpan o, TimeSpan d) fst,
            (TimeSpan o, TimeSpan d) snd)
        {
            if (End(fst) < snd.o ||
                End(snd) < fst.o)
                return Overlap.None;
            if (fst.o <= snd.o && End(fst) < End(snd) ||
                snd.o <= fst.o && End(snd) < End(fst))
                return Overlap.Intersect;
            if (fst.o == snd.o && fst.d == snd.d)
                return Overlap.Equal;

            return Overlap.ProperSubset;
        }
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
        /// Functional toString
        /// </summary>
        /// <param name="o">open</param>
        /// <param name="d">duration</param>
        /// <returns>a string representation of the timeslot</returns>
        public static string Show(
            (TimeSpan o, TimeSpan d) span)
        {
            return $"o: {span.o.ToString()} d: {span.d.ToString()}";
        }
        /// <summary>
        /// Move a given timespan forward
        /// </summary>
        /// <param name="o">open</param>
        /// <param name="d">duration</param>
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

        public static (TimeSpan o, TimeSpan d) TileForward(
            (TimeSpan o, TimeSpan d) slot)
        {
            return MoveForward(slot, slot.d);
        }
    }
}