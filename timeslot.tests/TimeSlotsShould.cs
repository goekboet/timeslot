using System;
using Xunit;
using static timeslot.TimeSlot;
using static timeslot.Overlap;

namespace timeslot.tests
{
    public class TimeSlotShould
    {
        [Fact]
        public void Classify_overlap_as_None()
        {
            var timeSlot = (Minutes(60), Minutes(60));
            var overlaps = new [] 
            {
                (Minutes(40), Minutes(10)),
                (Minutes(130), Minutes(10))
            };

            Assert.All(overlaps, x => Assert.True(Overlap(x, timeSlot).Equals(None)));
        }

        [Fact]
        public void Classify_overlap_as_Equal()
        {

            var first = (Minutes(60), Minutes(60));
            var second = (Minutes(60), Minutes(60));

            Assert.True(Overlap(first, second).Equals(Equal));
        }

        [Fact]
        public void Classify_overlap_as_Partial()
        {
            var businesHours = (Minutes(60), Minutes(60));
            var (start, dur) = businesHours;
            var overlaps = new[]
            {
                (start.Subtract(Minutes(10)), Minutes(20)),
                (start.Add(dur).Subtract(Minutes(10)), Minutes(20))
            };

            Assert.All(overlaps, o =>
                Assert.True(
                    Overlap(o, businesHours).Equals(Intersect),
                    $"Should be partial was {Overlap(o, businesHours)}"));
        }

        // Continus terms intersect because we want union over many to merge continous result,
        // this classification makes that much simpler
        [Fact]
        public void Clasify_continous_spans_as_overlap_intersect()
        {
            var fst = (Minutes(60), Minutes(10));
            var snd = (Minutes(70), Minutes(10));

            var result = new[] { Overlap(fst, snd), Overlap(snd, fst)};

            Assert.All(result, x => Assert.True(x.Equals(Intersect), $"was: {x}"));
        }

        [Fact]
        public void Classify_overlap_as_propersubset()
        {
            var businessHours = (Minutes(60), Minutes(60));
            var (open, dur) = businessHours;

            var overlaps = new[]
                {
                    (open.Add(Minutes(10)), Minutes(20)),
                    (open.Add(Minutes(-30)), TimeSpan.FromHours(2))
                };

            Assert.All(overlaps, o =>
                Assert.True(
                    Overlap(o, businessHours).Equals(ProperSubset),
                    $"Should be ProperSubset was {Overlap(o, businessHours)}"));
        }
    }
    
}