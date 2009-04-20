// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.24

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Parameters;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using System.Linq;

namespace Xtensive.Storage.Rse.Optimization.Implementation
{
  internal sealed class OrderbyRewriter : CompilableProviderVisitor
  {
    private readonly CompilableProvider origin;
    private readonly Parameter<DirectionCollection<int>> pSortOrder = new Parameter<DirectionCollection<int>>();

    public CompilableProvider Rewrite()
    {
      using (new ParameterScope()) {
        if (origin.Type == ProviderType.Select) {
          var selectProvider = (SelectProvider)origin;
          var sourceProvider = VisitCompilable(selectProvider.Source);
          if (sourceProvider == selectProvider.Source)
            return selectProvider;
          if (pSortOrder.HasValue && pSortOrder.Value.Count > 0)
            sourceProvider = new SortProvider(sourceProvider, pSortOrder.Value);
          var provider = new SelectProvider(sourceProvider, selectProvider.ColumnIndexes);
          return provider;
        }
        else {
          var provider = VisitCompilable(origin);
          if (pSortOrder.HasValue && pSortOrder.Value.Count > 0)
            provider = new SortProvider(provider, pSortOrder.Value);
          return provider;
        }
      }
    }

    protected override Provider VisitSelect(SelectProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      if (pSortOrder.HasValue) {
        var outerDirectionCollection = new DirectionCollection<int>();
        foreach (KeyValuePair<int, Direction> sortOrder in pSortOrder.Value) {
          var columnIndex = provider.ColumnIndexes.IndexOf(sortOrder.Key);
          if (columnIndex < 0)
            throw new InvalidOperationException();
          outerDirectionCollection.Add(columnIndex, sortOrder.Value);
        }
        pSortOrder.Value = outerDirectionCollection;
        return new SelectProvider(source, provider.ColumnIndexes);
      }
      return provider;
    }

    protected override Provider VisitSort(SortProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      pSortOrder.Value = pSortOrder.HasValue
        ? new DirectionCollection<int>(pSortOrder.Value.Union(provider.Order))
        : new DirectionCollection<int>(provider.Order);
      return source;
    }

    protected override Provider VisitJoin(JoinProvider provider)
    {
      var sortOrder = new DirectionCollection<int>();
      CompilableProvider left;
      CompilableProvider right;
      using (new ParameterScope()) {
        left = VisitCompilable(provider.Left);
        if (pSortOrder.HasValue)
          sortOrder = pSortOrder.Value;
      }
      using (new ParameterScope()) {
        right = VisitCompilable(provider.Right);
        if (pSortOrder.HasValue) {
          sortOrder = new DirectionCollection<int>(
            sortOrder.Union(pSortOrder.Value.Select(p => new KeyValuePair<int,Direction>(p.Key + left.Header.Length, p.Value)))
            );
        }
      }
      if (left == provider.Left && right == provider.Right)
        return provider;
      pSortOrder.Value = sortOrder;
      return new JoinProvider(left, right, provider.Outer, provider.JoinType, provider.EqualIndexes);
    }

    protected override Provider VisitPredicateJoin(PredicateJoinProvider provider)
    {
      var sortOrder = new DirectionCollection<int>();
      CompilableProvider left;
      CompilableProvider right;
      using (new ParameterScope()) {
        left = VisitCompilable(provider.Left);
        if (pSortOrder.HasValue)
          sortOrder = pSortOrder.Value;
      }
      using (new ParameterScope()) {
        right = VisitCompilable(provider.Right);
        if (pSortOrder.HasValue) {
          sortOrder = new DirectionCollection<int>(
            sortOrder.Union(pSortOrder.Value.Select(p => new KeyValuePair<int,Direction>(p.Key + left.Header.Length, p.Value)))
            );
        }
      }
      if (left == provider.Left && right == provider.Right)
        return provider;
      pSortOrder.Value = sortOrder;
      return new PredicateJoinProvider(left, right, provider.Predicate, provider.Outer);
    }

    protected override Provider VisitApply(ApplyProvider provider)
    {
      var sortOrder = new DirectionCollection<int>();
      CompilableProvider left;
      CompilableProvider right;
      using (new ParameterScope()) {
        left = VisitCompilable(provider.Left);
        if (pSortOrder.HasValue)
          sortOrder = pSortOrder.Value;
      }
      using (new ParameterScope()) {
        right = VisitCompilable(provider.Right);
        if ((provider.ApplyType ==ApplyType.Cross || provider.ApplyType == ApplyType.Outer) && pSortOrder.HasValue) {
          sortOrder = new DirectionCollection<int>(
            sortOrder.Union(pSortOrder.Value.Select(p => new KeyValuePair<int, Direction>(p.Key + left.Header.Length, p.Value)))
            );
        }
      }
      if (left == provider.Left && right == provider.Right)
        return provider;
      pSortOrder.Value = sortOrder;
      return new ApplyProvider(provider.ApplyParameter, left, right, provider.ApplyType);
    }

    protected override Provider VisitExistence(ExistenceProvider provider)
    {
      using (new ParameterScope())
        return base.VisitExistence(provider);
    }

    protected override Provider VisitAggregate(AggregateProvider provider)
    {
      var sortOrder = new DirectionCollection<int>();
      Provider result;
      using (new ParameterScope()) {
        result = base.VisitAggregate(provider);
        if (pSortOrder.HasValue)
          sortOrder = new DirectionCollection<int>(
            pSortOrder.Value.Where(p => provider.GroupColumnIndexes.Contains(p.Key)));
      }
      if(sortOrder.Count > 0)
        pSortOrder.Value = sortOrder;
      return result;
    }

    protected override Provider VisitUnion(UnionProvider provider)
    {
      using (new ParameterScope())
        return base.VisitUnion(provider);
    }

    protected override Provider VisitConcat(ConcatProvider provider)
    {
      using (new ParameterScope())
        return base.VisitConcat(provider);
    }

    protected override Provider VisitExcept(ExceptProvider provider)
    {
      using (new ParameterScope())
        return base.VisitExcept(provider);
    }

    protected override Provider VisitIntersect(IntersectProvider provider)
    {
      using (new ParameterScope())
        return base.VisitIntersect(provider);
    }


    // Constructors

    public OrderbyRewriter(CompilableProvider origin)
    {
      this.origin = origin;
    }
  }
}