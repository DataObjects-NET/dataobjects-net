// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Vakhtina Elena
// Created:    2009.02.13

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Compilation;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  public partial class SqlCompiler : Compiler<SqlProvider>
  {
    private readonly BooleanExpressionConverter booleanExpressionConverter;
    private readonly Dictionary<SqlColumnStub, SqlExpression> stubColumnMap;
    private readonly ProviderInfo providerInfo;
    private readonly bool temporaryTablesSupported;
    private readonly HashSet<Column> rootColumns = new HashSet<Column>();

    private bool anyTemporaryTablesRequired;

    /// <summary>
    /// Gets model mapping.
    /// </summary>
    protected ModelMapping Mapping { get; private set; }

    /// <summary>
    /// Gets type identifier registry.
    /// </summary>
    protected TypeIdRegistry TypeIdRegistry { get; private set; }

    /// <summary>
    /// Gets the SQL domain handler.
    /// </summary>
    protected DomainHandler DomainHandler { get { return Handlers.DomainHandler; } }

    /// <summary>
    /// Gets the SQL driver.
    /// </summary>
    protected StorageDriver Driver { get { return Handlers.StorageDriver; } }

    /// <summary>
    /// Gets the <see cref="HandlerAccessor"/> object providing access to available storage handlers.
    /// </summary>
    protected HandlerAccessor Handlers { get; private set; }

    /// <summary>
    /// Gets collection of outer references.
    /// </summary>
    protected BindingCollection<ApplyParameter, Pair<SqlProvider, bool>> OuterReferences { get; private set; }

    /// <inheritdoc/>
    protected override SqlProvider VisitAlias(AliasProvider provider)
    {
      var source = Compile(provider.Source);

      SqlSelect sourceSelect = source.Request.Statement;
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
        SqlSelect sourceSelect = source.Request.Statement;
        sqlSelect = sourceSelect.ShallowClone();
        sqlSelect.Columns.Clear();
      }
      else
        sqlSelect = ExtractSqlSelect(provider, source);

      var sourceColumns = ExtractColumnExpressions(sqlSelect);
      var allBindings = EnumerableUtils<QueryParameterBinding>.Empty;
      foreach (var column in provider.CalculatedColumns) {
        var result = ProcessExpression(column.Expression, sourceColumns);
        var predicate = result.First;
        var bindings = result.Second;
        if (column.Type.StripNullable()==typeof (bool))
          predicate = GetBooleanColumnExpression(predicate);
        AddInlinableColumn(provider, column, sqlSelect, predicate);
        allBindings = allBindings.Concat(bindings);
      }
      return CreateProvider(sqlSelect, allBindings, provider, source);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitDistinct(DistinctProvider provider)
    {
      var source = Compile(provider.Source);

      var sourceSelect = source.Request.Statement;
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

      var sourceColumns = ExtractColumnExpressions(query);
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

      // SQLite does not allow certain join combinations
      // Any right part of join expression should not be join itself
      // See IssueA363_WrongInnerJoin for example of such query

      var strictJoinWorkAround =
        providerInfo.Supports(ProviderFeatures.StrictJoinSyntax)
        && right.Request.Statement.From is SqlJoinedTable;

      var leftShouldUseReference = ShouldUseQueryReference(provider, left);
      var leftTable = leftShouldUseReference
        ? left.PermanentReference
        : left.Request.Statement.From;
      var leftColumns = leftShouldUseReference
        ? leftTable.Columns.Cast<SqlColumn>()
        : left.Request.Statement.Columns;
      var leftExpressions = leftShouldUseReference
        ? leftTable.Columns.Cast<SqlExpression>().ToList()
        : ExtractColumnExpressions(left.Request.Statement);

      var rightShouldUseReference = strictJoinWorkAround || ShouldUseQueryReference(provider, right);
      var rightTable = rightShouldUseReference
        ? right.PermanentReference
        : right.Request.Statement.From;
      var rightColumns = rightShouldUseReference
        ? rightTable.Columns.Cast<SqlColumn>()
        : right.Request.Statement.Columns;
      var rightExpressions = rightShouldUseReference
        ? rightTable.Columns.Cast<SqlExpression>().ToList()
        : ExtractColumnExpressions(right.Request.Statement);

      var joinType = provider.JoinType==JoinType.LeftOuter
        ? SqlJoinType.LeftOuterJoin
        : SqlJoinType.InnerJoin;

      SqlExpression joinExpression = null;
      for (var i = 0; i < provider.EqualIndexes.Count(); ++i) {
        var leftExpression = leftExpressions[provider.EqualIndexes[i].First];
        var rightExpression = rightExpressions[provider.EqualIndexes[i].Second];
        joinExpression &= GetJoinExpression(leftExpression, rightExpression, provider, i);
      }

      var joinedTable = SqlDml.Join(
        joinType,
        leftTable,
        rightTable,
        leftColumns.ToList(),
        rightColumns.ToList(),
        joinExpression);

      var query = SqlDml.Select(joinedTable);
      if (!leftShouldUseReference)
        query.Where &= left.Request.Statement.Where;
      if (!rightShouldUseReference)
        query.Where &= right.Request.Statement.Where;
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
        : left.Request.Statement.From;
      var leftColumns = leftShouldUseReference
        ? leftTable.Columns.Cast<SqlColumn>()
        : left.Request.Statement.Columns;
      var leftExpressions = leftShouldUseReference
        ? leftTable.Columns.Cast<SqlExpression>().ToList()
        : ExtractColumnExpressions(left.Request.Statement);

      var rightShouldUseReference = ShouldUseQueryReference(provider, right);
      var rightTable = rightShouldUseReference
        ? right.PermanentReference
        : right.Request.Statement.From;
      var rightColumns = rightShouldUseReference
        ? rightTable.Columns.Cast<SqlColumn>()
        : right.Request.Statement.Columns;
      var rightExpressions = rightShouldUseReference
        ? rightTable.Columns.Cast<SqlExpression>().ToList()
        : ExtractColumnExpressions(right.Request.Statement);


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
        query.Where &= left.Request.Statement.Where;
      if (!rightShouldUseReference)
        query.Where &= right.Request.Statement.Where;
      query.Columns.AddRange(joinedTable.AliasedColumns);
      return CreateProvider(query, bindings, provider, left, right);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitSeek(SeekProvider provider)
    {
      var compiledSource = Compile(provider.Source);

      SqlSelect source = compiledSource.Request.Statement;
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
        var binding = new QueryParameterBinding(typeMapping, GetSeekKeyElementAccessor(provider.Key, i));
        parameterBindings.Add(binding);
        query.Where &= sqlColumn==binding.ParameterReference;
      }

      return CreateProvider(query, parameterBindings, provider, compiledSource);
    }

    private static Func<object> GetSeekKeyElementAccessor(Func<Tuple> seekKeyAccessor, int index)
    {
      return () => seekKeyAccessor.Invoke().GetValue(index);
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
      var currentIsOwnedRootSelect = (rootSelectProvider!=null && rootSelectProvider.Source==provider);
      var currentIsOwnedByPaging = !currentIsRoot && Owner.Type.In(ProviderType.Take, ProviderType.Skip, ProviderType.Paging);

      if (currentIsRoot || currentIsOwnedRootSelect || currentIsOwnedByPaging) {
        query.OrderBy.Clear();
        if (currentIsRoot) {
          foreach (var pair in provider.Header.Order)
            query.OrderBy.Add(GetOrderByExpression(query.Columns[pair.Key], provider, pair.Key), pair.Value==Direction.Positive);
        }
        else {
          var columnExpressions = ExtractColumnExpressions(query);
          var shouldUseColumnPosition = provider.Header.Order.Any(o => o.Key >= columnExpressions.Count);
          if (shouldUseColumnPosition) {
            foreach (var pair in provider.Header.Order) {
              if (pair.Key >= columnExpressions.Count)
                query.OrderBy.Add(pair.Key + 1, pair.Value==Direction.Positive);
              else
                query.OrderBy.Add(GetOrderByExpression(columnExpressions[pair.Key], provider, pair.Key), pair.Value==Direction.Positive);
            }
          }
          else {
            foreach (var pair in provider.Header.Order)
              query.OrderBy.Add(GetOrderByExpression(columnExpressions[pair.Key], provider, pair.Key), pair.Value==Direction.Positive);
          }
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
            ? (ExecutableProvider) (new Rse.Providers.ExecutableRawProvider((RawProvider) provider.Source))
            : Compile((CompilableProvider) provider.Source));
      var columnNames = provider.Header.Columns.Select(column => column.Name).ToArray();
      var descriptor = DomainHandler.TemporaryTableManager
        .BuildDescriptor(Mapping, provider.Name, provider.Header.TupleDescriptor, columnNames);
      var request = new QueryRequest(Driver, descriptor.QueryStatement, null, descriptor.TupleDescriptor, QueryRequestOptions.Empty);
      anyTemporaryTablesRequired = true;
      return new SqlStoreProvider(Handlers, request, descriptor, provider, source);
    }
    
    /// <inheritdoc/>
    protected override SqlProvider VisitExistence(ExistenceProvider provider)
    {
      var source = Compile(provider.Source);

      var query = source.Request.Statement.ShallowClone();
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

      var leftSelect = left.Request.Statement;
      var rightSelect = right.Request.Statement;
      leftSelect.OrderBy.Clear();
      rightSelect.OrderBy.Clear();
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

      var leftSelect = left.Request.Statement;
      var rightSelect = right.Request.Statement;
      leftSelect.OrderBy.Clear();
      rightSelect.OrderBy.Clear();
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

      var leftSelect = left.Request.Statement;
      var rightSelect = right.Request.Statement;
      leftSelect.OrderBy.Clear();
      rightSelect.OrderBy.Clear();
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

      var leftSelect = left.Request.Statement;
      var rightSelect = right.Request.Statement;
      leftSelect.OrderBy.Clear();
      rightSelect.OrderBy.Clear();
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
      var source = Compile(provider.Source);

      var query = ExtractSqlSelect(provider, source);
      var rowNumber = SqlDml.RowNumber();
      query.Columns.Add(rowNumber, provider.Header.Columns.Last().Name);
      var columns = ExtractColumnExpressions(query);
      foreach (var order in directionCollection)
        rowNumber.OrderBy.Add(columns[order.Key], order.Value==Direction.Positive);
      return CreateProvider(query, provider, source);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitLock(LockProvider provider)
    {
      var source = Compile(provider.Source);

      var query = source.Request.Statement.ShallowClone();
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

    protected override void Initialize()
    {
      foreach (var column in RootProvider.Header.Columns)
        rootColumns.Add(column.Origin);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public SqlCompiler(HandlerAccessor handlers, CompilerConfiguration configuration)
    {
      Handlers = handlers;
      OuterReferences = new BindingCollection<ApplyParameter, Pair<SqlProvider, bool>>();
      Mapping = configuration.StorageNode.Mapping;
      TypeIdRegistry = configuration.StorageNode.TypeIdRegistry;

      providerInfo = Handlers.ProviderInfo;
      temporaryTablesSupported = DomainHandler.TemporaryTableManager.Supported;

      if (!providerInfo.Supports(ProviderFeatures.FullFeaturedBooleanExpressions))
        booleanExpressionConverter = new BooleanExpressionConverter(Driver);

      stubColumnMap = new Dictionary<SqlColumnStub, SqlExpression>();
    }
  }
}
