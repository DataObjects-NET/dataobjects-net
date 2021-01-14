// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.05.07

using System;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Orm.Internals;

namespace Xtensive.Orm.Linq.Expressions.Visitors
{
  internal abstract class PersistentExpressionVisitor : ExtendedExpressionVisitor
  {
    protected override Expression VisitProjectionExpression(ProjectionExpression projectionExpression) =>
      throw Exceptions.InternalError(string.Format(Strings.ExXDoesNotSupportX,
        typeof(PersistentExpressionVisitor), WellKnownOrmTypes.ProjectionExpression), OrmLog.Instance);

    protected override Expression VisitItemProjectorExpression(ItemProjectorExpression itemProjectorExpression)
    {
      throw Exceptions.InternalError(String.Format(Strings.ExXDoesNotSupportX, typeof (PersistentExpressionVisitor), typeof (ItemProjectorExpression)), OrmLog.Instance);
    }
  }
}