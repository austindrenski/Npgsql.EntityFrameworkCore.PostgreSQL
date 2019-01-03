﻿using System;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class SystemColumnTest : IDisposable
    {
        [Fact]
        public void Xmin()
        {
            using (var context = CreateContext())
            {
                var e = new SomeEntity { Name = "Bart" };
                context.Entities.Add(e);
                context.SaveChanges();
                var firstVersion = e.Version;

                e.Name = "Lisa";
                context.SaveChanges();
                var secondVersion = e.Version;

                Assert.NotEqual(firstVersion, secondVersion);
            }
        }

        private class SystemColumnContext : DbContext
        {
            internal SystemColumnContext(DbContextOptions options) : base(options) {}

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public DbSet<SomeEntity> Entities { get; set; }

            protected override void OnModelCreating(ModelBuilder builder)
                => builder.Entity<SomeEntity>().Property(e => e.Version)
                          .HasColumnName("xmin")
                          .HasColumnType("xid")
                          .ValueGeneratedOnAddOrUpdate()
                          .IsConcurrencyToken();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public class SomeEntity
        {
            // ReSharper disable UnusedMember.Global
            // ReSharper disable UnusedAutoPropertyAccessor.Global
            public int Id { get; set; }
            public string Name { get; set; }
            public uint Version { get; set; }
            // ReSharper restore UnusedMember.Global
            // ReSharper restore UnusedAutoPropertyAccessor.Global
        }

        public SystemColumnTest()
        {
            _testStore = NpgsqlTestStore.CreateScratch();

            _options = new DbContextOptionsBuilder()
                .UseNpgsql(_testStore.ConnectionString)
                .Options;

            using (var context = CreateContext())
                context.Database.EnsureCreated();
        }

        SystemColumnContext CreateContext() => new SystemColumnContext(_options);

        readonly DbContextOptions _options;
        readonly NpgsqlTestStore _testStore;

        public void Dispose() => _testStore.Dispose();
    }
}
