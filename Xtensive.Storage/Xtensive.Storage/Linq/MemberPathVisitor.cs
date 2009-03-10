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
  internal abstract class MemberPathVisitor : QueryableVisitor
  {
    private readonly DomainModel model;

    protected override Expression VisitMemberAccess(MemberExpression ma)
    {
      var memberPath = MemberPath.Parse(ma, model);
      if (memberPath.IsValid)
        return VisitMemberPath(memberPath, ma);
      return base.VisitMemberAccess(ma);
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      var memberPath = MemberPath.Parse(mc, model);
      if (memberPath.IsValid)
        return VisitMemberPath(memberPath, mc);
      return base.VisitMethodCall(mc);
    }

    protected abstract Expression VisitMemberPath(MemberPath path, Expression e);


    // Constructors

    protected MemberPathVisitor(DomainModel model)
    {
      this.model = model;
    }
  }
}