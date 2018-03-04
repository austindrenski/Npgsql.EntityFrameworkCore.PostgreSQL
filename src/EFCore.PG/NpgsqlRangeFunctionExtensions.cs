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
    public static class NpgsqlRangeFunctionExtensions
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

            if (range.LowerBoundInfinite && range.UpperBoundInfinite || value.LowerBoundInfinite && range.UpperBoundInfinite)
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
    }
}
