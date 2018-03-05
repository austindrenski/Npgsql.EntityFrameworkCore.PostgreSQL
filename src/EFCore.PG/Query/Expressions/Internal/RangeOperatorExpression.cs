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
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions.Internal
{
    /// <summary>
    /// Represents a PostgreSQL range operator.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/functions-range.html
    /// </remarks>
    public class RangeOperatorExpression : Expression
    {
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
        public virtual string OperatorSymbol => OperatorString(Operator);

        /// <summary>
        /// Creates a new instance of <see cref="RangeOperatorExpression"/>.
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
        public RangeOperatorExpression([NotNull] Expression left, [NotNull] Expression right, OperatorType operatorType)
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
                    : new RangeOperatorExpression(newRange, newItem, Operator);
        }

        /// <inheritdoc />
        [NotNull]
        public override string ToString()
        {
            return $"{Left} {OperatorSymbol} {Right}";
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (!(obj is RangeOperatorExpression other))
            {
                return false;
            }

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
        /// Translates the <see cref="OperatorType"/> into a PostgreSQL operator symbol.
        /// </summary>
        /// <returns>
        /// The PostgreSQL operator symbol.
        /// </returns>
        /// <exception cref="NotSupportedException" />
        [NotNull]
        private static string OperatorString(OperatorType operatorType)
        {
            switch (operatorType)
            {
            case OperatorType.None:
            {
                goto default;
            }
            case OperatorType.Equal:
            {
                return "=";
            }
            case OperatorType.NotEqual:
            {
                return "<>";
            }
            case OperatorType.LessThan:
            {
                return "<";
            }
            case OperatorType.GreaterThan:
            {
                return ">";
            }
            case OperatorType.LessThanOrEqual:
            {
                return "<=";
            }
            case OperatorType.GreaterThanOrEqual:
            {
                return ">=";
            }
            case OperatorType.Contains:
            {
                return "@>";
            }
            case OperatorType.ContainedBy:
            {
                return "<@";
            }
            case OperatorType.Overlaps:
            {
                return "&&";
            }
            case OperatorType.StrictlyLeftOf:
            {
                return "<<";
            }
            case OperatorType.StrictlyRightOf:
            {
                return ">>";
            }
            case OperatorType.DoesNotExtendToTheRightOf:
            {
                return "&<";
            }
            case OperatorType.DoesNotExtendToTheLeftOf:
            {
                return "&>";
            }
            case OperatorType.IsAdjacentTo:
            {
                return "-|-";
            }
            case OperatorType.Union:
            {
                return "+";
            }
            case OperatorType.Intersection:
            {
                return "*";
            }
            case OperatorType.Difference:
            {
                return "-";
            }
            default:
            {
                throw new NotSupportedException($"Range operator '{operatorType}' is not supported.");
            }
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
            StrictlyLeftOf,

            /// <summary>
            /// The >> operator.
            /// </summary>
            StrictlyRightOf,

            /// <summary>
            /// The &amp;&lt; operator.
            /// </summary>
            DoesNotExtendToTheRightOf,

            /// <summary>
            /// The &amp;&gt; operator.
            /// </summary>
            DoesNotExtendToTheLeftOf,

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
