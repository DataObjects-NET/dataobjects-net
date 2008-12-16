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
using Xtensive.Storage.Rse.Compilation.Expressions;

namespace Xtensive.Storage.Linq.Linq2Rse
{
  public sealed class RseQueryRewriter : ExpressionVisitor
  {
    private readonly QueryProvider provider;

    public Expression Rewrite(Expression expression)
    {
      return Visit(expression);
    }

    protected override Expression VisitMethodCall(MethodCallExpression m)
    {
      if (m.Method.DeclaringType == typeof(Queryable) || m.Method.DeclaringType == typeof(Enumerable)) {
//        switch (m.Method.Name) {
//          case "Where":
//            return RewriteWhere(m.Type, m.Arguments[0], (LambdaExpression)m.Arguments[1].StripQuotes());
//        }
      }
      return base.VisitMethodCall(m);
    }

    private Expression RewriteWhere(Type type, Expression expression, LambdaExpression lambdaExpression)
    {
      throw new NotImplementedException();
    }

    protected override Expression VisitUnknown(Expression expression)
    {
      return expression;
    }


    // Constructor

    public RseQueryRewriter(QueryProvider provider)
    {
      this.provider = provider;
    }
  }
}