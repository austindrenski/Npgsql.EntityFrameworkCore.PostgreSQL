using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using System.Linq.Expressions;
using System.Reflection;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Translates a range containment method call.
    /// </summary>
    public class NpgsqlRangeContainmentTranslator : IMethodCallTranslator
    {
        /// <summary>
        /// Caches runtime method information for <see cref="NpgsqlRangeFunctionExtensions.Contains{T}(NpgsqlRange{T}, T)"/>.
        /// </summary>
        private static readonly MethodInfo ContainsMethodInfo;

        /// <summary>
        /// Caches runtime method information for <see cref="NpgsqlRangeFunctionExtensions.ContainedBy{T}(T, NpgsqlRange{T})"/>.
        /// </summary>
        private static readonly MethodInfo ContainedByMethodInfo;

        /// <summary>
        /// Initializes static resources.
        /// </summary>
        static NpgsqlRangeContainmentTranslator()
        {
            ContainsMethodInfo =
                typeof(NpgsqlRangeFunctionExtensions).GetMethod(nameof(NpgsqlRangeFunctionExtensions.Contains));

            ContainedByMethodInfo =
                typeof(NpgsqlRangeFunctionExtensions).GetMethod(nameof(NpgsqlRangeFunctionExtensions.ContainedBy));
        }

        /// <inheritdoc />
        public Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (!methodCallExpression.Method.IsGenericMethod)
            {
                return null;
            }

            MethodInfo generic = methodCallExpression.Method.GetGenericMethodDefinition();

            if (generic == ContainsMethodInfo)
            {
                return new RangeContainsExpression(methodCallExpression.Arguments[0], methodCallExpression.Arguments[1]);
            }

            return
                generic == ContainedByMethodInfo
                    ? new RangeContainsExpression(methodCallExpression.Arguments[1], methodCallExpression.Arguments[0])
                    : null;
        }
    }
}
