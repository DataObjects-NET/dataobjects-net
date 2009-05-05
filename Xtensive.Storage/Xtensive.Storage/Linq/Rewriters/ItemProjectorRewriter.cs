// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.18

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Expressions;
using Xtensive.Core.Tuples.Transform;

namespace Xtensive.Storage.Linq.Rewriters
{
  internal sealed class ItemProjectorRewriter : TupleAccessRewriter
  {
    private readonly IList<int> groupMapping;
    private readonly RecordSetHeader header;

    public IList<int> GroupMapping
    {
      get { return groupMapping; }
    }

    public RecordSetHeader Header
    {
      get { return header; }
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.Object!=null) {
        if (mc.Object.Type==typeof (Record) && mc.Method.Name=="get_Item") {
          var groupIndex = (int) ((ConstantExpression) mc.Arguments[0]).Value;
          return Expression.Call(mc.Object, mc.Method, Expression.Constant(groupMapping.IndexOf(groupIndex)));
        }
        if (mc.Object!=null && mc.Object.NodeType==ExpressionType.Constant && mc.Object.Type==typeof (SegmentTransform) && mc.Method.Name=="Apply") {
          var segmentTransform = (SegmentTransform) ((ConstantExpression) mc.Object).Value;
          var offset = mappings.IndexOf(segmentTransform.Segment.Offset);
          var newTransformExpression = Expression.Constant(new SegmentTransform(segmentTransform.IsReadOnly, header.TupleDescriptor, new Segment<int>(offset, segmentTransform.Segment.Length)));
          return Expression.Call(newTransformExpression, mc.Method, mc.Arguments);
        }
      }
      return base.VisitMethodCall(mc);
    }

    protected override Expression VisitNew(NewExpression newExpression)
    {
      switch (newExpression.GetMemberType()) {
// TODO: Rewrite grouping apply parameter access (maybe)
//      case MemberType.Grouping:
//        break;
      case MemberType.Subquery:
        return VisitSubquery(newExpression);
      default:
        return base.VisitNew(newExpression);
      }
      
    }

    private Expression VisitSubquery(NewExpression expression)
    {
      var parameter = expression.GetSubqueryParameter();
      var resultExpression = expression.GetSubqueryItemsResult();
      var newRecordset = TupleParameterAccessRewriter
        .Rewrite(resultExpression.RecordSet.Provider, parameter, mappings)
        .Result;
      var newResultExpression = new ResultExpression(
        resultExpression.Type,
        newRecordset,
        resultExpression.Mapping, 
        resultExpression.ItemProjector, 
        resultExpression.ResultType);
      return Expression.New(
        expression.Constructor, 
        Expression.Constant(newResultExpression), 
        expression.Arguments[1], 
        expression.Arguments[2]);
    }

    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ItemProjectorRewriter(IList<int> mappings, IList<int> groupMapping, RecordSetHeader header)
      : base(mappings)
    {
      this.groupMapping = groupMapping;
      this.header = header;
    }
  }
}