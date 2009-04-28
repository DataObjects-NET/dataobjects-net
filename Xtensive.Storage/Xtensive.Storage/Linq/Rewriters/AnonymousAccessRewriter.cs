// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.28

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Linq;

namespace Xtensive.Storage.Linq.Rewriters
{
  [Serializable]
  internal class AnonymousAccessRewriter : ExpressionVisitor
  {
    public static Expression Rewrite(Expression e)
    {
      return new AnonymousAccessRewriter().Visit(e);
    }

    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      if (m.Expression.NodeType==ExpressionType.New) {
        var newExpression = (NewExpression) m.Expression;
        var methodName = ((PropertyInfo) m.Member).GetGetMethod().Name;
        var method = newExpression.Members.First(member => member.Name==methodName);
        var argument = newExpression.Arguments[newExpression.Members.IndexOf(method)];
        return Visit(argument);
      }
      return base.VisitMemberAccess(m);
    }
  }
}