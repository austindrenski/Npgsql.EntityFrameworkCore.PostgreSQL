using System.Linq;
using NpgsqlTypes;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    /// Provides unit tests for the range contained by (e.g. &lt;@) operator translation.
    /// </summary>
    public class RangeContainedByNpgsqlTest : IClassFixture<RangeQueryNpgsqlFixture>
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
        public RangeContainedByNpgsqlTest(RangeQueryNpgsqlFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
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
        /// Tests containment translation for <see cref="NpgsqlRangeExtensions.ContainedBy{T}(T, NpgsqlRange{T})"/>.
        /// </summary>
        [Fact]
        public void ValueContainedByRange()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .Where(x => 0.ContainedBy(x.Range))
                           .ToArray();

                Assert.Equal(3, actual.Length);
                Assert.Contains("WHERE 0 <@ \"x\".\"Range\"", Fixture.TestSqlLoggerFactory.Sql);
            }
        }

        /// <summary>
        /// Tests containment translation for <see cref="NpgsqlRangeExtensions.ContainedBy{T}(T, NpgsqlRange{T})"/>.
        /// </summary>
        [Fact]
        public void ValueContainedByRangeNonSql()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .ToArray()
                           .Where(x => 0.ContainedBy(x.Range))
                           .ToArray();

                Assert.Equal(3, actual.Length);
            }
        }

        /// <summary>
        /// Tests negative containment translation for <see cref="NpgsqlRangeExtensions.ContainedBy{T}(T, NpgsqlRange{T})"/>.
        /// </summary>
        [Fact]
        public void ValueNotContainedByRange()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .Where(x => !0.ContainedBy(x.Range))
                           .ToArray();

                Assert.Equal(1, actual.Length);
                Assert.Contains("WHERE NOT (0 <@ \"x\".\"Range\" = TRUE)", Fixture.TestSqlLoggerFactory.Sql);
            }
        }

        /// <summary>
        /// Tests negative containment translation for <see cref="NpgsqlRangeExtensions.ContainedBy{T}(T, NpgsqlRange{T})"/>.
        /// </summary>
        [Fact]
        public void ValueNotContainedByRangeNonSql()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                RangeTestEntity[] actual =
                    context.RangeTestEntities
                           .ToArray()
                           .Where(x => !0.ContainedBy(x.Range))
                           .ToArray();

                Assert.Equal(1, actual.Length);
            }
        }
    }
}
