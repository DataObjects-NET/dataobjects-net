// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.21

using System.Linq.Expressions;

namespace Xtensive.Core.Linq
{
  /// <summary>
  /// <see cref="Expression"/> realted extension methods.
  /// </summary>
  public static class ExpressionExtensions
  {
    /// <summary>
    /// Strips <see cref="Expression.Quote"/> expressions.
    /// </summary>
    /// <param name="expression">The expression.</param>
    public static LambdaExpression StripQuotes(this Expression expression)
    {
      while (expression.NodeType == ExpressionType.Quote)
        expression = ((UnaryExpression)expression).Operand;
      return (LambdaExpression)expression;
    }

    /// <summary>
    /// Strips <see cref="Expression.Convert"/> and <see cref="Expression.TypeAs"/>.
    /// </summary>
    /// <param name="expression">The expression.</param>
    public static Expression StripCasts(this Expression expression)
    {
      while (expression.NodeType == ExpressionType.Convert
        || expression.NodeType == ExpressionType.TypeAs)
        expression = ((UnaryExpression)expression).Operand;
      return expression;
    }
  }
}