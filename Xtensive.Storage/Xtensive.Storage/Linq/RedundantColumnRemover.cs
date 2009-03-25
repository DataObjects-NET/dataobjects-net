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
    private readonly Dictionary<Provider, List<int>> mapping;
    private readonly Dictionary<Parameter<Tuple>, Provider> outerProviders;
    private readonly TupleAccessProcessor tupleAccessProcessor;

    private readonly ResultExpression origin;
    private List<int> resultMap;

    public ResultExpression RemoveRedundantColumn()
    {
      var map = new List<int>();
      if (origin.Mapping != null) {
        if (origin.Mapping.Fields.Count == 0)
          map.Add(origin.Mapping.Segment.Offset);
        else {
          foreach (var item in origin.Mapping.Fields)
            map.Add(item.Value.Offset);
          map.Sort();
          map = map.Distinct().ToList();
        }
      }
      if (map.Count < origin.RecordSet.Header.Length) {
        var provider = origin.RecordSet.Provider;
        mapping.Add(provider, map);
        var resultProvider = Visit(provider);

        resultMap = mapping[provider];
        var rs = AddSelectProvider((CompilableProvider)resultProvider).Result;

        var withAggregate = mapping.Keys.Any(k => k.Type==ProviderType.Aggregate);

        var originGroups = origin.RecordSet.Header.ColumnGroups.ToList();
        var rsGroups = rs.Header.ColumnGroups.ToList();
        var groupMap = new List<int>();

        foreach(var group in originGroups)
          foreach(var rsGroup in rsGroups)
            if (group.HierarchyInfoRef.TypeName == rsGroup.HierarchyInfoRef.TypeName)
              groupMap.Add(originGroups.IndexOf(group));

        var projector = withAggregate
          ? origin.Projector
          : (Expression<Func<RecordSet, object>>)tupleAccessProcessor.ReplaceMappings(origin.Projector, resultMap, groupMap);
        var itemProjector = origin.ItemProjector == null
          ? null
          : (LambdaExpression)tupleAccessProcessor.ReplaceMappings(origin.ItemProjector, resultMap, groupMap);
        var result = new ResultExpression(origin.Type, rs, (origin.Mapping == null) ? null : new ResultMapping(), projector, itemProjector);
        return result;
      }
      return origin;
    }

    #region Visit methods

    protected override Provider VisitIndex(IndexProvider provider)
    {
      var columnsCount = provider.Header.Length;
      var value = mapping[provider];
      if (columnsCount > value.Count)
        return new SelectProvider(provider, value.ToArray());
      return provider;
    }

    protected override Provider VisitFilter(FilterProvider provider)
    {
      List<int> value;
      mapping.TryGetValue(provider, out value);
      if (value != null)
        mapping.Add(provider.Source, value
          .Union(tupleAccessProcessor.Process(provider.Predicate))
          .OrderBy(i => i)
          .Distinct()
          .ToList());
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

      mapping.Add(provider.Left, leftMap.OrderBy(i=>i).Distinct().ToList());
      mapping.Add(provider.Right, rightMap.OrderBy(i => i).Distinct().ToList());

      return base.VisitJoin(provider);
    }

    protected override Provider VisitSort(SortProvider provider)
    {
      List<int> value;
      mapping.TryGetValue(provider, out value);
      if (value != null) {
        var map = new List<int>();
        foreach (var key in provider.Order.Keys)
          map.Add(key);
        mapping.Add(provider.Source, value
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
      mapping.Add(provider.Left, leftMap);
      mapping.Add(provider.Right, rightMap);

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
      mapping.TryGetValue(provider, out value);
      if (value != null) {
        var map = new List<int>();
        foreach (var key in provider.Order.Keys)
          map.Add(key);
        mapping.Add(provider.Source, value
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
      mapping.TryGetValue(provider, out value);
      if (value != null) {
        var map = new List<int>();
        foreach (var column in provider.AggregateColumns)
          map.Add(column.SourceIndex);
        foreach (var index in provider.GroupColumnIndexes)
          map.Add(index);
        mapping.Add(provider.Source, value
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
      var value = mapping[provider]
        .Where(i => i < sourceLength)
        .Union(provider.CalculatedColumns.SelectMany(c => tupleAccessProcessor.Process(c.Expression)))
        .OrderBy(i => i)
        .Distinct()
        .ToList();
      mapping.Add(provider.Source, value);
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
      case ProviderType.Filter:
      case ProviderType.Calculate:
        break;
      case ProviderType.Apply:
      case ProviderType.Join:
        var left = mapping[provider.Sources[0]];
        var right = mapping[provider.Sources[1]];
        var leftCount = ((BinaryProvider) provider).Left.Header.Columns.Count;
        foreach (var item in right)
          left.Add(item + leftCount);
        left.Sort();
        left = left.Distinct().ToList();
        mapping[provider] = left;
        if (provider.Type == ProviderType.Join)
          return ((JoinProvider)provider).EqualIndexes
            .Select(pair => new Pair<int>(left.IndexOf(pair.First), right.IndexOf(pair.Second)))
            .ToArray();
        return null;

      case ProviderType.Aggregate:
        var aggregateProvider = (AggregateProvider) provider;
        var value = mapping[aggregateProvider.Source];
        var columns = new List<AggregateColumnDescriptor>();
        var groupIndexes = new List<int>();
        foreach (var column in aggregateProvider.AggregateColumns)
          columns.Add(new AggregateColumnDescriptor(column.Name, value.IndexOf(column.SourceIndex), column.AggregateType));
        foreach(var index in aggregateProvider.GroupColumnIndexes)
          groupIndexes.Add(value.IndexOf(index));
        return new Pair<int[], AggregateColumnDescriptor[]> (groupIndexes.ToArray(), columns.ToArray());
        
      case ProviderType.Sort:
        var sortProvider = (SortProvider) provider;
        var map = mapping[sortProvider.Source];
        var orders = new DirectionCollection<int>();
        foreach(KeyValuePair<int,Direction> order in sortProvider.Order)
          orders.Add(map.IndexOf(order.Key),order.Value);
        return orders;
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
          mapping.Add(provider.Sources[0], mapping[provider]);
          break;
      }
    }

    #endregion

    #region Private methods

    private CompilableProvider AddSelectProvider(CompilableProvider resultProvider)
    {
      var map = mapping[origin.RecordSet.Provider];
      var projectorMap = origin.Mapping!=null
        ? origin.Mapping.GetColumns().ToList()
        : tupleAccessProcessor.Process(origin.Projector);

      if (projectorMap.Count > 0 && projectorMap.Count < map.Count) {
        var columnIndexes = new int[projectorMap.Count];
        for (int i = 0; i < projectorMap.Count; i++)
          columnIndexes[i] = map.IndexOf(projectorMap[i]);
        resultMap = projectorMap;
        return new SelectProvider(resultProvider, columnIndexes);
      }
      return resultProvider;
    }

    private void SplitMappings(BinaryProvider provider, out List<int> leftMapping, out List<int> rightMapping)
    {
      var binaryMapping = mapping[provider];
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
      var map = mapping[outerProviders[parameter]];
      int index = map.BinarySearch(value);
      if (index < 0)
        map.Insert(~index, value);
    }

    private int ResolveOuterMapping(Parameter<Tuple> parameter, int value)
    {
      return mapping[outerProviders[parameter]].IndexOf(value);
    }

    #endregion

    // Constructor

    public RedundantColumnRemover(ResultExpression resultExpression)
    {
      mapping = new Dictionary<Provider, List<int>>();
      outerProviders = new Dictionary<Parameter<Tuple>, Provider>();
      tupleAccessProcessor = new TupleAccessProcessor(RegisterOuterMapping, ResolveOuterMapping);
      origin = resultExpression;
      translate = (provider, e) => {
        if (provider.Type == ProviderType.Filter || provider.Type == ProviderType.Calculate)
          return tupleAccessProcessor.ReplaceMappings(e, mapping[provider.Sources[0]], null);
        return e;
      };
    }
  }
}
