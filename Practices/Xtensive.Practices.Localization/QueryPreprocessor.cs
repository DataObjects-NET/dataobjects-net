// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.28

using System;
using System.Linq.Expressions;
using Xtensive.IoC;
using Xtensive.Orm;
using Xtensive.Practices.Localization.Internals;

namespace Xtensive.Practices.Localization
{
  [Service(typeof(IQueryPreprocessor))]
  public class QueryPreprocessor : IQueryPreprocessor
  {
    private static readonly LocalizationExpressionVisitor Visitor = new LocalizationExpressionVisitor();

    /// <inheritdoc/>
    public Expression Apply(Expression query)
    {
      return Visitor.VisitExpression(query);
    }

    /// <inheritdoc/>
    public bool IsDependentOn(IQueryPreprocessor other)
    {
      return false;
    }
  }
}