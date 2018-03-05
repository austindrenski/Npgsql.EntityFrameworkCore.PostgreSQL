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
        private RangeQueryNpgsqlFixture Fixture { get; }

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
        /// Tests containment translation for <see cref="NpgsqlRangeExtensions.Contains{T}(NpgsqlRange{T}, T)"/>.
        /// </summary>
        [Fact]
        public void RangeContainsRange()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .Where(x => x.Range.Contains(new NpgsqlRange<int>(0, 5)))
                           .ToArray();

                Assert.Equal(3, actual.Length);
                Assert.Contains("WHERE \"x\".\"Range\" @> '[0,5]'::int4range = TRUE", Fixture.TestSqlLoggerFactory.Sql);
            }
        }

        /// <summary>
        /// Tests containment translation for <see cref="NpgsqlRangeExtensions.Contains{T}(NpgsqlRange{T}, T)"/>.
        /// </summary>
        [Fact]
        public void RangeContainsRangeNonSql()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .ToArray()
                           .Where(x => x.Range.Contains(new NpgsqlRange<int>(0, 5)))
                           .ToArray();

                Assert.Equal(3, actual.Length);
            }
        }

        /// <summary>
        /// Tests containment translation for <see cref="NpgsqlRangeExtensions.Contains{T}(NpgsqlRange{T}, T)"/>.
        /// </summary>
        [Fact]
        public void RangeContainsValue()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .Where(x => x.Range.Contains(0))
                           .ToArray();

                Assert.Equal(3, actual.Length);
                Assert.Contains("WHERE \"x\".\"Range\" @> 0", Fixture.TestSqlLoggerFactory.Sql);
            }
        }

        /// <summary>
        /// Tests containment translation for <see cref="NpgsqlRangeExtensions.Contains{T}(NpgsqlRange{T}, T)"/>.
        /// </summary>
        [Fact]
        public void RangeContainsValueNonSql()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .ToArray()
                           .Where(x => x.Range.Contains(0))
                           .ToArray();

                Assert.Equal(3, actual.Length);
            }
        }

        /// <summary>
        /// Tests containment translation for <see cref="NpgsqlRangeExtensions.Contains{T}(NpgsqlRange{T}, T)"/>.
        /// </summary>
        [Fact]
        public void RangeDoesNotContainRange()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .Where(x => !x.Range.Contains(new NpgsqlRange<int>(0, 5)))
                           .ToArray();

                Assert.Equal(1, actual.Length);
                Assert.Contains("WHERE NOT (\"x\".\"Range\" @> '[0,5]'::int4range = TRUE)", Fixture.TestSqlLoggerFactory.Sql);
            }
        }

        /// <summary>
        /// Tests containment translation for <see cref="NpgsqlRangeExtensions.Contains{T}(NpgsqlRange{T}, T)"/>.
        /// </summary>
        [Fact]
        public void RangeDoesNotContainRangeNonSql()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .ToArray()
                           .Where(x => !x.Range.Contains(new NpgsqlRange<int>(0, 5)))
                           .ToArray();

                Assert.Equal(1, actual.Length);
            }
        }

        /// <summary>
        /// Tests negative containment translation for <see cref="NpgsqlRangeExtensions.Contains{T}(NpgsqlRange{T}, T)"/>.
        /// </summary>
        [Fact]
        public void RangeDoesNotContainValue()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .Where(x => !x.Range.Contains(0))
                           .ToArray();

                Assert.Equal(1, actual.Length);
                Assert.Contains("WHERE NOT (\"x\".\"Range\" @> 0 = TRUE)", Fixture.TestSqlLoggerFactory.Sql);
            }
        }

        /// <summary>
        /// Tests negative containment translation for <see cref="NpgsqlRangeExtensions.Contains{T}(NpgsqlRange{T}, T)"/>.
        /// </summary>
        [Fact]
        public void RangeDoesNotContainValueNonSql()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .ToArray()
                           .Where(x => !x.Range.Contains(0))
                           .ToArray();

                Assert.Equal(1, actual.Length);
            }
        }

        /// <summary>
        /// Tests containment translation for <see cref="NpgsqlRangeExtensions.ContainedBy{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Fact]
        public void RangeContainedByRange()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .Where(x => new NpgsqlRange<int>(0, 5).ContainedBy(x.Range))
                           .ToArray();

                Assert.Equal(3, actual.Length);
                Assert.Contains("WHERE '[0,5]'::int4range <@ \"x\".\"Range\" = TRUE", Fixture.TestSqlLoggerFactory.Sql);
            }
        }

        /// <summary>
        /// Tests containment translation for <see cref="NpgsqlRangeExtensions.ContainedBy{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Fact]
        public void RangeContainedByRangeNonSql()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .ToArray()
                           .Where(x => new NpgsqlRange<int>(0, 5).ContainedBy(x.Range))
                           .ToArray();

                Assert.Equal(3, actual.Length);
            }
        }

        /// <summary>
        /// Tests negative containment translation for <see cref="NpgsqlRangeExtensions.ContainedBy{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Fact]
        public void RangeNotContainedByRange()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .Where(x => !new NpgsqlRange<int>(0, 5).ContainedBy(x.Range))
                           .ToArray();

                Assert.Equal(1, actual.Length);
                Assert.Contains("WHERE NOT ('[0,5]'::int4range <@ \"x\".\"Range\" = TRUE)", Fixture.TestSqlLoggerFactory.Sql);
            }
        }

        /// <summary>
        /// Tests negative containment translation for <see cref="NpgsqlRangeExtensions.ContainedBy{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Fact]
        public void RangeNotContainedByRangeNonSql()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .ToArray()
                           .Where(x => !new NpgsqlRange<int>(0, 5).ContainedBy(x.Range))
                           .ToArray();

                Assert.Equal(1, actual.Length);
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlRange{T}.Equals(NpgsqlRange{T})"/>.
        /// </summary>
        [Fact]
        public void RangeEqual0()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .Where(x => x.Range == new NpgsqlRange<int>(-10, 10))
                           .ToArray();

                Assert.Contains("WHERE \"x\".\"Range\" = '[-10,10]'::int4range", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(1, actual.Length);
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlRange{T}.Equals(NpgsqlRange{T})"/>.
        /// </summary>
        [Fact]
        public void RangeEqual1()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .Where(x => x.Range.Equals(new NpgsqlRange<int>(-10, 10)))
                           .ToArray();

                Assert.Contains("WHERE \"x\".\"Range\" = '[-10,10]'::int4range", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(1, actual.Length);
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlRange{T}.Equals(NpgsqlRange{T})"/>.
        /// </summary>
        [Fact]
        public void RangeEqualNonSql0()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .ToArray()
                           .Where(x => x.Range == new NpgsqlRange<int>(-10, 10))
                           .ToArray();

                Assert.Equal(1, actual.Length);
            }
        }

        /// <summary>
        /// Tests translation for <see cref="NpgsqlRange{T}.Equals(NpgsqlRange{T})"/>.
        /// </summary>
        [Fact]
        public void RangeEqualNonSql1()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .ToArray()
                           .Where(x => x.Range.Equals(new NpgsqlRange<int>(-10, 10)))
                           .ToArray();

                Assert.Equal(1, actual.Length);
            }
        }

        /// <summary>
        /// Tests negative translation for <see cref="NpgsqlRange{T}.Equals(NpgsqlRange{T})"/>.
        /// </summary>
        [Fact]
        public void RangeNotEqual0()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .Where(x => x.Range != new NpgsqlRange<int>(-10, 10))
                           .ToArray();

                Assert.Contains("WHERE \"x\".\"Range\" <> '[-10,10]'::int4range", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(3, actual.Length);
            }
        }

        /// <summary>
        /// Tests negative translation for <see cref="NpgsqlRange{T}.Equals(NpgsqlRange{T})"/>.
        /// </summary>
        [Fact]
        public void RangeNotEqual1()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .Where(x => !x.Range.Equals(new NpgsqlRange<int>(-10, 10)))
                           .ToArray();

                Assert.Contains("WHERE \"x\".\"Range\" <> '[-10,10]'::int4range", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(3, actual.Length);
            }
        }

        /// <summary>
        /// Tests negative translation for <see cref="NpgsqlRange{T}.Equals(NpgsqlRange{T})"/>.
        /// </summary>
        [Fact]
        public void RangeNotEqualNonSql0()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .ToArray()
                           .Where(x => x.Range != new NpgsqlRange<int>(-10, 10))
                           .ToArray();

                Assert.Equal(3, actual.Length);
            }
        }

        /// <summary>
        /// Tests negative translation for <see cref="NpgsqlRange{T}.Equals(NpgsqlRange{T})"/>.
        /// </summary>
        [Fact]
        public void RangeNotEqualNonSql1()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .ToArray()
                           .Where(x => !x.Range.Equals(new NpgsqlRange<int>(-10, 10)))
                           .ToArray();

                Assert.Equal(3, actual.Length);
            }
        }

        /// <summary>
        /// Tests containment translation for <see cref="NpgsqlRangeExtensions.Overlaps{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Fact]
        public void RangeOvelaps()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .Where(x => x.Range.Overlaps(new NpgsqlRange<int>(0, 1)))
                           .ToArray();

                Assert.Equal(4, actual.Length);
                Assert.Contains("WHERE \"x\".\"Range\" && '[0,1]'::int4range", Fixture.TestSqlLoggerFactory.Sql);
            }
        }

        /// <summary>
        /// Tests containment translation for <see cref="NpgsqlRangeExtensions.Overlaps{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Fact]
        public void RangeOvelapsNonSql()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .ToArray()
                           .Where(x => x.Range.Overlaps(new NpgsqlRange<int>(0, 1)))
                           .ToArray();

                Assert.Equal(4, actual.Length);
            }
        }
    }
}
