// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.24

using System;
using System.Diagnostics;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Linq
{
  public class OrderbyRewriter : CompilableProviderVisitor
  {
    protected override Provider VisitSelect(SelectProvider provider)
    {
      return base.VisitSelect(provider);
    }

    protected override Provider VisitSort(SortProvider provider)
    {
      return base.VisitSort(provider);
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
  }
}