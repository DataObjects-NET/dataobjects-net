// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.18

using System;
using System.Linq.Expressions;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Linq;

namespace Xtensive.Storage.Linq
{
  /// <summary>
  /// Expression visitor that determines whether <see cref="Expression"/> could be parameter.
  /// </summary>
  public sealed class ParameterExtractor : ExpressionVisitor
  {
    private readonly ExpressionEvaluator evaluator;
    private bool containsMemberAccess;

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
      containsMemberAccess = false;
      Visit(e);
      return containsMemberAccess;
    }

    /// <summary>
    /// Extracts the parameter.
    /// </summary>
    /// <param name="expression">The expression.</param>
    public Expression<Func<T>> ExtractParameter<T>(Expression expression)
    {
      if (expression.NodeType == ExpressionType.Lambda)
        return (Expression<Func<T>>) expression;
      Type type = expression.Type;
      if (type.IsValueType)
        expression = Expression.Convert(expression, typeof(T));
      var lambda = Expression.Lambda<Func<T>>(expression);
      return lambda;
    }

    /// <inheritdoc/>
    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      containsMemberAccess = true;
      return base.VisitMemberAccess(m);
    }

    /// <inheritdoc/>
    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }
    
    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ParameterExtractor(ExpressionEvaluator evaluator)
    {
      this.evaluator = evaluator;
    }
  }
}