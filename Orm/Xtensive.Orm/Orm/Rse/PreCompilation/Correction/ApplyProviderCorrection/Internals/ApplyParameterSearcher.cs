// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.15

using System;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Rse.Helpers;

using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Rse.PreCompilation.Correction.ApplyProviderCorrection
{
  internal sealed class ApplyParameterSearcher : ExpressionVisitor
  {
    private ApplyParameter result;

    public ApplyParameter Find(LambdaExpression predicate)
    {
      ArgumentValidator.EnsureArgumentNotNull(predicate, "predicate");
      result = null;
      Visit(predicate);
      return result;
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.AsTupleAccess() != null) {
        var applyParameter = mc.GetApplyParameter();
        if (result == null)
          result = applyParameter;
        else if (applyParameter != null && result != applyParameter)
          throw new InvalidOperationException(Strings.ExPredicateContainsAccessesToDifferentApplyParameters);
        return mc;
      }
      return base.VisitMethodCall(mc);
    }

    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }
  }
}