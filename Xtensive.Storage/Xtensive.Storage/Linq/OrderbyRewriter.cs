// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.24

using System;
using System.Diagnostics;
using Xtensive.Core.Collections;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using System.Linq;

namespace Xtensive.Storage.Linq
{
  internal sealed class OrderbyRewriter : CompilableProviderVisitor
  {
    private readonly ResultExpression origin;
    private DirectionCollection<int> sortOrder;

    public ResultExpression Rewrite()
    {
      var provider = (CompilableProvider)Visit(origin.RecordSet.Provider);
      if (sortOrder != null) {
        if (provider.Type == ProviderType.Select) {
          var selectProvider = (SelectProvider)provider;
          provider = new SelectProvider(new SortProvider(selectProvider.Source, sortOrder), selectProvider.ColumnIndexes);
        }
        else
          provider = new SortProvider(provider, sortOrder);
      }
      var result = new ResultExpression(
        origin.Type,
        provider.Result,
        origin.Mapping,
        origin.Projector,
        origin.ItemProjector);
      return result;
    }

    protected override Provider VisitSelect(SelectProvider provider)
    {
      var source = (CompilableProvider)Visit(provider.Source);
      if (sortOrder != null) {
        return new SelectProvider(source, provider.ColumnIndexes);
      }
      return provider;
    }

    protected override Provider VisitSort(SortProvider provider)
    {
      var source = Visit(provider.Source);
      sortOrder = sortOrder != null
        ? new DirectionCollection<int>(sortOrder.Union(provider.Order))
        : new DirectionCollection<int>(provider.Order);
      return source;
    }

    protected override Provider VisitJoin(JoinProvider provider)
    {

      return base.VisitJoin(provider);
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