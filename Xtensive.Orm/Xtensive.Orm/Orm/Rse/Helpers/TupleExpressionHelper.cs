// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.03.20

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Reflection;
using Xtensive.Core;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Rse.Resources;

namespace Xtensive.Orm.Rse.Helpers
{
  /// <summary>
  /// Various extension methods for manipulating expressions with <see cref="Tuple"/>.
  /// </summary>
  public static class TupleExpressionHelper
  {
    /// <summary>
    /// Checks if expression is access to tuple.
    /// </summary>
    /// <param name="expression">Expression to check.</param>
    /// <param name="tupleParameter">Tuple parameter that access must be on.</param>
    /// <returns></returns>
    public static  bool IsTupleAccess(this Expression expression, ParameterExpression tupleParameter)
    {
      if (tupleParameter==null)
        return expression.AsTupleAccess()!=null;
      return expression.AsTupleAccess(tupleParameter)!=null;
    }

    /// <summary>
    /// Checks if expression is access to tuple.
    /// </summary>
    /// <param name="expression">Expression to check.</param>
    /// <returns></returns>
    public static bool IsTupleAccess(this Expression expression)
    {
      return expression.IsTupleAccess(null);
    }

    /// <summary>
    /// If <paramref name="expression"/> is an access to tuple element
    /// returns <paramref name="expression"/> casted to <see cref="MethodCallExpression"/>.
    /// Otherwise returns <see langword="null"/>.
    /// </summary>
    /// <param name="expression">An expression to check.</param>
    /// <returns></returns>
    public static MethodCallExpression AsTupleAccess(this Expression expression)
    {
      if (expression.NodeType == ExpressionType.Call) {
        var mc = (MethodCallExpression)expression;
        if (mc.Object != null && mc.Object.Type == typeof(Tuple))
          if (mc.Method.Name == Reflection.WellKnown.Tuple.GetValue || mc.Method.Name == Reflection.WellKnown.Tuple.GetValueOrDefault)
            return mc;
      }
      return null;
    }

    /// <summary>
    /// If <paramref name="expression"/> is an access to tuple element.
    /// returns <paramref name="expression"/> casted to <see cref="MethodCallExpression"/>.
    /// Otherwise returns <see langword="null"/>.
    /// This method only accepts access to specified parameter and access to outer parameters (<see cref="ApplyParameter"/>).
    /// </summary>
    /// <param name="expression">An expression to check.</param>
    /// <param name="currentParameter"><see cref="ParameterExpression"/> considered as current parameter.</param>
    /// <returns></returns>
    public static MethodCallExpression AsTupleAccess(this Expression expression, ParameterExpression currentParameter)
    {
      var tupleAccess = expression.AsTupleAccess();
      if (tupleAccess == null)
        return null;
      var target = tupleAccess.Object;
      if (target == currentParameter || GetApplyParameterExpression(tupleAccess) != null)
        return tupleAccess;
      return null;
    }

    /// <summary>
    /// If <paramref name="expression"/> is an access to tuple element.
    /// returns <paramref name="expression"/> casted to <see cref="MethodCallExpression"/>.
    /// Otherwise returns <see langword="null"/>.
    /// This method only accepts access to specified parameters and access to outer parameters (<see cref="ApplyParameter"/>).
    /// </summary>
    /// <param name="expression">An expression to check.</param>
    /// <param name="currentParameters"><see cref="ParameterExpression"/>s  considered as current parameters.</param>
    /// <returns></returns>
    public static MethodCallExpression AsTupleAccess(this Expression expression, IEnumerable<ParameterExpression> currentParameters)
    {
      var tupleAccess = expression.AsTupleAccess();
      if (tupleAccess == null)
        return null;
      var target = tupleAccess.Object as ParameterExpression;
      if (target!=null && currentParameters.Contains(target) || GetApplyParameterExpression(tupleAccess) != null)
        return tupleAccess;
      return null;
    }

    /// <summary>
    /// Gets the tuple access argument (column index).
    /// </summary>
    /// <param name="expression">An expression describing an access to tuple element.</param>
    /// <returns></returns>
    public static int GetTupleAccessArgument(this Expression expression)
    {
      var mc = expression.AsTupleAccess();
      if (mc == null)
        throw new ArgumentException(string.Format(Strings.ExParameterXIsNotATupleAccessExpression, "expression"));
      return Evaluate<int>(mc.Arguments[0]);
    }

    /// <summary>
    /// Tries to extract apply parameter from <paramref name="expression"/>.
    /// If <paramref name="expression"/> is an access to column of outer tuple returns <see cref="ApplyParameter"/> instance.
    /// Otherwise returns <see langword="null"/>.
    /// </summary>
    /// <param name="expression">The expression describing an access to outer tuple.</param>
    /// <returns></returns>
    public static ApplyParameter GetApplyParameter(this Expression expression)
    {
      var e = GetApplyParameterExpression(expression);
      return e==null ? null : Evaluate<ApplyParameter>(e);
    }

    private static Expression GetApplyParameterExpression(Expression expression)
    {
      var tupleAccess = expression.AsTupleAccess();
      if (tupleAccess == null)
        return null;
      if (tupleAccess.Object.NodeType != ExpressionType.MemberAccess)
        return null;
      var memberAccess = (MemberExpression)tupleAccess.Object;
      if (memberAccess.Expression == null ||
        memberAccess.Expression.Type != typeof(ApplyParameter) ||
          memberAccess.Member.Name != "Value")
        return null;
      return memberAccess.Expression;
    }

    private static T Evaluate<T> (Expression expression)
    {
      if (expression.NodeType == ExpressionType.Constant)
        return (T) ((ConstantExpression) expression).Value;
      return Expression.Lambda<Func<T>>(expression).CachingCompile().Invoke();
    }
  }
}