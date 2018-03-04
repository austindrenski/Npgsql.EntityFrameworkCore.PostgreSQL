using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NpgsqlTypes;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    /// Provides unit tests for range operator translation.
    /// </summary>
    public class RangeOperatorQueryTest : IClassFixture<RangeFixture>
    {
        /// <summary>
        /// Provides resources for unit tests.
        /// </summary>
        private RangeFixture Fixture { get; }

        /// <summary>
        /// Initializes resources for unit tests.
        /// </summary>
        /// <param name="fixture">
        /// The fixture of resources for testing.
        /// </param>
        public RangeOperatorQueryTest(RangeFixture fixture)
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

                Assert.Equal(3, actual.Count);
                Assert.Contains("WHERE NOT (\"x\".\"Range\" @> 0 = TRUE)", Fixture.TestSqlLoggerFactory.Sql);
            }
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
    }

    /// <summary>
    /// Represents an entity suitable for testing range operators.
    /// </summary>
    public class RangeTestEntity
    {
        /// <summary>
        /// The primary key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The range of integers.
        /// </summary>
        public NpgsqlRange<int> Range { get; set; }
    }

    /// <summary>
    /// Represents a database suitable for testing range operators.
    /// </summary>
    public class RangeContext : DbContext
    {
        /// <summary>
        /// Represents a set of entities with <see cref="NpgsqlRange{T}"/> properties.
        /// </summary>
        public DbSet<RangeTestEntity> RangeTestEntities { get; set; }

        /// <summary>
        /// Initializes a <see cref="RangeContext"/>.
        /// </summary>
        /// <param name="options">
        /// The options to be used for configuration.
        /// </param>
        public RangeContext(DbContextOptions options) : base(options) { }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder builder) { }
    }

    /// <summary>
    /// Represents a fixture suitable for testing range operators.
    /// </summary>
    public class RangeFixture : IDisposable
    {
        /// <summary>
        /// The <see cref="NpgsqlTestStore"/> used for testing.
        /// </summary>
        private readonly NpgsqlTestStore _testStore;

        /// <summary>
        /// The <see cref="DbContextOptions"/> used for testing.
        /// </summary>
        private readonly DbContextOptions _options;

        /// <summary>
        /// The logger factory used for testing.
        /// </summary>
        public TestSqlLoggerFactory TestSqlLoggerFactory { get; }

        /// <summary>
        /// Initializes a <see cref="RangeFixture"/>.
        /// </summary>
        public RangeFixture()
        {
            TestSqlLoggerFactory = new TestSqlLoggerFactory();

            _testStore = NpgsqlTestStore.CreateScratch();

            _options =
                new DbContextOptionsBuilder()
                    .UseNpgsql(_testStore.ConnectionString, b => b.ApplyConfiguration())
                    .UseInternalServiceProvider(
                        new ServiceCollection()
                            .AddEntityFrameworkNpgsql()
                            .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                            .BuildServiceProvider())
                    .Options;

            using (RangeContext context = CreateContext())
            {
                context.Database.EnsureCreated();

                context.RangeTestEntities.AddRange(
                    new RangeTestEntity
                    {
                        Id = 1,
                        Range = new NpgsqlRange<int>(-10, 10)
                    },
                    new RangeTestEntity
                    {
                        Id = 2,
                        Range = new NpgsqlRange<int>(-5, 5)
                    },
                    new RangeTestEntity
                    {
                        Id = 3,
                        Range = new NpgsqlRange<int>(0, false, 10, false)
                    },
                    new RangeTestEntity
                    {
                        Id = 4,
                        Range = new NpgsqlRange<int>(0, false, true, 0, false, true)
                    });

                context.SaveChanges();
            }
        }

        /// <summary>
        /// Creates a new <see cref="RangeContext"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="RangeContext"/> for testing.
        /// </returns>
        public RangeContext CreateContext()
        {
            return new RangeContext(_options);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _testStore.Dispose();
        }
    }
}
