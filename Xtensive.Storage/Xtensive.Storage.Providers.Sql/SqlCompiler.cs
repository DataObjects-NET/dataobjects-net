// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Vakhtina Elena
// Created:    2009.02.13

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Storage.Providers.Sql.Expressions;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql
{
  /// <inheritdoc/>
  [Serializable]
  public partial class SqlCompiler : Compiler<SqlProvider>
  {
    private readonly BooleanExpressionConverter booleanExpressionConverter;
    private readonly Dictionary<SqlColumnStub, SqlExpression> stubColumnMap;

    /// <summary>
    /// Gets the SQL domain handler.
    /// </summary>
    protected DomainHandler DomainHandler { get { return ((DomainHandler) Handlers.DomainHandler); } }

    /// <summary>
    /// Gets the SQL driver.
    /// </summary>
    protected Driver Driver { get { return DomainHandler.Driver; } }

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
        var column = columns[i];
        var columnRef = column as SqlColumnRef;
        var columnStub = column as SqlColumnStub;
        if (!ReferenceEquals(null, columnRef))
          sqlSelect.Columns.Add(SqlDml.ColumnRef(columnRef.SqlColumn, columnName));
        else if (!ReferenceEquals(null, columnStub))
          sqlSelect.Columns.Add(columnStub);
        else
          sqlSelect.Columns.Add(column, columnName);
      }
      return CreateProvider(sqlSelect, provider, source);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitCalculate(CalculateProvider provider)
    {
      var source = Compile(provider.Source);

      SqlSelect sqlSelect;
      if (provider.Source.Header.Length==0) {
        SqlSelect sourceSelect = source.Request.SelectStatement;
        sqlSelect = sourceSelect.ShallowClone();
        sqlSelect.Columns.Clear();
      }
      else
        sqlSelect = ExtractSqlSelect(provider, source);

      var sourceColumns = ExtractColumnExpressions(sqlSelect, provider);
      var allBindings = EnumerableUtils<QueryParameterBinding>.Empty;
      foreach (var column in provider.CalculatedColumns) {
        var result = ProcessExpression(column.Expression, sourceColumns);
        var predicate = result.First;
        var bindings = result.Second;
        if (column.Type.StripNullable()==typeof (bool))
          predicate = GetBooleanColumnExpression(predicate);
        AddInlinableColumn(provider, sqlSelect, column.Name, predicate);
        allBindings = allBindings.Concat(bindings);
      }
      return CreateProvider(sqlSelect, allBindings, provider, source);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitDistinct(DistinctProvider provider)
    {
      var source = Compile(provider.Source);

      var sourceSelect = source.Request.SelectStatement;
      SqlSelect query;
      if (!sourceSelect.Limit.IsNullReference() || !sourceSelect.Offset.IsNullReference()) {
        var queryRef = SqlDml.QueryRef(sourceSelect);
        query = SqlDml.Select(queryRef);
        query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      }
      else
        query = sourceSelect.ShallowClone();
      query.Distinct = true;
      return CreateProvider(query, provider, source);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitFilter(FilterProvider provider)
    {
      var source = Compile(provider.Source);

      var query = ExtractSqlSelect(provider, source);

      var sourceColumns = ExtractColumnExpressions(query, provider);
      var result = ProcessExpression(provider.Predicate, sourceColumns);
      var predicate = result.First;
      var bindings = result.Second;

      query.Where &= predicate;

      return CreateProvider(query, bindings, provider, source);
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
      var leftExpressions = leftShouldUseReference
        ? leftTable.Columns.Cast<SqlExpression>().ToList()
        : ExtractColumnExpressions(left.Request.SelectStatement, provider.Left);

      var rightShouldUseReference = ShouldUseQueryReference(provider, right);
      var rightTable = rightShouldUseReference
        ? right.PermanentReference
        : right.Request.SelectStatement.From;
      var rightColumns = rightShouldUseReference
        ? rightTable.Columns.Cast<SqlColumn>()
        : right.Request.SelectStatement.Columns;
      var rightExpressions = rightShouldUseReference
        ? rightTable.Columns.Cast<SqlExpression>().ToList()
        : ExtractColumnExpressions(right.Request.SelectStatement, provider.Right);

      var joinType = provider.JoinType==JoinType.LeftOuter
        ? SqlJoinType.LeftOuterJoin
        : SqlJoinType.InnerJoin;
      var joinExpression = provider.EqualIndexes
        .Select(pair => leftExpressions[pair.First]==rightExpressions[pair.Second])
        .Aggregate(null as SqlExpression, (expression, binary) => expression & binary);

      var joinedTable = SqlDml.Join(
        joinType,
        leftTable,
        rightTable,
        leftColumns.ToList(),
        rightColumns.ToList(),
        joinExpression);

      var query = SqlDml.Select(joinedTable);
      if (!leftShouldUseReference)
        query.Where &= left.Request.SelectStatement.Where;
      if (!rightShouldUseReference)
        query.Where &= right.Request.SelectStatement.Where;
      query.Columns.AddRange(joinedTable.AliasedColumns);
      return CreateProvider(query, provider, left, right);
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
      var leftExpressions = leftShouldUseReference
        ? leftTable.Columns.Cast<SqlExpression>().ToList()
        : ExtractColumnExpressions(left.Request.SelectStatement, provider.Left);

      var rightShouldUseReference = ShouldUseQueryReference(provider, right);
      var rightTable = rightShouldUseReference
        ? right.PermanentReference
        : right.Request.SelectStatement.From;
      var rightColumns = rightShouldUseReference
        ? rightTable.Columns.Cast<SqlColumn>()
        : right.Request.SelectStatement.Columns;
      var rightExpressions = rightShouldUseReference
        ? rightTable.Columns.Cast<SqlExpression>().ToList()
        : ExtractColumnExpressions(right.Request.SelectStatement, provider.Right);


      var joinType = provider.JoinType==JoinType.LeftOuter ? SqlJoinType.LeftOuterJoin : SqlJoinType.InnerJoin;

      var result = ProcessExpression(provider.Predicate, leftExpressions, rightExpressions);
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
      if (!leftShouldUseReference)
        query.Where &= left.Request.SelectStatement.Where;
      if (!rightShouldUseReference)
        query.Where &= right.Request.SelectStatement.Where;
      query.Columns.AddRange(joinedTable.AliasedColumns);
      return CreateProvider(query, bindings, provider, left, right);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitSeek(SeekProvider provider)
    {
      var compiledSource = Compile(provider.Source);

      SqlSelect source = compiledSource.Request.SelectStatement;
      var query = source.ShallowClone();
      var parameterBindings = new List<QueryParameterBinding>();
      var typeIdColumnName = Handlers.NameBuilder.TypeIdColumnName;
      Func<KeyValuePair<int, Direction>, bool> filterNonTypeId =
        pair => ((MappedColumn) provider.Header.Columns[pair.Key]).ColumnInfoRef.ColumnName!=typeIdColumnName;
      var keyColumns = provider.Header.Order
        .Where(filterNonTypeId)
        .ToList();

      for (int i = 0; i < keyColumns.Count; i++) {
        int columnIndex = keyColumns[i].Key;
        var sqlColumn = query.Columns[columnIndex];
        var column = provider.Header.Columns[columnIndex];
        TypeMapping typeMapping = Driver.GetTypeMapping(column.Type);
        int index = i;
        var binding = new QueryParameterBinding(() => provider.CompiledKey.Invoke().GetValue(index), typeMapping);
        parameterBindings.Add(binding);
        query.Where &= sqlColumn==binding.ParameterReference;
      }

      return CreateProvider(query, parameterBindings, provider, compiledSource);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitSelect(SelectProvider provider)
    {
      var compiledSource = Compile(provider.Source);

      SqlSelect query = ExtractSqlSelect(provider, compiledSource);
      var originalColumns = query.Columns.ToList();
      query.Columns.Clear();
      query.Columns.AddRange(provider.ColumnIndexes.Select(i => originalColumns[i]));

      return CreateProvider(query, provider, compiledSource);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitSort(SortProvider provider)
    {
      var compiledSource = Compile(provider.Source);

      var query = ExtractSqlSelect(provider, compiledSource);
      var rootSelectProvider = RootProvider as SelectProvider;
      var currentIsRoot = RootProvider==provider;

      if (currentIsRoot || (rootSelectProvider != null && rootSelectProvider.Source == provider)) {
        query.OrderBy.Clear();
        if (currentIsRoot) {
          foreach (var pair in provider.Header.Order)
            query.OrderBy.Add(query.Columns[pair.Key], pair.Value == Direction.Positive);
        }
        else {
          var columnExpressions = ExtractColumnExpressions(query, provider);
          var shouldUseColumnPosition = provider.Header.Order.Any(o => o.Key >= columnExpressions.Count);
          if (shouldUseColumnPosition)
            foreach (var pair in provider.Header.Order) {
              if (pair.Key >= columnExpressions.Count)
                query.OrderBy.Add(pair.Key + 1, pair.Value == Direction.Positive);
              else
                query.OrderBy.Add(columnExpressions[pair.Key], pair.Value == Direction.Positive);
            }
          else
            foreach (var pair in provider.Header.Order)
              query.OrderBy.Add(columnExpressions[pair.Key], pair.Value == Direction.Positive);
        }
      }
      return CreateProvider(query, provider, compiledSource);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitStore(StoreProvider provider)
    {
      var source =
        provider.Source as ExecutableProvider
          ?? (provider.Source is RawProvider
            ? (ExecutableProvider) (new Rse.Providers.Executable.RawProvider((RawProvider) provider.Source))
            : Compile((CompilableProvider) provider.Source));
      if (provider.Scope!=TemporaryDataScope.Enumeration)
        throw new NotSupportedException(string.Format(Strings.ExXIsNotSupported, provider.Scope));
      var columnNames = provider.Header.Columns.Select(column => column.Name).ToArray();
      var descriptor = DomainHandler.TemporaryTableManager
        .BuildDescriptor(provider.Name, provider.Header.TupleDescriptor, columnNames);
      var request = new QueryRequest(descriptor.QueryStatement, descriptor.TupleDescriptor, RequestOptions.Empty);
      return new SqlStoreProvider(Handlers, request, descriptor, provider, source);
    }
    
    /// <inheritdoc/>
    protected override SqlProvider VisitExistence(ExistenceProvider provider)
    {
      var source = Compile(provider.Source);

      var query = source.Request.SelectStatement.ShallowClone();
      query.Columns.Clear();
      query.Columns.Add(query.Asterisk);
      query.OrderBy.Clear();
      query.GroupBy.Clear();
      SqlExpression existsExpression = SqlDml.Exists(query);
      existsExpression = GetBooleanColumnExpression(existsExpression);
      var select = SqlDml.Select();
      select.Columns.Add(existsExpression, provider.ExistenceColumnName);

      return CreateProvider(select, provider, source);
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

      return CreateProvider(query, provider, left, right);
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

      return CreateProvider(query, provider, left, right);
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

      return CreateProvider(query, provider, left, right);
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

      return CreateProvider(query, provider, left, right);
    }

    protected override SqlProvider VisitRowNumber(RowNumberProvider provider)
    {
      var directionCollection = provider.Header.Order;
      if (directionCollection.Count == 0)
        directionCollection = new DirectionCollection<int>(1);
//        throw new InvalidOperationException(Strings.ExOrderingOfRecordsIsNotSpecifiedForRowNumberProvider);
      var source = Compile(provider.Source);

      var query = ExtractSqlSelect(provider, source);
      var rowNumber = SqlDml.RowNumber();
      query.Columns.Add(rowNumber, provider.Header.Columns.Last().Name);
      var columns = ExtractColumnExpressions(query, provider);
      foreach (var order in directionCollection)
        rowNumber.OrderBy.Add(columns[order.Key], order.Value==Direction.Positive);
      return CreateProvider(query, provider, source);
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
      return CreateProvider(query, provider, source);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SqlCompiler(HandlerAccessor handlers)
      : base(handlers.DomainHandler.StorageLocation)
    {
      Handlers = handlers;

      if (!handlers.DomainHandler.ProviderInfo.Supports(ProviderFeatures.FullFeaturedBooleanExpressions))
        booleanExpressionConverter = new BooleanExpressionConverter(Driver);

      stubColumnMap = new Dictionary<SqlColumnStub, SqlExpression>();
    }
  }
}