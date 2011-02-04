// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.02

using System;
using System.Diagnostics;

namespace Xtensive.Orm.Linq.Expressions.Visitors
{
  internal class EntityExpressionJoiner : ExtendedExpressionVisitor
  {
    private readonly Translator translator;
    private readonly ItemProjectorExpression itemProjectorExpression;

    protected override System.Linq.Expressions.Expression VisitEntityExpression(EntityExpression expression)
    {
      translator.EnsureEntityFieldsAreJoined(expression, itemProjectorExpression);
      return base.VisitEntityExpression(expression);
    }

    public static ItemProjectorExpression JoinEntities(Translator translator, ItemProjectorExpression itemProjectorExpression)
    {
      var item = new EntityExpressionJoiner(translator, itemProjectorExpression).Visit(itemProjectorExpression.Item);
      return new ItemProjectorExpression(item, itemProjectorExpression.DataSource, itemProjectorExpression.Context);
    }

    private EntityExpressionJoiner(Translator translator, ItemProjectorExpression itemProjectorExpression)
    {
      this.translator = translator;
      this.itemProjectorExpression = itemProjectorExpression;
    }
  }
}