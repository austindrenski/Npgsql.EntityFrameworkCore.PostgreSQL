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
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using NpgsqlTypes;
using static Microsoft.EntityFrameworkCore.Query.Expressions.Internal.NpgsqlRangeOperatorExpression;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Translates a range operator method call.
    /// </summary>
    public class NpgsqlRangeOperatorTranslator : IMethodCallTranslator
    {
        /// <summary>
        /// Caches the <see cref="MethodInfo"/> for the methods in <see cref="NpgsqlRangeExtensions"/>.
        /// </summary>
        [NotNull] static readonly MethodInfo[] NpgsqlRangeExtensionsMethods = typeof(NpgsqlRangeExtensions).GetRuntimeMethods().ToArray();

        /// <summary>
        /// Maps the generic definitions of the methods supported by this translator to the appropriate PostgreSQL operator.
        /// </summary>
        /// <remarks>
        /// The <see cref="MakeGeneric{T}"/> method returns the generic method definition.
        /// </remarks>
        [NotNull] static readonly Dictionary<MethodInfo, OperatorType> SupportedMethodTranslations =
            new Dictionary<MethodInfo, OperatorType>
            {
                // @formatter:off
                { MakeGeneric<int>(nameof(NpgsqlRangeExtensions.Contains),             typeof(NpgsqlRange<int>), typeof(int)),              OperatorType.Contains },
                { MakeGeneric<int>(nameof(NpgsqlRangeExtensions.Contains),             typeof(NpgsqlRange<int>), typeof(NpgsqlRange<int>)), OperatorType.Contains },
                { MakeGeneric<int>(nameof(NpgsqlRangeExtensions.ContainedBy),          typeof(NpgsqlRange<int>), typeof(NpgsqlRange<int>)), OperatorType.ContainedBy },
                { MakeGeneric<int>(nameof(NpgsqlRangeExtensions.IsStrictlyLeftOf),     typeof(NpgsqlRange<int>), typeof(NpgsqlRange<int>)), OperatorType.IsStrictlyLeftOf },
                { MakeGeneric<int>(nameof(NpgsqlRangeExtensions.IsStrictlyRightOf),    typeof(NpgsqlRange<int>), typeof(NpgsqlRange<int>)), OperatorType.IsStrictlyRightOf },
                { MakeGeneric<int>(nameof(NpgsqlRangeExtensions.DoesNotExtendLeftOf),  typeof(NpgsqlRange<int>), typeof(NpgsqlRange<int>)), OperatorType.DoesNotExtendLeftOf },
                { MakeGeneric<int>(nameof(NpgsqlRangeExtensions.DoesNotExtendRightOf), typeof(NpgsqlRange<int>), typeof(NpgsqlRange<int>)), OperatorType.DoesNotExtendRightOf },
                { MakeGeneric<int>(nameof(NpgsqlRangeExtensions.IsAdjacentTo),         typeof(NpgsqlRange<int>), typeof(NpgsqlRange<int>)), OperatorType.IsAdjacentTo },
                { MakeGeneric<int>(nameof(NpgsqlRangeExtensions.Overlaps),             typeof(NpgsqlRange<int>), typeof(NpgsqlRange<int>)), OperatorType.Overlaps },
                { MakeGeneric<int>(nameof(NpgsqlRangeExtensions.Union),                typeof(NpgsqlRange<int>), typeof(NpgsqlRange<int>)), OperatorType.Union },
                { MakeGeneric<int>(nameof(NpgsqlRangeExtensions.Intersect),            typeof(NpgsqlRange<int>), typeof(NpgsqlRange<int>)), OperatorType.Intersection },
                { MakeGeneric<int>(nameof(NpgsqlRangeExtensions.Except),               typeof(NpgsqlRange<int>), typeof(NpgsqlRange<int>)), OperatorType.Difference }
                // @formatter:on
            };

        /// <inheritdoc />
        [CanBeNull]
        public Expression Translate(MethodCallExpression methodCallExpression) =>
            methodCallExpression.Method.IsGenericMethod &&
            SupportedMethodTranslations.TryGetValue(methodCallExpression.Method.GetGenericMethodDefinition(), out OperatorType operatorType)
                ? new NpgsqlRangeOperatorExpression(methodCallExpression.Arguments[0], methodCallExpression.Arguments[1], operatorType)
                : null;

        /// <summary>
        /// Returns the generic method definition for given the name and parameter types.
        /// </summary>
        /// <param name="name">
        /// The name of the method to find in <see cref="NpgsqlRangeExtensions"/>.
        /// </param>
        /// <param name="types">
        /// The parameter types of the method.
        /// </param>
        /// <returns>
        /// The generic method definition.
        /// </returns>
        /// <exception cref="InvalidOperationException" />
        [NotNull]
        static MethodInfo MakeGeneric<T>([NotNull] string name, [ItemNotNull] params Type[] types) =>
            NpgsqlRangeExtensionsMethods.Where(x => x.Name == name)
                                        .Select(x => x.MakeGenericMethod(typeof(T)))
                                        .Single(x => x.GetParameters().Select(y => y.ParameterType).SequenceEqual(types))
                                        .GetGenericMethodDefinition();
    }
}
