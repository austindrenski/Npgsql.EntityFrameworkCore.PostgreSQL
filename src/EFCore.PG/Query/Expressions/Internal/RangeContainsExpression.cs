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
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions.Internal
{
    /// <summary>
    /// Represents a PostgreSQL @> operator (e.g. [2018-03-01,2019-03-01] @> 2018-04-01)
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/functions-range.html
    /// </remarks>
    public class RangeContainsExpression : Expression, IEquatable<RangeContainsExpression>
    {
        /// <inheritdoc />
        public override ExpressionType NodeType { get; } = ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type { get; } = typeof(bool);

        /// <summary>
        /// Gets the range.
        /// </summary>
        public virtual Expression Range { get; }

        /// <summary>
        /// Gets the item.
        /// </summary>
        public virtual Expression Item { get; }

        /// <summary>
        /// Creates a new instance of <see cref="RangeContainsExpression"/>.
        /// </summary>
        /// <param name="range">
        /// The range.
        /// </param>
        /// <param name="item">
        /// The item.
        /// </param>
        public RangeContainsExpression([NotNull] Expression range, [NotNull] Expression item)
        {
            Check.NotNull(range, nameof(range));
            Check.NotNull(item, nameof(item));

            Range = range;
            Item = item;
        }

        /// <inheritdoc />
        protected override Expression Accept([NotNull] ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return visitor is NpgsqlQuerySqlGenerator npsgqlGenerator
                ? npsgqlGenerator.VisitRangeContains(this)
                : base.Accept(visitor);
        }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Expression newRange = visitor.Visit(Range);
            Expression newItem = visitor.Visit(Item);

            return
                Range == newRange && Item == newItem
                    ? this
                    : new RangeContainsExpression(newRange, newItem);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Range} @> {Item}";
        }

        /// <inheritdoc />
        public bool Equals(RangeContainsExpression other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return
                Equals(Range, other.Range) &&
                Equals(Item, other.Item) &&
                NodeType == other.NodeType &&
                Type == other.Type;
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

            return GetType() == obj.GetType() && Equals((RangeContainsExpression)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Range?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (Item?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (int)NodeType;
                hashCode = (hashCode * 397) ^ (Type?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}
