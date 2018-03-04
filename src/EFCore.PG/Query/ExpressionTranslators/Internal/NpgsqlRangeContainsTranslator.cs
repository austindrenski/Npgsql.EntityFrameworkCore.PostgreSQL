using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Translates a range containment method call.
    /// </summary>
    public class NpgsqlRangeContainsTranslator : IMethodCallTranslator
    {
        /// <summary>
        /// Caches runtime method information.
        /// </summary>
        private static readonly MethodInfo ContainsMethodInfo;

        /// <summary>
        /// Initializes static resources.
        /// </summary>
        static NpgsqlRangeContainsTranslator()
        {
            ContainsMethodInfo =
                typeof(NpgsqlDbFunctionsExtensions).GetMethod(nameof(NpgsqlDbFunctionsExtensions.Contains));
        }

        /// <inheritdoc />
        public Expression Translate(MethodCallExpression methodCallExpression)
        {
            return
                methodCallExpression.Method == ContainsMethodInfo
                    ? new RangeContainsExpression(methodCallExpression.Arguments[0], methodCallExpression.Arguments[1])
                    : null;
        }
    }
}
