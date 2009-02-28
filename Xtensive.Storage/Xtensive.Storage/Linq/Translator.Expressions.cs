// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.27

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Parameters;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Linq
{
  internal partial class Translator
  {
    private readonly Parameter<bool> joinFinalEntity = new Parameter<bool>();

    protected override Expression Visit(Expression e)
    {
      if (e == null)
        return null;
      if (context.Evaluator.CanBeEvaluated(e)) {
        if (context.ParameterExtractor.IsParameter(e))
          return e;
        return context.Evaluator.Evaluate(e);
      }
      return base.Visit(e);
    }

    protected override Expression VisitMemberPath(MemberPath path, Expression e)
    {
      var pe = path.Parameter;
      var source = context.GetBound(pe);
      var mapping = source.Mapping;
      int number = 0;
      foreach (var item in path) {
        number++;
        if (item.Type == MemberType.Entity && (joinFinalEntity.Value || number != path.Count)) {
          ResultMapping innerMapping;
          var name = item.Name;
          var typeInfo = context.Model.Types[item.Expression.Type];
          if (!mapping.JoinedRelations.TryGetValue(name, out innerMapping)) {
            var joinedIndex = typeInfo.Indexes.PrimaryIndex;
            var joinedRs = IndexProvider.Get(joinedIndex).Result.Alias(context.GetNextAlias());
            var keySegment = mapping.Fields[name];
            var keyPairs = keySegment.GetItems()
              .Select((leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex))
              .ToArray();
            var rs = source.RecordSet.Join(joinedRs, JoinType.Default, keyPairs);
            var fieldMapping = Translator.BuildFieldMapping(typeInfo, source.RecordSet.Header.Columns.Count);
            var joinedMapping = new ResultMapping(fieldMapping, new Dictionary<string, ResultMapping>());
            mapping.JoinedRelations.Add(name, joinedMapping);

            source = new ResultExpression(source.Type, rs, source.Mapping, source.Projector, source.ItemProjector);
            context.ReplaceBound(pe, source);
          }
          mapping = innerMapping;
        }
      }
//      var method = e.Type == typeof(object) ? nonGenericAccessor : genericAccessor.MakeGenericMethod(e.Type);
//      if (path.PathType != MemberType.Primitive)
//        throw new NotSupportedException();
//      var source = context.GetBound(path.Parameter);
//      return Expression.Call(resultParameter, method, Expression.Constant(source.GetMemberSegment(path).Offset));

//      var resultType = e.Type;
//      var source = context.GetBound(path.Parameter);
//      switch (path.PathType)
//      {
//        case MemberType.Primitive:
//          {
//            var method = resultType == typeof(object)
//              ? nonGenericAccessor
//              : genericAccessor.MakeGenericMethod(resultType);
//            var segment = source.GetMemberSegment(path);
//            pSegment.Value = segment;
//            return Expression.Call(tuple, method, Expression.Constant(segment.Offset));
//          }
//        case MemberType.Key:
//          {
//            recordIsUsed = true;
//            var segment = source.GetMemberSegment(path);
//            var keyColumn = (MappedColumn)source.RecordSet.Header.Columns[segment.Offset];
//            var type = keyColumn.ColumnInfoRef.Resolve(context.Model).Field.ReflectedType;
//            var transform = new SegmentTransform(true, type.Hierarchy.KeyInfo.TupleDescriptor, source.GetMemberSegment(path));
//            var keyExtractor = Expression.Call(keyCreateMethod, Expression.Constant(type),
//                                               Expression.Call(Expression.Constant(transform), transformApplyMethod,
//                                                               Expression.Constant(TupleTransformType.Auto), tuple),
//                                               Expression.Constant(false));
//            var rm = source.GetMemberMapping(path);
//            pSegment.Value = segment;
//            return keyExtractor;
//          }
//        case MemberType.Structure:
//          {
//            recordIsUsed = true;
//            var segment = source.GetMemberSegment(path);
//            var structureColumn = (MappedColumn)source.RecordSet.Header.Columns[segment.Offset];
//            var field = structureColumn.ColumnInfoRef.Resolve(context.Model).Field;
//            while (field.Parent != null)
//              field = field.Parent;
//            int groupIndex = source.RecordSet.Header.ColumnGroups.GetGroupIndexBySegment(segment);
//            var result =
//              Expression.MakeMemberAccess(
//                Expression.Convert(
//                  Expression.Call(
//                    Expression.Call(record, recordKeyAccessor, Expression.Constant(groupIndex)),
//                    keyResolveMethod),
//                  field.ReflectedType.UnderlyingType),
//                field.UnderlyingProperty);
//            var rm = source.GetMemberMapping(path);
//            var mapping = rm.Fields.Where(p => p.Value.Offset >= segment.Offset && p.Value.EndOffset <= segment.EndOffset).ToList();
//            var name = mapping.Select(pair => pair.Key).OrderBy(s => s.Length).First();
//            foreach (var pair in mapping)
//            {
//              var key = pair.Key.TryCutPrefix(name).TrimStart('.');
//              if (!pFm.Value.ContainsKey(key))
//                pFm.Value.Add(key, pair.Value);
//            }
//            return result;
//          }
//        case MemberType.Entity:
//          {
//            recordIsUsed = true;
//            var segment = source.GetMemberSegment(path);
//            int groupIndex = source.RecordSet.Header.ColumnGroups.GetGroupIndexBySegment(segment);
//            var result = Expression.Convert(
//              Expression.Call(
//                Expression.Call(record, recordKeyAccessor, Expression.Constant(groupIndex)),
//                keyResolveMethod), resultType);
//            var rm = source.GetMemberMapping(path);
//            var name = rm.Fields.Select(pair => pair.Key).OrderBy(s => s.Length).First();
//            foreach (var pair in rm.Fields)
//            {
//              var key = pair.Key.TryCutPrefix(name).TrimStart('.');
//              if (!pFm.Value.ContainsKey(key))
//                pFm.Value.Add(key, pair.Value);
//            }
//            foreach (var pair in rm.JoinedRelations)
//            {
//              var key = pair.Key.TryCutPrefix(name).TrimStart('.');
//              if (!pJr.Value.ContainsKey(key))
//                pJr.Value.Add(key, pair.Value);
//            }
//            pJr.Value.Add(string.Empty, rm);
//            return result;
//          }
//        case MemberType.EntitySet:
//          {
//            recordIsUsed = true;
//            var m = (MemberExpression)e;
//            var expression = Visit(m.Expression);
//            var result = Expression.MakeMemberAccess(expression, m.Member);
//            return result;
//          }
//        case MemberType.Anonymous:
//          {
//            var rm = source.GetMemberMapping(path);
//            pJr.Value.Add(string.Empty, rm);
//            if (path.Count == 0)
//              return VisitParameter(path.Parameter);
//            var projector = source.Mapping.AnonymousProjections[path.First().Name];
//            var result = parameterRewriter.Rewrite(projector, out recordIsUsed);
//            return result;
//          }
//        default:
//          throw new ArgumentOutOfRangeException();
//      }
      return e;
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      return base.VisitMethodCall(mc);
    }

    protected override Expression VisitUnary(UnaryExpression u)
    {
      return base.VisitUnary(u);
    }

    protected override Expression VisitBinary(BinaryExpression b)
    {
      return base.VisitBinary(b);
    }

    protected override Expression VisitParameter(ParameterExpression p)
    {
      return base.VisitParameter(p);
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      return base.VisitMemberAccess(m);
    }

    protected override Expression VisitLambda(LambdaExpression l)
    {
      return base.VisitLambda(l);
    }

    protected override Expression VisitNew(NewExpression n)
    {
      return base.VisitNew(n);
    }
  }
}