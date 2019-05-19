using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class SkipTakeQueryNpgsqlTest : IClassFixture<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
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
        public SkipTakeQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture)
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

            // ReSharper disable once ConvertToConstant.Local
            var take = 1;

            var _ =
                ctx.Employees
                    .Take(take)
                    .Select(x => x.City)
                    .ToArray();

            AssertContainsSql(@"@__take_1='1'

SELECT x.""City""
FROM ""Employees"" AS x
LIMIT @__take_1");
        }

        [Fact]
        public void ComplicatedTest()
        {
            using var ctx = Fixture.CreateContext();

            // ReSharper disable once ConvertToConstant.Local
            var take = 1;

            var _ =
                ctx.Employees
                    .Where(x =>
                        ctx.Employees
                            .Take(take)
                            .Select(y => y.City)
                            .Contains(x.City))
                    .ToArray();

            AssertContainsSql(@"@__take_1='1'

SELECT x.""EmployeeID"", x.""City"", x.""Country"", x.""FirstName"", x.""ReportsTo"", x.""Title""
FROM ""Employees"" AS x
WHERE x.""City"" IN (
    SELECT y.""City""
    FROM ""Employees"" AS y
    LIMIT @__take_1
)");
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
