// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.18

using System;
using System.Linq.Expressions;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Linq.Expressions.Visitors
{
  public class ParameterExtractor : ExpressionVisitor
  {
    private readonly ExpressionEvaluator evaluator;
    private bool containsMemberAccess;

    public bool IsParameter(Expression e)
    {
      if (!evaluator.CanBeEvaluated(e))
        return false;
      containsMemberAccess = false;
      Visit(e);
      return containsMemberAccess;
    }

    public Expression<Func<object>> ExtractParameter(Expression expression)
    {
      Type type = expression.Type;
      if (type.IsValueType)
        expression = Expression.Convert(expression, typeof(object));
      var lambda = Expression.Lambda<Func<object>>(expression);
      return lambda;
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      containsMemberAccess = true;
      return base.VisitMemberAccess(m);
    }

    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }


    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ParameterExtractor(ExpressionEvaluator evaluator)
    {
      this.evaluator = evaluator;
    }
  }
}