// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.27

using System;
using System.Linq.Expressions;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Linq.Expressions.Visitors
{
  internal class ExtendedExpressionReplacer : PersistentExpressionVisitor
  {
    private readonly Func<Expression, Expression> replaceDelegate;
    private readonly CompilableProviderVisitor providerVisitor;

    public Expression Replace(Expression e)
    {
      return Visit(e);
    }

    protected override Expression Visit(Expression e)
    {
      if (e == null)
        return null;
      var result = replaceDelegate(e);
      return result ?? base.Visit(e);
    }

    protected override Expression VisitProjectionExpression(ProjectionExpression projectionExpression)
    {
        var item = Visit(projectionExpression.ItemProjector.Item);
        var provider = providerVisitor.VisitCompilable(projectionExpression.ItemProjector.DataSource.Provider);
        var providerChanged = provider != projectionExpression.ItemProjector.DataSource.Provider;
        var itemChanged = item != projectionExpression.ItemProjector.Item;
        if (providerChanged || itemChanged) {
          var itemProjector = new ItemProjectorExpression(item, provider.Result, projectionExpression.ItemProjector.Context);
          return new ProjectionExpression(
            projectionExpression.Type, 
            itemProjector, 
            projectionExpression.TupleParameterBindings, 
            projectionExpression.ResultType);
        }
        return projectionExpression;
    }

    protected override Expression VisitGroupingExpression(GroupingExpression expression)
    {
      var keyExpression = Visit(expression.KeyExpression);
      if (keyExpression != expression.KeyExpression)
        return new GroupingExpression(
          expression.Type,
          expression.OuterParameter,
          expression.DefaultIfEmpty,
          expression.ProjectionExpression,
          expression.ApplyParameter,
          keyExpression,
          expression.SelectManyInfo);
      return expression;
    }

    protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
    {
      return expression;
    }

    private Expression ExpressionTranslator(Provider provider, Expression original)
    {
      var result = Visit(original);
      return result ?? original;
    }

    protected override Expression VisitFieldExpression(FieldExpression expression)
    {
      return expression;
    }

    protected override Expression VisitStructureExpression(StructureExpression expression)
    {
      return expression;
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
      return expression;
    }

    protected override Expression VisitEntitySetExpression(EntitySetExpression expression)
    {
      return expression;
    }

    protected override Expression VisitColumnExpression(ColumnExpression expression)
    {
      return expression;
    }

    // Constructors

    public ExtendedExpressionReplacer(Func<Expression,Expression> replaceDelegate)
    {
      this.replaceDelegate = replaceDelegate;
      providerVisitor = new CompilableProviderVisitor(ExpressionTranslator);
    }
  }
}