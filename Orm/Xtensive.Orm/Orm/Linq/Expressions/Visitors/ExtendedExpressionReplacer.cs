// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.27

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Linq.Expressions.Visitors
{
  internal sealed class ExtendedExpressionReplacer : PersistentExpressionVisitor
  {
    private readonly Func<Expression, Expression> replaceDelegate;
    private readonly CompilableProviderVisitor providerVisitor;

    public Expression Replace(Expression e)
    {
      return Visit(e);
    }

    protected override Expression Visit(Expression e)
    {
      if (e==null)
        return null;
      var result = replaceDelegate(e);
      return result ?? base.Visit(e);
    }

    protected override Expression VisitProjectionExpression(ProjectionExpression projectionExpression)
    {
      var item = Visit(projectionExpression.ItemProjector.Item);
      var provider = providerVisitor.VisitCompilable(projectionExpression.ItemProjector.DataSource);
      var providerChanged = provider!=projectionExpression.ItemProjector.DataSource;
      var itemChanged = item!=projectionExpression.ItemProjector.Item;
      if (providerChanged || itemChanged) {
        var itemProjector = new ItemProjectorExpression(item, provider, projectionExpression.ItemProjector.Context);
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
      if (keyExpression!=expression.KeyExpression)
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

    protected override Expression VisitFullTextExpression(FullTextExpression expression)
    {
      var rankExpression = (ColumnExpression) Visit(expression.RankExpression);
      var entityExpression = (EntityExpression) Visit(expression.EntityExpression);
      if (rankExpression!=expression.RankExpression || entityExpression!=expression.EntityExpression)
        return new FullTextExpression(expression.FullTextIndex, entityExpression, rankExpression, expression.OuterParameter);
      return expression;
    }

    protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
    {
      return expression;
    }

    private Expression TranslateExpression(Provider provider, Expression original)
    {
      var result = Visit(original);
      return result ?? original;
    }

    protected override Expression VisitFieldExpression(FieldExpression expression)
    {
      return expression;
    }

    protected override Expression VisitStructureFieldExpression(StructureFieldExpression expression)
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

    protected override Expression VisitConstructorExpression(ConstructorExpression expression)
    {
      var arguments = new List<Expression>();
      var bindings = new Dictionary<MemberInfo, Expression>();
      var nativeBindings = new Dictionary<MemberInfo, Expression>();
      bool recreate = false;
      foreach (var argument in expression.ConstructorArguments) {
        var result = Visit(argument);
        if (result != argument)
          recreate = true;
        arguments.Add(result);
      }
      foreach (var binding in expression.Bindings) {
        var result = Visit(binding.Value);
        if (result != binding.Value)
          recreate = true;
        bindings.Add(binding.Key, result);
      }
      foreach (var nativeBinding in expression.NativeBindings) {
        var result = Visit(nativeBinding.Value);
        if (result!=nativeBinding.Value)
          recreate = true;
        nativeBindings.Add(nativeBinding.Key, result);
      }
      if (!recreate)
        return expression;
      return new ConstructorExpression(
        expression.Type,
        bindings,
        nativeBindings,
        expression.Constructor,
        arguments);
    }

    protected override Expression VisitMarker(MarkerExpression expression)
    {
      var target = Visit(expression.Target);
      return target == expression.Target 
        ? expression 
        : new MarkerExpression(target, expression.MarkerType);
    }

    // Constructors

    public ExtendedExpressionReplacer(Func<Expression, Expression> replaceDelegate)
    {
      this.replaceDelegate = replaceDelegate;
      providerVisitor = new CompilableProviderVisitor(TranslateExpression);
    }
  }
}