using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
    private static MethodInfo inMethod = GetInMethod();
    protected IQueryable<T> query;

    protected QueryOperation(QueryProvider queryProvider)
      : base(queryProvider)
    {
    }

    private static MethodInfo GetInMethod()
    {
      foreach (var method in typeof (QueryableExtensions).GetMethods().Where(a=>a.Name=="In"))
      {
        var parameters = method.GetParameters();
        if (parameters.Length == 3 && parameters[2].ParameterType.Name == "IEnumerable`1")
          return method;
      }
      return null;
    }

    protected override int ExecuteInternal()
    {
      Expression e = query.Expression.Visit((MethodCallExpression ex) =>
        {
          if (ex.Method.DeclaringType == typeof (QueryableExtensions) && ex.Method.Name == "In" &&
              ex.Arguments.Count > 1)
          {
            if (ex.Arguments[1].Type == typeof (IncludeAlgorithm))
            {
              var v = (IncludeAlgorithm) ex.Arguments[1].Invoke();
              if (v == IncludeAlgorithm.TemporaryTable)
              {
                throw new NotSupportedException("IncludeAlgorithm.TemporaryTable is not supported");
              }
              if (v == IncludeAlgorithm.Auto)
              {
                List<Expression> arguments = ex.Arguments.ToList();
                arguments[1] = Expression.Constant(IncludeAlgorithm.ComplexCondition);
                ex = Expression.Call(ex.Method, arguments);
              }
            }
            else
            {
              List<Expression> arguments = ex.Arguments.ToList();
              arguments.Insert(1, Expression.Constant(IncludeAlgorithm.ComplexCondition));
              List<Type> types = ex.Method.GetParameters().Select(a => a.ParameterType).ToList();
              types.Insert(1, typeof (IncludeAlgorithm));
              ex = Expression.Call(inMethod.MakeGenericMethod(ex.Method.GetGenericArguments()),
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
      if (select.HasLimit)
        JoinWhenQueryHasLimitation(statement, select);
      else
        JoinWhenQueryHasNoLimitation(statement, select);
    }

    protected abstract void SetStatementFrom(SqlStatement statement, SqlTable from);
    protected abstract void SetStatementTable(SqlStatement statement, SqlTableRef table);
    protected abstract void SetStatementWhere(SqlStatement statement, SqlExpression where);
    protected abstract void SetStatementLimit(SqlStatement statement, SqlExpression limit);
    protected abstract bool SupportsJoin();
    protected abstract bool SupportsLimitation();

    private void JoinWhenQueryHasLimitation(SqlStatement statement, SqlSelect select)
    {
      if (!SupportsLimitation() && !SupportsJoin())
        throw new NotSupportedException("This provider does not supported limitation of affected rows.");

      var sqlTableRef = @select.From as SqlTableRef;
      if (sqlTableRef!=null) {
        SetStatementTable(statement, sqlTableRef);
        JoinedTableRef = sqlTableRef;
      }
      else
        sqlTableRef = GetStatementTable(statement);
      if (SupportsLimitation()) {
        SetStatementLimit(statement, select.Limit);
        SetStatementWhere(statement, select.Where);
      }
      else
        JoinViaFrom(statement, select);
      
      
    }
    private void JoinWhenQueryHasNoLimitation(SqlStatement statement, SqlSelect select)
    {
      var sqlTableRef = @select.From as SqlTableRef;
      if (sqlTableRef!=null) {
        SetStatementTable(statement, sqlTableRef);
        SetStatementWhere(statement, select.Where);
        JoinedTableRef = sqlTableRef;
        return;
      }

      if (SupportsJoin())
        JoinViaFrom(statement, select);
      else
        JoinViaIn(statement, select);
    }

    private void JoinViaIn(SqlStatement statement, SqlSelect @select)
    {
      SqlTableRef table = GetStatementTable(statement);
      SqlExpression where = GetStatementWhere(statement);
      JoinedTableRef = table;
      PrimaryIndexMapping indexMapping = PrimaryIndexes[0];
      var columns = new List<ColumnInfo>();
      foreach (ColumnInfo columnInfo in indexMapping.PrimaryIndex.KeyColumns.Keys)
      {
        SqlSelect s = select.ShallowClone();
        foreach (ColumnInfo column in columns)
        {
          SqlBinary ex = SqlDml.Equals(SqlDml.TableColumn(s.From, column.Name), SqlDml.TableColumn(table, column.Name));
          s.Where = s.Where.IsNullReference() ? ex : SqlDml.And(s.Where, ex);
        }
        s.Columns.Clear();
        s.Columns.Add(SqlDml.TableColumn(s.From, columnInfo.Name));
        SqlBinary @in = SqlDml.In(SqlDml.TableColumn(table, columnInfo.Name), s);
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
      PrimaryIndexMapping indexMapping = PrimaryIndexes[0];
      foreach (ColumnInfo columnInfo in indexMapping.PrimaryIndex.KeyColumns.Keys) {
        var leftColumn = queryRef.Columns[columnInfo.Name];
        var rightColumn = sqlTableRef==null ? GetStatementTable(statement).Columns[columnInfo.Name] : sqlTableRef.Columns[columnInfo.Name];
        if (leftColumn==null || rightColumn==null)
          throw new InvalidOperationException("Source query doesn't contain one of key columns of updated table.");
        var columnEqualityExperssion = SqlDml.Equals(queryRef.Columns[columnInfo.Name], sqlTableRef.Columns[columnInfo.Name]);
        if (whereExpression==null)
          whereExpression = columnEqualityExperssion;
        else
          whereExpression = SqlDml.And(whereExpression, columnEqualityExperssion);
      }
      SetStatementWhere(statement, whereExpression);
    }

    private void JoinViaJoin(SqlStatement statement, SqlSelect @select)
    {
      PrimaryIndexMapping indexMapping = PrimaryIndexes[0];
      SqlTableRef left = SqlDml.TableRef(indexMapping.Table);
      SqlQueryRef right = SqlDml.QueryRef(@select);
      SqlExpression joinExpression = null;
      for (int i = 0; i < indexMapping.PrimaryIndex.KeyColumns.Count; i++)
      {
        SqlBinary binary = (left.Columns[i] == right.Columns[i]);
        if (joinExpression.IsNullReference())
          joinExpression = binary;
        else
          joinExpression &= binary;
      }
      JoinedTableRef = left;
      SqlJoinedTable joinedTable = left.InnerJoin(right, joinExpression);
      SetStatementFrom(statement, joinedTable);
    }

    #endregion
  }
}