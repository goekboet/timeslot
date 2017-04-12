using System;
using System.Linq;
using Xunit;
using static bookings.core.TimeSlotFunctions;
using static bookings.core.TimeSlotFactories;
using System.Collections.Generic;

namespace bookings.core.tests
{
    public class TimeSlotFactoriesShould
    {
        private static readonly TimeSpan _open = TimeSpan.FromHours(9);
        private static readonly TimeSpan _close = TimeSpan.FromHours(8);
        private static Func<int, IEnumerable<(TimeSpan, TimeSpan)>> _referenceweek0908 = Weekdays(_open, _close);

        
        
        [Fact]
        public void MakeWeekdayBusinessHourRuleReturnEmptyBusinesHourOnWeekends()
        {
            var result = Enumerable.Range(5, 2).Select(i => _referenceweek0908(i));

            Assert.All(result, x => Assert.Empty(x));
        }

        [Fact]
        public void MakeCorrectWeekdayBusinessHours()
        {
            var expected = Show((_open, _close));
            var result = Enumerable.Range(0, 5)
                .Concat(Enumerable.Range(7, 5))
                .Select(i => _referenceweek0908(i));

            Assert.Equal(10, result.Count());
            Assert.All(result.SelectMany(x => x), x => Assert.Equal(expected, Show(x)));
        }

        private static Func<(TimeSpan o, TimeSpan d), IEnumerable<(TimeSpan o, TimeSpan d)>> currySplit(int d) => slots => Split(slots, d).AsEnumerable();
        
        [Fact]
        public void Construct_slots_with_businesshours_shifts_and_breaks()
        {
            var slots = BusinessWeek(
                _referenceweek0908,
                currySplit(8),
                _ => new [] { (Hours(12), Hours(1))}
            );

            Assert.True(false);
        }
    }
}