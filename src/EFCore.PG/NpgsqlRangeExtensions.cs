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
        /// Determines whether two ranges are equal.
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
        /// <value>true</value> if the ranges are equal; otherwise, <value>false</value>.
        /// </returns>
        [Pure]
        public static bool Equal<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T>
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether two ranges are not equal.
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
        /// <value>true</value> if the ranges are not equal; otherwise, <value>false</value>.
        /// </returns>
        [Pure]
        public static bool NotEqual<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T>
        {
            return !Equal(a, b);
        }

        /// <summary>
        /// Determines whether one range is less than the other.
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
        /// <value>true</value> if the first range is less than the second; otherwise, <value>false</value>.
        /// </returns>
        [Pure]
        public static bool LessThan<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T>
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether one range is greater than the other.
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
        /// <value>true</value> if the first range is greater than the second; otherwise, <value>false</value>.
        /// </returns>
        [Pure]
        public static bool GreaterThan<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T>
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether one range is less than or equal to the other.
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
        /// <value>true</value> if the first range is less than or equal to the second; otherwise, <value>false</value>.
        /// </returns>
        [Pure]
        public static bool LessThanOrEqual<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T>
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether one range is greater than or eqaul the other.
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
        /// <value>true</value> if the first range is greater than or equal to the second; otherwise, <value>false</value>.
        /// </returns>
        [Pure]
        public static bool GreaterThanOrEqual<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T>
        {
            throw new NotImplementedException();
        }

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
            if (range.IsEmpty)
            {
                return false;
            }

            if (range.LowerBoundInfinite && range.UpperBoundInfinite)
            {
                return true;
            }

            Comparer<T> comparer = Comparer<T>.Default;
            int compareLower = comparer.Compare(value, range.LowerBound);
            int compareUpper = comparer.Compare(value, range.UpperBound);

            bool testLower = compareLower > 0 || compareLower == 0 && range.LowerBoundIsInclusive;
            bool testUpper = compareUpper > 0 || compareUpper == 0 && range.UpperBoundIsInclusive;

            return testLower || testUpper;
        }

        /// <summary>
        /// Determines whether a range contains a specified range.
        /// </summary>
        /// <param name="range">
        /// The range in which to locate the specified range.
        /// </param>
        /// <param name="value">
        /// The specified range to locate in the range.
        /// </param>
        /// <typeparam name="T">
        /// The type of the elements of <paramref name="range"/>.
        /// </typeparam>
        /// <returns>
        /// <value>true</value> if the range contains the specified range; otherwise, <value>false</value>.
        /// </returns>
        [Pure]
        public static bool Contains<T>(this NpgsqlRange<T> range, NpgsqlRange<T> value) where T : IComparable<T>
        {
            if (range.IsEmpty || value.IsEmpty)
            {
                return false;
            }

            if (range.LowerBoundInfinite && range.UpperBoundInfinite || value.LowerBoundInfinite && value.UpperBoundInfinite)
            {
                return true;
            }

            Comparer<T> comparer = Comparer<T>.Default;
            int compareLower = comparer.Compare(value.LowerBound, range.LowerBound);
            int compareUpper = comparer.Compare(value.UpperBound, range.UpperBound);

            bool testLower = compareLower > 0 || compareLower == 0 && range.LowerBoundIsInclusive;
            bool testUpper = compareUpper > 0 || compareUpper == 0 && range.UpperBoundIsInclusive;

            return testLower || testUpper;
        }

        /// <summary>
        /// Determines whether a value is contained by a specified range.
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
        public static bool ContainedBy<T>(this T value, NpgsqlRange<T> range) where T : IComparable<T>
        {
            return range.Contains(value);
        }

        /// <summary>
        /// Determines whether a range is contained by a specified range.
        /// </summary>
        /// <param name="range">
        /// The range in which to locate the specified range.
        /// </param>
        /// <param name="value">
        /// The specified range to locate in the range.
        /// </param>
        /// <typeparam name="T">
        /// The type of the elements of <paramref name="range"/>.
        /// </typeparam>
        /// <returns>
        /// <value>true</value> if the range contains the specified range; otherwise, <value>false</value>.
        /// </returns>
        [Pure]
        public static bool ContainedBy<T>(this NpgsqlRange<T> value, NpgsqlRange<T> range) where T : IComparable<T>
        {
            return range.Contains(value);
        }

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
            if (a.IsEmpty || b.IsEmpty)
            {
                return false;
            }

            if (a.LowerBoundInfinite && a.UpperBoundInfinite || b.LowerBoundInfinite && b.UpperBoundInfinite)
            {
                return true;
            }

            Comparer<T> comparer = Comparer<T>.Default;
            int compareLower = comparer.Compare(a.LowerBound, b.LowerBound);
            int compareUpper = comparer.Compare(a.UpperBound, b.UpperBound);

            bool testLower = compareLower > 0 || compareLower == 0 && b.LowerBoundIsInclusive;
            bool testUpper = compareUpper > 0 || compareUpper == 0 && b.UpperBoundIsInclusive;

            return testLower || testUpper;
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
        public static bool StrictlyLeftOf<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T>
        {
            throw new NotImplementedException();
        }

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
        public static bool StrictlyRightOf<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T>
        {
            throw new NotImplementedException();
        }

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
        public static bool DoesNotExtendToTheLeftOf<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T>
        {
            throw new NotImplementedException();
        }

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
        public static bool DoesNotExtendToTheRightOf<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T>
        {
            throw new NotImplementedException();
        }

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
        public static bool Adjacent<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T>
        {
            throw new NotImplementedException();
        }

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
        public static NpgsqlRange<T> Union<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T>
        {
            throw new NotImplementedException();
        }

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
        public static NpgsqlRange<T> Intersect<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T>
        {
            throw new NotImplementedException();
        }

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
        public static NpgsqlRange<T> Except<T>(this NpgsqlRange<T> a, NpgsqlRange<T> b) where T : IComparable<T>
        {
            throw new NotImplementedException();
        }
    }
}
