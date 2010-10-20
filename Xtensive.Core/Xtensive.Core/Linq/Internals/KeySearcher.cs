// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.02

using System;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Linq
{
  internal class KeySearcher : ExpressionVisitor
  {
    private Func<Expression, bool> selector;
    private bool keyIsFound;

    public bool ContainsKey(Expression value, Func<Expression, bool> keySelector)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      ArgumentValidator.EnsureArgumentNotNull(keySelector, "keySelector");
      selector = keySelector;
      keyIsFound = false;
      Visit(value);
      return keyIsFound;
    }

    private bool FindKey(Expression exp)
    {
      if(!keyIsFound)
        keyIsFound = selector(exp);
      return keyIsFound;
    }

    protected override Expression VisitBinary(BinaryExpression exp)
    {
      if (FindKey(exp))
        return exp;
      return base.VisitBinary(exp);
    }

    protected override Expression VisitConditional(ConditionalExpression exp)
    {
      if (FindKey(exp))
        return exp;
      return base.VisitConditional(exp);
    }

    protected override Expression VisitConstant(ConstantExpression exp)
    {
      if (FindKey(exp))
        return exp;
      return base.VisitConstant(exp);
    }

    protected override Expression VisitInvocation(InvocationExpression exp)
    {
      if (FindKey(exp))
        return exp;
      return base.VisitInvocation(exp);
    }

    protected override Expression VisitLambda(LambdaExpression exp)
    {
      if (FindKey(exp))
        return exp;
      return base.VisitLambda(exp);
    }

    protected override Expression VisitMemberAccess(MemberExpression exp)
    {
      if (FindKey(exp))
        return exp;
      return base.VisitMemberAccess(exp);
    }

    protected override Expression VisitMemberInit(MemberInitExpression exp)
    {
      if (FindKey(exp))
        return exp;
      return base.VisitMemberInit(exp);
    }

    protected override Expression VisitMethodCall(MethodCallExpression exp)
    {
      if (FindKey(exp))
        return exp;
      return base.VisitMethodCall(exp);
    }

    protected override Expression VisitListInit(ListInitExpression exp)
    {
      if (FindKey(exp))
        return exp;
      return base.VisitListInit(exp);
    }

    protected override Expression VisitNew(NewExpression exp)
    {
      if (FindKey(exp))
        return exp;
      return base.VisitNew(exp);
    }

    protected override Expression VisitNewArray(NewArrayExpression exp)
    {
      if (FindKey(exp))
        return exp;
      return base.VisitNewArray(exp);
    }

    protected override Expression VisitParameter(ParameterExpression exp)
    {
      if (FindKey(exp))
        return exp;
      return base.VisitParameter(exp);
    }

    protected override Expression VisitTypeIs(TypeBinaryExpression exp)
    {
      if (FindKey(exp))
        return exp;
      return base.VisitTypeIs(exp);
    }

    protected override Expression VisitUnary(UnaryExpression exp)
    {
      if (FindKey(exp))
        return exp;
      return base.VisitUnary(exp);
    }
  }
}