// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.11

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Rse;
using System.Linq;

namespace Xtensive.Storage.Linq
{
  internal sealed class ResultExpression : Expression
  {
    public RecordSet RecordSet { get; private set; }
    public Expression<Func<RecordSet, object>> Projector { get; private set; }
    public LambdaExpression ItemProjector { get; private set; }
    public ResultMapping Mapping { get; private set; }

    public Segment<int> GetMemberSegment(MemberPath fieldPath)
    {
      Segment<int> result;
      var pathList = fieldPath.ToList();
      if (pathList.Count == 0)
        return Mapping.Segment;
      var first = pathList[0];
      var mapping = Mapping;
      if (first.Type != MemberType.Entity) {
        if (mapping.Fields.TryGetValue(first.Name, out result))
          return result;
      }
      else {
        mapping = mapping.JoinedRelations[first.Name];
        result = mapping.Segment;
      }

      for (int i = 1; i < pathList.Count; i++) {
        var item = pathList[i];
        if (item.Type != MemberType.Entity) {
          if (mapping.Fields.TryGetValue(item.Name, out result))
            return result;
        }
        mapping = mapping.JoinedRelations[item.Name];
        result = mapping.Segment;
      }
      return result;
    }


    public ResultMapping GetMemberMapping(MemberPath fieldPath)
    {
      var pathList = fieldPath.ToList();
      if (pathList.Count == 0)
        return Mapping;
      var first = pathList[0];
      var mapping = Mapping;
      if (first.Type == MemberType.Entity)
        mapping = mapping.JoinedRelations[first.Name];
      else
        return mapping;

      for (int i = 1; i < pathList.Count; i++) {
        var item = pathList[i];
        if (item.Type != MemberType.Entity) {
          return mapping;
        }
        mapping = mapping.JoinedRelations[item.Name];
      }
      return mapping;
    }


    // Constructors

    public ResultExpression(Type type, RecordSet recordSet, ResultMapping mapping, Expression<Func<RecordSet, object>> projector, LambdaExpression itemProjector)
      : base((ExpressionType)ExtendedExpressionType.Result, type)
    {
      RecordSet = recordSet;
      Mapping = mapping;
      Projector = projector;
      ItemProjector = itemProjector;
    }
  }
}