// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.15

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core.Linq;

namespace Xtensive.Storage.Linq.Rewriters
{
  [Serializable]
  internal class OfTypeRewriter : ExpressionVisitor
  {
    protected override System.Linq.Expressions.Expression VisitMethodCall(System.Linq.Expressions.MethodCallExpression mc)
    {
      if (mc.Method.DeclaringType == typeof(Queryable) && mc.Method.Name == "OfType") {
        var source = mc.Arguments[0];
        var sourceType = mc.Method.GetGenericArguments()[0];
        var targetType = mc.Arguments[0].Type.GetGenericArguments()[0];
        if (targetType == sourceType)
          return Visit(source);

        var parameter = Expression.Parameter(sourceType, "p");
        var isExpression = Expression.TypeIs(parameter, targetType);
        LambdaExpression le = Expression.Lambda(isExpression, parameter);
        var whereExpression = Expression.Call(WellKnownMembers.QueryableWhere.MakeGenericMethod(sourceType), source, le);
        return whereExpression;
      }
      return base.VisitMethodCall(mc);
    }

    

    public static Expression Rewrite(Expression expression)
    {
      return new OfTypeRewriter().Visit(expression);
    }
  }
}