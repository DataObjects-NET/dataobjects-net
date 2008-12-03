// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.03

using System;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Storage.Rse.Compilation.Expressions;
using Xtensive.Storage.Rse.Compilation.Expressions.Visitors;

namespace Xtensive.Storage.Rse.Compilation.Linq
{
  public sealed class RseQueryRewriter : ExpressionVisitor
  {
    private readonly QueryProvider provider;

    public Expression Rewrite(Expression expression)
    {
      return Visit(expression);
    }

    protected override Expression VisitConstant(ConstantExpression c)
    {
      if (c.Value == null)
        return c;
      var rootPoint = c.Value as IQueryable;
      if (rootPoint != null) {
        var type = provider.Model.Types[rootPoint.ElementType];
        var index = type.Indexes.PrimaryIndex;
        return new IndexAccessExpression(c.Type, index);
      }
      return base.VisitConstant(c);
    }

    protected override Expression VisitMethodCall(MethodCallExpression m)
    {
      if (m.Method.DeclaringType == typeof(Queryable) || m.Method.DeclaringType == typeof(Enumerable)) {
        switch (m.Method.Name) {
          case "Where":
            return RewriteWhere(m.Type, m.Arguments[0], (LambdaExpression)m.Arguments[1].StripQuotes());
        }
      }
      return base.VisitMethodCall(m);
    }

    private Expression RewriteWhere(Type type, Expression expression, LambdaExpression lambdaExpression)
    {
      throw new NotImplementedException();
    }


    // Constructor

    public RseQueryRewriter(QueryProvider provider)
    {
      this.provider = provider;
    }
  }
}