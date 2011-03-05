// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.21

using System;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Linq;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Internals
{
  internal sealed class DelegatingExpressionVisitor : ExpressionVisitor
  {
    private Action<Expression> action;
    private bool keyIsFound;

    public void Visit(Expression expression, Action<Expression> action)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentNotNull(action, "action");
      this.action = action;
      Visit(expression);
    }

    protected override Expression VisitBinary(BinaryExpression exp)
    {
      action.Invoke(exp);
      return base.VisitBinary(exp);
    }

    protected override Expression VisitConditional(ConditionalExpression exp)
    {
      action.Invoke(exp);
      return base.VisitConditional(exp);
    }

    protected override Expression VisitConstant(ConstantExpression exp)
    {
      action.Invoke(exp);
      return base.VisitConstant(exp);
    }

    protected override Expression VisitInvocation(InvocationExpression exp)
    {
      action.Invoke(exp);
      return base.VisitInvocation(exp);
    }

    protected override Expression VisitLambda(LambdaExpression exp)
    {
      action.Invoke(exp);
      return base.VisitLambda(exp);
    }

    protected override Expression VisitMemberAccess(MemberExpression exp)
    {
      action.Invoke(exp);
      return base.VisitMemberAccess(exp);
    }

    protected override Expression VisitMemberInit(MemberInitExpression exp)
    {
      action.Invoke(exp);
      return base.VisitMemberInit(exp);
    }

    protected override Expression VisitMethodCall(MethodCallExpression exp)
    {
      action.Invoke(exp);
      return base.VisitMethodCall(exp);
    }

    protected override Expression VisitListInit(ListInitExpression exp)
    {
      action.Invoke(exp);
      return base.VisitListInit(exp);
    }

    protected override Expression VisitNew(NewExpression exp)
    {
      action.Invoke(exp);
      return base.VisitNew(exp);
    }

    protected override Expression VisitNewArray(NewArrayExpression exp)
    {
      action.Invoke(exp);
      return base.VisitNewArray(exp);
    }

    protected override Expression VisitParameter(ParameterExpression exp)
    {
      action.Invoke(exp);
      return base.VisitParameter(exp);
    }

    protected override Expression VisitTypeIs(TypeBinaryExpression exp)
    {
      action.Invoke(exp);
      return base.VisitTypeIs(exp);
    }

    protected override Expression VisitUnary(UnaryExpression exp)
    {
      action.Invoke(exp);
      return base.VisitUnary(exp);
    }
  }
}