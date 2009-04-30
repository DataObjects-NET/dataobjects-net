// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.04.24

using System;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.PreCompilation.Correction
{
  internal sealed class SkipRewriter : CompilableProviderVisitor
  {
    private readonly CompilableProvider origin;
    private int count;

    public CompilableProvider Rewrite()
    {
      count = 0;
      return VisitCompilable(origin);
    }

    protected override Provider VisitSkip(SkipProvider provider)
    {
      var visitedSource = VisitCompilable(provider.Source);
      var columnName = String.Format(Strings.RowNumberX, count++);
      return new SkipProvider(new RowNumberProvider(visitedSource, columnName), provider.Count);
    }


    // Constructors

    public SkipRewriter(CompilableProvider origin)
    {
      this.origin = origin;
    }
  }
}