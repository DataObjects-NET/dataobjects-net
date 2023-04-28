// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.02.18

using System;
using System.Linq.Expressions;
using System.Reflection;
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
      var mcMethod = mc.Method; 
      if (mcMethod.Name != Xtensive.Reflection.WellKnown.Object.Equals)
        return base.VisitMethodCall(mc);

      var declaringType = mcMethod.DeclaringType;

      if (mcMethod.IsStatic) {
        var parameterTypes = mcMethod.GetParameterTypes();
        var mcArguments = mc.Arguments; 
        if (mcArguments.Count == 2 
          && declaringType == parameterTypes[0] 
          && declaringType == parameterTypes[1])
          return Expression.Equal(mcArguments[0], mcArguments[1]);
        return base.VisitMethodCall(mc);
      }

      if (mcMethod.GetInterfaceMember() is MemberInfo interfaceMember) {
        return interfaceMember.ReflectedType.IsGenericType(typeof(IEquatable<>))
          ? Expression.Equal(mc.Object, mc.Arguments[0])
          : base.VisitMethodCall(mc);
      }

      if (declaringType == WellKnownTypes.Object) {
        var mcArguments = mc.Arguments;
        if (mcArguments.Count == 1) {
          return Expression.Equal(mc.Object, mcArguments[0]);
        }
      }

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