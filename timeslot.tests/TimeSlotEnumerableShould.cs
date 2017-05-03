using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static timeslot.TimeSlot;
using static timeslot.TimeSlotEnumerable;

namespace timeslot.tests
{
    public class TimeSlotEnumerableShould
    {
        private static Func<(TimeSpan open, TimeSpan dur)> referenceSlot01h01h = () => (TimeSpan.FromHours(1), TimeSpan.FromHours(1));
        private static Func<(TimeSpan, TimeSpan), (TimeSpan, TimeSpan), ((TimeSpan, TimeSpan) e, (TimeSpan, TimeSpan) r)> Pairwise = (e, r) => (e: e, r: r);

        public static string ShowSlots(IEnumerable<(TimeSpan, TimeSpan)> slots)
        {
            return string.Join("\n", slots.Select(x => Show(x)));
        }

        [Fact]
        public void Subtract_distinct_terms()
        {
            var firstTerm = (Minutes(60), Minutes(60));
            var secondTerms = new[] {
                (Minutes(40), Minutes(10)),
                (Minutes(50), Minutes(10)),
                (Minutes(120), Minutes(10)),
                (Minutes(130), Minutes(10))
            };

            Assert.All(
                secondTerms, 
                o => Assert.Equal(Show(firstTerm), Show(Difference(firstTerm, o).Single())));
        }

        [Fact]
        public void Union_distinct_terms()
        {
            var firstTerm = (Minutes(30), Minutes(20));
            var secondTerm = (Minutes(60), Minutes(20));
            var expect = new[]
            {
                (Minutes(30), Minutes(20)),
                (Minutes(60), Minutes(20))
            };

            var result = Union(firstTerm, secondTerm);

            Assert.Equal(expect.Count(), result.Count());
            Assert.All(
                expect.Zip(result, Pairwise), 
                x => Assert.Equal(Show(x.e), Show(x.r)));
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
            Assert.All(
                expected.Zip(result, Pairwise), 
                x => Assert.Equal(Show(x.e), Show(x.r)));
        }

        [Fact]
        public void Union_terms_intersect()
        {
            var terms = new[]
            {
                (Minutes(50), Minutes(20)),
                (Minutes(60), Minutes(20))
            };

            var expected = (Minutes(50), Minutes(30));
            var results = Union(terms[0], terms[1])
                .Concat(Union(terms[1], terms[0]))
                .ToArray();

            Assert.Equal(Show(results[0]), Show(results[1]));
            Assert.Equal(Show(expected), Show(results[0]));
        }

        [Fact]
        public void Subtract_second_term_is_proper_subset_of_the_first()
        {
            var firstTerm = (Minutes(60), Minutes(60));
            var secondTerm = (Minutes(80), Minutes(20));
            var expected = new[]
            {
                (Minutes(60), Minutes(20)),
                (Minutes(100), Minutes(20))
            };

            var result = Difference(firstTerm, secondTerm);

            Assert.Equal(2, result.Count());
            Assert.All(
                result.Zip(expected, (r, e) => new { result = r, expected = e }),
                x => Assert.Equal(Show(x.expected), Show(x.result)));
        }

        [Fact]
        public void Union_one_term_is_proper_subset_of_the_other()
        {
            var terms = new[]
            {
                (Minutes(60), Minutes(30)),
                (Minutes(70), Minutes(10))
            };
            var expected = (Minutes(60), Minutes(30));

            var result = Union(terms[0], terms[1])
                    .Concat(Union(terms[1], terms[0]))
                    .ToArray();


            Assert.Equal(Show(result[0]), Show(result[1]));
            Assert.Equal(Show(expected), Show(result[0]));
        }

        [Fact]
        public void Union_terms_are_continous()
        {
            var terms = new[]
            {
                (Minutes(60), Minutes(20)),
                (Minutes(80), Minutes(40))
            };
            var expected = (Minutes(60), Minutes(60));

            var result = Union(terms[0], terms[1])
                .Concat(Union(terms[1], terms[0]))
                .ToArray();

            Assert.Equal(Show(result[0]), Show(result[1]));
            Assert.Equal(Show(expected), Show(result[0]));
        }

