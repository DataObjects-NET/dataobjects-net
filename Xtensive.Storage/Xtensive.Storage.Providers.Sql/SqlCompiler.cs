// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Vakhtina Elena
// Created:    2009.02.13

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql.Expressions;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Sql.ValueTypeMapping;

namespace Xtensive.Storage.Providers.Sql
{
  /// <inheritdoc/>
  [Serializable]
  public class SqlCompiler : RseCompiler
  {
    protected const string TableNamePattern = "Tmp_{0}";

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="bool"/> is natively supported by undelying RDBMS.
    /// </summary>
    protected bool BoolIsNativelySupported { get; set; }
    
    /// <summary>
    /// Gets the value type mapper.
    /// </summary>
    protected SqlValueTypeMapper ValueTypeMapper {
      get { return ((DomainHandler) Handlers.DomainHandler).ValueTypeMapper; }
    }

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
      return Compile(new StoreProvider(provider));
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitAggregate(AggregateProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source);
      var source = compiledSource as SqlProvider;
      if (source == null)
        return null;

      SqlTable queryRef = source.PermanentReference;
      SqlSelect sqlSelect = SqlDml.Select(queryRef);

      var columns = queryRef.Columns.ToList();
      sqlSelect.Columns.Clear();

      for (int i = 0; i < provider.GroupColumnIndexes.Length; i++) {
        var columnIndex = provider.GroupColumnIndexes[i];
        var column = columns[columnIndex];
        sqlSelect.Columns.Add(column);
        sqlSelect.GroupBy.Add(column);
      }

      foreach (var column in provider.AggregateColumns) {
        SqlExpression expr = TranslateAggregate(source, columns, column);
        sqlSelect.Columns.Add(expr, column.Name);
      }

      return new SqlProvider(provider, sqlSelect, Handlers, source);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitAlias(AliasProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source);
      var source = compiledSource as SqlProvider;
      if (source == null)
        return null;

      var sqlSelect = ShallowCopy(source.Request.SelectStatement);
      var columns = sqlSelect.Columns.ToList();
      sqlSelect.Columns.Clear();
      for (int i = 0; i < columns.Count; i++) {
        var columnName = provider.Header.Columns[i].Name;
        var columnRef = columns[i] as SqlColumnRef;
        if (columnRef != null)
          sqlSelect.Columns.Add(SqlDml.ColumnRef(columnRef.SqlColumn, columnName));
        else
          sqlSelect.Columns.Add(columns[i], columnName);
      }
      return new SqlProvider(provider, sqlSelect, Handlers, source);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitCalculate(CalculateProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source);
      var source = compiledSource as SqlProvider;
      if (source == null)
        return null;
      
      SqlSelect sqlSelect;
      if (provider.Source.Header.Length == 0) {
        sqlSelect = ShallowCopy(source.Request.SelectStatement);
        sqlSelect.Columns.Clear();
      }
      else {
        var queryRef = source.PermanentReference;
        sqlSelect = SqlDml.Select(queryRef);
        sqlSelect.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      }
      
      IEnumerable<SqlFetchParameterBinding> allBindings = EnumerableUtils<SqlFetchParameterBinding>.Empty;
      foreach (var column in provider.CalculatedColumns) {
        HashSet<SqlFetchParameterBinding> bindings;
        var predicate = TranslateExpression(column.Expression, out bindings, sqlSelect);
        if (!BoolIsNativelySupported && (column.Type==typeof(bool) || column.Type==typeof(bool?)))
          predicate = ExpressionProcessor.BooleanToInt(predicate);
        sqlSelect.Columns.Add(predicate, column.Name);
        allBindings = allBindings.Concat(bindings);
      }

      return new SqlProvider(provider, sqlSelect, Handlers, allBindings, source);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitDistinct(DistinctProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source);
      var source = compiledSource as SqlProvider;
      if (source == null)
        return null;

      var queryRef = SqlDml.QueryRef(source.Request.SelectStatement);
      var query = SqlDml.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      query.Distinct = true;
      return new SqlProvider(provider, query, Handlers, source);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitFilter(FilterProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source);
      var source = compiledSource as SqlProvider;
      if (source==null)
        return null;

      SqlSelect query = ExtractSqlSelect(source);

