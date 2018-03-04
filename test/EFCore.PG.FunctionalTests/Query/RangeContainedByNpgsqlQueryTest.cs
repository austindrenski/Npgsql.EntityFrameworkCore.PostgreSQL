using System.Collections.Generic;
using System.Linq;
using NpgsqlTypes;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    /// Provides unit tests for the range contains (e.g. @>) operator translation.
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
        /// Tests containment translation for <see cref="NpgsqlRangeFunctionExtensions.ContainedBy{T}(T, NpgsqlRange{T})"/>.
        /// </summary>
        [Fact]
        public void ValueContainedByRange()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                List<RangeTestEntity> actual =
                    context.RangeTestEntities
                           .Where(x => 0.ContainedBy(x.Range))
                           .ToList();

                Assert.Equal(3, actual.Count);
                Assert.Contains("WHERE \"x\".\"Range\" @> 0", Fixture.TestSqlLoggerFactory.Sql);
            }
        }

        /// <summary>
        /// Tests containment translation for <see cref="NpgsqlRangeFunctionExtensions.ContainedBy{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Fact]
        public void RangeContainedByRange()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                List<RangeTestEntity> actual =
                    context.RangeTestEntities
                           .Where(x => new NpgsqlRange<int>(0, 5).ContainedBy(x.Range))
                           .ToList();

                Assert.Equal(3, actual.Count);
                Assert.Contains("WHERE \"x\".\"Range\" @> '[0,5]'::int4range = TRUE", Fixture.TestSqlLoggerFactory.Sql);
            }
        }

        /// <summary>
        /// Tests negative containment translation for <see cref="NpgsqlRangeFunctionExtensions.ContainedBy{T}(T, NpgsqlRange{T})"/>.
        /// </summary>
        [Fact]
        public void ValueNotContainedByRange()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                List<RangeTestEntity> actual =
                    context.RangeTestEntities
                           .Where(x => !0.ContainedBy(x.Range))
                           .ToList();

                Assert.Equal(1, actual.Count);
                Assert.Contains("WHERE NOT (\"x\".\"Range\" @> 0 = TRUE)", Fixture.TestSqlLoggerFactory.Sql);
            }
        }

        /// <summary>
        /// Tests negative containment translation for <see cref="NpgsqlRangeFunctionExtensions.ContainedBy{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        [Fact]
        public void RangeNotContainedByRange()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                List<RangeTestEntity> actual =
                    context.RangeTestEntities
                           .Where(x => !new NpgsqlRange<int>(0, 5).ContainedBy(x.Range))
                           .ToList();

                Assert.Equal(1, actual.Count);
                Assert.Contains("WHERE NOT (\"x\".\"Range\" @> '[0,5]'::int4range = TRUE)", Fixture.TestSqlLoggerFactory.Sql);
            }
        }
    }
}
