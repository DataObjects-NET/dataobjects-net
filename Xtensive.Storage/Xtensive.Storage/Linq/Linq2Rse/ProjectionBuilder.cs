// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2009.01.13

using System;
using System.Linq.Expressions;
using Xtensive.Storage.Linq.Expressions.Visitors;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq.Linq2Rse
{
  internal class ProjectionBuilder : ExpressionVisitor
  {
    private readonly RseQueryTranslator translator;

    public Expression<Func<RecordSet, object>> Build(LambdaExpression le)
    {
      throw new NotImplementedException();
    }


    // Constructor

    public ProjectionBuilder(RseQueryTranslator translator)
    {
      this.translator = translator;
    }
  }
}