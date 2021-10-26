// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.11.13

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Transformation;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Tuples;

namespace Xtensive.Orm.Providers
{
  partial class SqlCompiler 
  {
    protected SqlProvider CreateProvider(SqlSelect statement,
      CompilableProvider origin, params ExecutableProvider[] sources) =>
      CreateProvider(statement, (IEnumerable<QueryParameterBinding>) null, origin, sources);

    protected SqlProvider CreateProvider(SqlSelect statement, QueryParameterBinding extraBinding,
      CompilableProvider origin, params ExecutableProvider[] sources)
    {
      var extraBindings = extraBinding!=null ? Enumerable.Repeat(extraBinding, 1) : null;
      return CreateProvider(statement, extraBindings, origin, sources);
    }

    protected SqlProvider CreateProvider(SqlSelect statement, IEnumerable<QueryParameterBinding> extraBindings,
      CompilableProvider origin, params ExecutableProvider[] sources)
    {
      var allowBatching = true;
      var parameterBindings = extraBindings ?? Enumerable.Empty<QueryParameterBinding>();
      foreach (var provider in sources.OfType<SqlProvider>()) {
        var queryRequest = provider.Request;
        allowBatching &= queryRequest.CheckOptions(QueryRequestOptions.AllowOptimization);
        parameterBindings = parameterBindings.Concat(queryRequest.ParameterBindings);
      }

      var tupleDescriptor = origin.Header.TupleDescriptor;

      var options = QueryRequestOptions.Empty;
      if (allowBatching) {
        options |= QueryRequestOptions.AllowOptimization;
      }

      if (statement.Columns.Count < origin.Header.TupleDescriptor.Count) {
        tupleDescriptor = origin.Header.TupleDescriptor.Head(statement.Columns.Count);
      }

      var request = CreateQueryRequest(Driver, statement, parameterBindings, tupleDescriptor, options);

      return new SqlProvider(Handlers, request, origin, sources);
    }

    protected virtual string ProcessAliasedName(string name) => name;

    protected Pair<SqlExpression, IEnumerable<QueryParameterBinding>> ProcessExpression(LambdaExpression le,
      params IReadOnlyList<SqlExpression>[] sourceColumns)
    {
      var processor = new ExpressionProcessor(le, Handlers, this, sourceColumns);
      var result = new Pair<SqlExpression, IEnumerable<QueryParameterBinding>>(
        processor.Translate(), processor.GetBindings());
      return result;
    }

    protected static SqlSelect ExtractSqlSelect(CompilableProvider origin, SqlProvider compiledSource)
    {
      var sourceSelect = compiledSource.Request.Statement;
      if (ShouldUseQueryReference(origin, compiledSource)) {
        var queryRef = compiledSource.PermanentReference;
        var query = SqlDml.Select(queryRef);
        query.Columns.AddRange(queryRef.Columns);
        query.Comment = sourceSelect.Comment;
        return query;
      }
      return sourceSelect.ShallowClone();
    }

    protected List<SqlExpression> ExtractColumnExpressions(SqlSelect query)
    {
      var result = new List<SqlExpression>(query.Columns.Count);
      result.AddRange(query.Columns.Select(ExtractColumnExpression));
      return result;
    }

    protected SqlExpression ExtractColumnExpression(SqlColumn column)
    {
      SqlExpression expression;
      if (IsColumnStub(column)) {
        expression = stubColumnMap[ExtractColumnStub(column)];
        if (expression is SqlSubQuery subQuery && subQuery.Query is SqlSelect subSelect && subSelect.From == null) {
          if (subSelect.Columns.Count == 1 && subSelect.Columns[0] is SqlUserColumn userColumn) {
            if (userColumn.Expression is SqlCast cast && cast.Type.Type == SqlType.Boolean) {
              if (cast.Operand is SqlCase sqlCase && sqlCase.Count == 1) {
                var pair = sqlCase.First();
                if (pair.Key is SqlUnary && pair.Value is SqlLiteral<int>) {
                  expression = cast;
                }
              }
            }
          }
        }
      }
      else {
        expression = column;
      }

      if (expression is SqlColumnRef columnRef) {
        expression = columnRef.SqlColumn;
      }

      return expression;
    }

    protected void AddInlinableColumn(IInlinableProvider provider, Column column,
      SqlSelect resultQuery, SqlExpression columnExpression)
    {
      var columnName = ProcessAliasedName(column.Name);
      var columnRef = SqlDml.ColumnRef(SqlDml.Column(columnExpression), columnName);
      if (provider.IsInlined && !rootColumns.Contains(column.Origin)) {
        var columnStub = SqlDml.ColumnStub(columnRef);
        stubColumnMap.Add(columnStub, columnExpression);
        resultQuery.Columns.Add(columnStub);
      }
      else {
        resultQuery.Columns.Add(columnRef);
      }
    }

