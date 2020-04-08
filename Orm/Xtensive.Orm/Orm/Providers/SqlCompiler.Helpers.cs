// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.13

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Transformation;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Tuples;

namespace Xtensive.Orm.Providers
{
  partial class SqlCompiler 
  {
    protected SqlProvider CreateProvider(SqlSelect statement,
      CompilableProvider origin, params ExecutableProvider[] sources)
    {
      return CreateProvider(statement, (IEnumerable<QueryParameterBinding>) null, origin, sources);
    }

    protected SqlProvider CreateProvider(SqlSelect statement, QueryParameterBinding extraBinding,
      CompilableProvider origin, params ExecutableProvider[] sources)
    {
      var extraBindings = extraBinding!=null ? EnumerableUtils.One(extraBinding) : null;
      return CreateProvider(statement, extraBindings, origin, sources);
    }

    protected SqlProvider CreateProvider(SqlSelect statement, IEnumerable<QueryParameterBinding> extraBindings,
      CompilableProvider origin, params ExecutableProvider[] sources)
    {
      var sqlSources = sources.OfType<SqlProvider>();

      var parameterBindings = sqlSources.SelectMany(p => p.Request.ParameterBindings);
      if (extraBindings!=null) {
        parameterBindings = parameterBindings.Concat(extraBindings);
      }

      var allowBatching = sqlSources
        .Aggregate(true, (current, provider) =>
          current && provider.Request.CheckOptions(QueryRequestOptions.AllowOptimization));
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

    protected virtual string ProcessAliasedName(string name)
    {
      return name;
    }

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
        var subQuery = expression as SqlSubQuery;
        if (!subQuery.IsNullReference()) {
          var subSelect = subQuery.Query as SqlSelect;
          if (subSelect!=null) {
            if (subSelect.Columns.Count==1 && subSelect.From==null) {
              var userColumn = subSelect.Columns[0] as SqlUserColumn;
              if (!userColumn.IsNullReference()) {
                var cast = userColumn.Expression as SqlCast;
                if (!cast.IsNullReference() && cast.Type.Type==SqlType.Boolean) {
                  var sqlCase = cast.Operand as SqlCase;
                  if (!sqlCase.IsNullReference() && sqlCase.Count==1) {
                    var pair = sqlCase.First();
                    var key = pair.Key as SqlUnary;
                    if (!key.IsNullReference() && pair.Value is SqlLiteral<int>)
                      expression = cast;
                  }
                }
              }
            }
          }
        }
      }
      else {
        expression = column;
      }

      var columnRef = expression as SqlColumnRef;
      if (!columnRef.IsNullReference()) {
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

    protected SqlExpression GetBooleanColumnExpression(SqlExpression originalExpression)
    {
      return providerInfo.Supports(ProviderFeatures.FullFeaturedBooleanExpressions)
        ? originalExpression
        : booleanExpressionConverter.BooleanToInt(originalExpression);
    }

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
      var cRef = column as SqlColumnRef;
      return cRef?.SqlColumn is SqlUserColumn;
    }

    private static bool IsColumnStub(SqlColumn column)
    {
      if (column is SqlColumnStub) {
        return true;
      }

      var cRef = column as SqlColumnRef;
      return cRef?.SqlColumn is SqlColumnStub;
    }

    private static SqlColumnStub ExtractColumnStub(SqlColumn column)
    {
      switch (column) {
        case SqlColumnStub columnStub:
          return columnStub;
        case SqlColumnRef columnRef:
          return (SqlColumnStub) columnRef.SqlColumn;
        default:
          return (SqlColumnStub) column;
      }
    }

    private static SqlUserColumn ExtractUserColumn(SqlColumn column)
    {
      switch (column) {
        case SqlUserColumn userColumn:
          return userColumn;
        case SqlColumnRef columnRef:
          return (SqlUserColumn) columnRef.SqlColumn;
        default:
          return (SqlUserColumn) column;
      }
    }

    private static bool ShouldUseQueryReference(CompilableProvider origin, SqlProvider compiledSource)
    {
      var sourceSelect = compiledSource.Request.Statement;
      if (sourceSelect.From==null) {
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
          var aggregateProvider = (AggregateProvider)origin;
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
          return containsCalculatedColumns || distinctIsUsed || pagingIsUsed || groupByIsUsed;
      }
    }

    private SqlExpression GetOrderByExpression(SqlExpression expression, SortProvider provider, int index)
    {
      var columns = provider.Header.Columns;
      if (columns.Count <= index) {
        return expression;
      }

      var columnType = columns[index].Type;
      if (providerInfo.Supports(ProviderFeatures.DateTimeEmulation) && columnType == typeof(DateTime)) {
        return SqlDml.Cast(expression, SqlType.DateTime);
      }

      if (providerInfo.Supports(ProviderFeatures.DateTimeOffsetEmulation) && columnType == typeof(DateTimeOffset)) {
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
          if (columnPair.First.Type == typeof(DateTime)) {
            leftExpression = SqlDml.Cast(leftExpression, SqlType.DateTime);
          }

          if (columnPair.Second.Type == typeof(DateTime)) {
            rightExpression = SqlDml.Cast(rightExpression, SqlType.DateTime);
          }
        }

        if (providerInfo.Supports(ProviderFeatures.DateTimeOffsetEmulation)) {
          columnPair = provider.EqualColumns[index];
          if (columnPair.First.Type == typeof(DateTimeOffset)) {
            leftExpression = SqlDml.Cast(leftExpression, SqlType.DateTimeOffset);
          }

          if (columnPair.Second.Type == typeof(DateTimeOffset)) {
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
