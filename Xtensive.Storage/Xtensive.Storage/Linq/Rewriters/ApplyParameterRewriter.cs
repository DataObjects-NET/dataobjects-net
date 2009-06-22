// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.06.09

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Expressions.Visitors;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Core.Linq;

namespace Xtensive.Storage.Linq.Rewriters
{
  [Serializable]
  internal class ApplyParameterRewriter: ExtendedExpressionVisitor
  {
    private readonly Expression newApplyParameterValueExpression;
    private readonly ApplyParameter oldApplyParameter;
    private readonly ApplyParameter newApplyParameter;

    public static CompilableProvider Rewrite(CompilableProvider provider,
      ApplyParameter oldParameter, ApplyParameter newParameter)
    {
      var expressionRewriter = new ApplyParameterRewriter(oldParameter, newParameter);
      var providerRewriter = new CompilableProviderVisitor(expressionRewriter.RewriteExpression);
      return providerRewriter.VisitCompilable(provider);
    }

    public static Expression Rewrite(Expression expression,
      ApplyParameter oldParameter, ApplyParameter newParameter)
    {
      var expressionRewriter = new ApplyParameterRewriter(oldParameter, newParameter);
      return expressionRewriter.Visit(expression);
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      if (m.Member!=WellKnownMembers.ApplyParameterValue)
        return base.VisitMemberAccess(m);
      if (m.Expression.NodeType!=ExpressionType.Constant)
        return base.VisitMemberAccess(m);
      var parameter = ((ConstantExpression) m.Expression).Value;
      if (parameter!=oldApplyParameter)
        return base.VisitMemberAccess(m);
      return newApplyParameterValueExpression;
    }


    protected override Expression VisitGroupingExpression(GroupingExpression expression)
    {
      var newProvider = Rewrite(expression.ProjectionExpression.ItemProjector.DataSource.Provider, oldApplyParameter, newApplyParameter);
      var newItemProjectorBody = Visit(expression.ProjectionExpression.ItemProjector.Item);
      var newKeyExpression = Visit(expression.KeyExpression);
      if (newProvider != expression.ProjectionExpression.ItemProjector.DataSource.Provider 
        || newItemProjectorBody!=expression.ProjectionExpression.ItemProjector.Item
        || newKeyExpression!=expression.KeyExpression
        ) {
        var newItemProjector = new ItemProjectorExpression(newItemProjectorBody, newProvider.Result, expression.ProjectionExpression.ItemProjector.Context);
        var newProjectionExpression = new ProjectionExpression(expression.ProjectionExpression.Type, newItemProjector, expression.ProjectionExpression.ResultType);
        return new GroupingExpression(expression.Type, expression.OuterParameter, expression.DefaultIfEmpty, newProjectionExpression, expression.ApplyParameter, expression.KeyExpression, expression.Mapping);
      }
      return expression;
    }

    protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
    {
      var newProvider = Rewrite(expression.ProjectionExpression.ItemProjector.DataSource.Provider, oldApplyParameter, newApplyParameter);
      var newItemProjectorBody = Visit(expression.ProjectionExpression.ItemProjector.Item);
      if (newProvider != expression.ProjectionExpression.ItemProjector.DataSource.Provider || newItemProjectorBody!=expression.ProjectionExpression.ItemProjector.Item) {
        var newItemProjector = new ItemProjectorExpression(newItemProjectorBody, newProvider.Result, expression.ProjectionExpression.ItemProjector.Context);
        var newProjectionExpression = new ProjectionExpression(expression.ProjectionExpression.Type, newItemProjector, expression.ProjectionExpression.ResultType);
        return new SubQueryExpression(expression.Type, expression.OuterParameter, expression.DefaultIfEmpty, newProjectionExpression, expression.ApplyParameter, expression.ExtendedType);
      }
      return expression;
    }

    private Expression RewriteExpression(Provider provider, Expression expression)
    {
      return Visit(expression);
    }


    // Constructors

    private ApplyParameterRewriter(ApplyParameter oldParameter, ApplyParameter newParameter)
    {
      newApplyParameterValueExpression = Expression.Property(Expression.Constant(newParameter), WellKnownMembers.ApplyParameterValue);
      oldApplyParameter = oldParameter;
      newApplyParameter = newParameter;
    }
  }
}