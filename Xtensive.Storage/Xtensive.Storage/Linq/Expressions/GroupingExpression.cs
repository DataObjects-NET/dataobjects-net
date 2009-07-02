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
    public class SelectManyGroupingInfo
    {
      public ProjectionExpression GroupByProjection { get; private set; }

      public ProjectionExpression GroupJoinOuterProjection { get; private set; }
      public ProjectionExpression GroupJoinInnerProjection { get; private set; }
      public LambdaExpression GroupJoinOuterKeySelector { get; private set; }
      public LambdaExpression GroupJoinInnerKeySelector { get; private set; }

      public SelectManyGroupingInfo(ProjectionExpression groupJoinOuterProjection, ProjectionExpression groupJoinInnerProjection, LambdaExpression groupJoinOuterKeySelector, LambdaExpression groupJoinInnerKeySelector)
      {
        GroupJoinOuterProjection = groupJoinOuterProjection;
        GroupJoinInnerProjection = groupJoinInnerProjection;
        GroupJoinOuterKeySelector = groupJoinOuterKeySelector;
        GroupJoinInnerKeySelector = groupJoinInnerKeySelector;
      }

      public SelectManyGroupingInfo(ProjectionExpression groupByProjection)
      {
        GroupByProjection = groupByProjection;
      }
    }

    private readonly Segment<int> segment;

    public Expression KeyExpression { get; private set; }

    public LambdaExpression OriginalKeySelector { get; private set; }
    public SelectManyGroupingInfo SelectManyInfo { get; private set; }

    public override Segment<int> Mapping
    {
      get { return segment; }
    }

    public override Expression Remap(int[] map, Dictionary<Expression, Expression> processedExpressions)
    {
      var remappedSubquery = (SubQueryExpression) base.Remap(map, processedExpressions);
      var mapping = new Segment<int>(map.IndexOf(Mapping.Offset), 1);
      var remappedKeyExpression = GenericExpressionVisitor<IMappedExpression>.Process(KeyExpression, mapped => mapped.Remap(map, processedExpressions));
      return new GroupingExpression(remappedSubquery.Type, remappedSubquery.OuterParameter, DefaultIfEmpty, remappedSubquery.ProjectionExpression, remappedSubquery.ApplyParameter, remappedKeyExpression, mapping, OriginalKeySelector, SelectManyInfo);
    }

    public override Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      var remappedSubquery = (SubQueryExpression) base.Remap(offset, processedExpressions);
      var mapping = new Segment<int>(Mapping.Offset + offset, 1);
      var remappedKeyExpression = GenericExpressionVisitor<IMappedExpression>.Process(KeyExpression, mapped => mapped.Remap(offset, processedExpressions));
      return new GroupingExpression(remappedSubquery.Type, remappedSubquery.OuterParameter, DefaultIfEmpty, remappedSubquery.ProjectionExpression, remappedSubquery.ApplyParameter, remappedKeyExpression, mapping, OriginalKeySelector, SelectManyInfo);
    }

    public override Expression ReplaceApplyParameter(ApplyParameter newApplyParameter)
    {
      if (newApplyParameter==ApplyParameter)
        return new GroupingExpression(Type, OuterParameter, DefaultIfEmpty, ProjectionExpression, ApplyParameter, KeyExpression, Mapping, OriginalKeySelector, SelectManyInfo);

      var newItemProjector = ProjectionExpression.ItemProjector.RewriteApplyParameter(ApplyParameter, newApplyParameter);
      var newProjectionExpression = new ProjectionExpression(ProjectionExpression.Type, newItemProjector, ProjectionExpression.TupleParameterBindings, ProjectionExpression.ResultType);
      return new GroupingExpression(Type, OuterParameter, DefaultIfEmpty, newProjectionExpression, newApplyParameter, KeyExpression, Mapping, OriginalKeySelector, SelectManyInfo);
    }

    public GroupingExpression(Type type, ParameterExpression parameterExpression, bool defaultIfEmpty, ProjectionExpression projectionExpression, ApplyParameter applyParameter, Expression keyExpression, Segment<int> segment, LambdaExpression originalKeySelector, SelectManyGroupingInfo selectManyInfo)
      : base(type, parameterExpression, defaultIfEmpty, projectionExpression, applyParameter, ExtendedExpressionType.Grouping)
    {
      SelectManyInfo = selectManyInfo;
      OriginalKeySelector = originalKeySelector;
      KeyExpression = keyExpression;
      this.segment = segment;
    }
  }
}