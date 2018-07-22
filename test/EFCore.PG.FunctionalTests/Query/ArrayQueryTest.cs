﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class ArrayQueryTest : IClassFixture<ArrayQueryTest.ArrayFixture>
    {
        #region ArrayTests

        #region Roundtrip

        [Fact]
        public void Roundtrip()
        {
            using (var ctx = CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.Id == 1);
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);
                Assert.Equal(new List<int> { 3, 4 }, x.SomeList);
            }
        }

        #endregion

        #region Indexers

        [Fact]
        public void Index_with_constant()
        {
            using (var ctx = CreateContext())
            {
                var actual = ctx.SomeEntities.Where(e => e.SomeArray[0] == 3).ToList();
                Assert.Equal(1, actual.Count);
                AssertContainsInSql(@"WHERE (e.""SomeArray""[1]) = 3");
            }
        }

        [Fact]
        public void Index_with_non_constant()
        {
            using (var ctx = CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var x = 0;
                var actual = ctx.SomeEntities.Where(e => e.SomeArray[x] == 3).ToList();
                Assert.Equal(1, actual.Count);
                AssertContainsInSql(@"WHERE (e.""SomeArray""[@__x_0 + 1]) = 3");
            }
        }

        [Fact]
        public void Index_bytea_with_constant()
        {
            using (var ctx = CreateContext())
            {
                var actual = ctx.SomeEntities.Where(e => e.SomeBytea[0] == 3).ToList();
                Assert.Equal(1, actual.Count);
                AssertContainsInSql(@"WHERE (get_byte(e.""SomeBytea"", 0)) = 3");
            }
        }

        [Fact]
        public void Index_multidimensional()
        {
            using (var ctx = CreateContext())
            {
                // Operations on multidimensional arrays aren't mapped to SQL yet
                var actual = ctx.SomeEntities.Where(e => e.SomeMatrix[0, 0] == 5).ToList();
                Assert.Equal(1, actual.Count);
            }
        }

        #endregion

        #region Equality

        [Fact]
        public void SequenceEqual_with_parameter()
        {
            using (var ctx = CreateContext())
            {
                var arr = new[] { 3, 4 };
                var x = ctx.SomeEntities.Single(e => e.SomeArray.SequenceEqual(arr));
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);
                AssertContainsInSql(@"WHERE e.""SomeArray"" = @");
            }
        }

        [Fact]
        public void SequenceEqual_with_array_literal()
        {
            using (var ctx = CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.SomeArray.SequenceEqual(new[] { 3, 4 }));
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);
                AssertContainsInSql(@"WHERE e.""SomeArray"" = ARRAY[3,4]::integer");
            }
        }

        #endregion

        #region Containment

        [Fact]
        public void Contains_with_literal()
        {
            using (var ctx = CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.SomeArray.Contains(3));
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);
                AssertContainsInSql(@"WHERE 3 = ANY (e.""SomeArray"")");
            }
        }

        [Fact]
        public void Contains_with_parameter()
        {
            using (var ctx = CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var p = 3;
                var x = ctx.SomeEntities.Single(e => e.SomeArray.Contains(p));
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);
                AssertContainsInSql(@"WHERE @__p_0 = ANY (e.""SomeArray"")");
            }
        }

        [Fact]
        public void Contains_with_column()
        {
            using (var ctx = CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.SomeArray.Contains(e.Id + 2));
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);
                AssertContainsInSql(@"WHERE e.""Id"" + 2 = ANY (e.""SomeArray"")");
            }
        }

        #endregion

        #region Length

        [Fact]
        public void Length()
        {
            using (var ctx = CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.SomeArray.Length == 2);
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);
                AssertContainsInSql(@"WHERE array_length(e.""SomeArray"", 1) = 2");
            }
        }

        [Fact(Skip = "https://github.com/aspnet/EntityFramework/issues/9242")]
        public void Length_on_EF_Property()
        {
            using (var ctx = CreateContext())
            {
                // TODO: This fails
                var x = ctx.SomeEntities.Single(e => EF.Property<int[]>(e, nameof(SomeArrayEntity.SomeArray)).Length == 2);
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);
                AssertContainsInSql(@"WHERE array_length(e.""SomeArray"", 1) = 2");
            }
        }

        [Fact]
        public void Length_on_literal_not_translated()
        {
            using (var ctx = CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => new[] { 1, 2, 3 }.Length == e.Id).ToList();
                AssertDoesNotContainInSql("array_length");
            }
        }

        #endregion

        #region LikeAnyAll

        [Fact]
        public void Array_like_any_when_match_expression_is_column()
        {
            using (var ctx = CreateContext())
            {
                var patterns = new[] { "a", "b", "c" };

                var anon =
                    ctx.SomeEntities
                       .Select(x => new { Text = x.SomeText });

                var _ = anon.Where(x => patterns.Any(p => EF.Functions.Like(x.Text, p))).ToList();

                AssertContainsInSql("x.\"SomeText\" LIKE ANY (@__patterns_0) = TRUE");
            }
        }

        [Fact]
        public void Array_like_all_when_match_expression_is_column()
        {
            using (var ctx = CreateContext())
            {
                var patterns = new[] { "a", "b", "c" };

                var anon =
                    ctx.SomeEntities
                       .Select(x => new { Text = x.SomeText });

                var _ = anon.Where(x => patterns.All(p => EF.Functions.Like(x.Text, p))).ToList();

                AssertContainsInSql("x.\"SomeText\" LIKE ALL (@__patterns_0) = TRUE");
            }
        }

        [Fact]
        public void Array_like_any_not_translated_when_match_expression_is_qsre()
        {
            using (var ctx = CreateContext())
            {
                var matches = new[] { "a", "b", "c" };

                var anon =
                    ctx.SomeEntities
                       .Select(x => new { Text = x.SomeText });

                var _ = anon.Where(x => matches.Any(m => EF.Functions.Like(m, x.Text))).ToList();

                AssertDoesNotContainInSql("LIKE");
                AssertDoesNotContainInSql("ANY");
                AssertDoesNotContainInSql("@__matches_0");
            }
        }

        #endregion

        #endregion

        #region Support

        ArrayFixture Fixture { get; }

        public ArrayQueryTest(ArrayFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        ArrayContext CreateContext() => Fixture.CreateContext();

        void AssertContainsInSql(string expected)
            => Assert.Contains(expected, Fixture.TestSqlLoggerFactory.Sql);

        void AssertDoesNotContainInSql(string expected)
            => Assert.DoesNotContain(expected, Fixture.TestSqlLoggerFactory.Sql);

        public class ArrayContext : DbContext
        {
            public DbSet<SomeArrayEntity> SomeEntities { get; set; }
            public ArrayContext(DbContextOptions options) : base(options) {}
            protected override void OnModelCreating(ModelBuilder builder) {}
        }

        public class SomeArrayEntity
        {
            public int Id { get; set; }
            public int[] SomeArray { get; set; }
            public int[,] SomeMatrix { get; set; }
            public List<int> SomeList { get; set; }
            public byte[] SomeBytea { get; set; }

            // ReSharper disable once UnusedMember.Global
            public string SomeText { get; set; }
        }

        public class ArrayFixture : IDisposable
        {
            readonly DbContextOptions _options;
            public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

            public ArrayFixture()
            {
                _testStore = NpgsqlTestStore.CreateScratch();
                _options = new DbContextOptionsBuilder()
                           .UseNpgsql(_testStore.ConnectionString, b => b.ApplyConfiguration())
                           .UseInternalServiceProvider(
                               new ServiceCollection()
                                   .AddEntityFrameworkNpgsql()
                                   .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                                   .BuildServiceProvider())
                           .Options;

                using (var ctx = CreateContext())
                {
                    ctx.Database.EnsureCreated();
                    ctx.SomeEntities.Add(new SomeArrayEntity
                    {
                        Id = 1,
                        SomeArray = new[] { 3, 4 },
                        SomeBytea = new byte[] { 3, 4 },
                        SomeMatrix = new[,] { { 5, 6 }, { 7, 8 } },
                        SomeList = new List<int> { 3, 4 }
                    });
                    ctx.SomeEntities.Add(new SomeArrayEntity
                    {
                        Id = 2,
                        SomeArray = new[] { 5, 6, 7 },
                        SomeBytea = new byte[] { 5, 6, 7 },
                        SomeMatrix = new[,] { { 10, 11 }, { 12, 13 } },
                        SomeList = new List<int> { 3, 4 }
                    });
                    ctx.SaveChanges();
                }
            }

            readonly NpgsqlTestStore _testStore;
            public ArrayContext CreateContext() => new ArrayContext(_options);
            public void Dispose() => _testStore.Dispose();
        }

        #endregion
    }
}
