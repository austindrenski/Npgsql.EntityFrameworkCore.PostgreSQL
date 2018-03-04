using System.Collections.Generic;
using System.Linq;
using NpgsqlTypes;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    /// Provides unit tests for range operator translation.
    /// </summary>
    public class RangeContainsNpgsqlTest : IClassFixture<RangeQueryNpgsqlFixture>
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
        public RangeContainsNpgsqlTest(RangeQueryNpgsqlFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        /// <summary>
        /// Tests containment translation for <see cref="NpgsqlRangeFunctionExtensions.Contains{T}(NpgsqlRange{T}, T)"/>.
        /// </summary>
        [Fact]
        public void RangeContainsValue()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                List<RangeTestEntity> actual =
                    context.RangeTestEntities
                           .Where(x => x.Range.Contains(0))
                           .ToList();

                Assert.Equal(3, actual.Count);
                Assert.Contains("WHERE \"x\".\"Range\" @> 0", Fixture.TestSqlLoggerFactory.Sql);
            }
        }

        /// <summary>
        /// Tests negative containment translation for <see cref="NpgsqlRangeFunctionExtensions.Contains{T}(NpgsqlRange{T}, T)"/>.
        /// </summary>
        [Fact]
        public void RangeDoesNotContainsValue()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                List<RangeTestEntity> actual =
                    context.RangeTestEntities
                           .Where(x => !x.Range.Contains(0))
                           .ToList();

                Assert.Equal(1, actual.Count);
                Assert.Contains("WHERE NOT (\"x\".\"Range\" @> 0 = TRUE)", Fixture.TestSqlLoggerFactory.Sql);
            }
        }

        /// <summary>
        /// Tests containment translation for <see cref="NpgsqlRangeFunctionExtensions.Contains{T}(NpgsqlRange{T}, T)"/>.
        /// </summary>
        [Fact]
        public void RangeContainsRange()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                List<RangeTestEntity> actual =
                    context.RangeTestEntities
                           .Where(x => x.Range.Contains(new NpgsqlRange<int>(0, 5)))
                           .ToList();

                Assert.Equal(3, actual.Count);
                Assert.Contains("WHERE \"x\".\"Range\" @> '[0,5]'::int4range = TRUE", Fixture.TestSqlLoggerFactory.Sql);
            }
        }

        /// <summary>
        /// Tests containment translation for <see cref="NpgsqlRangeFunctionExtensions.Contains{T}(NpgsqlRange{T}, T)"/>.
        /// </summary>
        [Fact]
        public void RangeDoesNotContainsRange()
        {
            using (RangeContext context = Fixture.CreateContext())
            {
                List<RangeTestEntity> actual =
                    context.RangeTestEntities
                           .Where(x => !x.Range.Contains(new NpgsqlRange<int>(0, 5)))
                           .ToList();

                Assert.Equal(1, actual.Count);
                Assert.Contains("WHERE NOT (\"x\".\"Range\" @> '[0,5]'::int4range = TRUE)", Fixture.TestSqlLoggerFactory.Sql);
            }
        }
    }
}
