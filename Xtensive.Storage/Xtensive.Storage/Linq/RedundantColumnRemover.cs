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
  internal class RedundantColumnRemover : CompilableProviderVisitor
  {
    private readonly Dictionary<Provider, List<int>> mappings;
    private readonly Dictionary<Parameter<Tuple>, Provider> outerProviders;
    private readonly TupleAccessProcessor tupleAccessProcessor;

    private readonly ResultExpression origin;

    public ResultExpression RemoveRedundantColumn()
    {
      var originProvider = origin.RecordSet.Provider;
      var projectorMap = tupleAccessProcessor.Process(origin.Projector)
          .OrderBy(i => i)
          .Distinct()
          .ToList();
      if (projectorMap.Count < origin.RecordSet.Header.Length) {
        mappings[originProvider] = projectorMap;
        var resultProvider = VisitCompilable(originProvider);

        if (projectorMap.Count < resultProvider.Header.Length) {

          //          var columnIndexes = new int[projectorMap.Count];
          //          for (int i = 0; i < projectorMap.Count; i++)
          //            columnIndexes[i] = map.IndexOf(projectorMap[i]);
          //          resultMap = projectorMap;
          //          return new SelectProvider(resultProvider, columnIndexes);

        }


//        var projectorMap = origin.Mapping != null
//          ? map
//          : tupleAccessProcessor.Process(origin.Projector).Distinct().ToList();
//
//        projectorMap.Sort();
//
//        if (projectorMap.Count > 0 && projectorMap.Count < map.Count)
//        {
//          var columnIndexes = new int[projectorMap.Count];
//          for (int i = 0; i < projectorMap.Count; i++)
//            columnIndexes[i] = map.IndexOf(projectorMap[i]);
//          resultMap = projectorMap;
//          return new SelectProvider(resultProvider, columnIndexes);
//        }
//        return resultProvider;

//        var rs = AddSelectProvider((CompilableProvider)resultProvider).Result;

        var rs = resultProvider.Result;
        var withAggregate = mappings.Keys.Any(k => k.Type==ProviderType.Aggregate);

        var originGroups = origin.RecordSet.Header.ColumnGroups.ToList();
        var rsGroups = rs.Header.ColumnGroups.ToList();
        var groupMap = new List<int>();

        foreach(var group in originGroups)
          foreach(var rsGroup in rsGroups)
            if (group.HierarchyInfoRef.TypeName == rsGroup.HierarchyInfoRef.TypeName)
              groupMap.Add(originGroups.IndexOf(group));

//        var projector = withAggregate
//          ? origin.Projector
//          : (Expression<Func<RecordSet, object>>)tupleAccessProcessor.ReplaceMappings(origin.Projector, resultMap, groupMap);
//        var itemProjector = origin.ItemProjector == null
//          ? null
//          : (LambdaExpression)tupleAccessProcessor.ReplaceMappings(origin.ItemProjector, resultMap, groupMap);
//        var result = new ResultExpression(origin.Type, rs, (origin.Mapping == null) ? null : new ResultMapping(), projector, itemProjector);
//        return result;
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
      List<int> value;
      mappings.TryGetValue(provider, out value);
      if (value != null)
        mappings.Add(provider.Source, value
          .Union(tupleAccessProcessor.Process(provider.Predicate))
          .OrderBy(i => i)
          .Distinct()
          .ToList());
      return base.VisitFilter(provider);
    }

    /*{
            case ProviderType.Apply:
            case ProviderType.Join: {
              var bp = (BinaryProvider)provider;
              var leftMap = mappings[bp.Left];
              var rightMap = mappings[bp.Right];
              var leftCount = ((BinaryProvider)provider).Left.Header.Columns.Count;
              foreach (var item in rightMap)
                leftMap.Add(item + leftCount);
              leftMap.Sort();
              leftMap = leftMap.Distinct().ToList();
              mappings[provider] = leftMap;
              if (provider.Type == ProviderType.Join)
                return ((JoinProvider)provider).EqualIndexes
                  .Select(pair => new Pair<int>(leftMap.IndexOf(pair.First), rightMap.IndexOf(pair.Second)))
                  .ToArray();
              return null;
            }*/

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
      var equalIndexes = provider.EqualIndexes
        .Select(pair => new Pair<int>(leftMap.IndexOf(pair.First), rightMap.IndexOf(pair.Second)))
        .ToArray();
      if (left == provider.Left && right == provider.Right)
        return provider;
      return new JoinProvider(left, right, provider.Outer, provider.JoinType,
        equalIndexes != null ? (Pair<int>[])equalIndexes : provider.EqualIndexes);
    }

    protected override Provider VisitSort(SortProvider provider)
    {
      List<int> value;
      mappings.TryGetValue(provider, out value);
      if (value != null) {
        var map = new List<int>();
        foreach (var key in provider.Order.Keys)
          map.Add(key);
        mappings.Add(provider.Source, value
          .Union(map)
          .OrderBy(i => i)
          .Distinct()
          .ToList());
      }
      return base.VisitSort(provider);
    }

    protected override Provider VisitApply(ApplyProvider provider)
    {
      List<int> leftMap;
      List<int> rightMap;

      SplitMappings(provider, out leftMap, out rightMap);
      mappings.Add(provider.Left, leftMap);
      mappings.Add(provider.Right, rightMap);

      // note: order of visiting does matter
      outerProviders.Add(provider.LeftItemParameter, provider.Left);
      OnRecursionEntrance(provider);
      var right = VisitCompilable(provider.Right);
      var left = VisitCompilable(provider.Left);
      OnRecursionExit(provider);
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
        .Union(provider.CalculatedColumns.SelectMany(c => tupleAccessProcessor.Process(c.Expression)))
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

    private void RegisterOuterMapping(Parameter<Tuple> parameter, int value)
    {
      var map = mappings[outerProviders[parameter]];
      int index = map.BinarySearch(value);
      if (index < 0)
        map.Insert(~index, value);
    }

    private int ResolveOuterMapping(Parameter<Tuple> parameter, int value)
    {
      return mappings[outerProviders[parameter]].IndexOf(value);
    }

    #endregion

    // Constructor

    public RedundantColumnRemover(ResultExpression resultExpression)
    {
      mappings = new Dictionary<Provider, List<int>>();
      outerProviders = new Dictionary<Parameter<Tuple>, Provider>();
      tupleAccessProcessor = new TupleAccessProcessor(RegisterOuterMapping, ResolveOuterMapping);
      origin = resultExpression;
      translate = (provider, e) => {
        if (provider.Type == ProviderType.Filter || provider.Type == ProviderType.Calculate)
          return tupleAccessProcessor.ReplaceMappings(e, mappings[provider], null);
        return e;
      };
    }
  }
}
