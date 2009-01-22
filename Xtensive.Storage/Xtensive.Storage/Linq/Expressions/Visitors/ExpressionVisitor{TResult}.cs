// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.05

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Linq.Expressions.Visitors
{
  /// <summary>
  /// Abstract <see cref="Expression"/> visitor class.
  /// </summary>
  /// <typeparam name="TResult">Type of the visit result.</typeparam>
  public abstract class ExpressionVisitor<TResult>
    where TResult : class
  {
    private readonly Dictionary<Expression, TResult> cache = null;

    protected virtual TResult Visit(Expression e)
    {
      if (e==null)
        return null;

      TResult result;
      if (cache!=null) {
        if (cache.TryGetValue(e, out result))
          return result;
      }

      switch (e.NodeType) {
      case ExpressionType.Negate:
      case ExpressionType.NegateChecked:
      case ExpressionType.Not:
      case ExpressionType.Convert:
      case ExpressionType.ConvertChecked:
      case ExpressionType.ArrayLength:
      case ExpressionType.Quote:
      case ExpressionType.TypeAs:
        result = VisitUnary((UnaryExpression) e);
        break;
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
        result = VisitBinary((BinaryExpression) e);
        break;
      case ExpressionType.TypeIs:
        result = VisitTypeIs((TypeBinaryExpression) e);
        break;
      case ExpressionType.Conditional:
        result = VisitConditional((ConditionalExpression) e);
        break;
      case ExpressionType.Constant:
        result = VisitConstant((ConstantExpression) e);
        break;
      case ExpressionType.Parameter:
        result = VisitParameter((ParameterExpression) e);
        break;
      case ExpressionType.MemberAccess:
        result = VisitMemberAccess((MemberExpression) e);
        break;
      case ExpressionType.Call:
        result = VisitMethodCall((MethodCallExpression) e);
        break;
      case ExpressionType.Lambda:
        result = VisitLambda((LambdaExpression) e);
        break;
      case ExpressionType.New:
        result = VisitNew((NewExpression) e);
        break;
      case ExpressionType.NewArrayInit:
      case ExpressionType.NewArrayBounds:
        result = VisitNewArray((NewArrayExpression) e);
        break;
      case ExpressionType.Invoke:
        result = VisitInvocation((InvocationExpression) e);
        break;
      case ExpressionType.MemberInit:
        result = VisitMemberInit((MemberInitExpression) e);
        break;
      case ExpressionType.ListInit:
        result = VisitListInit((ListInitExpression) e);
        break;
      default:
        result = VisitUnknown(e);
        break;
      }

      if (cache!=null)
        cache[e] = result;
      return result;
    }

    protected virtual ReadOnlyCollection<TResult> VisitExpressionList(ReadOnlyCollection<Expression> expressions)
    {
      var results = new List<TResult>();
      for (int i = 0, n = expressions.Count; i < n; i++) {
        TResult p = Visit(expressions[i]);
        results.Add(p);
      }
      return results.AsReadOnly();
    }

    protected virtual TResult VisitUnknown(Expression e)
    {
      throw new NotSupportedException(string.Format(
        Strings.ExUnknownExpressionType, e.GetType().GetShortName(), e.NodeType));
    }

    protected abstract TResult VisitUnary(UnaryExpression u);
    protected abstract TResult VisitBinary(BinaryExpression b);
    protected abstract TResult VisitTypeIs(TypeBinaryExpression tb);
    protected abstract TResult VisitConstant(ConstantExpression c);
    protected abstract TResult VisitConditional(ConditionalExpression c);
    protected abstract TResult VisitParameter(ParameterExpression p);
    protected abstract TResult VisitMemberAccess(MemberExpression m);
    protected abstract TResult VisitMethodCall(MethodCallExpression mc);
    protected abstract TResult VisitLambda(LambdaExpression l);
    protected abstract TResult VisitNew(NewExpression n);
    protected abstract TResult VisitMemberInit(MemberInitExpression mi);
    protected abstract TResult VisitListInit(ListInitExpression li);
    protected abstract TResult VisitNewArray(NewArrayExpression na);
    protected abstract TResult VisitInvocation(InvocationExpression i);

    
    // Constructors

    protected ExpressionVisitor()
      : this(false)
    {
    }

    protected ExpressionVisitor(bool isCaching)
    {
      if (isCaching)
        cache = new Dictionary<Expression, TResult>();
    }
  }
}