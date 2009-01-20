// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2009.01.13

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Expressions.Visitors;
using Xtensive.Storage.Linq.Linq2Rse.Internal;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq.Linq2Rse
{
  internal class ProjectionBuilder : ExpressionVisitor
  {
    private readonly RseQueryTranslator translator;
    private ParameterExpression tuple;
    private ParameterExpression record;
    private bool tupleIsUsed;
    private bool recordIsUsed;
    private RecordSet recordSet;
    private TypeMapping mapping;
    private static readonly MethodInfo selectMethod;
    private static readonly MethodInfo genericAccessor;
    private static readonly MethodInfo nonGenericAccessor;

    public ProjectionExpression Build(ProjectionExpression source, Expression body)
    {
      tuple = Expression.Parameter(typeof (Tuple), "t");
      record = Expression.Parameter(typeof (Record), "r");
      tupleIsUsed = false;
      recordIsUsed = false;
      recordSet = source.RecordSet;
      mapping = source.Mapping;
      Expression<Func<RecordSet, object>> lambda = null;

      var newBody = Visit(body);
      if (tupleIsUsed || recordIsUsed) {
        
      }
      else {
        var rs = Expression.Parameter(typeof(RecordSet), "rs");
        var method = selectMethod.MakeGenericMethod(typeof (Tuple), newBody.Type);
        lambda = Expression.Lambda<Func<RecordSet, object>>(Expression.Convert(Expression.Call(null, method, rs, Expression.Lambda(newBody, tuple)), typeof(object)), rs);
      }
      return new ProjectionExpression(body.Type, recordSet, mapping, lambda);
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      if (translator.Evaluator.CanBeEvaluated(m) && translator.ParameterExtractor.IsParameter(m))
        return m;
      var path = translator.FieldAccessTranslator.Translate(m);
      var method = m.Type == typeof(object) ? nonGenericAccessor : genericAccessor.MakeGenericMethod(m.Type);
//      return Expression.Call(tuple, method, Expression.Constant(source.GetFieldSegment(path)));
      throw new NotImplementedException();
    }


    // Constructor

    public ProjectionBuilder(RseQueryTranslator translator)
    {
      this.translator = translator;
    }

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