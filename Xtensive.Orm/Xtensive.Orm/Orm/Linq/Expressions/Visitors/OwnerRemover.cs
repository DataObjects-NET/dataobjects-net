// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.26

using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Xtensive.Orm.Linq.Expressions.Visitors
{
  internal class OwnerRemover : PersistentExpressionVisitor
  {
    public static Expression RemoveOwner(Expression target)
    {
      var remover = new OwnerRemover();
      return remover.Visit(target);
    }

    protected override Expression VisitGroupingExpression(GroupingExpression expression)
    {
      return expression;
    }

    protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
    {
      return expression;
    }

    protected override Expression VisitFieldExpression(FieldExpression expression)
    {
      return expression.RemoveOwner();
    }

    protected override Expression VisitStructureFieldExpression(StructureFieldExpression expression)
    {
      return expression.RemoveOwner();
    }

    protected override Expression VisitKeyExpression(KeyExpression expression)
    {
      return expression;
    }

    protected override Expression VisitEntityExpression(EntityExpression expression)
    {
      return expression;
    }

    protected override Expression VisitEntityFieldExpression(EntityFieldExpression expression)
    {
      return expression.RemoveOwner();
    }

    protected override Expression VisitEntitySetExpression(EntitySetExpression expression)
    {
      return expression;
    }

    protected override Expression VisitColumnExpression(ColumnExpression expression)
    {
      return expression;
    }
  }
}