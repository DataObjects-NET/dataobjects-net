// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.18

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core.Linq;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Linq
{
  [Serializable]
  internal sealed class ExpressionPreprocessor : ExpressionVisitor
  {
    public static Expression Preprocess(Expression e)
    {
      var ep = new ExpressionPreprocessor();
      return ep.Visit(e);
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.Method.DeclaringType == typeof(object) && mc.Method.Name == WellKnown.Object.Equals) {
        if (mc.Object == null && mc.Method.IsStatic)
          return Expression.Equal(mc.Arguments[0], mc.Arguments[1]);
        return Expression.Equal(mc.Object, mc.Arguments[0]);
      }
      if (mc.Method.DeclaringType == typeof (Structure) && mc.Method.Name == WellKnown.Object.Equals)
        return Expression.Equal(mc.Object, mc.Arguments[0]);
      return base.VisitMethodCall(mc);
    }
  }
}