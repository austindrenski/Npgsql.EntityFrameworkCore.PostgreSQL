using System.Linq;
using NpgsqlTypes;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    /// Provides unit tests for range overlaps (&&) translation.
    /// </summary>
    public class RangeOverlapsNpgsqlQueryTest : IClassFixture<RangeQueryNpgsqlFixture>
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
        public RangeOverlapsNpgsqlQueryTest(RangeQueryNpgsqlFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
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
