// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.16

using System;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Orm.Linq.Expressions.Visitors
{
  internal sealed class IncludeFilterMappingGatherer : ExtendedExpressionVisitor
  {
    private readonly int[] mapping;
    private readonly Expression tupleExpression;

    private int tupleIndex = -1;
    private int providerIndex = -1;

    public static int[] Visit(Expression tupleExpression, int length, Expression expression)
    {
      var mapping = Enumerable.Repeat(-1, length).ToArray();
      new IncludeFilterMappingGatherer(tupleExpression, mapping).Visit(expression);
      if (mapping.Contains(-1))
        throw new InvalidOperationException();
      return mapping;
    }

    protected override Expression VisitBinary(BinaryExpression b)
    {
      if (tupleIndex!=-1 || providerIndex!=-1)
        throw new InvalidOperationException();
      var result = base.VisitBinary(b);
      if (tupleIndex * providerIndex < 0)
        throw new InvalidOperationException();
      if (tupleIndex >= 0)
        mapping[tupleIndex] = providerIndex;
      tupleIndex = -1;
      providerIndex = -1;
      return result;
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      var isTupleAccess = mc.Method.IsGenericMethod && mc.Method.GetGenericMethodDefinition()==WellKnownMembers.Tuple.GenericAccessor;
      if (isTupleAccess && mc.Arguments[0].StripCasts().NodeType==ExpressionType.Constant) {
        if (mc.Object==tupleExpression)
          tupleIndex = (int) ((ConstantExpression) mc.Arguments[0].StripCasts()).Value;
        else
          providerIndex = (int) ((ConstantExpression) mc.Arguments[0].StripCasts()).Value;
      }
      return base.VisitMethodCall(mc);
    }

    private IncludeFilterMappingGatherer(Expression tupleExpression, int[] mapping)
    {
      this.tupleExpression = tupleExpression;
      this.mapping = mapping;
    }
  }
}