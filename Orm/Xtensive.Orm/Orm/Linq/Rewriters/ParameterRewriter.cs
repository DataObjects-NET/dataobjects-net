// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.07

using System.Collections.Generic;
using System.Linq.Expressions;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Linq.Rewriters
{
  internal class ParameterRewriter: ExpressionVisitor
  {
    private readonly Dictionary<ParameterExpression, Expression> parameterReplacements = new Dictionary<ParameterExpression, Expression>();

    protected override Expression VisitParameter(ParameterExpression p)
    {
      Expression replacement;

      if (parameterReplacements.TryGetValue(p, out replacement))
        return replacement;

      return base.VisitParameter(p);
    }

    public static Expression Rewrite(Expression expression, ParameterExpression parameter, Expression parameterReplacement)
    {
      return new ParameterRewriter(parameter, parameterReplacement).Visit(expression);
    }

    // Constructors

    private ParameterRewriter(ParameterExpression parameter, Expression parameterReplacement)
    {
      parameterReplacements.Add(parameter, parameterReplacement);
    }
  }
}