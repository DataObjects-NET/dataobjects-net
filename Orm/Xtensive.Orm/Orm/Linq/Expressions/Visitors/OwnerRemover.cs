// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.26

using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Orm.Linq.Expressions.Visitors
{
  internal sealed class OwnerRemover : PersistentExpressionVisitor
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

    protected override Expression VisitConstructorExpression(ConstructorExpression expression)
    {
      var oldConstructorArguments = expression.ConstructorArguments.ToList().AsReadOnly();
      var newConstructorArguments = VisitExpressionList(oldConstructorArguments);

      var oldBindings = expression.Bindings.Select(b => b.Value).ToList().AsReadOnly();
      var newBindings = VisitExpressionList(oldBindings);

      var oldNativeBindings = expression.NativeBindings.Select(b => b.Value).ToList().AsReadOnly();
      var newNativeBindings = VisitExpressionList(oldNativeBindings);
      
      var notChanged =
        ReferenceEquals(oldConstructorArguments, newConstructorArguments)
        && ReferenceEquals(oldBindings, newBindings)
        && ReferenceEquals(oldNativeBindings, newNativeBindings);

      if (notChanged)
        return expression;

      var bindings = expression.Bindings
        .Zip(newBindings, (first, second) => (first, second))
        .ToDictionary(item => item.first.Key, item => item.second);
      var nativeBingings = expression.NativeBindings
        .Zip(newNativeBindings, (first, second) => (first, second))
        .ToDictionary(item => item.first.Key, item => item.second);
      return new ConstructorExpression(expression.Type, bindings, nativeBingings, expression.Constructor, newConstructorArguments);
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