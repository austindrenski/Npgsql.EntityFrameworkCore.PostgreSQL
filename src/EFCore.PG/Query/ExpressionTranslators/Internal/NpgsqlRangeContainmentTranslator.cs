using System;
using System.Linq;
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
        private static readonly MethodInfo RangeContainsValue;

        /// <summary>
        /// Caches runtime method information for <see cref="NpgsqlRangeFunctionExtensions.Contains{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        private static readonly MethodInfo RangeContainsRange;

        /// <summary>
        /// Caches runtime method information for <see cref="NpgsqlRangeFunctionExtensions.ContainedBy{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        private static readonly MethodInfo RangeContainedByRange;

        /// <summary>
        /// Caches runtime method information for <see cref="NpgsqlRangeFunctionExtensions.ContainedBy{T}(T, NpgsqlRange{T})"/>.
        /// </summary>
        private static readonly MethodInfo ValueContainedByRange;

        /// <summary>
        /// Initializes static resources.
        /// </summary>
        static NpgsqlRangeContainmentTranslator()
        {
            MethodInfo[] extensions =
                typeof(NpgsqlRangeFunctionExtensions)
                    .GetMethods()
                    .Where(x => x.IsGenericMethod)
                    .Select(x => x.GetGenericMethodDefinition())
                    .Select(x => x.MakeGenericMethod(typeof(int)))
                    .ToArray();

            RangeContainsValue =
                extensions.Where(x => x.Name == nameof(NpgsqlRangeFunctionExtensions.Contains))
                          .Single(
                              x =>
                                  x.GetParameters()
                                   .Select(y => y.ParameterType)
                                   .SequenceEqual(new Type[] { typeof(NpgsqlRange<int>), typeof(int) }))
                          .GetGenericMethodDefinition();

            RangeContainsRange =
                extensions.Where(x => x.Name == nameof(NpgsqlRangeFunctionExtensions.Contains))
                          .Single(
                              x =>
                                  x.GetParameters()
                                   .Select(y => y.ParameterType)
                                   .SequenceEqual(new Type[] { typeof(NpgsqlRange<int>), typeof(NpgsqlRange<int>) }))
                          .GetGenericMethodDefinition();

            ValueContainedByRange =
                extensions.Where(x => x.Name == nameof(NpgsqlRangeFunctionExtensions.ContainedBy))
                          .Single(
                              x =>
                                  x.GetParameters()
                                   .Select(y => y.ParameterType)
                                   .SequenceEqual(new Type[] { typeof(int), typeof(NpgsqlRange<int>) }))
                          .GetGenericMethodDefinition();

            RangeContainedByRange =
                extensions.Where(x => x.Name == nameof(NpgsqlRangeFunctionExtensions.ContainedBy))
                          .Single(
                              x =>
                                  x.GetParameters()
                                   .Select(y => y.ParameterType)
                                   .SequenceEqual(new Type[] { typeof(NpgsqlRange<int>), typeof(NpgsqlRange<int>) }))
                          .GetGenericMethodDefinition();
        }

        /// <inheritdoc />
        public Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (!methodCallExpression.Method.IsGenericMethod)
            {
                return null;
            }

            MethodInfo generic = methodCallExpression.Method.GetGenericMethodDefinition();

            if (generic == RangeContainsValue || generic == RangeContainsRange)
            {
                return new RangeContainsExpression(methodCallExpression.Arguments[0], methodCallExpression.Arguments[1]);
            }

            if (generic == ValueContainedByRange || generic == RangeContainedByRange)
            {
                return new RangeContainsExpression(methodCallExpression.Arguments[1], methodCallExpression.Arguments[0]);
            }

            return null;
        }
    }
}
