// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.11

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Linq.Expressions
{
  internal class ProjectionExpression : ExtendedExpression
  {
    private readonly Dictionary<Parameter<Tuple>, Tuple> tupleParameterBindings = new Dictionary<Parameter<Tuple>, Tuple>();
    public ItemProjectorExpression ItemProjector { get; private set;}
    public ResultType ResultType { get; private set; }

    public Dictionary<Parameter<Tuple>, Tuple> TupleParameterBindings
    {
      get { return tupleParameterBindings; }
    }

    public bool IsScalar
    {
      get {  return ResultType != ResultType.All; }
    }

    public override string ToString()
    {
      return string.Format("Projection:  {0}, IsScalar = {1}", ItemProjector, IsScalar);
    }

    public Tuple GetParameterTuple(Parameter<Tuple> parameter)
    {
      Tuple tuple;
      if (tupleParameterBindings.TryGetValue(parameter, out tuple))
        return tuple;
      return null;
    }

    // Constructors

    public ProjectionExpression(
      Type type,
      ItemProjectorExpression itemProjectorExpression, 
      IDictionary<Parameter<Tuple>, Tuple> tupleParameterBindings)
      : this(type, itemProjectorExpression, ResultType.All, tupleParameterBindings)
    {}

    public ProjectionExpression(
      Type type,
      ItemProjectorExpression itemProjectorExpression,
      ResultType resultType, 
      IDictionary<Parameter<Tuple>, Tuple> tupleParameterBindings)
      : base(ExtendedExpressionType.Projection, type)
    {
      ItemProjector = itemProjectorExpression;
      ResultType = resultType;
      this.tupleParameterBindings = new Dictionary<Parameter<Tuple>, Tuple>(tupleParameterBindings);
    }
  }
}