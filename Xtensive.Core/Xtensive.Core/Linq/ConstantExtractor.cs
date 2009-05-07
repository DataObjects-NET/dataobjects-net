// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.06

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Linq
{
  /// <summary>
  /// An <see cref="ExpressionVisitor"/> specialized for extracting constants from specified <see cref="Expression"/>.
  /// This class can be used to produce "normalized" expression with all constants extracted to additional parameter.
  /// </summary>
  public sealed class ConstantExtractor : ExpressionVisitor
  {
    private readonly LambdaExpression lambda;
    private readonly ParameterExpression constantParameter;
    private List<object> constantValues;

    /// <summary>
    /// Gets an array of extracted constants.
    /// </summary>
    /// <value></value>
    public object[] GetConstants()
    {
      if (constantValues == null)
        throw new InvalidOperationException();
      return constantValues.ToArray();
    }

    /// <summary>
    /// Extracts constants from <see cref="LambdaExpression"/> specified in constructor.
    /// Result is a <see cref="LambdaExpression"/> with one additional parameter (array of objects).
    /// Extra parameter is added to first position.
    /// </summary>
    /// <returns><see cref="LambdaExpression"/> with all constants extracted to additional parameter.</returns>
    public LambdaExpression Process()
    {
      if (constantValues != null)
        throw new InvalidOperationException();
      constantValues = new List<object>();
      var parameters = EnumerableUtils.One(constantParameter).Concat(lambda.Parameters).ToArray();
      return ExpressionExtensions.FastLambda(Visit(lambda.Body), parameters);
    }

    /// <inheritdoc/>
    protected override Expression VisitConstant(ConstantExpression c)
    {
      var result = Expression.Convert(
        Expression.ArrayIndex(constantParameter, Expression.Constant(constantValues.Count)), c.Type);
      constantValues.Add(c.Value);
      return result;
    }

    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="lambda">An expression to process.</param>
    public ConstantExtractor(LambdaExpression lambda)
    {
      ArgumentValidator.EnsureArgumentNotNull(lambda, "lambda");
      this.lambda = lambda;
      constantParameter = Expression.Parameter(typeof (object[]), "constants");
    }
  }
}
