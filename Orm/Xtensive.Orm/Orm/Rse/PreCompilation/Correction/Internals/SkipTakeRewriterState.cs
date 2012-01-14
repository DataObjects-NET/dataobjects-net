// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.01.21

using System;
using Xtensive.Disposing;

namespace Xtensive.Orm.Rse.PreCompilation.Correction
{
  internal sealed class SkipTakeRewriterState
  {
    private readonly SkipTakeRewriter rewriter;

    public Func<int> Skip { get; private set; }
    public Func<int> Take { get; private set; }

    public bool IsSkipTakeChain { get; set; }

    public void AddSkip(Func<int> skip)
    {
      var value = PositiveSelector(skip);
      var oldSkip = Skip;
      var oldTake = Take;
      Skip = Skip == null
        ? value
        : () => oldSkip() + value();
      if (Take != null) 
        Take = () => oldTake() - value();
    }

    public void AddTake(Func<int> take)
    {
      var value = PositiveSelector(take);
      Take = Take == null
        ? value
        : MinimumSelector(Take, value);
    }

    private static Func<int> MinimumSelector(Func<int> takeSelector, Func<int> valueSelector)
    {
      return () => {
        var take = takeSelector();
        var value = valueSelector();
        return value > take ? take : value;
      };
    }

    private static Func<int> PositiveSelector(Func<int> valueSelector)
    {
      return () => {
        var value = valueSelector();
        return value > 0 ? value : 0;
      };
    }

    public IDisposable CreateScope()
    {
      var currentState = rewriter.State;
      var newState = new SkipTakeRewriterState(currentState);
      rewriter.State = newState;
      return new Disposable(_ => rewriter.State = currentState);
    }


    // Constructors

    private SkipTakeRewriterState(SkipTakeRewriterState state)
    {
      rewriter = state.rewriter;
    }

    public SkipTakeRewriterState(SkipTakeRewriter rewriter)
    {
      this.rewriter = rewriter;
    }
  }
}