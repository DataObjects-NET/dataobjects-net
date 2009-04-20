// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.11

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Linq.Expressions.Mappings;

namespace Xtensive.Storage.Linq.Expressions
{
  [DebuggerDisplay("ItemProjector = {ItemProjector}, RecordSet = {RecordSet}, IsScalar = {IsScalar}")]
  internal class ResultExpression : Expression
  {
    private object projectionDelegate;
    public RecordSet RecordSet { get; private set; }
    public Mapping Mapping { get; private set; }
    public LambdaExpression ItemProjector { get; private set; }
    public LambdaExpression ScalarTransform { get; private set; }

    public bool IsScalar
    {
      get { return ScalarTransform != null; }
    }

    private Func<RecordSet, TResult> GetProjector<TResult>()
    {
      if (projectionDelegate == null) lock (this) if (projectionDelegate == null) {
        var rs = Parameter(typeof(RecordSet), "rs");
        var itemProjector = ItemProjector;
        var severalArguments = itemProjector.Parameters.Count > 1;
        var method = severalArguments
          ? typeof(Translator)
            .GetMethod("MakeProjection", BindingFlags.NonPublic | BindingFlags.Static)
            .MakeGenericMethod(itemProjector.Body.Type)
          : WellKnownMembers.EnumerableSelect.MakeGenericMethod(itemProjector.Parameters[0].Type, itemProjector.Body.Type);
        Expression body = (!severalArguments && itemProjector.Parameters[0].Type == typeof(Record))
          ? Call(method, Call(WellKnownMembers.RecordSetParse, rs), itemProjector)
          : Call(method, rs, itemProjector);
        body = IsScalar
          ? Invoke(ScalarTransform, body)
          : (body.Type == typeof(TResult) ? body : Convert(body, typeof(TResult)));
        var projector = Lambda<Func<RecordSet, TResult>>(Convert(body, typeof(TResult)), rs);
        projectionDelegate = projector.Compile();
      }
      return (Func<RecordSet, TResult>)projectionDelegate;
    }

    public TResult GetResult<TResult>()
    {
      var projector = GetProjector<TResult>();
      return projector(RecordSet);
    }

    
    // Constructors

    public ResultExpression(
      Type type,
      RecordSet recordSet,
      Mapping mapping,
      LambdaExpression itemProjector)
      : this(type, recordSet, mapping, itemProjector, null)
    {}

    public ResultExpression(
      Type type,
      RecordSet recordSet,
      Mapping mapping,
      LambdaExpression itemProjector,
      LambdaExpression scalarTransform)
      : base((ExpressionType)ExtendedExpressionType.Result, type)
    {
      RecordSet = recordSet;
      Mapping = mapping;
      ItemProjector = itemProjector;
      ScalarTransform = scalarTransform;
    }
  }
}