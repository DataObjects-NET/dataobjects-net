// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.07.03

using System.Linq;
using System.Linq.Expressions;
using Xtensive.Linq;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Linq.Rewriters
{
  internal class SelectManySelectorRewriter : ExpressionVisitor
  {
    private readonly ParameterExpression sourceParameter;
    private readonly ParameterExpression targetParameter;
    private bool processingFailed;

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      if (m.Expression==sourceParameter && m.Type==targetParameter.Type)
        return targetParameter;
      return base.VisitMemberAccess(m);
    }

    protected override Expression VisitParameter(ParameterExpression p)
    {
      if (p==sourceParameter)
        processingFailed = true;
      return p;
    }

    public static bool TryRewrite(LambdaExpression resultSelector,
      ParameterExpression sourceParameter, ParameterExpression targetParameter, out LambdaExpression result)
    {
      var parameters = resultSelector.Parameters
        .Select(p => p==sourceParameter ? targetParameter : p)
        .ToArray();
      var rewriter = new SelectManySelectorRewriter(sourceParameter, targetParameter);
      var body = rewriter.Visit(resultSelector.Body);
      if (rewriter.processingFailed) {
        result = null;
        return false;
      }
      result = FastExpression.Lambda(body, parameters);
      return true;
    }

    // Constructors

    private SelectManySelectorRewriter(ParameterExpression sourceParameter, ParameterExpression targetParameter)
    {
      this.sourceParameter = sourceParameter;
      this.targetParameter = targetParameter;
    }
  }
}