    protected SqlExpression GetBooleanColumnExpression(SqlExpression originalExpression) =>
      providerInfo.Supports(ProviderFeatures.FullFeaturedBooleanExpressions)
        ? originalExpression
        : booleanExpressionConverter.BooleanToInt(originalExpression);

    protected QueryRequest CreateQueryRequest(StorageDriver driver, SqlSelect statement,
      IEnumerable<QueryParameterBinding> parameterBindings,
      TupleDescriptor tupleDescriptor, QueryRequestOptions options)
    {
      if (Handlers.Domain.Configuration.ShareStorageSchemaOverNodes) {
        return new QueryRequest(driver, statement, parameterBindings, tupleDescriptor, options, NodeConfiguration);
      }

      return new QueryRequest(driver, statement, parameterBindings, tupleDescriptor, options);
    }

    private static bool IsCalculatedColumn(SqlColumn column)
    {
      if (column is SqlUserColumn) {
        return true;
      }

      return column is SqlColumnRef columnRef && columnRef.SqlColumn is SqlUserColumn;
    }

    private static bool IsColumnStub(SqlColumn column)
    {
      if (column is SqlColumnStub) {
        return true;
      }

      return column is SqlColumnRef columnRef && columnRef.SqlColumn is SqlColumnStub;
    }

    private static bool IsTypeIdColumn(SqlColumn column) =>
      column switch {
        SqlUserColumn _ => string.Equals(column.Name, "TypeId", StringComparison.OrdinalIgnoreCase),
        SqlColumnRef cRef => string.Equals(cRef.Name, "TypeId", StringComparison.OrdinalIgnoreCase),
        _ => false
      };

    private static SqlColumnStub ExtractColumnStub(SqlColumn column) =>
      column switch {
        SqlColumnRef columnRef => (SqlColumnStub) columnRef.SqlColumn,
        _ => (SqlColumnStub) column
      };

    private static SqlUserColumn ExtractUserColumn(SqlColumn column) =>
      column switch {
        SqlColumnRef columnRef => (SqlUserColumn) columnRef.SqlColumn,
        _ => (SqlUserColumn) column
      };

    private static bool ShouldUseQueryReference(CompilableProvider origin, SqlProvider compiledSource)
    {
      var sourceSelect = compiledSource.Request.Statement;
      if (sourceSelect.From == null) {
        return false;
      }

      var columnIndex = 0;
      var rowNumberIsUsed = false;
      var calculatedColumnIndexes = new List<int>(8);
      foreach (var column in sourceSelect.Columns) {
        if (IsCalculatedColumn(column)) {
          calculatedColumnIndexes.Add(columnIndex);
          rowNumberIsUsed = rowNumberIsUsed || ExtractUserColumn(column).Expression is SqlRowNumber;
        }

        columnIndex++;
      }

      var containsCalculatedColumns = calculatedColumnIndexes.Count > 0;
      var pagingIsUsed = rowNumberIsUsed
        || !sourceSelect.Limit.IsNullReference() || !sourceSelect.Offset.IsNullReference();
      var groupByIsUsed = sourceSelect.GroupBy.Count > 0;
      var distinctIsUsed = sourceSelect.Distinct;
      var filterIsUsed = !sourceSelect.Where.IsNullReference();

      switch (origin.Type) {
        case ProviderType.Filter: {
          var filterProvider = (FilterProvider) origin;
          var usedColumnIndexes = new TupleAccessGatherer().Gather(filterProvider.Predicate.Body);
          return pagingIsUsed || usedColumnIndexes.Any(calculatedColumnIndexes.Contains);
        }
        case ProviderType.Select:
          return distinctIsUsed;
        case ProviderType.RowNumber: {
          var usedColumnIndexes = origin.Header.Order.Select(o => o.Key);
          return pagingIsUsed || groupByIsUsed || distinctIsUsed
            || usedColumnIndexes.Any(calculatedColumnIndexes.Contains);
        }
        case ProviderType.Calculate: {
          var calculateProvider = (CalculateProvider) origin;
          var columnGatherer = new TupleAccessGatherer();
          var usedColumnIndexes = new List<int>();
          foreach (var column in calculateProvider.CalculatedColumns) {
            usedColumnIndexes.AddRange(
              columnGatherer.Gather(column.Expression.Body, column.Expression.Parameters[0]));
          }

          return usedColumnIndexes.Any(calculatedColumnIndexes.Contains);
        }
        case ProviderType.Aggregate: {
          var aggregateProvider = (AggregateProvider) origin;
          var usedColumnIndexes = (aggregateProvider.AggregateColumns ?? Enumerable.Empty<AggregateColumn>())
            .Select(ac => ac.SourceIndex)
            .Concat(aggregateProvider.GroupColumnIndexes);

          return pagingIsUsed || distinctIsUsed || groupByIsUsed
            || usedColumnIndexes.Any(calculatedColumnIndexes.Contains);
        }
        case ProviderType.Take:
        case ProviderType.Skip:
        case ProviderType.Paging: {
          return distinctIsUsed || pagingIsUsed || groupByIsUsed
            || (origin.Sources[0] is SortProvider sortProvider &&
              sortProvider.Header.Order.Select(order => order.Key).Any(calculatedColumnIndexes.Contains));
        }
        case ProviderType.Apply:
          return containsCalculatedColumns || distinctIsUsed || pagingIsUsed || groupByIsUsed;
        case ProviderType.Join: {
          var shouldUseQueryReference = distinctIsUsed || pagingIsUsed || groupByIsUsed;
          if (shouldUseQueryReference) {
            return true;
          }

          var joinProvider = (JoinProvider) origin;
          var isRight = joinProvider.Right == compiledSource.Origin;
          var indexes = joinProvider.EqualIndexes.Select(p => isRight ? p.Second : p.First);
          return (joinProvider.JoinType == JoinType.LeftOuter && filterIsUsed && isRight)
            || (containsCalculatedColumns && indexes.Any(calculatedColumnIndexes.Contains));
        }
        case ProviderType.PredicateJoin: {
          var shouldUseQueryReference = distinctIsUsed || pagingIsUsed || groupByIsUsed;
          if (shouldUseQueryReference) {
            return true;
          }

          var joinProvider = (PredicateJoinProvider) origin;
          var isRight = joinProvider.Right == compiledSource.Origin;
          var indexes = new TupleAccessGatherer()
            .Gather(joinProvider.Predicate.Body, joinProvider.Predicate.Parameters[isRight ? 1 : 0]);
          return (joinProvider.JoinType == JoinType.LeftOuter && filterIsUsed && isRight)
            || (containsCalculatedColumns && indexes.Any(calculatedColumnIndexes.Contains));
        }
        case ProviderType.Sort when distinctIsUsed:
          return true;
        case ProviderType.Sort: {
          var orderingOverCalculatedColumn = origin.Header.Order
            .Select(order => order.Key)
            .Any(calculatedColumnIndexes.Contains);
          return orderingOverCalculatedColumn;
        }
        default:
          var typeIdIsOnlyCalculatedColumn = containsCalculatedColumns && calculatedColumnIndexes.Count == 1
            && IsTypeIdColumn(sourceSelect.Columns[calculatedColumnIndexes[0]]);
          return (containsCalculatedColumns && !typeIdIsOnlyCalculatedColumn) || distinctIsUsed || pagingIsUsed || groupByIsUsed;
      }
    }

