// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.27

using System.Linq;
using System.Linq.Expressions;
using Xtensive.Storage.Rse.Compilation.Expressions.Visitors;

namespace Xtensive.Storage.Rse.Compilation.Linq
{
  public class RseQueryTranslator : ExpressionVisitor
  {
    private RecordSet result;

    public static RecordSet Translate(Expression expression)
    {
      var translator = new RseQueryTranslator();
      translator.Visit(expression);
      return translator.result;
    }


    protected override Expression VisitConstant(ConstantExpression c)
    {
      if (c.Value == null)
        return c;
      var rootPoint = c.Value as IQueryable;
      if (rootPoint != null) {
        
      }
      return base.VisitConstant(c);
    }


    private RseQueryTranslator()
    {
    }
  }
}