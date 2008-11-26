// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.11

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Xtensive.Storage.Rse.Compilation.Expressions.Visitors
{
  /// <summary>
  /// Rewrites an expression tree so that locally isolatable sub-expressions are evaluated and converted into ConstantExpression nodes.
  /// </summary>
  public sealed class ConstantEvaluator : ExpressionVisitor
  {
    private readonly HashSet<Expression> candidates;

    public static Expression Evaluate(Expression expression, HashSet<Expression> candidates)
    {
      var ce = new ConstantEvaluator(candidates);
      return ce.Visit(expression);
    }

    protected override Expression Visit(Expression exp)
    {
      if (exp == null)
        return null;
      if (candidates.Contains(exp))
        return Evaluate(exp);
      return base.Visit(exp);
    }

    protected override Expression VisitUnknown(Expression expression)
    {
      return expression; 
    }

    private Expression Evaluate(Expression e)
    {
      if (e.NodeType==ExpressionType.Constant)
        return e;
      Type type = e.Type;
      if (type.IsValueType)
        e = Expression.Convert(e, typeof (object));
      Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(e);
      Func<object> fn = lambda.Compile();
      return Expression.Constant(fn(), type);
    }

    
    // Constructor

    private ConstantEvaluator(HashSet<Expression> candidates)
    {
      this.candidates = candidates;
    }
  }
}