        [Fact]
        public void Union_terms_are_equal()
        {
            var terms = new[]
            {
                (Minutes(60), Minutes(60)),
                (Minutes(60), Minutes(60))
            };
            var expected = (Minutes(60), Minutes(60));

            var result = Union(terms[0], terms[1])
                .Concat(Union(terms[1], terms[0]))
                .ToArray();

            Assert.Equal(Show(result[0]), Show(result[1]));
            Assert.Equal(Show(expected), Show(result[0]));
        }

        [Fact]
        public void Subtract_first_term_is_proper_subset_of_the_second()
        {
            var firstTerm = (Minutes(60), Minutes(60));
            var secondTerm = (Minutes(50), Minutes(80));

            Assert.Empty(Difference(firstTerm, secondTerm));
        }

        [Fact]
        public void Subtract_equal_terms()
        {
            var firstTerm = (Minutes(60), Minutes(60));
            var secondTerm = (Minutes(60), Minutes(60));

            Assert.Empty(Difference(firstTerm, secondTerm));
        }

        [Fact]
        public void Difference_terms_are_successors()
        {
            var firstTerm = (Minutes(60), Minutes(30));
            var secondTerm = (Minutes(80), Minutes(10));
            var expected = new[] { (Minutes(60), Minutes(20)) };

            var result = Difference(firstTerm, secondTerm);

            Assert.Equal(ShowSlots(expected), ShowSlots(result));
        }

        [Fact]
        public void Difference_terms_share_open_TimeSpan()
        {
            var firstTerm = (Minutes(60), Minutes(30));
            var secondTerm = (Minutes(60), Minutes(10));
            var expected = new[] { (Minutes(70), Minutes(20)) };

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
                es.Zip(result, Pairwise),
                x => Assert.Equal(Show(x.e), Show(x.r)));
        }

        [Theory]
        [MemberData(nameof(Union))]
        public void Union_returns_correct_result(
            (TimeSpan, TimeSpan)[] fsts,
            (TimeSpan, TimeSpan)[] snds,
            (TimeSpan, TimeSpan)[] exps
        )
        {
            var result = Union(fsts, snds);

            Assert.True(exps.Count() == result.Count(), ShowSlots(result));
            Assert.All(
                exps.Zip(result, Pairwise),
                x => Assert.Equal(Show(x.e), Show(x.r)));
        }

        [Theory]
        [MemberData(nameof(IntersectionSingle))]
        public void apply_intersection_correctly_on_single_terms(
            (TimeSpan, TimeSpan) fst,
            (TimeSpan, TimeSpan) snd,
            (TimeSpan, TimeSpan)[] exp)
        {
            var result = Intersection(fst, snd)
                .Concat(Intersection(snd, fst));

            Assert.True(exp.Count() * 2 == result.Count(), ShowSlots(result));
            Assert.All(
                result.Zip(exp, Pairwise),
                x => Assert.Equal(Show(x.e), Show(x.r)));
        }

