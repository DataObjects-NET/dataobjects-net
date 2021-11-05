// Copyright (C) a Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    20.06.2008

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core;
using Xtensive.Reflection;

namespace Xtensive.Tuples.Transform
{
  /// <summary>
  /// Cuts in specified value to the <see cref="Tuple"/>.
  /// </summary>
  public class CutInTransform : MapTransform
  {
    private int index;
    private TupleDescriptor[] sources;

    /// <summary>
    /// Gets the start index at witch this transform cuts in specified value.
    /// </summary>
    public int Index
    {
      [DebuggerStepThrough]
      get { return index; }
    }

    /// <summary>
    /// Gets tuple descriptors this transform cuts in.
    /// </summary>
    public IReadOnlyList<TupleDescriptor> Sources
    {
      [DebuggerStepThrough]
      get => Array.AsReadOnly(sources);
    }

    /// <see cref="MapTransform.Apply(TupleTransformType,Tuple)" copy="true" />
    public new Tuple Apply(TupleTransformType transformType, Tuple source1, Tuple source2)
    {
      return base.Apply(transformType, source1, source2);
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
    /// <param name="index">Start index.</param>
    /// <param name="sources">Source tuple descriptors.</param>
    public CutInTransform(bool isReadOnly,  int index , params TupleDescriptor[] sources)
      : base(isReadOnly)
    {
      this.index = index;
      int totalLength = sources.Sum(s => s.Count);
      Type[] types = new Type[totalLength];
      Pair<int, int>[] map = new Pair<int, int>[totalLength];
      TupleDescriptor sourceDescriptor = sources[0];
      TupleDescriptor cutInDescriptor = sources[1];
      int sourceCount = sourceDescriptor.Count;
      int cutInCount = cutInDescriptor.Count;

      bool isIndex = false;
      bool isEndOfTuple = false;
      int ind = 0;

      if (index == sourceCount) {
        sourceCount++;
        isEndOfTuple = true;
      }
      else if (index < 0 || index > sourceCount)
        throw new ArgumentOutOfRangeException("index");
      for (int i = 0; i < sourceCount; i++)
      {
        if ((i == index) && !isIndex) {
          for (int j = 0; j < cutInCount; j++)
          {
            types[ind] = cutInDescriptor[j];
            map[ind++] = new Pair<int, int>(1, j);
          }
          if (!isEndOfTuple) {
            i--;
            isIndex = true;
          }
        }
        else {
          types[ind] = sourceDescriptor[i];
          map[ind++] = new Pair<int, int>(0, i);
        }
      }
      this.sources = sources;
      Descriptor = TupleDescriptor.Create(types);
      SetMap(map);
    }
  }
}