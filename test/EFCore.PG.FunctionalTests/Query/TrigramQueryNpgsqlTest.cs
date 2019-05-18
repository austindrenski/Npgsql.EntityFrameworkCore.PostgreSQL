using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Extensions;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class TrigramQueryNpgsqlTest : IClassFixture<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        #region Setup

        /// <summary>
        /// Provides resources for unit tests.
        /// </summary>
        NorthwindQueryNpgsqlFixture<NoopModelCustomizer> Fixture { get; }

        /// <summary>
        /// Initializes resources for unit tests.
        /// </summary>
        /// <param name="fixture"> The fixture of resources for testing. </param>
        public TrigramQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        #endregion

        #region Tests

        [Fact]
        public void SimpleTest()
        {
            using var ctx = Fixture.CreateContext();

            var _ =
                ctx.Employees
                    .Where(x => EF.Functions.FuzzyMatches(x.City, "foobar"))
                    .ToArray();

            AssertContainsSql("(x.\"City\" %> 'foobar') = TRUE");
        }

        [Fact]
        public void ComplicatedTest()
        {
            using var ctx = Fixture.CreateContext();

            // ReSharper disable once ConvertToConstant.Local
            var search = "foobar";
            // ReSharper disable once ConvertToConstant.Local
            var skip = 1;
            // ReSharper disable once ConvertToConstant.Local
            var take = 1;

            var _ =
                ctx.Employees
                    .Where(x =>
                        ctx.Employees
                            .Where(y => EF.Functions.FuzzyMatches(y.City, search))
                            .OrderBy(y => EF.Functions.WordSimilarity(y.City, search))
                            .Skip(skip)
                            .Take(take)
                            .Select(y => y.City)
                            .Contains(x.City))
                    .OrderBy(x => EF.Functions.WordSimilarity(x.City, search))
                    .ToArray();

            AssertContainsSql(@"@__search_1='foobar'
@__take_3='1'
@__skip_2='1'

SELECT x.""EmployeeID"", x.""City"", x.""Country"", x.""FirstName"", x.""ReportsTo"", x.""Title""
FROM ""Employees"" AS x
WHERE x.""City"" IN (
    SELECT y.""City""
    FROM ""Employees"" AS y
    WHERE (y.""City"" %> @__search_1) = TRUE
    ORDER BY (y.""City"" <->> @__search_1)
    LIMIT @__take_3 OFFSET @__skip_2
)
ORDER BY (x.""City"" <->> @__search_1)");
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Asserts that the SQL fragment appears in the logs.
        /// </summary>
        /// <param name="sql">The SQL statement or fragment to search for in the logs.</param>
        void AssertContainsSql(string sql) => Assert.Contains(sql, Fixture.TestSqlLoggerFactory.Sql);

        #endregion
    }
}
