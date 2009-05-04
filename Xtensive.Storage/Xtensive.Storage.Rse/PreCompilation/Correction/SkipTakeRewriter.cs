// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.04.24

using System;
using System.Linq.Expressions;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.PreCompilation.Correction
{
  internal sealed class SkipTakeRewriter : CompilableProviderVisitor
  {
    private readonly CompilableProvider origin;
    private Expression skipCount;
    private Expression takeCount;
    private int rowNumberCount;

    public CompilableProvider Rewrite()
    {
      skipCount = null;
      takeCount = null;
      rowNumberCount = 0;
      return VisitCompilable(origin);
    }

    protected override Provider VisitSkip(SkipProvider provider)
    {
      skipCount = AddCount(skipCount, provider.Count);

      var prevSkipCount = skipCount;
      var isSourceSkip = provider.Source is SkipProvider;
      if (!isSourceSkip)
        skipCount = null;
      var visitedSource = VisitCompilable(provider.Source);
      skipCount = prevSkipCount;
      if (isSourceSkip)
        return visitedSource;

      return new SkipProvider(CreateRowNumberProvider(visitedSource, ref rowNumberCount),
        (Func<int>) Expression.Lambda(skipCount).Compile());
    }

    protected override Provider VisitTake(TakeProvider provider)
    {
      takeCount = SelectMiminal(takeCount, provider.Count);

      var prevTakeCount = takeCount;
      var isSourceTake = provider.Source is TakeProvider;
      if (!isSourceTake)
        takeCount = null;
      var visitedSource = VisitCompilable(provider.Source);
      takeCount = prevTakeCount;
      if (isSourceTake)
        return visitedSource;
      
      return new TakeProvider(CreateRowNumberProvider(visitedSource, ref rowNumberCount),
        (Func<int>) Expression.Lambda(takeCount).Compile());
    }

    #region Private \ internal methods

    private static RowNumberProvider CreateRowNumberProvider(CompilableProvider source,
      ref int rowNumberCount)
    {
      var columnName = String.Format(Strings.RowNumberX, rowNumberCount++);
      return new RowNumberProvider(source, columnName);
    }

    private static Expression AddCount(Expression source, Func<int> increment)
    {
      var result = source;
      if (result == null)
        result = CreateDelegateInvocation(increment);
      else
        result = Expression.Add(result, CreateDelegateInvocation(increment));
      return result;
    }

    private static Expression SelectMiminal(Expression oldValue, Func<int> newValue)
    {
      var newExp = CreateDelegateInvocation(newValue);
      if (oldValue == null)
        return newExp;
      return Expression.Condition(Expression.LessThan(newExp, oldValue), newExp, oldValue);
    }
    
    private static MethodCallExpression CreateDelegateInvocation(Func<int> arg)
    {
      return Expression.Call(Expression.Constant(arg), "Invoke", null);
    }

    #endregion


    // Constructors

    public SkipTakeRewriter(CompilableProvider origin)
    {
      this.origin = origin;
    }
  }
}