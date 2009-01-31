// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2009.01.13

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Linq;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Expressions.Visitors;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq
{
  internal class ProjectionBuilder : ExpressionVisitor
  {
    private readonly QueryTranslator translator;
    private ResultExpression source;
    private ParameterExpression tuple;
    private ParameterExpression record;
    private bool tupleIsUsed;
    private bool recordIsUsed;
    private RecordSet recordSet;
    private ResultMapping mapping;
    private static readonly MethodInfo transformApplyMethod;
    private static readonly MethodInfo keyCreateMethod;
    private static readonly MethodInfo selectMethod;
    private static readonly MethodInfo genericAccessor;
    private static readonly MethodInfo nonGenericAccessor;
    private static readonly MethodInfo recordKeyAccessor;
    private static readonly MethodInfo keyResolveMethod;

    public ResultExpression Build(ResultExpression source, Expression body)
    {
      this.source = translator.MemberAccessBasedJoiner.Process(source, body, true);
      tuple = Expression.Parameter(typeof (Tuple), "t");
      record = Expression.Parameter(typeof (Record), "r");
      tupleIsUsed = false;
      recordIsUsed = false;
      recordSet = this.source.RecordSet;
      mapping = source.Mapping;
      Expression<Func<RecordSet, object>> projector;
      LambdaExpression itemProjector;

      var rs = Expression.Parameter(typeof(RecordSet), "rs");
      var newBody = Visit(body);
      if (recordIsUsed) {
        var method = typeof (ProjectionBuilder)
          .GetMethod("MakeProjection", BindingFlags.NonPublic | BindingFlags.Static)
          .MakeGenericMethod(newBody.Type);
        itemProjector = Expression.Lambda(
          typeof (Func<,,>).MakeGenericType(typeof(Tuple), typeof(Record), newBody.Type),
          newBody,
          tuple,
          record);
        projector = Expression.Lambda<Func<RecordSet, object>>(
          Expression.Convert(
            Expression.Call(method, rs,itemProjector),
            typeof (object)),
          rs);
      }
      else {
        var method = selectMethod.MakeGenericMethod(typeof (Tuple), newBody.Type);
        itemProjector = Expression.Lambda(newBody, tuple);
        projector = Expression.Lambda<Func<RecordSet, object>>(Expression.Convert(Expression.Call(method, rs, itemProjector), typeof(object)), rs);
      }
      return new ResultExpression(body.Type, recordSet, mapping, projector, itemProjector);
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      if (translator.Evaluator.CanBeEvaluated(m) && translator.ParameterExtractor.IsParameter(m))
        return m;
      var isEntity = typeof(IEntity).IsAssignableFrom(m.Type);
      var isEntitySet = typeof(EntitySetBase).IsAssignableFrom(m.Type);
      var isStructure = typeof(Structure).IsAssignableFrom(m.Type);
      var isKey = typeof(Key).IsAssignableFrom(m.Type);
      if (isEntity || isEntitySet || isStructure) {
        recordIsUsed = true;
        if (isStructure) {
          // TODO: implement
        }
        else if (isEntity) {
          var entityPath = MemberPath.Parse(m, translator.Model);
          var entitySegment = source.GetMemberSegment(entityPath);
          int groupIndex = source.RecordSet.Header.ColumnGroups.GetGroupIndexBySegment(entitySegment);
          var result = Expression.Convert(
              Expression.Call(
                Expression.Call(record, recordKeyAccessor, Expression.Constant(groupIndex)),
                  keyResolveMethod), m.Type);
          return result;
        }
        else {
          // TODO: implement
        }
        throw new NotImplementedException();
      }
      else if (isKey) {
        var keyPath = MemberPath.Parse(m, translator.Model);
        var type = translator.Model.Types[m.Expression.Type];
        var transform = new SegmentTransform(true, type.Hierarchy.KeyTupleDescriptor, source.GetMemberSegment(keyPath));
        var keyExtractor = Expression.Call(keyCreateMethod, Expression.Constant(type),
                                           Expression.Call(Expression.Constant(transform), transformApplyMethod,
                                                           Expression.Constant(TupleTransformType.Auto), tuple),
                                           Expression.Constant(false));
        return keyExtractor;
      }
      var path = MemberPath.Parse(m, translator.Model);
      var method = m.Type == typeof(object) ? 
        nonGenericAccessor : 
        genericAccessor.MakeGenericMethod(m.Type);
      var segment = source.GetMemberSegment(path);
      tupleIsUsed = true;
      return Expression.Call(tuple, method, Expression.Constant(segment.Offset));
    }

    protected override Expression VisitParameter(ParameterExpression p)
    {
      return base.VisitParameter(p);
    }

    protected override MemberBinding VisitBinding(MemberBinding binding)
    {
      return base.VisitBinding(binding);
    }

    protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
    {
      return base.VisitMemberMemberBinding(binding);
    }

    protected override Expression VisitMemberInit(MemberInitExpression mi)
    {
      return base.VisitMemberInit(mi);
    }

    protected override Expression VisitNew(NewExpression n)
    {
      return base.VisitNew(n);
    }

    private static IEnumerable<TResult> MakeProjection<TResult>(RecordSet rs, Expression<Func<Tuple, Record, TResult>> le)
    {
      var func = le.Compile();
      foreach (var r in rs.Parse())
        yield return func(r.Data, r);
    }


    // Constructors

    public ProjectionBuilder(QueryTranslator translator)
    {
      this.translator = translator;
    }

    // Type initializer

    static ProjectionBuilder()
    {
      selectMethod = typeof (Enumerable).GetMethods().Where(m => m.Name==WellKnown.Queryable.Select).First();
      keyCreateMethod = typeof (Key).GetMethod("Create", new[] {typeof (TypeInfo), typeof (Tuple), typeof (bool)});
      transformApplyMethod = typeof (SegmentTransform).GetMethod("Apply", new[] {typeof (TupleTransformType), typeof (Tuple)});
      recordKeyAccessor = typeof(Record).GetProperty("Item", typeof(Key), new[]{typeof(int)}).GetGetMethod();
      keyResolveMethod =typeof (Key).GetMethods()
        .Where(
          mi => mi.Name == "Resolve" && 
          mi.IsGenericMethodDefinition == false && 
          mi.GetParameters().Length == 0)
        .Single();
      foreach (var method in typeof(Tuple).GetMethods()) {
        if (method.Name == "GetValueOrDefault") {
          if (method.IsGenericMethod)
            genericAccessor = method;
          else
            nonGenericAccessor = method;
        }
      }
    }
  }
}