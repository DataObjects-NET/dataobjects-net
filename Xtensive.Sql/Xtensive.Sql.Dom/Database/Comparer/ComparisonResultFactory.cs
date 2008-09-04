// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.03

using System;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  public class ComparisonResultFactory
  {
    private readonly ComparisonContext context;

    public ComparisonResultFactory(ComparisonContext context)
    {
      this.context = context;
    }


    public TResult CreateComparisonResult<T, TResult>(T originalNode, T newNode, ComparisonResultType resultType)
      where T : Node
      where TResult : NodeComparisonResult, IComparisonResult<T>, new()
    {
      TResult result = CreateComparisonResult<T, TResult>(originalNode, newNode);
      result.ResultType = resultType;
      return result;
    }

    public TResult CreateComparisonResult<T, TResult>(T originalNode, T newNode)
      where T : Node
      where TResult : NodeComparisonResult, IComparisonResult<T>, new()
    {
      if (originalNode==null && newNode==null) {
        return new TResult();
      }
      IComparisonResult currentResult;
      if (!context.Registry.TryGetValue(originalNode, newNode, out currentResult)) {
        var newResult = new TResult();
        newResult.Initialize(originalNode, newNode);
        context.Registry.Register(originalNode, newNode, newResult);
        return newResult;
      }
      return (TResult) currentResult;
    }
  }
}