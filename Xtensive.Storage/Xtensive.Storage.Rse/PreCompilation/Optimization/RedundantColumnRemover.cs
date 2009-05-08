// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.02.17

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Expressions;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.PreCompilation.Optimization
{
  internal sealed class RedundantColumnRemover : CompilableProviderVisitor
  {
    private readonly Dictionary<Provider, List<int>> mappings;
    private readonly TupleAccessGatherer mappingsGatherer;
    private readonly Dictionary<ApplyParameter, List<int>> outerColumnUsages;

    private readonly CompilableProviderVisitor outerColumnUsageVisitor;
    private readonly SelectProvider rootProvider;

    public CompilableProvider RemoveRedundantColumns()
    {
      CompilableProvider originalProvider = rootProvider.Source;
      List<int> originalMap = rootProvider.ColumnIndexes.ToList();

      if (originalMap.Count == originalProvider.Header.Length)
        return rootProvider;

      CompilableProvider resultProvider;
      List<int> resultMap;

      mappings.Add(originalProvider, originalMap);
      resultProvider = VisitCompilable(originalProvider);
      resultMap = mappings[rootProvider.Source];

      if (originalMap.Count < resultMap.Count || originalProvider != resultProvider) {
        int[] columnIndexes = originalMap.Select(i => resultMap.IndexOf(i)).ToArray();
        return new SelectProvider(resultProvider, columnIndexes);
      }

      return rootProvider;
    }

    #region Visit methods

    protected override Provider VisitSelect(SelectProvider provider)
    {
      List<int> requiredColumns = mappings[provider];
      List<int> remappedColumns = requiredColumns.Select(c => provider.ColumnIndexes[c]).ToList();
      mappings[provider.Source] = remappedColumns;
      CompilableProvider source = VisitCompilable(provider.Source);
      if (source == provider.Source && requiredColumns.Count == provider.ColumnIndexes.Length)
        return provider;
      List<int> sourceColumns = mappings[provider.Source];
      int[] outputColumns = remappedColumns.Select(c => sourceColumns.IndexOf(c)).ToArray();
      var sourceAsSelect = provider.Source as SelectProvider;
      if (sourceAsSelect != null && sourceAsSelect.ColumnIndexes.SequenceEqual(outputColumns))
        return sourceAsSelect;
      return new SelectProvider(source, outputColumns);
    }

    protected override Provider VisitIndex(IndexProvider provider)
    {
      return SubstituteSelect(provider);
    }

    protected override Provider VisitRangeSet(RangeSetProvider provider)
    {
      return SubstituteSelect(provider);
    }

    protected override Provider VisitRange(RangeProvider provider)
    {
      return SubstituteSelect(provider);
    }

    protected override Provider VisitFilter(FilterProvider provider)
    {
      mappings[provider.Source] = Merge(mappings[provider], mappingsGatherer.Gather(provider.Predicate));

      OnRecursionEntrance(provider);
      CompilableProvider newSourceProvider = VisitCompilable(provider.Source);
      OnRecursionExit(provider);

      Expression predicate = TranslateExpression(provider, provider.Predicate);
      if (newSourceProvider == provider.Source && predicate == provider.Predicate)
        return provider;
      return new FilterProvider(newSourceProvider, (Expression<Func<Tuple, bool>>)predicate);
    }

    protected override Provider VisitJoin(JoinProvider provider)
    {
      // split

      List<int> leftMapping;
      List<int> rightMapping;

      SplitMappings(provider, out leftMapping, out rightMapping);

      foreach (var item in provider.EqualIndexes) {
        leftMapping.Add(item.First);
        rightMapping.Add(item.Second);
      }

      leftMapping = leftMapping.Distinct().OrderBy(i => i).ToList();
      rightMapping = rightMapping.Distinct().OrderBy(i => i).ToList();

      // visit

      mappings[provider.Left] = leftMapping;
      CompilableProvider newLeftProvider = VisitCompilable(provider.Left);
      leftMapping = mappings[provider.Left];

      mappings[provider.Right] = rightMapping;
      CompilableProvider newRightProvider = VisitCompilable(provider.Right);
      rightMapping = mappings[provider.Right];

      if (newLeftProvider == provider.Left && newRightProvider == provider.Right)
        return provider;

      // merge

      mappings[provider] = MergeMappings(provider.Left, leftMapping, rightMapping);
      Pair<int>[] equalIndexes = provider.EqualIndexes
        .Select(pair => new Pair<int>(leftMapping.IndexOf(pair.First), rightMapping.IndexOf(pair.Second)))
        .ToArray();
      return new JoinProvider(newLeftProvider, newRightProvider, provider.Outer, provider.JoinType, equalIndexes);
    }

    protected override Provider VisitSort(SortProvider provider)
    {
      mappings[provider.Source] = Merge(mappings[provider], provider.Order.Keys);
      return base.VisitSort(provider);
    }

    protected override Provider VisitApply(ApplyProvider provider)
    {
      // split

      List<int> leftMapping;
      List<int> rightMapping;

      SplitMappings(provider, out leftMapping, out rightMapping);

      ApplyParameter applyParameter = provider.ApplyParameter;
      var currentOuterUsages = new List<int>();

      outerColumnUsages.Add(applyParameter, currentOuterUsages);
      outerColumnUsageVisitor.VisitCompilable(provider.Right);
      outerColumnUsages.Remove(applyParameter);

      leftMapping = Merge(leftMapping, currentOuterUsages);

      if (leftMapping.Count==0)
        leftMapping.Add(0);

      // visit

      mappings[provider.Left] = leftMapping;
      CompilableProvider newLeftProvider = VisitCompilable(provider.Left);
      leftMapping = mappings[provider.Left];

      mappings[provider.Right] = rightMapping;
      outerColumnUsages.Add(applyParameter, leftMapping);
      CompilableProvider newRightProvider = VisitCompilable(provider.Right);
      outerColumnUsages.Remove(applyParameter);
      rightMapping = mappings[provider.Right];

      if (newLeftProvider == provider.Left && newRightProvider == provider.Right)
        return provider;

      // merge

      mappings[provider] = MergeMappings(provider.Left, leftMapping, rightMapping);
      return new ApplyProvider(applyParameter, newLeftProvider, newRightProvider, provider.ApplyType);
    }

    protected override Provider VisitReindex(ReindexProvider provider)
    {
      mappings[provider.Source] = Merge(mappings[provider], provider.Order.Keys);
      return base.VisitReindex(provider);
    }

    protected override Provider VisitAggregate(AggregateProvider provider)
    {
      var map = provider.AggregateColumns
        .Select(c => c.SourceIndex)
        .Concat(provider.GroupColumnIndexes);
      mappings[provider.Source] = Merge(mappings[provider], map);
      return base.VisitAggregate(provider);
    }

    protected override Provider VisitCalculate(CalculateProvider provider)
    {
      int sourceLength = provider.Source.Header.Length;
      var sourceMapping = Merge(
        mappings[provider].Where(i => i < sourceLength),
        provider.CalculatedColumns.SelectMany(c => mappingsGatherer.Gather(c.Expression))
        );
      mappings[provider.Source] = sourceMapping;

      OnRecursionEntrance(provider);
      CompilableProvider newSourceProvider = VisitCompilable(provider.Source);
      OnRecursionExit(provider);

      bool translated = false;
      var descriptors = new List<CalculatedColumnDescriptor>(provider.CalculatedColumns.Length);
      foreach (CalculatedColumn column in provider.CalculatedColumns) {
        Expression expression = TranslateExpression(provider, column.Expression);
        if (expression != column.Expression)
          translated = true;
        var ccd = new CalculatedColumnDescriptor(column.Name, column.Type, (Expression<Func<Tuple, object>>)expression);
        descriptors.Add(ccd);
      }
      if (!translated && newSourceProvider == provider.Source)
        return provider;
      return new CalculateProvider(newSourceProvider, descriptors.ToArray());
    }

    protected override Provider VisitRowNumber(RowNumberProvider provider)
    {
      OnRecursionEntrance(provider);
      CompilableProvider newSource = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      List<int> currentMapping = mappings[provider.Source];
      Column rowNumberColumn = provider.Header.Columns.Last();
      mappings[provider] = Merge(currentMapping, EnumerableUtils.One(rowNumberColumn.Index));
      if (newSource == provider.Source)
        return provider;
      return new RowNumberProvider(newSource, rowNumberColumn.Name);
    }

    protected override Provider VisitConcat(ConcatProvider provider)
    {
      return VisitSetOperationProvider(provider);
    }

    protected override Provider VisitExcept(ExceptProvider provider)
    {
      return VisitSetOperationProvider(provider);
    }

    protected override Provider VisitIntersect(IntersectProvider provider)
    {
      return VisitSetOperationProvider(provider);
    }

    protected override Provider VisitUnion(UnionProvider provider)
    {
      return VisitSetOperationProvider(provider);
    }

    private Provider VisitSetOperationProvider(BinaryProvider provider)
    {
      var leftMapping = mappings[provider];
      var rightMapping = mappings[provider];

      mappings[provider.Left] = leftMapping;
      CompilableProvider newLeftProvider = VisitCompilable(provider.Left);
      leftMapping = mappings[provider.Left];

      mappings[provider.Right] = rightMapping;
      CompilableProvider newRightProvider = VisitCompilable(provider.Right);
      rightMapping = mappings[provider.Right];

      if (newLeftProvider == provider.Left && newRightProvider == provider.Right)
        return provider;

      var expectedColumns = mappings[provider];
      newLeftProvider = BuildSetOperationSource(newLeftProvider, expectedColumns, leftMapping);
      newRightProvider = BuildSetOperationSource(newRightProvider, expectedColumns, rightMapping);
      switch (provider.Type) {
        case ProviderType.Concat:
          return new ConcatProvider(newLeftProvider, newRightProvider);
        case ProviderType.Intersect:
          return new IntersectProvider(newLeftProvider, newRightProvider);
        case ProviderType.Except:
          return new ExceptProvider(newLeftProvider, newRightProvider);
        case ProviderType.Union:
          return new UnionProvider(newLeftProvider, newRightProvider);
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private static CompilableProvider BuildSetOperationSource(CompilableProvider provider, IEnumerable<int> expectedColumns, IList<int> returningColumns)
    {
      var columns = expectedColumns.Select(c => returningColumns.IndexOf(c)).ToArray();
      return provider.Type == ProviderType.Select
        ? new SelectProvider(((SelectProvider)provider).Source, columns)
        : new SelectProvider(provider, columns);
    }

    #endregion

    #region OnRecursionExit, OnRecursionEntrance methods

    protected override object OnRecursionExit(Provider provider)
    {
      switch (provider.Type) {
        case ProviderType.Aggregate: {
          var aggregateProvider = (AggregateProvider)provider;
          List<int> sourceMap = mappings[aggregateProvider.Source];
          List<int> currentMap = mappings[aggregateProvider];
          var columns = new List<AggregateColumnDescriptor>();
          var groupIndexes = new List<int>();
          for (int i = 0; i < aggregateProvider.AggregateColumns.Length; i++) {
            int columnIndex = i + aggregateProvider.GroupColumnIndexes.Length;
            if (currentMap.BinarySearch(columnIndex) >= 0) {
              AggregateColumn column = aggregateProvider.AggregateColumns[i];
              columns.Add(new AggregateColumnDescriptor(column.Name, sourceMap.IndexOf(column.SourceIndex), column.AggregateType));
            }
          }
          foreach (int index in aggregateProvider.GroupColumnIndexes)
            groupIndexes.Add(sourceMap.IndexOf(index));
          return new Pair<int[], AggregateColumnDescriptor[]>(groupIndexes.ToArray(), columns.ToArray());
        }
        case ProviderType.Sort: {
          var sortProvider = (SortProvider)provider;
          List<int> sourceMap = mappings[sortProvider.Source];
          var orders = new DirectionCollection<int>();
          foreach (KeyValuePair<int, Direction> order in sortProvider.Order)
            orders.Add(sourceMap.IndexOf(order.Key), order.Value);
          mappings[sortProvider] = Merge(sourceMap, mappings[sortProvider]);
          return orders;
        }
        case ProviderType.Select: {
          var selectProvider = (SelectProvider)provider;
          List<int> sourceMap = mappings[selectProvider.Source];
          List<Pair<int>> columns = selectProvider.ColumnIndexes
            .Select((i, j) => new Pair<int>(sourceMap.IndexOf(i), j))
            .Where(i => i.First >= 0)
            .ToList();
          mappings[selectProvider] = columns.Select(c => c.Second).ToList();
          return columns.Select(c => c.First).ToArray();
        }
        default: {
          var unaryProvider = (UnaryProvider)provider;
          mappings[unaryProvider] = Merge(mappings[unaryProvider], mappings[unaryProvider.Source]);
          break;
        }
      }
      return null;
    }

    protected override void OnRecursionEntrance(Provider provider)
    {
      switch (provider.Type) {
        case ProviderType.Index:
        case ProviderType.Filter:
        case ProviderType.Sort:
        case ProviderType.Reindex:
        case ProviderType.Join:
        case ProviderType.Aggregate:
        case ProviderType.Calculate:
        case ProviderType.Apply:
          break;
        case ProviderType.Range:
        case ProviderType.RangeSet:
          mappings.Add(
            provider.Sources[0],
            Merge(mappings[provider], provider.Header.Order.Select(o => o.Key))
            );
          break;
        default:
          mappings.Add(provider.Sources[0], mappings[provider]);
          break;
      }
    }

    #endregion

    #region Private methods

    private Provider SubstituteSelect(CompilableProvider provider)
    {
      int columnsCount = provider.Header.Length;
      List<int> value = mappings[provider];
//      var value = Merge(mappings.Value[provider], provider.Header.Order.Select(o => o.Key));
//     mappings[provider] = value;
      if (columnsCount > value.Count)
        return new SelectProvider(provider, value.ToArray());
      return provider;
    }

    private static List<int> Merge(IEnumerable<int> left, IEnumerable<int> right)
    {
      return left
        .Union(right)
        .Distinct()
        .OrderBy(i => i)
        .ToList();
    }

    private static List<int> MergeMappings(Provider originalLeft, List<int> leftMap, List<int> rightMap)
    {
      int leftCount = originalLeft.Header.Length;
      List<int> result = leftMap
        .Concat(rightMap.Select(i => i + leftCount))
        .ToList();
      return result;
    }

    private void SplitMappings(BinaryProvider provider, out List<int> leftMapping, out List<int> rightMapping)
    {
      List<int> binaryMapping = mappings[provider];
      leftMapping = new List<int>();
      rightMapping = new List<int>();
      int leftCount = provider.Left.Header.Length;
      int index = 0;
      while (index < binaryMapping.Count && binaryMapping[index] < leftCount) {
        leftMapping.Add(binaryMapping[index]);
        index++;
      }
      for (int i = index; i < binaryMapping.Count; i++)
        rightMapping.Add(binaryMapping[i] - leftCount);
    }

    private void RegisterOuterMapping(ApplyParameter parameter, int value)
    {
      List<int> map;
      if (outerColumnUsages.TryGetValue(parameter, out map))
        map.Add(value);
    }

    private int ResolveOuterMapping(ApplyParameter parameter, int value)
    {
      int result = outerColumnUsages[parameter].IndexOf(value);
      if (result < 0)
        throw new InvalidOperationException();
      return result;
    }

    private Expression TranslateExpression(Provider originalProvider, Expression expression)
    {
      if (originalProvider.Type == ProviderType.Filter || originalProvider.Type == ProviderType.Calculate) {
        var replacer = new TupleAccessRewriter(mappings[originalProvider], ResolveOuterMapping);
        return replacer.Rewrite(expression);
      }
      return expression;
    }

    #endregion


    // Constructor

    public RedundantColumnRemover(SelectProvider originalProvider)
    {
      rootProvider = originalProvider;

      mappings = new Dictionary<Provider, List<int>>();
      outerColumnUsages = new Dictionary<ApplyParameter, List<int>>();

      mappingsGatherer = new TupleAccessGatherer((a, b) => { });

      var outerMappingsGatherer = new TupleAccessGatherer(RegisterOuterMapping);
      outerColumnUsageVisitor = new CompilableProviderVisitor((_, e) => {
        outerMappingsGatherer.Gather(e);
        return e;
      });
    }
  }
}