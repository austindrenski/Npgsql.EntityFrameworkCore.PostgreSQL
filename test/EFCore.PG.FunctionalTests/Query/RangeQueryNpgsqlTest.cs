using System.Collections.Generic;
using System.Linq;
using NpgsqlTypes;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    /// Provides unit tests for range operator translations.
    /// </summary>
    public class RangeQueryNpgsqlTest : IClassFixture<RangeQueryNpgsqlFixture>
    {
        /// <summary>
        /// Provides resources for unit tests.
        /// </summary>
        RangeQueryNpgsqlFixture Fixture { get; }

        /// <summary>
        /// Provides theory data for integers.
        /// </summary>
        public static IEnumerable<object[]> IntegerTheoryData => Enumerable.Range(-5, 10).Select(x => new object[] { x });

        /// <summary>
        /// Provides theory data for ranges.
        /// </summary>
        public static IEnumerable<object[]> RangeTheoryData =>
            new List<object[]>
            {
                // (0,5)
                new object[] { new NpgsqlRange<int>(0, false, false, 5, false, false) },
                // [0,5]
                new object[] { new NpgsqlRange<int>(0, true, false, 5, true, false) },
                // (,)
                new object[] { new NpgsqlRange<int>(0, false, true, 0, false, true) },
                // (,)
                new object[] { new NpgsqlRange<int>(0, false, true, 5, false, true) },
                // (0,)
                new object[] { new NpgsqlRange<int>(0, false, false, 0, false, true) },
                // (0,)
                new object[] { new NpgsqlRange<int>(0, false, false, 5, false, true) },
                // (,5)
                new object[] { new NpgsqlRange<int>(0, false, true, 5, false, false) }
            };

        /// <summary>
        /// Initializes resources for unit tests.
        /// </summary>
        /// <param name="fixture">
        /// The fixture of resources for testing.
        /// </param>
        public RangeQueryNpgsqlTest(RangeQueryNpgsqlFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        /// <summary>
        /// Tests containment translation for <see cref="NpgsqlRangeExtensions.Contains{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void ContainsRange(NpgsqlRange<int> range)
        {
            using (RangeContext context0 = Fixture.CreateContext())
            using (RangeContext context1 = Fixture.CreateContext())
            {
                RangeTestEntity[] sqlActual =
                    context0.RangeTestEntities
                            .Where(x => x.Range.Contains(range))
                            .ToArray();

                RangeTestEntity[] clientActual =
                    context1.RangeTestEntities
                            .ToArray()
                            .Where(x => x.Range.Contains(range))
                            .ToArray();

                Assert.Contains("WHERE \"x\".\"Range\" @> @__range_0 = TRUE", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(sqlActual.Length, clientActual.Length);
            }
        }

        /// <summary>
        /// Tests containment translation for <see cref="NpgsqlRangeExtensions.Contains{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void DoesNotContainRange(NpgsqlRange<int> range)
        {
            using (RangeContext context0 = Fixture.CreateContext())
            using (RangeContext context1 = Fixture.CreateContext())
            {
                RangeTestEntity[] sqlActual =
                    context0.RangeTestEntities
                            .Where(x => !x.Range.Contains(range))
                            .ToArray();

                RangeTestEntity[] clientActual =
                    context1.RangeTestEntities
                            .ToArray()
                            .Where(x => !x.Range.Contains(range))
                            .ToArray();

                Assert.Contains("WHERE NOT (\"x\".\"Range\" @> @__range_0 = TRUE)", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(sqlActual.Length, clientActual.Length);
            }
        }

        /// <summary>
        /// Tests containment translation for <see cref="NpgsqlRangeExtensions.Contains{T}(NpgsqlRange{T}, T)"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(IntegerTheoryData))]
        public void ContainsValue(int value)
        {
            using (RangeContext context0 = Fixture.CreateContext())
            using (RangeContext context1 = Fixture.CreateContext())
            {
                RangeTestEntity[] sqlActual =
                    context0.RangeTestEntities
                            .Where(x => x.Range.Contains(value))
                            .ToArray();

                RangeTestEntity[] clientActual =
                    context1.RangeTestEntities
                            .ToArray()
                            .Where(x => x.Range.Contains(value))
                            .ToArray();

                Assert.Contains("WHERE \"x\".\"Range\" @> @__value_0", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(sqlActual.Length, clientActual.Length);
            }
        }

        /// <summary>
        /// Tests negative containment translation for <see cref="NpgsqlRangeExtensions.Contains{T}(NpgsqlRange{T}, T)"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(IntegerTheoryData))]
        public void DoesNotContainValue(int value)
        {
            using (RangeContext context0 = Fixture.CreateContext())
            using (RangeContext context1 = Fixture.CreateContext())
            {
                RangeTestEntity[] sqlActual =
                    context0.RangeTestEntities
                            .Where(x => !x.Range.Contains(value))
                            .ToArray();

                RangeTestEntity[] clientActual =
                    context1.RangeTestEntities
                            .ToArray()
                            .Where(x => !x.Range.Contains(value))
                            .ToArray();

                Assert.Contains("WHERE NOT (\"x\".\"Range\" @> @__value_0 = TRUE)", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(sqlActual.Length, clientActual.Length);
            }
        }

        /// <summary>
        /// Tests containment translation for <see cref="NpgsqlRangeExtensions.ContainedBy{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void ContainedByRange(NpgsqlRange<int> range)
        {
            using (RangeContext context0 = Fixture.CreateContext())
            using (RangeContext context1 = Fixture.CreateContext())
            {
                RangeTestEntity[] sqlActual =
                    context0.RangeTestEntities
                            .Where(x => range.ContainedBy(x.Range))
                            .ToArray();

                RangeTestEntity[] clientActual =
                    context1.RangeTestEntities
                            .ToArray()
                            .Where(x => range.ContainedBy(x.Range))
                            .ToArray();

                Assert.Contains("WHERE @__range_0 <@ \"x\".\"Range\" = TRUE", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(sqlActual.Length, clientActual.Length);
            }
        }

        /// <summary>
        /// Tests negative containment translation for <see cref="NpgsqlRangeExtensions.ContainedBy{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void NotContainedByRange(NpgsqlRange<int> range)
        {
            using (RangeContext context0 = Fixture.CreateContext())
            using (RangeContext context1 = Fixture.CreateContext())
            {
                RangeTestEntity[] sqlActual =
                    context0.RangeTestEntities
                            .Where(x => !range.ContainedBy(x.Range))
                            .ToArray();

                RangeTestEntity[] clientActual =
                    context1.RangeTestEntities
                            .ToArray()
                            .Where(x => !range.ContainedBy(x.Range))
                            .ToArray();

                Assert.Contains("WHERE NOT (@__range_0 <@ \"x\".\"Range\" = TRUE)", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(sqlActual.Length, clientActual.Length);
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlRange{T}.Equals(NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void Equal_0(NpgsqlRange<int> range)
        {
            using (RangeContext context0 = Fixture.CreateContext())
            using (RangeContext context1 = Fixture.CreateContext())
            {
                RangeTestEntity[] sqlActual =
                    context0.RangeTestEntities
                            .Where(x => x.Range == range)
                            .ToArray();

                RangeTestEntity[] clientActual =
                    context1.RangeTestEntities
                            .ToArray()
                            .Where(x => x.Range == range)
                            .ToArray();

                Assert.Contains("WHERE \"x\".\"Range\" = @__range_0", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(sqlActual.Length, clientActual.Length);
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlRange{T}.Equals(NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void Equal_1(NpgsqlRange<int> range)
        {
            using (RangeContext context0 = Fixture.CreateContext())
            using (RangeContext context1 = Fixture.CreateContext())
            {
                RangeTestEntity[] sqlActual =
                    context0.RangeTestEntities
                            .Where(x => x.Range.Equals(range))
                            .ToArray();

                RangeTestEntity[] clientActual =
                    context1.RangeTestEntities
                            .ToArray()
                            .Where(x => x.Range.Equals(range))
                            .ToArray();

                Assert.Contains("WHERE \"x\".\"Range\" = @__range_0", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(sqlActual.Length, clientActual.Length);
            }
        }

        /// <summary>
        /// Tests negative translation for <see cref="NpgsqlRange{T}.Equals(NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void NotEqual_0(NpgsqlRange<int> range)
        {
            using (RangeContext context0 = Fixture.CreateContext())
            using (RangeContext context1 = Fixture.CreateContext())
            {
                RangeTestEntity[] sqlActual =
                    context0.RangeTestEntities
                            .Where(x => x.Range != range)
                            .ToArray();

                RangeTestEntity[] clientActual =
                    context1.RangeTestEntities
                            .ToArray()
                            .Where(x => x.Range != range)
                            .ToArray();

                Assert.Contains("WHERE \"x\".\"Range\" <> @__range_0", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(sqlActual.Length, clientActual.Length);
            }
        }

        /// <summary>
        /// Tests negative translation for <see cref="NpgsqlRange{T}.Equals(NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void NotEqual_1(NpgsqlRange<int> range)
        {
            using (RangeContext context0 = Fixture.CreateContext())
            using (RangeContext context1 = Fixture.CreateContext())
            {
                RangeTestEntity[] sqlActual =
                    context0.RangeTestEntities
                            .Where(x => !x.Range.Equals(range))
                            .ToArray();

                RangeTestEntity[] clientActual =
                    context1.RangeTestEntities
                            .ToArray()
                            .Where(x => !x.Range.Equals(range))
                            .ToArray();

                Assert.Contains("WHERE \"x\".\"Range\" <> @__range_0", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(sqlActual.Length, clientActual.Length);
            }
        }

        /// <summary>
        /// Tests containment translation for <see cref="NpgsqlRangeExtensions.Overlaps{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(RangeTheoryData))]
        public void Ovelaps(NpgsqlRange<int> range)
        {
            using (RangeContext context0 = Fixture.CreateContext())
            using (RangeContext context1 = Fixture.CreateContext())
            {
                RangeTestEntity[] sqlActual =
                    context0.RangeTestEntities
                            .Where(x => x.Range.Overlaps(range))
                            .ToArray();

                RangeTestEntity[] clientActual =
                    context1.RangeTestEntities
                            .ToArray()
                            .Where(x => x.Range.Overlaps(range))
                            .ToArray();

                Assert.Contains("WHERE \"x\".\"Range\" && @__range_0", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(sqlActual.Length, clientActual.Length);
            }
        }
    }
}
