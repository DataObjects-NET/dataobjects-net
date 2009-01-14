// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.11

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq.Linq2Rse.Internal;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using System.Linq;

namespace Xtensive.Storage.Linq.Expressions
{
  [Serializable]
  internal sealed class ProjectionExpression : Expression
  {
    public RecordSet RecordSet { get; private set; }
    public Expression<Func<RecordSet, object>> Projector { get; private set; }
    public TypeMapping Mapping { get; private set; }

    public int GetColumnIndex(IEnumerable<MappingPathItem> fieldPath)
    {
      var result = -1;
      var pathList = fieldPath.ToList();
      if (pathList.Count == 0)
        return result;
      var first = pathList[0];
      var mapping = Mapping;
      if (first.FieldName != null) {
        if (mapping.FieldMapping.TryGetValue(first.FieldName, out result))
          return result;
      }
      else
        mapping = mapping.JoinedRelations[first.JoinedFieldName];

      for (int i = 1; i < pathList.Count; i++) {
        var item = pathList[i];
        if (item.FieldName != null) {
          if (mapping.FieldMapping.TryGetValue(item.FieldName, out result))
            return result;
        }
        mapping = mapping.JoinedRelations[item.JoinedFieldName];
      }
      return -1;
    }


    // Constructors

    public ProjectionExpression(Type type, RecordSet recordSet, TypeMapping mapping, Expression<Func<RecordSet, object>> projector)
      : base(ExpressionType.Constant, type)
    {
      RecordSet = recordSet;
      Mapping = mapping;
      Projector = projector;
    }
  }
}