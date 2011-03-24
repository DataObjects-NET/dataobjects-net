// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.04.24

using System;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Rse.Resources;
using Xtensive.Orm;

namespace Xtensive.Storage.Rse.PreCompilation.Correction
{
  internal sealed class SkipTakeRewriter : CompilableProviderVisitor
  {
    private int rowNumberCount;
    private readonly CompilableProvider origin;
    private readonly bool takeSupported;
    private readonly bool skipSupported;

    public SkipTakeRewriterState State { get; set; }

    public CompilableProvider Rewrite()
    {
      return VisitCompilable(origin);
    }

    protected override Provider Visit(CompilableProvider cp)
    {
      var isPagingProvider = cp.Type.In(ProviderType.Take, ProviderType.Skip, ProviderType.Paging);
      if (isPagingProvider && !State.IsSkipTakeChain) {
        var visitedProvider = (CompilableProvider) base.Visit(cp);

        var requiresRowNumber = (State.Take!=null && !takeSupported) || (State.Skip!=null && !skipSupported);

        // add rownumber column (if needed)
        if (requiresRowNumber) {
          // Arrray to avoid access to modified closure
          string[] columnName = {String.Format(Strings.RowNumberX, rowNumberCount++)};
          while (visitedProvider.Header.Columns.Any(column => column.Name==columnName[0]))
            columnName[0] = String.Format(Strings.RowNumberX, rowNumberCount++);
          visitedProvider = new RowNumberProvider(visitedProvider, columnName[0]);
        }

        if (State.Take != null && State.Skip != null)
          visitedProvider = new PagingProvider(visitedProvider, State.Skip, State.Take);
        else if (State.Take != null)
          visitedProvider = new TakeProvider(visitedProvider, State.Take);
        else
          visitedProvider = new SkipProvider(visitedProvider, State.Skip);

        // add select removing RowNumber column
        if (requiresRowNumber)
          visitedProvider = new SelectProvider(
            visitedProvider,
            Enumerable.Range(0, visitedProvider.Header.Length - 1).ToArray());

        return visitedProvider;
      }

      if (!isPagingProvider && State.IsSkipTakeChain)
        using (State.CreateScope())
          return base.Visit(cp);

      return base.Visit(cp);
    }

    protected override Provider VisitSkip(SkipProvider provider)
    {
      State.IsSkipTakeChain = true;
      var visitedSource = VisitCompilable(provider.Source);
      State.AddSkip(provider.Count);
      return visitedSource;
    }

    protected override Provider VisitTake(TakeProvider provider)
    {
      State.IsSkipTakeChain = true;
      var visitedSource = VisitCompilable(provider.Source);
      State.AddTake(provider.Count);
      return visitedSource;
    }

    protected override Provider VisitPaging(PagingProvider provider)
    {
      State.IsSkipTakeChain = true;
      var visitedSource = VisitCompilable(provider.Source);
      State.AddSkip(provider.Skip);
      State.AddTake(provider.Take);
      return visitedSource;
    }


    // Constructors

    public SkipTakeRewriter(CompilableProvider origin, bool takeSupported, bool skipSupported)
    {
      State = new SkipTakeRewriterState(this);
      this.origin = origin;
      this.takeSupported = takeSupported;
      this.skipSupported = skipSupported;
    }
  }
}