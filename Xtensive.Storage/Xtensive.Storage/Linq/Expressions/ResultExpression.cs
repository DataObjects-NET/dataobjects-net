// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.11

using System;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Expressions.Mappings;

namespace Xtensive.Storage.Linq.Expressions
{
  internal class ResultExpression : Expression
  {
    public RecordSet RecordSet { get; private set; }
    public Expression<Func<RecordSet, object>> Projector { get; private set; }
    public LambdaExpression ItemProjector { get; private set; }
    public FieldMapping Mapping { get; private set; }

    public Segment<int> GetMemberSegment(MemberPath fieldPath)
    {
      var pathList = fieldPath.ToList();
      if (pathList.Count == 0) {
        var pm = Mapping as PrimitiveFieldMapping;
        if (pm == null)
          throw new InvalidOperationException();
        return pm.Segment;
      }
      var mapping = (ComplexFieldMapping)Mapping;
      for (int i = 0; i < pathList.Count - 1; i++) {
        var item = pathList[i];
        if (item.Type == MemberType.Entity || item.Type == MemberType.Anonymous)
          mapping = mapping.GetJoinedFieldMapping(item.Name);
      }
      var lastItem = pathList.Last();
      if (lastItem.Type == MemberType.Anonymous)
        throw new InvalidOperationException();

      if (lastItem.Type == MemberType.Entity) {
        mapping = mapping.GetJoinedFieldMapping(lastItem.Name);
        if (mapping.Fields.Count > 0) {
          var offset = mapping.Fields.Min(pair => pair.Value.Offset);
          var endOffset = mapping.Fields.Max(pair => pair.Value.Offset);
          var length = endOffset - offset + 1;
          return new Segment<int>(offset, length);
        }
      }
      return mapping.GetFieldSegment(lastItem.Name);
    }


    public FieldMapping GetMemberMapping(MemberPath fieldPath)
    {
      var pathList = fieldPath.ToList();
      if (pathList.Count == 0)
        return Mapping;
      var first = pathList[0];
      var mapping = Mapping;
      if (first.Type == MemberType.Entity || first.Type == MemberType.Anonymous)
        mapping = ((ComplexFieldMapping)mapping).GetJoinedFieldMapping(first.Name);
      else
        return mapping;

      for (int i = 1; i < pathList.Count; i++) {
        var item = pathList[i];
        if (item.Type != MemberType.Entity || first.Type == MemberType.Anonymous)
          return mapping;
        mapping = ((ComplexFieldMapping)mapping).GetJoinedFieldMapping(item.Name);
      }
      return mapping;
    }


    // Constructors

    public ResultExpression(
      Type type, 
      RecordSet recordSet, 
      FieldMapping mapping, 
      Expression<Func<RecordSet, object>> projector, 
      LambdaExpression itemProjector)
      : base((ExpressionType)ExtendedExpressionType.Result, type)
    {
      RecordSet = recordSet;
      Mapping = mapping;
      Projector = projector;
      ItemProjector = itemProjector;
    }
  }
}