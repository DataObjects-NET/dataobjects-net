// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.13

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Storage.Providers.Sql.Expressions;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Helpers;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql
{
  partial class SqlCompiler 
  {
    protected SqlProvider CreateProvider(SqlSelect statement,
      CompilableProvider origin, params ExecutableProvider[] sources)
    {
      var extraBindings = (IEnumerable<QueryParameterBinding>) null;
      return CreateProvider(statement, extraBindings, origin, sources);
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
      if (extraBindings!=null)
        parameterBindings = parameterBindings.Concat(extraBindings);

      bool allowBatching = sqlSources
        .Aggregate(true, (current, provider) =>
          current && provider.Request.CheckOptions(RequestOptions.AllowBatching));
      var tupleDescriptor = origin.Header.TupleDescriptor;

      var options = RequestOptions.Empty;
      if (allowBatching)
        options |= RequestOptions.AllowBatching;

      if (statement.Columns.Count < origin.Header.TupleDescriptor.Count)
        tupleDescriptor = origin.Header.TupleDescriptor.TrimFields(statement.Columns.Count);
      
      var request = new QueryRequest(statement, tupleDescriptor, options, parameterBindings);

      return new SqlProvider(Handlers, request, origin, sources);
    }

    protected virtual string ProcessAliasedName(string name)
    {
      return name;
    }
    
    protected Pair<SqlExpression, HashSet<QueryParameterBinding>> ProcessExpression(LambdaExpression le, params List<SqlExpression>[] sourceColumns)
    {
      var processor = new ExpressionProcessor(le, this, Handlers, sourceColumns);
      var result = new Pair<SqlExpression, HashSet<QueryParameterBinding>>(
        processor.Translate(),
        processor.Bindings);
      return result;
    }

    protected SqlSelect ExtractSqlSelect(CompilableProvider origin, SqlProvider compiledSource)
    {
      var sourceSelect = compiledSource.Request.SelectStatement;
      if (ShouldUseQueryReference(origin, compiledSource)) {
        var queryRef = compiledSource.PermanentReference;
        var query = SqlDml.Select(queryRef);
        query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
        return query;
      }
      return sourceSelect.ShallowClone();
    }

    public List<SqlExpression> ExtractColumnExpressions(SqlSelect query, CompilableProvider origin)
    {
      var result = new List<SqlExpression>(query.Columns.Count);
      foreach (var column in query.Columns) {
        SqlExpression expression;
        if (IsColumnStub(column)) {
          expression = stubColumnMap[ExtractColumnStub(column)];
          var subQuery = expression as SqlSubQuery;
          if (!subQuery.IsNullReference()) {
            var subSelect = subQuery.Query as SqlSelect;
            if (subSelect != null) {
              if (subSelect.Columns.Count == 1 && subSelect.From == null) {
                var userColumn = subSelect.Columns[0] as SqlUserColumn;
                if (!userColumn.IsNullReference()) {
                  var cast = userColumn.Expression as SqlCast;
                  if (!cast.IsNullReference() &&  cast.Type.Type == SqlType.Boolean) {
                    var sqlCase = cast.Operand as SqlCase;
                    if(!sqlCase.IsNullReference() && sqlCase.Count == 1) {
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
        else
          expression = column;
        var columnRef = expression as SqlColumnRef;
        if (!columnRef.IsNullReference())
          expression = columnRef.SqlColumn;
        result.Add(expression);
      }
      return result;
    }

    protected void AddInlinableColumn(IInlinableProvider provider,
      SqlSelect resultQuery, string columnName, SqlExpression columnExpression)
    {
      columnName = ProcessAliasedName(columnName);
      var columnRef = SqlDml.ColumnRef(SqlDml.Column(columnExpression), columnName);
      if (provider.IsInlined) {
        var columnStub = SqlDml.ColumnStub(columnRef);
        stubColumnMap.Add(columnStub, columnExpression);
        resultQuery.Columns.Add(columnStub);
      }
      else
        resultQuery.Columns.Add(columnRef);      
    }

    protected SqlExpression GetBooleanColumnExpression(SqlExpression originalExpression)
    {
      return ProviderInfo.Supports(ProviderFeatures.FullFeaturedBooleanExpressions)
        ? originalExpression
        : booleanExpressionConverter.BooleanToInt(originalExpression);
    }

    #region Private methods

    private static bool IsCalculatedColumn(SqlColumn column)
    {
      if (column is SqlUserColumn)
        return true;
      var cRef = column as SqlColumnRef;
      if (!ReferenceEquals(null, cRef))
        return cRef.SqlColumn is SqlUserColumn;
      return false;
    }

    private static bool IsColumnStub(SqlColumn column)
    {
      if (column is SqlColumnStub)
        return true;
      var cRef = column as SqlColumnRef;
      if (!ReferenceEquals(null, cRef))
        return cRef.SqlColumn is SqlColumnStub;
      return false;
    }

    private static SqlColumnStub ExtractColumnStub(SqlColumn column)
    {
      var columnStub = column as SqlColumnStub;
      if (!ReferenceEquals(null, columnStub))
        return columnStub;
      var columnRef = column as SqlColumnRef;
      if (!ReferenceEquals(null, columnRef))
        return (SqlColumnStub) columnRef.SqlColumn;
      return (SqlColumnStub) column;
    }

    private static SqlUserColumn ExtractUserColumn(SqlColumn column)
    {
      var userColumn = column as SqlUserColumn;
      if (!ReferenceEquals(null, userColumn))
        return userColumn;
      var columnRef = column as SqlColumnRef;
      if (!ReferenceEquals(null, columnRef))
        return (SqlUserColumn) columnRef.SqlColumn;
      return (SqlUserColumn) column;
    }

    private static bool ShouldUseQueryReference(CompilableProvider origin, SqlProvider compiledSource)
    {
      var sourceSelect = compiledSource.Request.SelectStatement;

      var calculatedColumnIndexes = sourceSelect.Columns
        .Select((c, i) => IsCalculatedColumn(c) ? i : -1)
        .Where(i => i >= 0)
        .ToList();
      var containsCalculatedColumns = calculatedColumnIndexes.Count > 0;
      var rowNumberIsUsed = calculatedColumnIndexes.Count > 0 && sourceSelect.Columns
        .Select((c, i) => new { c, i })
        .Any(a => calculatedColumnIndexes.Contains(a.i) && ExtractUserColumn(a.c).Expression is SqlRowNumber);
      var pagingIsUsed = !sourceSelect.Limit.IsNullReference() || !sourceSelect.Offset.IsNullReference() || rowNumberIsUsed;
      var groupByIsUsed = sourceSelect.GroupBy.Count > 0;
      var distinctIsUsed = sourceSelect.Distinct;
      var filterIsUsed = !sourceSelect.Where.IsNullReference();
      if (sourceSelect.From == null)
        return false;
      var columnCountIsNotSame = sourceSelect.From.Columns.Count!=sourceSelect.Columns.Count;

      if (origin.Type==ProviderType.Filter) {
        var filterProvider = (FilterProvider) origin;
        var usedColumnIndexes = new TupleAccessGatherer().Gather(filterProvider.Predicate.Body);
        return pagingIsUsed || usedColumnIndexes.Any(calculatedColumnIndexes.Contains);
      }

      if (origin.Type==ProviderType.Select)
        return distinctIsUsed;

      if (origin.Type==ProviderType.RowNumber) {
        var usedColumnIndexes = origin.Header.Order.Select(o => o.Key);
        return pagingIsUsed || groupByIsUsed || distinctIsUsed || usedColumnIndexes.Any(calculatedColumnIndexes.Contains);
      }

      if (origin.Type==ProviderType.Calculate) {
        var calculateProvider = (CalculateProvider) origin;
        var columnGatherer = new TupleAccessGatherer();
        var usedColumnIndexes = new List<int>();
        foreach (var column in calculateProvider.CalculatedColumns)
          usedColumnIndexes.AddRange(
            columnGatherer.Gather(column.Expression.Body, column.Expression.Parameters[0]));

        return usedColumnIndexes.Any(calculatedColumnIndexes.Contains) || columnCountIsNotSame;
      }

      if (origin.Type==ProviderType.Take || origin.Type==ProviderType.Skip) {
        var sortProvider = origin.Sources[0] as SortProvider;
        var orderingOverCalculatedColumn = sortProvider!=null &&
          sortProvider.Header.Order
            .Select(order => order.Key)
            .Any(calculatedColumnIndexes.Contains);
        return distinctIsUsed || pagingIsUsed || groupByIsUsed || orderingOverCalculatedColumn;
      }

      if (origin.Type == ProviderType.Apply)
        return containsCalculatedColumns || distinctIsUsed || pagingIsUsed || groupByIsUsed;

      if (origin.Type == ProviderType.Join) {
        var shouldUseQueryReference = containsCalculatedColumns || distinctIsUsed || pagingIsUsed || groupByIsUsed;
        if (shouldUseQueryReference)
          return true;
        var joinProvider = (JoinProvider) origin;
        return joinProvider.JoinType == JoinType.LeftOuter && filterIsUsed; 
      }

      if (origin.Type == ProviderType.PredicateJoin) {
        var shouldUseQueryReference = containsCalculatedColumns || distinctIsUsed || pagingIsUsed || groupByIsUsed;
        if (shouldUseQueryReference)
          return true;
        var joinProvider = (PredicateJoinProvider) origin;
        return joinProvider.JoinType == JoinType.LeftOuter && filterIsUsed;
      }

      if (origin.Type == ProviderType.Sort) {
        var orderingOverCalculatedColumn = origin.Header.Order
          .Select(order => order.Key)
          .Any(calculatedColumnIndexes.Contains);
        return orderingOverCalculatedColumn;
      }

      return containsCalculatedColumns || distinctIsUsed || pagingIsUsed || groupByIsUsed;
    }

    #endregion
  }
}