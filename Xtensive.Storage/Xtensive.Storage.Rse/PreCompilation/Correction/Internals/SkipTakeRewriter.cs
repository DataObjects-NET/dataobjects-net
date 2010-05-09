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
      if ((cp.Type==ProviderType.Take || cp.Type==ProviderType.Skip) && !State.IsSkipTakeChain) {
        var visitedProvider = (CompilableProvider) base.Visit(cp);

        bool requiresRowNumber = (State.TakeExpression!=null && !takeSupported)
          || (State.SkipExpression!=null && !skipSupported);

        // add rownumber column (if needed)
        if (requiresRowNumber) {
          // Arrray to avoid access to modified closure
          string[] columnName = {String.Format(Strings.RowNumberX, rowNumberCount++)};
          while (visitedProvider.Header.Columns.Any(column => column.Name==columnName[0]))
            columnName[0] = String.Format(Strings.RowNumberX, rowNumberCount++);
          visitedProvider = new RowNumberProvider(visitedProvider, columnName[0]);
        }
        // Add take
        if (State.TakeExpression!=null) {
          var takeExpression = State.TakeExpression;
          if (State.SkipExpression!=null)
            takeExpression = Expression.Add(takeExpression, State.SkipExpression);
          var count = Expression.Lambda<Func<int>>(takeExpression).CachingCompile();
          visitedProvider = new TakeProvider(visitedProvider, count);
        }
        // add skip
        if (State.SkipExpression!=null) {
          var count = Expression.Lambda<Func<int>>(State.SkipExpression).CachingCompile();
          visitedProvider = new SkipProvider(visitedProvider, count);
        }
        // add select removing RowNumber column
        if (requiresRowNumber)
          visitedProvider = new SelectProvider(
            visitedProvider,
            Enumerable.Range(0, visitedProvider.Header.Length - 1).ToArray());

        return visitedProvider;
      }

      if (cp.Type!=ProviderType.Take && cp.Type!=ProviderType.Skip && State.IsSkipTakeChain)
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