// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.05

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Xtensive.Storage.Rse.Compilation.Expressions.Visitors
{
  /// <summary>
  /// Abstract Expression visitor class.
  /// </summary>
  /// <typeparam name="TResult">The type of the result.</typeparam>
  public abstract class Visitor<TResult>
    where TResult : class
  {
    protected virtual TResult Visit(Expression exp)
    {
      if (exp==null)
        return null;

      switch (exp.NodeType) {
      case ExpressionType.Negate:
      case ExpressionType.NegateChecked:
      case ExpressionType.Not:
      case ExpressionType.Convert:
      case ExpressionType.ConvertChecked:
      case ExpressionType.ArrayLength:
      case ExpressionType.Quote:
      case ExpressionType.TypeAs:
        return VisitUnary((UnaryExpression) exp);
      case ExpressionType.Add:
      case ExpressionType.AddChecked:
      case ExpressionType.Subtract:
      case ExpressionType.SubtractChecked:
      case ExpressionType.Multiply:
      case ExpressionType.MultiplyChecked:
      case ExpressionType.Divide:
      case ExpressionType.Modulo:
      case ExpressionType.And:
      case ExpressionType.AndAlso:
      case ExpressionType.Or:
      case ExpressionType.OrElse:
      case ExpressionType.LessThan:
      case ExpressionType.LessThanOrEqual:
      case ExpressionType.GreaterThan:
      case ExpressionType.GreaterThanOrEqual:
      case ExpressionType.Equal:
      case ExpressionType.NotEqual:
      case ExpressionType.Coalesce:
      case ExpressionType.ArrayIndex:
      case ExpressionType.RightShift:
      case ExpressionType.LeftShift:
      case ExpressionType.ExclusiveOr:
        return VisitBinary((BinaryExpression) exp);
      case ExpressionType.TypeIs:
        return VisitTypeIs((TypeBinaryExpression) exp);
      case ExpressionType.Conditional:
        return VisitConditional((ConditionalExpression) exp);
      case ExpressionType.Constant:
        return VisitConstant((ConstantExpression) exp);
      case ExpressionType.Parameter:
        return VisitParameter((ParameterExpression) exp);
      case ExpressionType.MemberAccess:
        return VisitMemberAccess((MemberExpression) exp);
      case ExpressionType.Call:
        return VisitMethodCall((MethodCallExpression) exp);
      case ExpressionType.Lambda:
        return VisitLambda((LambdaExpression) exp);
      case ExpressionType.New:
        return VisitNew((NewExpression) exp);
      case ExpressionType.NewArrayInit:
      case ExpressionType.NewArrayBounds:
        return VisitNewArray((NewArrayExpression) exp);
      case ExpressionType.Invoke:
        return VisitInvocation((InvocationExpression) exp);
      case ExpressionType.MemberInit:
        return VisitMemberInit((MemberInitExpression) exp);
      case ExpressionType.ListInit:
        return VisitListInit((ListInitExpression) exp);
      default:
        throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
      }
    }

    protected virtual ReadOnlyCollection<TResult> VisitExpressionList(ReadOnlyCollection<Expression> original)
    {
      var results = new List<TResult>();
      for (int i = 0, n = original.Count; i < n; i++) {
        TResult p = Visit(original[i]);
        results.Add(p);
      }
      return results.AsReadOnly();
    }


    protected abstract TResult VisitUnary(UnaryExpression u);
    protected abstract TResult VisitBinary(BinaryExpression b);
    protected abstract TResult VisitTypeIs(TypeBinaryExpression b);
    protected abstract TResult VisitConstant(ConstantExpression c);
    protected abstract TResult VisitConditional(ConditionalExpression c);
    protected abstract TResult VisitParameter(ParameterExpression p);
    protected abstract TResult VisitMemberAccess(MemberExpression m);
    protected abstract TResult VisitMethodCall(MethodCallExpression m);
    protected abstract TResult VisitLambda(LambdaExpression lambda);
    protected abstract TResult VisitNew(NewExpression nex);
    protected abstract TResult VisitMemberInit(MemberInitExpression init);
    protected abstract TResult VisitListInit(ListInitExpression init);
    protected abstract TResult VisitNewArray(NewArrayExpression na);
    protected abstract TResult VisitInvocation(InvocationExpression iv);
  }
}