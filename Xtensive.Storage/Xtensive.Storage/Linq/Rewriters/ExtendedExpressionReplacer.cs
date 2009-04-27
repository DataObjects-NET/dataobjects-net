// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.27

using System;
using System.Linq.Expressions;
using Xtensive.Core.Linq;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Linq.Rewriters
{
  internal class ExtendedExpressionReplacer : ExpressionVisitor
  {
    private readonly Func<Expression, Expression> replaceDelegate;
    private readonly CompilableProviderVisitor providerVisitor;

    public Expression Replace(Expression e)
    {
      return Visit(e);
    }

    protected override Expression Visit(Expression e)
    {
      if (e == null)
        return null;
      var result = replaceDelegate(e);
      return result ?? base.Visit(e);
    }

    protected override Expression VisitUnknown(Expression e)
    {
      if (!e.IsResult())
        return base.VisitUnknown(e);
      var resultExpression = (ResultExpression)e;
      var itemProjector = Visit(resultExpression.ItemProjector);
      var provider = providerVisitor.VisitCompilable(resultExpression.RecordSet.Provider);
      var providerChanged = provider != resultExpression.RecordSet.Provider;
      var projectorChanged = itemProjector != resultExpression.ItemProjector;
      if (providerChanged || projectorChanged) {
        if (providerChanged)
          return resultExpression = new ResultExpression(
            resultExpression.Type, 
            provider.Result, 
            resultExpression.Mapping, 
            (LambdaExpression)itemProjector, 
            resultExpression.ResultType);
        return resultExpression = new ResultExpression(
          resultExpression.Type, 
          resultExpression.RecordSet, 
          resultExpression.Mapping, 
          (LambdaExpression)itemProjector, 
          resultExpression.ResultType);
      }
      return e;
    }

    private Expression ExpressionTranslator(Provider provider, Expression original)
    {
      var result = Visit(original);
      return result ?? original;
    }

    // Constructors

    public ExtendedExpressionReplacer(Func<Expression,Expression> replaceDelegate)
    {
      this.replaceDelegate = replaceDelegate;
      providerVisitor = new CompilableProviderVisitor(ExpressionTranslator);
    }
  }
}