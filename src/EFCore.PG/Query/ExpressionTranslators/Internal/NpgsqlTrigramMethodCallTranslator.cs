using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Npgsql.EntityFrameworkCore.PostgreSQL.Extensions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    public class NpgsqlTrigramMethodCallTranslator : IMethodCallTranslator
    {
        public Expression Translate(MethodCallExpression expression, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (expression.Method.DeclaringType != typeof(NpgsqlTrigramExtensions))
                return null;

            switch (expression.Method.Name)
            {
            case nameof(NpgsqlTrigramExtensions.FuzzyMatches):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "%>", typeof(bool));

            case nameof(NpgsqlTrigramExtensions.WordSimilarity):
                return new CustomBinaryExpression(expression.Arguments[1], expression.Arguments[2], "<->>", typeof(double));

            default:
                return null;
            }
        }
    }
}
