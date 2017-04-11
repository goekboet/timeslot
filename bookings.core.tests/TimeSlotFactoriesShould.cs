using System;
using System.Linq;
using Xunit;
using static bookings.core.TimeSlotFunctions;
using static bookings.core.TimeSlotFactories;

namespace bookings.core.tests
{
    public class TimeSlotFactoriesShould
    {
        private static readonly TimeSpan _open = TimeSpan.FromHours(9);
        private static readonly TimeSpan _close = TimeSpan.FromHours(8);
        private static Func<int, (TimeSpan, TimeSpan)[]> _sut = Weekdays(_open, _close);

        
        
        [Fact]
        public void MakeWeekdayBusinessHourRuleReturnEmptyBusinesHourOnWeekends()
        {
            var result = Enumerable.Range(5, 3).Select(i => _sut(i));

            Assert.All(result, x => Assert.Empty(x));
        }

        [Fact]
        public void MakeCorrectWeekdayBusinessHours()
        {
            var expected = Show((_open, _close));
            var result = Enumerable.Range(0, 5).Select(i => _sut(i));

            Assert.False(result.Any(x => x.Count() < 1), "Failed to make slot for weekday");
            Assert.All(result.SelectMany(x => x), x => Assert.Equal(expected, Show(x)));
        }
    }
}