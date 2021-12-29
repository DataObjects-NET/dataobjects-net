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
  internal sealed class MapTransformTuple : Tuple, ITransformedTuple
  {
    private readonly MapTransform transform;
    private readonly Tuple source;
    private Tuple defaultResult;

    /// <summary>
    /// Gets the default result tuple.
    /// Can be used to get default values for the result tuple fields.
    /// </summary>
    private Tuple DefaultResult => defaultResult ??= Tuple.Create(transform.Descriptor);

    /// <inheritdoc/>
    public override TupleDescriptor Descriptor => transform.Descriptor;

    #region GetFieldState, GetValue, SetValue methods

    /// <inheritdoc/>
    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      var index = GetSourceFieldIndex(fieldIndex);
      return index == TransformUtil.NoMapping ? TupleFieldState.Default : source.GetFieldState(index);
    }

    protected internal override void SetFieldState(int fieldIndex, TupleFieldState fieldState)
    {
      var index = GetSourceFieldIndex(fieldIndex);
      if (index == TransformUtil.NoMapping) {
        return;
      }
      source.SetFieldState(index, fieldState);
    }

    /// <inheritdoc/>
    public override object GetValue(int fieldIndex, out TupleFieldState fieldState)
    {
      int index = GetSourceFieldIndex(fieldIndex);
      return index == TransformUtil.NoMapping
        ? DefaultResult.GetValue(fieldIndex, out fieldState)
        : source.GetValue(index, out fieldState);
    }

    /// <inheritdoc/>
    public override void SetValue(int fieldIndex, object fieldValue)
    {
      if (transform.IsReadOnly) {
        throw Exceptions.ObjectIsReadOnly(null);
      }
      source.SetValue(GetSourceFieldIndex(fieldIndex), fieldValue);
    }

    #endregion

    private int GetSourceFieldIndex(int fieldIndex)
    {
      var mappedIndex = transform.Map[fieldIndex];
      return mappedIndex < 0 ? TransformUtil.NoMapping : mappedIndex;
    }

    protected internal override Pair<Tuple, int> GetMappedContainer(int fieldIndex, bool isWriting)
    {
      if (isWriting && transform.IsReadOnly) {
        throw Exceptions.ObjectIsReadOnly(null);
      }
      var index = GetSourceFieldIndex(fieldIndex);
      return index == TransformUtil.NoMapping ? default : source.GetMappedContainer(index, isWriting);
    }

    /// <inheritdoc/>
    public override string ToString() =>
      string.Format(Strings.TransformedTupleFormat, base.ToString(), transform, source);


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="transform">The transform.</param>
    /// <param name="source">Source tuple.</param>
    public MapTransformTuple(MapTransform transform, Tuple source)
    {
      this.transform = transform;
      this.source = source;
    }
  }
}