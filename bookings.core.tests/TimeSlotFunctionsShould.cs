using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static bookings.core.TimeSlotFunctions;

namespace bookings.core.tests
{
    public class TimeSlotFunctionsShould
    {
        public static Func<(TimeSpan open, TimeSpan dur)> referenceSlot01h01h = () => (TimeSpan.FromHours(1), TimeSpan.FromHours(1));
        public static Func<(TimeSpan open, TimeSpan dur), IEnumerable<(TimeSpan, TimeSpan)>> NoOverlap = s =>
            Enumerable.Range(1, 1)
                .Select(x => (s.open.Add(TimeSpan.FromHours(-x - 1)), s.dur))
                .Concat(Enumerable.Range(1, 1)
                .Select(x => (s.open.Add(s.dur.Add(TimeSpan.FromHours(x))), s.dur)));

        

        [Fact]
        public void Classify_overlap_as_None()
        {
            var businessHours = referenceSlot01h01h();
            var overlaps = NoOverlap(businessHours);

            Assert.All(overlaps, x => Assert.True(ClassifyOverlap(x, businessHours).Equals(Overlap.None)));
        }

        [Fact]
        public void Classify_overlap_as_Equal()
        {

            var first = referenceSlot01h01h();
            var second = referenceSlot01h01h();

            Assert.True(ClassifyOverlap(first, second).Equals(Overlap.Equal));
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
                    ClassifyOverlap(o, businesHours).Equals(Overlap.Partial),
                    $"Should be partial was {ClassifyOverlap(o, businesHours)}"));
        }

        [Fact]
        public void Classify_overlap_as_Adjacent()
        {
            var businessHours = referenceSlot01h01h();
            var (start, dur) = businessHours;
            var overlap = new[] {
                (start.Subtract(dur), dur),
                (start.Add(dur), dur)
            };

            Assert.All(overlap, o =>
                Assert.True(
                    ClassifyOverlap(businessHours, o) == Overlap.Adjacent,
                    $"Should be Adjacent, was {ClassifyOverlap(businessHours, o)}"));
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
                    ClassifyOverlap(o, businessHours).Equals(Overlap.ProperSubset),
                    $"Should be ProperSubset was {ClassifyOverlap(o, businessHours)}"));
        }

        [Fact]
        public void Split_By_2()
        {
            var hours = referenceSlot01h01h();
            var result = Split(hours, 2);
            var half = hours.dur.Subtract(Minutes(30));
            var expected = new[] {
                (hours.open, half),
                (hours.open.Add(half), half)
            };

            Assert.Equal(result.Length, 2);
            Assert.All(
                result.Zip(expected, (r, e) => (result: Show(r), expected: Show(e))),
                x => Assert.Equal(x.expected, x.result));
        }

        [Fact]
        public void Subtract_non_overlapping()
        {
            var minuend = referenceSlot01h01h();
            var subtrahends = new [] {
                (minuend.open.Subtract(Minutes(20)), Minutes(10)),
                (minuend.open.Subtract(Minutes(10)), Minutes(10)),
                (End(minuend), Minutes(10)),
                (End(minuend).Add(Minutes(10)), Minutes(10))
            };

            Assert.All(subtrahends, o => Assert.Equal(Show(minuend), Show(Minus(minuend, o).SingleOrDefault())));
        }

        [Fact]
        public void Subtract_overlapping()
        {
            var minuend = referenceSlot01h01h();
            var subtrahends = new [] 
            {
                (minuend.open.Subtract(Minutes(10)), Minutes(20)),
                (End(minuend).Subtract(Minutes(10)), Minutes(20))
            };
            var expected = new []
            {
                (minuend.open.Add(Minutes(10)), minuend.dur.Subtract(Minutes(10))),
                (minuend.open, minuend.dur.Subtract(Minutes(10)))
            };

            Assert.All(
                subtrahends.Zip(expected, (a, e) => new { arg = a, expected = e}), 
                x => Assert.Equal(Show(x.expected), Show(Minus(minuend, x.arg).Single())));
        }

        [Fact]
        public void Subtract_proper_subset_subtrahend_of_minuend()
        {
            var minuend = referenceSlot01h01h();
            var subtrahend = (minuend.open.Add(Minutes(20)), Minutes(20));
            var expected = new [] 
            {
                (minuend.open, Minutes(20)),
                (minuend.open.Add(Minutes(40)), Minutes(20))
            };

            var result = Minus(minuend, subtrahend);

            Assert.Equal(2, result.Count());
            Assert.All(
                result.Zip(expected, (r, e) => new { result = r, expected = e }),
                x => Assert.Equal(Show(x.expected), Show(x.result)));
        }

        [Fact]
        public void Subtract_proper_subset_minuend_of_subtrahend()
        {
            var minuend = referenceSlot01h01h();
            var subrahend = (minuend.open.Subtract(Minutes(10)), minuend.dur.Add(Minutes(20)));

            Assert.Equal(Show((Zero, Zero)), Show(Minus(minuend, subrahend).Single()));
        }

        [Fact]
        public void Subtract_equal()
        {
            var minuend = referenceSlot01h01h();
            var subtrahend = referenceSlot01h01h();

            Assert.Equal(Show((Zero, Zero)), Show(Minus(minuend, subtrahend).Single()));
        }
    }
}