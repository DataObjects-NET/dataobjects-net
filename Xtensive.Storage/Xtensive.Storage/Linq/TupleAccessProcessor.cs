// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.02.18

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Linq;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq
{
  internal class TupleAccessProcessor : ExpressionVisitor
  {
    private bool isReplacing;
    private List<int> group;
    private List<int> map;
    private readonly Action<Parameter<Tuple>, int> registerOuterColumn;
    private readonly Func<Parameter<Tuple>, int, int> resolveOuterColumn;

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.AsTupleAccess() != null) {
        int index = (int)((ConstantExpression)mc.Arguments[0]).Value;
        var outerParameter = TryExtractOuterParameter(mc);
        if (!isReplacing) {
          if (outerParameter != null)
            registerOuterColumn(outerParameter, index);
          else
            map.Add(index);
        }
        else {
          int newIndex = outerParameter!=null
            ? resolveOuterColumn(outerParameter, index)
            : map.IndexOf(index);
          return Expression.Call(mc.Object, mc.Method, Expression.Constant(newIndex));
        }
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

    private Parameter<Tuple> TryExtractOuterParameter(MethodCallExpression tupleAccess)
    {
      if (tupleAccess.Object.NodeType != ExpressionType.MemberAccess)
        return null;
      var memberAccess = (MemberExpression) tupleAccess.Object;
      if (memberAccess.Member != Translator.WellKnownMethods.ParameterOfTupleValue)
        return null;
      if (memberAccess.Expression.NodeType != ExpressionType.Constant)
        return null;
      var constant = (ConstantExpression) memberAccess.Expression;
      return (Parameter<Tuple>) constant.Value;
    }

    public TupleAccessProcessor()
      : this(null, null)
    {
    }

    public TupleAccessProcessor(
      Action<Parameter<Tuple>, int> registerOuterColumn,
      Func<Parameter<Tuple>, int, int> resolveOuterColumn)
    {
      this.registerOuterColumn = registerOuterColumn ?? ((p, i) => { throw new NotSupportedException(); });
      this.resolveOuterColumn = resolveOuterColumn ?? ((p, i) => { throw new NotSupportedException(); });
    }
  }
}