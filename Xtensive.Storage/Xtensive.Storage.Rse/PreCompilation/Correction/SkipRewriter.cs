// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.04.24

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Parameters;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using System.Linq;

namespace Xtensive.Storage.Rse.PreCompilation.Correction
{
  internal sealed class SkipRewriter : CompilableProviderVisitor
  {
    private readonly CompilableProvider origin;

    public CompilableProvider Rewrite()
    {
      return VisitCompilable(origin);
    }

    protected override Provider VisitSkip(SkipProvider provider)
    {
      const string rowNumber = "RowNumber";
      return new SkipProvider(new RowNumberProvider(provider.Source, rowNumber), provider.Count);
    }


    // Constructors

    public SkipRewriter(CompilableProvider origin)
    {
      this.origin = origin;
    }
  }
}