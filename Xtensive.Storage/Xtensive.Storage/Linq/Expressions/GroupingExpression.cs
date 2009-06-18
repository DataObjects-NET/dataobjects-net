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
      return new GroupingExpression(remappedSubquery.Type, remappedSubquery.OuterParameter, DefaultIfEmpty, remappedSubquery.ProjectionExpression, remappedSubquery.ApplyParameter, remappedKeyExpression, mapping);
    }

    public override Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      var remappedSubquery = (SubQueryExpression)base.Remap(offset, processedExpressions);
      var mapping = new Segment<int>(Mapping.Offset + offset, 1);
      var remappedKeyExpression = GenericExpressionVisitor<IMappedExpression>.Process(KeyExpression, mapped => mapped.Remap(offset, processedExpressions));
      return new GroupingExpression(remappedSubquery.Type, remappedSubquery.OuterParameter, DefaultIfEmpty, remappedSubquery.ProjectionExpression, remappedSubquery.ApplyParameter, remappedKeyExpression, mapping);
    }

    public override Expression ReplaceApplyParameter(ApplyParameter newApplyParameter)
    {
      if (newApplyParameter==ApplyParameter)
        return new GroupingExpression(Type, OuterParameter, DefaultIfEmpty, ProjectionExpression, ApplyParameter, KeyExpression, Mapping);

      var newItemProjector = ProjectionExpression.ItemProjector.RewriteApplyParameter(ApplyParameter, newApplyParameter);
      var newProjectionExpression = new ProjectionExpression(ProjectionExpression.Type, newItemProjector, ProjectionExpression.TupleParameterBindings, ProjectionExpression.ResultType);
      return new GroupingExpression(Type, OuterParameter, DefaultIfEmpty, newProjectionExpression, newApplyParameter, KeyExpression, Mapping);
    }

    public GroupingExpression(Type type, ParameterExpression parameterExpression, bool defaultIfEmpty, ProjectionExpression projectionExpression, ApplyParameter applyParameter, Expression keyExpression, Segment<int> segment)
      : base(type, parameterExpression, defaultIfEmpty, projectionExpression, applyParameter, ExtendedExpressionType.Grouping)
    {
      KeyExpression = keyExpression;
      this.segment = segment;
    }
  }
}
