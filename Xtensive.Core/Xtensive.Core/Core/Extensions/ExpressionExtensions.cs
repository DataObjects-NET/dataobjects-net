// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.21

using System;
using System.Linq.Expressions;
using Xtensive.Linq;
using Xtensive.Linq.SerializableExpressions;
using Xtensive.Linq.SerializableExpressions.Internals;
using Xtensive.Resources;

namespace Xtensive.Core
{
  /// <summary>
  /// <see cref="Expression"/> related extension methods.
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
    /// Bind parameter expressions to <see cref="LambdaExpression"/>.
    /// </summary>
    /// <param name="lambdaExpression"><see cref="LambdaExpression"/> to bind parameters.</param>
    /// <param name="parameters"><see cref="Expression"/>s to bind to <paramref name="lambdaExpression"/></param>
    /// <returns>Body of <paramref name="lambdaExpression"/> with lambda's parameters replaced with corresponding expression from <paramref name="parameters"/></returns>
    /// <exception cref="InvalidOperationException">Something went wrong :(.</exception>
    public static Expression BindParameters(this LambdaExpression lambdaExpression, params Expression[] parameters)
    {
      if (lambdaExpression.Parameters.Count!=parameters.Length)
        throw new InvalidOperationException(String.Format(
          Strings.ExUnableToBindParametersToLambdaXParametersCountIsIncorrect, lambdaExpression.ToString(true)));
      var convertedParameters = new Expression[parameters.Length];
      for (int i = 0; i < lambdaExpression.Parameters.Count; i++) {
        var expressionParameter = lambdaExpression.Parameters[i];
        if (expressionParameter.Type.IsAssignableFrom(parameters[i].Type))
          convertedParameters[i] = expressionParameter.Type==parameters[i].Type
            ? parameters[i]
            : Expression.Convert(parameters[i], expressionParameter.Type);
        else
          throw new InvalidOperationException(String.Format(
            Strings.ExUnableToUseExpressionXAsXParameterOfLambdaXBecauseOfTypeMistmatch, parameters[i].ToString(true), i , lambdaExpression.Parameters[i].ToString(true)));
      }
      return ExpressionReplacer.ReplaceAll(lambdaExpression.Body, lambdaExpression.Parameters.ToArray(), convertedParameters);
    }


    /// <summary>
    /// Converts specified <see cref="Expression"/> to <see cref="SerializableExpression"/>.
    /// </summary>
    /// <param name="expression">The expression to convert.</param>
    /// <returns>Serializable expression that represents <paramref name="expression"/>.</returns>
    public static SerializableExpression ToSerializableExpression(this Expression expression)
    {
      return new ExpressionToSerializableExpressionConverter(expression).Convert();
    }

    /// <summary>
    /// Converts specified <see cref="SerializableExpression"/> to <see cref="Expression"/>.
    /// </summary>
    /// <param name="expression">The expression to convert.</param>
    /// <returns></returns>
    public static Expression ToExpression(this SerializableExpression expression)
    {
      return new SerializableExpressionToExpressionConverter(expression).Convert();
    }
  }
}