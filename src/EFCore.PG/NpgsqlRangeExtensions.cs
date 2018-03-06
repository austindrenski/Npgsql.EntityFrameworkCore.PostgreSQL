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
using System.Diagnostics.Contracts;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Provides extension methods for <see cref="NpgsqlRange{T}"/> supporting PostgreSQL translation.
    /// </summary>
    public static class NpgsqlRangeExtensions
    {
        /// <summary>
        /// Determines whether a range contains a specified value.
        /// </summary>
        /// <param name="range">
        /// The range in which to locate the value.
        /// </param>
        /// <param name="value">
        /// The value to locate in the range.
        /// </param>
        /// <typeparam name="T">
        /// The type of the elements of <paramref name="range"/>.
        /// </typeparam>
        /// <returns>
        /// <value>true</value> if the range contains the specified value; otherwise, <value>false</value>.
        /// </returns>
        [Pure]
        public static bool Contains<T>(this NpgsqlRange<T> range, T value) where T : IComparable<T>
        {
            // Empty ranges contain no elements.
            if (range.IsEmpty)
                return false;

            // The infinite range contains all possible elements.
            if (range.LowerBoundInfinite && range.UpperBoundInfinite)
                return true;

            Comparer<T> comparer = Comparer<T>.Default;
            int compareLower = comparer.Compare(value, range.LowerBound);
            int compareUpper = comparer.Compare(value, range.UpperBound);

            // The lower bound is valid given one of the following:
            //   1. The lower bound of the range is infinite.
            //   2. The value is strictly greater than the lower bound of the range.
            //   3. The value is at the lower bound and the lower bound is inclusive.
            bool lower = range.LowerBoundInfinite || compareLower > 0 || (compareLower == 0 && range.LowerBoundIsInclusive);

            // The upper bound is valid given one of the following:
            //   1. The upper bound of the range is infinite.
            //   2. The value is strictly less than the upper bound of the range.
            //   3. The value is at the upper bound and the upper bound is inclusive.
            bool upper = range.UpperBoundInfinite || compareUpper < 0 || (compareUpper == 0 && range.UpperBoundIsInclusive);

            return lower && upper;
        }

        /// <summary>
        /// Determines whether a range contains a specified range.
        /// </summary>
        /// <param name="a">
        /// The range in which to locate the specified range.
        /// </param>
        /// <param name="b">
        /// The specified range to locate in the range.
        /// </param>
        /// <typeparam name="T">
        /// The type of the elements of <paramref name="a"/>.
        /// </typeparam>
        /// <returns>
        /// <value>true</value> if the range contains the specified range; otherwise, <value>false</value>.
        /// </returns>
        [Pure]
        public static bool Contains<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T>
        {
            // TODO: Postgres handles integral ranges s.t. the following query returns TRUE:
            //
            //   SELECT '[1,10)'::int4range @> '(0,5)'::int4range;
            //
            // Questions:
            //   - How to match this behavior?
            //   - What does this mean for floating point numbers?
            //   - Special handling for integrals, or generically applied to all?

            // Ranges have containment identity, s.t. A @> A.
            //   - SELECT '(,)'::int4range @> '(,)'::int4range;
            //   - SELECT '(0,0)'::int4range @> '(0,0)'::int4range;
            //   - SELECT '(0,1)'::int4range @> '(0,1)'::int4range;
            //   - SELECT '[0,1)'::int4range @> '[0,1)'::int4range;
            if (a.Flags == b.Flags && a.LowerBound.Equals(b.LowerBound) && a.UpperBound.Equals(b.UpperBound))
                return true;

            // All range contain the empty range, including other empty ranges.
            //   - SELECT '(0,0)'::int4range @> '(0,0)'::int4range;
            //   - SELECT '(0,1)'::int4range @> '(0,0)'::int4range;
            if (b.IsEmpty)
                return true;

            // A contains all possible forms of B, including infinite forms.
            //   - SELECT '(,)'::int4range @> '(0,0)'::int4range;
            //   - SELECT '(,)'::int4range @> '(,)'::int4range;
            if (a.LowerBoundInfinite && a.UpperBoundInfinite)
                return true;

            Comparer<T> comparer = Comparer<T>.Default;
            int compareLower = comparer.Compare(b.LowerBound, a.LowerBound);
            int compareUpper = comparer.Compare(b.UpperBound, a.UpperBound);

            // The lower bound is valid given one of the following:
            //   1. The lower bound of A is infinite.
            //     - SELECT '(,1)'::int4range @> '(0,1)'::int4range;
            //   2. The lower bound of B is not infinite and the lower bound of B is strictly greater than the lower bound of A.
            //     - SELECT '(-1,1)'::int4range @> '(0,1)'::int4range;
            //   3. The lower bounds are equal and either the lower bound of A is inclusive or the lower bound of B is not inclusive.
            //     - SELECT '[0,1)'::int4range @> '(0,1)'::int4range;
            //     - SELECT '[0,1)'::int4range @> '[0,1)'::int4range;
            //     - SELECT '(0,1)'::int4range @> '(0,1)'::int4range;
            bool lower = a.LowerBoundInfinite || (!b.LowerBoundInfinite && compareLower > 0) || (compareLower == 0 && !b.LowerBoundInfinite && (a.LowerBoundIsInclusive || !b.LowerBoundIsInclusive));

            // The upper bound is valid given one of the following:
            //   1. The upper bound of A is infinite.
            //     - SELECT '(0,)'::int4range @> '(0,1)'::int4range;
            //   2. The upper bound of B is not infinite and the upper bound of B is strictly less than the upper bound of A.
            //     - SELECT '(-1,1)'::int4range @> '(-1,0)'::int4range;
            //   3. The upper bounds are equal and either the upper bound of A is inclusive or the upper bound of B is not inclusive.
            //     - SELECT '(0,1]'::int4range @> '(0,1)'::int4range;
            //     - SELECT '(0,1]'::int4range @> '(0,1]'::int4range;
            //     - SELECT '(0,1)'::int4range @> '(0,1)'::int4range;
            bool upper = a.UpperBoundInfinite || (!b.UpperBoundInfinite && compareUpper < 0) || (compareUpper == 0 && !b.UpperBoundInfinite && (a.UpperBoundIsInclusive || !b.UpperBoundIsInclusive));

            return lower && upper;
        }

        /// <summary>
        /// Determines whether a range is contained by a specified range.
        /// </summary>
        /// <param name="a">
        /// The specified range to locate in the range.
        /// </param>
        /// <param name="b">
        /// The range in which to locate the specified range.
        /// </param>
        /// <typeparam name="T">
        /// The type of the elements of <paramref name="a"/>.
        /// </typeparam>
        /// <returns>
        /// <value>true</value> if the range contains the specified range; otherwise, <value>false</value>.
        /// </returns>
        [Pure]
        public static bool ContainedBy<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T> => b.Contains(a);

        /// <summary>
        /// Determines whether a range overlaps another range.
        /// </summary>
        /// <param name="a">
        /// The first range.
        /// </param>
        /// <param name="b">
        /// The second range.
        /// </param>
        /// <typeparam name="T">
        /// The type of the elements of <paramref name="a"/>.
        /// </typeparam>
        /// <returns>
        /// <value>true</value> if the ranges overlap (share points in common); otherwise, <value>false</value>.
        /// </returns>
        [Pure]
        public static bool Overlaps<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T>
        {
            if (a.Flags == b.Flags && a.LowerBound.Equals(b.LowerBound) && a.UpperBound.Equals(b.UpperBound))
                return true;
            if (a.IsEmpty || b.IsEmpty)
                return false;
            if (a.LowerBoundInfinite && b.LowerBoundInfinite || a.UpperBoundInfinite && b.UpperBoundInfinite)
                return true;

            return
                a.Contains(b.LowerBound) ||
                a.Contains(b.UpperBound) ||
                b.Contains(a.LowerBound) ||
                b.Contains(a.UpperBound);
        }

        /// <summary>
        /// Determines whether a range is strictly to the left of another range.
        /// </summary>
        /// <param name="a">
        /// The first range.
        /// </param>
        /// <param name="b">
        /// The second range.
        /// </param>
        /// <typeparam name="T">
        /// The type of the elements of <paramref name="a"/>.
        /// </typeparam>
        /// <returns>
        /// <value>true</value> if the first range is strictly to the left of the second; otherwise, <value>false</value>.
        /// </returns>
        [Pure]
        public static bool IsStrictlyLeftOf<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T> => throw new NotImplementedException();

        /// <summary>
        /// Determines whether a range is strictly to the right of another range.
        /// </summary>
        /// <param name="a">
        /// The first range.
        /// </param>
        /// <param name="b">
        /// The second range.
        /// </param>
        /// <typeparam name="T">
        /// The type of the elements of <paramref name="a"/>.
        /// </typeparam>
        /// <returns>
        /// <value>true</value> if the first range is strictly to the right of the second; otherwise, <value>false</value>.
        /// </returns>
        [Pure]
        public static bool IsStrictlyRightOf<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T> => throw new NotImplementedException();

        /// <summary>
        /// Determines whether a range does not extend to the left of another range.
        /// </summary>
        /// <param name="a">
        /// The first range.
        /// </param>
        /// <param name="b">
        /// The second range.
        /// </param>
        /// <typeparam name="T">
        /// The type of the elements of <paramref name="a"/>.
        /// </typeparam>
        /// <returns>
        /// <value>true</value> if the first range does not extend to the left of the second; otherwise, <value>false</value>.
        /// </returns>
        [Pure]
        public static bool DoesNotExtendLeftOf<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T> => throw new NotImplementedException();

        /// <summary>
        /// Determines whether a range does not extend to the right of another range.
        /// </summary>
        /// <param name="a">
        /// The first range.
        /// </param>
        /// <param name="b">
        /// The second range.
        /// </param>
        /// <typeparam name="T">
        /// The type of the elements of <paramref name="a"/>.
        /// </typeparam>
        /// <returns>
        /// <value>true</value> if the first range does not extend to the right of the second; otherwise, <value>false</value>.
        /// </returns>
        [Pure]
        public static bool DoesNotExtendRightOf<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T> => throw new NotImplementedException();

        /// <summary>
        /// Determines whether a range is adjacent to another range.
        /// </summary>
        /// <param name="a">
        /// The first range.
        /// </param>
        /// <param name="b">
        /// The second range.
        /// </param>
        /// <typeparam name="T">
        /// The type of the elements of <paramref name="a"/>.
        /// </typeparam>
        /// <returns>
        /// <value>true</value> if the ranges are adjacent; otherwise, <value>false</value>.
        /// </returns>
        [Pure]
        public static bool IsAdjacentTo<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T> => throw new NotImplementedException();

        /// <summary>
        /// Returns the set union, which means unique elements that appear in either of two ranges.
        /// </summary>
        /// <param name="a">
        /// The first range.
        /// </param>
        /// <param name="b">
        /// The second range.
        /// </param>
        /// <typeparam name="T">
        /// The type of the elements of <paramref name="a"/>.
        /// </typeparam>
        /// <returns>
        /// The unique elements that appear in either range.
        /// </returns>
        [Pure]
        public static NpgsqlRange<T> Union<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T> => throw new NotImplementedException();

        /// <summary>
        /// Returns the set intersection, which means elements that appear in each of two ranges.
        /// </summary>
        /// <param name="a">
        /// The first range.
        /// </param>
        /// <param name="b">
        /// The second range.
        /// </param>
        /// <typeparam name="T">
        /// The type of the elements of <paramref name="a"/>.
        /// </typeparam>
        /// <returns>
        /// The elements that appear in both ranges.
        /// </returns>
        [Pure]
        public static NpgsqlRange<T> Intersect<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T> => throw new NotImplementedException();

        /// <summary>
        /// Returns the set difference, which means the elements of one range that do not appear in a second range.
        /// </summary>
        /// <param name="a">
        /// The first range.
        /// </param>
        /// <param name="b">
        /// The second range.
        /// </param>
        /// <typeparam name="T">
        /// The type of the elements of <paramref name="a"/>.
        /// </typeparam>
        /// <returns>
        /// The elements that appear in the first range, but not the second range.
        /// </returns>
        [Pure]
        public static NpgsqlRange<T> Except<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T> => throw new NotImplementedException();
    }
}
