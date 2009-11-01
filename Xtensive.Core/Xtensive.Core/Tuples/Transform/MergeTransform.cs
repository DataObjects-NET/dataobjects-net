// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.04.30

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Core.Resources;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Core.Tuples.Transform.Internals;

namespace Xtensive.Core.Tuples.Transform
{
  /// <summary>
  /// This class is used for merge of source <see cref="Tuple"/>.
  /// </summary>
  [Serializable]
  public sealed class MergeTransform : MapTransform
  {
    private TupleDescriptor[] sources;

    /// <summary>
    /// Gets tuple descriptors this transform merges.
    /// </summary>
    public TupleDescriptor[] Sources
    {
      get { return sources.Copy(); }
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
      string description = String.Format("{0}, {1}", 
        sources.ToSeparatedString(" + "), 
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
    /// <param name="sources">Source tuple descriptors.</param>
    public MergeTransform(bool isReadOnly, params TupleDescriptor[] sources)
      : base(isReadOnly)
    {
      int totalLength = sources.Sum(s => s.Count);
      Type[] types = new Type[totalLength];
      Pair<int,int>[] map = new Pair<int, int>[totalLength];
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
      Map = map;
    }
  }
}