// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.21

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core.Linq.Internals;

namespace Xtensive.Core.Linq
{
  /// <summary>
  /// <see cref="Expression"/> realted extension methods.
  /// </summary>
  public static class ExpressionExtensions
  {
    /// <summary>
    /// Formats the <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">The expression to format.</param>
    /// <param name="inCSharpNotation">If set to <see langword="true"/>, 
    /// the result will be returned in C# notation 
    /// (<see cref="ExpressionWriter"/> will be used).</param>
    /// <returns>A string containing formatted expression.</returns>
    public static string ToString(this Expression expression, bool inCSharpNotation)
    {
      if (!inCSharpNotation)
        return expression.ToString();

      return ExpressionWriter.Write(expression);

      //      // The old code
      //      string result = expression.ToString();
      //      result = Regex.Replace(result, @"value\([^)]+DisplayClass[^)]+\)\.", "");
      //      return result;
    }
    
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
    
    /// <summary>
    /// Converts specified <see cref="Expression"/> to <see cref="ExpressionTree"/>.
    /// </summary>
    /// <param name="expression">The expression to convert.</param>
    /// <returns>Expression tree that wraps <paramref name="expression"/>.</returns>
    public static ExpressionTree ToExpressionTree(this Expression expression)
    {
      return new ExpressionTree(expression);
    }

    /// <summary>
    /// Generates <see cref="LambdaExpression"/> faster than <see cref="Expression.Lambda(Expression,ParameterExpression[])"/>.
    /// </summary>
    /// <param name="body">The body of lambda expression.</param>
    /// <param name="parameters">The parameters of lambda expression.</param>
    /// <returns>Constructed lambda expression.</returns>
    public static LambdaExpression FastLambda(Expression body, params ParameterExpression[] parameters)
    {
      return LambdaExpressionFactory.CreateLambda(body, parameters);
    }

    /// <summary>
    /// Generates <see cref="LambdaExpression"/> faster than <see cref="Expression.Lambda(Expression,ParameterExpression[])"/>.
    /// </summary>
    /// <param name="body">The body of lambda expression.</param>
    /// <param name="parameters">The parameters of lambda expression.</param>
    /// <returns>Constructed lambda expression.</returns>
    public static LambdaExpression FastLambda(Expression body, IEnumerable<ParameterExpression> parameters)
    {
      return LambdaExpressionFactory.CreateLambda(body, parameters.ToArray());
    }
  }
}