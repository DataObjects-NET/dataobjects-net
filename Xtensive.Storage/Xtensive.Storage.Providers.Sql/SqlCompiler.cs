// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Vakhtina Elena
// Created:    2009.02.13

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Core.Threading;
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
  public class SqlCompiler : Compiler<SqlProvider>,
    IHasSyncRoot
  {
    private const string TableNamePattern = "Tmp_{0}";
    private ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

    private readonly BooleanExpressionConverter booleanExpressionConverter;

    public readonly Dictionary<SqlColumnStub, SqlExpression> stubColumnMap;

    /// <inheritdoc/>
    public object SyncRoot
    {
      get { return rwLock; }
    }

    /// <summary>
    /// Gets the value type mapper.
    /// </summary>
    protected Driver Driver
    {
      get { return ((DomainHandler) Handlers.DomainHandler).Driver; }
    }

    /// <summary>
    /// Gets the provider info.
    /// </summary>
    protected ProviderInfo ProviderInfo
    {
      get { return Handlers.DomainHandler.ProviderInfo; }
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
    public override SqlProvider ToCompatible(ExecutableProvider provider)
    {
      return Compile(new StoreProvider(provider));
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitAggregate(AggregateProvider provider)
    {
      var source = Compile(provider.Source);

      var sqlSelect = ExtractSqlSelect(provider, source);

      var columns = ExtractColumnExpressions(sqlSelect, provider);
      var columnNames = columns.Select((c, i) =>
        i >= sqlSelect.Columns.Count
          ? sqlSelect.From.Columns[i].Name
          : sqlSelect.Columns[i].Name).ToList();
      sqlSelect.Columns.Clear();

      for (int i = 0; i < provider.GroupColumnIndexes.Length; i++) {
        var columnIndex = provider.GroupColumnIndexes[i];
        var column = columns[columnIndex];
        sqlSelect.GroupBy.Add(column);
        if (!(column is SqlColumn)) {
          var columnName = ProcessAliasedName(columnNames[columnIndex]);
          column = SqlDml.ColumnRef(SqlDml.Column(column), columnName);
        }
        sqlSelect.Columns.Add(column);
      }

      foreach (var column in provider.AggregateColumns) {
        var expression = ProcessAggregate(source, columns, column);
        sqlSelect.Columns.Add(expression, column.Name);
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
      return new SqlProvider(provider, sqlSelect, Handlers, source);
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
      var allBindings = EnumerableUtils<SqlQueryParameterBinding>.Empty;
      foreach (var column in provider.CalculatedColumns) {
        var result = ProcessExpression(column.Expression, sourceColumns);
        var predicate = result.First;
        var bindings = result.Second;
        if (!ProviderInfo.Supports(ProviderFeatures.FullFledgedBooleanExpressions) && (column.Type.StripNullable()==typeof (bool)))
          predicate = booleanExpressionConverter.BooleanToInt(predicate);
        var columnName = ProcessAliasedName(column.Name);
        var columnRef = SqlDml.ColumnRef(SqlDml.Column(predicate), columnName);
        if (provider.CouldBeInlined) {
          var columnStub = SqlDml.ColumnStub(columnRef);
          stubColumnMap.Add(columnStub, predicate);
          sqlSelect.Columns.Add(columnStub);
        }
        else
          sqlSelect.Columns.Add(columnRef);
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
      if (!sourceSelect.Limit.IsNullReference() || !sourceSelect.Offset.IsNullReference()) {
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

      var sourceColumns = ExtractColumnExpressions(query, provider);
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
        query.Where &= query.Columns[keyColumns[i].Key]==binding.ParameterReference;
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
        var binding = new SqlQueryParameterBinding(() => provider.CompiledKey.Invoke().GetValue(index), typeMapping);
        parameterBindings.Add(binding);
        query.Where &= sqlColumn==binding.ParameterReference;
      }

      return new SqlProvider(provider, query, Handlers, parameterBindings, compiledSource);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitSelect(SelectProvider provider)
    {
      var compiledSource = Compile(provider.Source);
      SqlSelect query;

      if (provider.ColumnIndexes.Length==0) {
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

      var query = ExtractSqlSelect(provider, compiledSource);
      var rootSelectProvider = RootProvider as SelectProvider;
      var currentIsRoot = RootProvider==provider;
      if (currentIsRoot || (rootSelectProvider!=null && rootSelectProvider.Source==provider)) {
        if (query.OrderBy.Count==0) {
          if (currentIsRoot) {
            foreach (KeyValuePair<int, Direction> pair in provider.Header.Order)
              query.OrderBy.Add(query.Columns[pair.Key], pair.Value==Direction.Positive);
          }
          else {
            var columnExpressions = ExtractColumnExpressions(query, provider);
            var shouldUseColumnPosition = provider.Header.Order.Any(o => o.Key >= columnExpressions.Count);
            if (shouldUseColumnPosition)
              foreach (KeyValuePair<int, Direction> pair in provider.Header.Order) {
                if (pair.Key >= columnExpressions.Count)
                  query.OrderBy.Add(pair.Key + 1, pair.Value==Direction.Positive);
                else
                  query.OrderBy.Add(columnExpressions[pair.Key], pair.Value==Direction.Positive);
              }
            else
              foreach (KeyValuePair<int, Direction> pair in provider.Header.Order)
                query.OrderBy.Add(columnExpressions[pair.Key], pair.Value==Direction.Positive);
          }
        }
      }
      return new SqlProvider(provider, query, Handlers, compiledSource);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitStore(StoreProvider provider)
    {
      ExecutableProvider ex = null;
      var domainHandler = (DomainHandler) Handlers.DomainHandler;
      Schema schema = domainHandler.Schema;
      Table table;
      string tableName = string.Format(TableNamePattern, provider.Name);
      SqlSelect query;
      using (LockType.Exclusive.LockRegion(rwLock)) {
        if (provider.Source!=null) {
          ex = provider.Source as ExecutableProvider
            ?? (provider.Source is RawProvider
              ? (ExecutableProvider) (new Rse.Providers.Executable.RawProvider((RawProvider) provider.Source))
              : Compile((CompilableProvider) provider.Source));
          table = provider.Scope==TemporaryDataScope.Global ? schema.CreateTable(tableName)
            : schema.CreateTemporaryTable(tableName);

          foreach (Column column in provider.Header.Columns) {
            SqlValueType svt;
            var mappedColumn = column as MappedColumn;
            if (mappedColumn!=null) {
              ColumnInfo ci = mappedColumn.ColumnInfoRef.Resolve(domainHandler.Domain.Model);
              TypeMapping tm = Driver.GetTypeMapping(ci);
              svt = Driver.BuildValueType(ci);
            }
            else
              svt = Driver.BuildValueType(column.Type, null, null, null);
            TableColumn tableColumn = table.CreateColumn(column.Name, svt);
            tableColumn.IsNullable = true;
            // TODO: Dmitry Maximov, remove this workaround than collation problem will be fixed
            if (column.Type==typeof (string))
              tableColumn.Collation = schema.Collations.FirstOrDefault();
          }
        }
        else
          table = schema.Tables[tableName];

        SqlTableRef tr = SqlDml.TableRef(table);
        query = SqlDml.Select(tr);
        foreach (SqlTableColumn column in tr.Columns)
          query.Columns.Add(column);
        schema.Tables.Remove(table);
      }

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
      var binding = CreateLimitOffsetParameterBinding(provider.Count);
      query.Limit = binding.ParameterReference;
      if (!(provider.Source is TakeProvider) && !(provider.Source is SkipProvider))
        AddOrderByStatement(provider, query);
      return new SqlProvider(provider, query, Handlers, EnumerableUtils.One(binding), compiledSource);
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
          ? ProcessApplyViaCrossApply(provider, left, right)
          : ProcessApplyViaSubqueries(provider, left, right, shouldUseQueryReference);

        return new SqlProvider(provider, query, Handlers, left, right);
      }
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
    /// <returns>Aggregate processing result (expression).</returns>
    protected virtual SqlExpression ProcessAggregate(SqlProvider source, List<SqlExpression> sourceColumns, AggregateColumn aggregateColumn)
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
        if ((index.Attributes & IndexAttributes.View) > 0)
          return BuildViewQuery(index);
        throw new NotSupportedException(String.Format(Strings.ExUnsupportedIndex, index.Name, index.Attributes));
      }
      return BuildTableQuery(index);
    }

    private SqlSelect BuildTableQuery(IndexInfo index)
    {
      var domainHandler = (DomainHandler) Handlers.DomainHandler;
      var table = domainHandler.Schema.Tables[index.ReflectedType.MappingName];
      var atRootPolicy = false;
      if (table==null) {
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
        result = result==null
          ? (ISqlQueryExpression) select
          : result.Union(select);
      }

      var unionRef = SqlDml.QueryRef(result);
      var query = SqlDml.Select(unionRef);
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
        if (result==null) {
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
      var typeIds = index.FilterByTypes.Select(t => t.TypeId).ToArray();
      var underlyingIndex = index.UnderlyingIndexes[0];
      var baseQuery = BuildProviderQuery(underlyingIndex);
      var typeIdColumn = baseQuery.Columns[Handlers.Domain.NameBuilder.TypeIdColumnName];
      var inQuery = SqlDml.In(typeIdColumn, SqlDml.Array(typeIds));
      var query = SqlDml.Select(baseQuery.From);
      query.Columns.AddRange(baseQuery.Columns);
      query.Where = inQuery;
      return query;
    }

    private SqlSelect BuildViewQuery(IndexInfo index)
    {
      var underlyingIndex = index.UnderlyingIndexes[0];
      var baseQuery = BuildProviderQuery(underlyingIndex);
      var query = SqlDml.Select(baseQuery.From);
      query.Where = baseQuery.Where;
      query.Columns.AddRange(index.SelectColumns.Select(i => baseQuery.Columns[i]));
      return query;
    }

    protected void AddOrderByStatement(UnaryProvider provider, SqlSelect query)
    {
      var columnExpressions = ExtractColumnExpressions(query, provider);
      foreach (KeyValuePair<int, Direction> pair in provider.Source.ExpectedOrder)
        query.OrderBy.Add(columnExpressions[pair.Key], pair.Value==Direction.Positive);
    }

    public static bool IsCalculatedColumn(SqlColumn column)
    {
      if (column is SqlUserColumn)
        return true;
      var cRef = column as SqlColumnRef;
      if (!ReferenceEquals(null, cRef))
        return cRef.SqlColumn is SqlUserColumn;
      return false;
    }

    public static bool IsColumnStub(SqlColumn column)
    {
      if (column is SqlColumnStub)
        return true;
      var cRef = column as SqlColumnRef;
      if (!ReferenceEquals(null, cRef))
        return cRef.SqlColumn is SqlColumnStub;
      return false;
    }

    public List<SqlExpression> ExtractColumnExpressions(SqlSelect query, CompilableProvider origin)
    {
      var result = new List<SqlExpression>(query.Columns.Count);
      var shouldUseQueryColumns = origin.Type==ProviderType.Filter && query.Columns.Count < query.From.Columns.Count;
      if (query.Columns.Any(IsColumnStub) || query.GroupBy.Count > 0 || shouldUseQueryColumns) {
        foreach (var column in query.Columns) {
          var expression = IsColumnStub(column)
            ? stubColumnMap[ExtractColumnStub(column)]
            : column;
          result.Add(expression);
        }
      }
      else
        result.AddRange(query.From.Columns.Cast<SqlExpression>());
      return result;
    }

    public static SqlColumnStub ExtractColumnStub(SqlColumn column)
    {
      var columnStub = column as SqlColumnStub;
      if (!ReferenceEquals(null, columnStub))
        return columnStub;
      var columnRef = column as SqlColumnRef;
      if (!ReferenceEquals(null, columnRef))
        return (SqlColumnStub) columnRef.SqlColumn;
      return (SqlColumnStub) column;
    }

    public static SqlUserColumn ExtractUserColumn(SqlColumn column)
    {
      var userColumn = column as SqlUserColumn;
      if (!ReferenceEquals(null, userColumn))
        return userColumn;
      var columnRef = column as SqlColumnRef;
      if (!ReferenceEquals(null, columnRef))
        return (SqlUserColumn) columnRef.SqlColumn;
      return (SqlUserColumn) column;
    }

    protected static bool ShouldUseQueryReference(CompilableProvider origin, SqlProvider compiledSource)
    {
      var sourceSelect = compiledSource.Request.SelectStatement;
      var calculatedColumnIndexes = sourceSelect.Columns
        .Select((c, i) => IsCalculatedColumn(c) ? i : -1)
        .Where(i => i >= 0)
        .ToList();
      var containsCalculatedColumns = calculatedColumnIndexes.Count > 0;
      var pagingIsUsed = !sourceSelect.Limit.IsNullReference() || !sourceSelect.Offset.IsNullReference();
      var groupByIsUsed = sourceSelect.GroupBy.Count > 0;
      var distinctIsUsed = sourceSelect.Distinct;
      var filterIsUsed = !sourceSelect.Where.IsNullReference();
      var columnCountIsNotSame = sourceSelect.From.Columns.Count!=sourceSelect.Columns.Count;

      if (origin.Type==ProviderType.Filter) {
        var filterProvider = (FilterProvider) origin;
        var usedColumnIndexes = new TupleAccessGatherer().Gather(filterProvider.Predicate.Body);
        return pagingIsUsed || usedColumnIndexes.Any(calculatedColumnIndexes.Contains);
      }

      if (origin.Type==ProviderType.Select) {
        var selectProvider = (SelectProvider) origin;
        return containsCalculatedColumns && !calculatedColumnIndexes.All(ci => selectProvider.ColumnIndexes.Contains(ci));
      }

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
          sortProvider.ExpectedOrder
            .Select(order => order.Key)
            .Any(calculatedColumnIndexes.Contains);
        return distinctIsUsed || pagingIsUsed || groupByIsUsed || orderingOverCalculatedColumn;
      }

      if (origin.Type==ProviderType.Apply || origin.Type==ProviderType.Join || origin.Type==ProviderType.PredicateJoin)
        return containsCalculatedColumns || distinctIsUsed || pagingIsUsed || groupByIsUsed;

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

    protected SqlQueryParameterBinding CreateLimitOffsetParameterBinding(Func<int> accessor)
    {
      return new SqlQueryParameterBinding(
        BuildLimitOffsetAccessor(accessor),
        Driver.GetTypeMapping(typeof (int)),
        SqlQueryParameterBindingType.LimitOffset);
    }

    private static Func<object> BuildLimitOffsetAccessor(Func<int> originalAccessor)
    {
      return () => {
        var value = originalAccessor.Invoke();
        // debug helper, don't remove :-)
        // Console.WriteLine("Take/Skip count is " + value);
        if (value < 0)
          throw new InvalidOperationException();
        return value;
      };
    }

    private Pair<SqlExpression, HashSet<SqlQueryParameterBinding>> ProcessExpression(LambdaExpression le, params List<SqlExpression>[] sourceColumns)
    {
      var processor = new ExpressionProcessor(le, this, Handlers, sourceColumns);
      var result = new Pair<SqlExpression, HashSet<SqlQueryParameterBinding>>(
        processor.Translate(),
        processor.Bindings);
      return result;
    }

    private SqlSelect ProcessApplyViaSubqueries(ApplyProvider provider, SqlProvider left, SqlProvider right, bool shouldUseQueryReference)
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

      if (provider.Right.Type==ProviderType.Existence) {
        var column = rightQuery.Columns[0];
        if (provider.CouldBeInlined) {
          var columnStub = SqlDml.ColumnStub(column);
          var userColumn = ExtractUserColumn(column);
          stubColumnMap.Add(columnStub, userColumn.Expression);
          column = columnStub;
        }
        query.Columns.Add(column);
      }
      else {
        if (provider.CouldBeInlined) {
          for (int i = 0; i < rightQuery.Columns.Count; i++) {
            var subquery = rightQuery.ShallowClone();
            var columnRef = (SqlColumnRef) subquery.Columns[i];
            var column = columnRef.SqlColumn;
            subquery.Columns.Clear();
            subquery.Columns.Add(column);
            var columnName = ProcessAliasedName(provider.Right.Header.Columns[i].Name);
            var userColumnRef = SqlDml.ColumnRef(SqlDml.Column(subquery), columnName);
            var columnStub = SqlDml.ColumnStub(userColumnRef);
            stubColumnMap.Add(columnStub, subquery);
            query.Columns.Add(columnStub);
          }
        }
        else
          for (int i = 0; i < rightQuery.Columns.Count; i++) {
            var subquery = rightQuery.ShallowClone();
            var columnRef = (SqlColumnRef) subquery.Columns[i];
            var column = columnRef.SqlColumn;
            subquery.Columns.Clear();
            subquery.Columns.Add(column);
            query.Columns.Add(subquery, columnRef.Name);
          }
      }
      return query;
    }

    private static SqlSelect ProcessApplyViaCrossApply(ApplyProvider provider, SqlProvider left, SqlProvider right)
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
      if (!leftShouldUseReference)
        query.Where &= left.Request.SelectStatement.Where;
      if (!rightShouldUseReference)
        query.Where &= right.Request.SelectStatement.Where;
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

      stubColumnMap = new Dictionary<SqlColumnStub, SqlExpression>();
    }
  }
}