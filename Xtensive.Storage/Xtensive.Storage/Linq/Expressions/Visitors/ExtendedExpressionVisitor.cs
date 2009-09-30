// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.06

using System;
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
        throw new NotSupportedException(string.Format(Resources.Strings.ExpressionXIsUnknown, expression));
      switch (extendedExpression.ExtendedType) {
      case ExtendedExpressionType.Projection:
        return VisitProjectionExpression((ProjectionExpression) expression);
      case ExtendedExpressionType.Field:
        return VisitFieldExpression((FieldExpression) expression);
      case ExtendedExpressionType.StructureField:
        return VisitStructureExpression((StructureFieldExpression) expression);
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
      case ExtendedExpressionType.Marker:
        return VisitMarker((MarkerExpression) expression);
      case ExtendedExpressionType.SubQuery:
        return VisitSubQueryExpression((SubQueryExpression) expression);
      case ExtendedExpressionType.Grouping:
        return VisitGroupingExpression((GroupingExpression) expression);
      case ExtendedExpressionType.LocalCollection:
        return VisitLocalCollectionExpression((LocalCollectionExpression) expression);
      case ExtendedExpressionType.Structure:
        return VisitLocalCollectionStructureExpression((StructureExpression) expression);
      default:
        return base.VisitUnknown(expression);
      }
    }

    protected virtual Expression VisitLocalCollectionStructureExpression(StructureExpression expression)
    {
      return expression;
    }

    protected virtual Expression VisitLocalCollectionExpression(LocalCollectionExpression expression)
    {
      return expression;
    }

    protected virtual Expression VisitMarker(MarkerExpression expression)
    {
      var processedTarget = Visit(expression.Target);
      if (processedTarget==expression.Target)
        return expression;
      return new MarkerExpression(processedTarget, expression.MarkerType);
    }

    protected virtual Expression VisitGroupingExpression(GroupingExpression expression)
    {
      return expression;
    }

    protected virtual Expression VisitSubQueryExpression(SubQueryExpression expression)
    {
      return expression;
    }

    protected virtual Expression VisitProjectionExpression(ProjectionExpression projectionExpression)
    {
      return projectionExpression;
    }

    protected virtual Expression VisitFieldExpression(FieldExpression expression)
    {
      return expression;
    }

    protected virtual Expression VisitStructureExpression(StructureFieldExpression expression)
    {
      return expression;
    }

    protected virtual Expression VisitKeyExpression(KeyExpression expression)
    {
      return expression;
    }

    protected virtual Expression VisitEntityExpression(EntityExpression expression)
    {
      return expression;
    }

    protected virtual Expression VisitEntityFieldExpression(EntityFieldExpression expression)
    {
      return expression;
    }

    protected virtual Expression VisitEntitySetExpression(EntitySetExpression expression)
    {
      return expression;
    }

    protected virtual Expression VisitItemProjectorExpression(ItemProjectorExpression itemProjectorExpression)
    {
      return itemProjectorExpression;
    }

    protected virtual Expression VisitColumnExpression(ColumnExpression expression)
    {
      return expression;
    }
  }
}