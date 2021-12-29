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
  public sealed class SegmentTransform// : ITupleTransform
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
    /// either <see cref="SegmentTransformTuple"/> instance or the <see cref="RegularTuple"/> descendant,
    /// dependently on specified <paramref name="transformType"/>.</returns>
    public Tuple Apply(TupleTransformType transformType, Tuple source)
    {
      return transformType switch {
        TupleTransformType.Auto when source is ITransformedTuple => CopySourceSegment(source),
        TupleTransformType.Auto => new SegmentTransformTuple(this, source),
        TupleTransformType.Tuple => CopySourceSegment(source),
        TupleTransformType.TransformedTuple => new SegmentTransformTuple(this, source),
        _ => throw new ArgumentOutOfRangeException(nameof(transformType))
      };

      Tuple CopySourceSegment(Tuple source) {
        var result = Tuple.Create(Descriptor);
        source.CopyTo(result, segment.Offset, segment.Length);
        return result;
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
    /// <param name="isReadOnly">Indicates whethere the transformed <see cref="Tuple"/> is read only.</param>
    /// <param name="sourceDescriptor">The <see cref="TupleDescriptor"/> of the source <see cref="Tuple"/>.</param>
    /// <param name="segment">The segment to extract.</param>
    public SegmentTransform(bool isReadOnly, TupleDescriptor sourceDescriptor, in Segment<int> segment)
    {
      IsReadOnly = isReadOnly;

      var fields = new ArraySegment<Type>(sourceDescriptor.FieldTypes, segment.Offset, segment.Length);
      Descriptor = TupleDescriptor.Create(fields.ToArray());
      this.segment = segment;
    }
  }
}