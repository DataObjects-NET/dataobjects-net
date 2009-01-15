// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2009.01.13

using System;
using System.Linq.Expressions;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Expressions.Visitors;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq.Linq2Rse
{
  internal class ProjectionBuilder : ExpressionVisitor
  {
    private readonly RseQueryTranslator translator;
    private ParameterExpression parameter;

    public ProjectionExpression Build(ProjectionExpression source, Expression body)
    {
      parameter = Expression.Parameter(typeof(RecordSet), "rs");
      var newBody = Expression.Convert(Visit(body), typeof(object));
      var lambda = Expression.Lambda(newBody, parameter);
//      source.RecordSet.Calculate()
      return new ProjectionExpression(body.Type, null, null, (Expression<Func<RecordSet, object>>) lambda);
    }


    // Constructor

    public ProjectionBuilder(RseQueryTranslator translator)
    {
      this.translator = translator;
    }
  }
}