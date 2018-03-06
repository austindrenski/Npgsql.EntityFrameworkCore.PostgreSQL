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
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Query.Expressions.Internal
{
    /// <summary>
    /// Represents a PostgreSQL range operator.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/functions-range.html
    /// </remarks>
    public class NpgsqlRangeOperatorExpression : Expression
    {
        /// <summary>
        /// Maps the <see cref="ExpressionType"/> to the <see cref="OperatorType"/>.
        /// </summary>
        [NotNull] static readonly Dictionary<ExpressionType, OperatorType> ExpressionOperators =
            new Dictionary<ExpressionType, OperatorType>
            {
                // @formatter:off
                { ExpressionType.Equal,              OperatorType.Equal              },
                { ExpressionType.NotEqual,           OperatorType.NotEqual           },
                { ExpressionType.LessThan,           OperatorType.LessThan           },
                { ExpressionType.GreaterThan,        OperatorType.GreaterThan        },
                { ExpressionType.LessThanOrEqual,    OperatorType.LessThanOrEqual    },
                { ExpressionType.GreaterThanOrEqual, OperatorType.GreaterThanOrEqual }
                // @formatter:on
            };

        /// <summary>
        /// Maps the <see cref="OperatorType"/> into a PostgreSQL operator symbol.
        /// </summary>
        [NotNull] static readonly Dictionary<OperatorType, string> OperatorSymbols =
            new Dictionary<OperatorType, string>
            {
                // @formatter:off
                { OperatorType.Equal,                "="   },
                { OperatorType.NotEqual,             "<>"  },
                { OperatorType.LessThan,             "<"   },
                { OperatorType.GreaterThan,          ">"   },
                { OperatorType.LessThanOrEqual,      "<="  },
                { OperatorType.GreaterThanOrEqual,   ">="  },
                { OperatorType.Contains,             "@>"  },
                { OperatorType.ContainedBy,          "<@"  },
                { OperatorType.Overlaps,             "&&"  },
                { OperatorType.IsStrictlyLeftOf,     "<<"  },
                { OperatorType.IsStrictlyRightOf,    ">>"  },
                { OperatorType.DoesNotExtendRightOf, "&<"  },
                { OperatorType.DoesNotExtendLeftOf,  "&>"  },
                { OperatorType.IsAdjacentTo,         "-|-" },
                { OperatorType.Union,                "+"   },
                { OperatorType.Intersection,         "*"   },
                { OperatorType.Difference,           "-"   }
                // @formatter:on
            };

        /// <summary>
        /// The generic type definition for <see cref="NpgsqlRange{T}"/>.
        /// </summary>
        [NotNull] static readonly Type NpgsqlRangeType = typeof(NpgsqlRange<>);

        /// <inheritdoc />
        public override ExpressionType NodeType { get; } = ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type { get; } = typeof(bool);

        /// <summary>
        /// Gets the item to the left of the operator.
        /// </summary>
        public virtual Expression Left { get; }

        /// <summary>
        /// Gets the item to the right of the operator.
        /// </summary>
        public virtual Expression Right { get; }

        /// <summary>
        /// The operator.
        /// </summary>
        public virtual OperatorType Operator { get; }

        /// <summary>
        /// The operator symbol.
        /// </summary>
        [NotNull]
        public virtual string OperatorSymbol => OperatorSymbols[Operator];

        /// <summary>
        /// Creates a new instance of <see cref="NpgsqlRangeOperatorExpression"/>.
        /// </summary>
        /// <param name="left">
        /// The item to the left of the operator.
        /// </param>
        /// <param name="right">
        /// The item to the right of the operator.
        /// </param>
        /// <param name="operatorType">
        /// The type of range operation.
        /// </param>
        public NpgsqlRangeOperatorExpression([NotNull] Expression left, [NotNull] Expression right, OperatorType operatorType)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            Left = left;
            Right = right;
            Operator = operatorType;
        }

        /// <inheritdoc />
        protected override Expression Accept([NotNull] ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return visitor is NpgsqlQuerySqlGenerator npsgqlGenerator
                ? npsgqlGenerator.VisitRangeOperator(this)
                : base.Accept(visitor);
        }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Expression newRange = visitor.Visit(Left);
            Expression newItem = visitor.Visit(Right);

            return
                Left == newRange && Right == newItem
                    ? this
                    : new NpgsqlRangeOperatorExpression(newRange, newItem, Operator);
        }

        /// <summary>
        /// Returns a <see cref="NpgsqlRangeOperatorExpression"/> if applicable.
        /// </summary>
        /// <remarks>
        /// This returns a NpgsqlRangeOperatorExpression IFF:
        ///   1. Both left and right are <see cref="NpgsqlRange{T}"/>
        ///   2. The expression node type is one of:
        ///     - Equal
        ///     - NotEqual
        ///     - LessThan
        ///     - GreaterThan
        ///     - LessThanOrEqual
        ///     - GreaterThanOrEqual
        /// </remarks>
        /// <param name="expression">
        /// The binary expression to test.
        /// </param>
        /// <returns>
        /// A <see cref="NpgsqlRangeOperatorExpression"/> or null.
        /// </returns>
        [CanBeNull]
        public static NpgsqlRangeOperatorExpression TryVisitBinary([NotNull] BinaryExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            Type leftType = expression.Left.Type;
            Type rightType = expression.Right.Type;

            if (!leftType.IsGenericType)
                return null;
            if (!rightType.IsGenericType)
                return null;
            if (leftType.GetGenericTypeDefinition() != NpgsqlRangeType)
                return null;
            if (rightType.GetGenericTypeDefinition() != NpgsqlRangeType)
                return null;

            return
                ExpressionOperators.TryGetValue(expression.NodeType, out OperatorType operatorType)
                    ? new NpgsqlRangeOperatorExpression(expression.Left, expression.Right, operatorType)
                    : null;
        }

        /// <inheritdoc />
        [NotNull]
        public override string ToString() => $"{Left} {OperatorSymbol} {Right}";

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (!(obj is NpgsqlRangeOperatorExpression other))
                return false;

            return
                Equals(Left, other.Left) &&
                Equals(Right, other.Right) &&
                NodeType == other.NodeType &&
                Operator == other.Operator &&
                Type == other.Type;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Left?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (Right?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (int)NodeType;
                hashCode = (hashCode * 397) ^ (int)Operator;
                hashCode = (hashCode * 397) ^ (Type?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        /// <summary>
        /// Describes the operator type of a range expression.
        /// </summary>
        public enum OperatorType
        {
            /// <summary>
            /// No operator specified.
            /// </summary>
            None,

            /// <summary>
            /// The = operator.
            /// </summary>
            Equal,

            /// <summary>
            /// The &lt;> operator.
            /// </summary>
            NotEqual,

            /// <summary>
            /// The &lt; operator.
            /// </summary>
            LessThan,

            /// <summary>
            /// The > operator.
            /// </summary>
            GreaterThan,

            /// <summary>
            /// The &lt;= operator.
            /// </summary>
            LessThanOrEqual,

            /// <summary>
            /// The >= operator.
            /// </summary>
            GreaterThanOrEqual,

            /// <summary>
            /// The @> operator.
            /// </summary>
            Contains,

            /// <summary>
            /// The &lt;@ operator.
            /// </summary>
            ContainedBy,

            /// <summary>
            /// The && operator.
            /// </summary>
            Overlaps,

            /// <summary>
            /// The &lt;&lt; operator.
            /// </summary>
            IsStrictlyLeftOf,

            /// <summary>
            /// The >> operator.
            /// </summary>
            IsStrictlyRightOf,

            /// <summary>
            /// The &amp;&lt; operator.
            /// </summary>
            DoesNotExtendRightOf,

            /// <summary>
            /// The &amp;&gt; operator.
            /// </summary>
            DoesNotExtendLeftOf,

            /// <summary>
            /// The -|- operator.
            /// </summary>
            IsAdjacentTo,

            /// <summary>
            /// The + operator.
            /// </summary>
            Union,

            /// <summary>
            /// The * operator.
            /// </summary>
            Intersection,

            /// <summary>
            /// The - operator.
            /// </summary>
            Difference
        }
    }
}
