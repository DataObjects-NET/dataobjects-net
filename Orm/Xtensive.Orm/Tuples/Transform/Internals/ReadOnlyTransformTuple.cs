// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.06.15

using System;
using Xtensive.Core;


namespace Xtensive.Tuples.Transform.Internals
{
  /// <summary>
  /// A tuple wrapper for <see cref="ReadOnlyTransform"/>.
  /// </summary>
  [Serializable]
  public sealed class ReadOnlyTransformTuple : TransformedTuple
  {
    private readonly Tuple origin;

    /// <inheritdoc/>
    public override TupleDescriptor Descriptor => origin.Descriptor;

    /// <inheritdoc />
    public override int Count => origin.Count;

    /// <inheritdoc/>
    public override TupleFieldState GetFieldState(int fieldIndex) => origin.GetFieldState(fieldIndex);

    /// <inheritdoc/>
    public override object GetValue(int fieldIndex, out TupleFieldState fieldState) => origin.GetValue(fieldIndex, out fieldState);

    protected internal override void SetFieldState(int fieldIndex, TupleFieldState fieldState) => Exceptions.ObjectIsReadOnly(null);

    /// <inheritdoc />
    public override void SetValue(int fieldIndex, object fieldValue) => throw Exceptions.ObjectIsReadOnly(null);

    protected internal override Pair<Tuple, int> GetMappedContainer(int fieldIndex, bool isWriting)
    {
      if (isWriting)
        throw Exceptions.ObjectIsReadOnly(null);
      return origin.GetMappedContainer(fieldIndex, isWriting);
    }

    /// <inheritdoc/>
    public override bool Equals(Tuple other) => origin.Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => origin.GetHashCode();


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="tuple">Tuple to provide read-only wrapper for.</param>
    public ReadOnlyTransformTuple(Tuple tuple)
    {
      ArgumentValidator.EnsureArgumentNotNull(tuple, nameof(tuple));
      origin = tuple;
    }
  }
}
