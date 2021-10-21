// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.04.30

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core;
using Xtensive.Reflection;


namespace Xtensive.Tuples.Transform
{
  /// <summary>
  /// This class is used for source <see cref="Tuple"/>s combining.
  /// </summary>
  [Serializable]
  public sealed class CombineTransform : MapTransform
  {
    private TupleDescriptor[] sources;

    /// <summary>
    /// Gets tuple descriptors this transform merges.
    /// </summary>
    public IReadOnlyList<TupleDescriptor> Sources
    {
      [DebuggerStepThrough]
      get => Array.AsReadOnly(sources);
    }

    /// <see cref="MapTransform.Apply(TupleTransformType,Tuple,Tuple)" copy="true" />
    public new Tuple Apply(TupleTransformType transformType, Tuple source1, Tuple source2)
    {
      return base.Apply(transformType, source1, source2);
    }

    /// <see cref="MapTransform.Apply(TupleTransformType,Tuple,Tuple,Tuple)" copy="true" />
    public new Tuple Apply(TupleTransformType transformType, Tuple source1, Tuple source2, Tuple source3)
    {
      return base.Apply(transformType, source1, source2, source3);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      string description = $"{sources.ToDelimitedString(" + ")}, {(IsReadOnly ? Strings.ReadOnlyShort : Strings.ReadWriteShort)}";
      return string.Format(Strings.TupleTransformFormat, 
        GetType().GetShortName(), 
        description);
    }


    // Constructors
    
    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="isReadOnly"><see cref="MapTransform.IsReadOnly"/> property value.</param>
    /// <param name="sources">Source tuple descriptors.</param>
    public CombineTransform(bool isReadOnly, params TupleDescriptor[] sources)
      : base(isReadOnly)
    {
      int totalLength = sources.Sum(s => s.Count);
      var types = new Type[totalLength];
      var map = new Pair<int, int>[totalLength];
      int index = 0;
      for (int i = 0; i<sources.Length; i++) {
        TupleDescriptor currentDescriptor = sources[i];
        int currentCount = currentDescriptor.Count;
        for (int j = 0; j<currentCount; j++) {
          types[index] = currentDescriptor[j];
          map[index++] = new Pair<int, int>(i, j);
        }
      }
      this.sources = sources;
      Descriptor = TupleDescriptor.Create(types);
      SetMap(map);
    }
  }
}