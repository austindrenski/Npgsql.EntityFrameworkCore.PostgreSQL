﻿#region License

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
        private static readonly MethodInfo RangeContainsValue;

        /// <summary>
        /// Caches runtime method information for <see cref="NpgsqlRangeExtensions.Contains{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        private static readonly MethodInfo RangeContainsRange;

        /// <summary>
        /// Caches runtime method information for <see cref="NpgsqlRangeExtensions.ContainedBy{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        private static readonly MethodInfo RangeContainedByRange;

        /// <summary>
        /// Caches runtime method information for <see cref="NpgsqlRangeExtensions.ContainedBy{T}(T, NpgsqlRange{T})"/>.
        /// </summary>
        private static readonly MethodInfo ValueContainedByRange;

        /// <summary>
        /// Caches runtime method information for <see cref="NpgsqlRangeExtensions.Overlaps{T}(NpgsqlRange{T}, NpgsqlRange{T})"/>.
        /// </summary>
        private static readonly MethodInfo RangeOverlaps;

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

            RangeContainsValue =
                extensions.Where(x => x.Name == nameof(NpgsqlRangeExtensions.Contains))
                          .Single(
                              x =>
                                  x.GetParameters()
                                   .Select(y => y.ParameterType)
                                   .SequenceEqual(new Type[] { typeof(NpgsqlRange<int>), typeof(int) }))
                          .GetGenericMethodDefinition();

            RangeContainsRange =
                extensions.Where(x => x.Name == nameof(NpgsqlRangeExtensions.Contains))
                          .Single(
                              x =>
                                  x.GetParameters()
                                   .Select(y => y.ParameterType)
                                   .SequenceEqual(new Type[] { typeof(NpgsqlRange<int>), typeof(NpgsqlRange<int>) }))
                          .GetGenericMethodDefinition();

            ValueContainedByRange =
                extensions.Where(x => x.Name == nameof(NpgsqlRangeExtensions.ContainedBy))
                          .Single(
                              x =>
                                  x.GetParameters()
                                   .Select(y => y.ParameterType)
                                   .SequenceEqual(new Type[] { typeof(int), typeof(NpgsqlRange<int>) }))
                          .GetGenericMethodDefinition();

            RangeContainedByRange =
                extensions.Where(x => x.Name == nameof(NpgsqlRangeExtensions.ContainedBy))
                          .Single(
                              x =>
                                  x.GetParameters()
                                   .Select(y => y.ParameterType)
                                   .SequenceEqual(new Type[] { typeof(NpgsqlRange<int>), typeof(NpgsqlRange<int>) }))
                          .GetGenericMethodDefinition();

            RangeOverlaps =
                extensions.Where(x => x.Name == nameof(NpgsqlRangeExtensions.Overlaps))
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
                return new RangeOperatorExpression(methodCallExpression.Arguments[0], methodCallExpression.Arguments[1], RangeOperatorExpression.OperatorType.Contains);
            }

            if (generic == ValueContainedByRange || generic == RangeContainedByRange)
            {
                return new RangeOperatorExpression(methodCallExpression.Arguments[0], methodCallExpression.Arguments[1], RangeOperatorExpression.OperatorType.ContainedBy);
            }

            if (generic == RangeOverlaps)
            {
                return new RangeOperatorExpression(methodCallExpression.Arguments[0], methodCallExpression.Arguments[1], RangeOperatorExpression.OperatorType.Overlaps);
            }

            return null;
        }
    }
}
