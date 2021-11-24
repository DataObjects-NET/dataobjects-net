// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.04.02

using System;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Orm.Internals;
using Xtensive.Reflection;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Linq.Rewriters
{
  internal sealed class EntitySetAccessRewriter : ExpressionVisitor
  {
    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (!IsEntitySet(mc.Object))
        return base.VisitMethodCall(mc);

      var method = mc.Method;
      if (method.Name=="Contains" && mc.Object!=null) {
        var elementType = GetEntitySetElementType(mc.Object.Type);
        var actualMethod = WellKnownMembers.Queryable.Contains.CachedMakeGenericMethod(elementType);
        return Expression.Call(actualMethod, Visit(mc.Object), Visit(mc.Arguments[0]));
      }

      throw NotSupported(method);
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      if (!IsEntitySet(m.Expression))
        return base.VisitMemberAccess(m);

      var member = m.Member;
      if (member.Name=="Count") {
        var elementType = GetEntitySetElementType(m.Expression.Type);
        var actualMethod = WellKnownMembers.Queryable.LongCount.CachedMakeGenericMethod(elementType);
        return Expression.Call(actualMethod, Visit(m.Expression));
      }

      throw NotSupported(member);
    }

    private static Type GetEntitySetElementType(Type entitySetType)
    {
      return entitySetType.GetGenericType(WellKnownOrmTypes.EntitySetOfT).GetGenericArguments()[0];
    }

    private static NotSupportedException NotSupported(MemberInfo member)
    {
      return new NotSupportedException(string.Format(Strings.ExMemberXIsNotSupported, member.GetFullName(true)));
    }

    private static bool IsEntitySet(Expression expression)
    {
      return expression!=null && expression.Type.IsOfGenericType(WellKnownOrmTypes.EntitySetOfT);
    }

    public static Expression Rewrite(Expression e)
    {
      return new EntitySetAccessRewriter().Visit(e);
    }

    // Constructors

    private EntitySetAccessRewriter()
    {
    }
  }
}