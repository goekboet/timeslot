using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static timeslot.TimeSlot;
using static timeslot.Overlap;

namespace timeslot.tests
{
    public class TimeSlotShould
    {
        public static Func<(TimeSpan open, TimeSpan dur)> referenceSlot01h01h = () => (TimeSpan.FromHours(1), TimeSpan.FromHours(1));

        private static Func<(TimeSpan open, TimeSpan dur), IEnumerable<(TimeSpan, TimeSpan)>> NoOverlap = s =>
            Enumerable.Range(1, 1)
                .Select(x => (s.open.Add(TimeSpan.FromHours(-x - 1)), s.dur))
                .Concat(Enumerable.Range(1, 1)
                .Select(x => (s.open.Add(s.dur.Add(TimeSpan.FromHours(x))), s.dur)));
                
        [Fact]
        public void Classify_overlap_as_None()
        {
            var businessHours = referenceSlot01h01h();
            var overlaps = NoOverlap(businessHours);

            Assert.All(overlaps, x => Assert.True(ClassifyOverlap(x, businessHours).Equals(None)));
        }

        [Fact]
        public void Classify_overlap_as_Equal()
        {

            var first = referenceSlot01h01h();
            var second = referenceSlot01h01h();

            Assert.True(ClassifyOverlap(first, second).Equals(Equal));
        }

        [Fact]
        public void Classify_overlap_as_Partial()
        {
            var businesHours = referenceSlot01h01h();
            var (start, dur) = businesHours;
            var overlaps = new[]
            {
                (start.Subtract(Minutes(10)), Minutes(20)),
                (start.Add(dur).Subtract(Minutes(10)), Minutes(20))
            };

            Assert.All(overlaps, o =>
                Assert.True(
                    ClassifyOverlap(o, businesHours).Equals(Intersect),
                    $"Should be partial was {ClassifyOverlap(o, businesHours)}"));
        }

        [Fact]
        public void Classify_overlap_as_propersubset()
        {
            var businessHours = referenceSlot01h01h();
            var (open, dur) = businessHours;

            var overlaps = new[]
                {
                    (open.Add(Minutes(10)), Minutes(20)),
                    (open.Add(Minutes(-30)), TimeSpan.FromHours(2))
                };

            Assert.All(overlaps, o =>
                Assert.True(
                    ClassifyOverlap(o, businessHours).Equals(ProperSubset),
                    $"Should be ProperSubset was {ClassifyOverlap(o, businessHours)}"));
        }
    }
    
}