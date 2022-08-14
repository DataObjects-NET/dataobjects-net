// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Model;
using Xtensive.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.BulkOperations
{
  internal abstract class QueryOperation<T> : Operation<T>
    where T : class, IEntity
  {
    protected IQueryable<T> query;

    protected QueryOperation(QueryProvider queryProvider)
      : base(queryProvider)
    {
    }

    protected override int ExecuteInternal()
    {
      var e = query.Expression.Visit((MethodCallExpression ex) => {

          var methodInfo = ex.Method;
          //rewrite localCollection.Contains(entity.SomeField) -> entity.SomeField.In(localCollection)
          if (methodInfo.DeclaringType == WellKnownMembers.EnumerableType &&
              string.Equals(methodInfo.Name, "Contains", StringComparison.Ordinal) &&
              ex.Arguments.Count == 2) {
            var localCollection = ex.Arguments[0];//IEnumerable<T>
            var valueToCheck = ex.Arguments[1];
            var genericInMethod = WellKnownMembers.InMethod.CachedMakeGenericMethod(valueToCheck.Type);
            ex = Expression.Call(genericInMethod, valueToCheck, Expression.Constant(IncludeAlgorithm.ComplexCondition), localCollection);
            methodInfo = ex.Method;
          }

          if (methodInfo.DeclaringType == WellKnownMembers.QueryableExtensionsType &&
              string.Equals(methodInfo.Name, WellKnownMembers.InMethodName, StringComparison.Ordinal) &&
              ex.Arguments.Count > 1) {
            if (ex.Arguments[1].Type == WellKnownMembers.IncludeAlgorithmType) {
              var algorithm = (IncludeAlgorithm) ex.Arguments[1].Invoke();
              if (algorithm == IncludeAlgorithm.TemporaryTable) {
                throw new NotSupportedException("IncludeAlgorithm.TemporaryTable is not supported");
              }

              if (algorithm == IncludeAlgorithm.Auto) {
                var arguments = ex.Arguments.ToList();
                arguments[1] = Expression.Constant(IncludeAlgorithm.ComplexCondition);
                ex = Expression.Call(methodInfo, arguments);
              }
            }
            else {
              var arguments = ex.Arguments.ToList();
              arguments.Insert(1, Expression.Constant(IncludeAlgorithm.ComplexCondition));
              ex = Expression.Call(WellKnownMembers.InMethod.MakeGenericMethod(methodInfo.GetGenericArguments()),
                arguments.ToArray());
            }
          }

          return ex;
        });
      query = QueryProvider.CreateQuery<T>(e);
      return 0;
    }

    #region Non-public methods

    protected abstract SqlTableRef GetStatementTable(SqlStatement statement);
    protected abstract SqlExpression GetStatementWhere(SqlStatement statement);

    protected void Join(SqlQueryStatement statement, SqlSelect select)
    {
      if (select.HasLimit) {
        JoinWhenQueryHasLimitation(statement, select);
      }
      else {
        JoinWhenQueryHasNoLimitation(statement, select);
      }
    }

    protected abstract void SetStatementFrom(SqlStatement statement, SqlTable from);
    protected abstract void SetStatementTable(SqlStatement statement, SqlTableRef table);
    protected abstract void SetStatementWhere(SqlStatement statement, SqlExpression where);
    protected abstract void SetStatementLimit(SqlStatement statement, SqlExpression limit);
    protected abstract bool SupportsJoin();
    protected abstract bool SupportsLimitation();

    private void JoinWhenQueryHasLimitation(SqlStatement statement, SqlSelect select)
    {
      if (!SupportsLimitation() && !SupportsJoin()) {
        throw new NotSupportedException("This provider does not supported limitation of affected rows.");
      }

      if (select.From is SqlTableRef sqlTableRef) {
        SetStatementTable(statement, sqlTableRef);
        JoinedTableRef = sqlTableRef;
      }
      else {
        _ = GetStatementTable(statement);
      }

      if (SupportsLimitation()) {
        SetStatementLimit(statement, select.Limit);
        SetStatementWhere(statement, select.Where);
      }
      else {
        JoinViaFrom(statement, select);
      }
    }

    private void JoinWhenQueryHasNoLimitation(SqlStatement statement, SqlSelect select)
    {
      if (select.From is SqlTableRef sqlTableRef) {
        SetStatementTable(statement, sqlTableRef);
        SetStatementWhere(statement, select.Where);
        JoinedTableRef = sqlTableRef;
        return;
      }
      if (SupportsJoin()) {
        JoinViaFrom(statement, select);
      }
      else {
        JoinViaIn(statement, select);
      }
    }

    private void JoinViaIn(SqlStatement statement, SqlSelect select)
    {
      var table = GetStatementTable(statement);
      var where = GetStatementWhere(statement);
      JoinedTableRef = table;
      var indexMapping = PrimaryIndexes[0];
      var columns = new List<ColumnInfo>();
      foreach (var columnInfo in indexMapping.PrimaryIndex.KeyColumns.Keys) {
        var s = (SqlSelect) select.Clone();
        foreach (var column in columns) {
          var ex = SqlDml.Equals(s.From.Columns[column.Name], table.Columns[column.Name]);
          s.Where = s.Where is null ? ex : SqlDml.And(s.Where, ex);
        }
        var existingColumns = s.Columns.ToChainedBuffer();
        s.Columns.Clear();
        var columnToAdd = existingColumns.First(c => c.Name.Equals(columnInfo.Name, StringComparison.Ordinal));
        s.Columns.Add(columnToAdd);
        var @in = SqlDml.In(SqlDml.TableColumn(table, columnInfo.Name), s);
        where = where is null ? @in : SqlDml.And(where, @in);
        columns.Add(columnInfo);
      }

      SetStatementWhere(statement, where);
    }


    private void JoinViaFrom(SqlStatement statement, SqlSelect select)
    {
      var queryRef = SqlDml.QueryRef(select, "source");
      SetStatementFrom(statement, queryRef);
      var sqlTableRef = GetStatementTable(statement);
      SqlExpression whereExpression = null;
      var indexMapping = PrimaryIndexes[0];
      foreach (var columnInfo in indexMapping.PrimaryIndex.KeyColumns.Keys) {
        var leftColumn = queryRef.Columns[columnInfo.Name];
        var rightColumn = sqlTableRef == null
          ? GetStatementTable(statement).Columns[columnInfo.Name]
          : sqlTableRef.Columns[columnInfo.Name];
        if (leftColumn == null || rightColumn == null) {
          throw new InvalidOperationException("Source query doesn't contain one of key columns of updated table.");
        }

        var columnEqualityExpression =
          SqlDml.Equals(queryRef.Columns[columnInfo.Name], sqlTableRef.Columns[columnInfo.Name]);
        whereExpression = whereExpression == null
          ? columnEqualityExpression
          : SqlDml.And(whereExpression, columnEqualityExpression);
      }

      SetStatementWhere(statement, whereExpression);
    }

    #endregion
  }
}
