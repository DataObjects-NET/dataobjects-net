// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.24

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Parameters;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using System.Linq;

namespace Xtensive.Storage.Linq
{
  internal sealed class OrderbyRewriter : CompilableProviderVisitor
  {
    private readonly ResultExpression origin;
    private readonly Parameter<DirectionCollection<int>> pSortOrder = new Parameter<DirectionCollection<int>>();

    public ResultExpression Rewrite()
    {
      using (new ParameterScope()) {
        var provider = VisitCompilable(origin.RecordSet.Provider);
        if (pSortOrder.HasValue) {
          if (provider.Type == ProviderType.Select) {
            var selectProvider = (SelectProvider)provider;
            provider = new SelectProvider(new SortProvider(selectProvider.Source, pSortOrder.Value), selectProvider.ColumnIndexes);
          }
          else
            provider = new SortProvider(provider, pSortOrder.Value);
        }
        var result = new ResultExpression(
          origin.Type,
          provider.Result,
          origin.Mapping,
          origin.Projector,
          origin.ItemProjector);
        return result;
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


    // Constructors

    public OrderbyRewriter(ResultExpression expression)
    {
      origin = expression;
    }
  }
}