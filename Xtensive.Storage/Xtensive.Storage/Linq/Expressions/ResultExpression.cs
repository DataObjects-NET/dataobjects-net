// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.11

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Expressions.Mappings;

namespace Xtensive.Storage.Linq.Expressions
{
  [DebuggerDisplay("ItemProjector = {ItemProjector}, RecordSet = {RecordSet}, IsScalar = {IsScalar}")]
  internal class ResultExpression : Expression
  {
    public RecordSet RecordSet { get; private set; }
    public FieldMapping Mapping { get; private set; }
    public LambdaExpression ItemProjector { get; private set; }
    public LambdaExpression ScalarTransform { get; private set; }

    public bool IsScalar
    {
      get { return ScalarTransform != null; }
    }

    public TResult GetResult<TResult>()
    {
      var rs = Expression.Parameter(typeof (RecordSet), "rs");
      var severalArguments = ItemProjector.Parameters.Count > 1;
      var method = severalArguments
        ? typeof (Translator)
          .GetMethod("MakeProjection", BindingFlags.NonPublic | BindingFlags.Static)
          .MakeGenericMethod(ItemProjector.Body.Type)
        : WellKnownMembers.EnumerableSelect.MakeGenericMethod(ItemProjector.Parameters[0].Type, ItemProjector.Body.Type);
      Expression body = (!severalArguments && ItemProjector.Parameters[0].Type == typeof (Record))
        ? Expression.Call(method, Expression.Call(WellKnownMembers.RecordSetParse, rs), ItemProjector)
        : Expression.Call(method, rs, ItemProjector);
      body = IsScalar
        ? Expression.Invoke(ScalarTransform, body)
        : body;
      var projector = Expression.Lambda<Func<RecordSet, TResult>>(Expression.Convert(body, typeof(TResult)), rs);
      var project = projector.Compile();
      return project(RecordSet);
    }

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
      LambdaExpression itemProjector)
      : this(type, recordSet, mapping, itemProjector, null)
    {}

    public ResultExpression(
      Type type,
      RecordSet recordSet,
      FieldMapping mapping,
      LambdaExpression itemProjector,
      LambdaExpression scalarTransform)
      : base((ExpressionType)ExtendedExpressionType.Result, type)
    {
      RecordSet = recordSet;
      Mapping = mapping;
      ItemProjector = itemProjector;
      ScalarTransform = scalarTransform;
    }
  }
}