        [Theory]
        [MemberData(nameof(InterSectionMany))]
        public void apply_intersection_correctly_on_multiple_terms(
            IEnumerable<(TimeSpan, TimeSpan)> fst,
            IEnumerable<(TimeSpan, TimeSpan)> snd,
            IEnumerable<(TimeSpan, TimeSpan)> exp,
            bool reverseargs = true
        )
        {
            var res = Intersection(fst, snd);

            Assert.True(exp.Count() == res.Count(), ShowSlots(res));
            Assert.All(
                exp.Zip(res, Pairwise), 
                x => Assert.Equal(Show(x.e), Show(x.r)));
            
            if(reverseargs)
                apply_intersection_correctly_on_multiple_terms(snd, fst, exp, false);
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
                yield return new[] //fst before and disjunct from snd
                {
                    new[]
                    {
                        (Minutes(50), Minutes(10)),
                        (Minutes(70), Minutes(10)),
                        (Minutes(90), Minutes(10))
                    },
                    new[]
                    {
                        (Minutes(110), Minutes(10))
                    },
                    new[]
                    {
                        (Minutes(50), Minutes(10)),
                        (Minutes(70), Minutes(10)),
                        (Minutes(90), Minutes(10))
                    }
                };
                yield return new[] //fst after and disjunct from snd
                {
                    new[]
                    {
                        (Minutes(110), Minutes(10))
                    },
                    new[]
                    {
                        (Minutes(50), Minutes(10)),
                        (Minutes(70), Minutes(10)),
                        (Minutes(90), Minutes(10))
                    },
                    new[]
                    {
                        (Minutes(110), Minutes(10))
                    },
                };
                yield return new[] //fst preceded and followed by distinct snd
                {
                    new[]
                    {
                        (Minutes(50), Minutes(10)),
                        (Minutes(90), Minutes(10))
                    },
                    new[]
                    {
                        (Minutes(70), Minutes(10)),
                    },
                    new[]
                    {
                        (Minutes(50), Minutes(10)),
                        (Minutes(90), Minutes(10))
                    }
                };
                yield return new[] //fst preceded and followed by distinct snd
                {
                    new[]
                    {
                        (Minutes(50), Minutes(20)),
                        (Minutes(80), Minutes(20))
                    },
                    new[]
                    {
                        (Minutes(60), Minutes(30)),
                    },
                    new[]
                    {
                        (Minutes(50), Minutes(10)),
                        (Minutes(90), Minutes(10))
                    }
                };
                yield return new[]
                {
                    new[]
                    {
                        (Minutes(50), Minutes(20)), //overlap snd from left
                        (Minutes(80), Minutes(10)), //proper subset of snd
                        (Minutes(100), Minutes(20)) //overlap snd from right
                    },
                    new[]
                    {
                        (Minutes(60), Minutes(50)),
                    },
                    new[]
                    {
                        (Minutes(50), Minutes(10)),
                        (Minutes(110), Minutes(10))
                    }
                };
                yield return new[]
                {
                    new[]
                    {
                        (Minutes(60), Minutes(30)),
                    },
                    new[]
                    {
                        (Minutes(50), Minutes(20)), //overlap fst from left
                        (Minutes(80), Minutes(20)) //overlap fst from right
                    },
                    new[]
                    {
                        (Minutes(70), Minutes(10)),
                    }
                };
                yield return new[]
                {
                    new[]
                    {
                        (Minutes(50), Minutes(10)),
                        (Minutes(80), Minutes(30)),
                        (Minutes(130), Minutes(10))
                    },
                    new[]
                    {
                        (Minutes(70), Minutes(20)), //overlap fst from left
                        (Minutes(100), Minutes(20)) //overlap fst from right
                    },
                    new[]
                    {
                        (Minutes(50), Minutes(10)),
                        (Minutes(90), Minutes(10)),
                        (Minutes(130), Minutes(10))
                    }
                };
                yield return new[]
                {
                    new[]
                    {
                        (Minutes(50), Minutes(20)),
                        (Minutes(80), Minutes(30)),
                        (Minutes(120), Minutes(20))
                    },
                    new[]
                    {
                        (Minutes(60), Minutes(30)), //overlap fst from left
                        (Minutes(100), Minutes(30)) //overlap fst from right
                    },
                    new[]
                    {
                        (Minutes(50), Minutes(10)),
                        (Minutes(90), Minutes(10)),
                        (Minutes(130), Minutes(10))
                    }
                };
                yield return new[]
                {
                    new[]
                    {
                        (Minutes(50), Minutes(70))
                    },
                    new[]
                    {
                        (Minutes(60), Minutes(10)),
                        (Minutes(80), Minutes(10)),
                        (Minutes(100), Minutes(10))
                    },
                    new[]
                    {
                        (Minutes(50), Minutes(10)),
                        (Minutes(70), Minutes(10)),
                        (Minutes(90), Minutes(10)),
                        (Minutes(110), Minutes(10))
                    }
                };
                yield return new[] //terms share open timespan
                {
                    new []{(Hours(1), Minutes(30))},
                    new []{(Hours(1), Minutes(10))},
                    new []{(Minutes(70), Minutes(20))}
                };
                yield return new[] // terms share end
                {
                    new []{(Minutes(60), Minutes(30))},
                    new []{(Minutes(80), Minutes(10))},
                    new []{(Minutes(60), Minutes(20))}
                };
                yield return new[]
                {
                    new[]
                    {
                        (Minutes(60), Minutes(60))
                    },
                    new[]
                    {
                        (Minutes(70), Minutes(10)),
                        (Minutes(90), Minutes(10)),
                        (Minutes(110), Minutes(10))
                    },
                    new[]
                    {
                        (Minutes(60), Minutes(10)),
                        (Minutes(80), Minutes(10)),
                        (Minutes(100), Minutes(10))
                    }
                };
            }
        }

        public static IEnumerable<object[]> Union
        {
            get
            {
                yield return new[] {
                    new[] {
                        (Minutes(60), Minutes(20))
                    },
                    Empty,
                    new[] {
                        (Minutes(60), Minutes(20))
                    },
                };
                yield return new[] {
                    Empty,
                    new[] {
                        (Minutes(60), Minutes(20)),
                        (Minutes(90), Minutes(20))
                    },
                    new[] {
                        (Minutes(60), Minutes(20)),
                        (Minutes(90), Minutes(20))
                    },
                };
                yield return new[] {
                    new [] {
                        (Minutes(60), Minutes(10)),
                        (Minutes(80), Minutes(10)),
                        (Minutes(100), Minutes(10))
                    },
                    new [] {
                        (Minutes(120), Minutes(10))
                    },
                    new [] {
                        (Minutes(60), Minutes(10)),
                        (Minutes(80), Minutes(10)),
                        (Minutes(100), Minutes(10)),
                        (Minutes(120), Minutes(10))
                    }
                };
                yield return new[] {
                    new [] {
                        (Minutes(120), Minutes(10))
                    },
                    new [] {
                        (Minutes(60), Minutes(10)),
                        (Minutes(80), Minutes(10)),
                        (Minutes(100), Minutes(10))
                    },
                    new [] {
                        (Minutes(60), Minutes(10)),
                        (Minutes(80), Minutes(10)),
                        (Minutes(100), Minutes(10)),
                        (Minutes(120), Minutes(10))
                    }
                };
                yield return new[] {
                    new [] {
                        (Minutes(60), Minutes(10)),
                        (Minutes(100), Minutes(10))
                    },
                    new [] {
                        (Minutes(80), Minutes(10)),
                    },
                    new [] {
                        (Minutes(60), Minutes(10)),
                        (Minutes(80), Minutes(10)),
                        (Minutes(100), Minutes(10))
                    }
                };
                yield return new[] {
                    new [] {
                        (Minutes(60), Minutes(20)),
                        (Minutes(100), Minutes(20))
                    },
                    new [] {
                        (Minutes(70), Minutes(20)),
                    },
                    new [] {
                        (Minutes(60), Minutes(30)),
                        (Minutes(100), Minutes(20))
                    }
                };
                yield return new[] {
                    new [] {
                        (Minutes(60), Minutes(20)),
                        (Minutes(100), Minutes(20))
                    },
                    new [] {
                        (Minutes(90), Minutes(20)),
                    },
                    new [] {
                        (Minutes(60), Minutes(20)),
                        (Minutes(90), Minutes(30))
                    }
                };
                yield return new[] {
                    new [] {
                        (Minutes(60), Minutes(20)),
                        (Minutes(100), Minutes(20))
                    },
                    new [] {
                        (Minutes(70), Minutes(40)),
                    },
                    new [] {
                        (Minutes(60), Minutes(60))
                    }
                };
                yield return new[] {
                    new [] {
                        (Minutes(70), Minutes(40)),
                    },
                    new [] {
                        (Minutes(60), Minutes(20)),
                        (Minutes(100), Minutes(20))
                    },
                    new [] {
                        (Minutes(60), Minutes(60))
                    }
                };
                yield return new[] {
                    new [] {
                        (Minutes(60), Minutes(10)),
                        (Minutes(80), Minutes(10))
                    },
                    new [] {
                        (Minutes(70), Minutes(10))
                    },
                    new [] {
                        (Minutes(60), Minutes(30))
                    }
                };
                yield return new[] {
                    new [] {
                        (Minutes(60), Minutes(70))
                    },
                    new [] {
                        (Minutes(70), Minutes(10)),
                        (Minutes(90), Minutes(10)),
                        (Minutes(110), Minutes(10))
                    },
                    new [] {
                        (Minutes(60), Minutes(70))
                    }
                };
                yield return new[] {
                    new [] {
                        (Minutes(60), Minutes(20)),
                        (Minutes(100), Minutes(10)),
                        (Minutes(120), Minutes(10))
                    },
                    new [] {
                        (Minutes(70), Minutes(20)),
                        (Minutes(110), Minutes(10))
                    },
                    new [] {
                        (Minutes(60), Minutes(30)),
                        (Minutes(100), Minutes(30))
                    }
                };
            }
        }

        public static IEnumerable<object[]> IntersectionSingle
        {
            get
            {
                yield return new object[] {
                    (Minutes(60), Minutes(10)),
                    (Minutes(80), Minutes(10)),
                    Empty
                };
                yield return new object[] {
                    (Minutes(60), Minutes(10)),
                    (Minutes(70), Minutes(10)),
                    Empty
                };
                yield return new object[] {
                    (Minutes(60), Minutes(20)),
                    (Minutes(70), Minutes(20)),
                    new [] {(Minutes(70), Minutes(10))}
                };
                yield return new object[] {
                    (Minutes(60), Minutes(10)),
                    (Minutes(60), Minutes(10)),
                    new [] {(Minutes(60), Minutes(10))}
                };
                yield return new object[] {
                    (Minutes(60), Minutes(20)),
                    (Minutes(60), Minutes(10)),
                    new [] {(Minutes(60), Minutes(10))}
                };
                yield return new object[] {
                    (Minutes(60), Minutes(20)),
                    (Minutes(70), Minutes(10)),
                    new [] {(Minutes(70), Minutes(10))}
                };
                yield return new object[] {
                    (Minutes(100), Minutes(20)),
                    (Minutes(70), Minutes(40)),
                    new [] {(Minutes(100), Minutes(10))}
                };
                yield return new object[] {
                    (Minutes(60), Minutes(30)),
                    (Minutes(70), Minutes(10)),
                    new [] {(Minutes(70), Minutes(10))}
                };
            }
        }

        public static IEnumerable<object[]> InterSectionMany
        {
            get
            {
                yield return new object[] {
                    Empty,
                    Empty,
                    Empty,
                    false
                };
                yield return new object[] {
                    Empty,
                    new[] { (Minutes(60), Minutes(60)) },
                    Empty
                };
                yield return new object[] {
                    new[] { 
                        (Minutes(60), Minutes(10)),
                        (Minutes(80), Minutes(10)) 
                    },
                    new[] { 
                        (Minutes(100), Minutes(10))
                    },
                    Empty
                };
                yield return new object[] {
                    new[] { 
                        (Minutes(60), Minutes(10)),
                        (Minutes(80), Minutes(10)) 
                    },
                    new[] { 
                        (Minutes(70), Minutes(10)),
                        (Minutes(90), Minutes(10))
                    },
                    Empty
                };
                yield return new object[] {
                    new[] { 
                        (Minutes(60), Minutes(20)),
                        (Minutes(100), Minutes(20)) 
                    },
                    new[] { 
                        (Minutes(70), Minutes(20))
                    },
                    new[] {
                        (Minutes(70), Minutes(10))
                    }
                };
                yield return new object[] {
                    new[] { 
                        (Minutes(60), Minutes(20)),
                        (Minutes(100), Minutes(20)) 
                    },
                    new[] { 
                        (Minutes(90), Minutes(20))
                    },
                    new[] {
                        (Minutes(100), Minutes(10))
                    }
                };
                yield return new object[] {
                    new[] { 
                        (Minutes(60), Minutes(20)),
                        (Minutes(100), Minutes(20)) 
                    },
                    new[] { 
                        (Minutes(70), Minutes(40))
                    },
                    new[] {
                        (Minutes(70), Minutes(10)),
                        (Minutes(100), Minutes(10))
                    }
                };
                yield return new object[] {
                    new[] {
                        (Minutes(60), Minutes(80))
                    },
                    new[] {
                        (Minutes(70), Minutes(10)),
                        (Minutes(90), Minutes(10)),
                        (Minutes(120), Minutes(10))
                    },
                    new [] {
                        (Minutes(70), Minutes(10)),
                        (Minutes(90), Minutes(10)),
                        (Minutes(120), Minutes(10))
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