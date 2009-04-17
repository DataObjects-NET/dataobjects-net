// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.02.18

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Linq;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Expressions;

namespace Xtensive.Storage.Linq
{
  internal class TupleAccessProcessor : ExpressionVisitor
  {
    private bool isReplacing;
    private List<int> group;
    private List<int> map;
    private RecordSetHeader header;
    private readonly Action<ApplyParameter, int> registerOuterColumn;
    private readonly Func<ApplyParameter, int, int> resolveOuterColumn;

    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (isReplacing)
        return ReplaceMappings(mc);
      return GatherMappings(mc);
    }

    private Expression GatherMappings(MethodCallExpression mc)
    {
      if (mc.AsTupleAccess()!=null) {
        var columnIndex = mc.GetTupleAccessArgument();
        var outerParameter = mc.ExtractApplyParameterFromTupleAccess();
        if (outerParameter!=null)
          registerOuterColumn(outerParameter, columnIndex);
        else
          map.Add(columnIndex);
        return mc;
      }
      if (header!=null) {
        if (mc.Object!=null) {
          if (mc.Object.Type==typeof (Record) && mc.Method.Name=="get_Item") {
            var groupIndex = (int) ((ConstantExpression) mc.Arguments[0]).Value;
            map.AddRange(header.ColumnGroups[groupIndex].Keys);
            return mc;
          }
          if (mc.Object.Type==typeof (Key) && mc.Object.NodeType==ExpressionType.Call && mc.Method.Name=="Resolve") {
            var key = (MethodCallExpression) mc.Object;
            if (key.Method.Name=="get_Item") {
              var groupIndex = (int) ((ConstantExpression) key.Arguments[0]).Value;
              map.AddRange(header.ColumnGroups[groupIndex].Columns);
              return mc;
            }
          }
        }
        else {
          if (mc.Arguments.Count==1 && mc.Arguments[0].Type==typeof (Key) && mc.Arguments[0].NodeType==ExpressionType.Call && mc.Method.Name=="TryResolve") {
            var key = (MethodCallExpression) mc.Arguments[0];
            if (key.Method.Name=="get_Item") {
              var groupIndex = (int) ((ConstantExpression) key.Arguments[0]).Value;
              map.AddRange(header.ColumnGroups[groupIndex].Columns);
              return mc;
            }
          }
        }
      }
      if (mc.Object!=null && mc.Object.NodeType==ExpressionType.Constant && mc.Object.Type==typeof (SegmentTransform) && mc.Method.Name=="Apply") {
        var segmentTransform = (SegmentTransform) ((ConstantExpression) mc.Object).Value;
        map.AddRange(segmentTransform.Segment.GetItems());
        return mc;
      }
      return base.VisitMethodCall(mc);
    }

    private Expression ReplaceMappings(MethodCallExpression mc)
    {
      if (mc.AsTupleAccess()!=null) {
        var columnIndex = mc.GetTupleAccessArgument();
        var outerParameter = mc.ExtractApplyParameterFromTupleAccess();
        int newIndex = outerParameter!=null
          ? resolveOuterColumn(outerParameter, columnIndex)
          : map.IndexOf(columnIndex);
        return Expression.Call(mc.Object, mc.Method, Expression.Constant(newIndex));
      }
      if (mc.Object!=null) {
        if (mc.Object.Type==typeof (Record) && mc.Method.Name=="get_Item") {
          var groupIndex = (int) ((ConstantExpression) mc.Arguments[0]).Value;
          return Expression.Call(mc.Object, mc.Method, Expression.Constant(group.IndexOf(groupIndex)));
        }
        if (mc.Object!=null && mc.Object.NodeType==ExpressionType.Constant && mc.Object.Type==typeof (SegmentTransform) && mc.Method.Name=="Apply") {
          var segmentTransform = (SegmentTransform) ((ConstantExpression) mc.Object).Value;
          var offset = map.IndexOf(segmentTransform.Segment.Offset);
          var newTransformExpression = Expression.Constant(new SegmentTransform(segmentTransform.IsReadOnly, header.TupleDescriptor, new Segment<int>(offset, segmentTransform.Segment.Length)));
          return Expression.Call(newTransformExpression, mc.Method, mc.Arguments);
        }
      }
      return base.VisitMethodCall(mc);
    }


    public List<int> GatherMappings(Expression predicate)
    {
      return GatherMappings(predicate, null);
    }

    public List<int> GatherMappings(Expression predicate, RecordSetHeader header)
    {
      try {
        this.header = header;
        isReplacing = false;
        map = new List<int>();
        group = null;
        Visit(predicate);
        return map;
      }
      finally {
        map = null;
      }
    }

    public Expression ReplaceMappings(Expression predicate, List<int> mapping, List<int> groupMap, RecordSetHeader header)
    {
      isReplacing = true;
      group = groupMap;
      map = mapping;
      this.header = header;
      return Visit(predicate);
    }
    
    // Constructor

    public TupleAccessProcessor()
      : this(null, null)
    {
    }

    public TupleAccessProcessor(
      Action<ApplyParameter, int> registerOuterColumn,
      Func<ApplyParameter, int, int> resolveOuterColumn)
    {
      this.registerOuterColumn = registerOuterColumn ?? ((p, i) => { throw new NotSupportedException(); });
      this.resolveOuterColumn = resolveOuterColumn ?? ((p, i) => { throw new NotSupportedException(); });
    }
  }
}