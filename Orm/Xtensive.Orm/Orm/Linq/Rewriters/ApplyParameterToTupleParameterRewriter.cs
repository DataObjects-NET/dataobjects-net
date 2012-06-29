// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.24

using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Orm.Linq.Expressions;
using Xtensive.Orm.Linq.Expressions.Visitors;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Linq.Rewriters
{
  internal class ApplyParameterToTupleParameterRewriter : ExtendedExpressionVisitor
  {
    private readonly Expression parameterOfTupleExpression;
    private readonly ApplyParameter applyParameter;
    private readonly Parameter<Tuple> parameterOfTuple;

    public static Expression Rewrite(Expression expression,
      Parameter<Tuple> parameterOfTuple, ApplyParameter applyParameter)
    {
      var expressionRewriter = new ApplyParameterToTupleParameterRewriter(parameterOfTuple, applyParameter);
      return expressionRewriter.Visit(expression);
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      if (m.Member!=WellKnownMembers.ApplyParameterValue)
        return base.VisitMemberAccess(m);
      if (m.Expression.NodeType!=ExpressionType.Constant)
        return base.VisitMemberAccess(m);
      var parameter = ((ConstantExpression) m.Expression).Value;
      if (parameter!=applyParameter)
        return base.VisitMemberAccess(m);
      return parameterOfTupleExpression;
    }

    protected override Expression VisitGroupingExpression(GroupingExpression expression)
    {
      var newProvider = Rewrite(expression.ProjectionExpression.ItemProjector.DataSource, parameterOfTuple, applyParameter);
      var newItemProjectorBody = Visit(expression.ProjectionExpression.ItemProjector.Item);
      var newKeyExpression = Visit(expression.KeyExpression);
      if (newProvider!=expression.ProjectionExpression.ItemProjector.DataSource
        || newItemProjectorBody!=expression.ProjectionExpression.ItemProjector.Item
          || newKeyExpression!=expression.KeyExpression) {
        var newItemProjector = new ItemProjectorExpression(newItemProjectorBody, newProvider, expression.ProjectionExpression.ItemProjector.Context);
        var newProjectionExpression = new ProjectionExpression(
          expression.ProjectionExpression.Type, 
          newItemProjector, 
          expression.ProjectionExpression.TupleParameterBindings, 
          expression.ProjectionExpression.ResultType);
        return new GroupingExpression(
          expression.Type, expression.OuterParameter, expression.DefaultIfEmpty,
          newProjectionExpression, expression.ApplyParameter,
          expression.KeyExpression, expression.SelectManyInfo);
      }
      return expression;
    }

    protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
    {
      var newProvider = Rewrite(expression.ProjectionExpression.ItemProjector.DataSource, parameterOfTuple, applyParameter);
      var newItemProjectorBody = Visit(expression.ProjectionExpression.ItemProjector.Item);
      if (newProvider!=expression.ProjectionExpression.ItemProjector.DataSource || newItemProjectorBody!=expression.ProjectionExpression.ItemProjector.Item) {
        var newItemProjector = new ItemProjectorExpression(newItemProjectorBody, newProvider, expression.ProjectionExpression.ItemProjector.Context);
        var newProjectionExpression = new ProjectionExpression(
          expression.ProjectionExpression.Type, 
          newItemProjector, 
          expression.ProjectionExpression.TupleParameterBindings, 
          expression.ProjectionExpression.ResultType);
        return new SubQueryExpression(
          expression.Type, expression.OuterParameter,
          expression.DefaultIfEmpty, newProjectionExpression,
          expression.ApplyParameter, expression.ExtendedType);
      }
      return expression;
    }

    private Expression RewriteExpression(Provider provider, Expression expression)
    {
      return Visit(expression);
    }

    public static CompilableProvider Rewrite(CompilableProvider provider, Parameter<Tuple> parameterOfTuple, ApplyParameter applyParameter)
    {
      var expressionRewriter = new ApplyParameterToTupleParameterRewriter(parameterOfTuple, applyParameter);
      var providerRewriter = new CompilableProviderVisitor(expressionRewriter.RewriteExpression);
      return providerRewriter.VisitCompilable(provider);
    }

    // Constructors

    private ApplyParameterToTupleParameterRewriter(Parameter<Tuple> parameterOfTuple, ApplyParameter applyParameter)
    {
      this.parameterOfTuple = parameterOfTuple;
      parameterOfTupleExpression = Expression.Property(Expression.Constant(parameterOfTuple), WellKnownMembers.ParameterOfTupleValue);
      this.applyParameter = applyParameter;
    }
  }
}