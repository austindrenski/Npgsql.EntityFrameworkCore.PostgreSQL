using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Extensions
{
    public static class NpgsqlTrigramExtensions
    {
        public static bool FuzzyMatches(this DbFunctions _, string value, string search) => throw ClientEvaluationNotSupportedException();

        public static double WordSimilarity(this DbFunctions _, string value, string search) => throw ClientEvaluationNotSupportedException();

        #region Utilities

        /// <summary>
        /// Helper method to throw a <see cref="NotSupportedException"/> with the name of the throwing method.
        /// </summary>
        /// <param name="method">The method that throws the exception.</param>
        /// <returns>
        /// A <see cref="NotSupportedException"/>.
        /// </returns>
        [NotNull]
        static NotSupportedException ClientEvaluationNotSupportedException([CallerMemberName] string method = default)
            => new NotSupportedException($"{method} is only intended for use via SQL translation as part of an EF Core LINQ query.");

        #endregion
    }
}
