// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.11

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Rse.Compilation.Expressions.Visitors
{
  public sealed class ParameterAccessTranslator : ExpressionVisitor
  {
    #region Nested helper classes

    class MemberAccessChecker : ExpressionVisitor
    {
      private bool containsMemberAccess;

      public static bool ContainsMemberAccess(Expression expression)
      {
        var mac = new MemberAccessChecker();
        mac.Visit(expression);
        return mac.containsMemberAccess;
      }

      protected override Expression VisitMemberAccess(MemberExpression m)
      {
        containsMemberAccess = true;
        return base.VisitMemberAccess(m);
      }

      protected override Expression VisitUnknown(Expression expression)
      {
        return expression;
      }

      private MemberAccessChecker()
      {}
    }

    #endregion
    
    private readonly HashSet<Expression> candidates;

    public static Expression Translate(Expression expression, HashSet<Expression> candidates)
    {
      var pat = new ParameterAccessTranslator(candidates);
      return pat.Visit(expression);
    }

    protected override Expression Visit(Expression exp)
    {
      if (exp == null)
        return null;
      if (candidates.Contains(exp) && MemberAccessChecker.ContainsMemberAccess(exp))
        return ExtractParameter(exp);
      return base.Visit(exp);
    }

    private Expression ExtractParameter(Expression expression)
    {
      Type type = expression.Type;
      if (type.IsValueType)
        expression = Expression.Convert(expression, typeof(object));
      Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(expression);
      return new ParameterAccessExpression(type, lambda);
    }

    protected override Expression VisitUnknown(Expression expression)
    {
      return expression;
    }


    // Constructor
    
    private ParameterAccessTranslator(HashSet<Expression> candidates)
    {
      this.candidates = candidates;
    }
  }
}