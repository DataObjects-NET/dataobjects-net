using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Rse.Helpers;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Orm.Rse.Providers.Compilable;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;

namespace Xtensive.Orm.Rse.PreCompilation.Optimization
{
  internal abstract class ColumnMappingInspector : CompilableProviderVisitor
  {
    protected Dictionary<Provider, List<int>> mappings;
    private readonly TupleAccessGatherer mappingsGatherer;
    private readonly Dictionary<ApplyParameter, List<int>> outerColumnUsages;
    private readonly CompilableProviderVisitor outerColumnUsageVisitor;
    private readonly CompilableProvider rootProvider;

    public virtual CompilableProvider RemoveRedundantColumns()
    {
      mappings.Add(rootProvider, Enumerable.Range(0, rootProvider.Header.Length).ToList());
      var visitedProvider = VisitCompilable(rootProvider);
      return visitedProvider!=rootProvider
        ? visitedProvider
        : rootProvider;
    }

    #region Visit methods

    protected override Provider VisitInclude(IncludeProvider provider)
    {
      int sourceLength = provider.Source.Header.Length;
      mappings[provider.Source] = mappings[provider].Where(i => i < sourceLength).ToList();
      var source = VisitCompilable(provider.Source);
      mappings[provider] = Merge(mappings[provider], mappings[provider.Source]);
      if (source==provider.Source)
        return provider;
      return new IncludeProvider(source, provider.Algorithm, provider.IsInlined,
        provider.FilterDataSource, provider.ResultColumnName, provider.FilteredColumns);
    }

    protected override Provider VisitSelect(SelectProvider provider)
    {
      var requiredColumns = mappings[provider];
      var remappedColumns = requiredColumns.Select(c => provider.ColumnIndexes[c]).ToList();
      mappings[provider.Source] = remappedColumns;
      var source = VisitCompilable(provider.Source);
      var sourceMap = mappings[provider.Source];
      var columns = provider.ColumnIndexes
        .Select((i, j) => new Pair<int>(sourceMap.IndexOf(i), j))
        .Where(i => i.First >= 0)
        .ToList();
      mappings[provider] = columns.Select(c => c.Second).ToList();
      var indexColumns = columns.Select(c => c.First).ToList();
      if (source==provider.Source)
        return provider;
      return new SelectProvider(source, indexColumns.ToArray());
    }

    /// <inheritdoc/>
    protected override Provider VisitFreeText(FreeTextProvider provider)
    {
      mappings[provider] = Enumerable.Range(0, provider.Header.Length).ToList();
      return provider;
    }

    protected override Provider VisitIndex(IndexProvider provider)
    {
      mappings[provider] = Enumerable.Range(0, provider.Header.Length).ToList();
      return provider;
    }

    protected override Provider VisitSeek(SeekProvider provider)
    {
      mappings[provider] = Enumerable.Range(0, provider.Header.Length).ToList();
      return provider;
    }

    protected override Provider VisitFilter(FilterProvider provider)
    {
      mappings[provider.Source] = Merge(mappings[provider], mappingsGatherer.Gather(provider.Predicate));
      var newSourceProvider = VisitCompilable(provider.Source);
      mappings[provider] = mappings[provider.Source];

      var predicate = TranslateLambda(provider, provider.Predicate);
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
      return new JoinProvider(newLeftProvider, newRightProvider, provider.JoinType, newIndexes.ToArray());
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
      return new PredicateJoinProvider(newLeftProvider, newRightProvider, (Expression<Func<Tuple, Tuple, bool>>) predicate, provider.JoinType);
    }

    protected override Provider VisitSort(SortProvider provider)
    {
      mappings[provider.Source] = Merge(mappings[provider], provider.Order.Keys);
      var source = VisitCompilable(provider.Source);

      var sourceMap = mappings[provider.Source];
      var order = new DirectionCollection<int>();
      foreach (var pair in provider.Order) {
        var index = sourceMap.IndexOf(pair.Key);
        if (index < 0)
          throw Exceptions.InternalError(Strings.ExOrderKeyNotFoundInMapping, Log.Instance);
        order.Add(index, pair.Value);
      }
      mappings[provider] = sourceMap;

      if (source==provider.Source)
        return provider;
      return new SortProvider(source, order);
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

      var pair = OverrideRightApplySource(provider, newRightProvider, rightMapping);
      if (pair.First==null)
        rightMapping = mappings[provider.Right];
      else {
        newRightProvider = pair.First;
        rightMapping = pair.Second;
      }
      RestoreMappings(oldMappings);

      mappings[provider] = Merge(leftMapping, rightMapping.Select(map => map + provider.Left.Header.Length));

      return newLeftProvider==provider.Left && newRightProvider==provider.Right
        ? provider
        : new ApplyProvider(applyParameter, newLeftProvider, newRightProvider, provider.IsInlined, provider.SequenceType, provider.ApplyType);
    }

