// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.21

using System;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Linq;
using Xtensive.Core.Linq.SerializableExpressions;
using Xtensive.Core.Linq.SerializableExpressions.Internals;
using Xtensive.Core.Resources;

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
    /// Determines whether the specified expression is <see cref="ConstantExpression"/> 
    /// with <see langword="null" /> <see cref="ConstantExpression.Value"/>.
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
    /// Bind parameter expressions to <see cref="LambdaExpression"/>.
    /// </summary>
    /// <param name="lambdaExpression"><see cref="LambdaExpression"/> to bind parameters.</param>
    /// <param name="parameters"><see cref="Expression"/>s to bind to <paramref name="lambdaExpression"/></param>
    /// <returns>Body of <paramref name="lambdaExpression"/> with lambda's parameters replaced 
    /// with corresponding expression from <paramref name="parameters"/></returns>
    /// <exception cref="InvalidOperationException">Something went wrong :(.</exception>
    public static Expression BindParameters(this LambdaExpression lambdaExpression, params Expression[] parameters)
    {
      if (lambdaExpression.Parameters.Count!=parameters.Length)
        throw new InvalidOperationException(String.Format(
          Strings.ExUnableToBindParametersToLambdaXParametersCountIsIncorrect, 
          lambdaExpression.ToString(true)));
      if (parameters.Length==0)
        return lambdaExpression;
      var convertedParameters = new Expression[parameters.Length];
      for (int i = 0; i < lambdaExpression.Parameters.Count; i++) {
        var expressionParameter = lambdaExpression.Parameters[i];
        if (expressionParameter.Type.IsAssignableFrom(parameters[i].Type))
          convertedParameters[i] = expressionParameter.Type==parameters[i].Type
            ? parameters[i]
            : Expression.Convert(parameters[i], expressionParameter.Type);
        else
          throw new InvalidOperationException(String.Format(
            Strings.ExUnableToUseExpressionXAsXParameterOfLambdaXBecauseOfTypeMistmatch, 
            parameters[i].ToString(true), i , lambdaExpression.Parameters[i].ToString(true)));
      }
      return ExpressionReplacer.ReplaceAll(
        lambdaExpression.Body, lambdaExpression.Parameters.ToArray(), convertedParameters);
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

    #region GetXxx methods

    /// <summary>
    /// Gets the <see cref="MemberInfo"/> from passed <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">The expression to analyze.</param>
    /// <returns><see cref="MemberInfo"/> the expression references by its root <see cref="MemberExpression"/>.</returns>
    /// <exception cref="ArgumentException">The root node of expression isn't of <see cref="MemberExpression"/> type.</exception>
    public static MemberInfo GetMember(this Expression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      expression = expression.StripLambda().StripCasts();
      var me = expression as MemberExpression;
      if (me == null)
        throw new ArgumentException(
          Strings.ExInvalidArgumentType.FormatWith(typeof (MemberExpression)), "expression");
      return me.Member;
    }

    /// <summary>
    /// Gets the <see cref="FieldInfo"/> from passed <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">The expression to analyze.</param>
    /// <returns><see cref="MemberInfo"/> the expression references by its root <see cref="MemberExpression"/>.</returns>
    /// <exception cref="ArgumentException">Expression must reference field.</exception>
    public static FieldInfo GetField(this Expression expression)
    {
      var mi = GetMember(expression);
      var fi = mi as FieldInfo;
      if (fi == null)
        throw new ArgumentException(
          Strings.ExExpression0MustReferenceField.FormatWith(expression.ToString(true)));
      return fi;
    }

    /// <summary>
    /// Gets the <see cref="PropertyInfo"/> from passed <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">The expression to analyze.</param>
    /// <returns><see cref="MemberInfo"/> the expression references by its root <see cref="MemberExpression"/>.</returns>
    /// <exception cref="ArgumentException">Expression must reference property.</exception>
    public static PropertyInfo GetProperty(this Expression expression)
    {
      var mi = GetMember(expression);
      var pi = mi as PropertyInfo;
      if (pi == null)
        throw new ArgumentException(
          Strings.ExExpression0MustReferenceProperty.FormatWith(expression.ToString(true)));
      return pi;
    }

    /// <summary>
    /// Gets the <see cref="MethodInfo"/> from passed <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">The expression to analyze.</param>
    /// <returns><see cref="MethodInfo"/> the expression references by its root <see cref="MethodCallExpression"/>.</returns>
    /// <exception cref="ArgumentException">Expression must reference event.</exception>
    public static MethodInfo GetMethod(this Expression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      expression = expression.StripLambda().StripCasts();
      var mce = expression as MethodCallExpression;
      if (mce == null)
        throw new ArgumentException(
          Strings.ExInvalidArgumentType.FormatWith(typeof (MethodCallExpression)), "expression");
      return mce.Method;
    }

#if NET40
    /// <summary>
    /// Gets the index <see cref="PropertyInfo"/> from passed <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">The expression to analyze.</param>
    /// <returns><see cref="PropertyInfo"/> the expression references by its root <see cref="IndexExpression"/>.</returns>
    /// <exception cref="ArgumentException">Expression must reference event.</exception>
    public static PropertyInfo GetIndexer(this Expression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      expression = expression.StripLambda().StripCasts();
      var ie = expression as IndexExpression;
      if (ie == null)
        throw new ArgumentException(
          Strings.ExInvalidArgumentType.FormatWith(typeof (IndexExpression)), "expression");
      return ie.Indexer;
    }
#endif

    /// <summary>
    /// Gets the <see cref="ConstructorInfo"/> from passed <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">The expression to analyze.</param>
    /// <returns><see cref="ConstructorInfo"/> the expression references by its root <see cref="NewExpression"/>.</returns>
    /// <exception cref="ArgumentException">Expression must reference event.</exception>
    public static ConstructorInfo GetConstructor(this Expression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      expression = expression.StripLambda().StripCasts();
      var ne = expression as NewExpression;
      if (ne == null)
        throw new ArgumentException(
          Strings.ExInvalidArgumentType.FormatWith(typeof (NewExpression)), "expression");
      return ne.Constructor;
    }

    #endregion

    #region StripXxx methods

    /// <summary>
    /// Strips <see cref="Expression.Quote"/> expressions.
    /// </summary>
    /// <param name="expression">The expression.</param>
    public static LambdaExpression StripQuotes(this Expression expression)
    {
      while (expression.NodeType == ExpressionType.Quote)
        expression = ((UnaryExpression)expression).Operand;
      return (LambdaExpression) expression;
    }

    /// <summary>
    /// Strips <see cref="ExpressionType.Convert"/> and <see cref="ExpressionType.TypeAs"/>.
    /// </summary>
    /// <param name="expression">The expression.</param>
    public static Expression StripCasts(this Expression expression)
    {
      while (expression.NodeType == ExpressionType.Convert
             || expression.NodeType == ExpressionType.TypeAs)
        expression = ((UnaryExpression) expression).Operand;
      return expression;
    }

    /// <summary>
    /// Strips the lambda.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns></returns>
    public static Expression StripLambda(this Expression expression)
    {
      if (expression.NodeType == ExpressionType.Lambda)
        return ((LambdaExpression) expression).Body;
      else
        return expression;
    }

    #endregion
  }
}