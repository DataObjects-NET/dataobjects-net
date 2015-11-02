// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.09

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Orm.Internals;
using Xtensive.Reflection;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Linq.Rewriters
{
  internal class ClosureAccessRewriter : ExpressionVisitor
  {
    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }

    protected override Expression VisitMemberAccess(MemberExpression memberExpression)
    {
      if (memberExpression.Type.IsOfGenericInterface(typeof (IQueryable<>))
        && memberExpression.Expression!=null
        && memberExpression.Expression.NodeType==ExpressionType.Constant
        && memberExpression.Member!=null
        && memberExpression.Member.ReflectedType.IsClosure()
        && memberExpression.Member.MemberType==MemberTypes.Field) {
        var fieldInfo = (FieldInfo) memberExpression.Member;
        if (!fieldInfo.FieldType.IsOfGenericType(typeof (EntitySet<>))) {
          if (QueryCachingScope.Current!=null)
            throw new InvalidOperationException(String.Format(Strings.ExUnableToUseIQueryableXInQueryExecuteStatement, fieldInfo.Name));
          var constantValue = ((ConstantExpression) memberExpression.Expression).Value;
          var queryable = (IQueryable) fieldInfo.GetValue(constantValue);
          if (queryable.Expression.Type.IsOfGenericInterface(typeof (IQueryable<>)))
            return Visit(queryable.Expression);
          return queryable.Expression;
        }
      }
      return base.VisitMemberAccess(memberExpression);
    }

    public static Expression Rewrite(Expression e)
    {
      return new ClosureAccessRewriter().Visit(e);
    }

    // Constructors

    private ClosureAccessRewriter()
    {
    }
  }
}