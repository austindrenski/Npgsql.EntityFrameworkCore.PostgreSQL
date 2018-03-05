using System.Linq;
using NpgsqlTypes;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    /// Provides unit tests for range not equal (&lt;>) translation.
    /// </summary>
    public class RangeNotEqualNpgsqlQueryTest : IClassFixture<RangeQueryNpgsqlFixture>
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
        public RangeNotEqualNpgsqlQueryTest(RangeQueryNpgsqlFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
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
    }
}
