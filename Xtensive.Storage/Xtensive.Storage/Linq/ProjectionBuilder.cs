// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2009.01.13

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Linq;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Expressions.Visitors;
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
    private static readonly MethodInfo selectMethod;
    private static readonly MethodInfo genericAccessor;
    private static readonly MethodInfo nonGenericAccessor;

    public ResultExpression Build(ResultExpression source, Expression body)
    {
      this.source = translator.FieldAccessBasedJoiner.Process(source, body);
      tuple = Expression.Parameter(typeof (Tuple), "t");
      record = Expression.Parameter(typeof (Record), "r");
      tupleIsUsed = false;
      recordIsUsed = false;
      recordSet = this.source.RecordSet;
      mapping = source.Mapping;
      Expression<Func<RecordSet, object>> lambda = null;

      var newBody = Visit(body);
      if (recordIsUsed) {
        // TODO: implement
      }
      else {
        var rs = Expression.Parameter(typeof(RecordSet), "rs");
        var method = selectMethod.MakeGenericMethod(typeof (Tuple), newBody.Type);
        lambda = Expression.Lambda<Func<RecordSet, object>>(Expression.Convert(Expression.Call(null, method, rs, Expression.Lambda(newBody, tuple)), typeof(object)), rs);
      }
      return new ResultExpression(body.Type, recordSet, mapping, lambda);
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
          // TODO: implement
        }
        else {
          // TODO: implement
        }
        throw new NotImplementedException();
      }
      else if (isKey) {
        // TODO: implement
      }
      var path = translator.FieldAccessReplacer.GetAccessPath(m);
      var method = m.Type == typeof(object) ? 
        nonGenericAccessor : 
        genericAccessor.MakeGenericMethod(m.Type);
      var segment = source.GetFieldSegment(path);
      tupleIsUsed = true;
      return Expression.Call(tuple, method, Expression.Constant(segment.Offset));
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