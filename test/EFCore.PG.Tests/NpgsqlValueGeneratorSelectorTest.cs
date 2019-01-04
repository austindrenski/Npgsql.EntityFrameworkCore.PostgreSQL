﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration.Internal;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class NpgsqlValueGeneratorSelectorTest
    {
        [Fact]
        public void Returns_built_in_generators_for_types_setup_for_value_generation()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(AnEntity));

            var selector = NpgsqlTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.IsType<TemporaryIntValueGenerator>(selector.Select(entityType.FindProperty("Id"), entityType));
            Assert.IsType<CustomValueGenerator>(selector.Select(entityType.FindProperty("Custom"), entityType));
            Assert.IsType<TemporaryLongValueGenerator>(selector.Select(entityType.FindProperty("Long"), entityType));
            Assert.IsType<TemporaryShortValueGenerator>(selector.Select(entityType.FindProperty("Short"), entityType));
            Assert.IsType<TemporaryByteValueGenerator>(selector.Select(entityType.FindProperty("Byte"), entityType));
            Assert.IsType<TemporaryCharValueGenerator>(selector.Select(entityType.FindProperty("Char"), entityType));
            Assert.IsType<TemporaryIntValueGenerator>(selector.Select(entityType.FindProperty("NullableInt"), entityType));
            Assert.IsType<TemporaryLongValueGenerator>(selector.Select(entityType.FindProperty("NullableLong"), entityType));
            Assert.IsType<TemporaryShortValueGenerator>(selector.Select(entityType.FindProperty("NullableShort"), entityType));
            Assert.IsType<TemporaryByteValueGenerator>(selector.Select(entityType.FindProperty("NullableByte"), entityType));
            Assert.IsType<TemporaryCharValueGenerator>(selector.Select(entityType.FindProperty("NullableChar"), entityType));
            Assert.IsType<TemporaryDecimalValueGenerator>(selector.Select(entityType.FindProperty("Decimal"), entityType));
            Assert.IsType<StringValueGenerator>(selector.Select(entityType.FindProperty("String"), entityType));
            Assert.IsType<GuidValueGenerator>(selector.Select(entityType.FindProperty("Guid"), entityType));
            Assert.IsType<BinaryValueGenerator>(selector.Select(entityType.FindProperty("Binary"), entityType));
            Assert.IsType<TemporaryIntValueGenerator>(selector.Select(entityType.FindProperty("AlwaysIdentity"), entityType));
            Assert.IsType<NpgsqlSequenceHiLoValueGenerator<int>>(selector.Select(entityType.FindProperty("AlwaysSequence"), entityType));
        }

        [Fact]
        public void Returns_temp_guid_generator_when_default_sql_set()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(AnEntity));

            entityType.FindProperty("Guid").Npgsql().DefaultValueSql = "newid()";

            var selector = NpgsqlTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.IsType<TemporaryGuidValueGenerator>(selector.Select(entityType.FindProperty("Guid"), entityType));
        }

        [Fact]
        public void Returns_sequence_value_generators_when_configured_for_model()
        {
            var model = BuildModel();
            model.Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SequenceHiLo;
            model.Npgsql().GetOrAddSequence(NpgsqlModelAnnotations.DefaultHiLoSequenceName);
            var entityType = model.FindEntityType(typeof(AnEntity));

            foreach (var property in entityType.GetProperties())
            {
                property.ValueGenerated = ValueGenerated.OnAdd;
            }

            var selector = NpgsqlTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.IsType<NpgsqlSequenceHiLoValueGenerator<int>>(selector.Select(entityType.FindProperty("Id"), entityType));
            Assert.IsType<CustomValueGenerator>(selector.Select(entityType.FindProperty("Custom"), entityType));
            Assert.IsType<NpgsqlSequenceHiLoValueGenerator<long>>(selector.Select(entityType.FindProperty("Long"), entityType));
            Assert.IsType<NpgsqlSequenceHiLoValueGenerator<short>>(selector.Select(entityType.FindProperty("Short"), entityType));
            Assert.IsType<NpgsqlSequenceHiLoValueGenerator<byte>>(selector.Select(entityType.FindProperty("Byte"), entityType));
            Assert.IsType<TemporaryCharValueGenerator>(selector.Select(entityType.FindProperty("Char"), entityType));
            Assert.IsType<NpgsqlSequenceHiLoValueGenerator<int>>(selector.Select(entityType.FindProperty("NullableInt"), entityType));
            Assert.IsType<NpgsqlSequenceHiLoValueGenerator<long>>(selector.Select(entityType.FindProperty("NullableLong"), entityType));
            Assert.IsType<NpgsqlSequenceHiLoValueGenerator<short>>(selector.Select(entityType.FindProperty("NullableShort"), entityType));
            Assert.IsType<NpgsqlSequenceHiLoValueGenerator<byte>>(selector.Select(entityType.FindProperty("NullableByte"), entityType));
            Assert.IsType<TemporaryCharValueGenerator>(selector.Select(entityType.FindProperty("NullableChar"), entityType));
            Assert.IsType<TemporaryDecimalValueGenerator>(selector.Select(entityType.FindProperty("Decimal"), entityType));
            Assert.IsType<StringValueGenerator>(selector.Select(entityType.FindProperty("String"), entityType));
            Assert.IsType<GuidValueGenerator>(selector.Select(entityType.FindProperty("Guid"), entityType));
            Assert.IsType<BinaryValueGenerator>(selector.Select(entityType.FindProperty("Binary"), entityType));
            Assert.IsType<TemporaryIntValueGenerator>(selector.Select(entityType.FindProperty("AlwaysIdentity"), entityType));
            Assert.IsType<NpgsqlSequenceHiLoValueGenerator<int>>(selector.Select(entityType.FindProperty("AlwaysSequence"), entityType));
        }

        [Fact]
        public void Throws_for_unsupported_combinations()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(AnEntity));

            var selector = NpgsqlTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.Equal(
                CoreStrings.NoValueGenerator("Random", "AnEntity", typeof(Random).Name),
                Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("Random"), entityType)).Message);
        }

        [Fact]
        public void Returns_generator_configured_on_model_when_property_is_Identity()
        {
            var model = NpgsqlTestHelpers.Instance.BuildModelFor<AnEntity>();
            model.Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SequenceHiLo;
            model.Npgsql().GetOrAddSequence(NpgsqlModelAnnotations.DefaultHiLoSequenceName);
            var entityType = model.FindEntityType(typeof(AnEntity));

            var selector = NpgsqlTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.IsType<NpgsqlSequenceHiLoValueGenerator<int>>(selector.Select(entityType.FindProperty("Id"), entityType));
        }

        private static IMutableModel BuildModel(bool generateValues = true)
        {
            var builder = NpgsqlTestHelpers.Instance.CreateConventionBuilder();

            builder.Ignore<Random>();
            builder.Entity<AnEntity>().Property(e => e.Custom).HasValueGenerator<CustomValueGenerator>();

            var model = builder.Model;
            model.Npgsql().GetOrAddSequence(NpgsqlModelAnnotations.DefaultHiLoSequenceName);

            var entityType = model.FindEntityType(typeof(AnEntity));
            entityType.AddProperty("Random", typeof(Random));

            foreach (var property in entityType.GetProperties())
            {
                property.ValueGenerated = generateValues ? ValueGenerated.OnAdd : ValueGenerated.Never;
            }

            entityType.FindProperty("AlwaysIdentity").ValueGenerated = ValueGenerated.OnAdd;
            entityType.FindProperty("AlwaysIdentity").Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SerialColumn;

            entityType.FindProperty("AlwaysSequence").ValueGenerated = ValueGenerated.OnAdd;
            entityType.FindProperty("AlwaysSequence").Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SequenceHiLo;

            return model;
        }

        private class AnEntity
        {
            // ReSharper disable UnusedMember.Local
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public int Id { get; set; }
            public int Custom { get; set; }
            public long Long { get; set; }
            public short Short { get; set; }
            public byte Byte { get; set; }
            public char Char { get; set; }
            public int? NullableInt { get; set; }
            public long? NullableLong { get; set; }
            public short? NullableShort { get; set; }
            public byte? NullableByte { get; set; }
            public char? NullableChar { get; set; }
            public string String { get; set; }
            public Guid Guid { get; set; }
            public byte[] Binary { get; set; }
            public float Float { get; set; }
            public decimal Decimal { get; set; }
            public int AlwaysIdentity { get; set; }
            public int AlwaysSequence { get; set; }
            public Random Random { get; set; }
            // ReSharper restore UnusedMember.Local
            // ReSharper restore UnusedAutoPropertyAccessor.Local
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class CustomValueGenerator : ValueGenerator<int>
        {
            public override int Next(EntityEntry entry) => throw new NotImplementedException();

            public override bool GeneratesTemporaryValues => false;
        }
    }
}
