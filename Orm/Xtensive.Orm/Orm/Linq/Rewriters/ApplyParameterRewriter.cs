// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2009.06.09

using System.Linq.Expressions;
using Xtensive.Orm.Linq.Expressions;
using Xtensive.Orm.Linq.Expressions.Visitors;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Linq.Rewriters
{
  internal class ApplyParameterRewriter : ExtendedExpressionVisitor
  {
    private readonly Expression newApplyParameterValueExpression;
    private readonly ApplyParameter oldApplyParameter;
    private readonly ApplyParameter newApplyParameter;

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
      var projectionExpression = expression.ProjectionExpression;
      var newProvider = Rewrite(projectionExpression.ItemProjector.DataSource, oldApplyParameter, newApplyParameter);
      var newItemProjectorBody = Visit(projectionExpression.ItemProjector.Item);
      var newKeyExpression = Visit(expression.KeyExpression);
      if (newProvider != projectionExpression.ItemProjector.DataSource
        || newItemProjectorBody != projectionExpression.ItemProjector.Item
          || newKeyExpression != expression.KeyExpression) {
        var newItemProjector = new ItemProjectorExpression(newItemProjectorBody, newProvider, projectionExpression.ItemProjector.Context);
        var newProjectionExpression = projectionExpression.ApplyItemProjector(newItemProjector);
        return new GroupingExpression(
          expression.Type, expression.OuterParameter, expression.DefaultIfEmpty, newProjectionExpression,
          expression.ApplyParameter, expression.KeyExpression, expression.SelectManyInfo);
      }
      return expression;
    }

    protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
    {
      var projectionExpression = expression.ProjectionExpression;
      var newProvider = Rewrite(projectionExpression.ItemProjector.DataSource, oldApplyParameter, newApplyParameter);
      var newItemProjectorBody = Visit(projectionExpression.ItemProjector.Item);
      if (newProvider != projectionExpression.ItemProjector.DataSource
        || newItemProjectorBody != projectionExpression.ItemProjector.Item) {
        var newItemProjector = new ItemProjectorExpression(
          newItemProjectorBody, newProvider, projectionExpression.ItemProjector.Context);
        var newProjectionExpression = projectionExpression.ApplyItemProjector(newItemProjector);
        return new SubQueryExpression(
          expression.Type, expression.OuterParameter, expression.DefaultIfEmpty, newProjectionExpression,
          expression.ApplyParameter, expression.ExtendedType);
      }
      return expression;
    }

    private Expression RewriteExpression(Provider provider, Expression expression)
    {
      return Visit(expression);
    }

    public static CompilableProvider Rewrite(CompilableProvider provider, ApplyParameter oldParameter, ApplyParameter newParameter)
    {
      var expressionRewriter = new ApplyParameterRewriter(oldParameter, newParameter);
      var providerRewriter = new CompilableProviderVisitor(expressionRewriter.RewriteExpression);
      return providerRewriter.VisitCompilable(provider);
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