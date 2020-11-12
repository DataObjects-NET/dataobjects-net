using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Model;
using Xtensive.Orm.Rse;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.BulkOperations
{
  internal abstract class QueryOperation<T> : Operation<T>
    where T : class, IEntity
  {
    private readonly static MethodInfo InMethod = GetInMethod();
    protected IQueryable<T> query;

    protected QueryOperation(QueryProvider queryProvider)
      : base(queryProvider)
    {
    }

    private static MethodInfo GetInMethod()
    {
      foreach (var method in typeof(QueryableExtensions).GetMethods().Where(a => string.Equals(a.Name, "In", StringComparison.Ordinal))) {
        var parameters = method.GetParameters();
        if (parameters.Length == 3 && string.Equals(parameters[2].ParameterType.Name, "IEnumerable`1", StringComparison.Ordinal)) {
          return method;
        }
      }
      return null;
    }

    protected override int ExecuteInternal()
    {
      var e = query.Expression.Visit((MethodCallExpression ex) => {
        var methodInfo = ex.Method;
        //rewrite localCollection.Contains(entity.SomeField) -> entity.SomeField.In(localCollection)
        if (methodInfo.DeclaringType == typeof(Enumerable) &&
            string.Equals(methodInfo.Name, "Contains", StringComparison.Ordinal) &&
            ex.Arguments.Count == 2) {
          var localCollection = ex.Arguments[0];//IEnumerable<T>
          var valueToCheck = ex.Arguments[1];
          var genericInMethod = InMethod.MakeGenericMethod(new[] { valueToCheck.Type });
          ex = Expression.Call(genericInMethod, valueToCheck, Expression.Constant(IncludeAlgorithm.ComplexCondition), localCollection);
          methodInfo = ex.Method;
        }

        if (methodInfo.DeclaringType == typeof(QueryableExtensions) &&
            string.Equals(methodInfo.Name, "In", StringComparison.Ordinal) &&
            ex.Arguments.Count > 1) {
          if (ex.Arguments[1].Type == typeof(IncludeAlgorithm)) {
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
            var types = methodInfo.GetParameters().Select(a => a.ParameterType).ToList();
            types.Insert(1, typeof(IncludeAlgorithm));
            ex = Expression.Call(InMethod.MakeGenericMethod(methodInfo.GetGenericArguments()),
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

      if (@select.From is SqlTableRef sqlTableRef) {
        SetStatementTable(statement, sqlTableRef);
        JoinedTableRef = sqlTableRef;
      }
      else {
        sqlTableRef = GetStatementTable(statement);
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
      if (@select.From is SqlTableRef sqlTableRef) {
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

    private void JoinViaIn(SqlStatement statement, SqlSelect @select)
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
          s.Where = s.Where.IsNullReference() ? ex : SqlDml.And(s.Where, ex);
        }
        var existingColumns = s.Columns.ToChainedBuffer();
        s.Columns.Clear();
        var columnToAdd = existingColumns.First(c => c.Name == columnInfo.Name);
        s.Columns.Add(columnToAdd);
        var @in = SqlDml.In(SqlDml.TableColumn(table, columnInfo.Name), s);
        @where = @where.IsNullReference() ? @in : SqlDml.And(@where, @in);
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
        var columnEqualityExperssion = SqlDml.Equals(queryRef.Columns[columnInfo.Name], sqlTableRef.Columns[columnInfo.Name]);
        whereExpression = whereExpression == null
          ? columnEqualityExperssion
          : SqlDml.And(whereExpression, columnEqualityExperssion);
      }
      SetStatementWhere(statement, whereExpression);
    }

    private void JoinViaJoin(SqlStatement statement, SqlSelect @select)
    {
      var indexMapping = PrimaryIndexes[0];
      var left = SqlDml.TableRef(indexMapping.Table);
      var right = SqlDml.QueryRef(@select);
      SqlExpression joinExpression = null;
      for (var i = 0; i < indexMapping.PrimaryIndex.KeyColumns.Count; i++) {
        var binary = (left.Columns[i] == right.Columns[i]);
        if (joinExpression.IsNullReference()) {
          joinExpression = binary;
        }
        else {
          joinExpression &= binary;
        }
      }
      JoinedTableRef = left;
      var joinedTable = left.InnerJoin(right, joinExpression);
      SetStatementFrom(statement, joinedTable);
    }

    #endregion
  }
}