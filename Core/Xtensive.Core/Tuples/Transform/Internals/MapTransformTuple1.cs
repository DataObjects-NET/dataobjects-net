// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.04

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Tuples.Transform.Internals
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

    #region GetFieldState, GetValue, SetValue methods

    /// <inheritdoc/>
    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      int index = GetMappedFieldIndex(fieldIndex);
      return index == MapTransform.NoMapping ? TupleFieldState.Default : tuple.GetFieldState(index);
    }

    protected internal override void SetFieldState(int fieldIndex, TupleFieldState fieldState)
    {
      int index = GetMappedFieldIndex(fieldIndex);
      if (index == MapTransform.NoMapping) 
        return;
      tuple.SetFieldState(index, fieldState);
    }

    /// <inheritdoc/>
    public override object GetValue(int fieldIndex, out TupleFieldState fieldState)
    {
      int index = GetMappedFieldIndex(fieldIndex);
      if (index==MapTransform.NoMapping)
        return TypedTransform.DefaultResult.GetValue(fieldIndex, out fieldState);
      return tuple.GetValue(index, out fieldState);
    }

    /// <inheritdoc/>
    public override void SetValue(int fieldIndex, object fieldValue)
    {
      if (Transform.IsReadOnly)
        throw Exceptions.ObjectIsReadOnly(null);
      tuple.SetValue(GetMappedFieldIndex(fieldIndex), fieldValue);
    }

    #endregion

    private int GetMappedFieldIndex(int fieldIndex)
    {
      int mappedIndex = TypedTransform.singleSourceMap[fieldIndex];
      return mappedIndex < 0 ? MapTransform.NoMapping :mappedIndex;
    }

    public override Pair<Tuple, int> GetMappedContainer(int fieldIndex, bool isWriting)
    {
      if (isWriting && Transform.IsReadOnly)
        throw Exceptions.ObjectIsReadOnly(null);
      var index = GetMappedFieldIndex(fieldIndex);
      return index == MapTransform.NoMapping 
        ? new Pair<Tuple, int>() 
        : tuple.GetMappedContainer(index, isWriting);
    }


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