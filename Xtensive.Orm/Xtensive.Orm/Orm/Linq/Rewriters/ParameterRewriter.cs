// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.07

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Linq;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Linq.Rewriters
{
  internal class ParameterRewriter: ExpressionVisitor
  {
    private readonly Dictionary<ParameterExpression, Expression> parameterReplacements = new Dictionary<ParameterExpression, Expression>();

    public static Expression Rewrite(Expression expression, params Pair<ParameterExpression, Expression>[] parameterReplacements)
    {
      return new ParameterRewriter(parameterReplacements).Visit(expression);
    }

    public static Expression Rewrite(Expression expression, ParameterExpression parameter, Expression parameterReplacement)
    {
      return new ParameterRewriter(parameter, parameterReplacement).Visit(expression);
    }

    protected override Expression VisitParameter(ParameterExpression p)
    {
      Expression replacement;

      if (parameterReplacements.TryGetValue(p, out replacement))
        return replacement;

      return base.VisitParameter(p);
    }

    private ParameterRewriter(params Pair<ParameterExpression, Expression>[] parameterReplacements)
    {
      foreach (Pair<ParameterExpression, Expression> parameterReplacement in parameterReplacements) {
        this.parameterReplacements.Add(parameterReplacement.First, parameterReplacement.Second);
      }
    }

    private ParameterRewriter(ParameterExpression parameter, Expression parameterReplacement)
    {
      parameterReplacements.Add(parameter, parameterReplacement);
    }
  }
}