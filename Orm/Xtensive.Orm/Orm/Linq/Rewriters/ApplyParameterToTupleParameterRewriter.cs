// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
      var projectionExpression = expression.ProjectionExpression;
      var newProvider = Rewrite(projectionExpression.ItemProjector.DataSource, parameterOfTuple, applyParameter);
      var newItemProjectorBody = Visit(projectionExpression.ItemProjector.Item);
      var newKeyExpression = Visit(expression.KeyExpression);
      if (newProvider != projectionExpression.ItemProjector.DataSource
        || newItemProjectorBody != projectionExpression.ItemProjector.Item
          || newKeyExpression != expression.KeyExpression) {
        var newItemProjector = new ItemProjectorExpression(newItemProjectorBody, newProvider, projectionExpression.ItemProjector.Context);
        var newProjectionExpression = projectionExpression.Apply(newItemProjector);
        return new GroupingExpression(
          expression.Type, expression.OuterParameter, expression.DefaultIfEmpty,
          newProjectionExpression, expression.ApplyParameter,
          expression.KeyExpression, expression.SelectManyInfo);
      }
      return expression;
    }

    protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
    {
      var projectionExpression = expression.ProjectionExpression;
      var newProvider = Rewrite(projectionExpression.ItemProjector.DataSource, parameterOfTuple, applyParameter);
      var newItemProjectorBody = Visit(projectionExpression.ItemProjector.Item);
      if (newProvider != projectionExpression.ItemProjector.DataSource || newItemProjectorBody != projectionExpression.ItemProjector.Item) {
        var newItemProjector = new ItemProjectorExpression(newItemProjectorBody, newProvider, projectionExpression.ItemProjector.Context);
        var newProjectionExpression = projectionExpression.Apply(newItemProjector);
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