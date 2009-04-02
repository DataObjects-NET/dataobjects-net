// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.02

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core.Linq;
using System.Linq;

namespace Xtensive.Storage.Linq.Rewriters
{
  [Serializable]
  internal class ConstantParameterRewriter : ExpressionVisitor
  {
    private readonly ParameterExpression[] parameters;

    public static Expression Rewrite(Expression e, params ParameterExpression[] constantParameters)
    {
      return new ConstantParameterRewriter(constantParameters).Visit(e);
    }

    protected override Expression VisitParameter(ParameterExpression p)
    {
      if (parameters.Contains(p))
        return p;

      return base.VisitParameter(p);
    }

    private ConstantParameterRewriter(params ParameterExpression[] parameters)
    {
      this.parameters = parameters;
    }
  }
}