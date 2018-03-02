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
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using NpgsqlTypes;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class NpgsqlRangeFunctionTranslator : IMethodCallTranslator
    {
        static readonly MethodInfo _containsInt4
            = typeof(NpgsqlRangeFunctionExtensions).GetRuntimeMethod(
                nameof(NpgsqlRangeFunctionExtensions.Contains),
                new[] { typeof(int) });

        static readonly MethodInfo _containsInt8
            = typeof(NpgsqlRangeFunctionExtensions).GetRuntimeMethod(
                nameof(NpgsqlRangeFunctionExtensions.Contains),
                new[] { typeof(long) });

        static readonly MethodInfo _containsDate
            = typeof(NpgsqlRangeFunctionExtensions).GetRuntimeMethod(
                nameof(NpgsqlRangeFunctionExtensions.Contains),
                new[] { typeof(DateTime) });

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.Equals(_containsInt4))
            {
                return
                    new SqlFunctionExpression("STRPOS", typeof(int), new[]
                    {
                        methodCallExpression.Object,
                        methodCallExpression.Arguments[0]
                    });
            }

            if (methodCallExpression.Method.Equals(_containsInt8))
            {
                return
                    new SqlFunctionExpression("STRPOS", typeof(int), new[]
                    {
                        methodCallExpression.Object,
                        methodCallExpression.Arguments[0]
                    });
            }

            if (methodCallExpression.Method.Equals(_containsDate))
            {
                return
                    new SqlFunctionExpression("STRPOS", typeof(int), new[]
                    {
                        methodCallExpression.Object,
                        methodCallExpression.Arguments[0]
                    });
            }

            return methodCallExpression.Method.Equals(_methodInfo)
                ? Expression.GreaterThan(
                    new SqlFunctionExpression("STRPOS", typeof(int), new[]
                    {
                        methodCallExpression.Object,
                        methodCallExpression.Arguments[0]
                    }), Expression.Constant(0))
                : null;
        }
    }
}
