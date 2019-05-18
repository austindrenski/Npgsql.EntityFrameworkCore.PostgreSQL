using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// A composite method call translator that dispatches to multiple specialized method call translators specific to Npgsql.
    /// </summary>
    public class NpgsqlCompositeMethodCallTranslator : RelationalCompositeMethodCallTranslator
    {
        /// <summary>
        /// The default method call translators registered by the Npgsql provider.
        /// </summary>
        [NotNull] [ItemNotNull] static readonly IMethodCallTranslator[] MethodCallTranslators =
        {
            new NpgsqlArraySequenceEqualTranslator(),
            new NpgsqlConvertTranslator(),
            new NpgsqlGuidTranslator(),
            new NpgsqlLikeTranslator(),
            new NpgsqlObjectToStringTranslator(),
            new NpgsqlStringTranslator(),
            new NpgsqlStringToLowerTranslator(),
            new NpgsqlStringToUpperTranslator(),
            new NpgsqlRegexIsMatchTranslator(),
            new NpgsqlFullTextSearchMethodTranslator(),
            new NpgsqlRangeTranslator(),
            new NpgsqlNetworkTranslator(),
            new NpgsqlTrigramMethodCallTranslator()
        };

        /// <inheritdoc />
        public NpgsqlCompositeMethodCallTranslator(
            [NotNull] RelationalCompositeMethodCallTranslatorDependencies dependencies,
            [NotNull] INpgsqlOptions npgsqlOptions)
            : base(dependencies)
        {
            var versionDependentTranslators = new IMethodCallTranslator[]
            {
                new NpgsqlDateAddTranslator(npgsqlOptions.PostgresVersion),
                new NpgsqlMathTranslator(npgsqlOptions.PostgresVersion)
            };

            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            AddTranslators(MethodCallTranslators);

            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            AddTranslators(versionDependentTranslators);
        }

        /// <summary>
        /// Adds additional dispatches to the translators list.
        /// </summary>
        /// <param name="translators">The translators.</param>
        public new virtual void AddTranslators([NotNull] [ItemNotNull] IEnumerable<IMethodCallTranslator> translators)
            => base.AddTranslators(translators);
    }
}
