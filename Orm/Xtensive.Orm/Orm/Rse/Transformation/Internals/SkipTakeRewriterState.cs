// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2010.01.21

using System;
using Xtensive.Core;

namespace Xtensive.Orm.Rse.Transformation
{
  internal sealed class SkipTakeRewriterState
  {
    private readonly SkipTakeRewriter rewriter;

    public Func<ParameterContext, int> Skip { get; private set; }
    public Func<ParameterContext, int> Take { get; private set; }

    public bool IsSkipTakeChain { get; set; }

    public void AddSkip(Func<ParameterContext, int> skip)
    {
      var value = PositiveSelector(skip);
      var oldSkip = Skip;
      var oldTake = Take;
      Skip = Skip == null
        ? value
        : context => oldSkip(context) + value(context);
      if (Take != null) 
        Take = context => oldTake(context) - value(context);
    }

    public void AddTake(Func<ParameterContext, int> take)
    {
      var value = PositiveSelector(take);
      Take = Take == null
        ? value
        : MinimumSelector(Take, value);
    }

    private static Func<ParameterContext, int> MinimumSelector(Func<ParameterContext, int> takeSelector, Func<ParameterContext, int> valueSelector)
    {
      return context => {
        var take = takeSelector(context);
        var value = valueSelector(context);
        return value > take ? take : value;
      };
    }

    private static Func<ParameterContext, int> PositiveSelector(Func<ParameterContext, int> valueSelector)
    {
      return context => {
        var value = valueSelector(context);
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