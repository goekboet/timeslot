using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static bookings.core.TimeSlotFunctions;

namespace bookings.core.tests
{
    public class TimeSlotFunctionsShould
    {
        public static Func<(TimeSpan, TimeSpan)> slotmaker = () => (TimeSpan.FromHours(1), TimeSpan.FromHours(1));
        public static Func<(TimeSpan open, TimeSpan dur), IEnumerable<(TimeSpan, TimeSpan)>> NoOverlap = s => 
            Enumerable.Range(0, 2)
                .Select(x => (s.open.Add(TimeSpan.FromHours(-x - 1)), s.dur))
                .Concat(Enumerable.Range(0, 2)
                .Select(x => (s.open.Add(s.dur.Add(TimeSpan.FromHours(x))), s.dur))); 

        [Fact]
        public void GetCorrectNoneOverlap()
        {
            var businessHours = slotmaker();
            var overlaps = NoOverlap(businessHours);

            Assert.All(overlaps, x => Assert.True(GetOverlap(x, businessHours).Equals(Overlap.None)));
        }

        [Fact]
        public void GetCorrectEqualOverlap()
        {
            
            var first = slotmaker();
            var second = slotmaker();

            Assert.True(GetOverlap(first, second).Equals(Overlap.Equal));
        }

        [Fact]
        public void GetCorrectPartialOverlap()
        {
            var businesHours = slotmaker();
            var (start, dur) = businesHours;
            var overlaps = Enumerable.Range(0, 1)
                .Select(x => (start.Add(TimeSpan.FromMinutes(x == 0 ? -30 : 30)), dur));
            
            Assert.All(overlaps, o => GetOverlap(o, businesHours).Equals(Overlap.Partial));
        }

        [Fact]
        public void GetCorrectCompleteOverlap()
        {
            var businessHours = slotmaker();
            var (open, dur) = businessHours;

            var overlaps = Enumerable.Range(0, 3)
                .Select(x => (open.Add(TimeSpan.FromMinutes(20 * x)), TimeSpan.FromMinutes(20)))
                .Concat(new [] 
                {
                    (open.Add(TimeSpan.FromMinutes(-30)), TimeSpan.FromHours(2)),
                });

            Assert.All(overlaps, o => GetOverlap(o, businessHours).Equals(Overlap.Complete));
        }

        [Fact]
        public void SplitNoOverlapCorrectly()
        {
            var businesHours = slotmaker();
            Assert.All(NoOverlap(
                businesHours).SelectMany(x => Split(businesHours, x)), 
                x => Assert.Equal(Show(businesHours), Show(x)));
        }
    }
}