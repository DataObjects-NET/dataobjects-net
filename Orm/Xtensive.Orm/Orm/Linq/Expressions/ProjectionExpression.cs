// Copyright (C) 2008-2021 Xtensive LLC.
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
    public ItemProjectorExpression ItemProjector { get; }
    public ResultAccessMethod ResultAccessMethod { get; }
    public IReadOnlyDictionary<Parameter<Tuple>, Tuple> TupleParameterBindings { get; }

    public bool IsScalar => ResultAccessMethod != ResultAccessMethod.All;

    public override string ToString() => $"Projection:  {ItemProjector}, IsScalar = {IsScalar}";

    /// <summary>
    /// Creates new <see cref="ProjectionExpression"/> based on this instance. New projection
    /// inherits all properties but <see cref="ProjectionExpression.ItemProjector"/>, which is
    /// replaced by the given <paramref name="itemProjectorExpression"/>.
    /// </summary>
    /// <param name="itemProjectorExpression">Replacement of <see cref="ProjectionExpression.ItemProjector"/>.</param>
    /// <returns>New instance with replaced item projector.</returns>
    public ProjectionExpression ApplyItemProjector(ItemProjectorExpression itemProjectorExpression) =>
      new ProjectionExpression(Type, itemProjectorExpression, TupleParameterBindings, ResultAccessMethod);

    // Constructors

    public ProjectionExpression(
      Type type,
      ItemProjectorExpression itemProjectorExpression,
      IReadOnlyDictionary<Parameter<Tuple>, Tuple> tupleParameterBindings,
      ResultAccessMethod resultAccessMethod = ResultAccessMethod.All)
      : base(ExtendedExpressionType.Projection, type)
    {
      ItemProjector = itemProjectorExpression;
      ResultAccessMethod = resultAccessMethod;
      TupleParameterBindings = tupleParameterBindings;
    }
  }
}
