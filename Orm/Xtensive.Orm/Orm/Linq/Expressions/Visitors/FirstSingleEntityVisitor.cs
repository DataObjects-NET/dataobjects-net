// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.07.03

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Linq.Expressions.Visitors
{
  internal class FirstSingleEntityVisitor : PersistentExpressionVisitor
  {
    private readonly List<int> columns;
    private readonly Dictionary<TypeInfo, EntityInfo> entities;

    protected override Expression VisitFieldExpression(FieldExpression expression)
    {
      columns.Add(expression.Mapping.Offset);
      return expression.RemoveOwner();
    }

    protected override Expression VisitStructureFieldExpression(StructureFieldExpression expression)
    {
      var hasNoOwner = expression.Owner == null;
      if (hasNoOwner || expression.Mapping.Length <= 2) {
        columns.AddRange(expression.Mapping.GetItems());
        return hasNoOwner
          ? expression 
          : expression.RemoveOwner();
      }

      var entity = expression.Owner as EntityExpression;
      var structure = expression.Owner as StructureFieldExpression;
      while (entity == null && structure != null) {
        entity = structure.Owner as EntityExpression;
        structure = structure.Owner as StructureFieldExpression;
      }
      if (entity == null)
        throw new InvalidOperationException(String.Format(Strings.ExUnableToResolveOwnerOfStructureExpressionX, expression));

      EntityInfo entityInfo;
      if (entities.TryGetValue(entity.PersistentType, out entityInfo))
        entityInfo.Columns.AddRange(expression.Mapping.GetItems());
      else {
        entityInfo = new EntityInfo(
          entity.PersistentType, 
          entity.Key.Mapping.GetItems().ToArray(), 
          expression.Mapping.GetItems().ToList());
        entities.Add(entity.PersistentType, entityInfo);
      }
      return expression;
    }

    protected override Expression VisitKeyExpression(KeyExpression expression)
    {
      columns.AddRange(expression.Mapping.GetItems());
      return expression;
    }

    protected override Expression VisitEntityExpression(EntityExpression expression)
    {
      throw new NotImplementedException();
    }

    protected override Expression VisitEntityFieldExpression(EntityFieldExpression expression)
    {
      columns.AddRange(expression.Mapping.GetItems());
      return expression;
    }

    protected override Expression VisitEntitySetExpression(EntitySetExpression expression)
    {
      columns.AddRange(expression.Owner.Mapping.GetItems());
      return expression;
    }

    protected override Expression VisitColumnExpression(ColumnExpression expression)
    {
      columns.Add(expression.Mapping.Offset);
      return expression;
    }


    // Constructors

    public FirstSingleEntityVisitor()
    {
      columns = new List<int>();
      entities = new Dictionary<TypeInfo, EntityInfo>();
    }
  }
}