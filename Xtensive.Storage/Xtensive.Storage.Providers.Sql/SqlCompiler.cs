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
using Xtensive.Core.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Storage.Linq.Expressions.Visitors;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql.Expressions;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Helpers;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Sql.ValueTypeMapping;
using ColumnInfo=Xtensive.Storage.Model.ColumnInfo;
using IndexInfo=Xtensive.Storage.Model.IndexInfo;

namespace Xtensive.Storage.Providers.Sql
{
  /// <inheritdoc/>
  [Serializable]
  public class SqlCompiler : Compiler<SqlProvider>
  {
    private const string TableNamePattern = "Tmp_{0}";

    private readonly BooleanExpressionConverter booleanExpressionConverter;
    
    /// <summary>
    /// Gets the value type mapper.
    /// </summary>
    protected Driver Driver { get { return ((DomainHandler) Handlers.DomainHandler).Driver; } }

    /// <summary>
    /// Gets the provider info.
    /// </summary>
    protected ProviderInfo ProviderInfo { get { return Handlers.DomainHandler.ProviderInfo; } }

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
    public override SqlProvider ToCompatible(ExecutableProvider provider)
    {
      return Compile(new StoreProvider(provider));
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitAggregate(AggregateProvider provider)
    {
      var source = Compile(provider.Source);

//      SqlTable queryRef = source.PermanentReference;
//      var sqlSelect = SqlDml.Select(queryRef);
      var sqlSelect = ExtractSqlSelect(provider, source);

//      var columns = queryRef.Columns.ToList();
      var columns = sqlSelect.From.Columns.ToList();
      sqlSelect.Columns.Clear();

      for (int i = 0; i < provider.GroupColumnIndexes.Length; i++) {
        var columnIndex = provider.GroupColumnIndexes[i];
        var column = columns[columnIndex];
        sqlSelect.Columns.Add(column);
        sqlSelect.GroupBy.Add(column);
      }

      foreach (var column in provider.AggregateColumns) {
        SqlExpression expr = ProcessAggregate(source, columns, column);
        sqlSelect.Columns.Add(expr, column.Name);
      }

      return new SqlProvider(provider, sqlSelect, Handlers, source);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitAlias(AliasProvider provider)
    {
      var source = Compile(provider.Source);

      SqlSelect sourceSelect = source.Request.SelectStatement;
      var sqlSelect = sourceSelect.ShallowClone();
      var columns = sqlSelect.Columns.ToList();
      sqlSelect.Columns.Clear();
      for (int i = 0; i < columns.Count; i++) {
        var columnName = provider.Header.Columns[i].Name;
        columnName = ProcessAliasedName(columnName);
        var columnRef = columns[i] as SqlColumnRef;
        if (columnRef != null)
          sqlSelect.Columns.Add(SqlDml.ColumnRef(columnRef.SqlColumn, columnName));
        else
          sqlSelect.Columns.Add(columns[i], columnName);
      }
      return new SqlProvider(provider, sqlSelect, Handlers, source);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitCalculate(CalculateProvider provider)
    {
      var source = Compile(provider.Source);
      
      SqlSelect sqlSelect;
      if (provider.Source.Header.Length == 0) {
        SqlSelect sourceSelect = source.Request.SelectStatement;
        sqlSelect = sourceSelect.ShallowClone();
        sqlSelect.Columns.Clear();
      }
      else
        sqlSelect = ExtractSqlSelect(provider, source);

      var allBindings = EnumerableUtils<SqlQueryParameterBinding>.Empty;
      foreach (var column in provider.CalculatedColumns) {
        var result = ProcessExpression(column.Expression, sqlSelect.From.Columns.ToList());
        var predicate = result.First;
        var bindings = result.Second;
        if (!ProviderInfo.Supports(ProviderFeatures.FullFledgedBooleanExpressions) && (column.Type.StripNullable()==typeof(bool)))
          predicate = booleanExpressionConverter.BooleanToInt(predicate);
        sqlSelect.Columns.Add(predicate, column.Name);
        allBindings = allBindings.Concat(bindings);
      }

      return new SqlProvider(provider, sqlSelect, Handlers, allBindings, source);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitDistinct(DistinctProvider provider)
    {
      var source = Compile(provider.Source);

      var sourceSelect = source.Request.SelectStatement;
      SqlSelect query;
      if (sourceSelect.Limit != 0 || sourceSelect.Offset != 0) {
        var queryRef = SqlDml.QueryRef(sourceSelect);
        query = SqlDml.Select(queryRef);
        query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      }
      else
        query = sourceSelect.ShallowClone();
      query.Distinct = true;
      return new SqlProvider(provider, query, Handlers, source);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitFilter(FilterProvider provider)
    {
      var source = Compile(provider.Source);

      var query = ExtractSqlSelect(provider, source);

      var sourceColumns = query.From.Columns.ToList();
      var result = ProcessExpression(provider.Predicate, sourceColumns);
      var predicate = result.First;
      var bindings = result.Second;

      query.Where &= predicate;

      return new SqlProvider(provider, query, Handlers, bindings, source);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitIndex(IndexProvider provider)
    {
      var index = provider.Index.Resolve(Handlers.Domain.Model);
      SqlSelect query = BuildProviderQuery(index);
      return new SqlProvider(provider, query, Handlers);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitJoin(JoinProvider provider)
    {
      var left = Compile(provider.Left);
      var right = Compile(provider.Right);

      var leftShouldUseReference = ShouldUseQueryReference(provider, left);
      var leftTable = leftShouldUseReference 
        ? left.PermanentReference
        : left.Request.SelectStatement.From;
      var leftColumns = leftShouldUseReference
        ? leftTable.Columns.Cast<SqlColumn>()
        : left.Request.SelectStatement.Columns;

      var rightShouldUseReference = ShouldUseQueryReference(provider, right);
      var rightTable = rightShouldUseReference
        ? right.PermanentReference
        : right.Request.SelectStatement.From;
      var rightColumns = rightShouldUseReference
        ? rightTable.Columns.Cast<SqlColumn>()
        : right.Request.SelectStatement.Columns;

      var joinType = provider.JoinType == JoinType.LeftOuter 
        ? SqlJoinType.LeftOuterJoin 
        : SqlJoinType.InnerJoin;
      var joinExpression = provider.EqualIndexes
        .Select(pair => leftTable.Columns[pair.First] == rightTable.Columns[pair.Second])
        .Aggregate(null as SqlExpression, (expression, binary) => expression & binary);

      var joinedTable = SqlDml.Join(
        joinType,
        leftTable,
        rightTable,
        leftColumns.ToList(),
        rightColumns.ToList(),
        joinExpression);

      var query = SqlDml.Select(joinedTable);
      query.Columns.AddRange(joinedTable.AliasedColumns);
      return new SqlProvider(provider, query, Handlers, left, right);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitPredicateJoin(PredicateJoinProvider provider)
    {
      var left = Compile(provider.Left);
      var right = Compile(provider.Right);

      var leftShouldUseReference = ShouldUseQueryReference(provider, left);
      var leftTable = leftShouldUseReference
        ? left.PermanentReference
        : left.Request.SelectStatement.From;
      var leftColumns = leftShouldUseReference
        ? leftTable.Columns.Cast<SqlColumn>()
        : left.Request.SelectStatement.Columns;

      var rightShouldUseReference = ShouldUseQueryReference(provider, right);
      var rightTable = rightShouldUseReference
        ? right.PermanentReference
        : right.Request.SelectStatement.From;
      var rightColumns = rightShouldUseReference
        ? rightTable.Columns.Cast<SqlColumn>()
        : right.Request.SelectStatement.Columns;

      var joinType = provider.JoinType == JoinType.LeftOuter ? SqlJoinType.LeftOuterJoin : SqlJoinType.InnerJoin;

      var result = ProcessExpression(provider.Predicate, leftTable.Columns.ToList(), rightTable.Columns.ToList());
      var joinExpression = result.First;
      var bindings = result.Second;

      var joinedTable = SqlDml.Join(
        joinType, 
        leftTable,
        rightTable,
        leftColumns.ToList(),
        rightColumns.ToList(),
        joinExpression);

      var query = SqlDml.Select(joinedTable);
      query.Columns.AddRange(joinedTable.AliasedColumns);
      return new SqlProvider(provider, query, Handlers, bindings, left, right);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitRange(RangeProvider provider)
    {
      var compiledSource = Compile(provider.Source);

      var originalRange = provider.CompiledRange.Invoke();
      SqlSelect source = compiledSource.Request.SelectStatement;
      var query = (SqlSelect) source.ShallowClone();
      var keyColumns = provider.Header.Order.ToList();
      var rangeProvider = new SqlRangeProvider(provider, query, Handlers, compiledSource);
      var bindings = (HashSet<SqlQueryParameterBinding>) rangeProvider.Request.ParameterBindings;
      for (int i = 0; i < originalRange.EndPoints.First.Value.Count; i++) {
        var column = provider.Header.Columns[keyColumns[i].Key];
        TypeMapping typeMapping = Driver.GetTypeMapping(column.Type);
        int fieldIndex = i;
        var binding = new SqlQueryParameterBinding(() => rangeProvider.CurrentRange.EndPoints.First.Value.GetValue(fieldIndex), typeMapping);
        bindings.Add(binding);
        query.Where &= query.Columns[keyColumns[i].Key] == binding.ParameterReference;
      }
      return rangeProvider;
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitSeek(SeekProvider provider)
    {
      var compiledSource = Compile(provider.Source);

      SqlSelect source = compiledSource.Request.SelectStatement;
      var query = (SqlSelect) source.ShallowClone();
      var parameterBindings = new List<SqlQueryParameterBinding>();
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
        TypeMapping typeMapping = Driver.GetTypeMapping(column.Type);
        int index = i;
        var binding = new SqlQueryParameterBinding(() => provider.CompiledKey.Invoke().GetValue(index), typeMapping);
        parameterBindings.Add(binding);
        query.Where &= sqlColumn == binding.ParameterReference;
      }

      return new SqlProvider(provider, query, Handlers, parameterBindings, compiledSource);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitSelect(SelectProvider provider)
    {
      var compiledSource = Compile(provider.Source);
      SqlSelect query;

      if (provider.ColumnIndexes.Length == 0) {
        SqlSelect source = compiledSource.Request.SelectStatement;
        query = source.ShallowClone();
        query.Columns.Clear();
        query.Columns.Add(SqlDml.Null, "NULL");
      }
      else {
        query = ExtractSqlSelect(provider, compiledSource);
        var originalColumns = query.Columns.ToList();
        query.Columns.Clear();
        query.Columns.AddRange(provider.ColumnIndexes.Select(i => originalColumns[i]));
      }

      return new SqlProvider(provider, query, Handlers, compiledSource);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitSort(SortProvider provider)
    {
      var compiledSource = Compile(provider.Source);
      return new SqlProvider(provider, compiledSource.Request.SelectStatement, Handlers, compiledSource);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitStore(StoreProvider provider)
    {
      ExecutableProvider ex = null;
      var domainHandler = (DomainHandler) Handlers.DomainHandler;
      Schema schema = domainHandler.Schema;
      Table table;
      string tableName = string.Format(TableNamePattern, provider.Name);
      if (provider.Source != null) {
        ex = provider.Source as ExecutableProvider ?? Compile((CompilableProvider) provider.Source);
        table = provider.Scope == TemporaryDataScope.Global ? schema.CreateTable(tableName)
          : schema.CreateTemporaryTable(tableName);

        foreach (Column column in provider.Header.Columns) {
          SqlValueType svt;
          var mappedColumn = column as MappedColumn;
          if (mappedColumn != null) {
            ColumnInfo ci = mappedColumn.ColumnInfoRef.Resolve(domainHandler.Domain.Model);
            TypeMapping tm = Driver.GetTypeMapping(ci);
            svt = Driver.BuildValueType(ci);
          }
          else
            svt = Driver.BuildValueType(column.Type, null, null, null);
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
    protected override SqlProvider VisitSkip(SkipProvider provider)
    {
      var compiledSource = Compile(provider.Source);

      var queryRef = compiledSource.PermanentReference;
      var query = SqlDml.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      query.Offset = provider.Count();
      AddOrderByStatement(provider, query);
      return new SqlProvider(provider, query, Handlers, compiledSource);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitTake(TakeProvider provider)
    {
      var compiledSource = Compile(provider.Source);
      
      var query = ExtractSqlSelect(provider, compiledSource);
      var count = provider.Count();
      if (query.Limit == 0 || query.Limit > count)
        query.Limit = count;
      if (!(provider.Source is TakeProvider) && !(provider.Source is SkipProvider))
        AddOrderByStatement(provider, query);
      return new SqlProvider(provider, query, Handlers, compiledSource);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitApply(ApplyProvider provider)
    {
      bool processViaCrossApply;
      switch (provider.SequenceType) {
        case ApplySequenceType.All:
          // apply is required
          if (!ProviderInfo.Supports(ProviderFeatures.CrossApply))
            throw new NotSupportedException();
          processViaCrossApply = true;
          break;
        case ApplySequenceType.First:
        case ApplySequenceType.FirstOrDefault:
          // apply is prefered but is not required
          processViaCrossApply = ProviderInfo.Supports(ProviderFeatures.CrossApply);
          break;
        case ApplySequenceType.Single:
        case ApplySequenceType.SingleOrDefault:
          // apply is not allowed
          processViaCrossApply = false;
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      var left = Compile(provider.Left);
      var shouldUseQueryReference = true;
      var sourceSelect = left.Request.SelectStatement;

      if (processViaCrossApply) {
        shouldUseQueryReference = ShouldUseQueryReference(provider, left);
      }
      else {
        var calculatedColumnIndexes = sourceSelect.Columns
          .Select((c, i) => IsCalculatedColumn(c) ? i : -1)
          .Where(i => i >= 0)
          .ToList();
        var containsCalculatedColumns = calculatedColumnIndexes.Count > 0;
        var groupByIsUsed = sourceSelect.GroupBy.Count > 0;

        var usedOuterColumns = new List<int>();
        var visitor = new ApplyParameterAccessVisitor(provider.ApplyParameter, (mc, index) => {
           usedOuterColumns.Add(index);
           return mc;
          });
        var providerVisitor = new CompilableProviderVisitor((p, e) => visitor.Visit(e));
        providerVisitor.VisitCompilable(provider.Right);
        shouldUseQueryReference = usedOuterColumns.Any(calculatedColumnIndexes.Contains) || groupByIsUsed;
      }
      if (!shouldUseQueryReference)
        left = new SqlProvider(left, sourceSelect.From);

      using (OuterReferences.Add(provider.ApplyParameter, left)) {
        var right = Compile(provider.Right);

        var query = processViaCrossApply
          ? ProcessApplyViaCrossApply(provider, left, right, shouldUseQueryReference)
          : ProcessApplyViaSubqueries(provider, left, right, shouldUseQueryReference);

        return new SqlProvider(provider, query, Handlers, left, right);
      }
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitExistence(ExistenceProvider provider)
    {
      var source = Compile(provider.Source);

      SqlExpression existsExpression = SqlDml.Exists(source.Request.SelectStatement);
      if (!ProviderInfo.Supports(ProviderFeatures.FullFledgedBooleanExpressions))
        existsExpression = booleanExpressionConverter.BooleanToInt(existsExpression);
      var select = SqlDml.Select();
      select.Columns.Add(existsExpression, provider.ExistenceColumnName);

      return new SqlProvider(provider, select, Handlers, source);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitIntersect(IntersectProvider provider)
    {
      var left = Compile(provider.Left);
      var right = Compile(provider.Right);

      var leftSelect = left.Request.SelectStatement;
      var rightSelect = right.Request.SelectStatement;
      var result = SqlDml.Intersect(leftSelect, rightSelect);
      var queryRef = SqlDml.QueryRef(result);

      SqlSelect query = SqlDml.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());

      return new SqlProvider(provider, query, Handlers, left, right);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitExcept(ExceptProvider provider)
    {
      var left = Compile(provider.Left);
      var right = Compile(provider.Right);

      var leftSelect = left.Request.SelectStatement;
      var rightSelect = right.Request.SelectStatement;
      var result = SqlDml.Except(leftSelect, rightSelect);
      var queryRef = SqlDml.QueryRef(result);
      SqlSelect query = SqlDml.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());

      return new SqlProvider(provider, query, Handlers, left, right);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitConcat(ConcatProvider provider)
    {
      var left = Compile(provider.Left);
      var right = Compile(provider.Right);

      var leftSelect = left.Request.SelectStatement;
      var rightSelect = right.Request.SelectStatement;
      var result = SqlDml.UnionAll(leftSelect, rightSelect);
      var queryRef = SqlDml.QueryRef(result);
      SqlSelect query = SqlDml.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());

      return new SqlProvider(provider, query, Handlers, left, right);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitUnion(UnionProvider provider)
    {
      var left = Compile(provider.Left);
      var right = Compile(provider.Right);

      var leftSelect = left.Request.SelectStatement;
      var rightSelect = right.Request.SelectStatement;
      var result = SqlDml.Union(leftSelect, rightSelect);
      var queryRef = SqlDml.QueryRef(result);
      SqlSelect query = SqlDml.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());

      return new SqlProvider(provider, query, Handlers, left, right);
    }
    
    protected override SqlProvider VisitRowNumber(RowNumberProvider provider)
    {
      if (provider.Header.Order.Count==0)
        throw new InvalidOperationException(Strings.ExOrderingOfRecordsIsNotSpecifiedForRowNumberProvider);
      var source = Compile(provider.Source);

      var query = ExtractSqlSelect(provider, source);
      var rowNumber = SqlDml.RowNumber();
      query.Columns.Add(rowNumber, provider.Header.Columns.Last().Name);
      foreach (KeyValuePair<int, Direction> order in provider.Header.Order)
        rowNumber.OrderBy.Add(query.From.Columns[order.Key], order.Value==Direction.Positive);
      return new SqlProvider(provider, query, Handlers, source);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitLock(LockProvider provider)
    {
      var source = Compile(provider.Source);

      var query = source.Request.SelectStatement.ShallowClone();
      switch (provider.LockMode.Invoke()) {
      case LockMode.Shared:
        query.Lock = SqlLockType.Shared;
        break;
      case LockMode.Exclusive:
        query.Lock = SqlLockType.Exclusive;
        break;
      case LockMode.Update:
        query.Lock = SqlLockType.Update;
        break;
      }
      switch (provider.LockBehavior.Invoke()) {
      case LockBehavior.Wait:
        break;
      case LockBehavior.ThrowIfLocked:
        query.Lock |= SqlLockType.ThrowIfLocked;
        break;
      case LockBehavior.Skip:
        query.Lock |= SqlLockType.SkipLocked;
        break;
      }
      return new SqlProvider(provider, query, Handlers, source);
    }

    /// <summary>
    /// Translates <see cref="AggregateColumn"/> to corresponding <see cref="SqlExpression"/>.
    /// </summary>
    /// <param name="source">The source <see cref="SqlProvider">.</param>
    /// <param name="sourceColumns">The source columns.</param>
    /// <param name="aggregateColumn">The aggregate column.</param>
    /// <returns></returns>
    protected virtual SqlExpression ProcessAggregate(SqlProvider source,
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
    /// Processes the aliased.
    /// </summary>
    /// <param name="name">The name to process.</param>
    /// <returns>Processed name.</returns>
    protected virtual string ProcessAliasedName(string name)
    {
      return name;
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
      var table = domainHandler.Schema.Tables[index.ReflectedType.MappingName];
      var atRootPolicy = false;
      if (table == null) {
        table = domainHandler.Schema.Tables[index.ReflectedType.GetRoot().MappingName];
        atRootPolicy = true;
      }

      SqlSelect query;
      if (!atRootPolicy) {
        var tableRef = SqlDml.TableRef(table, index.Columns.Select(c => c.Name));
        query = SqlDml.Select(tableRef);
        query.Columns.AddRange(tableRef.Columns.Cast<SqlColumn>());
      }
      else {
        var root = index.ReflectedType.GetRoot().AffectedIndexes.First(i => i.IsPrimary);
        var lookup = root.Columns.ToDictionary(c => c.Field, c => c.Name);
        var tableRef = SqlDml.TableRef(table, index.Columns.Select(c => lookup[c.Field]));
        query = SqlDml.Select(tableRef);
        query.Columns.AddRange(tableRef.Columns.Cast<SqlColumn>());
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
          if (column.IsNullReference()) {
            var valueType = Driver.BuildValueType(columnInfo);
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
          result = SqlDml.QueryRef(baseQuery);
          rootTable = result;
          columns = rootTable.Columns.Cast<SqlColumn>();
        }
        else {
          var queryRef = SqlDml.QueryRef(baseQuery);
          SqlExpression joinExpression = null;
          for (int i = 0; i < keyColumnCount; i++) {
            var binary = (queryRef.Columns[i]==rootTable.Columns[i]);
            if (joinExpression.IsNullReference())
              joinExpression = binary;
            else
              joinExpression &= binary;
          }
          result = result.LeftOuterJoin(queryRef, joinExpression);
          columns = columns.Concat(queryRef.Columns.Skip(nonValueColumnsCount).Cast<SqlColumn>());
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
      var typeIdColumn = baseQuery.Columns[Handlers.Domain.NameBuilder.TypeIdColumnName];
      var inQuery = SqlDml.In(typeIdColumn, SqlDml.Array(typeIds));
      var query = SqlDml.Select(baseQuery.From);
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

    protected static void AddOrderByStatement(UnaryProvider provider, SqlSelect query)
    {
      foreach (KeyValuePair<int, Direction> pair in provider.Source.ExpectedOrder)
        query.OrderBy.Add(query.From.Columns[pair.Key], pair.Value==Direction.Positive);
    }

    private static bool IsCalculatedColumn(SqlColumn column)
    {
      if (column is SqlUserColumn)
        return true;
      var cRef = column as SqlColumnRef;
      if (!ReferenceEquals(null, cRef))
        return cRef.SqlColumn is SqlUserColumn;
      return false;
    }

    protected static bool ShouldUseQueryReference(CompilableProvider origin, SqlProvider compiledSource)
    {
      var sourceSelect = compiledSource.Request.SelectStatement;
      var calculatedColumnIndexes = sourceSelect.Columns
        .Select((c, i) => IsCalculatedColumn(c) ? i : -1)
        .Where(i => i >= 0)
        .ToList();
      var containsCalculatedColumns = calculatedColumnIndexes.Count > 0;
      var pagingIsUsed = sourceSelect.Limit != 0 || sourceSelect.Offset != 0;
      var groupByIsUsed = sourceSelect.GroupBy.Count > 0;
      var distinctIsUsed = sourceSelect.Distinct;
      var filterIsUsed = !sourceSelect.Where.IsNullReference();
      var columnCountIsNotSame = sourceSelect.From.Columns.Count != sourceSelect.Columns.Count;
      
      if (origin.Type == ProviderType.Filter) {
        var filterProvider = (FilterProvider)origin;
        var usedColumnIndexes = new TupleAccessGatherer().Gather(filterProvider.Predicate.Body);
        return pagingIsUsed || usedColumnIndexes.Any(calculatedColumnIndexes.Contains) || columnCountIsNotSame;
      }

      if (origin.Type == ProviderType.Select) {
        var selectProvider = (SelectProvider)origin;
        return containsCalculatedColumns && !calculatedColumnIndexes.All(ci => selectProvider.ColumnIndexes.Contains(ci));
      }

      if (origin.Type == ProviderType.RowNumber) {
        var usedColumnIndexes = origin.Header.Order.Select(o => o.Key);
        return pagingIsUsed || groupByIsUsed || distinctIsUsed || usedColumnIndexes.Any(calculatedColumnIndexes.Contains);
      }

      if (origin.Type == ProviderType.Calculate) {
        var calculateProvider = (CalculateProvider)origin;
        var columnGatherer = new TupleAccessGatherer();
        var usedColumnIndexes = new List<int>();
        foreach (var column in calculateProvider.CalculatedColumns)
          usedColumnIndexes.AddRange(
            columnGatherer.Gather(column.Expression.Body, column.Expression.Parameters[0]));

        return usedColumnIndexes.Any(calculatedColumnIndexes.Contains) || columnCountIsNotSame;
      }

      if (origin.Type == ProviderType.Take || origin.Type == ProviderType.Skip) {
        var sortProvider = origin.Sources[0] as SortProvider;
        var orderingOverCalculatedColumn = sortProvider != null && 
          sortProvider.ExpectedOrder
            .Select(order => order.Key)
            .Any(calculatedColumnIndexes.Contains);
        return distinctIsUsed || pagingIsUsed || groupByIsUsed || orderingOverCalculatedColumn;
      }

      if (origin.Type == ProviderType.Apply || origin.Type == ProviderType.Join || origin.Type == ProviderType.PredicateJoin)
        return containsCalculatedColumns || distinctIsUsed || pagingIsUsed || groupByIsUsed || filterIsUsed || columnCountIsNotSame;

      return containsCalculatedColumns || distinctIsUsed || pagingIsUsed || groupByIsUsed;
    }

    protected static SqlSelect ExtractSqlSelect(CompilableProvider origin, SqlProvider compiledSource)
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

    private Pair<SqlExpression, HashSet<SqlQueryParameterBinding>> ProcessExpression(LambdaExpression le, params List<SqlTableColumn>[] sourceColumns)
    {
      var processor = new ExpressionProcessor(le, this, Handlers, sourceColumns);
      var result = new Pair<SqlExpression, HashSet<SqlQueryParameterBinding>>(
        processor.Translate(),
        processor.Bindings);
      return result;
    }

    private static SqlSelect ProcessApplyViaSubqueries(ApplyProvider provider, SqlProvider left, SqlProvider right, bool shouldUseQueryReference)
    {
      var rightQuery = right.Request.SelectStatement;
      SqlSelect query;
      if (shouldUseQueryReference) {
        var leftTable = left.PermanentReference;
        query = SqlDml.Select(leftTable);
        query.Columns.AddRange(leftTable.Columns.Cast<SqlColumn>());
      }
      else
        query = left.Request.SelectStatement.ShallowClone();

      if (provider.Right.Type==ProviderType.Existence)
        query.Columns.Add(rightQuery.Columns[0]);
      else {
        for (int i = 0; i < rightQuery.Columns.Count; i++) {
          var subquery = rightQuery.ShallowClone();
          var columnRef = (SqlColumnRef) subquery.Columns[i];
          var column = columnRef.SqlColumn;
          subquery.Columns.Clear();
          subquery.Columns.Add(column);
          query.Columns.Add(subquery, provider.Right.Header.Columns[i].Name);
        }
      }
      return query;
    }

    private static SqlSelect ProcessApplyViaCrossApply(ApplyProvider provider, SqlProvider left, SqlProvider right, bool shouldUseQueryReference)
    {
      var leftShouldUseReference = ShouldUseQueryReference(provider, left);
      var leftTable = leftShouldUseReference
        ? left.PermanentReference
        : left.Request.SelectStatement.From;
      var leftColumns = leftShouldUseReference
        ? leftTable.Columns.Cast<SqlColumn>()
        : left.Request.SelectStatement.Columns;

      var rightShouldUseReference = ShouldUseQueryReference(provider, right);
      var rightTable = rightShouldUseReference
        ? right.PermanentReference
        : right.Request.SelectStatement.From;
      var rightColumns = rightShouldUseReference
        ? rightTable.Columns.Cast<SqlColumn>()
        : right.Request.SelectStatement.Columns;

      var joinType = provider.ApplyType==JoinType.LeftOuter
        ? SqlJoinType.LeftOuterApply
        : SqlJoinType.CrossApply;

      var joinedTable = SqlDml.Join(
        joinType,
        leftTable,
        rightTable,
        leftColumns.ToList(),
        rightColumns.ToList());

      var query = SqlDml.Select(joinedTable);
      query.Columns.AddRange(joinedTable.AliasedColumns);
      return query;
    }

    #endregion

    #region Not supported providers

    /// <inheritdoc/>
    protected override SqlProvider VisitTransfer(TransferProvider provider)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitRaw(RawProvider provider)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitRangeSet(RangeSetProvider provider)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitReindex(ReindexProvider provider)
    {
      throw new NotSupportedException();
    }

    #endregion

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SqlCompiler(HandlerAccessor handlers)
      : base(handlers.Domain.Configuration.ConnectionInfo)
    {
      Handlers = handlers;

      if (!handlers.DomainHandler.ProviderInfo.Supports(ProviderFeatures.FullFledgedBooleanExpressions))
        booleanExpressionConverter = new BooleanExpressionConverter(Driver);
    }
  }
}