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

                Assert.Contains("WHERE \"x\".\"Range\" @> '[0,5]'::int4range = TRUE", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(6, actual.Length);
            }
        }

        /// <summary>
        /// Tests containment translation for <see cref="NpgsqlRangeExtensions.Contains{T}(NpgsqlRange{T}, T)"/>.
        /// </summary>
        [Fact]
        public void RangeContainsRangeNonSql()
        {
            // (0, 1)
            NpgsqlRange<int> a = new NpgsqlRange<int>(0, false, 1, false);
            // [0, 1)
            NpgsqlRange<int> b = new NpgsqlRange<int>(0, true, 1, false);
            // (0, 1]
            NpgsqlRange<int> c = new NpgsqlRange<int>(0, false, 1, true);
            // [0, 1]
            NpgsqlRange<int> d = new NpgsqlRange<int>(0, true, 1, true);

            Assert.True(a.Contains(a));
            Assert.True(b.Contains(a));
            Assert.True(c.Contains(a));
            Assert.True(d.Contains(a));

            Assert.False(a.Contains(b));
            Assert.True(b.Contains(b));
            Assert.False(c.Contains(b));
            Assert.True(d.Contains(b));

            Assert.False(a.Contains(c));
            Assert.False(b.Contains(c));
            Assert.True(c.Contains(c));
            Assert.True(d.Contains(c));

            Assert.False(a.Contains(d));
            Assert.False(b.Contains(d));
            Assert.False(c.Contains(d));
            Assert.True(d.Contains(d));

            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .ToArray()
                           .Where(x => x.Range.Contains(new NpgsqlRange<int>(0, 5)))
                           .ToArray();

                Assert.Equal(6, actual.Length);
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

                Assert.Contains("WHERE \"x\".\"Range\" @> 0", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(6, actual.Length);
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

                Assert.Equal(6, actual.Length);
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

                Assert.Contains("WHERE NOT (\"x\".\"Range\" @> '[0,5]'::int4range = TRUE)", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(1, actual.Length);
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

                Assert.Contains("WHERE NOT (\"x\".\"Range\" @> 0 = TRUE)", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(1, actual.Length);
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

                Assert.Contains("WHERE '[0,5]'::int4range <@ \"x\".\"Range\" = TRUE", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(6, actual.Length);
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

                Assert.Equal(6, actual.Length);
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

                Assert.Contains("WHERE NOT ('[0,5]'::int4range <@ \"x\".\"Range\" = TRUE)", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(1, actual.Length);
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
                           .Where(x => x.Range == new NpgsqlRange<int>(0, 10))
                           .ToArray();

                Assert.Contains("WHERE \"x\".\"Range\" = '[0,10]'::int4range", Fixture.TestSqlLoggerFactory.Sql);
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
                           .Where(x => x.Range.Equals(new NpgsqlRange<int>(0, 10)))
                           .ToArray();

                Assert.Contains("WHERE \"x\".\"Range\" = '[0,10]'::int4range", Fixture.TestSqlLoggerFactory.Sql);
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
                           .Where(x => x.Range == new NpgsqlRange<int>(0, 10))
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
                           .Where(x => x.Range.Equals(new NpgsqlRange<int>(0, 10)))
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
                           .Where(x => x.Range != new NpgsqlRange<int>(0, 10))
                           .ToArray();

                Assert.Contains("WHERE \"x\".\"Range\" <> '[0,10]'::int4range", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(7, actual.Length);
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
                           .Where(x => !x.Range.Equals(new NpgsqlRange<int>(0, 10)))
                           .ToArray();

                Assert.Contains("WHERE \"x\".\"Range\" <> '[0,10]'::int4range", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(7, actual.Length);
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
                           .Where(x => x.Range != new NpgsqlRange<int>(0, 10))
                           .ToArray();

                Assert.Equal(7, actual.Length);
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
                           .Where(x => !x.Range.Equals(new NpgsqlRange<int>(0, 10)))
                           .ToArray();

                Assert.Equal(7, actual.Length);
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

                Assert.Contains("WHERE \"x\".\"Range\" && '[0,1]'::int4range", Fixture.TestSqlLoggerFactory.Sql);
                Assert.Equal(7, actual.Length);
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

                Assert.Equal(7, actual.Length);
            }
        }
    }
}
