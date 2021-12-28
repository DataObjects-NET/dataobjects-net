// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.20

using System;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Tuples.Transform.Internals;

namespace Xtensive.Tuples.Transform
{
  /// <summary>
  /// Extracts specified <see cref="Segment"/> from the <see cref="Tuple"/>.
  /// </summary>
  public sealed class SegmentTransform : ITupleTransform
  {
    private readonly Segment<int> segment;

    /// <inheritdoc/>
    public TupleDescriptor Descriptor { get; }

    /// <inheritdoc/>
    public bool IsReadOnly { get; }

    /// <summary>
    /// Gets the segment this transform extracts.
    /// </summary>
    public ref readonly Segment<int> Segment
    {
      get { return ref segment; }
    }

    /// <summary>
    /// Applies the transformation.
    /// </summary>
    /// <param name="transformType">The type of transformation to perform.</param>
    /// <param name="source">Transformation source.</param>
    /// <returns>Transformation result - 
    /// either <see cref="TransformedTuple{TTupleTransform}"/> or <see cref="Tuple"/> descendant,
    /// dependently on specified <paramref name="transformType"/>.</returns>
    public Tuple Apply(TupleTransformType transformType, Tuple source)
    {
      switch (transformType) {
        case TupleTransformType.Auto:
          if (source is ITransformedTuple)
            goto case TupleTransformType.Tuple;
          goto case TupleTransformType.TransformedTuple;
        case TupleTransformType.TransformedTuple:
          return new SegmentTransformTuple(this, source);
        case TupleTransformType.Tuple:
          Tuple result = Tuple.Create(Descriptor);
          source.CopyTo(result, segment.Offset, segment.Length);
          return result;
        default:
          throw new ArgumentOutOfRangeException(nameof(transformType));
      }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      string description = $"{segment}, {(IsReadOnly ? Strings.ReadOnlyShort : Strings.ReadWriteShort)}";
      return string.Format(Strings.TupleTransformFormat, 
        GetType().GetShortName(), 
        description);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="isReadOnly"><see cref="ITupleTransform.IsReadOnly"/> property value.</param>
    /// <param name="sourceDescriptor">Source tuple descriptor.</param>
    /// <param name="segment">The segment to extract.</param>
    public SegmentTransform(bool isReadOnly, TupleDescriptor sourceDescriptor, in Segment<int> segment)
    {
      IsReadOnly = isReadOnly;

      var fields = new Type[segment.Length];
      for (int i = 0, j = segment.Offset; i < segment.Length; i++, j++) {
        fields[i] = sourceDescriptor[j];
      }
      Descriptor = TupleDescriptor.Create(fields);
      this.segment = segment;
    }
  }
}