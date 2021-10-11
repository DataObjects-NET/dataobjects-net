// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2009.05.19

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Orm.Linq.Expressions.Visitors;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Collections;

namespace Xtensive.Orm.Linq.Expressions
{
  [Serializable]
  internal class SubQueryExpression : ParameterizedExpression,
    IMappedExpression
  {
    public ProjectionExpression ProjectionExpression { get; private set; }

    public ApplyParameter ApplyParameter { get; private set; }

    public virtual Expression BindParameter(ParameterExpression parameter, Dictionary<Expression, Expression> processedExpressions)
    {
      return this;
    }

    public virtual Expression RemoveOuterParameter(Dictionary<Expression, Expression> processedExpressions)
    {
      return this;
    }

    public virtual Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      // Don't check CanRemap - Remap always.

      // Save Subquery parameter
      var remapContext = RemapScope.CurrentContext;
      bool isTopSubquery = remapContext.SubqueryParameterExpression==null;
      if (isTopSubquery)
        remapContext.SubqueryParameterExpression = OuterParameter;

      // Remap recordset parameters
      var visitor = new ApplyParameterAccessVisitor(ApplyParameter, (mc, index) => Call(mc.Object, mc.Method, Constant(index + offset)));
      var providerVisitor = new CompilableProviderVisitor((provider, expression) => visitor.Visit(expression));
      var newDataSource = providerVisitor.VisitCompilable(ProjectionExpression.ItemProjector.DataSource);

      // Remap Field parametrized parameters
      var item = GenericExpressionVisitor<IMappedExpression>.Process(ProjectionExpression.ItemProjector.Item, mapped => {
        var parametrizedExpression = mapped as ParameterizedExpression;
        if (parametrizedExpression!=null && parametrizedExpression.OuterParameter==OuterParameter)
          return mapped.Remap(offset, new Dictionary<Expression, Expression>());
        return (Expression) mapped;
      });
      var newItemProjector = new ItemProjectorExpression(item, newDataSource, ProjectionExpression.ItemProjector.Context);
      var newProjectionExpression = new ProjectionExpression(ProjectionExpression.Type, newItemProjector, ProjectionExpression.TupleParameterBindings);
      var result = new SubQueryExpression(Type, OuterParameter, DefaultIfEmpty, newProjectionExpression, ApplyParameter, ExtendedType);
      processedExpressions.Add(this, result);

      // Restore subquery parameter
      if (isTopSubquery)
        remapContext.SubqueryParameterExpression = null;

      return result;
    }

    public virtual Expression Remap(IReadOnlyList<int> map, Dictionary<Expression, Expression> processedExpressions)
    {
      // Don't check CanRemap - Remap always.

      // Save Subquery parameter
      var remapContext = RemapScope.CurrentContext;
      bool isTopSubquery = remapContext.SubqueryParameterExpression==null;
      if (isTopSubquery)
        remapContext.SubqueryParameterExpression = OuterParameter;

      // Remap recordset parameters
      var visitor = new ApplyParameterAccessVisitor(ApplyParameter, (mc, index) => Call(mc.Object, mc.Method, Constant(map.IndexOf(index))));
      var providerVisitor = new CompilableProviderVisitor((provider, expression) => visitor.Visit(expression));
      var newDataSource = providerVisitor.VisitCompilable(ProjectionExpression.ItemProjector.DataSource);

      // Remap Field parametrized parameters
      var item = GenericExpressionVisitor<IMappedExpression>.Process(ProjectionExpression.ItemProjector.Item, mapped => {
        var parametrizedExpression = mapped as ParameterizedExpression;
        if (parametrizedExpression!=null && parametrizedExpression.OuterParameter==OuterParameter)
          return mapped.Remap(map, new Dictionary<Expression, Expression>());
        return (Expression) mapped;
      });
      var newItemProjector = new ItemProjectorExpression(item, newDataSource, ProjectionExpression.ItemProjector.Context);
      var newProjectionExpression = new ProjectionExpression(ProjectionExpression.Type, newItemProjector, ProjectionExpression.TupleParameterBindings);
      var result = new SubQueryExpression(Type, OuterParameter, DefaultIfEmpty, newProjectionExpression, ApplyParameter, ExtendedType);
      processedExpressions.Add(this, result);

      // Restore subquery parameter
      if (isTopSubquery)
        remapContext.SubqueryParameterExpression = null;

      return result;
    }

    // Constructors

    public virtual Expression ReplaceApplyParameter(ApplyParameter newApplyParameter)
    {
      if (newApplyParameter==ApplyParameter)
        return new SubQueryExpression(Type, OuterParameter, DefaultIfEmpty, ProjectionExpression, ApplyParameter);
      
      var newItemProjector = ProjectionExpression.ItemProjector.RewriteApplyParameter(ApplyParameter, newApplyParameter);
      var newProjectionExpression = new ProjectionExpression(
        ProjectionExpression.Type, 
        newItemProjector, 
        ProjectionExpression.TupleParameterBindings, 
        ProjectionExpression.ResultAccessMethod);
      return new SubQueryExpression(Type, OuterParameter, DefaultIfEmpty, newProjectionExpression, newApplyParameter);
    }

    public SubQueryExpression(
      Type type,
      ParameterExpression parameterExpression,
      bool defaultIfEmpty,
      ProjectionExpression projectionExpression,
      ApplyParameter applyParameter)
      : base(ExtendedExpressionType.SubQuery, type, parameterExpression, defaultIfEmpty)
    {
      ProjectionExpression = projectionExpression;
      ApplyParameter = applyParameter;
    }

    public SubQueryExpression(Type type, ParameterExpression parameterExpression, bool defaultIfEmpty, ProjectionExpression projectionExpression, ApplyParameter applyParameter, ExtendedExpressionType expressionType)
      : base(expressionType, type, parameterExpression, defaultIfEmpty)
    {
      ProjectionExpression = projectionExpression;
      ApplyParameter = applyParameter;
    }
  }
}