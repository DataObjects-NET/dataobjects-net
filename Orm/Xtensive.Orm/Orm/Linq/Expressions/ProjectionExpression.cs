// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.11

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Linq.Expressions
{
  internal class ProjectionExpression : ExtendedExpression
  {
    public ItemProjectorExpression ItemProjector { get; private set;}
    public ResultAccessMethod ResultAccessMethod { get; private set; }
    public Dictionary<Parameter<Tuple>, Tuple> TupleParameterBindings { get; private set; }

    public bool IsScalar
    {
      get {  return ResultAccessMethod != ResultAccessMethod.All; }
    }

    public override string ToString()
    {
      return string.Format("Projection:  {0}, IsScalar = {1}", ItemProjector, IsScalar);
    }


    // Constructors

    public ProjectionExpression(
      Type type,
      ItemProjectorExpression itemProjectorExpression,
      Dictionary<Parameter<Tuple>, Tuple> tupleParameterBindings)
      : this(type, itemProjectorExpression, tupleParameterBindings, ResultAccessMethod.All)
    {}

    public ProjectionExpression(
      Type type, 
      ItemProjectorExpression itemProjectorExpression, 
      Dictionary<Parameter<Tuple>, Tuple> tupleParameterBindings, 
      ResultAccessMethod resultAccessMethod)
      : base(ExtendedExpressionType.Projection, type)
    {
      ItemProjector = itemProjectorExpression;
      ResultAccessMethod = resultAccessMethod;
      TupleParameterBindings = new Dictionary<Parameter<Tuple>, Tuple>(tupleParameterBindings);
    }
  }
}