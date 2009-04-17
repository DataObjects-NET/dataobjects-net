// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.03.20

using System;
using System.Linq.Expressions;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.Expressions
{
  /// <summary>
  /// Various extension methods for manipulating expressions with <see cref="Tuple"/>.
  /// </summary>
  public static class TupleExpressionHelper
  {
    /// <summary>
    /// If <paramref name="expression"/> is an access to tuple element
    /// returns <paramref name="expression"/> casted to <see cref="MethodCallExpression"/>.
    /// Otherwise returns <see langword="null"/>.
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static MethodCallExpression AsTupleAccess(this Expression expression)
    {
      if (expression.NodeType == ExpressionType.Call) {
        var mc = (MethodCallExpression)expression;
        if (mc.Object != null && mc.Object.Type == typeof(Tuple))
          if (mc.Method.Name == WellKnown.Tuple.GetValue || mc.Method.Name == WellKnown.Tuple.GetValueOrDefault)
            return mc;
      }
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
    public static ApplyParameter ExtractApplyParameterFromTupleAccess(this Expression expression)
    {
      var tupleAccess = expression.AsTupleAccess();
      if (tupleAccess == null)
        return null;
      if (tupleAccess.Object.NodeType != ExpressionType.MemberAccess)
        return null;
      var memberAccess = (MemberExpression) tupleAccess.Object;
      if (memberAccess.Expression == null ||
          memberAccess.Expression.Type != typeof(ApplyParameter) ||
          memberAccess.Member.Name != "Value")
        return null;
      return Evaluate<ApplyParameter>(memberAccess.Expression);
    }

    private static T Evaluate<T> (Expression expression)
    {
      if (expression.NodeType == ExpressionType.Constant)
        return (T) ((ConstantExpression) expression).Value;
      return Expression.Lambda<Func<T>>(expression).Compile().Invoke();
    }
  }
}