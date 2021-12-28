// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.20

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Reflection;



namespace Xtensive.Tuples.Transform
{
  /// <summary>
  /// Extracts specified <see cref="Segment"/> from the <see cref="Tuple"/>.
  /// </summary>
  public sealed class SegmentTransform : ITupleTransform
  {
    private readonly SingleSourceMapTransform mapTransform;
    private readonly Segment<int> segment;

    /// <inheritdoc/>
    public TupleDescriptor Descriptor => mapTransform.Descriptor;

    /// <inheritdoc/>
    public bool IsReadOnly => mapTransform.IsReadOnly;

    /// <summary>
    /// Gets the segment this transform extracts.
    /// </summary>
    public Segment<int> Segment
    {
      [DebuggerStepThrough]
      get { return segment; }
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
      return mapTransform.Apply(transformType, source);
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

    private static TupleDescriptor CreateDescriptorAndMap(TupleDescriptor sourceDescriptor, in Segment<int> segment, out int[] map)
    {
      var fields = new Type[segment.Length];
      map = new int[segment.Length];
      for (int i = 0, j = segment.Offset; i < segment.Length; i++, j++) {
        fields[i] = sourceDescriptor[j];
        map[i] = j;
      }
      return TupleDescriptor.Create(fields);
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="isReadOnly"><see cref="ITupleTransform.IsReadOnly"/> property value.</param>
    /// <param name="sourceDescriptor">Source tuple descriptor.</param>
    /// <param name="segment">The segment to extract.</param>
    public SegmentTransform(bool isReadOnly, TupleDescriptor sourceDescriptor, in Segment<int> segment)
    {
      mapTransform = new SingleSourceMapTransform(isReadOnly, CreateDescriptorAndMap(sourceDescriptor, segment, out var map), map);
      this.segment = segment;
    }
  }
}