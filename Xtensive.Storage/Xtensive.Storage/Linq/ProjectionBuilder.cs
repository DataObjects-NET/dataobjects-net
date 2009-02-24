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
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Linq;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq
{
  internal class ProjectionBuilder : MemberPathVisitor
  {
    private readonly TranslatorContext context;
    private ParameterExpression tuple;
    private ParameterExpression record;
    private bool recordIsUsed;
    private static readonly Parameter<Dictionary<string, Segment<int>>> pFm = new Parameter<Dictionary<string, Segment<int>>>();
    private static readonly Parameter<Dictionary<string, ResultMapping>> pJr = new Parameter<Dictionary<string, ResultMapping>>();
    private static readonly Parameter<Dictionary<string, Expression>> pAp = new Parameter<Dictionary<string, Expression>>();
    private static readonly Parameter<Segment<int>> pSegment = new Parameter<Segment<int>>();
    private ProjectionParameterRewriter parameterRewriter;
    private ParameterExpression[] parameters;
    private List<CalculatedColumnDescriptor> calculatedColumns;
    private static readonly MethodInfo transformApplyMethod;
    private static readonly MethodInfo keyCreateMethod;
    private static readonly MethodInfo selectMethod;
    private static readonly MethodInfo genericAccessor;
    private static readonly MethodInfo nonGenericAccessor;
    private static readonly MethodInfo recordKeyAccessor;
    private static readonly MethodInfo keyResolveMethod;

    public ResultExpression Build(LambdaExpression le)
    {
      calculatedColumns = new List<CalculatedColumnDescriptor>();
      parameters = le.Parameters.ToArray();
      var body = le.Body;
      context.MemberAccessBasedJoiner.Process(body, true);
      tuple = Expression.Parameter(typeof (Tuple), "t");
      record = Expression.Parameter(typeof (Record), "r");
      parameterRewriter = new ProjectionParameterRewriter(tuple, record);
      recordIsUsed = false;
      Expression<Func<RecordSet, object>> projector;
      LambdaExpression itemProjector;

      var rs = Expression.Parameter(typeof(RecordSet), "rs");
      Expression newBody = null;
      ResultMapping mapping;
      using (new ParameterScope()) {
        pFm.Value = new Dictionary<string, Segment<int>>();
        pJr.Value = new Dictionary<string, ResultMapping>();
        pAp.Value = new Dictionary<string, Expression>();
        pSegment.Value = default(Segment<int>);
        if (body.NodeType != ExpressionType.New) {
          var path = MemberPath.Parse(body, context.Model);
          if (!path.IsValid) {
            // TODO check for queries
            var сс = context.MemberAccessReplacer.ProcessCalculated(Expression.Lambda(body, parameters));
            var ccd = new CalculatedColumnDescriptor(context.GetNextColumnAlias(), body.Type, (Expression<Func<Tuple, object>>)сс);
            calculatedColumns.Add(ccd);
            int position = context.GetBound(parameters[0]).RecordSet.Header.Columns.Count + calculatedColumns.Count - 1;
            var method = genericAccessor.MakeGenericMethod(body.Type);
            newBody = Expression.Call(tuple, method, Expression.Constant(position));
            pFm.Value.Add(string.Empty, new Segment<int>(position, 1));
          }
        }
        newBody = newBody ?? Visit(body);
        mapping = pSegment.Value.Length == 0
          ? new ResultMapping(pFm.Value, pJr.Value, pAp.Value)
          : new ResultMapping(pSegment.Value);
      }
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
      // projection for any parameter could be used
      // because these projections contain same record set
      var recordSet = context.GetBound(le.Parameters[0]).RecordSet;
      if (calculatedColumns.Count > 0)
        recordSet = recordSet.Calculate(calculatedColumns.ToArray());
      return new ResultExpression(body.Type, recordSet, mapping, projector, itemProjector);
    }

    protected override Expression VisitMemberPath(MemberPathExpression mpe)
    {
      var resultType = mpe.Expression.Type;
      var path = mpe.Path;
      var source = context.GetBound(path.Parameter);
      switch (path.PathType) {
        case MemberType.Primitive: {
          var method = resultType == typeof (object)
            ? nonGenericAccessor
            : genericAccessor.MakeGenericMethod(resultType);
          var segment = source.GetMemberSegment(path);
          pSegment.Value = segment;
          return Expression.Call(tuple, method, Expression.Constant(segment.Offset));
        }
        case MemberType.Key: {
          recordIsUsed = true;
          var segment = source.GetMemberSegment(path);
          var keyColumn = (MappedColumn) source.RecordSet.Header.Columns[segment.Offset];
          var type = keyColumn.ColumnInfoRef.Resolve(context.Model).Field.ReflectedType;
          var transform = new SegmentTransform(true, type.Hierarchy.KeyInfo.TupleDescriptor, source.GetMemberSegment(path));
          var keyExtractor = Expression.Call(keyCreateMethod, Expression.Constant(type),
                                             Expression.Call(Expression.Constant(transform), transformApplyMethod,
                                                             Expression.Constant(TupleTransformType.Auto), tuple),
                                             Expression.Constant(false));
          var rm = source.GetMemberMapping(path);
          pSegment.Value = segment;
          return keyExtractor;
        }
        case MemberType.Structure: {
          recordIsUsed = true;
          var segment = source.GetMemberSegment(path);
          var structureColumn = (MappedColumn)source.RecordSet.Header.Columns[segment.Offset];
          var field = structureColumn.ColumnInfoRef.Resolve(context.Model).Field;
          while (field.Parent != null)
            field = field.Parent;
          int groupIndex = source.RecordSet.Header.ColumnGroups.GetGroupIndexBySegment(segment);
          var result = 
            Expression.MakeMemberAccess(
              Expression.Convert(
                Expression.Call(
                  Expression.Call(record, recordKeyAccessor, Expression.Constant(groupIndex)),
                  keyResolveMethod), 
                field.ReflectedType.UnderlyingType), 
              field.UnderlyingProperty);
          var rm = source.GetMemberMapping(path);
          var mapping = rm.Fields.Where(p => p.Value.Offset >= segment.Offset && p.Value.EndOffset <= segment.EndOffset).ToList();
          var name = mapping.Select(pair => pair.Key).OrderBy(s => s.Length).First();
          var prefix = name + ".";
          foreach (var pair in mapping.Where(p => p.Key != name)) {
            var key = pair.Key.Replace(prefix, string.Empty);
            if (!pFm.Value.ContainsKey(key))
              pFm.Value.Add(key, pair.Value);
          }
          return result;
        }
        case MemberType.Entity: {
          recordIsUsed = true;
          var segment = source.GetMemberSegment(path);
          int groupIndex = source.RecordSet.Header.ColumnGroups.GetGroupIndexBySegment(segment);
          var result = Expression.Convert(
            Expression.Call(
              Expression.Call(record, recordKeyAccessor, Expression.Constant(groupIndex)),
              keyResolveMethod), resultType);
          var rm = source.GetMemberMapping(path);
          var name = rm.Fields.Select(pair => pair.Key).OrderBy(s => s.Length).First();
          var prefix = name + ".";
          foreach (var pair in rm.Fields) {
            var key = pair.Key.Replace(prefix, string.Empty);
            if (!pFm.Value.ContainsKey(key))
              pFm.Value.Add(key, pair.Value);
          }
          foreach (var pair in rm.JoinedRelations) {
            var key = pair.Key.Replace(prefix, string.Empty);
            if (!pJr.Value.ContainsKey(key))
              pJr.Value.Add(key, pair.Value);
          }
          pJr.Value.Add(string.Empty, rm);
          return result;
        }
        case MemberType.EntitySet: {
          recordIsUsed = true;
          var m = (MemberExpression) mpe.Expression;
          var expression = Visit(m.Expression);
          var result = Expression.MakeMemberAccess(expression, m.Member);
          return result;
        }
        case MemberType.Anonymous: {
          var rm = source.GetMemberMapping(path);
          pJr.Value.Add(string.Empty, rm);
          if (path.Count == 0)
            return VisitParameter(path.Parameter);
          var projector = source.Mapping.AnonymousProjections[path.First().Name];
          var result = parameterRewriter.Rewrite(projector, out recordIsUsed);
          return result;
        }
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      if (context.Evaluator.CanBeEvaluated(m) && context.ParameterExtractor.IsParameter(m))
        return m;
      return base.VisitMemberAccess(m);
    }

    protected override Expression VisitParameter(ParameterExpression p)
    {
      var source = context.GetBound(p);
      var rm = source.Mapping;
      foreach (var pair in rm.Fields)
        if (!pFm.Value.ContainsKey(pair.Key))
          pFm.Value.Add(pair.Key, pair.Value);
      foreach (var pair in rm.JoinedRelations)
        if (!pJr.Value.ContainsKey(pair.Key))
          pJr.Value.Add(pair.Key, pair.Value);
      foreach (var pair in rm.AnonymousProjections)
        if (!pAp.Value.ContainsKey(pair.Key))
          pAp.Value.Add(pair.Key, pair.Value);
      return parameterRewriter.Rewrite(source.ItemProjector.Body, out recordIsUsed);
    }

    protected override Expression VisitNew(NewExpression n)
    {
      var arguments = new List<Expression>();
      if (n.Members == null)
        throw new NotSupportedException(n.ToString(true));
      for (int i = 0; i < n.Arguments.Count; i++) {
        var arg = n.Arguments[i];
        var newArg = (Expression) null;
        var member = n.Members[i];
        var memberName = member.Name.Replace(WellKnown.GetterPrefix, string.Empty);
        var path = MemberPath.Parse(arg, context.Model);
        if (path.IsValid) {
          using (new ParameterScope()) {
            Dictionary<string, Segment<int>> fm;
            Dictionary<string, ResultMapping> jr;
            Dictionary<string, Expression> ap;
            Segment<int> segment;
            using (new ParameterScope()) {
              pFm.Value = new Dictionary<string, Segment<int>>();
              pJr.Value = new Dictionary<string, ResultMapping>();
              pAp.Value = new Dictionary<string, Expression>();
              pSegment.Value = default(Segment<int>);
              newArg = Visit(arg);
              fm = pFm.Value;
              jr = pJr.Value;
              ap = pAp.Value;
              segment = pSegment.Value;
            }
            if (segment.Length != 0)
              pFm.Value.Add(memberName, segment);
            foreach (var p in fm)
              pFm.Value.Add(memberName + "." + p.Key, p.Value);
            foreach (var p in jr)
              pJr.Value.Add(memberName + "." + p.Key, p.Value);
            foreach (var p in ap)
              pAp.Value.Add(memberName + "." + p.Key, p.Value);
            pJr.Value.Add(memberName, new ResultMapping(fm, jr, ap));
            pAp.Value.Add(memberName, newArg);
          }

//          switch (path.PathType) {
//            case MemberType.Default:
//            case MemberType.Primitive:
//            case MemberType.Key:
//            case MemberType.Structure:
//            case MemberType.Entity:
//            case MemberType.EntitySet: {
//              var source = context.GetBound(path.Parameter);
//              var segment = source.GetMemberSegment(path);
//              var rm = source.GetMemberMapping(path);
//              var mapping = rm.Fields.Where(p => p.Value.Offset >= segment.Offset && p.Value.EndOffset <= segment.EndOffset).ToList();
//              var oldName = mapping.Select(pair => pair.Key).OrderBy(s => s.Length).First();
//              var startsWithOldName = mapping.All(pair => pair.Key.StartsWith(oldName));
//              if (startsWithOldName) {
//                mapping = mapping.Select(pair => new KeyValuePair<string, Segment<int>>(pair.Key.Replace(oldName, memberName), pair.Value)).ToList();
//              }
//              else {
//                mapping = mapping.Select(pair => new KeyValuePair<string, Segment<int>>(memberName + "." + pair.Key, pair.Value)).ToList();
//                mapping.Add(new KeyValuePair<string, Segment<int>>(memberName, segment));
//              }
//              
//              foreach (var pair in mapping) {
//                if(!pFm.Value.ContainsKey(pair.Key))
//                  pFm.Value.Add(pair.Key, pair.Value);
//              }
//              var memberType = arg.GetMemberType();
//              if (memberType==MemberType.Entity)
//                pJr.Value.Add(memberName, rm);
//              break;
//            }
//            case MemberType.Anonymous: {
//              var source = context.GetBound(path.Parameter);
//              var rm = source.GetMemberMapping(path);
//              pJr.Value.Add(memberName, rm);
//              var projector = source.Mapping.AnonymousProjections[path.First().Name];
//              newArg = parameterRewriter.Rewrite(projector, out recordIsUsed);
//              break;
//            }
//            default:
//              throw new ArgumentOutOfRangeException();
//          }
//          newArg = newArg ?? VisitMemberPath(new MemberPathExpression(path, arg));
        }
        else {
          if (arg.NodeType == ExpressionType.New) {
            Dictionary<string, Segment<int>> fm;
            Dictionary<string, ResultMapping> jr;
            Dictionary<string, Expression> ap;
            using (new ParameterScope()) {
              pFm.Value = new Dictionary<string, Segment<int>>();
              pJr.Value = new Dictionary<string, ResultMapping>();
              pAp.Value = new Dictionary<string, Expression>();
              newArg = VisitNew((NewExpression)arg);
              fm = pFm.Value;
              jr = pJr.Value;
              ap = pAp.Value;
            }
            foreach (var p in fm)
              pFm.Value.Add(memberName + "." + p.Key, p.Value);
            foreach (var p in jr)
              pJr.Value.Add(memberName + "." + p.Key, p.Value);
            foreach (var p in ap)
              pAp.Value.Add(memberName + "." + p.Key, p.Value);
            pJr.Value.Add(memberName, new ResultMapping(fm, jr, ap));
            pAp.Value.Add(memberName, newArg);
          }
          else {
            // TODO: Add check of queries
            var le = context.MemberAccessReplacer.ProcessCalculated(Expression.Lambda(arg, parameters));
            var ccd = new CalculatedColumnDescriptor(context.GetNextColumnAlias(), arg.Type, (Expression<Func<Tuple, object>>) le);
            calculatedColumns.Add(ccd);
            int position = context.GetBound(parameters[0]).RecordSet.Header.Columns.Count + calculatedColumns.Count - 1;
            var method = genericAccessor.MakeGenericMethod(arg.Type);
            newArg = Expression.Call(tuple, method, Expression.Constant(position));
            pFm.Value.Add(memberName, new Segment<int>(position, 1));
          }
        }
        newArg = newArg ?? Visit(arg);
        arguments.Add(newArg);
      }
      return Expression.New(n.Constructor, arguments, n.Members);
    }

    private static IEnumerable<TResult> MakeProjection<TResult>(RecordSet rs, Expression<Func<Tuple, Record, TResult>> le)
    {
      var func = le.Compile();
      foreach (var r in rs.Parse())
        yield return func(r.Data, r);
    }


    // Constructors

    public ProjectionBuilder(TranslatorContext context)
      : base(context.Model)
    {
      this.context = context;
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