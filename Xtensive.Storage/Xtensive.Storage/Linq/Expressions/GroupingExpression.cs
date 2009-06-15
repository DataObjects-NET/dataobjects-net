// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.05.19

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Storage.Linq.Expressions.Visitors;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq.Expressions
{
  [Serializable]
  internal class GroupingExpression : SubQueryExpression
  {
    private readonly Segment<int> segment;

    public Expression KeyExpression { get; private set; }
    public LambdaExpression ElementSelector { get; private set; }

    public override Segment<int> Mapping
    {
      get
      {
        return segment;
      }
    } 

    public override Expression Remap(int[] map, Dictionary<Expression, Expression> processedExpressions)
    {
      var remappedSubquery = (SubQueryExpression)base.Remap(map, processedExpressions);
      var mapping = new Segment<int>(map.IndexOf(Mapping.Offset), 1);
      var remappedKeyExpression = GenericExpressionVisitor<IMappedExpression>.Process(KeyExpression, mapped => mapped.Remap(map, processedExpressions));
      return new GroupingExpression(remappedSubquery.Type, remappedSubquery.OuterParameter, remappedSubquery.ProjectionExpression, remappedSubquery.ApplyParameter, remappedKeyExpression, ElementSelector, mapping, DefaultIfEmpty);
    }

    public override Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      var remappedSubquery = (SubQueryExpression)base.Remap(offset, processedExpressions);
      var mapping = new Segment<int>(Mapping.Offset + offset, 1);
      var remappedKeyExpression = GenericExpressionVisitor<IMappedExpression>.Process(KeyExpression, mapped => mapped.Remap(offset, processedExpressions));
      return new GroupingExpression(remappedSubquery.Type, remappedSubquery.OuterParameter, remappedSubquery.ProjectionExpression, remappedSubquery.ApplyParameter, remappedKeyExpression, ElementSelector, mapping, DefaultIfEmpty);
    }

    public override SubQueryExpression ReplaceApplyParameter(ApplyParameter newApplyParameter)
    {
      var newItemProjector = ProjectionExpression.ItemProjector.RewriteApplyParameter(ApplyParameter, newApplyParameter);
      var newProjectionExpression = new ProjectionExpression(ProjectionExpression.Type, newItemProjector, ProjectionExpression.TupleParameterBindings, ProjectionExpression.ResultType);
      return new GroupingExpression(Type, OuterParameter, newProjectionExpression, newApplyParameter, KeyExpression, ElementSelector, Mapping, DefaultIfEmpty);
    }

    public GroupingExpression(
      Type type,
      ParameterExpression parameterExpression,
      ProjectionExpression projectionExpression,
      ApplyParameter applyParameter,
      Expression keyExpression,
      LambdaExpression elementSelector, Segment<int> segment,
      bool defaultIfEmpty)
      : base(type, parameterExpression, projectionExpression, applyParameter, ExtendedExpressionType.Grouping, defaultIfEmpty)
    {
      KeyExpression = keyExpression;
      ElementSelector = elementSelector;
      this.segment = segment;
    }
  }
}
