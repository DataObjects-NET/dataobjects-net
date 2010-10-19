// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.01.21

using System;
using System.Linq.Expressions;
using Xtensive.Disposing;

namespace Xtensive.Storage.Rse.PreCompilation.Correction
{
  internal sealed class SkipTakeRewriterState
  {
    private static readonly Expression ZeroExpression = Expression.Constant(0, typeof (int));
    
    private readonly SkipTakeRewriter rewriter;

    public Expression SkipExpression { get; private set; }
    public Expression TakeExpression { get; private set; }

    public bool IsSkipTakeChain { get; set; }

    public void AddSkip(Func<int> skipCount)
    {
      var additionalSkip = SelectMaximal(ZeroExpression, skipCount);
      SkipExpression = SkipExpression==null 
        ? additionalSkip 
        : Expression.Add(SkipExpression, additionalSkip);
      if (TakeExpression!=null)
        TakeExpression = Expression.Subtract(TakeExpression, SkipExpression);
    }

    public void AddTake(Func<int> takeCount)
    {
      var additionalTake = SelectMaximal(ZeroExpression, takeCount);
      TakeExpression = TakeExpression==null 
        ? additionalTake 
        : SelectMiminal(TakeExpression, additionalTake);
    }

    public IDisposable CreateScope()
    {
      var currentState = rewriter.State;
      var newState = new SkipTakeRewriterState(currentState);
      rewriter.State = newState;
      return new Disposable(_ => rewriter.State = currentState);
    }

    private static Expression SelectMaximal(Expression oldValue, Func<int> newValue)
    {
      var newExp = CreateDelegateInvocation(newValue);
      return Expression.Condition(Expression.GreaterThan(newExp, oldValue), newExp, oldValue);
    }

    private static Expression SelectMiminal(Expression oldValue, Expression newValue)
    {
      return Expression.Condition(Expression.LessThan(newValue, oldValue), newValue, oldValue);
    }

    private static MethodCallExpression CreateDelegateInvocation(Func<int> arg)
    {
      return Expression.Call(Expression.Constant(arg), "Invoke", null);
    }

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