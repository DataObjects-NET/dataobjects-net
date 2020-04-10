// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.01.21

using System;

namespace Xtensive.Orm.Rse.Transformation
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

    internal readonly ref struct SkipTakeRewriterScope
    {
      private readonly SkipTakeRewriter rewriter;
      private readonly SkipTakeRewriterState prevState;

      public void Dispose() => rewriter.State = prevState;

      public SkipTakeRewriterScope(SkipTakeRewriter rewriter, SkipTakeRewriterState prevState)
      {
        this.rewriter = rewriter;
        this.prevState = prevState;
      }
    }

    public SkipTakeRewriterScope CreateScope()
    {
      var currentState = rewriter.State;
      rewriter.State = new SkipTakeRewriterState(currentState);
      return new SkipTakeRewriterScope(rewriter, currentState);
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