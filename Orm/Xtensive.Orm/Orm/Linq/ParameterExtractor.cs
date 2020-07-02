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
using Xtensive.Orm.Internals;
using Xtensive.Reflection;
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

    /// <inheritdoc/>
    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      isParameter = true;
      return base.VisitMemberAccess(m);
    }

    /// <inheritdoc/>
    protected override Expression VisitUnknown(Expression e) => e;

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
        if (string.Equals(nameof(Parameter<T>.Value), ma.Member.Name, StringComparison.Ordinal)
          && WellKnownOrmTypes.Parameter.IsAssignableFrom(ma.Expression.Type)) {
          var parameterType = ma.Expression.Type;
          var parameterValueType = parameterType.IsGenericType
            ? parameterType.GetGenericArguments()[0]
            : WellKnownTypes.Object;
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