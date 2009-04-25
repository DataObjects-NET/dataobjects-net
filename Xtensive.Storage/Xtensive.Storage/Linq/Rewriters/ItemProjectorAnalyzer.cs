// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.18

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Core.Helpers;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Linq.Expressions.Mappings;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Expressions;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Linq.Rewriters
{
  internal sealed class ItemProjectorAnalyzer : TupleAccessGatherer
  {
    private Provider provider;
    private DomainModel model;
    private Parameter<IMapping> fieldMapping = new Parameter<IMapping>("fieldMapping");

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (provider!=null) {
        if (mc.Object!=null) {
          if (mc.Object.Type==typeof (Record) && mc.Method.Name=="get_Item") {
            var groupIndex = (int) ((ConstantExpression) mc.Arguments[0]).Value;
            mappings.AddRange(provider.Header.ColumnGroups[groupIndex].Keys);
            return mc;
          }
          if (mc.Object.Type==typeof (Key) && mc.Object.NodeType==ExpressionType.Call && mc.Method.Name=="Resolve") {
            var key = (MethodCallExpression) mc.Object;
            if (key.Method.Name=="get_Item") {
              var groupIndex = (int) ((ConstantExpression) key.Arguments[0]).Value;
              //mappings.AddRange(header.ColumnGroups[groupIndex].Columns);
              RegisterColumns(groupIndex, mc.Method.ReturnType);
              return mc;
            }
          }
        }
        else {
          if (mc.Arguments.Count==1 && mc.Arguments[0].Type==typeof (Key) && mc.Arguments[0].NodeType==ExpressionType.Call && mc.Method.Name=="TryResolve") {
            var key = (MethodCallExpression) mc.Arguments[0];
            if (key.Method.Name=="get_Item") {
              var groupIndex = (int) ((ConstantExpression) key.Arguments[0]).Value;
              //mappings.AddRange(header.ColumnGroups[groupIndex].Columns);
              RegisterColumns(groupIndex, mc.Method.ReturnType);
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

    private void RegisterColumns(int columnGroupIndex, Type type)
    {
      var complexMapping = (ComplexMapping) fieldMapping.Value;
      var lazyLoadFields = model.Types[type].Fields.Where(f => f.IsLazyLoad).Select(f => f.Name);
      var usedColumns = provider.Header.ColumnGroups[columnGroupIndex].Columns
        .Except(lazyLoadFields.SelectMany(f => complexMapping.GetFieldMapping(f).GetItems()));
      mappings.AddRange(usedColumns);
    }

    protected override Expression VisitNew(NewExpression newExpression)
    {
      switch (newExpression.GetMemberType()) {
      case MemberType.Anonymous:
        return VisitAnonymous(newExpression);
      case MemberType.Subquery:
        return VisitSubquery(newExpression);
      default:
        return base.VisitNew(newExpression);
      }
    }

    private NewExpression VisitSubquery(NewExpression expression)
    {
      var parameter = expression.GetSubqueryParameter();
      var resultExpression = expression.GetSubqueryItemsResult();
      var mappingColumns = TupleParameterAccessAnalyzer
        .Analyze(resultExpression.RecordSet.Provider, parameter);
      mappings.AddRange(mappingColumns);
      return expression;
    }

    private NewExpression VisitAnonymous(NewExpression newExpression)
    {
      var complexMapping = (ComplexMapping) fieldMapping.Value;
      var arguments = newExpression.Members.Select(m => m.Name.TryCutPrefix(WellKnown.GetterPrefix)).Zip(newExpression.Arguments);
      foreach (var arg in arguments) {
        ComplexMapping newMapping = null;
        switch (arg.Second.GetMemberType()) {
        case MemberType.Entity:
          complexMapping.Entities.TryGetValue(arg.First, out newMapping);
          break;
        case MemberType.Anonymous:
          Pair<ComplexMapping, Expression> pair;
          if (complexMapping.AnonymousTypes.TryGetValue(arg.First, out pair))
            newMapping = pair.First;
          break;
        case MemberType.Grouping:
          complexMapping.Groupings.TryGetValue(arg.First, out newMapping);
          break;
        case MemberType.Subquery:
          complexMapping.Subqueries.TryGetValue(arg.First, out newMapping);
          break;
        }
        using (new ParameterScope()) {
          if (newMapping!=null)
            fieldMapping.Value = newMapping;
          Visit(arg.Second);
        }
      }
      return newExpression;
    }

    public List<int> Gather(Expression expression, Provider provider, DomainModel model, IMapping fieldMapping)
    {
      try {
        this.provider = provider;
        this.model = model;
        mappings = new List<int>();
        using (new ParameterScope()) {
          this.fieldMapping.Value = fieldMapping;
          Visit(expression);
        }
        return mappings;
      }
      finally {
        this.provider = null;
        mappings = null;
      }
    }
  }
}