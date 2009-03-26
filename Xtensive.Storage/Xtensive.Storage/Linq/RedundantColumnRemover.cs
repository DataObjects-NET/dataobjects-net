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
    private readonly Dictionary<Provider, List<int>> mappings;
    private readonly Dictionary<Parameter<Tuple>, List<int>> outerColumnUsages;
    private readonly Dictionary<Parameter<Tuple>, Provider> outerProviders;
    private readonly TupleAccessProcessor tupleAccessReplacer;
    private readonly TupleAccessProcessor tupleAccessProcessor;
    private readonly CompilableProviderVisitor outerColumnUsageVisitor;
    private readonly ResultExpression origin;

    public ResultExpression RemoveRedundantColumn()
    {
      var originProvider = origin.RecordSet.Provider;
      var projectorMap = tupleAccessReplacer.Process(origin.Projector)
          .OrderBy(i => i)
          .Distinct()
          .ToList();
      if (projectorMap.Count < origin.RecordSet.Header.Length) {
        mappings[originProvider] = projectorMap;
        var resultProvider = VisitCompilable(originProvider);

        if (projectorMap.Count < resultProvider.Header.Length) {
          var map = mappings[originProvider];
          var columnIndexes = projectorMap
            .Select(i => map.IndexOf(i))
            .ToArray();
          resultProvider = new SelectProvider(resultProvider, columnIndexes);
        }

        var rs = resultProvider.Result;
        var originGroups = originProvider.Header.ColumnGroups.ToList();
        var resultGroups = resultProvider.Header.ColumnGroups.ToList();

        var groupMap = originGroups
          .Select((og, i) => new {Group = og, Index = i})
          .Where(gi => resultGroups.Any(rg => rg.Keys.Select(rki => projectorMap[rki]).SequenceEqual(gi.Group.Keys)))
          .Select(gi => gi.Index)
          .ToList();

        var projector = (Expression<Func<RecordSet, object>>)tupleAccessReplacer.ReplaceMappings(origin.Projector, projectorMap, groupMap);
        var itemProjector = origin.ItemProjector == null
          ? null
          : (LambdaExpression)tupleAccessReplacer.ReplaceMappings(origin.ItemProjector, projectorMap, groupMap);
        var result = new ResultExpression(origin.Type, rs, (origin.Mapping == null) ? null : new ResultMapping(), projector, itemProjector);
        return result;
      }
      return origin;
    }

    #region Visit methods

    protected override Provider VisitIndex(IndexProvider provider)
    {
      var columnsCount = provider.Header.Length;
      var value = mappings[provider];
      if (columnsCount > value.Count)
        return new SelectProvider(provider, value.ToArray());
      return provider;
    }

    protected override Provider VisitFilter(FilterProvider provider)
    {
      List<int> map = mappings[provider];
      mappings.Add(provider.Source, Merge(map, tupleAccessReplacer.Process(provider.Predicate)));
      return base.VisitFilter(provider);
    }

    protected override Provider VisitJoin(JoinProvider provider)
    {
      List<int> leftMap;
      List<int> rightMap;
      
      SplitMappings(provider, out leftMap, out rightMap);

      foreach(var item in provider.EqualIndexes) {
        leftMap.Add(item.First);
        rightMap.Add(item.Second);
      }

      mappings.Add(provider.Left, leftMap.OrderBy(i=>i).Distinct().ToList());
      mappings.Add(provider.Right, rightMap.OrderBy(i => i).Distinct().ToList());

      var left = VisitCompilable(provider.Left);
      var right = VisitCompilable(provider.Right);
      if (left == provider.Left && right == provider.Right)
        return provider;
      leftMap = mappings[provider.Left];
      rightMap = mappings[provider.Right];
      mappings[provider] = MergeMappings(left, provider.Left, provider.Right);
      var equalIndexes = provider.EqualIndexes
        .Select(pair => new Pair<int>(leftMap.IndexOf(pair.First), rightMap.IndexOf(pair.Second)))
        .ToArray();
      return new JoinProvider(left, right, provider.Outer, provider.JoinType, equalIndexes);
    }

    protected override Provider VisitSort(SortProvider provider)
    {
      var currentMap = mappings[provider];
      var sourceMap = new List<int>();
      foreach (var key in provider.Order.Keys)
        sourceMap.Add(key);
      mappings.Add(provider.Source, Merge(currentMap, sourceMap));
      return base.VisitSort(provider);
    }

    protected override Provider VisitApply(ApplyProvider provider)
    {
      List<int> leftMap;
      List<int> rightMap;

      var currentOuterUsages = new List<int>();
      outerColumnUsages.Add(provider.LeftItemParameter, currentOuterUsages);
      outerColumnUsageVisitor.VisitCompilable(provider.Right);
      outerColumnUsages.Remove(provider.LeftItemParameter);

      SplitMappings(provider, out leftMap, out rightMap);
      mappings.Add(provider.Left, leftMap);
      mappings.Add(provider.Right, Merge(rightMap, currentOuterUsages));

      outerProviders.Add(provider.LeftItemParameter, provider.Left);
      var left = VisitCompilable(provider.Left);
      var right = VisitCompilable(provider.Right);
      outerProviders.Remove(provider.LeftItemParameter);

      if (left == provider.Left && right == provider.Right)
        return provider;
      return new ApplyProvider(provider.LeftItemParameter, left, right, provider.ApplyType);
    }

    protected override Provider VisitReindex(ReindexProvider provider)
    {
      List<int> value;
      mappings.TryGetValue(provider, out value);
      if (value != null) {
        var map = new List<int>();
        foreach (var key in provider.Order.Keys)
          map.Add(key);
        mappings.Add(provider.Source, value
          .Union(map)
          .OrderBy(i=>i)
          .Distinct()
          .ToList());
      }
      return base.VisitReindex(provider);
    }

    protected override Provider VisitAggregate(AggregateProvider provider)
    {
      List<int> value;
      mappings.TryGetValue(provider, out value);
      if (value != null) {
        var map = new List<int>();
        foreach (var column in provider.AggregateColumns)
          map.Add(column.SourceIndex);
        foreach (var index in provider.GroupColumnIndexes)
          map.Add(index);
        mappings.Add(provider.Source, value
          .Union(map)
          .OrderBy(i => i)
          .Distinct()
          .ToList());
      }
      return base.VisitAggregate(provider);
    }

    protected override Provider VisitCalculate(CalculateProvider provider)
    {
      var sourceLength = provider.Source.Header.Length;
      var value = mappings[provider]
        .Where(i => i < sourceLength)
        .Union(provider.CalculatedColumns.SelectMany(c => tupleAccessReplacer.Process(c.Expression)))
        .OrderBy(i => i)
        .Distinct()
        .ToList();
      mappings.Add(provider.Source, value);
      return base.VisitCalculate(provider);
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
          var sourceMap = mappings[aggregateProvider.Source];
          var currentMap = mappings[aggregateProvider];
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
          var sourceMap = mappings[sortProvider.Source];
          var orders = new DirectionCollection<int>();
          foreach (KeyValuePair<int, Direction> order in sortProvider.Order)
            orders.Add(sourceMap.IndexOf(order.Key), order.Value);
          return orders;
        }
        default: {
          var up = (UnaryProvider)provider;
          var sourceMap = mappings[up.Source];
          var currentMap = mappings[up];
          var resultMap = sourceMap.Union(currentMap).OrderBy(i => i).Distinct().ToList();
          mappings[up] = resultMap;
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
          mappings.Add(provider.Sources[0], mappings[provider]);
          break;
      }
    }

    #endregion

    #region Private methods

    private void SplitMappings(BinaryProvider provider, out List<int> leftMapping, out List<int> rightMapping)
    {
      var binaryMapping = mappings[provider];
      leftMapping = new List<int>();
      rightMapping = new List<int>();
      int leftCount = provider.Left.Header.Columns.Count;
      int index = 0;
      while (index < binaryMapping.Count && binaryMapping[index] < leftCount) {
        leftMapping.Add(binaryMapping[index]);
        index++;
      }
      for (int i = index; i < binaryMapping.Count; i++)
        rightMapping.Add(binaryMapping[i] - leftCount);
    }

    private List<int> MergeMappings(Provider newLeft, Provider originalLeft, Provider originalRight)
    {
      var leftMap = mappings[originalLeft];
      var rightMap = mappings[originalRight];
      var leftCount = newLeft.Header.Length;
      var result = leftMap
        .Concat(rightMap.Select(i => i + leftCount))
        .ToList();
      return result;
    }

    private void RegisterOuterMapping(Parameter<Tuple> parameter, int value)
    {
      List<int> map;
      if (outerColumnUsages.TryGetValue(parameter, out map))
        map.Add(value);
    }

    private int ResolveOuterMapping(Parameter<Tuple> parameter, int value)
    {
      return mappings[outerProviders[parameter]].IndexOf(value);
    }

    private static List<int> Merge(IEnumerable<int> left, IEnumerable<int> right)
    {
      return left
        .Union(right)
        .OrderBy(i => i)
        .Distinct()
        .ToList();
    }

    #endregion

    // Constructor

    public RedundantColumnRemover(ResultExpression resultExpression)
    {
      mappings = new Dictionary<Provider, List<int>>();
      outerProviders = new Dictionary<Parameter<Tuple>, Provider>();
      outerColumnUsages = new Dictionary<Parameter<Tuple>, List<int>>();
      tupleAccessReplacer = new TupleAccessProcessor(null, ResolveOuterMapping);
      tupleAccessProcessor = new TupleAccessProcessor(RegisterOuterMapping, null);
      outerColumnUsageVisitor = new CompilableProviderVisitor((_,e) => {
        tupleAccessProcessor.Process(e);
        return e;
        });
      origin = resultExpression;
      translate = (provider, e) => {
        if (provider.Type == ProviderType.Filter || provider.Type == ProviderType.Calculate)
          return tupleAccessReplacer.ReplaceMappings(e, mappings[provider], null);
        return e;
      };
    }
  }
}
