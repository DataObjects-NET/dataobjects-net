// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Vakhtina Elena
// Created:    2009.02.13

using System;
using Xtensive.Core;
using Xtensive.Indexing;
using Xtensive.Storage.Providers.Sql.Mappings;
using Xtensive.Storage.Providers.Sql.Resources;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql.Expressions;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Compilation.New;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Rse.Compilation;
using SqlFactory = Xtensive.Sql.Dom.Sql;


namespace Xtensive.Storage.Providers.Sql
{
  /// <inheritdoc/>
  [Serializable]
  public class SqlCompiler : RseCompiler
  {
    /// <summary>
    /// Gets the <see cref="HandlerAccessor"/> object providing access to available storage handlers.
    /// </summary>
    protected HandlerAccessor Handlers { get; private set; }

    /// <inheritdoc/>
    public override bool IsCompatible(ExecutableProvider provider)
    {
      return provider is SqlProvider;
    }

    /// <inheritdoc/>
    public override ExecutableProvider ToCompatible(ExecutableProvider provider)
    {
      return new StoredProvider(provider).Compile();
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitAggregate(AggregateProvider provider, ExecutableProvider[] sources)
    {
      var compiledSource = sources[0];
      var source = compiledSource as SqlProvider;
      if (source == null)
        return null;

      var queryRef = SqlFactory.QueryRef(source.Request.Statement as SqlSelect);
      SqlSelect sqlSelect = SqlFactory.Select(queryRef);

      var columns = queryRef.Columns.ToList();
      sqlSelect.Columns.Clear();

      for (int i = 0; i < provider.GroupColumnIndexes.Length; i++) {
        var columnIndex = provider.GroupColumnIndexes[i];
        var column = columns[columnIndex];
        sqlSelect.Columns.Add(column);
        sqlSelect.GroupBy.Add(column);
      }

      foreach (var col in provider.AggregateColumns) {
        SqlExpression expr = null;
        switch (col.AggregateType) {
        case AggregateType.Avg:
          expr = SqlFactory.Avg(columns[col.SourceIndex]);
          break;
        case AggregateType.Count:
          expr = SqlFactory.Count(columns[col.SourceIndex]);
          break;
        case AggregateType.Max:
          expr = SqlFactory.Max(columns[col.SourceIndex]);
          break;
        case AggregateType.Min:
          expr = SqlFactory.Min(columns[col.SourceIndex]);
          break;
        case AggregateType.Sum:
          expr = SqlFactory.Sum(columns[col.SourceIndex]);
          break;
        }
        sqlSelect.Columns.Add(expr, col.Name);
      }

      var request = new SqlFetchRequest(sqlSelect, provider.Header, source.Request.ParameterBindings);
      return new SqlProvider(provider, request, Handlers, source);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitAlias(AliasProvider provider, ExecutableProvider[] sources)
    {
      var compiledSource = sources[0];
      var source = compiledSource as SqlProvider;
      if (source == null)
        return null;

      var sqlSelect = (SqlSelect) source.Request.Statement.Clone();
      var columns = sqlSelect.Columns.ToList();
      sqlSelect.Columns.Clear();
      for (int i = 0; i < columns.Count; i++)
        sqlSelect.Columns.Add(columns[i], provider.Header.Columns[i].Name);
      var request = new SqlFetchRequest(sqlSelect, provider.Header, source.Request.ParameterBindings);
      return new SqlProvider(provider, request, Handlers, source);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitCalculate(CalculationProvider provider, ExecutableProvider[] sources)
    {
      var compiledSource = sources[0];
      var source = compiledSource as SqlProvider;
      if (source == null)
        return null;

      var sqlSelect = (SqlSelect) source.Request.Statement.Clone();
      var request = new SqlFetchRequest(sqlSelect, provider.Header, source.Request.ParameterBindings);
      var visitor = new ExpressionProcessor(request, Handlers.Domain.Model);

      foreach (var column in provider.CalculatedColumns)
        visitor.AppendCalculatedColumnToRequest(column.Expression, column.Name);

      return new SqlProvider(provider, request, Handlers, source);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitDistinct(DistinctProvider provider, ExecutableProvider[] sources)
    {
      var compiledSource = sources[0];
      var source = compiledSource as SqlProvider;
      if (source == null)
        return null;

      var query = (SqlSelect) source.Request.Statement;
      if (query.Distinct)
        return source;

      var clone = (SqlSelect) query.Clone();
      clone.Distinct = true;
      var request = new SqlFetchRequest(clone, provider.Header, source.Request.ParameterBindings);
      return new SqlProvider(provider, request, Handlers, source);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitFilter(FilterProvider provider, ExecutableProvider[] sources)
    {
      var compiledSource = sources[0];
      var source = compiledSource as SqlProvider;
      if (source == null)
        return null;

      SqlSelect query;
      if (provider.Source is AggregateProvider || provider.Source is CalculationProvider) {
        var queryRef = SqlFactory.QueryRef(source.Request.Statement as SqlSelect);
        query = SqlFactory.Select(queryRef);
        query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      }
      else
        query = (SqlSelect) source.Request.Statement.Clone();

      var request = new SqlFetchRequest(query, provider.Header, source.Request.ParameterBindings);
      var visitor = new ExpressionProcessor(request, Handlers.Domain.Model);
      visitor.AppendFilterToRequest(provider.Predicate);

      return new SqlProvider(provider, request, Handlers, source);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitIndex(IndexProvider provider, ExecutableProvider[] sources)
    {
      var index = provider.Index.Resolve(Handlers.Domain.Model);
      SqlSelect query = BuildProviderQuery(index);
      var request = new SqlFetchRequest(query, provider.Header);
      return new SqlProvider(provider, request, Handlers);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitJoin(JoinProvider provider, ExecutableProvider[] sources)
    {
      var left = sources[0] as SqlProvider;
      var right = sources[1] as SqlProvider;

      if (left == null || right == null)
        return null;
      var leftSelect = (SqlSelect) left.Request.Statement;
      var leftQuery = SqlFactory.QueryRef(leftSelect);
      var rightSelect = (SqlSelect) right.Request.Statement;
      var rightQuery = SqlFactory.QueryRef(rightSelect);
      var joinedTable = SqlFactory.Join(
        provider.LeftJoin ? SqlJoinType.LeftOuterJoin : SqlJoinType.InnerJoin,
        leftQuery,
        rightQuery,
        provider.EqualIndexes
          .Select(pair => leftQuery.Columns[pair.First] == rightQuery.Columns[pair.Second])
          .Aggregate(null as SqlExpression, (expression, binary) => expression & binary)
        );

      SqlSelect query = SqlFactory.Select(joinedTable);
      query.Columns.AddRange(leftQuery.Columns.Union(rightQuery.Columns).Cast<SqlColumn>());
      var request = new SqlFetchRequest(query, provider.Header, left.Request.ParameterBindings.Union(right.Request.ParameterBindings));
      return new SqlProvider(provider, request, Handlers, left, right);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitRange(RangeProvider provider, ExecutableProvider[] sources)
    {
      var source = sources[0] as SqlProvider;
      if (source == null)
        return null;

      var query = (SqlSelect) source.Request.Statement.Clone();
      var keyColumns = provider.Header.Order.ToList();
      var originalRange = provider.CompiledRange.Invoke();
      var request = new SqlFetchRequest(query, provider.Header, source.Request.ParameterBindings);
      var rangeProvider = new SqlRangeProvider(provider, request, Handlers, originalRange, source);

      if (originalRange.EndPoints.First.HasValue) {
        for (int i = 0; i < originalRange.EndPoints.First.Value.Count; i++) {
          var column = provider.Header.Columns[keyColumns[i].Key];
          DataTypeMapping typeMapping = ((DomainHandler) Handlers.DomainHandler).ValueTypeMapper.GetTypeMapping(column.Type);
          int fieldIndex = i;
          var binding = new SqlFetchParameterBinding(() => rangeProvider.CurrentRange.EndPoints.First.Value.GetValue(fieldIndex), typeMapping);
          request.ParameterBindings.Add(binding);
          if (i == originalRange.EndPoints.First.Value.Count - 1) {
            switch (originalRange.EndPoints.First.ValueType) {
            case EntireValueType.Default:
              request.ParameterBindings.Add(binding);
              query.Where &= query.Columns[keyColumns[i].Key] >= binding.SqlParameter;
              break;
            case EntireValueType.PositiveInfinitesimal:
              request.ParameterBindings.Add(binding);
              query.Where &= query.Columns[keyColumns[i].Key] > binding.SqlParameter;
              break;
            case EntireValueType.NegativeInfinitesimal:
              request.ParameterBindings.Add(binding);
              query.Where &= query.Columns[keyColumns[i].Key] >= binding.SqlParameter;
              break;
            case EntireValueType.PositiveInfinity:
              query.Where &= SqlFactory.Native("1") == SqlFactory.Native("0");
              break;
            case EntireValueType.NegativeInfinity:
              break;
            }
          }
          else
            query.Where &= query.Columns[keyColumns[i].Key] >= binding.SqlParameter;
        }
      }
      else if (originalRange.EndPoints.First.ValueType == EntireValueType.PositiveInfinity)
        query.Where &= SqlFactory.Native("1") == SqlFactory.Native("0");

      if (originalRange.EndPoints.Second.HasValue) {
        for (int i = 0; i < originalRange.EndPoints.Second.Value.Count; i++) {
          var column = provider.Header.Columns[keyColumns[i].Key];
          DataTypeMapping typeMapping = ((DomainHandler) Handlers.DomainHandler).ValueTypeMapper.GetTypeMapping(column.Type);
          int fieldIndex = i;
          var binding = new SqlFetchParameterBinding(() => rangeProvider.CurrentRange.EndPoints.Second.Value.GetValue(fieldIndex), typeMapping);
          request.ParameterBindings.Add(binding);
          if (i == originalRange.EndPoints.Second.Value.Count - 1) {
            switch (originalRange.EndPoints.Second.ValueType) {
            case EntireValueType.Default:
              request.ParameterBindings.Add(binding);
              query.Where &= query.Columns[keyColumns[i].Key] <= binding.SqlParameter;
              break;
            case EntireValueType.PositiveInfinitesimal:
              request.ParameterBindings.Add(binding);
              query.Where &= query.Columns[keyColumns[i].Key] <= binding.SqlParameter;
              break;
            case EntireValueType.NegativeInfinitesimal:
              request.ParameterBindings.Add(binding);
              query.Where &= query.Columns[keyColumns[i].Key] < binding.SqlParameter;
              break;
            case EntireValueType.PositiveInfinity:
              break;
            case EntireValueType.NegativeInfinity:
              query.Where &= SqlFactory.Native("1") == SqlFactory.Native("0");
              break;
            }
          }
          else
            query.Where &= query.Columns[keyColumns[i].Key] <= binding.SqlParameter;
        }
      }
      else if (originalRange.EndPoints.First.ValueType == EntireValueType.PositiveInfinity)
        query.Where &= SqlFactory.Native("1") == SqlFactory.Native("0");

      return rangeProvider;
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitSeek(SeekProvider provider, ExecutableProvider[] sources)
    {
      var source = sources[0] as SqlProvider;
      if (source == null)
        return null;

      var query = (SqlSelect) source.Request.Statement.Clone();
      var request = new SqlFetchRequest(query, provider.Header, source.Request.ParameterBindings);
      var typeIdColumnName = Handlers.NameBuilder.TypeIdColumnName;
      Func<KeyValuePair<int, Direction>, bool> filterNonTypeId =
        pair => ((MappedColumn) provider.Header.Columns[pair.Key]).ColumnInfoRef.ColumnName != typeIdColumnName;
      var keyColumns = provider.Header.Order
        .Where(filterNonTypeId)
        .ToList();

      for (int i = 0; i < keyColumns.Count; i++) {
        int columnIndex = keyColumns[i].Key;
        var sqlColumn = query.Columns[columnIndex];
        var column = provider.Header.Columns[columnIndex];
        DataTypeMapping typeMapping = ((DomainHandler) Handlers.DomainHandler).ValueTypeMapper.GetTypeMapping(column.Type);
        int index = i;
        var binding = new SqlFetchParameterBinding(() => provider.CompiledKey.Invoke().GetValue(index), typeMapping);
        request.ParameterBindings.Add(binding);
        query.Where &= sqlColumn == SqlFactory.ParameterRef(binding.SqlParameter);
      }

      return new SqlProvider(provider, request, Handlers, source);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitSelect(SelectProvider provider, ExecutableProvider[] sources)
    {
      var source = sources[0] as SqlProvider;
      if (source == null)
        return null;

      var queryRef = SqlFactory.QueryRef(source.Request.Statement as SqlSelect);
      SqlSelect query = SqlFactory.Select(queryRef);
      query.Columns.AddRange(provider.ColumnIndexes.Select(i => (SqlColumn) queryRef.Columns[i]));
      var request = new SqlFetchRequest(query, provider.Header, source.Request.ParameterBindings);

      return new SqlProvider(provider, request, Handlers, source);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitSkip(SkipProvider provider, ExecutableProvider[] sources)
    {
      var source = sources[0] as SqlProvider;
      if (source == null)
        return null;

      var queryRef = SqlFactory.QueryRef(source.Request.Statement as SqlSelect);
      var query = SqlFactory.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      query.Offset = provider.CompiledCount();
      var request = new SqlFetchRequest(query, provider.Header, source.Request.ParameterBindings);
      return new SqlProvider(provider, request, Handlers, source);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitSort(SortProvider provider, ExecutableProvider[] sources)
    {
      var source = sources[0] as SqlProvider;
      if (source == null)
        return null;

      var query = (SqlSelect) source.Request.Statement.Clone();
      query.OrderBy.Clear();
      foreach (KeyValuePair<int, Direction> sortOrder in provider.Order)
        query.OrderBy.Add(sortOrder.Key + 1, sortOrder.Value == Direction.Positive);

      var request = new SqlFetchRequest(query, provider.Header, source.Request.ParameterBindings);
      return new SqlProvider(provider, request, Handlers, source);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitStore(StoredProvider provider, ExecutableProvider[] sources)
    {
      const string TABLE_NAME_PATTERN = "Tmp_{0}";

      ExecutableProvider ex = null;
      var domainHandler = (DomainHandler) Handlers.DomainHandler;
      Schema schema = domainHandler.Schema;
      Table table;
      string tableName = string.Format(TABLE_NAME_PATTERN, provider.Name);
      if (provider.Source != null) {
        ex = sources[0] as ExecutableProvider;
        table = provider.Scope == TemporaryDataScope.Global ? schema.CreateTable(tableName)
          : schema.CreateTemporaryTable(tableName);

        foreach (Column column in provider.Header.Columns) {
          SqlValueType svt;
          var mappedColumn = column as MappedColumn;
          if (mappedColumn != null) {
            ColumnInfo ci = mappedColumn.ColumnInfoRef.Resolve(domainHandler.Domain.Model);
            DataTypeMapping tm = domainHandler.ValueTypeMapper.GetTypeMapping(ci);
            svt = domainHandler.ValueTypeMapper.BuildSqlValueType(ci);
          }
          else
            svt = domainHandler.ValueTypeMapper.BuildSqlValueType(column.Type, 0);
          TableColumn tableColumn = table.CreateColumn(column.Name, svt);
          tableColumn.IsNullable = true;
        }
      }
      else
        table = schema.Tables[tableName];

      SqlTableRef tr = SqlFactory.TableRef(table);
      SqlSelect query = SqlFactory.Select(tr);
      foreach (SqlTableColumn column in tr.Columns)
        query.Columns.Add(column);
      var request = new SqlFetchRequest(query, provider.Header);
      schema.Tables.Remove(table);

      return new SqlStoredProvider(provider, request, Handlers, ex, table);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitTake(TakeProvider provider, ExecutableProvider[] sources)
    {
      var source = sources[0] as SqlProvider;
      if (source == null)
        return null;

      var query = (SqlSelect) source.Request.Statement.Clone();
      var count = provider.CompiledCount();
      if (query.Top == 0 || query.Top > count)
        query.Top = count;
      var request = new SqlFetchRequest(query, provider.Header, source.Request.ParameterBindings);
      return new SqlProvider(provider, request, Handlers, source);
    }

    #region Private methods.

    private SqlSelect BuildProviderQuery(IndexInfo index)
    {
      if (index.IsVirtual) {
        if ((index.Attributes & IndexAttributes.Union) > 0)
          return BuildUnionQuery(index);
        if ((index.Attributes & IndexAttributes.Join) > 0)
          return BuildJoinQuery(index);
        if ((index.Attributes & IndexAttributes.Filtered) > 0)
          return BuildFilteredQuery(index);
        throw new NotSupportedException(String.Format(Strings.ExUnsupportedIndex, index.Name, index.Attributes));
      }
      return BuildTableQuery(index);
    }

    private SqlSelect BuildTableQuery(IndexInfo index)
    {
      var domainHandler = (DomainHandler) Handlers.DomainHandler;
      Table table = domainHandler.Schema.Tables[index.ReflectedType.MappingName];
      bool atRootPolicy = false;
      if (table == null) {
        table = domainHandler.Schema.Tables[index.ReflectedType.GetRoot().MappingName];
        atRootPolicy = true;
      }

      SqlTableRef tableRef = SqlFactory.TableRef(table);
      SqlSelect query = SqlFactory.Select(tableRef);
      if (!atRootPolicy)
        query.Columns.AddRange(index.Columns.Select(c => (SqlColumn) tableRef.Columns[c.Name]));
      else {
        var root = index.ReflectedType.GetRoot().AffectedIndexes.First(i => i.IsPrimary);
        var lookup = root.Columns.ToDictionary(c => c.Field, c => c.Name);
        query.Columns.AddRange(index.Columns.Select(c => (SqlColumn) tableRef.Columns[lookup[c.Field]]));
      }
      return query;
    }

    private SqlSelect BuildUnionQuery(IndexInfo index)
    {
      ISqlQueryExpression result = null;

      var baseQueries = index.UnderlyingIndexes.Select(i => BuildProviderQuery(i)).ToList();
      foreach (var select in baseQueries) {
        int i = 0;
        foreach (var columnInfo in index.Columns) {
          var column = select.Columns[columnInfo.Name];
          if (SqlExpression.IsNull(column))
            select.Columns.Insert(i, SqlFactory.Null, columnInfo.Name);
          i++;
        }
        if (result == null)
          result = select;
        else
          result = result.Union(select);
      }

      var unionRef = SqlFactory.QueryRef(result);
      SqlSelect query = SqlFactory.Select(unionRef);
      query.Columns.AddRange(unionRef.Columns.Cast<SqlColumn>());
      return query;
    }

    private SqlSelect BuildJoinQuery(IndexInfo index)
    {
      SqlTable result = null;
      SqlTable rootTable = null;
      IEnumerable<SqlColumn> columns = null;
      int keyColumnCount = index.KeyColumns.Count;
      int nonValueColumnsCount = keyColumnCount + index.IncludedColumns.Count;
      var baseQueries = index.UnderlyingIndexes.Select(i => BuildProviderQuery(i)).ToList();
      foreach (var baseQuery in baseQueries) {
        if (result == null) {
          result = SqlExpression.IsNull(baseQuery.Where) ? baseQuery.From : SqlFactory.QueryRef(baseQuery);
          rootTable = result;
          columns = rootTable.Columns.Cast<SqlColumn>();
        }
        else {
          var queryRef = SqlExpression.IsNull(baseQuery.Where) ? baseQuery.From : SqlFactory.QueryRef(baseQuery);
          SqlExpression joinExpression = null;
          for (int i = 0; i < keyColumnCount; i++) {
            SqlBinary binary = (queryRef.Columns[i] == rootTable.Columns[i]);
            if (SqlExpression.IsNull(joinExpression == null))
              joinExpression = binary;
            else
              joinExpression &= binary;
          }
          result = result.LeftOuterJoin(queryRef, joinExpression);
          columns = columns.Union(queryRef.Columns.Skip(nonValueColumnsCount).Cast<SqlColumn>());
        }
      }

      SqlSelect query = SqlFactory.Select(result);
      query.Columns.AddRange(columns);

      return query;
    }

    private SqlSelect BuildFilteredQuery(IndexInfo index)
    {
      var descendants = new List<TypeInfo> {index.ReflectedType};
      descendants.AddRange(index.ReflectedType.GetDescendants(true));
      var typeIds = descendants.Select(t => t.TypeId).ToArray();

      var underlyingIndex = index.UnderlyingIndexes[0];
      var baseQuery = BuildProviderQuery(underlyingIndex);
      SqlColumn typeIdColumn = baseQuery.Columns[Handlers.Domain.NameBuilder.TypeIdColumnName];
      SqlBinary inQuery = SqlFactory.In(typeIdColumn, SqlFactory.Array(typeIds));
      SqlSelect query = SqlFactory.Select(baseQuery.From);
      var atRootPolicy = index.ReflectedType.Hierarchy.Schema == InheritanceSchema.SingleTable;
      Dictionary<FieldInfo, string> lookup;
      if (atRootPolicy) {
        var rootIndex = index.ReflectedType.GetRoot().AffectedIndexes.First(i => i.IsPrimary);
        lookup = rootIndex.Columns.ToDictionary(c => c.Field, c => c.Name);
      }
      else
        lookup = underlyingIndex.Columns.ToDictionary(c => c.Field, c => c.Name);
      query.Columns.AddRange(index.Columns.Select(c => baseQuery.Columns[lookup[c.Field]]));
      query.Where = inQuery;

      return query;
    }

    #endregion

    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SqlCompiler(HandlerAccessor handlers)
      : base(handlers.Domain.Configuration.ConnectionInfo)
    {
      Handlers = handlers;
    }
  }
}