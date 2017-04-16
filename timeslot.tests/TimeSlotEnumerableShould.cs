using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static timeslot.TimeSlot;
using static timeslot.TimeSlotEnumerable;

namespace timeslot.tests
{
    public class MultipliersShould
    {
        private static Func<(TimeSpan open, TimeSpan dur)> referenceSlot01h01h = () => (TimeSpan.FromHours(1), TimeSpan.FromHours(1));
        private static Func<(TimeSpan open, TimeSpan dur), IEnumerable<(TimeSpan, TimeSpan)>> NoOverlap = s =>
            Enumerable.Range(1, 1)
                .Select(x => (s.open.Add(TimeSpan.FromHours(-x - 1)), s.dur))
                .Concat(Enumerable.Range(1, 1)
                .Select(x => (s.open.Add(s.dur.Add(TimeSpan.FromHours(x))), s.dur)));

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
            var subtrahends = new[] {
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
            var subtrahends = new[]
            {
                (minuend.open.Subtract(Minutes(10)), Minutes(20)),
                (End(minuend).Subtract(Minutes(10)), Minutes(20))
            };
            var expected = new[]
            {
                (minuend.open.Add(Minutes(10)), minuend.dur.Subtract(Minutes(10))),
                (minuend.open, minuend.dur.Subtract(Minutes(10)))
            };

            Assert.All(
                subtrahends.Zip(expected, (a, e) => new { arg = a, expected = e }),
                x => Assert.Equal(Show(x.expected), Show(Minus(minuend, x.arg).Single())));
        }

        [Fact]
        public void Subtract_proper_subset_subtrahend_of_minuend()
        {
            var minuend = referenceSlot01h01h();
            var subtrahend = (minuend.open.Add(Minutes(20)), Minutes(20));
            var expected = new[]
            {
                (minuend.open.Add(Minutes(40)), Minutes(20)),
                (minuend.open, Minutes(20))
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
            var subtrahend = (minuend.open.Subtract(Minutes(10)), minuend.dur.Add(Minutes(20)));

            Assert.Empty(Minus(minuend, subtrahend));
        }

        [Fact]
        public void Subtract_equal()
        {
            var minuend = referenceSlot01h01h();
            var subtrahend = referenceSlot01h01h();

            Assert.Empty(Minus(minuend, subtrahend));
        }

        [Fact]
        public void Minus_subtrahend_aligned_with_end_of_minuend()
        {
            var minuend = (Minutes(60), Minutes(30));
            var subtrahend = (Minutes(80), Minutes(10));
            var expected = new[] {(Minutes(60), Minutes(20))};

            var result = Minus(minuend, subtrahend);

            Assert.Equal(ShowSlots(expected), ShowSlots(result));
        }

        [Fact]
        public void Minus_subtrahend_aligned_with_start_of_minuend()
        {
            var minuend = (Minutes(60), Minutes(30));
            var subtrahend = (Minutes(60), Minutes(10));
            var expected = new[] {(Minutes(70), Minutes(20))};

            var result = Minus(minuend, subtrahend);

            Assert.Equal(ShowSlots(expected), ShowSlots(result));
        }

        [Theory]
        [MemberData(nameof(Empty_or_null))]
        public void Minus_returns_empty_on_emty_or_null_minuend(
            (TimeSpan, TimeSpan)[] ms,
            (TimeSpan, TimeSpan)[] ds)
        {
            var result = Minus(ms, ds);
            Assert.Empty(result);
        }

        [Theory]
        [MemberData(nameof(Minus))]
        public void Minus_returns_correct_result(
            (TimeSpan, TimeSpan)[] ms,
            (TimeSpan, TimeSpan)[] ds,
            (TimeSpan, TimeSpan)[] es)
        {
            var result = Minus(ms, ds);

            Assert.True(es.Count() == result.Count(), ShowSlots(result));
            Assert.All(
                result.Zip(es, (r, e) => (expected: e, result: r)),
                x => Assert.Equal(Show(x.expected), Show(x.result)));
        }

        public static IEnumerable<object[]> Minus
        {
            get
            {
                yield return new[]
                {
                    new[] {(Hours(1), Minutes(30))},
                    Empty,
                    new[] {(Hours(1), Minutes(30))}
                };
                yield return new[]
                {
                    new []{(Hours(1), Minutes(30))},
                    new []{(Minutes(30), Minutes(30))},
                    new []{(Hours(1), Minutes(30))}
                };
                yield return new[]
                {
                    new []{(Hours(1), Minutes(30))},
                    new []{(Minutes(40), Minutes(30))},
                    new []{(Minutes(70), Minutes(20))}
                };
                yield return new[]
                {
                    new []{(Hours(1), Minutes(30))},
                    new []{(Hours(1), Minutes(10))},
                    new []{(Minutes(70), Minutes(20))}
                };
                yield return new[]
                {
                    new []{(Hours(1), Minutes(30))},
                    new []{(Minutes(70), Minutes(10))},
                    new []
                    {
                        (Minutes(80), Minutes(10)),
                        (Minutes(60), Minutes(10))
                    }
                };
                yield return new[]
                {
                    new []{(Minutes(60), Minutes(30))},
                    new []{(Minutes(80), Minutes(10))},
                    new []{(Minutes(60), Minutes(20))}
                };
                yield return new[]
                {
                    new []{(Minutes(60), Minutes(30))},
                    new []{(Minutes(90), Minutes(10))},
                    new []{(Minutes(60), Minutes(30))}
                };
                yield return new[]
                {
                    new []{(Minutes(60), Minutes(30))},
                    new []{(Minutes(100), Minutes(10))},
                    new []{(Minutes(60), Minutes(30))}
                };
                yield return new[]
                {
                    new[]
                    {
                        (Minutes(90), Minutes(30)),
                        (Minutes(60), Minutes(30))
                    },
                    new[]
                    {
                        (Minutes(90), Minutes(10)),
                        (Minutes(60), Minutes(10))
                    },
                    new[]
                    {
                        (Minutes(100), Minutes(20)),
                        (Minutes(70), Minutes(20))
                    }
                };
                yield return new[]
                {
                    new[]
                    {
                        (Minutes(60), Minutes(50))
                    },
                    new[]
                    {
                        (Minutes(100), Minutes(10)),
                        (Minutes(80), Minutes(10)),
                        (Minutes(60), Minutes(10))
                    },
                    new[]
                    {
                        (Minutes(90), Minutes(10)),
                        (Minutes(70), Minutes(10))
                    }
                };
                yield return new[]
                {
                    new[]
                    {
                        (Minutes(90), Minutes(30)),
                        (Minutes(60), Minutes(30))
                    },
                    new[]
                    {
                        (Minutes(110), Minutes(10)),
                        (Minutes(80), Minutes(20)),
                        (Minutes(60), Minutes(10))
                    },
                    new[]
                    {
                        (Minutes(100), Minutes(10)),
                        (Minutes(70), Minutes(10))
                    }
                };
            }
        }

        public static IEnumerable<object[]> Empty_or_null
        {
            get
            {
                yield return new(TimeSpan, TimeSpan)[][]
                {
                    null,
                    null
                };
                yield return new[]
                {
                    Empty,
                    null,
                };
                yield return new[]
                {
                    Empty,
                    new [] {(Zero, Zero)},
                };
                yield return new[]
                {
                    new[] {(Hours(1), Hours(1))},
                    new[] {(Hours(1), Hours(1))}
                };
            }
        }
    }
}