    private SqlExpression GetOrderByExpression(SqlExpression expression, SortProvider provider, int index)
    {
      var columns = provider.Header.Columns;
      if (columns.Count <= index) {
        return expression;
      }

      var columnType = columns[index].Type;
      if (providerInfo.Supports(ProviderFeatures.DateTimeEmulation) && columnType == WellKnownTypes.DateTime) {
        return SqlDml.Cast(expression, SqlType.DateTime);
      }

      if (providerInfo.Supports(ProviderFeatures.DateTimeOffsetEmulation) && columnType == WellKnownTypes.DateTimeOffset) {
        return SqlDml.Cast(expression, SqlType.DateTimeOffset);
      }

      return expression;
    }

    private SqlExpression GetJoinExpression(SqlExpression leftExpression, SqlExpression rightExpression,
      JoinProvider provider, int index)
    {
      if (provider.EqualColumns.Length > index) {
        Pair<Column> columnPair;
        if (providerInfo.Supports(ProviderFeatures.DateTimeEmulation)) {
          columnPair = provider.EqualColumns[index];
          if (columnPair.First.Type == WellKnownTypes.DateTime) {
            leftExpression = SqlDml.Cast(leftExpression, SqlType.DateTime);
          }

          if (columnPair.Second.Type == WellKnownTypes.DateTime) {
            rightExpression = SqlDml.Cast(rightExpression, SqlType.DateTime);
          }
        }

        if (providerInfo.Supports(ProviderFeatures.DateTimeOffsetEmulation)) {
          columnPair = provider.EqualColumns[index];
          if (columnPair.First.Type == WellKnownTypes.DateTimeOffset) {
            leftExpression = SqlDml.Cast(leftExpression, SqlType.DateTimeOffset);
          }

          if (columnPair.Second.Type == WellKnownTypes.DateTimeOffset) {
            rightExpression = SqlDml.Cast(rightExpression, SqlType.DateTimeOffset);
          }
        }
      }

      return leftExpression == rightExpression;
    }

    public SqlExpression GetOuterExpression(ApplyParameter parameter, int columnIndex)
    {
      var reference = OuterReferences[parameter];
      var sqlProvider = reference.First;
      var useQueryReference = reference.Second;
      return useQueryReference
        ? sqlProvider.PermanentReference[columnIndex]
        : ExtractColumnExpression(sqlProvider.Request.Statement.Columns[columnIndex]);
    }
  }
}
