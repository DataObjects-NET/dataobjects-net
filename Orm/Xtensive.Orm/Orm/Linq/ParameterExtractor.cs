// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.18

using System;
using System.Linq.Expressions;
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

    /// <summary>
    /// Extracts the parameter.
    /// </summary>
    /// <param name="expression">The expression.</param>
    public Expression<Func<T>> ExtractParameter<T>(Expression expression)
    {
      if (expression.NodeType==ExpressionType.Lambda)
        return (Expression<Func<T>>) expression;
      Type type = expression.Type;
      if (type.IsValueType)
        expression = Expression.Convert(expression, typeof (T));
      var lambda = FastExpression.Lambda<Func<T>>(expression);
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
}