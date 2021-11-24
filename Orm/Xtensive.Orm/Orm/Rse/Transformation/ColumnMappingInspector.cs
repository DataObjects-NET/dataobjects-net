// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Rse.Transformation
{
  internal abstract class ColumnMappingInspector : CompilableProviderVisitor
  {
    protected Dictionary<Provider, List<int>> mappings;

    private readonly TupleAccessGatherer mappingsGatherer;
    private readonly Dictionary<ApplyParameter, List<int>> outerColumnUsages;
    private readonly CompilableProviderVisitor outerColumnUsageVisitor;
    private readonly CompilableProvider rootProvider;

    private bool hasGrouping;

    public virtual CompilableProvider RemoveRedundantColumns()
    {
      mappings.Add(rootProvider, CollectionUtils.RangeToList(0, rootProvider.Header.Length));
      var visitedProvider = VisitCompilable(rootProvider);
      return visitedProvider != rootProvider
        ? visitedProvider
        : rootProvider;
    }

    #region Visit methods

    protected override Provider VisitInclude(IncludeProvider provider)
    {
      var sourceLength = provider.Source.Header.Length;
      mappings[provider.Source] = Merge(mappings[provider].Where(i => i < sourceLength), provider.FilteredColumns);
      var source = VisitCompilable(provider.Source);
      mappings[provider] = Merge(mappings[provider], mappings[provider.Source]);
      if (source == provider.Source) {
        return provider;
      }
      var filteredColumns = provider.FilteredColumns
        .Select(el => mappings[provider].IndexOf(el))
        .ToArray(provider.FilteredColumns.Count);
      return new IncludeProvider(source, provider.Algorithm, provider.IsInlined,
        provider.FilterDataSource, provider.ResultColumnName, filteredColumns);
    }

    protected override Provider VisitSelect(SelectProvider provider)
    {
      var requiredColumns = mappings[provider];
      var remappedColumns = requiredColumns
        .Select(c => provider.ColumnIndexes[c])
        .ToList(requiredColumns.Count);

      mappings[provider.Source] = remappedColumns;
      var source = VisitCompilable(provider.Source);
      var sourceMap = mappings[provider.Source];

      var indexColumns = new List<int>(provider.ColumnIndexes.Count);
      var newMappings = new List<int>(provider.ColumnIndexes.Count);

      var currentItemIndex = 0;
      foreach(var item in provider.ColumnIndexes) {
        var indexInMap = sourceMap.IndexOf(item);
        if (indexInMap >= 0) {
          indexColumns.Add(indexInMap);
          newMappings.Add(currentItemIndex);
        }
        currentItemIndex++;
      }

      mappings[provider] = newMappings;
      return source == provider.Source
        ? provider
        : new SelectProvider(source, indexColumns.ToArray());
    }

    /// <inheritdoc/>
    protected override Provider VisitFreeText(FreeTextProvider provider)
    {
      mappings[provider] = CollectionUtils.RangeToList(0, provider.Header.Length);
      return provider;
    }

    protected override Provider VisitContainsTable(ContainsTableProvider provider)
    {
      mappings[provider] = CollectionUtils.RangeToList(0, provider.Header.Length);
      return provider;
    }

    protected override Provider VisitIndex(IndexProvider provider)
    {
      mappings[provider] = CollectionUtils.RangeToList(0, provider.Header.Length);
      return provider;
    }

    protected override Provider VisitSeek(SeekProvider provider)
    {
      mappings[provider] = CollectionUtils.RangeToList(0, provider.Header.Length);
      return provider;
    }

    protected override Provider VisitFilter(FilterProvider provider)
    {
      mappings[provider.Source] = Merge(mappings[provider], mappingsGatherer.Gather(provider.Predicate));
      var newSourceProvider = VisitCompilable(provider.Source);
      mappings[provider] = mappings[provider.Source];

      var predicate = TranslateLambda(provider, provider.Predicate);
      return newSourceProvider == provider.Source && predicate == provider.Predicate
        ? provider
        : new FilterProvider(newSourceProvider, (Expression<Func<Tuple, bool>>) predicate);
    }

    protected override Provider VisitJoin(JoinProvider provider)
    {
      // split

      SplitMappings(provider, out var leftMapping, out var rightMapping);

      leftMapping = Merge(leftMapping, provider.EqualIndexes.Select(p => p.First));
      rightMapping = Merge(rightMapping, provider.EqualIndexes.Select(p => p.Second));

      var newLeftProvider = provider.Left;
      var newRightProvider = provider.Right;
      VisitJoin(ref leftMapping, ref newLeftProvider, ref rightMapping, ref newRightProvider);

      mappings[provider] = MergeMappings(provider.Left, leftMapping, rightMapping);

      if (newLeftProvider == provider.Left && newRightProvider == provider.Right) {
        return provider;
      }

      var newIndexes = new List<Pair<int>>(provider.EqualIndexes.Length);
      foreach (var pair in provider.EqualIndexes) {
        var newLeftIndex = leftMapping.IndexOf(pair.First);
        var newRightIndex = rightMapping.IndexOf(pair.Second);
        newIndexes.Add(new Pair<int>(newLeftIndex, newRightIndex));
      }
      return new JoinProvider(newLeftProvider, newRightProvider, provider.JoinType, newIndexes.ToArray());
    }

    protected override Provider VisitPredicateJoin(PredicateJoinProvider provider)
    {
      SplitMappings(provider, out var leftMapping, out var rightMapping);

      leftMapping.AddRange(mappingsGatherer.Gather(provider.Predicate,
        provider.Predicate.Parameters[0]));
      rightMapping.AddRange(mappingsGatherer.Gather(provider.Predicate,
        provider.Predicate.Parameters[1]));

      var newLeftProvider = provider.Left;
      var newRightProvider = provider.Right;
      VisitJoin(ref leftMapping, ref newLeftProvider, ref rightMapping, ref newRightProvider);
      mappings[provider] = MergeMappings(provider.Left, leftMapping, rightMapping);
      var predicate = TranslateJoinPredicate(leftMapping, rightMapping, provider.Predicate);

      return newLeftProvider == provider.Left && newRightProvider == provider.Right
        && provider.Predicate == predicate
        ? provider
        : new PredicateJoinProvider(newLeftProvider, newRightProvider, (Expression<Func<Tuple, Tuple, bool>>) predicate, provider.JoinType);
    }

    protected override Provider VisitSort(SortProvider provider)
    {
      mappings[provider.Source] = Merge(mappings[provider], provider.Order.Keys);
      var source = VisitCompilable(provider.Source);

      var sourceMap = mappings[provider.Source];
      var order = new DirectionCollection<int>();
      foreach (var pair in provider.Order) {
        var index = sourceMap.IndexOf(pair.Key);
        if (index < 0) {
          throw Exceptions.InternalError(Strings.ExOrderKeyNotFoundInMapping, OrmLog.Instance);
        }
        order.Add(index, pair.Value);
      }
      mappings[provider] = sourceMap;

      return source == provider.Source ? provider : new SortProvider(source, order);
    }

    protected override Provider VisitApply(ApplyProvider provider)
    {
      // split

      SplitMappings(provider, out var leftMapping, out var rightMapping);

      var applyParameter = provider.ApplyParameter;
      var currentOuterUsages = new List<int>();

      outerColumnUsages.Add(applyParameter, currentOuterUsages);
      _ = outerColumnUsageVisitor.VisitCompilable(provider.Right);
      _ = outerColumnUsages.Remove(applyParameter);

      leftMapping = Merge(leftMapping, currentOuterUsages);

      if (leftMapping.Count == 0) {
        leftMapping.Add(0);
      }

      // visit

      var oldMappings = ReplaceMappings(provider.Left, leftMapping);
      var newLeftProvider = VisitCompilable(provider.Left);
      leftMapping = mappings[provider.Left];

      _ = ReplaceMappings(provider.Right, rightMapping);
      outerColumnUsages.Add(applyParameter, leftMapping);
      var newRightProvider = VisitCompilable(provider.Right);
      _ = outerColumnUsages.Remove(applyParameter);

      var pair = OverrideRightApplySource(provider, newRightProvider, rightMapping);
      if (pair.First == null) {
        rightMapping = mappings[provider.Right];
      }
      else {
        newRightProvider = pair.First;
        rightMapping = pair.Second;
      }
      RestoreMappings(oldMappings);

      mappings[provider] = Merge(leftMapping, rightMapping.Select(map => map + provider.Left.Header.Length));

      return newLeftProvider == provider.Left && newRightProvider == provider.Right
        ? provider
        : new ApplyProvider(applyParameter, newLeftProvider, newRightProvider, provider.IsInlined, provider.SequenceType, provider.ApplyType);
    }

    protected override Provider VisitAggregate(AggregateProvider provider)
    {
      var map = provider.AggregateColumns
        .Select(c => c.SourceIndex)
        .Union(provider.GroupColumnIndexes);
      mappings[provider.Source] = Merge(mappings[provider], map);

      if (provider.GroupColumnIndexes.Length > 0) {
        hasGrouping = true;
      }

      var source = VisitCompilable(provider.Source);
      hasGrouping = false;

      var sourceMap = mappings[provider.Source];
      var currentMap = mappings[provider];

      mappings[provider] = provider.Header.Columns.Select(c => c.Index).ToList(provider.Header.Columns.Count);

      if (source == provider.Source) {
        return provider;
      }

      var columns = new List<AggregateColumnDescriptor>(provider.AggregateColumns.Length);
      for (var i = 0; i < provider.AggregateColumns.Length; i++) {
        var columnIndex = i + provider.GroupColumnIndexes.Length;
        if (currentMap.BinarySearch(columnIndex) >= 0) {
          var column = provider.AggregateColumns[i];
          columns.Add(new AggregateColumnDescriptor(column.Name, sourceMap.IndexOf(column.SourceIndex), column.AggregateType));
        }
      }

      var groupColumnIndexes = provider.GroupColumnIndexes
        .Select(index => sourceMap.IndexOf(index))
        .ToArray(provider.GroupColumnIndexes.Length);

      return new AggregateProvider(source, groupColumnIndexes, columns.ToArray());
    }

    protected override Provider VisitCalculate(CalculateProvider provider)
    {
      var sourceLength = provider.Source.Header.Length;
      var usedColumns = mappings[provider];
      var sourceMapping = Merge(
        mappings[provider].Where(i => i < sourceLength),
        provider.CalculatedColumns.SelectMany(c => mappingsGatherer.Gather(c.Expression)));

      mappings[provider.Source] = sourceMapping;
      var newSourceProvider = VisitCompilable(provider.Source);
      mappings[provider] = mappings[provider.Source];

      var translated = false;
      var descriptors = new List<CalculatedColumnDescriptor>(usedColumns.Count);
      var currentMapping = mappings[provider];
      for (var calculatedColumnIndex = 0; calculatedColumnIndex < provider.CalculatedColumns.Length; calculatedColumnIndex++) {
        if (usedColumns.Contains(provider.CalculatedColumns[calculatedColumnIndex].Index)) {
          currentMapping.Add(provider.Source.Header.Length + calculatedColumnIndex);
          var column = provider.CalculatedColumns[calculatedColumnIndex];
          var expression = TranslateLambda(provider, column.Expression);
          if (expression != column.Expression) {
            translated = true;
          }
          var ccd = new CalculatedColumnDescriptor(column.Name, column.Type, (Expression<Func<Tuple, object>>) expression);
          descriptors.Add(ccd);
        }
      }
      mappings[provider] = currentMapping;
      if (descriptors.Count == 0) {
        return newSourceProvider;
      }

      return !translated && newSourceProvider == provider.Source && descriptors.Count == provider.CalculatedColumns.Length
        ? provider
        : new CalculateProvider(newSourceProvider, descriptors.ToArray());
    }

    protected override Provider VisitRowNumber(RowNumberProvider provider)
    {
      var sourceLength = provider.Source.Header.Length;
      mappings[provider.Source] = mappings[provider].Where(i => i < sourceLength).ToList();
      var newSource = VisitCompilable(provider.Source);
      var currentMapping = mappings[provider.Source];
      var rowNumberColumn = provider.Header.Columns.Last();
      mappings[provider] = Merge(currentMapping, EnumerableUtils.One(rowNumberColumn.Index));
      return newSource == provider.Source
        ? provider
        : new RowNumberProvider(newSource, rowNumberColumn.Name);
    }

    protected override Provider VisitStore(StoreProvider provider)
    {
      if (!(provider.Source is CompilableProvider compilableSource)) {
        return provider;
      }

      if (hasGrouping) {
        mappings.Add(provider.Sources[0],
          Merge(mappings[provider], provider.Header.Columns.Select((c, i) => i)));
      }
      else {
        OnRecursionEntrance(provider);
      }

      var source = VisitCompilable(compilableSource);

      _ = OnRecursionExit(provider);
      return source == compilableSource
        ? provider
        : new StoreProvider(source, provider.Name);
    }

    protected override Provider VisitConcat(ConcatProvider provider) => VisitSetOperationProvider(provider);

    protected override Provider VisitExcept(ExceptProvider provider) => VisitSetOperationProvider(provider);

    protected override Provider VisitIntersect(IntersectProvider provider) => VisitSetOperationProvider(provider);

    protected override Provider VisitUnion(UnionProvider provider) => VisitSetOperationProvider(provider);

    private Provider VisitSetOperationProvider(BinaryProvider provider)
    {
      var leftMapping = mappings[provider];
      var rightMapping = mappings[provider];

      var oldMappings = ReplaceMappings(provider.Left, leftMapping);
      var newLeftProvider = VisitCompilable(provider.Left);
      leftMapping = mappings[provider.Left];

      _ = ReplaceMappings(provider.Right, rightMapping);
      var newRightProvider = VisitCompilable(provider.Right);
      rightMapping = mappings[provider.Right];
      RestoreMappings(oldMappings);

      var expectedColumns = mappings[provider];
      mappings[provider] = Merge(leftMapping, rightMapping);
      if (newLeftProvider == provider.Left && newRightProvider == provider.Right) {
        return provider;
      }

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

    private static CompilableProvider BuildSetOperationSource(CompilableProvider provider, ICollection<int> expectedColumns, IList<int> returningColumns)
    {
      if (provider.Type == ProviderType.Select) {
        return provider;
      }

      var columns = expectedColumns
        .Select(originalIndex => (OriginalIndex: originalIndex, NewIndex: returningColumns.IndexOf(originalIndex)))
        .Select(x => x.NewIndex < 0 ? x.OriginalIndex : x.NewIndex).ToArray(expectedColumns.Count);
      return new SelectProvider(provider, columns);
    }

    protected virtual Pair<CompilableProvider, List<int>> OverrideRightApplySource(ApplyProvider applyProvider,
      CompilableProvider provider, List<int> requestedMapping) =>
      new Pair<CompilableProvider, List<int>>(provider, requestedMapping);

    #endregion

    #region OnRecursionExit, OnRecursionEntrance methods

    protected override object OnRecursionExit(Provider provider)
    {
      mappings[provider] = mappings[provider.Sources[0]];
      return null;
    }

    protected override void OnRecursionEntrance(Provider provider)
    {
      mappings.Add(provider.Sources[0], mappings[provider]);
    }

    #endregion

    #region Private methods

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
      var leftCount = originalLeft.Header.Length;
      var result = leftMap
        .Concat(rightMap.Select(i => i + leftCount))
        .ToList(leftMap.Count + rightMap.Count);
      return result;
    }

    private void SplitMappings(BinaryProvider provider, out List<int> leftMapping, out List<int> rightMapping)
    {
      var binaryMapping = mappings[provider];
      leftMapping = new List<int>(binaryMapping.Count);
      var leftCount = provider.Left.Header.Length;
      var index = 0;
      while (index < binaryMapping.Count && binaryMapping[index] < leftCount) {
        leftMapping.Add(binaryMapping[index]);
        index++;
      }
      rightMapping = new List<int>(binaryMapping.Count - index);
      for (var i = index; i < binaryMapping.Count; i++) {
        rightMapping.Add(binaryMapping[i] - leftCount);
      }
    }

    private void RegisterOuterMapping(ApplyParameter parameter, int value)
    {
      if (outerColumnUsages.TryGetValue(parameter, out var map) && !map.Contains(value)) {
        map.Add(value);
      }
    }

    private int ResolveOuterMapping(ApplyParameter parameter, int value)
    {
      var result = outerColumnUsages[parameter].IndexOf(value);
      return result < 0 ? value : result;
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
      var newLeftProvider = VisitCompilable(left);
      leftMapping = mappings[left];

      _ = ReplaceMappings(right, rightMapping);
      var newRightProvider = VisitCompilable(right);
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

    private void RestoreMappings(Dictionary<Provider, List<int>> savedMappings) => mappings = savedMappings;

    #endregion

    // Constructors

    protected ColumnMappingInspector(CompilableProvider originalProvider)
    {
      rootProvider = originalProvider;

      mappings = new Dictionary<Provider, List<int>>();
      outerColumnUsages = new Dictionary<ApplyParameter, List<int>>();
      mappingsGatherer = new TupleAccessGatherer((a, b) => { });

      var outerMappingsGatherer = new TupleAccessGatherer(RegisterOuterMapping);
      outerColumnUsageVisitor = new CompilableProviderVisitor((p, e) => {
        _ = outerMappingsGatherer.Gather(e);
        return e;
      });
    }
  }
}