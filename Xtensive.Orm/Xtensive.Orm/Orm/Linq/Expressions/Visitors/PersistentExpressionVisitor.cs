// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.07

using System;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Orm.Linq.Expressions.Visitors
{
  internal abstract class PersistentExpressionVisitor : ExtendedExpressionVisitor
  {
    protected override Expression VisitProjectionExpression(ProjectionExpression projectionExpression)
    {
      throw Exceptions.InternalError(String.Format(Resources.Strings.ExXDoesNotSupportX, typeof (PersistentExpressionVisitor), typeof (ProjectionExpression)), Log.Instance);
    }

    protected override Expression VisitItemProjectorExpression(ItemProjectorExpression itemProjectorExpression)
    {
      throw Exceptions.InternalError(String.Format(Resources.Strings.ExXDoesNotSupportX, typeof (PersistentExpressionVisitor), typeof (ItemProjectorExpression)), Log.Instance);
    }
  }
}