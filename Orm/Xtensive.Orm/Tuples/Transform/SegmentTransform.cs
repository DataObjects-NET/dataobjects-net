// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.20

using System;
using System.Diagnostics;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Internals.DocTemplates;


namespace Xtensive.Tuples.Transform
{
  /// <summary>
  /// Extracts specified <see cref="Segment"/> from the <see cref="Tuple"/>.
  /// </summary>
  public sealed class SegmentTransform : MapTransform
  {
    private Segment<int> segment;

    /// <summary>
    /// Gets the segment this transform extracts.
    /// </summary>
    public Segment<int> Segment
    {
      [DebuggerStepThrough]
      get { return segment; }
    }

    /// <see cref="MapTransform.Apply(TupleTransformType,Tuple)" copy="true" />
    public new Tuple Apply(TupleTransformType transformType, Tuple source)
    {
      return base.Apply(transformType, source);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      string description = string.Format("{0}, {1}", 
        segment, 
        IsReadOnly ? Strings.ReadOnlyShort : Strings.ReadWriteShort);
      return string.Format(Strings.TupleTransformFormat, 
        GetType().GetShortName(), 
        description);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="isReadOnly"><see cref="MapTransform.IsReadOnly"/> property value.</param>
    /// <param name="sourceDescriptor">Source tuple descriptor.</param>
    /// <param name="segment">The segment to extract.</param>
    public SegmentTransform(bool isReadOnly, TupleDescriptor sourceDescriptor, Segment<int> segment)
      : base(isReadOnly)
    {
      this.segment = segment;
      Type[] fields = new Type[segment.Length];
      int[] map = new int[segment.Length];
      for (int i = 0, j = segment.Offset; i < segment.Length; i++, j++) {
        fields[i] = sourceDescriptor[j];
        map[i] = j;
      }
      Descriptor = TupleDescriptor.Create(fields);
      SingleSourceMap = map;
    }
  }
}