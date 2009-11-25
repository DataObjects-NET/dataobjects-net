// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.25

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Rse.Helpers;
using System.Linq;

namespace Xtensive.Storage.Rse.PreCompilation.Correction
{
  [Serializable]
  public class IncludeOnIndexRewriter : CompilableProviderVisitor
  {
    protected override Provider VisitFilter(FilterProvider provider)
    {
      if (provider.Source.Type==ProviderType.Include && provider.Predicate.Body.IsTupleAccess(provider.Predicate.Parameters[0])) {
        var includeProvider = (IncludeProvider) provider.Source;
        var columnIndex = provider.Predicate.Body.GetTupleAccessArgument();
        if (columnIndex==includeProvider.Header.Length - 1
          && includeProvider.Source.Type==ProviderType.Index
            && includeProvider.FilteredColumns.SequenceEqual(Enumerable.Range(0, includeProvider.FilteredColumns.Length))) {
          // throw new NotImplementedException();

        }
      }
      return base.VisitFilter(provider);
    }
  }
}