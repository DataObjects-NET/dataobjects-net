// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.18

using System.Linq.Expressions;

namespace Xtensive.Storage.Linq.Expressions.Visitors.Internals
{
  internal sealed class MemberAccessChecker : ExpressionVisitor
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

    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }

    private MemberAccessChecker()
    {}
  }
}