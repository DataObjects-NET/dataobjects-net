// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.07.07

using System;
using System.Diagnostics;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Core.Resources;


namespace Xtensive.Core.Tuples.Transform
{
  /// <summary>
  /// Cuts out specified <see cref="Segment"/> from the <see cref="Tuple"/>.
  /// </summary>
  public sealed class CutOutTransform : MapTransform
  {
    private Segment<int> segment;

    /// <summary>
    /// Gets the segment this transform cuts out.
    /// </summary>
    [DebuggerStepThrough]
    public Segment<int> Segment
    {
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
      string description = String.Format("{0}, {1}",
        segment,
        IsReadOnly ? Strings.ReadOnlyShort : Strings.ReadWriteShort);
      return String.Format(Strings.TupleTransformFormat,
        GetType().GetShortName(),
        description);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="isReadOnly"><see cref="MapTransform.IsReadOnly"/> property value.</param>
    /// <param name="sourceDescriptor">Source tuple descriptor.</param>
    /// <param name="segment">The segment to cut out.</param>
    public CutOutTransform(bool isReadOnly, TupleDescriptor sourceDescriptor, Segment<int> segment)
      : base(isReadOnly)
    {
      this.segment = segment;
      Type[] fields = new Type[sourceDescriptor.Count - segment.Length];
      int[] map = new int[sourceDescriptor.Count - segment.Length];
      int j = segment.Offset;
      bool flag = false;
      if (sourceDescriptor.Count >= j + segment.Length)
      for (int i = 0; i < sourceDescriptor.Count - segment.Length; i++) {
        if ((i < j)) {
          fields[i] = sourceDescriptor[i];
          map[i] = i;
        }
        if (i == j) {
          flag = true;
          j += segment.Length;
        }
        if (flag)
        {
          fields[i] = sourceDescriptor[j];
          map[i] = j;
          j++;
        }
      }
      else 
        throw new InvalidOperationException("Segment is out of range.");
      Descriptor = TupleDescriptor.Create(fields);
      SingleSourceMap = map;
    }
    
  }
}