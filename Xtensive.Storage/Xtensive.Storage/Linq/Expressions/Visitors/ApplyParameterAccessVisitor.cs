// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2009.06.02

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core.Linq;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Helpers;

namespace Xtensive.Storage.Linq.Expressions.Visitors
{
  [Serializable]
  internal class ApplyParameterAccessVisitor : ExpressionVisitor
  {
    private readonly ApplyParameter applyParameter;
    private readonly Func<MethodCallExpression, int, Expression> processor;

    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      var tupleAccess = mc.AsTupleAccess();
      if (tupleAccess==null)
        return base.VisitMethodCall(mc);
      var memberExpression = tupleAccess.Object as MemberExpression;
      if (memberExpression!=null
        && memberExpression.Member==WellKnownMembers.ApplyParameterValue
          && memberExpression.Expression.NodeType==ExpressionType.Constant
            && ((ConstantExpression) memberExpression.Expression).Value==applyParameter) {
        var index = (int) ((ConstantExpression) tupleAccess.Arguments[0]).Value;
        return processor.Invoke(mc, index);
      }
      return base.VisitMethodCall(mc);
    }

    public ApplyParameterAccessVisitor(ApplyParameter applyParameter, Func<MethodCallExpression,int,Expression> processor )
    {
      this.processor = processor;
      this.applyParameter = applyParameter;
    }

    public new Expression Visit(Expression e)
    {
      return base.Visit(e);
    }
  }
}