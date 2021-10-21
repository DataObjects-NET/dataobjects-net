// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
      return $"Projection:  {ItemProjector}, IsScalar = {IsScalar}";
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