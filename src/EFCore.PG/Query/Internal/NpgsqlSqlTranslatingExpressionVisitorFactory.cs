﻿using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal
{
    public class NpgsqlSqlTranslatingExpressionVisitorFactory : IRelationalSqlTranslatingExpressionVisitorFactory
    {
        [NotNull] readonly RelationalSqlTranslatingExpressionVisitorDependencies _dependencies;

        public NpgsqlSqlTranslatingExpressionVisitorFactory(
            [NotNull] RelationalSqlTranslatingExpressionVisitorDependencies dependencies)
            => _dependencies = dependencies;

        public virtual RelationalSqlTranslatingExpressionVisitor Create(
            IModel model,
            QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
            => new NpgsqlSqlTranslatingExpressionVisitor(
                _dependencies,
                model,
                queryableMethodTranslatingExpressionVisitor);
    }
}
