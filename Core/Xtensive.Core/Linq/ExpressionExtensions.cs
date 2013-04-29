// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.21

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Linq
{
  /// <summary>
  /// <see cref="Expression"/> related extension methods.
  /// </summary>
  public static class ExpressionExtensions
  {
    private static readonly MethodInfo TupleGenericAccessor;

    ///<summary>
    /// Makes <see cref="Tuples.Tuple.GetValueOrDefault{T}"/> method call.
    ///</summary>
    ///<param name="target">Target expression.</param>
    ///<param name="accessorType">Type of accessor.</param>
    ///<param name="index">Tuple field index.</param>
    ///<returns><see cref="MethodCallExpression"/></returns>
    public static MethodCallExpression MakeTupleAccess(this Expression target, Type accessorType, int index)
    {
      return Expression.Call(
        target,
        TupleGenericAccessor.MakeGenericMethod(accessorType),
        Expression.Constant(index)
        );
    }


    /// <summary>
    /// Makes <c>IsNull</c> condition expression.
    /// </summary>
    /// <param name="target">Target expression</param>
    /// <param name="ifNull">Result expression if <paramref name="target"/> is <see langword="null" />.</param>
    /// <param name="ifNotNull">Result expression if <paramref name="target"/> is not <see langword="null" />.</param>
    /// <returns><see cref="ConditionalExpression"/></returns>
    public static ConditionalExpression MakeIsNullCondition(this Expression target, Expression ifNull, Expression ifNotNull)
    {
      return Expression.Condition(
        Expression.Equal(target, Expression.Constant(null, target.Type)),
        ifNull, ifNotNull
        );
    }

    /// <summary>
    /// Converts expression type to nullable type (for value types).
    /// </summary>
    /// <param name="expression">The expression.</param>
    public static Expression LiftToNullable(this Expression expression)
    {
      return expression.Type.IsNullable() 
               ? expression 
               : Expression.Convert(expression, expression.Type.ToNullable());
    }

    /// <summary>
    /// Converts specified <see cref="Expression"/> to <see cref="ExpressionTree"/>.
    /// </summary>
    /// <param name="expression">The expression to convert.</param>
    /// <returns>Expression tree that wraps <paramref name="expression"/>.</returns>
    public static ExpressionTree ToExpressionTree(this Expression expression)
    {
      return new ExpressionTree(expression);
    }


    // Type initializer

    static ExpressionExtensions()
    {
      TupleGenericAccessor = typeof (Tuple).GetMethods().Single(mi => mi.Name==WellKnown.Tuple.GetValueOrDefault && mi.IsGenericMethod);
    }
  }
}