    protected override Provider VisitAggregate(AggregateProvider provider)
    {
      var map = provider.AggregateColumns
        .Select(c => c.SourceIndex)
        .Union(provider.GroupColumnIndexes);
      mappings[provider.Source] = Merge(mappings[provider], map);
      var source = VisitCompilable(provider.Source);

      var sourceMap = mappings[provider.Source];
      var currentMap = mappings[provider];

      var columns = new List<AggregateColumnDescriptor>();
      for (int i = 0; i < provider.AggregateColumns.Length; i++) {
        int columnIndex = i + provider.GroupColumnIndexes.Length;
        if (currentMap.BinarySearch(columnIndex) >= 0) {
          var column = provider.AggregateColumns[i];
          columns.Add(new AggregateColumnDescriptor(column.Name, sourceMap.IndexOf(column.SourceIndex), column.AggregateType));
        }
      }
      mappings[provider] = provider.Header.Columns.Select(c => c.Index).ToList();
      var groupColumnIndexes = provider.GroupColumnIndexes.Select(index => sourceMap.IndexOf(index));

      if (source==provider.Source)
        return provider;
      return new AggregateProvider(source, groupColumnIndexes.ToArray(), columns.ToArray());
    }

    protected override Provider VisitCalculate(CalculateProvider provider)
    {
      int sourceLength = provider.Source.Header.Length;
      var usedColumns = mappings[provider];
      var sourceMapping = Merge(
        mappings[provider].Where(i => i < sourceLength),
        provider.CalculatedColumns.SelectMany(c => mappingsGatherer.Gather(c.Expression))
        );
      mappings[provider.Source] = sourceMapping;
      var newSourceProvider = VisitCompilable(provider.Source);
      mappings[provider] = mappings[provider.Source];

      bool translated = false;
      var descriptors = new List<CalculatedColumnDescriptor>(provider.CalculatedColumns.Length);
      var currentMapping = mappings[provider];
      for (int calculatedColumnIndex = 0; calculatedColumnIndex < provider.CalculatedColumns.Length; calculatedColumnIndex++) {
        if (usedColumns.Contains(provider.CalculatedColumns[calculatedColumnIndex].Index)) {
          currentMapping.Add(provider.Source.Header.Length + calculatedColumnIndex);
          var column = provider.CalculatedColumns[calculatedColumnIndex];
          var expression = TranslateLambda(provider, column.Expression);
          if (expression!=column.Expression)
            translated = true;
          var ccd = new CalculatedColumnDescriptor(column.Name, column.Type, (Expression<Func<Tuple, object>>) expression);
          descriptors.Add(ccd);
        }
      }
      mappings[provider] = currentMapping;
      if (descriptors.Count==0)
        return newSourceProvider;
      if (!translated && newSourceProvider==provider.Source)
        return provider;
      return new CalculateProvider(newSourceProvider, descriptors.ToArray());
    }

    protected override Provider VisitRowNumber(RowNumberProvider provider)
    {
      int sourceLength = provider.Source.Header.Length;
      mappings[provider.Source] = mappings[provider].Where(i => i < sourceLength).ToList();
      var newSource = VisitCompilable(provider.Source);
      var currentMapping = mappings[provider.Source];
      var rowNumberColumn = provider.Header.Columns.Last();
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
      var newLeftProvider = VisitCompilable(provider.Left);
      leftMapping = mappings[provider.Left];

      ReplaceMappings(provider.Right, rightMapping);
      var newRightProvider = VisitCompilable(provider.Right);
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

    protected virtual Pair<CompilableProvider, List<int>> OverrideRightApplySource(ApplyProvider applyProvider, CompilableProvider provider, List<int> requestedMapping)
    {
      return new Pair<CompilableProvider, List<int>>(provider, requestedMapping);
    }

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

    protected ColumnMappingInspector(CompilableProvider originalProvider)
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