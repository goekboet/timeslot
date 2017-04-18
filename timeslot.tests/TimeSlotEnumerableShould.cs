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
        private static Func<(TimeSpan, TimeSpan),(TimeSpan, TimeSpan), ((TimeSpan, TimeSpan) e,(TimeSpan, TimeSpan) r)> Pairwise = (e, r) => (e: e, r: r);

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
        public void Subtract_distinct_terms()
        {
            var firstTerm = referenceSlot01h01h();
            var secondTerms = new[] {
                (firstTerm.open.Subtract(Minutes(20)), Minutes(10)),
                (firstTerm.open.Subtract(Minutes(10)), Minutes(10)),
                (End(firstTerm), Minutes(10)),
                (End(firstTerm).Add(Minutes(10)), Minutes(10))
            };

            Assert.All(secondTerms, o => Assert.Equal(Show(firstTerm), Show(Difference(firstTerm, o).Single())));
        }

        [Fact]
        public void Union_distinct_terms()
        {
            var firstTerm = (Minutes(30), Minutes(20));
            var secondTerm = (Minutes(60), Minutes(20));
            var expect = new [] 
            { 
                (Minutes(60), Minutes(20)),
                (Minutes(30), Minutes(20))
            };

            var result = Union(firstTerm, secondTerm);

            Assert.Equal(expect.Count(), result.Count());
            Assert.All(expect.Zip(result, Pairwise), x => Assert.Equal(Show(x.e), Show(x.r)));
        }

        [Fact]
        public void Subtract_terms_intersect()
        {
            var firstTerm = (Minutes(60), Minutes(60));
            var secondTerms = new[]
            {
                (Minutes(50), Minutes(20)),
                (Minutes(110), Minutes(20)),
            };
            var expected = new[]
            {
                (Minutes(70), Minutes(50)),
                (Minutes(60), Minutes(50))
            };

            var result = secondTerms.SelectMany(x => Difference(firstTerm, x));

            Assert.Equal(expected.Count(), result.Count());
            Assert.All(expected.Zip(result, Pairwise), x => Assert.Equal(Show(x.e), Show(x.r)));
        }

        [Fact]
        public void Subtract_second_term_is_proper_subset_of_the_first()
        {
            var firstTerm = referenceSlot01h01h();
            var secondTerm = (firstTerm.open.Add(Minutes(20)), Minutes(20));
            var expected = new[]
            {
                (firstTerm.open.Add(Minutes(40)), Minutes(20)),
                (firstTerm.open, Minutes(20))
            };

            var result = Difference(firstTerm, secondTerm);

            Assert.Equal(2, result.Count());
            Assert.All(
                result.Zip(expected, (r, e) => new { result = r, expected = e }),
                x => Assert.Equal(Show(x.expected), Show(x.result)));
        }

        [Fact]
        public void Subtract_first_term_is_proper_subset_of_the_second()
        {
            var firstTerm = referenceSlot01h01h();
            var secondTerm = (firstTerm.open.Subtract(Minutes(10)), firstTerm.dur.Add(Minutes(20)));

            Assert.Empty(Difference(firstTerm, secondTerm));
        }

        [Fact]
        public void Subtract_equal_terms()
        {
            var firstTerm = referenceSlot01h01h();
            var secondTerm = referenceSlot01h01h();

            Assert.Empty(Difference(firstTerm, secondTerm));
        }

        [Fact]
        public void Difference_terms_are_successors()
        {
            var firstTerm = (Minutes(60), Minutes(30));
            var secondTerm = (Minutes(80), Minutes(10));
            var expected = new[] {(Minutes(60), Minutes(20))};

            var result = Difference(firstTerm, secondTerm);

            Assert.Equal(ShowSlots(expected), ShowSlots(result));
        }

        [Fact]
        public void Difference_terms_share_open_TimeSpan()
        {
            var firstTerm = (Minutes(60), Minutes(30));
            var secondTerm = (Minutes(60), Minutes(10));
            var expected = new[] {(Minutes(70), Minutes(20))};

            var result = Difference(firstTerm, secondTerm);

            Assert.Equal(ShowSlots(expected), ShowSlots(result));
        }

        [Theory]
        [MemberData(nameof(Empty_or_null))]
        public void Difference_returns_empty_on_emty_or_null_first_term(
            (TimeSpan, TimeSpan)[] ms,
            (TimeSpan, TimeSpan)[] ds)
        {
            var result = Difference(ms, ds);
            Assert.Empty(result);
        }

        [Theory]
        [MemberData(nameof(Difference))]
        public void Difference_returns_correct_result(
            (TimeSpan, TimeSpan)[] fs,
            (TimeSpan, TimeSpan)[] ss,
            (TimeSpan, TimeSpan)[] es)
        {
            var result = Difference(fs, ss);

            Assert.True(es.Count() == result.Count(), ShowSlots(result));
            Assert.All(
                result.Zip(es, (r, e) => (expected: e, result: r)),
                x => Assert.Equal(Show(x.expected), Show(x.result)));
        }


        public static IEnumerable<object[]> Difference
        {
            get
            {
                yield return new[] //Empty second term
                {
                    new[] {(Hours(1), Minutes(30))},
                    Empty,
                    new[] {(Hours(1), Minutes(30))}
                };
                yield return new[] //Disjunct terms
                {
                    new []{(Hours(1), Minutes(30))},
                    new []{(Minutes(30), Minutes(30))},
                    new []{(Hours(1), Minutes(30))}
                };
                yield return new[] //terms intersect to the left
                {
                    new []{(Hours(1), Minutes(30))},
                    new []{(Minutes(40), Minutes(30))},
                    new []{(Minutes(70), Minutes(20))}
                };
                yield return new[] //terms share open timespan
                {
                    new []{(Hours(1), Minutes(30))},
                    new []{(Hours(1), Minutes(10))},
                    new []{(Minutes(70), Minutes(20))}
                };
                yield return new[] //second term is proper subset of the first
                {
                    new []{(Hours(1), Minutes(30))},
                    new []{(Minutes(70), Minutes(10))},
                    new []
                    {
                        (Minutes(80), Minutes(10)),
                        (Minutes(60), Minutes(10))
                    }
                };
                yield return new[] // terms intersect to the right
                {
                    new []{(Minutes(60), Minutes(30))},
                    new []{(Minutes(80), Minutes(10))},
                    new []{(Minutes(60), Minutes(20))}
                };
                yield return new[] // terms share end
                {
                    new []{(Minutes(60), Minutes(30))},
                    new []{(Minutes(90), Minutes(10))},
                    new []{(Minutes(60), Minutes(30))}
                };
                yield return new[] // terms are disjunct second term follows first
                {
                    new []{(Minutes(60), Minutes(30))},
                    new []{(Minutes(100), Minutes(10))},
                    new []{(Minutes(60), Minutes(30))}
                };
                yield return new[] // first and second term map sequentially
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
                yield return new[] // first first term apply to the first two second terms. last second term disjunct
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
                yield return new[] //first first term applies to first second term. Second first term applies to second and third second term.
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