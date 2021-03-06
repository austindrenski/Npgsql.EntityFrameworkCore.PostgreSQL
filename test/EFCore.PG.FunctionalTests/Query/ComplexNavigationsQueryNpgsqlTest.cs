﻿using Microsoft.EntityFrameworkCore.Query;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class ComplexNavigationsQueryNpgsqlTest
        : ComplexNavigationsQueryTestBase<ComplexNavigationsQueryNpgsqlFixture>
    {
        public ComplexNavigationsQueryNpgsqlTest(ComplexNavigationsQueryNpgsqlFixture fixture)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        // Should be fixed but could not verify as temporarily disabled upstream
//        [ConditionalTheory(Skip = "https://github.com/aspnet/EntityFrameworkCore/pull/12970")]
//        [MemberData(nameof(IsAsyncData))]
//        public override Task Null_check_in_anonymous_type_projection_should_not_be_removed(bool isAsync) => null;
    }
}
