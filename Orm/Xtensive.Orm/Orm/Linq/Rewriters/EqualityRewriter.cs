// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.02.18

using System;
using System.Linq.Expressions;
using Xtensive.Reflection;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Linq.Rewriters
{
  internal sealed class EqualityRewriter : ExpressionVisitor
  {
    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.Method.Name != Xtensive.Reflection.WellKnown.Object.Equals)
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
        if (interfaceMember.ReflectedType.IsGenericType && interfaceMember.ReflectedType.CachedGetGenericTypeDefinition() == typeof(IEquatable<>))
          return Expression.Equal(mc.Object, mc.Arguments[0]);
        return base.VisitMethodCall(mc);
      }

      if (declaringType == WellKnownTypes.Object && mc.Arguments.Count == 1)
        return Expression.Equal(mc.Object, mc.Arguments[0]);

      return base.VisitMethodCall(mc);
    }

    public static Expression Rewrite(Expression e)
    {
      return new EqualityRewriter().Visit(e);
    }

    // Constructors

    private EqualityRewriter()
    {
    }
  }
}