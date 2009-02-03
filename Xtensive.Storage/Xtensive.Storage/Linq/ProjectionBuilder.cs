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
    private Dictionary<string, Segment<int>> fieldsMapping;
    private Dictionary<Expression, string> prefixMap;
    private ProjectionParameterRewriter parameterRewriter;
    private ParameterExpression parameter;
    private static readonly MethodInfo transformApplyMethod;
    private static readonly MethodInfo keyCreateMethod;
    private static readonly MethodInfo selectMethod;
    private static readonly MethodInfo genericAccessor;
    private static readonly MethodInfo nonGenericAccessor;
    private static readonly MethodInfo recordKeyAccessor;
    private static readonly MethodInfo keyResolveMethod;

    public ResultExpression Build(ResultExpression source, LambdaExpression le)
    {
      parameter = le.Parameters[0];
      var body = le.Body;
      prefixMap = new Dictionary<Expression, string>();
      this.source = translator.MemberAccessBasedJoiner.Process(source, body, true);
      tuple = Expression.Parameter(typeof (Tuple), "t");
      record = Expression.Parameter(typeof (Record), "r");
      parameterRewriter = new ProjectionParameterRewriter(tuple, record);
      tupleIsUsed = false;
      recordIsUsed = false;
      recordSet = this.source.RecordSet;
      fieldsMapping = new Dictionary<string, Segment<int>>();
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
      var mapping = new ResultMapping(fieldsMapping, new Dictionary<string, ResultMapping>());
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
          var structurePath = MemberPath.Parse(m, translator.Model);
          var structureSegment = source.GetMemberSegment(structurePath);
          int groupIndex = source.RecordSet.Header.ColumnGroups.GetGroupIndexBySegment(structureSegment);
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
        var keySegment = source.GetMemberSegment(keyPath);
        var keyColumn = (MappedColumn) source.RecordSet.Header.Columns[keySegment.Offset];
        var type = keyColumn.ColumnInfoRef.Resolve(translator.Model).Field.ReflectedType;
//        var type = translator.Model.Types[m.Expression.Type];
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
      return parameterRewriter.Rewrite(source.ItemProjector.Body, out recordIsUsed);
    }

    protected override Expression VisitNew(NewExpression n)
    {
      var arguments = new List<Expression>();
      string prefix = null;
      prefixMap.TryGetValue(n, out prefix);
      if (n.Members == null)
        throw new NotSupportedException(n.ToSharpString());
      for (int i = 0; i < n.Arguments.Count; i++) {
        var arg = n.Arguments[i];
        var newArg = (Expression) null;
        var member = n.Members[i];
        var memberName = member.Name.Replace(WellKnown.GetterPrefix, string.Empty);
        var newName = prefix != null ? prefix + "." + memberName : memberName;
        var path = MemberPath.Parse(arg, translator.Model);
        if (path.IsValid) {
          var segment = source.GetMemberSegment(path);
          var mapping = source.Mapping.Fields.Where(p => p.Value.Offset >= segment.Offset && p.Value.EndOffset <= segment.EndOffset).ToList();
          var oldName = mapping.Select(pair => pair.Key).OrderBy(s => s.Length).First();
          var startsWithOldName = mapping.All(pair => pair.Key.StartsWith(oldName));
          if (startsWithOldName) {
            mapping = mapping.Select(pair => new KeyValuePair<string, Segment<int>>(pair.Key.Replace(oldName, newName), pair.Value)).ToList();
          }
          else {
            mapping = mapping.Select(pair => new KeyValuePair<string, Segment<int>>(newName + "." + pair.Key, pair.Value)).ToList();
            mapping.Add(new KeyValuePair<string, Segment<int>>(newName, segment));
            if (prefix != null)
              mapping.Add(new KeyValuePair<string, Segment<int>>(prefix, segment));
          }
          
          foreach (var pair in mapping) {
            if(!fieldsMapping.ContainsKey(pair.Key))
              fieldsMapping.Add(pair.Key, pair.Value);
          }
        }
        else {
          if (arg.NodeType == ExpressionType.New) {
            prefixMap.Add(arg, newName);
          }
          else {
            // TODO: Add check of queries
            var le = translator.MemberAccessReplacer.ProcessSelector(source, Expression.Lambda(arg, parameter));
            var ccd = new CalculatedColumnDescriptor(translator.GetNextAlias(), arg.Type, (Expression<Func<Tuple, object>>) le);
            recordSet = recordSet.Calculate(ccd);
            int position = recordSet.Header.Columns.Count - 1;
            var method = genericAccessor.MakeGenericMethod(arg.Type);
            tupleIsUsed = true;
            newArg = Expression.Call(tuple, method, Expression.Constant(position));
            fieldsMapping.Add(newName, new Segment<int>(position, 1));
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