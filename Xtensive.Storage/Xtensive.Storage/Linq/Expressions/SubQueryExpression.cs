// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.05.19

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Storage.Linq.Expressions.Visitors;
using Xtensive.Storage.Linq.Rewriters;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Linq.Expressions
{
  [Serializable]
  internal class SubQueryExpression : ParameterizedExpression,
    IMappedExpression
  {
    private readonly ApplyParameter applyParameter;

    public ProjectionExpression ProjectionExpression { get; private set; }

    public ApplyParameter ApplyParameter
    {
      get { return applyParameter; }
    }

    /// <exception cref="NotSupportedException"><c>NotSupportedException</c>.</exception>
    public virtual Segment<int> Mapping
    {
      get { throw new NotSupportedException(); }
    }

    public Expression BindParameter(ParameterExpression parameter, Dictionary<Expression, Expression> processedExpressions)
    {
      // TODO: Aleksey Kochetov, fix it!
      return this;
      // throw new NotSupportedException();
    }

    public Expression RemoveOuterParameter(Dictionary<Expression, Expression> processedExpressions)
    {
      // TODO: Aleksey Kochetov, fix it!
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
      var visitor = new ApplyParameterAccessVisitor(ApplyParameter, (mc, index) => Call(mc.Object, mc.Method, Constant(index+offset)));
      var providerVisitor = new CompilableProviderVisitor((provider, expression) => visitor.Visit(expression));
      var newDataSource = providerVisitor.VisitCompilable(ProjectionExpression.ItemProjector.DataSource.Provider).Result;

      // Remap Field parametrized parameters
      var item = GenericExpressionVisitor<IMappedExpression>.Process(ProjectionExpression.ItemProjector.Item, mapped => {
        var parametrizedExpression = mapped as ParameterizedExpression;
        if (parametrizedExpression!=null && parametrizedExpression.OuterParameter==OuterParameter)
          return mapped.Remap(offset, new Dictionary<Expression, Expression>());
        return (Expression) mapped;
      });
      var newItemProjector = new ItemProjectorExpression(item, newDataSource, ProjectionExpression.ItemProjector.Context);
      var newProjectionExpression = new ProjectionExpression(ProjectionExpression.Type, newItemProjector, ProjectionExpression.TupleParameterBindings);
      var result = new SubQueryExpression(Type, OuterParameter, newProjectionExpression, ApplyParameter, ExtendedType, DefaultIfEmpty);
      processedExpressions.Add(this, result);

      // Restore subquery parameter
      if (isTopSubquery)
        remapContext.SubqueryParameterExpression = null;

      return result ;
    }

    public virtual Expression Remap(int[] map, Dictionary<Expression, Expression> processedExpressions)
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
      var newDataSource = providerVisitor.VisitCompilable(ProjectionExpression.ItemProjector.DataSource.Provider).Result;

      // Remap Field parametrized parameters
      var item = GenericExpressionVisitor<IMappedExpression>.Process(ProjectionExpression.ItemProjector.Item, mapped => {
        var parametrizedExpression = mapped as ParameterizedExpression;
        if (parametrizedExpression!=null && parametrizedExpression.OuterParameter==OuterParameter)
          return mapped.Remap(map, new Dictionary<Expression, Expression>());
        return (Expression) mapped;
      });
      var newItemProjector = new ItemProjectorExpression(item, newDataSource, ProjectionExpression.ItemProjector.Context);
      var newProjectionExpression = new ProjectionExpression(ProjectionExpression.Type, newItemProjector, ProjectionExpression.TupleParameterBindings);
      var result = new SubQueryExpression(Type, OuterParameter, newProjectionExpression, ApplyParameter, ExtendedType, DefaultIfEmpty);
      processedExpressions.Add(this, result);
      
      // Restore subquery parameter
      if (isTopSubquery)
        remapContext.SubqueryParameterExpression = null;

      return result ;
    }

    // Constructors

    public SubQueryExpression(Type type,
      ParameterExpression parameterExpression,
      ProjectionExpression projectionExpression,
      ApplyParameter applyParameter,
      bool defaultIfEmpty)
      : base(ExtendedExpressionType.SubQuery, type, parameterExpression, defaultIfEmpty)
    {
      ProjectionExpression = projectionExpression;
      this.applyParameter = applyParameter;
    }

    public SubQueryExpression(Type type,
      ParameterExpression parameterExpression,
      ProjectionExpression projectionExpression,
      ApplyParameter applyParameter,
      ExtendedExpressionType expressionType,
      bool defaultIfEmpty)
      : base(expressionType, type, parameterExpression, defaultIfEmpty)
    {
      ProjectionExpression = projectionExpression;
      this.applyParameter = applyParameter;
    }

    public virtual SubQueryExpression ReplaceApplyParameter(ApplyParameter newApplyParameter)
    {
      var newItemProjector = ProjectionExpression.ItemProjector.RewriteApplyParameter(ApplyParameter, newApplyParameter);
      var newProjectionExpression = new ProjectionExpression(ProjectionExpression.Type, newItemProjector, ProjectionExpression.TupleParameterBindings, ProjectionExpression.ResultType);
      return new SubQueryExpression(Type, OuterParameter, newProjectionExpression, newApplyParameter, DefaultIfEmpty);
    }
  }
}