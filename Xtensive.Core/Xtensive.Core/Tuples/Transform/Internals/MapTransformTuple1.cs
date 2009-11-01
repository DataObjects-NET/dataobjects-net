// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.04

using System;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Tuples.Transform.Internals
{
  /// <summary>
  /// A <see cref="MapTransform"/> result tuple mapping 1 source tuple to a single one (this).
  /// </summary>
  [Serializable]
  public sealed class MapTransformTuple1 : TransformedTuple<MapTransform>
  {
    private Tuple tuple;

    /// <inheritdoc/>
    public override object[] Arguments {
      get {
        Tuple[] copy = new Tuple[1];
        copy[0] = tuple;
        return copy;
      }
    }

    #region GetFieldState, GetValueOrDefault, SetValue methods

    /// <inheritdoc/>
    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      return tuple.GetFieldState(TypedTransform.singleSourceMap[fieldIndex]);
    }

    /// <inheritdoc/>
    public override object GetValueOrDefault(int fieldIndex)
    {
      return tuple.GetValueOrDefault(TypedTransform.singleSourceMap[fieldIndex]);
    }

    /// <inheritdoc/>
    public override T GetValueOrDefault<T>(int fieldIndex)
    {
      return tuple.GetValueOrDefault<T>(TypedTransform.singleSourceMap[fieldIndex]);
    }

    /// <inheritdoc/>
    public override void SetValue<T>(int fieldIndex, T fieldValue)
    {
      if (Transform.IsReadOnly)
        throw Exceptions.ObjectIsReadOnly(null);
      tuple.SetValue(TypedTransform.singleSourceMap[fieldIndex], fieldValue);
    }

    /// <inheritdoc/>
    public override void SetValue(int fieldIndex, object fieldValue)
    {
      if (Transform.IsReadOnly)
        throw Exceptions.ObjectIsReadOnly(null);
      tuple.SetValue(TypedTransform.singleSourceMap[fieldIndex], fieldValue);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="transform">The transform.</param>
    /// <param name="source">Source tuple.</param>
    public MapTransformTuple1(MapTransform transform, Tuple source)
      : base(transform)
    {
      tuple = source;
    }
  }
}