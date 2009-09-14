// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.18

using System;
using System.Linq.Expressions;
using Xtensive.Core.Linq;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Linq.Rewriters
{
  internal sealed class EqualityRewriter : ExpressionVisitor
  {
    public static Expression Rewrite(Expression e)
    {
      return new EqualityRewriter().Visit(e);
    }

    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.Method.Name != Core.Reflection.WellKnown.Object.Equals)
        return base.VisitMethodCall(mc);

      var declaringType = mc.Method.DeclaringType;

      if (mc.Method.IsStatic) {
        var parameterTypes = mc.Method.GetParameterTypes();
        if (mc.Arguments.Count == 2 
          && declaringType == parameterTypes[0] 
          && declaringType == parameterTypes[1])
          return Expression.Equal(mc.Arguments[0], mc.Arguments[1]);
        return base.VisitMethodCall(mc);
      }

      var interfaceMember = mc.Method.GetInterfaceMember();
      if (interfaceMember != null) {
        if (interfaceMember.ReflectedType.IsGenericType && interfaceMember.ReflectedType.GetGenericTypeDefinition() == typeof(IEquatable<>))
          return Expression.Equal(mc.Object, mc.Arguments[0]);
        return base.VisitMethodCall(mc);
      }

      if (declaringType == typeof(object) && mc.Arguments.Count == 1)
        return Expression.Equal(mc.Object, mc.Arguments[0]);

      return base.VisitMethodCall(mc);
    }
  }
}