// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.06

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Linq
{
  internal sealed class ExpressionHashCodeCalculator : ExpressionVisitor<int>
  {
    private const int NullHashCode = 0x7abf3456;
    private readonly ParameterExpressionRegistry parameters = new ParameterExpressionRegistry();

    public int CalculateHashCode(Expression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      try {
        return Visit(expression);
      }
      finally {
        parameters.Reset();
      }
    }

    #region ExpressionVisitor<int> implementation

    protected override int Visit(Expression e)
    {
      if (e==null)
        return NullHashCode;
      var hash = (uint) (base.Visit(e) ^ (int) e.NodeType ^ e.Type.GetHashCode());
      // transform bytes 0123 -> 1302
      hash = (hash & 0xFF00) >> 8 | (hash & 0xFF000000) >> 16 | (hash & 0xFF) << 16 | (hash & 0xFF0000) << 8;
      return (int) hash;
    }

    protected override int VisitUnary(UnaryExpression u)
    {
      return Visit(u.Operand) ^ (u.Method == null ? 0 : u.Method.GetHashCode());
    }

    protected override int VisitBinary(BinaryExpression b)
    {
      return Visit(b.Left) ^ Visit(b.Right) ^ (b.Method == null ? 0 : b.Method.GetHashCode());
    }

    protected override int VisitTypeIs(TypeBinaryExpression tb)
    {
      return Visit(tb.Expression) ^ tb.TypeOperand.GetHashCode();
    }

    protected override int VisitConstant(ConstantExpression c)
    {
      return c.Value != null ? c.Value.GetHashCode() : NullHashCode;
    }

    protected override int VisitConditional(ConditionalExpression c)
    {
      return Visit(c.Test) ^ Visit(c.IfTrue) ^ Visit(c.IfFalse);
    }

    protected override int VisitParameter(ParameterExpression p)
    {
      return parameters.GetIndex(p);
    }

    protected override int VisitMemberAccess(MemberExpression m)
    {
      return Visit(m.Expression) ^ m.Member.GetHashCode();
    }

    protected override int VisitMethodCall(MethodCallExpression mc)
    {
      return Visit(mc.Object) ^ mc.Method.GetHashCode() ^ HashExpressionSequence(mc.Arguments);
    }

    protected override int VisitLambda(LambdaExpression l)
    {
      parameters.AddRange(l.Parameters);
      return HashExpressionSequence(l.Parameters.Cast<Expression>()) ^ Visit(l.Body);
    }

    protected override int VisitNew(NewExpression n)
    {
      int result = 0;
      result ^= n.Constructor!=null ? n.Constructor.GetHashCode() : NullHashCode;
      result ^= n.Members != null ? n.Members.CalculateHashCode() : NullHashCode;
      result ^= HashExpressionSequence(n.Arguments);
      return result;
    }

    protected override int VisitMemberInit(MemberInitExpression mi)
    {
      var result = Visit(mi.NewExpression);
      foreach (var b in mi.Bindings)
        result ^= b.BindingType.GetHashCode() ^ b.Member.GetHashCode();
      return result;
    }

    protected override int VisitListInit(ListInitExpression li)
    {
      var result = VisitNew(li.NewExpression);
      foreach (var e in li.Initializers)
        result ^= e.AddMethod.GetHashCode() ^ HashExpressionSequence(e.Arguments);
      return result;
    }

    protected override int VisitNewArray(NewArrayExpression na)
    {
      return HashExpressionSequence(na.Expressions);
    }

    protected override int VisitInvocation(InvocationExpression i)
    {
      return HashExpressionSequence(i.Arguments) ^ Visit(i.Expression);
    }

    protected override int VisitUnknown(Expression e)
    {
      return e.GetHashCode();
    }

    #endregion

    #region Private / internal methods

    private int HashExpressionSequence(IEnumerable<Expression> expressions)
    {
      int result = 0;
      foreach (var e in expressions)
        result ^= Visit(e);
      return result;
    }

    #endregion
  }
}