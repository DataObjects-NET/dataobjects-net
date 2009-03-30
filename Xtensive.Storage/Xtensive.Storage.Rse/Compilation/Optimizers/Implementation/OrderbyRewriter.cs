// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.24

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Parameters;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using System.Linq;

namespace Xtensive.Storage.Rse.Compilation.Optimizers.Implementation
{
  internal sealed class OrderbyRewriter : CompilableProviderVisitor
  {
    private readonly CompilableProvider origin;
    private readonly Parameter<DirectionCollection<int>> pSortOrder = new Parameter<DirectionCollection<int>>();

    public CompilableProvider Rewrite()
    {
      using (new ParameterScope()) {
        var provider = VisitCompilable(origin);
        if (pSortOrder.HasValue && pSortOrder.Value.Count > 0) {
          if (provider.Type == ProviderType.Select) {
            var selectProvider = (SelectProvider)provider;
            provider = new SelectProvider(new SortProvider(selectProvider.Source, pSortOrder.Value), selectProvider.ColumnIndexes);
          }
          else
            provider = new SortProvider(provider, pSortOrder.Value);
        }
        return provider;
      }
    }

    protected override Provider VisitSelect(SelectProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      if (pSortOrder.HasValue) {
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
      if (sortOrder.Count == 0)
        return provider;
      pSortOrder.Value = sortOrder;
      return new JoinProvider(left, right, provider.Outer, provider.JoinType, provider.EqualIndexes);
    }

    protected override Provider VisitPredicateJoin(PredicateJoinProvider provider)
    {
      return base.VisitPredicateJoin(provider);
    }

    protected override Provider VisitApply(ApplyProvider provider)
    {
      return base.VisitApply(provider);
    }

    protected override Provider VisitExistence(ExistenceProvider provider)
    {
      return base.VisitExistence(provider);
    }

    protected override Provider VisitAggregate(AggregateProvider provider)
    {
      return base.VisitAggregate(provider);
    }


    // Constructors

    public OrderbyRewriter(CompilableProvider origin)
    {
      this.origin = origin;
    }
  }
}