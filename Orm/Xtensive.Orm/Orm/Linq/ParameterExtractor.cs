// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.18

using System;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Linq;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Linq
{
  /// <summary>
  /// Expression visitor that determines whether <see cref="Expression"/> could be parameter.
  /// </summary>
  internal sealed class ParameterExtractor : ExpressionVisitor
  {
    private readonly ExpressionEvaluator evaluator;
    private bool isParameter;

    /// <summary>
    /// Determines whether the specified <paramref name="e"/> is parameter.
    /// </summary>
    /// <param name="e">The expression.</param>
    /// <returns>
    ///   <see langword="true" /> if the specified <paramref name="e"/> is parameter; otherwise, <see langword="false" />.
    /// </returns>
    public bool IsParameter(Expression e)
    {
      if (!evaluator.CanBeEvaluated(e))
        return false;
      isParameter = false;
      Visit(e);
      return isParameter;
    }

    private static readonly MethodInfo GetParameterValueMethod =
      typeof(ParameterContext).GetMethod(nameof(ParameterContext.GetValue));

    /// <summary>
    /// Extracts the parameter.
    /// </summary>
    /// <param name="expression">The expression.</param>
    public static Expression<Func<ParameterContext, T>> ExtractParameter<T>(Expression expression)
    {
      var contextParameter = Expression.Parameter(typeof(ParameterContext), "context");
      if (expression.NodeType==ExpressionType.Lambda) {
        var parameterLambda = (Expression<Func<T>>) expression;
        return FastExpression.Lambda<Func<ParameterContext, T>>(parameterLambda.Body, contextParameter);
      }
      if (expression.NodeType==ExpressionType.MemberAccess) {
        var memberExpression = (MemberExpression) expression;
        var memberOwner = memberExpression.Expression;
        if (typeof(Parameter).IsAssignableFrom(memberOwner.Type) && memberExpression.Member.Name == nameof(Parameter.Value)) {
          var body = Expression.Call(contextParameter, GetParameterValueMethod.MakeGenericMethod(typeof(T)),
            memberOwner);
          return FastExpression.Lambda<Func<ParameterContext, T>>(body, contextParameter);
        }
      }

      var type = expression.Type;
      if (type.IsValueType) {
        expression = Expression.Convert(expression, typeof (T));
      }

      var lambda = FastExpression.Lambda<Func<ParameterContext, T>>(expression, contextParameter);
      return lambda;
    }

    /// <inheritdoc/>
    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      isParameter = true;
      return base.VisitMemberAccess(m);
    }

    /// <inheritdoc/>
    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }

    protected override Expression VisitConstant(ConstantExpression c)
    {
      switch (c.GetMemberType()) {
      case MemberType.Entity:
      case MemberType.Structure:
        isParameter = true;
        break;
      }
      return c;
    }


    // Constructors

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    public ParameterExtractor(ExpressionEvaluator evaluator)
    {
      this.evaluator = evaluator;
    }
  }

  internal static class ParameterAccessorFactory
  {
    private static readonly MethodInfo GetParameterValueMethod =
      typeof(ParameterContext).GetMethod(nameof(ParameterContext.GetValue));

    private class ParameterAccessorFactoryImpl<T>: ExpressionVisitor
    {
      private readonly ParameterExpression parameterContextArgument;

      public Expression<Func<ParameterContext,T>> BindToParameterContext(Expression parameterExpression)
      {
        var body = Visit(parameterExpression);
        if (typeof(T) != body.Type) {
          body = Expression.Convert(body, typeof(T));
        }
        return FastExpression.Lambda<Func<ParameterContext, T>>(body, parameterContextArgument);
      }

      protected override Expression VisitMemberAccess(MemberExpression ma)
      {
        if (string.Equals(nameof(Parameter.Value), ma.Member.Name, StringComparison.Ordinal)
          && typeof(Parameter).IsAssignableFrom(ma.Expression.Type)) {
          var parameterType = ma.Expression.Type;
          var parameterValueType = parameterType.IsGenericType
            ? parameterType.GetGenericArguments()[0]
            : typeof(object);
          return Expression.Call(parameterContextArgument,
            GetParameterValueMethod.MakeGenericMethod(parameterValueType), ma.Expression);
        }

        return base.VisitMemberAccess(ma);
      }

      public ParameterAccessorFactoryImpl(ParameterExpression parameterContextArgument)
      {
        this.parameterContextArgument = parameterContextArgument;
      }
    }

    public static Expression<Func<ParameterContext, T>> CreateAccessorExpression<T>(Expression parameterExpression)
    {
      var parameterContextArgument = Expression.Parameter(typeof(ParameterContext), "context");
      return new ParameterAccessorFactoryImpl<T>(parameterContextArgument).BindToParameterContext(parameterExpression);
    }
  }
}