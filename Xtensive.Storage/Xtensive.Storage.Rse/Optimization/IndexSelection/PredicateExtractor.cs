// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.14

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Optimization.IndexSelection
{
  internal class PredicateExtractor : CompilableProviderVisitor
  {
    private Expression predicate;

    protected override Provider VisitFilter(FilterProvider provider)
    {
      var primaryProvider = provider.Source as IndexProvider;
      if (primaryProvider != null)
        predicate = provider.Predicate;
      return base.VisitFilter(provider);
    }
  }
}