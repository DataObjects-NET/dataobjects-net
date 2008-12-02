// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.27

using System.Linq;
using System.Linq.Expressions;
using Xtensive.Storage.Rse.Compilation.Expressions.Visitors;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation.Linq
{
  public class RseQueryTranslator : ExpressionVisitor
  {
    private readonly QueryProvider provider;
    private RecordSet result;

    public static RecordSet Translate(Expression expression, QueryProvider provider)
    {
      var translator = new RseQueryTranslator(provider);
      translator.Visit(expression);
      return translator.result;
    }


    protected override Expression VisitConstant(ConstantExpression c)
    {
      if (c.Value == null)
        return c;
      var rootPoint = c.Value as IQueryable;
      if (rootPoint != null) {
        var type = provider.Model.Types[rootPoint.ElementType];
        var index = type.Indexes.PrimaryIndex;
        result = IndexProvider.Get(index).Result;
      }
      return base.VisitConstant(c);
    }

    
    
    private RseQueryTranslator(QueryProvider provider)
    {
      this.provider = provider;
    }
  }
}