// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.03

using System;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Linq.Expressions.Visitors;

namespace Xtensive.Storage.Linq.Linq2Rse
{
  public sealed class RseQueryRewriter : ExpressionVisitor
  {
    private readonly QueryProvider provider;

    public Expression Rewrite(Expression expression)
    {
      return Visit(expression);
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.Method.DeclaringType == typeof(Queryable) || mc.Method.DeclaringType == typeof(Enumerable)) {
//        switch (mc.Method.Name) {
//          case "Where":
//            return RewriteWhere(mc.Type, mc.Arguments[0], (LambdaExpression)mc.Arguments[1].StripQuotes());
//        }
      }
      return base.VisitMethodCall(mc);
    }

    private Expression RewriteWhere(Type type, Expression expression, LambdaExpression lambdaExpression)
    {
      throw new NotImplementedException();
    }

    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }


    // Constructor

    public RseQueryRewriter(QueryProvider provider)
    {
      this.provider = provider;
    }
  }
}