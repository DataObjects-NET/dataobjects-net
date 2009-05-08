// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.11

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Linq.Expressions.Mappings;

namespace Xtensive.Storage.Linq.Expressions
{
  [DebuggerDisplay("ItemProjector = {ItemProjector}, RecordSet = {RecordSet}, IsScalar = {IsScalar}")]
  internal class ResultExpression : Expression
  {
    private object projectionDelegate;
    public RecordSet RecordSet { get; private set; }
    public IMapping Mapping { get; private set; }
    public LambdaExpression ItemProjector { get; private set; }
    public ResultType ResultType { get; private set; }
    
    public bool IsScalar
    {
      get {  return ResultType != ResultType.All; }
    }

    public TResult GetResult<TResult>()
    {
      var projector = GetProjector<TResult>();
      return projector(RecordSet);
    }

    #region Private methods...

    private Func<RecordSet, TResult> GetProjector<TResult>()
    {
      if (projectionDelegate == null) lock (this) if (projectionDelegate == null) {
        var rs = Parameter(typeof(RecordSet), "rs");
        var itemProjector = ItemProjector;
        var severalArguments = itemProjector.Parameters.Count > 1;
        var elementType = itemProjector.Body.Type;
        var method = severalArguments
          ? typeof(ResultExpression)
            .GetMethod("MakeProjection", BindingFlags.NonPublic | BindingFlags.Static)
            .MakeGenericMethod(elementType)
          : WellKnownMembers.EnumerableSelect.MakeGenericMethod(itemProjector.Parameters[0].Type, elementType);
        Expression body = (!severalArguments && itemProjector.Parameters[0].Type == typeof(Record))
          ? Call(method, Call(WellKnownMembers.RecordSetParse, rs), itemProjector)
          : Call(method, rs, itemProjector);
        if (IsScalar) {
          var scalarMethodName = ResultType.ToString();
          var enumerableMethod = typeof (Enumerable)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(m => m.Name==scalarMethodName && m.GetParameters().Length==1)
            .MakeGenericMethod(elementType);
          body = Call(enumerableMethod, body);
        }
        else
          body = body.Type == typeof (TResult)
            ? body
            : Convert(body, typeof (TResult));
        var projector = Lambda<Func<RecordSet, TResult>>(body, rs);
        projectionDelegate = projector.CompileCached();
      }
      return (Func<RecordSet, TResult>)projectionDelegate;
    }

    private static IEnumerable<TResult> MakeProjection<TResult>(RecordSet rs, Expression<Func<Tuple, Record, TResult>> le)
    {
      var func = le.CompileCached();
      foreach (var r in rs.Parse())
        yield return func(r.Data, r);
    }

    #endregion

    
    // Constructors

    public ResultExpression(
      Type type,
      RecordSet recordSet,
      IMapping mapping,
      LambdaExpression itemProjector)
      : this(type, recordSet, mapping, itemProjector, ResultType.All)
    {}

    public ResultExpression(
      Type type,
      RecordSet recordSet,
      IMapping mapping,
      LambdaExpression itemProjector,
      ResultType resultType)
      : base((ExpressionType)ExtendedExpressionType.Result, type)
    {
      RecordSet = recordSet;
      Mapping = mapping;
      ItemProjector = itemProjector;
      ResultType = resultType;
    }
  }
}