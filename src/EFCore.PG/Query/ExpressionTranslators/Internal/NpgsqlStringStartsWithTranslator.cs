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

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for <see cref="string.StartsWith(string)"/> as a PostgreSQL LIKE operation.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/10/static/functions-matching.html
    /// </remarks>
    public class NpgsqlStringStartsWithTranslator : IMethodCallTranslator
    {
        /// <summary>
        /// The static method info for <see cref="string.StartsWith(string)"/>.
        /// </summary>
        [NotNull] static readonly MethodInfo StartsWith =
            typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) });

        /// <summary>
        /// The static method info for <see cref="string.Concat(string, string)"/>.
        /// </summary>
        [NotNull] static readonly MethodInfo Concat =
            typeof(string).GetRuntimeMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) });

        /// <inheritdoc />
        [CanBeNull]
        public virtual Expression Translate(MethodCallExpression e)
        {
            if (e.Method != StartsWith)
                return null;

            if (!(e.Object is Expression match))
                return null;

            if (!(e.Arguments[0] is Expression pattern))
                return null;

            if (!(Escape(pattern) is Expression escaped))
                return null;

            if (escaped is ConstantExpression)
                return new LikeExpression(match, escaped);

            // The pattern isn't a constant (i.e. parameter, database column...).
            // First run LIKE against the *unescaped* pattern (which will efficiently use indices),
            // but then add another test to filter out false positives.
            Expression leftExpr =
                new SqlFunctionExpression(
                    "LEFT",
                    typeof(string),
                    new[]
                    {
                        match,
                        new SqlFunctionExpression("LENGTH", typeof(int), new[] { pattern }),
                    });

            // If StartsWith is being invoked on a citext, the LEFT() function above will return a reglar text
            // and the comparison will be case-sensitive. So we need to explicitly cast LEFT()'s return type
            // to citext. See #319.
            if (match.FindProperty(typeof(string))?.GetConfiguredColumnType() == "citext")
                leftExpr = new ExplicitStoreTypeCastExpression(leftExpr, typeof(string), "citext");

            return
                Expression.AndAlso(
                    new LikeExpression(match, escaped),
                    Expression.Equal(leftExpr, pattern));
        }

        /// <summary>
        /// Escapes the pattern if it is a <see cref="ConstantExpression"/>. Otherwise, returns null.
        /// </summary>
        /// <param name="pattern">The pattern expression.</param>
        /// <returns>
        /// The escaped pattern if constant; otherwise, null.
        /// </returns>
        /// <remarks>
        /// If the pattern is constant, escape all special characters (%, _, \).
        /// </remarks>
        [CanBeNull]
        public static Expression Escape([CanBeNull] Expression pattern)
        {
            switch (pattern)
            {
            case ConstantExpression c when c.Value is string literal:
                return Expression.Constant(Regex.Replace(literal, @"([%_\\])", @"\$1") + '%');

            case ConstantExpression c when c.Value is string[] array:
                return Expression.Constant(array.Select(x => Regex.Replace(x, @"([%_\\])", @"\$1") + '%').ToArray());

            case ConstantExpression c when c.Value is List<string> list:
                return Expression.Constant(list.Select(x => Regex.Replace(x, @"([%_\\])", @"\$1") + '%').ToArray());

            case Expression e when e.Type == typeof(string):
                return Expression.Add(e, Expression.Constant("%"), Concat);

            case Expression e when e.Type == typeof(string[]) || e.Type == typeof(List<string>):
            {
                var from =
                    new MainFromClause(
                        "<array_item>",
                        typeof(string),
                        new SqlFunctionExpression("unnest", typeof(IQueryable<string>), new[] { e }));

                var select =
                    new SelectClause(
                        Expression.Add(
                            new QuerySourceReferenceExpression(from),
                            Expression.Constant("%"),
                            Concat));

                var queryModel = new QueryModel(from, select);

                from.ItemName = queryModel.GetNewName(from.ItemName);

                return new SubQueryExpression(queryModel);
            }

            default:
                return null;
            }
        }
    }
}
