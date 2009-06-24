// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.23

using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core.Linq;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Helpers;

namespace Xtensive.Storage.Providers.MsSql
{
  internal class ExpressionPreprocessor : ExpressionVisitor
  {
    private readonly Expression one = Expression.Constant(1);
    private readonly Expression zero = Expression.Constant(0);

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.AsTupleAccess() == null || !IsBooleanExpression(mc))
        return base.VisitMethodCall(mc);
      var callTarget = mc.Object;
      if (callTarget.NodeType != ExpressionType.Parameter && callTarget.Type != typeof(ApplyParameter))
        return base.VisitMethodCall(mc);
      mc = Expression.Call(
        Visit(callTarget),
        mc.Method.GetGenericMethodDefinition().MakeGenericMethod(typeof (int)),
        mc.Arguments.Select(arg => Visit(arg))
        );
      return Expression.NotEqual(mc, zero);
    }

    protected override Expression VisitBinary(BinaryExpression b)
    {
      if (!IsBooleanExpression(b.Left) || !IsBooleanExpression(b.Right))
        return base.VisitBinary(b);

      switch (b.NodeType) {
        case ExpressionType.Equal:
        case ExpressionType.NotEqual:
          var left = Expression.Condition(Visit(b.Left), one, zero);
          var right = Expression.Condition(Visit(b.Right), one, zero);
          return Expression.MakeBinary(b.NodeType, left, right);
        default:
          return base.VisitBinary(b);
      }
    }

    private static bool IsBooleanExpression(Expression e)
    {
      return e.Type==typeof (bool);
    }

    public static LambdaExpression Preprocess(LambdaExpression le)
    {
      var preprocessor = new ExpressionPreprocessor();
      return (LambdaExpression) preprocessor.Visit(le);
    }
  }
}
