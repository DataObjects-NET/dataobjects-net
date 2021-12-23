// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.04

using System;
using Xtensive.Core;


namespace Xtensive.Tuples.Transform.Internals
{
  /// <summary>
  /// A <see cref="MapTransform"/> result tuple mapping 1 source tuple to a single one (this).
  /// </summary>
  [Serializable]
  internal sealed class MapTransformTuple1 : TransformedTuple<MapTransform>
  {
    private readonly Tuple source;
    private Tuple defaultResult;

    /// <summary>
    /// Gets the default result tuple.
    /// Can be used to get default values for the result tuple fields.
    /// </summary>
    private Tuple DefaultResult => defaultResult ??= Tuple.Create(TypedTransform.Descriptor);

    #region GetFieldState, GetValue, SetValue methods

    /// <inheritdoc/>
    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      var index = GetMappedFieldIndex(fieldIndex);
      return index == MapTransform.NoMapping ? TupleFieldState.Default : source.GetFieldState(index);
    }

    protected internal override void SetFieldState(int fieldIndex, TupleFieldState fieldState)
    {
      var index = GetMappedFieldIndex(fieldIndex);
      if (index == MapTransform.NoMapping) {
        return;
      }
      source.SetFieldState(index, fieldState);
    }

    /// <inheritdoc/>
    public override object GetValue(int fieldIndex, out TupleFieldState fieldState)
    {
      int index = GetMappedFieldIndex(fieldIndex);
      return index == MapTransform.NoMapping
        ? DefaultResult.GetValue(fieldIndex, out fieldState)
        : source.GetValue(index, out fieldState);
    }

    /// <inheritdoc/>
    public override void SetValue(int fieldIndex, object fieldValue)
    {
      if (TypedTransform.IsReadOnly) {
        throw Exceptions.ObjectIsReadOnly(null);
      }
      source.SetValue(GetMappedFieldIndex(fieldIndex), fieldValue);
    }

    #endregion

    private int GetMappedFieldIndex(int fieldIndex)
    {
      var mappedIndex = TypedTransform.singleSourceMap[fieldIndex];
      return mappedIndex < 0 ? MapTransform.NoMapping : mappedIndex;
    }

    protected internal override Pair<Tuple, int> GetMappedContainer(int fieldIndex, bool isWriting)
    {
      if (isWriting && TypedTransform.IsReadOnly) {
        throw Exceptions.ObjectIsReadOnly(null);
      }
      var index = GetMappedFieldIndex(fieldIndex);
      return index == MapTransform.NoMapping ? default : source.GetMappedContainer(index, isWriting);
    }


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="transform">The transform.</param>
    /// <param name="source">Source tuple.</param>
    public MapTransformTuple1(MapTransform transform, Tuple source)
      : base(transform)
    {
      this.source = source;
    }
  }
}