// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.11

using System;
using System.Linq.Expressions;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Rse.Compilation.Expressions.Visitors
{
  public sealed class ParameterAccessTranslator : ExpressionVisitor
  {
    public Expression Translate (Expression expression)
    {
      return Visit(expression);
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      Expression e = m;
      Type type = m.Type;
//      if (type.IsValueType)
//        e = Expression.Convert(e, typeof(object));
      Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(e);
      return new ParameterAccessExpression(type, lambda);
    }
  }
}