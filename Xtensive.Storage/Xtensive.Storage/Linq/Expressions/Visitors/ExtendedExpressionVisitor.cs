// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.06

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core.Linq;

namespace Xtensive.Storage.Linq.Expressions.Visitors
{
  internal abstract class ExtendedExpressionVisitor : ExpressionVisitor
  {
    protected override Expression VisitUnknown(Expression expression)
    {
      var extendedExpression = expression as ExtendedExpression;
      if (extendedExpression==null)
        throw new NotSupportedException(string.Format("Expression '{0}' is unknown", expression));
      switch (extendedExpression.ExtendedType) {
      case ExtendedExpressionType.Projection:
        return VisitProjectionExpression((ProjectionExpression) expression);
      case ExtendedExpressionType.Field:
        return VisitFieldExpression((FieldExpression) expression);
      case ExtendedExpressionType.Structure:
        return VisitStructureExpression((StructureExpression) expression);
      case ExtendedExpressionType.Key:
        return VisitKeyExpression((KeyExpression) expression);
      case ExtendedExpressionType.Entity:
        return VisitEntityExpression((EntityExpression) expression);
      case ExtendedExpressionType.EntityField:
        return VisitEntityFieldExpression((EntityFieldExpression) expression);
      case ExtendedExpressionType.EntitySet:
        return VisitEntitySetExpression((EntitySetExpression) expression);
      case ExtendedExpressionType.ItemProjector:
        return VisitItemProjectorExpression((ItemProjectorExpression) expression);
      case ExtendedExpressionType.Column:
        return VisitColumnExpression((ColumnExpression) expression);
      case ExtendedExpressionType.SubQuery:
        return VisitSubQueryExpression((SubQueryExpression) expression);
        case ExtendedExpressionType.Grouping:
        return VisitGroupingExpression((GroupingExpression) expression);
      default:
        throw new ArgumentOutOfRangeException("expression");
      }
    }

    protected abstract Expression VisitGroupingExpression(GroupingExpression expression);

    protected abstract Expression VisitSubQueryExpression(SubQueryExpression expression);

    protected abstract Expression VisitProjectionExpression(ProjectionExpression projectionExpression);

    protected abstract Expression VisitFieldExpression(FieldExpression expression);

    protected abstract Expression VisitStructureExpression(StructureExpression expression);

    protected abstract Expression VisitKeyExpression(KeyExpression expression);

    protected abstract Expression VisitEntityExpression(EntityExpression expression);

    protected abstract Expression VisitEntityFieldExpression(EntityFieldExpression expression);

    protected abstract Expression VisitEntitySetExpression(EntitySetExpression expression);

    protected abstract Expression VisitItemProjectorExpression(ItemProjectorExpression itemProjectorExpression);

    protected abstract Expression VisitColumnExpression(ColumnExpression expression);
  }
}