// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.28

using System;
using System.Linq.Expressions;
using Xtensive.IoC;
using Xtensive.Orm;

namespace Xtensive.Practices.Localization
{
  [Service(typeof(IQueryPreprocessor))]
  public class QueryPreprocessor : DomainBound, IQueryPreprocessor
  {
    private readonly LocalizationExpressionVisitor visitor;

    /// <inheritdoc/>
    public Expression Apply(Expression query)
    {
      return visitor.VisitExpression(query);
    }

    /// <inheritdoc/>
    public bool IsDependentOn(IQueryPreprocessor other)
    {
      return false;
    }

    [ServiceConstructor]
    public QueryPreprocessor(Domain domain)
      : base(domain)
    {
      visitor = new LocalizationExpressionVisitor(domain);
    }
  }
}