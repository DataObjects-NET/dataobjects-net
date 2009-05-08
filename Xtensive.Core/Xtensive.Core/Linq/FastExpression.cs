// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.08

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core.Linq.Internals;

namespace Xtensive.Core.Linq
{
  /// <summary>
  /// Factory methods for various descendants of <see cref="Expression"/> that is faster than orignal ones.
  /// </summary>
  public static class FastExpression
  {
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