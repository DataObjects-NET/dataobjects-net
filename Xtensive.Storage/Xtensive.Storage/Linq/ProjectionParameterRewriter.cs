// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.01.31

using System;
using System.Linq.Expressions;
using Xtensive.Core.Linq;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Linq
{
  internal class ProjectionParameterRewriter : ExpressionVisitor
  {
    private ParameterExpression tuple;
    private ParameterExpression record;
    private bool recordIsUsed;

    public Expression Rewrite(Expression e, out bool recordIsUsed)
    {
      this.recordIsUsed = false;
      var result = Visit(e);
      recordIsUsed = this.recordIsUsed;
      return result;
    }

    protected override Expression VisitParameter(ParameterExpression p)
    {
      if (p.Type == typeof(Tuple))
        return tuple;
      if (p.Type == typeof(Record)) {
        recordIsUsed = true;
        return record;
      }
      throw new NotSupportedException();
    }

    // Constructors

    public ProjectionParameterRewriter(ParameterExpression tuple, ParameterExpression record)
    {
      this.tuple = tuple;
      this.record = record;
    }
  }
}