#region License

// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

#endregion

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
    public class NpgsqlRangeOperatorTranslator : IMethodCallTranslator
    {
        /// <summary>
        /// Caches runtime method information for <see cref="NpgsqlRangeExtensions.Contains{T}(NpgsqlRange{T}, T)"/>.
        /// </summary>
        private static readonly MethodInfo ContainsValue;

        /// <summary>
        /// Caches runtime method information for <see cref="NpgsqlRangeExtensions.Contains{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        private static readonly MethodInfo ContainsRange;

        /// <summary>
        /// Caches runtime method information for <see cref="NpgsqlRangeExtensions.ContainedBy{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        private static readonly MethodInfo RangeContainedBy;

        /// <summary>
        /// Caches runtime method information for <see cref="NpgsqlRangeExtensions.ContainedBy{T}(T, NpgsqlRange{T})"/>.
        /// </summary>
        private static readonly MethodInfo ValueContainedBy;

        /// <summary>
        /// Caches runtime method information for <see cref="NpgsqlRangeExtensions.StrictlyLeftOf{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        private static readonly MethodInfo StrictlyLeftOf;

        /// <summary>
        /// Caches runtime method information for <see cref="NpgsqlRangeExtensions.StrictlyRightOf{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        private static readonly MethodInfo StrictlyRightOf;

        /// <summary>
        /// Caches runtime method information for <see cref="NpgsqlRangeExtensions.DoesNotExtendToTheLeftOf{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        private static readonly MethodInfo DoesNotExtendToTheLeftOf;

        /// <summary>
        /// Caches runtime method information for <see cref="NpgsqlRangeExtensions.DoesNotExtendToTheRightOf{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        private static readonly MethodInfo DoesNotExtendToTheRightOf;

        /// <summary>
        /// Caches runtime method information for <see cref="NpgsqlRangeExtensions.Adjacent{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        private static readonly MethodInfo Adjacent;

        /// <summary>
        /// Caches runtime method information for <see cref="NpgsqlRangeExtensions.Overlaps{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        private static readonly MethodInfo Overlaps;

        /// <summary>
        /// Caches runtime method information for <see cref="NpgsqlRangeExtensions.Union{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        private static readonly MethodInfo Union;

        /// <summary>
        /// Caches runtime method information for <see cref="NpgsqlRangeExtensions.Intersect{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        private static readonly MethodInfo Intersect;

        /// <summary>
        /// Caches runtime method information for <see cref="NpgsqlRangeExtensions.Except{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        private static readonly MethodInfo Except;

        /// <summary>
        /// Initializes static resources.
        /// </summary>
        static NpgsqlRangeOperatorTranslator()
        {
            MethodInfo[] extensions =
                typeof(NpgsqlRangeExtensions)
                    .GetMethods()
                    .Where(x => x.IsGenericMethod)
                    .Select(x => x.GetGenericMethodDefinition())
                    .Select(x => x.MakeGenericMethod(typeof(int)))
                    .ToArray();

            ContainsValue =
                extensions.Where(x => x.Name == nameof(NpgsqlRangeExtensions.Contains))
                          .Single(
                              x =>
                                  x.GetParameters()
                                   .Select(y => y.ParameterType)
                                   .SequenceEqual(new Type[] { typeof(NpgsqlRange<int>), typeof(int) }))
                          .GetGenericMethodDefinition();

            ContainsRange =
                extensions.Where(x => x.Name == nameof(NpgsqlRangeExtensions.Contains))
                          .Single(
                              x =>
                                  x.GetParameters()
                                   .Select(y => y.ParameterType)
                                   .SequenceEqual(new Type[] { typeof(NpgsqlRange<int>), typeof(NpgsqlRange<int>) }))
                          .GetGenericMethodDefinition();

            ValueContainedBy =
                extensions.Where(x => x.Name == nameof(NpgsqlRangeExtensions.ContainedBy))
                          .Single(
                              x =>
                                  x.GetParameters()
                                   .Select(y => y.ParameterType)
                                   .SequenceEqual(new Type[] { typeof(int), typeof(NpgsqlRange<int>) }))
                          .GetGenericMethodDefinition();

            RangeContainedBy =
                extensions.Where(x => x.Name == nameof(NpgsqlRangeExtensions.ContainedBy))
                          .Single(
                              x =>
                                  x.GetParameters()
                                   .Select(y => y.ParameterType)
                                   .SequenceEqual(new Type[] { typeof(NpgsqlRange<int>), typeof(NpgsqlRange<int>) }))
                          .GetGenericMethodDefinition();

            Overlaps =
                extensions.Where(x => x.Name == nameof(NpgsqlRangeExtensions.Overlaps))
                          .Single(
                              x =>
                                  x.GetParameters()
                                   .Select(y => y.ParameterType)
                                   .SequenceEqual(new Type[] { typeof(NpgsqlRange<int>), typeof(NpgsqlRange<int>) }))
                          .GetGenericMethodDefinition();

            StrictlyLeftOf = null;

            StrictlyRightOf = null;

            DoesNotExtendToTheLeftOf = null;

            DoesNotExtendToTheRightOf = null;

            Adjacent = null;

            Union = null;

            Intersect = null;

            Except = null;
        }

        /// <inheritdoc />
        public Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (!methodCallExpression.Method.IsGenericMethod)
            {
                return null;
            }

            if (methodCallExpression.Method.Name == "Equals")
            {
                return
                    new RangeOperatorExpression(
                        methodCallExpression.Arguments[0],
                        methodCallExpression.Arguments[1],
                        RangeOperatorExpression.OperatorType.Equal);
            }

            RangeOperatorExpression.OperatorType operatorType = RangeOperatorExpression.OperatorType.None;

            MethodInfo generic = methodCallExpression.Method.GetGenericMethodDefinition();

            if (generic == ContainsValue || generic == ContainsRange)
            {
                operatorType = RangeOperatorExpression.OperatorType.Contains;
            }
            else if (generic == ValueContainedBy || generic == RangeContainedBy)
            {
                operatorType = RangeOperatorExpression.OperatorType.ContainedBy;
            }
            else if (generic == Overlaps)
            {
                operatorType = RangeOperatorExpression.OperatorType.Overlaps;
            }
            else if (generic == StrictlyLeftOf)
            {
                operatorType = RangeOperatorExpression.OperatorType.StrictlyLeftOf;
            }
            else if (generic == StrictlyRightOf)
            {
                operatorType = RangeOperatorExpression.OperatorType.StrictlyRightOf;
            }
            else if (generic == DoesNotExtendToTheLeftOf)
            {
                operatorType = RangeOperatorExpression.OperatorType.DoesNotExtendToTheLeftOf;
            }
            else if (generic == DoesNotExtendToTheRightOf)
            {
                operatorType = RangeOperatorExpression.OperatorType.DoesNotExtendToTheRightOf;
            }
            else if (generic == Adjacent)
            {
                operatorType = RangeOperatorExpression.OperatorType.IsAdjacentTo;
            }
            else if (generic == Union)
            {
                operatorType = RangeOperatorExpression.OperatorType.Union;
            }
            else if (generic == Intersect)
            {
                operatorType = RangeOperatorExpression.OperatorType.Intersection;
            }
            else if (generic == Except)
            {
                operatorType = RangeOperatorExpression.OperatorType.Difference;
            }

            return
                operatorType != RangeOperatorExpression.OperatorType.None
                    ? new RangeOperatorExpression(methodCallExpression.Arguments[0], methodCallExpression.Arguments[1], operatorType)
                    : null;
        }
    }
}
