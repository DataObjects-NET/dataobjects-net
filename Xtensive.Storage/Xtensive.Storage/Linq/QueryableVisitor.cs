// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.25

using System;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core.Linq;

namespace Xtensive.Storage.Linq
{
  [Serializable]
  internal abstract class QueryableVisitor : ExpressionVisitor
  {
    protected override Expression VisitUnknown(Expression e)
    {
      var nodeType = (ExtendedExpressionType)e.NodeType;
      if (nodeType == ExtendedExpressionType.QueryableMethod)
        return VisitQueryableMethod((QueryableMethodCall)e);
      return base.VisitUnknown(e);
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.Method.DeclaringType==typeof (Queryable) || mc.Method.DeclaringType==typeof (Enumerable)) {
        var kind = (QueryableMethodKind)Enum.Parse(typeof(QueryableMethodKind), mc.Method.Name);
        return Visit(new QueryableMethodCall(kind, mc));
      }
      return base.VisitMethodCall(mc);
    }

    protected abstract Expression VisitQueryableMethod(QueryableMethodCall qmc);
  }
}