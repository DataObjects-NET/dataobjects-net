// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.21

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Linq.SerializableExpressions;
using Xtensive.Core.Linq.SerializableExpressions.Internals;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;

namespace Xtensive.Core.Linq
{
  /// <summary>
  /// <see cref="Expression"/> related extension methods.
  /// </summary>
  public static class ExpressionExtensions
  {
    private static readonly MethodInfo TupleGenericAccessor;

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

    ///<summary>
    /// Makes <see cref="Tuple.GetValueOrDefault{T}<>"/> method call.
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
    /// Bind parameter expressions to <see cref="LambdaExpression"/>.
    /// </summary>
    /// <param name="lambdaExpression"><see cref="LambdaExpression"/> to bind parameters.</param>
    /// <param name="parameters"><see cref="Expression"/>s to bind to <paramref name="lambdaExpression"/></param>
    /// <returns>Body of <paramref name="lambdaExpression"/> with lambda's parameters replaced with corresponding expression from <paramref name="parameters"/></returns>
    public static Expression BindParameters(this LambdaExpression lambdaExpression, params Expression[] parameters)
    {
      if (lambdaExpression.Parameters.Count!=parameters.Length)
        throw new InvalidOperationException(String.Format(Resources.Strings.ExUnableToBindParametersToLambdaXParametersCountIsIncorrect, lambdaExpression.ToString(true)));
      var convertedParameters = new Expression[parameters.Length];
      for (int i = 0; i < lambdaExpression.Parameters.Count; i++) {
        var expressionParameter = lambdaExpression.Parameters[i];
        if (expressionParameter.Type.IsAssignableFrom(parameters[i].Type))
          convertedParameters[i] = expressionParameter.Type==parameters[i].Type
            ? parameters[i]
            : Expression.Convert(parameters[i], expressionParameter.Type);
        else
          throw new InvalidOperationException(String.Format(Resources.Strings.ExUnableToUseExpressionXAsXParameterOfLambdaXBecauseOfTypeMistmatch, parameters[i].ToString(true), i , lambdaExpression.Parameters[i].ToString(true)));
      }
      return ExpressionReplacer.ReplaceAll(lambdaExpression.Body, lambdaExpression.Parameters.ToArray(), convertedParameters);
    }

    /// <summary>
    /// Makes <c>IsNull</c> condition expression.
    /// </summary>
    /// <param name="target">Target expression</param>
    /// <param name="ifNull">Result expression if <paramref name="target"/> is null.</param>
    /// <param name="ifNotNull">Result expression if <paramref name="target"/> is not null.</param>
    /// <returns><see cref="ConditionalExpression"/></returns>
    public static ConditionalExpression MakeIsNullCondition(this Expression target, Expression ifNull, Expression ifNotNull)
    {
      return Expression.Condition(
        Expression.Equal(target, Expression.Constant(null, target.Type)),
        ifNull, ifNotNull
        );
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
    /// Strips <see cref="ExpressionType.Convert"/> and <see cref="ExpressionType.TypeAs"/>.
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
    /// Determines whether the specified expression is ConstantExpression with <see langword="null" /> <see cref="ConstantExpression.Value"/>.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns>
    ///   <see langword="true" /> if the specified expression is null; otherwise, <see langword="false" />.
    /// </returns>
    public static bool IsNull(this Expression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      if (expression.NodeType == ExpressionType.Constant) {
        var constantExpression = (ConstantExpression)expression;
        return constantExpression.Value == null;
      }
      return false;
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

    static ExpressionExtensions()
    {
      TupleGenericAccessor = typeof (Tuples.Tuple)
          .GetMethods()
          .Where(mi => mi.Name==WellKnown.Tuple.GetValueOrDefault && mi.IsGenericMethod)
          .Single();
    }
  }
}