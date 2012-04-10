// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.06.02

using System;
using System.Linq.Expressions;

using Xtensive.Linq;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Helpers;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Linq.Expressions.Visitors
{
  /// <summary>
  /// Apply parameter access visitor. 
  /// This type is used internally by DataObjects.Net.
  /// </summary>
  public class ApplyParameterAccessVisitor : ExpressionVisitor
  {
    private readonly ApplyParameter applyParameter;
    private readonly Func<MethodCallExpression, int, Expression> processor;

    /// <summary>
    /// Visits the expression.
    /// </summary>
    /// <param name="e">The expression.</param>
    /// <returns></returns>
    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }

    /// <summary>
    /// Visits the <see cref="MethodCallExpression"/>.
    /// </summary>
    /// <param name="mc">MethodCallExpression</param>
    /// <returns></returns>
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


    /// <summary>
    /// Visits the specified expression.
    /// </summary>
    /// <param name="e">The expression.</param>
    /// <returns></returns>
    public new Expression Visit(Expression e)
    {
      return base.Visit(e);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public ApplyParameterAccessVisitor(ApplyParameter applyParameter, Func<MethodCallExpression,int,Expression> processor )
    {
      this.processor = processor;
      this.applyParameter = applyParameter;
    }
  }
}