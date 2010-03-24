// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.12
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Rse.Helpers;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.PreCompilation.Optimization
{
  internal sealed class StoreRedundantColumnRemover : CompilableProviderVisitor
  {
    private Dictionary<Provider, List<int>> mappings;
    private readonly TupleAccessGatherer mappingsGatherer;
    private readonly Dictionary<ApplyParameter, List<int>> outerColumnUsages;

    private readonly CompilableProviderVisitor outerColumnUsageVisitor;
    private readonly CompilableProvider rootProvider;

    public CompilableProvider RemoveRedundantColumns()
    {
      mappings.Add(rootProvider, Enumerable.Range(0, rootProvider.Header.Length).ToList());
      var visitedProvider = VisitCompilable(rootProvider);
      return visitedProvider!=rootProvider
        ? visitedProvider
        : rootProvider;
    }

    #region Visit methods

    protected override Provider VisitSelect(SelectProvider provider)
    {
      List<int> requiredColumns = mappings[provider];
      List<int> remappedColumns = requiredColumns.Select(c => provider.ColumnIndexes[c]).ToList();
      mappings[provider.Source] = remappedColumns;
      OnRecursionEntrance(provider);
      CompilableProvider source = VisitCompilable(provider.Source);
      var indexColumns = (List<int>) OnRecursionExit(provider);
      if (source==provider.Source)
        return provider;
      return new SelectProvider(source, indexColumns.ToArray());
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

    protected override Provider VisitSeek(SeekProvider provider)
    {
      return SubstituteSelect(provider);
    }

    protected override Provider VisitFilter(FilterProvider provider)
    {
      mappings[provider.Source] = Merge(mappings[provider], mappingsGatherer.Gather(provider.Predicate));

      OnRecursionEntrance(provider);
      CompilableProvider newSourceProvider = VisitCompilable(provider.Source);
      OnRecursionExit(provider);

      Expression predicate = TranslateLambda(provider, provider.Predicate);
      if (newSourceProvider==provider.Source && predicate==provider.Predicate)
        return provider;
      return new FilterProvider(newSourceProvider, (Expression<Func<Tuple, bool>>) predicate);
    }

    protected override Provider VisitJoin(JoinProvider provider)
    {
      // split

      List<int> leftMapping;
      List<int> rightMapping;

      SplitMappings(provider, out leftMapping, out rightMapping);

      leftMapping = Merge(leftMapping, provider.EqualIndexes.Select(p => p.First));
      rightMapping = Merge(rightMapping, provider.EqualIndexes.Select(p => p.Second));

      var newLeftProvider = provider.Left;
      var newRightProvider = provider.Right;
      VisitJoin(ref leftMapping, ref newLeftProvider, ref rightMapping, ref newRightProvider);

      mappings[provider] = MergeMappings(provider.Left, leftMapping, rightMapping);

      if (newLeftProvider==provider.Left && newRightProvider==provider.Right)
        return provider;

      var newIndexes = new List<Pair<int>>();
      foreach (var pair in provider.EqualIndexes) {
        var newLeftIndex = leftMapping.IndexOf(pair.First);
        var newRightIndex = rightMapping.IndexOf(pair.Second);
        newIndexes.Add(new Pair<int>(newLeftIndex, newRightIndex));
      }
      return new JoinProvider(newLeftProvider, newRightProvider, provider.JoinType, provider.JoinAlgorithm,
        newIndexes.ToArray());
    }

    protected override Provider VisitPredicateJoin(PredicateJoinProvider provider)
    {
      List<int> leftMapping;
      List<int> rightMapping;

      SplitMappings(provider, out leftMapping, out rightMapping);

      leftMapping.AddRange(mappingsGatherer.Gather(provider.Predicate,
        provider.Predicate.Parameters[0]));
      rightMapping.AddRange(mappingsGatherer.Gather(provider.Predicate,
        provider.Predicate.Parameters[1]));

      var newLeftProvider = provider.Left;
      var newRightProvider = provider.Right;
      VisitJoin(ref leftMapping, ref newLeftProvider, ref rightMapping, ref newRightProvider);
      mappings[provider] = MergeMappings(provider.Left, leftMapping, rightMapping);
      var predicate = TranslateJoinPredicate(leftMapping, rightMapping, provider.Predicate);
      if (newLeftProvider==provider.Left && newRightProvider==provider.Right
        && provider.Predicate==predicate)
        return provider;
      return new PredicateJoinProvider(newLeftProvider, newRightProvider,
        (Expression<Func<Tuple, Tuple, bool>>) predicate, provider.JoinType);
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

      var oldMappings = ReplaceMappings(provider.Left, leftMapping);
      CompilableProvider newLeftProvider = VisitCompilable(provider.Left);
      leftMapping = mappings[provider.Left];

      ReplaceMappings(provider.Right, rightMapping);
      outerColumnUsages.Add(applyParameter, leftMapping);
      CompilableProvider newRightProvider = VisitCompilable(provider.Right);
      outerColumnUsages.Remove(applyParameter);
      rightMapping = mappings[provider.Right];
      RestoreMappings(oldMappings);

      mappings[provider] = Merge(leftMapping, rightMapping.Select(map => map + provider.Left.Header.Length));

      return newLeftProvider==provider.Left && newRightProvider==provider.Right
        ? provider
        : new ApplyProvider(applyParameter, newLeftProvider, newRightProvider, provider.SequenceType, provider.ApplyType);
    }

    protected override Provider VisitStore(StoreProvider provider)
    {
      OnRecursionEntrance(provider);
      CompilableProvider newSourceProvider = VisitCompilable((CompilableProvider) provider.Source);
      OnRecursionExit(provider);
      if (newSourceProvider==provider.Source) {
        mappings[provider] = new List<int>(){0};
        return provider;
      }
      return new StoreProvider(newSourceProvider);
    }

    protected override Provider VisitRaw(RawProvider provider)
    {
      var mapping = mappings[provider];
      if (mapping.SequenceEqual(Enumerable.Range(0, provider.Header.Length)))
        return provider;
      var mappingTransform = new MapTransform(true, provider.Header.TupleDescriptor, mapping.ToArray());
      var newExpression = RemapRawProviderSource(provider.Source, mappingTransform);
      return new RawProvider(provider.Header.Select(mapping), newExpression);
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
        .Union(provider.GroupColumnIndexes);
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
      var currentMapping = mappings[provider];
      for (int calculatedColumnIndex = 0; calculatedColumnIndex < provider.CalculatedColumns.Length; calculatedColumnIndex++) {
        currentMapping.Add(provider.Source.Header.Length + calculatedColumnIndex);
        CalculatedColumn column = provider.CalculatedColumns[calculatedColumnIndex];
        Expression expression = TranslateLambda(provider, column.Expression);
        if (expression!=column.Expression)
          translated = true;
        var ccd = new CalculatedColumnDescriptor(column.Name, column.Type, (Expression<Func<Tuple, object>>) expression);
        descriptors.Add(ccd);
      }
      mappings[provider] = currentMapping;
      if (!translated && newSourceProvider==provider.Source)
        return provider;
      return new CalculateProvider(newSourceProvider, descriptors.ToArray());
    }

    protected override Provider VisitRowNumber(RowNumberProvider provider)
    {
      int sourceLength = provider.Source.Header.Length;
      mappings[provider.Source] = mappings[provider].Where(i => i < sourceLength).ToList();
      OnRecursionEntrance(provider);
      CompilableProvider newSource = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      List<int> currentMapping = mappings[provider.Source];
      Column rowNumberColumn = provider.Header.Columns.Last();
      mappings[provider] = Merge(currentMapping, EnumerableUtils.One(rowNumberColumn.Index));
      if (newSource==provider.Source)
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

      var oldMappings = ReplaceMappings(provider.Left, leftMapping);
      CompilableProvider newLeftProvider = VisitCompilable(provider.Left);
      leftMapping = mappings[provider.Left];

      ReplaceMappings(provider.Right, rightMapping);
      CompilableProvider newRightProvider = VisitCompilable(provider.Right);
      rightMapping = mappings[provider.Right];
      RestoreMappings(oldMappings);

      var expectedColumns = mappings[provider];
      mappings[provider] = Merge(leftMapping, rightMapping);
      if (newLeftProvider==provider.Left && newRightProvider==provider.Right)
        return provider;


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
      if (provider.Type==ProviderType.Select)
        return provider;
      var columns = expectedColumns
        .Select(originalIndex => new {OriginalIndex = originalIndex, NewIndex = returningColumns.IndexOf(originalIndex)})
        .Select(x => x.NewIndex < 0 ? x.OriginalIndex : x.NewIndex).ToArray();
      return new SelectProvider(provider, columns);
    }

    #endregion

    #region OnRecursionExit, OnRecursionEntrance methods

    protected override object OnRecursionExit(Provider provider)
    {
      switch (provider.Type) {
      case ProviderType.Aggregate: {
        var aggregateProvider = (AggregateProvider) provider;
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
        mappings[aggregateProvider] = aggregateProvider.Header.Columns.Select(c => c.Index).ToList();
        return new Pair<int[], AggregateColumnDescriptor[]>(groupIndexes.ToArray(), columns.ToArray());
      }
      case ProviderType.Sort: {
        var sortProvider = (SortProvider) provider;
        List<int> sourceMap = mappings[sortProvider.Source];
        var orders = new DirectionCollection<int>();
        foreach (KeyValuePair<int, Direction> order in sortProvider.Order) {
          var index = sourceMap.IndexOf(order.Key);
          if (index < 0)
            throw Exceptions.InternalError(Resources.Strings.ExOrderKeyNotFoundInMapping, Log.Instance);
          orders.Add(index, order.Value);
        }
        mappings[sortProvider] = sourceMap;
        return orders;
      }
      case ProviderType.Select: {
        var selectProvider = (SelectProvider) provider;
        List<int> sourceMap = mappings[selectProvider.Source];
        List<Pair<int>> columns = selectProvider.ColumnIndexes
          .Select((i, j) => new Pair<int>(sourceMap.IndexOf(i), j))
          .Where(i => i.First >= 0)
          .ToList();
        mappings[selectProvider] = columns.Select(c => c.Second).ToList();
        return columns.Select(c => c.First).ToList();
      }
      default: {
        mappings[provider] = mappings[provider.Sources[0]];
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
      case ProviderType.PredicateJoin:
      case ProviderType.Aggregate:
      case ProviderType.Calculate:
      case ProviderType.Apply:
      case ProviderType.Select:
      case ProviderType.RowNumber:
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
      mappings[provider] = Enumerable.Range(0, provider.Header.Length).ToList();
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

    private static Expression<Func<IEnumerable<Tuple>>> RemapRawProviderSource(Expression<Func<IEnumerable<Tuple>>> source, MapTransform mappingTransform)
    {
      var selectMethodInfo = typeof (Enumerable)
        .GetMethods()
        .Single(methodInfo => methodInfo.Name=="Select"
          && methodInfo.GetParameters()[1].ParameterType.GetGenericTypeDefinition()==typeof (Func<,>))
        .MakeGenericMethod(typeof (Tuple), typeof (Tuple));

      Func<Tuple, Tuple> selector = tuple => mappingTransform.Apply(TupleTransformType.Auto, tuple);
      var newExpression = Expression.Call(selectMethodInfo, source.Body, Expression.Constant(selector));
      return (Expression<Func<IEnumerable<Tuple>>>) Expression.Lambda(newExpression);
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
        if (!map.Contains(value))
          map.Add(value);
    }

    private int ResolveOuterMapping(ApplyParameter parameter, int value)
    {
      int result = outerColumnUsages[parameter].IndexOf(value);
      if (result < 0)
        return value;
      return result;
    }

    private Expression TranslateLambda(Provider originalProvider, LambdaExpression expression)
    {
      var replacer = new TupleAccessRewriter(mappings[originalProvider], ResolveOuterMapping, true);
      return replacer.Rewrite(expression, expression.Parameters[0]);
    }

    private Expression TranslateJoinPredicate(IList<int> leftMapping,
      IList<int> rightMapping, Expression<Func<Tuple, Tuple, bool>> expression)
    {
      var result = new TupleAccessRewriter(leftMapping, ResolveOuterMapping, true).Rewrite(expression,
        expression.Parameters[0]);
      return new TupleAccessRewriter(rightMapping, ResolveOuterMapping, true).Rewrite(result,
        expression.Parameters[1]);
    }

    private void VisitJoin(ref List<int> leftMapping, ref CompilableProvider left, ref List<int> rightMapping,
      ref CompilableProvider right)
    {
      leftMapping = leftMapping.Distinct().OrderBy(i => i).ToList();
      rightMapping = rightMapping.Distinct().OrderBy(i => i).ToList();

      // visit

      var oldMapping = ReplaceMappings(left, leftMapping);
      CompilableProvider newLeftProvider = VisitCompilable(left);
      leftMapping = mappings[left];

      ReplaceMappings(right, rightMapping);
      CompilableProvider newRightProvider = VisitCompilable(right);
      rightMapping = mappings[right];
      RestoreMappings(oldMapping);
      left = newLeftProvider;
      right = newRightProvider;
    }

    private Dictionary<Provider, List<int>> ReplaceMappings(Provider firstNewKey, List<int> firstNewValue)
    {
      var oldMappings = mappings;
      mappings = new Dictionary<Provider, List<int>> {{firstNewKey, firstNewValue}};
      return oldMappings;
    }

    private void RestoreMappings(Dictionary<Provider, List<int>> savedMappings)
    {
      mappings = savedMappings;
    }

    #endregion

    // Constructors

    public StoreRedundantColumnRemover(CompilableProvider originalProvider)
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