// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.07

using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Xtensive.Storage.Linq.Expressions.Visitors
{
  internal abstract class PersistentExpressionVisitor : ExtendedExpressionVisitor
  {
    protected override Expression VisitProjectionExpression(ProjectionExpression projectionExpression)
    {
      throw new NotSupportedException("PersistentExpressionVisitor does not support ProjectionExpression.");
    }

    protected override Expression VisitItemProjectorExpression(ItemProjectorExpression itemProjectorExpression)
    {
      throw new NotSupportedException("PersistentExpressionVisitor does not support ItemProjectorExpression.");
    }
  }
}