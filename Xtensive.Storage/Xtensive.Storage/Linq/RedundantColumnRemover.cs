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
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Linq
{
  ///<summary>
  /// Removes redundant columns from result <see cref="RecordSet"/>.
  ///</summary>
  internal class RedundantColumnRemover : CompilableProviderVisitor
  {
    private readonly Dictionary<Provider, List<int>> mapping;
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
      if (map.Count < origin.RecordSet.Header.Columns.Count) {
        var provider = origin.RecordSet.Provider;
        mapping.Add(provider, map);
        var resultProvider = Visit(provider);

        resultMap = mapping[provider];
        var rs = AddSelectProvider((CompilableProvider)resultProvider).Result;
        
        var withAggregate = false;
        foreach (var item in mapping)
          if (item.Key.Type == ProviderType.Aggregate) {
            withAggregate = true;
            break;
          }

        var originGroups = origin.RecordSet.Provider.Result.Header.ColumnGroups.ToList();
        var rsGroups= rs.Header.ColumnGroups.ToList();
        var groupMap = new List<int>();

        foreach(var group in originGroups)
          foreach(var rsGroup in rsGroups)
            if (group.HierarchyInfoRef.TypeName == rsGroup.HierarchyInfoRef.TypeName)
              groupMap.Add(originGroups.IndexOf(group));


        var projector = withAggregate ? origin.Projector : (Expression<Func<RecordSet, object>>)tupleAccessProcessor.ReplaceMappings(origin.Projector, resultMap, groupMap);
        var itemProjector = origin.ItemProjector == null ? null : (LambdaExpression)tupleAccessProcessor.ReplaceMappings(origin.ItemProjector, resultMap, groupMap);
        var result = new ResultExpression(origin.Type, rs, (origin.Mapping == null)? null : new ResultMapping(), projector, itemProjector);
        return result;
      }
      return origin;
    }

    #region Visit methods

    protected override Provider VisitIndex(IndexProvider provider)
    {
      List<int> value;
      mapping.TryGetValue(provider, out value);
      value.Sort();
      value = value.Distinct().ToList();
      var columnsCount = provider.Header.Columns.Count;
      int i = value.Count - 1;
      while (i >= 0) {
        if (value[i] >= columnsCount) {
          value.RemoveAt(i);
          i--;
        }
        else
          break;
      }
      if (columnsCount > value.Count) 
        return new SelectProvider(provider, value.ToArray());
      return provider;
    }

    protected override Provider VisitFilter(FilterProvider provider)
    {
      List<int> value;
      mapping.TryGetValue(provider, out value);
      if (value != null)
        mapping.Add(provider.Source, value.Union(tupleAccessProcessor.Process(provider.Predicate)).ToList());
      return base.VisitFilter(provider);
    }

    protected override Provider VisitJoin(JoinProvider provider)
    {
      List<int> value;
      var leftCount = provider.Left.Header.Columns.Count;
      var joinMapLeft = new List<int>();
      var joinMapRight = new List<int>();
      
      mapping.TryGetValue(provider, out value);
      if (value != null) {
        value.Sort();
        value = value.Distinct().ToList();
        int index = 0;
        while (index < origin.RecordSet.Header.Columns.Count)
          if (value[index] < leftCount) {
            joinMapLeft.Add(value[index]);
            index++;
          }
          else
            break;
        foreach(var item in provider.EqualIndexes)
          joinMapLeft.Add(item.First);
        mapping.Add(provider.Sources[0], joinMapLeft);

        for (int i = index; i < value.Count; i++ )
          joinMapRight.Add(value[i] - leftCount);
        foreach (var item in provider.EqualIndexes)
          joinMapRight.Add(item.Second);
        mapping.Add(provider.Sources[1], joinMapRight);
      }

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
        mapping.Add(provider.Source, value.Union(map).ToList());
      }
      return base.VisitSort(provider);
    }

    protected override Provider VisitReindex(ReindexProvider provider)
    {
      List<int> value;
      mapping.TryGetValue(provider, out value);
      if (value != null) {
        var map = new List<int>();
        foreach (var key in provider.Order.Keys)
          map.Add(key);
        mapping.Add(provider.Source, value.Union(map).ToList());
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
        mapping.Add(provider.Source, value.Union(map).ToList());
      }
      return base.VisitAggregate(provider);
    }

    protected override Provider VisitCalculate(CalculateProvider provider)
    {
      List<int> value;
      mapping.TryGetValue(provider, out value);
      if (value != null) {
        var map = new List<int>();
        foreach(var column in provider.CalculatedColumns){
          map.AddRange(tupleAccessProcessor.Process(column.Expression));
          map.Add(column.Index);}
        mapping.Add(provider.Source, value.Union(map).ToList());
      }
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
      case ProviderType.Join:
        List<int> left, right;
        mapping.TryGetValue(provider.Sources[0], out left);
        mapping.TryGetValue(provider.Sources[1], out right);
        var count = ((JoinProvider) provider).Left.Header.Columns.Count;
        foreach (var item in right)
          left.Add(item + count);
        left.Sort();
        left = left.Distinct().ToList();
        mapping[provider] = left;
        var equalIndexes = ((JoinProvider) provider).EqualIndexes;
        var result = new List<Pair<int>>();
        foreach(var pair in equalIndexes)
          result.Add(new Pair<int>(left.IndexOf(pair.First), right.IndexOf(pair.Second)));
        return result.ToArray();

      case ProviderType.Aggregate:
        var value = ModifyMapping(provider);
        var aProvider = (AggregateProvider) provider;
        var columns = new List<AggregateColumnDescriptor>();
        var groupIndexes = new List<int>();
        foreach (var column in aProvider.AggregateColumns)
          columns.Add(new AggregateColumnDescriptor(column.Name, value.IndexOf(column.SourceIndex), column.AggregateType));
        foreach(var index in aProvider.GroupColumnIndexes)
          groupIndexes.Add(value.IndexOf(index));
        return new Pair<int[], AggregateColumnDescriptor[]> (groupIndexes.ToArray(), columns.ToArray());
        
      case ProviderType.Sort:
        var map = ModifyMapping(provider);
        var sProvider = (SortProvider) provider;
        var orders = new DirectionCollection<int>();
        foreach(KeyValuePair<int,Direction> order in sProvider.Order)
          orders.Add(map.IndexOf(order.Key),order.Value);
        return orders;

      default:
        ModifyMapping(provider);
        break;
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
        break;
      default:
        AddValueToMapping(provider);
        break;
      }
    }

    #endregion

    #region Private methods

    private List<int> ModifyMapping(Provider provider)
    {
      List<int> value;
      mapping.TryGetValue(provider.Sources[0], out value);
      value.Sort();
      value = value.Distinct().ToList();
      mapping[provider] = value;
      return value;
    }

    private void AddValueToMapping(Provider provider)
    {
      List<int> value;
      mapping.TryGetValue(provider, out value);
      mapping.Add(provider.Sources[0], value);
    }

    private CompilableProvider AddSelectProvider(CompilableProvider resultProvider)
    {
      var map = mapping[origin.RecordSet.Provider];
      //var projectorMap = tupleAccessProcessor.Process(origin.Projector);
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

    #endregion


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public RedundantColumnRemover(ResultExpression resultExpression)
    {
      tupleAccessProcessor = new TupleAccessProcessor();
      mapping = new Dictionary<Provider, List<int>>();
      origin = resultExpression;
      translate = (provider, e) => {
        if (provider.Type == ProviderType.Filter || provider.Type == ProviderType.Calculate)
          return tupleAccessProcessor.ReplaceMappings(e, ModifyMapping(provider), null);
        return e;
      };
    }
  }
}