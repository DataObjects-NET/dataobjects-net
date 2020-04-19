// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.08

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Xtensive.Linq
{
  /// <summary>
  /// Factory methods for various descendants of <see cref="Expression"/> that are faster 
  /// than original ones.
  /// </summary>
  public static class FastExpression
  {
    /// <summary>
    /// Generates <see cref="LambdaExpression"/> faster than <see cref="Expression.Lambda(Type,Expression,ParameterExpression[])"/>.
    /// </summary>
    /// <param name="delegateType">A type that represents a delegate type.</param>
    /// <param name="body">The body of lambda expression.</param>
    /// <param name="parameters">The parameters of lambda expression.</param>
    /// <returns>Constructed lambda expression.</returns>
    public static LambdaExpression Lambda(Type delegateType, Expression body, params ParameterExpression[] parameters)
    {
      return LambdaExpressionFactory.Instance.CreateLambda(delegateType, body, parameters);
    }

    /// <summary>
    /// Generates <see cref="LambdaExpression"/> faster than <see cref="Expression.Lambda(Type,Expression,ParameterExpression[])"/>.
    /// </summary>
    /// <param name="body">The body of lambda expression.</param>
    /// <param name="parameters">The parameters of lambda expression.</param>
    /// <returns>Constructed lambda expression.</returns>
    public static Expression<TDelegate> Lambda<TDelegate>(Expression body, params ParameterExpression[] parameters)
    {
      return (Expression<TDelegate>) LambdaExpressionFactory.Instance.CreateLambda(typeof(TDelegate), body, parameters);
    }

    /// <summary>
    /// Generates <see cref="LambdaExpression"/> faster than <see cref="Expression.Lambda(Type,Expression,IEnumerable{ParameterExpression})"/>.
    /// </summary>
    /// <param name="delegateType">A type that represents a delegate type.</param>
    /// <param name="body">The body of lambda expression.</param>
    /// <param name="parameters">The parameters of lambda expression.</param>
    /// <returns>Constructed lambda expression.</returns>
    public static LambdaExpression Lambda(Type delegateType, Expression body, IEnumerable<ParameterExpression> parameters)
    {
      return LambdaExpressionFactory.Instance.CreateLambda(delegateType, body, parameters.ToArray());
    }

    /// <summary>
    /// Generates <see cref="LambdaExpression"/> faster than <see cref="Expression.Lambda(Expression,ParameterExpression[])"/>.
    /// </summary>
    /// <param name="body">The body of lambda expression.</param>
    /// <param name="parameters">The parameters of lambda expression.</param>
    /// <returns>Constructed lambda expression.</returns>
    public static LambdaExpression Lambda(Expression body, params ParameterExpression[] parameters)
    {
      return LambdaExpressionFactory.Instance.CreateLambda(body, parameters);
    }

    /// <summary>
    /// Generates <see cref="LambdaExpression"/> faster than <see cref="Expression.Lambda(Expression,ParameterExpression[])"/>.
    /// </summary>
    /// <param name="body">The body of lambda expression.</param>
    /// <param name="parameters">The parameters of lambda expression.</param>
    /// <returns>Constructed lambda expression.</returns>
    public static LambdaExpression Lambda(Expression body, IEnumerable<ParameterExpression> parameters)
    {
      return LambdaExpressionFactory.Instance.CreateLambda(body, parameters.ToArray());
    }
  }
}