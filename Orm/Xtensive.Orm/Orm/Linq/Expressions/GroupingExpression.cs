// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2009.05.19

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Orm.Linq.Expressions.Visitors;
using Xtensive.Orm.Rse;

namespace Xtensive.Orm.Linq.Expressions
{
  internal class GroupingExpression : SubQueryExpression
  {
    public class SelectManyGroupingInfo
    {
      public ProjectionExpression GroupByProjection { get; }

      public ProjectionExpression GroupJoinOuterProjection { get; }
      public ProjectionExpression GroupJoinInnerProjection { get; }
      public LambdaExpression GroupJoinOuterKeySelector { get; }
      public LambdaExpression GroupJoinInnerKeySelector { get; }

      public SelectManyGroupingInfo(ProjectionExpression groupJoinOuterProjection,
        ProjectionExpression groupJoinInnerProjection,
        LambdaExpression groupJoinOuterKeySelector,
        LambdaExpression groupJoinInnerKeySelector)
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

    public Expression KeyExpression { get; private set; }

    public SelectManyGroupingInfo SelectManyInfo { get; private set; }

    public override Expression BindParameter(ParameterExpression parameter, Dictionary<Expression, Expression> processedExpressions)
    {
      if (processedExpressions.TryGetValue(this, out var result))
        return result;
      if (KeyExpression is not IMappedExpression mappedKey)
        return this;
      var processedKey = mappedKey.BindParameter(parameter, processedExpressions);
      result = new GroupingExpression(Type, OuterParameter, DefaultIfEmpty, ProjectionExpression, ApplyParameter, processedKey, SelectManyInfo);
      processedExpressions.Add(this, result);
      return result;
    }

    public override Expression RemoveOuterParameter(Dictionary<Expression, Expression> processedExpressions)
    {
      if (processedExpressions.TryGetValue(this, out var result))
        return result;
      if (KeyExpression is not IMappedExpression mappedKey)
        return this;
      var processedKey = mappedKey.RemoveOuterParameter(processedExpressions);
      result = new GroupingExpression(Type, OuterParameter, DefaultIfEmpty, ProjectionExpression, ApplyParameter, processedKey, SelectManyInfo);
      processedExpressions.Add(this, result);
      return result;
    }

    public override Expression Remap(IReadOnlyList<int> map, Dictionary<Expression, Expression> processedExpressions)
    {
      var remappedSubquery = (SubQueryExpression) base.Remap(map, processedExpressions);
      var remappedKeyExpression = GenericExpressionVisitor<IMappedExpression>.Process(KeyExpression, mapped => mapped.Remap(map, processedExpressions));
      return new GroupingExpression(remappedSubquery.Type, remappedSubquery.OuterParameter, DefaultIfEmpty, remappedSubquery.ProjectionExpression, remappedSubquery.ApplyParameter, remappedKeyExpression, SelectManyInfo);
    }

    public override Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      var remappedSubquery = (SubQueryExpression) base.Remap(offset, processedExpressions);
      var remappedKeyExpression = GenericExpressionVisitor<IMappedExpression>.Process(KeyExpression, mapped => mapped.Remap(offset, processedExpressions));
      return new GroupingExpression(remappedSubquery.Type, remappedSubquery.OuterParameter, DefaultIfEmpty, remappedSubquery.ProjectionExpression, remappedSubquery.ApplyParameter, remappedKeyExpression, SelectManyInfo);
    }

    public override Expression ReplaceApplyParameter(ApplyParameter newApplyParameter)
    {
      if (newApplyParameter==ApplyParameter)
        return new GroupingExpression(Type, OuterParameter, DefaultIfEmpty, ProjectionExpression, ApplyParameter, KeyExpression, SelectManyInfo);

      var newItemProjector = ProjectionExpression.ItemProjector.RewriteApplyParameter(ApplyParameter, newApplyParameter);
      var newProjectionExpression = ProjectionExpression.ApplyItemProjector(newItemProjector);
      return new GroupingExpression(Type, OuterParameter, DefaultIfEmpty, newProjectionExpression, newApplyParameter, KeyExpression, SelectManyInfo);
    }

    public GroupingExpression(
      Type type, 
      ParameterExpression parameterExpression, 
      bool defaultIfEmpty, 
      ProjectionExpression projectionExpression, 
      ApplyParameter applyParameter, 
      Expression keyExpression, 
      SelectManyGroupingInfo selectManyInfo)
      : base(type, parameterExpression, defaultIfEmpty, projectionExpression, applyParameter, ExtendedExpressionType.Grouping)
    {
      SelectManyInfo = selectManyInfo;
      KeyExpression = keyExpression;
    }
  }
}