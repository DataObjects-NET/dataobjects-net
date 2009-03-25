// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.02.18

using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Linq;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq
{
  internal class TupleAccessProcessor : ExpressionVisitor
  {
    private List<int> map = new List<int>();
    private bool isReplacing;
    private List<int> group;


    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.AsTupleAccess()!=null) {
        if (isReplacing) {
          var value = (int) ((ConstantExpression) mc.Arguments[0]).Value;
          return Expression.Call(mc.Object, mc.Method, Expression.Constant(map.IndexOf(value)));
        }
        else
          map.Add((int) ((ConstantExpression) mc.Arguments[0]).Value);
      }
      else if (mc.Object!=null && mc.Object.Type==typeof (Record) && mc.Method.Name=="get_Item" && isReplacing && group!=null) {
        var value = (int) ((ConstantExpression) mc.Arguments[0]).Value;
        return Expression.Call(mc.Object, mc.Method, Expression.Constant(group.IndexOf(value)));
      }
      else if (mc.Object!=null && mc.Object.NodeType==ExpressionType.Constant && mc.Object.Type==typeof (SegmentTransform) && mc.Method.Name=="Apply" && isReplacing) {
        var segmentTransform = (SegmentTransform) ((ConstantExpression) mc.Object).Value;
        var offset = map.IndexOf(segmentTransform.Segment.Offset);
        var newTransformExpression = Expression.Constant(new SegmentTransform(segmentTransform.IsReadOnly, segmentTransform.Descriptor, new Segment<int>(offset, segmentTransform.Segment.Length)));
        return Expression.Call(newTransformExpression, mc.Method, mc.Arguments);
      }
      return base.VisitMethodCall(mc);
    }

    public List<int> Process(Expression predicate)
    {
      try {
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

    public Expression ReplaceMappings(Expression predicate, List<int> mapping, List<int> groupMap)
    {
      isReplacing = true;
      group = groupMap;
      map = mapping;
      return Visit(predicate);
    }
  }
}