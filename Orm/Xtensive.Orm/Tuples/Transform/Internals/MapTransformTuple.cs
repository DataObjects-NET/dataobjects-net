// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.07

using System;
using Xtensive.Collections;
using Xtensive.Core;


namespace Xtensive.Tuples.Transform.Internals
{
  /// <summary>
  /// A <see cref="MapTransform"/> result tuple mapping arbitrary count of source tuples to a single one (this).
  /// </summary>
  [Serializable]
  public sealed class MapTransformTuple : TransformedTuple<MapTransform>
  {
    private readonly Tuple[] tuples;

    /// <inheritdoc/>
    public override object[] Arguments {
      get {
        return tuples.Copy();
      }
    }

    #region GetFieldState, GetValue, SetValue methods

    /// <inheritdoc/>
    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      var indexes = TypedTransform.map[fieldIndex];
      return tuples[indexes.First].GetFieldState(indexes.Second);
    }

    protected internal override void SetFieldState(int fieldIndex, TupleFieldState fieldState)
    {
      var indexes = TypedTransform.map[fieldIndex];
      tuples[indexes.First].SetFieldState(indexes.Second, fieldState);
    }

    /// <inheritdoc/>
    public override object GetValue(int fieldIndex, out TupleFieldState fieldState)
    {
      var indexes = TypedTransform.map[fieldIndex];
      return tuples[indexes.First].GetValue(indexes.Second, out fieldState);
    }

    /// <inheritdoc/>
    public override void SetValue(int fieldIndex, object fieldValue)
    {
      if (Transform.IsReadOnly)
        throw Exceptions.ObjectIsReadOnly(null);
      var indexes = TypedTransform.map[fieldIndex];
      tuples[indexes.First].SetValue(indexes.Second, fieldValue);
    }

    #endregion

    protected internal override Pair<Tuple, int> GetMappedContainer(int fieldIndex, bool isWriting)
    {
      if (isWriting && Transform.IsReadOnly)
        throw Exceptions.ObjectIsReadOnly(null);
      var map = TypedTransform.map[fieldIndex];
      return tuples[map.First].GetMappedContainer(map.Second, isWriting);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="transform">The transform.</param>
    /// <param name="sources">Source tuples.</param>
    public MapTransformTuple(MapTransform transform, params Tuple[] sources)
      : base(transform)
    {
      ArgumentValidator.EnsureArgumentNotNull(sources, "tuples");
      // Other checks are omitted: this transform should be fast, so delayed errors are ok
      this.tuples = sources;
    }
  }
}