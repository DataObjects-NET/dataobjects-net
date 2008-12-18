// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.18

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Xtensive.Storage.Linq.Expressions.Visitors
{
  public sealed class ExpressionEvaluator : ExpressionVisitor
  {
    private readonly HashSet<Expression> candidates = new HashSet<Expression>();
    private bool couldBeEvaluated;

    public bool CanBeEvaluated(Expression e)
    {
      return candidates.Contains(e);
    }

    public ConstantExpression Evaluate(Expression e)
    {
      if (e == null)
        return null;
      if (e.NodeType == ExpressionType.Constant)
        return (ConstantExpression) e;
      Type type = e.Type;
      if (type.IsValueType)
        e = Expression.Convert(e, typeof(object));
      var lambda = Expression.Lambda<Func<object>>(e);
      var func = lambda.Compile();
      return Expression.Constant(func(), type);
    }

    protected override Expression Visit(Expression e)
    {
      if (e != null) {
        bool saved = couldBeEvaluated;
        couldBeEvaluated = true;
        base.Visit(e);
        if (couldBeEvaluated)
          if (CanEvaluateExpression(e))
            candidates.Add(e);
          else
            couldBeEvaluated = false;
        couldBeEvaluated &= saved;
      }
      return e;
    }

    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }


    private static bool CanEvaluateExpression(Expression expression)
    {
      var cex = expression as ConstantExpression;
      if (cex != null) {
        var query = cex.Value as IQueryable;
        return query==null;
      }
      var mc = expression as MethodCallExpression;
      if (mc != null && (mc.Method.DeclaringType == typeof(Enumerable) || mc.Method.DeclaringType == typeof(Queryable)))
        return false;
      if (expression.NodeType == ExpressionType.Convert && expression.Type == typeof(object))
        return true;
      if (expression is ExtendedExpression)
        return false;
      return expression.NodeType != ExpressionType.Parameter &&
        expression.NodeType != ExpressionType.Lambda;
    }


    // Constructors

    public ExpressionEvaluator(Expression e)
    {
      couldBeEvaluated = true;
      Visit(e);
    }
  }
}