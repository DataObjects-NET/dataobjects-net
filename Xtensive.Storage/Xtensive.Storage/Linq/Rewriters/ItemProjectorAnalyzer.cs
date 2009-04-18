// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.18

using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Expressions;

namespace Xtensive.Storage.Linq.Rewriters
{
  internal sealed class ItemProjectorAnalyzer : TupleAccessGatherer
  {
    private RecordSetHeader header;

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (header!=null) {
        if (mc.Object!=null) {
          if (mc.Object.Type==typeof (Record) && mc.Method.Name=="get_Item") {
            var groupIndex = (int) ((ConstantExpression) mc.Arguments[0]).Value;
            mappings.AddRange(header.ColumnGroups[groupIndex].Keys);
            return mc;
          }
          if (mc.Object.Type==typeof (Key) && mc.Object.NodeType==ExpressionType.Call && mc.Method.Name=="Resolve") {
            var key = (MethodCallExpression) mc.Object;
            if (key.Method.Name=="get_Item") {
              var groupIndex = (int) ((ConstantExpression) key.Arguments[0]).Value;
              mappings.AddRange(header.ColumnGroups[groupIndex].Columns);
              return mc;
            }
          }
        }
        else {
          if (mc.Arguments.Count==1 && mc.Arguments[0].Type==typeof (Key) && mc.Arguments[0].NodeType==ExpressionType.Call && mc.Method.Name=="TryResolve") {
            var key = (MethodCallExpression) mc.Arguments[0];
            if (key.Method.Name=="get_Item") {
              var groupIndex = (int) ((ConstantExpression) key.Arguments[0]).Value;
              mappings.AddRange(header.ColumnGroups[groupIndex].Columns);
              return mc;
            }
          }
        }
      }
      if (mc.Object!=null && mc.Object.NodeType==ExpressionType.Constant && mc.Object.Type==typeof (SegmentTransform) && mc.Method.Name=="Apply") {
        var segmentTransform = (SegmentTransform) ((ConstantExpression) mc.Object).Value;
        mappings.AddRange(segmentTransform.Segment.GetItems());
        return mc;
      }
      return base.VisitMethodCall(mc);
    }

    public List<int> Gather(Expression expression, RecordSetHeader header)
    {
      try {
        this.header = header;
        mappings = new List<int>();
        Visit(expression);
        return mappings;
      }
      finally {
        this.header = null;
        mappings = null;
      }
    }
  }
}