      HashSet<SqlFetchParameterBinding> bindings;
      var predicate = TranslateExpression(provider.Predicate, out bindings, query);
      if (predicate.NodeType==SqlNodeType.Literal) {
        var value = predicate as SqlLiteral<bool>;
        if (value!=null) {
          if (!value.Value)
            query.Where &= (1==0);
        }
      }
      else
        query.Where &= predicate;

      return new SqlProvider(provider, query, Handlers, bindings, source);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitIndex(IndexProvider provider)
    {
      var index = provider.Index.Resolve(Handlers.Domain.Model);
      SqlSelect query = BuildProviderQuery(index);
      return new SqlProvider(provider, query, Handlers);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitJoin(JoinProvider provider)
    {
      var left = GetCompiled(provider.Left) as SqlProvider;
      var right = GetCompiled(provider.Right) as SqlProvider;

      if (left == null || right == null)
        return null;
      var leftQuery = left.PermanentReference;
      var rightQuery = right.PermanentReference;
      var joinedTable = SqlDml.Join(
        provider.JoinType == JoinType.LeftOuter ? SqlJoinType.LeftOuterJoin : SqlJoinType.InnerJoin,
        leftQuery,
        rightQuery,
        provider.EqualIndexes
          .Select(pair => leftQuery.Columns[pair.First] == rightQuery.Columns[pair.Second])
          .Aggregate(null as SqlExpression, (expression, binary) => expression & binary)
        );

      SqlSelect query = SqlDml.Select(joinedTable);
      query.Columns.AddRange(leftQuery.Columns.Concat(rightQuery.Columns).Cast<SqlColumn>());
      return new SqlProvider(provider, query, Handlers, left, right);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitPredicateJoin(PredicateJoinProvider provider)
    {
      var left = GetCompiled(provider.Left) as SqlProvider;
      var right = GetCompiled(provider.Right) as SqlProvider;

      if (left == null || right == null)
        return null;

      var leftQuery = left.PermanentReference;
      var rightQuery = right.PermanentReference;
      HashSet<SqlFetchParameterBinding> bindings;

      var predicate = TranslateExpression(provider.Predicate, out bindings, leftQuery, rightQuery);
      var value = predicate as SqlLiteral<bool>;
        if (value!=null) {
          if (value.Value)
            predicate = SqlDml.Literal(1) == 1;
          else
            predicate = SqlDml.Literal(1) == 0;
        }
      var joinedTable = SqlDml.Join(
        provider.JoinType == JoinType.LeftOuter ? SqlJoinType.LeftOuterJoin : SqlJoinType.InnerJoin,
        leftQuery,
        rightQuery,
        predicate);

      SqlSelect query = SqlDml.Select(joinedTable);
      query.Columns.AddRange(leftQuery.Columns.Concat(rightQuery.Columns).Cast<SqlColumn>());
      return new SqlProvider(provider, query, Handlers, bindings, left, right);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitRange(RangeProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source) as SqlProvider;
      if (compiledSource == null)
        return null;

      var originalRange = provider.CompiledRange.Invoke();
      var query = ShallowCopy(compiledSource.Request.SelectStatement);
      var keyColumns = provider.Header.Order.ToList();
      var rangeProvider = new SqlRangeProvider(provider, query, Handlers, compiledSource);
      var bindings = (HashSet<SqlFetchParameterBinding>) rangeProvider.Request.ParameterBindings;
      for (int i = 0; i < originalRange.EndPoints.First.Value.Count; i++) {
        var column = provider.Header.Columns[keyColumns[i].Key];
        TypeMapping typeMapping = ((DomainHandler)Handlers.DomainHandler).ValueTypeMapper.GetTypeMapping(column.Type);
        int fieldIndex = i;
        var binding = new SqlFetchParameterBinding(() => rangeProvider.CurrentRange.EndPoints.First.Value.GetValue(fieldIndex), typeMapping);
        bindings.Add(binding);
        query.Where &= query.Columns[keyColumns[i].Key] == binding.ParameterReference;
      }
      return rangeProvider;
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitSeek(SeekProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source) as SqlProvider;
      if (compiledSource == null)
        return null;

      var query = ShallowCopy(compiledSource.Request.SelectStatement);
      var parameterBindings = new List<SqlFetchParameterBinding>();
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
        TypeMapping typeMapping = ValueTypeMapper.GetTypeMapping(column.Type);
        int index = i;
        var binding = new SqlFetchParameterBinding(() => provider.CompiledKey.Invoke().GetValue(index), typeMapping);
        parameterBindings.Add(binding);
        query.Where &= sqlColumn == binding.ParameterReference;
      }

      return new SqlProvider(provider, query, Handlers, parameterBindings, compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitSelect(SelectProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source) as SqlProvider;
      if (compiledSource == null)
        return null;
      SqlSelect query;

      if (provider.ColumnIndexes.Length == 0) {
        query = ShallowCopy(compiledSource.Request.SelectStatement);
        query.Columns.Clear();
        query.Columns.Add(SqlDml.Null, "NULL");
      }
      else {
        query = ShallowCopy(compiledSource.Request.SelectStatement);
        var originalColumns = query.Columns.ToList();
        query.Columns.Clear();
        query.Columns.AddRange(provider.ColumnIndexes.Select(i => originalColumns[i]));
      }

      return new SqlProvider(provider, query, Handlers, compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitSkip(SkipProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source) as SqlProvider;
      if (compiledSource == null)
        return null;

      var queryRef = compiledSource.PermanentReference;
      var query = SqlDml.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      query.Offset = provider.Count();
      AddOrderByStatement(provider, query);
      return new SqlProvider(provider, query, Handlers, compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitSort(SortProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source) as SqlProvider;
      if (compiledSource == null)
        return null;
      return new SqlProvider(provider, compiledSource.Request.SelectStatement, Handlers, compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitStore(StoreProvider provider)
    {
      ExecutableProvider ex = null;
      var domainHandler = (DomainHandler) Handlers.DomainHandler;
      Schema schema = domainHandler.Schema;
      Table table;
      string tableName = string.Format(TableNamePattern, provider.Name);
      if (provider.Source != null) {
        ex = provider.Source as ExecutableProvider ?? GetCompiled(provider.Source);
        table = provider.Scope == TemporaryDataScope.Global ? schema.CreateTable(tableName)
          : schema.CreateTemporaryTable(tableName);

        foreach (Column column in provider.Header.Columns) {
          SqlValueType svt;
          var mappedColumn = column as MappedColumn;
          if (mappedColumn != null) {
            ColumnInfo ci = mappedColumn.ColumnInfoRef.Resolve(domainHandler.Domain.Model);
            TypeMapping tm = domainHandler.ValueTypeMapper.GetTypeMapping(ci);
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

      SqlTableRef tr = SqlDml.TableRef(table);
      SqlSelect query = SqlDml.Select(tr);
      foreach (SqlTableColumn column in tr.Columns)
        query.Columns.Add(column);
      schema.Tables.Remove(table);

      return new SqlStoreProvider(provider, query, Handlers, ex, table);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitTake(TakeProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source) as SqlProvider;
      if (compiledSource == null)
        return null;

      //var query = ShallowCopy(compiledSource.Request.SelectStatement);
      var query = ExtractSqlSelect(compiledSource);
      var count = provider.Count();
      if (query.Top == 0 || query.Top > count)
        query.Top = count;
      if(!(provider.Source is TakeProvider) && !(provider.Source is SkipProvider))
        AddOrderByStatement(provider, query);
      return new SqlProvider(provider, query, Handlers, compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitApply(ApplyProvider provider)
    {
      var left = GetCompiled(provider.Left) as SqlProvider;
      var right = GetCompiled(provider.Right) as SqlProvider;

      if (left == null || right == null)
        return null;

      var leftQuery = left.PermanentReference;
      var rightQuery = right.Request.SelectStatement;

      var select = SqlDml.Select(leftQuery);
      if (left.Origin.Header.Length > 0)
        select.Columns.AddRange(leftQuery.Columns.Cast<SqlColumn>());

      if (provider.SequenceType == ApplySequenceType.All)
        return null;

      if (provider.Right.Type == ProviderType.Existence)
        select.Columns.Add(rightQuery.Columns[0]);
      else {
        for (int i = 0; i < rightQuery.Columns.Count; i++) {
          var subquery = ShallowCopy(rightQuery);
          var columnRef = (SqlColumnRef) subquery.Columns[i];
          var column = columnRef.SqlColumn;
          subquery.Columns.Clear();
          subquery.Columns.Add(column);
          select.Columns.Add(subquery, provider.Right.Header.Columns[i].Name);
        }
      }

      return new SqlProvider(provider, select, Handlers, left, right);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitExistence(ExistenceProvider provider)
    {
      var source = GetCompiled(provider.Source) as SqlProvider;

      if (source == null)
        return null;

      SqlExpression existsExpression = SqlDml.Exists(source.Request.SelectStatement);
      if (!BoolIsNativelySupported)
        existsExpression = ExpressionProcessor.BooleanToInt(existsExpression);
      var select = SqlDml.Select();
      select.Columns.Add(existsExpression, provider.ExistenceColumnName);

      return new SqlProvider(provider, select, Handlers, source);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitIntersect(IntersectProvider provider)
    {
      var left = GetCompiled(provider.Left) as SqlProvider;
      var right = GetCompiled(provider.Right) as SqlProvider;
      if (left == null || right == null)
        return null;

      var leftSelect = left.Request.SelectStatement;
      var rightSelect = right.Request.SelectStatement;
      
      var result = SqlDml.Intersect(
        leftSelect,
        rightSelect);

      var queryRef = SqlDml.QueryRef(result);
      SqlSelect query = SqlDml.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());

      return new SqlProvider(provider, query, Handlers, left, right);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitExcept(ExceptProvider provider)
    {
      var left = GetCompiled(provider.Left) as SqlProvider;
      var right = GetCompiled(provider.Right) as SqlProvider;
      if (left == null || right == null)
        return null;

      var leftSelect = left.Request.SelectStatement;
      var rightSelect = right.Request.SelectStatement;

      var result = SqlDml.Except(
        leftSelect,
        rightSelect);

      var queryRef = SqlDml.QueryRef(result);
      SqlSelect query = SqlDml.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());

      return new SqlProvider(provider, query, Handlers, left, right);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitConcat(ConcatProvider provider)
    {
      var left = GetCompiled(provider.Left) as SqlProvider;
      var right = GetCompiled(provider.Right) as SqlProvider;
      if (left == null || right == null)
        return null;

      var leftSelect = left.Request.SelectStatement;
      var rightSelect = right.Request.SelectStatement;

      var result = SqlDml.UnionAll(leftSelect, rightSelect);

      var queryRef = SqlDml.QueryRef(result);
      SqlSelect query = SqlDml.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());

      return new SqlProvider(provider, query, Handlers, left, right);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitUnion(UnionProvider provider)
    {
      var left = GetCompiled(provider.Left) as SqlProvider;
      var right = GetCompiled(provider.Right) as SqlProvider;
      if (left == null || right == null)
        return null;

      var leftSelect = left.Request.SelectStatement;
      var rightSelect = right.Request.SelectStatement;

      var result = SqlDml.Union(
        leftSelect,
        rightSelect);

      var queryRef = SqlDml.QueryRef(result);
      SqlSelect query = SqlDml.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());

      return new SqlProvider(provider, query, Handlers, left, right);
    }

    /// <summary>
    /// Translates <see cref="LambdaExpression"/> to SQL DOM tree.
    /// </summary>
    /// <param name="le">An expression to translate</param>
    /// <param name="parameterBindings">Parameter bindings generated during translation.</param>
    /// <param name="selects">Select statements associated with <paramref name="le"/> parameters.</param>
    /// <returns></returns>
    protected virtual SqlExpression TranslateExpression(LambdaExpression le,
      out HashSet<SqlFetchParameterBinding> parameterBindings, params SqlSelect[] selects)
    {
      var translator = new ExpressionProcessor(le, this, Handlers, !BoolIsNativelySupported, selects);
      var result = translator.Translate();
      parameterBindings = translator.Bindings;
      return result;
    }

    /// <summary>
    /// Translates <see cref="LambdaExpression"/> to SQL DOM tree.
    /// </summary>
    /// <param name="le">An expression to translate</param>
    /// <param name="parameterBindings">Parameter bindings generated during translation.</param>
    /// <param name="queryRefs"><see cref="SqlQueryRef"/> associated with <paramref name="le"/> 
    /// parameters.</param>
    /// <returns></returns>
    protected virtual SqlExpression TranslateExpression(LambdaExpression le,
      out HashSet<SqlFetchParameterBinding> parameterBindings, params SqlQueryRef[] queryRefs)
    {
      var translator = new ExpressionProcessor(le, this, Handlers, !BoolIsNativelySupported, queryRefs);
      var result = translator.Translate();
      parameterBindings = translator.Bindings;
      return result;
    }

    /// <summary>
    /// Translates <see cref="AggregateColumn"/> to corresponding <see cref="SqlExpression"/>.
    /// </summary>
    /// <param name="source">The source <see cref="SqlProvider">.</param>
    /// <param name="sourceColumns">The source columns.</param>
    /// <param name="aggregateColumn">The aggregate column.</param>
    /// <returns></returns>
    protected virtual SqlExpression TranslateAggregate(SqlProvider source,
      List<SqlTableColumn> sourceColumns, AggregateColumn aggregateColumn)
    {
      switch (aggregateColumn.AggregateType) {
        case AggregateType.Avg:
          return SqlDml.Avg(sourceColumns[aggregateColumn.SourceIndex]);
        case AggregateType.Count:
          return SqlDml.Count(SqlDml.Asterisk);
        case AggregateType.Max:
          return SqlDml.Max(sourceColumns[aggregateColumn.SourceIndex]);
        case AggregateType.Min:
          return SqlDml.Min(sourceColumns[aggregateColumn.SourceIndex]);
        case AggregateType.Sum:
          return SqlDml.Sum(sourceColumns[aggregateColumn.SourceIndex]);
        default:
          throw new ArgumentException();
      }
    }

    /// <summary>
    /// Create shallow copy of the specified <see cref="SqlSelect"/>.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The shallow copy of <paramref name="source"/>.</returns>
    protected static SqlSelect ShallowCopy(SqlSelect source)
    {
      var result = ReferenceEquals(source.From, null) 
        ? SqlDml.Select() 
        : SqlDml.Select(source.From);
      result.Columns.AddRange(source.Columns);
      result.Distinct = source.Distinct;
      result.GroupBy.AddRange(source.GroupBy);
      result.Having = source.Having;
      result.Offset = source.Offset;
      result.Top = source.Top;
      foreach (var order in source.OrderBy)
        result.OrderBy.Add(order);
      foreach (var hint in source.Hints)
        result.Hints.Add(hint);
      result.Where = source.Where;
      return result;
    }

    #region Private methods

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

      SqlTableRef tableRef = SqlDml.TableRef(table);
      SqlSelect query = SqlDml.Select(tableRef);
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
          if (SqlExpression.IsNull(column)) {
            var valueType = ValueTypeMapper.BuildSqlValueType(columnInfo);
            select.Columns.Insert(i, SqlDml.Cast(SqlDml.Null, valueType), columnInfo.Name);
          }
          i++;
        }
        if (result == null)
          result = select;
        else
          result = result.Union(select);
      }

      var unionRef = SqlDml.QueryRef(result);
      SqlSelect query = SqlDml.Select(unionRef);
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
          result = SqlExpression.IsNull(baseQuery.Where) ? baseQuery.From : SqlDml.QueryRef(baseQuery);
          rootTable = result;
          columns = rootTable.Columns.Cast<SqlColumn>();
        }
        else {
          var queryRef = SqlExpression.IsNull(baseQuery.Where) ? baseQuery.From : SqlDml.QueryRef(baseQuery);
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

      SqlSelect query = SqlDml.Select(result);
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
      SqlBinary inQuery = SqlDml.In(typeIdColumn, SqlDml.Array(typeIds));
      SqlSelect query = SqlDml.Select(baseQuery.From);
      var atRootPolicy = index.ReflectedType.Hierarchy.Schema==InheritanceSchema.SingleTable;
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
    
    private static void AddOrderByStatement(UnaryProvider provider, SqlSelect query)
    {
      foreach (KeyValuePair<int, Direction> pair in provider.Source.ExpectedOrder)
        query.OrderBy.Add(query.Columns[pair.Key], pair.Value==Direction.Positive);
    }

    private static SqlSelect ExtractSqlSelect(SqlProvider source)
    {
      var containsCalculatedColumns = source.Request.SelectStatement.Columns.Any(c => {
        if (c is SqlUserColumn)
          return true;
        var cRef = c as SqlColumnRef;
        if (!ReferenceEquals(null, cRef))
          return cRef.SqlColumn is SqlUserColumn;
        return false;
      });

      SqlSelect query;
      if (containsCalculatedColumns) {
        var queryRef = source.PermanentReference;
        query = SqlDml.Select(queryRef);
        query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      }
      else
        query = ShallowCopy(source.Request.SelectStatement);
      return query;
    }

    #endregion
    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SqlCompiler(HandlerAccessor handlers, BindingCollection<object, ExecutableProvider> compiledSources)
      : base(handlers.Domain.Configuration.ConnectionInfo, compiledSources)
    {
      Handlers = handlers;
      BoolIsNativelySupported = true;
    }
  }
}