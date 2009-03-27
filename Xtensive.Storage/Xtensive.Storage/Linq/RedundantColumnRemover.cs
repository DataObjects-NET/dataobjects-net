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
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Linq
{
  internal sealed class RedundantColumnRemover : CompilableProviderVisitor
  {
    private readonly Parameter<Dictionary<Provider, List<int>>> mappings;
    private readonly Dictionary<Parameter<Tuple>, List<int>> outerColumnUsages;
    private readonly TupleAccessProcessor mappingsReplacer;
    private readonly TupleAccessProcessor mappingsGatherer;

    private readonly CompilableProviderVisitor outerColumnUsageVisitor;
    private readonly ResultExpression origin;

    public ResultExpression RemoveRedundantColumn()
    {
      var originProvider = origin.RecordSet.Provider;
      var projectorMap = mappingsGatherer.GatherMappings(origin.Projector, originProvider.Header)
          .Distinct()
          .OrderBy()
          .ToList();
      if (projectorMap.Count == 0)
        projectorMap.Add(0);
      if (projectorMap.Count < origin.RecordSet.Header.Length)
        using (new ParameterScope()) {
          mappings.Value = new Dictionary<Provider, List<int>> {{originProvider, projectorMap}};
          var resultProvider = VisitCompilable(originProvider);

          if (projectorMap.Count < resultProvider.Header.Length) {
            var map = mappings.Value[originProvider];
            var columnIndexes = projectorMap
              .Select(i => map.IndexOf(i))
              .ToArray();
            resultProvider = new SelectProvider(resultProvider, columnIndexes);
          }

          var rs = resultProvider.Result;
          var groupMap = BuildGroupMapping(projectorMap, originProvider, resultProvider);

          var projector = (Expression<Func<RecordSet, object>>)mappingsReplacer.ReplaceMappings(origin.Projector, projectorMap, groupMap, origin.RecordSet.Header);
          var itemProjector = origin.ItemProjector == null
            ? null
            : (LambdaExpression)mappingsReplacer.ReplaceMappings(origin.ItemProjector, projectorMap, groupMap, origin.RecordSet.Header);
          var result = new ResultExpression(origin.Type, rs, (origin.Mapping == null) ? null : new ResultMapping(), projector, itemProjector);
          return result;
        }
      return origin;
    }
    
    #region Visit methods

    protected override Provider VisitIndex(IndexProvider provider)
    {
      var columnsCount = provider.Header.Length;
      var value = mappings.Value[provider];
      if (columnsCount > value.Count)
        return new SelectProvider(provider, value.ToArray());
      return provider;
    }

    protected override Provider VisitFilter(FilterProvider provider)
    {
      List<int> map = mappings.Value[provider];
      mappings.Value.Add(provider.Source, Merge(map, mappingsGatherer.GatherMappings(provider.Predicate)));

      OnRecursionEntrance(provider);
      var newSourceProvider = VisitCompilable(provider.Source);
      OnRecursionExit(provider);

      var predicate = TranslateExpression(provider, newSourceProvider, provider.Predicate);
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

      foreach(var item in provider.EqualIndexes) {
        leftMapping.Add(item.First);
        rightMapping.Add(item.Second);
      }

      leftMapping = leftMapping.Distinct().OrderBy().ToList();
      rightMapping = rightMapping.Distinct().OrderBy().ToList();

      // visit

      CompilableProvider newLeftProvider;
      CompilableProvider newRightProvider;

      using (new ParameterScope()) {
        mappings.Value = new Dictionary<Provider, List<int>> {{provider.Left, leftMapping}};
        newLeftProvider = VisitCompilable(provider.Left);
        leftMapping = mappings.Value[provider.Left];
      }

      using (new ParameterScope()) {
        mappings.Value = new Dictionary<Provider, List<int>> {{provider.Right, rightMapping}};
        newRightProvider = VisitCompilable(provider.Right);
        rightMapping = mappings.Value[provider.Right];
      }

      if (newLeftProvider == provider.Left && newRightProvider == provider.Right)
        return provider;
      
      // merge

      mappings.Value[provider] = MergeMappings(provider.Left, leftMapping, rightMapping);
      var equalIndexes = provider.EqualIndexes
        .Select(pair => new Pair<int>(leftMapping.IndexOf(pair.First), rightMapping.IndexOf(pair.Second)))
        .ToArray();
      return new JoinProvider(newLeftProvider, newRightProvider, provider.Outer, provider.JoinType, equalIndexes);
    }

    protected override Provider VisitSort(SortProvider provider)
    {
      var currentMap = mappings.Value[provider];
      mappings.Value.Add(provider.Source, Merge(currentMap, provider.Order.Keys));
      return base.VisitSort(provider);
    }

    protected override Provider VisitApply(ApplyProvider provider)
    {
      // split

      List<int> leftMapping;
      List<int> rightMapping;

      SplitMappings(provider, out leftMapping, out rightMapping);

      var applyParameter = provider.LeftItemParameter;
      var currentOuterUsages = new List<int>();

      outerColumnUsages.Add(applyParameter, currentOuterUsages);
      outerColumnUsageVisitor.VisitCompilable(provider.Right);
      outerColumnUsages.Remove(applyParameter);

      leftMapping = Merge(leftMapping, currentOuterUsages);
      
      // visit

      CompilableProvider newLeftProvider;
      CompilableProvider newRightProvider;

      using (new ParameterScope()) {
        mappings.Value = new Dictionary<Provider, List<int>> {{provider.Left, leftMapping}};
        newLeftProvider = VisitCompilable(provider.Left);
        leftMapping = mappings.Value[provider.Left];
      }

      using (new ParameterScope()) {
        mappings.Value = new Dictionary<Provider, List<int>> {{provider.Right, rightMapping}};
        outerColumnUsages.Add(applyParameter, leftMapping);
        newRightProvider = VisitCompilable(provider.Right);
        outerColumnUsages.Remove(applyParameter);
        rightMapping = mappings.Value[provider.Right];
      }

      if (newLeftProvider == provider.Left && newRightProvider == provider.Right)
        return provider;

      // merge

      mappings.Value[provider] = MergeMappings(provider.Left, leftMapping, rightMapping);
      return new ApplyProvider(applyParameter, newLeftProvider, newRightProvider, provider.ApplyType);
    }

    protected override Provider VisitReindex(ReindexProvider provider)
    {
      var currentMapping = mappings.Value[provider]; 
      mappings.Value.Add(provider.Source, Merge(currentMapping, provider.Order.Keys));
      return base.VisitReindex(provider);
    }

    protected override Provider VisitAggregate(AggregateProvider provider)
    {
      var currentMapping = mappings.Value[provider];
      var map = provider.AggregateColumns
        .Select(c => c.SourceIndex)
        .Concat(provider.GroupColumnIndexes);
      mappings.Value.Add(provider.Source, Merge(currentMapping, map));
      return base.VisitAggregate(provider);
    }

    protected override Provider VisitCalculate(CalculateProvider provider)
    {
      var sourceLength = provider.Source.Header.Length;
      var sourceMapping = Merge(
        mappings.Value[provider].Where(i => i < sourceLength),
        provider.CalculatedColumns.SelectMany(c => mappingsGatherer.GatherMappings(c.Expression))
        );
      mappings.Value.Add(provider.Source, sourceMapping);

      OnRecursionEntrance(provider);
      var newSourceProvider = VisitCompilable(provider.Source);
      OnRecursionExit(provider);

      var translated = false;
      var descriptors = new List<CalculatedColumnDescriptor>(provider.CalculatedColumns.Length);
      foreach (var column in provider.CalculatedColumns) {
        var expression = TranslateExpression(provider, newSourceProvider, column.Expression);
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
      throw new NotImplementedException();
    }

    #endregion

    #region OnRecursionExit, OnRecursionEntrance methods

    protected override object OnRecursionExit(Provider provider)
    {
      switch (provider.Type) {
        case ProviderType.Aggregate: {
          var aggregateProvider = (AggregateProvider)provider;
          var sourceMap = mappings.Value[aggregateProvider.Source];
          var currentMap = mappings.Value[aggregateProvider];
          var columns = new List<AggregateColumnDescriptor>();
          var groupIndexes = new List<int>();
          for (int i = 0; i < aggregateProvider.AggregateColumns.Length; i++) {
            var columnIndex = i + aggregateProvider.GroupColumnIndexes.Length;
            if (currentMap.BinarySearch(columnIndex) >= 0) {
              var column = aggregateProvider.AggregateColumns[i];
              columns.Add(new AggregateColumnDescriptor(column.Name, sourceMap.IndexOf(column.SourceIndex), column.AggregateType));
            }
          }
          foreach (var index in aggregateProvider.GroupColumnIndexes)
            groupIndexes.Add(sourceMap.IndexOf(index));
          return new Pair<int[], AggregateColumnDescriptor[]>(groupIndexes.ToArray(), columns.ToArray());
        }
        case ProviderType.Sort: {
          var sortProvider = (SortProvider)provider;
          var sourceMap = mappings.Value[sortProvider.Source];
          var orders = new DirectionCollection<int>();
          foreach (KeyValuePair<int, Direction> order in sortProvider.Order)
            orders.Add(sourceMap.IndexOf(order.Key), order.Value);
          mappings.Value[sortProvider] = Merge(sourceMap, mappings.Value[sortProvider]);
          return orders;
        }
        default: {
          var unaryProvider = (UnaryProvider)provider;
          mappings.Value[unaryProvider] = Merge(mappings.Value[unaryProvider], mappings.Value[unaryProvider.Source]);
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
        case ProviderType.RowNumber:
        case ProviderType.Apply:
          break;
        default:
          mappings.Value.Add(provider.Sources[0], mappings.Value[provider]);
          break;
      }
    }

    #endregion

    #region Private methods

    private void SplitMappings(BinaryProvider provider, out List<int> leftMapping, out List<int> rightMapping)
    {
      var binaryMapping = mappings.Value[provider];
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

    private void RegisterOuterMapping(Parameter<Tuple> parameter, int value)
    {
      List<int> map;
      if (outerColumnUsages.TryGetValue(parameter, out map))
        map.Add(value);
    }

    private int ResolveOuterMapping(Parameter<Tuple> parameter, int value)
    {
      int result = outerColumnUsages[parameter].IndexOf(value);
      if (result < 0)
        throw new InvalidOperationException();
      return result;
    }

    private Expression TranslateExpression(Provider originalProvider, Provider newSource, Expression expression)
    {
      if (originalProvider.Type == ProviderType.Filter || originalProvider.Type == ProviderType.Calculate)
        return mappingsReplacer.ReplaceMappings(
          expression,
          mappings.Value[originalProvider],
          BuildGroupMapping(mappings.Value[originalProvider], originalProvider.Sources[0], newSource),
          origin.RecordSet.Header
          );
      return expression;
    }

    private static List<int> BuildGroupMapping(List<int> mapping, Provider originProvider, Provider resultProvider)
    {
      var originGroups = originProvider.Header.ColumnGroups.ToList();
      var resultGroups = resultProvider.Header.ColumnGroups.ToList();

      return originGroups
        .Select((og, i) => new { Group = og, Index = i })
        .Where(gi => resultGroups.Any(rg => rg.Keys.Select(rki => mapping[rki]).SequenceEqual(gi.Group.Keys)))
        .Select(gi => gi.Index)
        .ToList();
    }

    private static List<int> Merge(IEnumerable<int> left, IEnumerable<int> right)
    {
      return left
        .Union(right)
        .Distinct()
        .OrderBy()
        .ToList();
    }

    private static List<int> MergeMappings(Provider originalLeft, List<int> leftMap, List<int> rightMap)
    {
      var leftCount = originalLeft.Header.Length;
      var result = leftMap
        .Concat(rightMap.Select(i => i + leftCount))
        .ToList();
      return result;
    }

    #endregion

    // Constructor

    public RedundantColumnRemover(ResultExpression resultExpression)
    {
      origin = resultExpression;

      mappings = new Parameter<Dictionary<Provider, List<int>>>();
      outerColumnUsages = new Dictionary<Parameter<Tuple>, List<int>>();

      mappingsGatherer = new TupleAccessProcessor((a,b)=> { }, null);
      mappingsReplacer = new TupleAccessProcessor(null, ResolveOuterMapping);

      var outerMappingsGatherer = new TupleAccessProcessor(RegisterOuterMapping, null);
      outerColumnUsageVisitor = new CompilableProviderVisitor((_,e) => {
        outerMappingsGatherer.GatherMappings(e);
        return e;
        });
    }
  }
}
