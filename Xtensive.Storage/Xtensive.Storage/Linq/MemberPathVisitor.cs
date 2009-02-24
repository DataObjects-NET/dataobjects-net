// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.05

using System;
using System.Linq.Expressions;
using Xtensive.Core.Linq;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Linq
{
  internal abstract class MemberPathVisitor : ExpressionVisitor
  {
    private readonly DomainModel model;

    protected override Expression VisitUnknown(Expression e)
    {
      var nodeType = (ExtendedExpressionType)e.NodeType;
      if (nodeType == ExtendedExpressionType.MemberPath)
        return VisitMemberPath((MemberPathExpression)e);
      return base.VisitUnknown(e);
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      var memberPath = MemberPath.Parse(m, model);
      if (memberPath.IsValid)
        return Visit(new MemberPathExpression(memberPath, m));
      return base.VisitMemberAccess(m);
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      var memberPath = MemberPath.Parse(mc, model);
      if (memberPath.IsValid)
        return Visit(new MemberPathExpression(memberPath, mc));
      return base.VisitMethodCall(mc);
    }

    protected abstract Expression VisitMemberPath(MemberPathExpression mpe);


    // Constructors

    protected MemberPathVisitor(DomainModel model)
    {
      this.model = model;
    }
  }
}