// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.07

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Reflection;


using Xtensive.Tuples.Transform.Internals;

namespace Xtensive.Tuples.Transform
{
  /// <summary>
  /// Base class for any tuple field mapping transform.
  /// Maps fields of destination tuple to fields of a set of source tuples.
  /// </summary>
  [Serializable]
  public class MapTransform : TupleTransformBase
  {
    private int sourceCount;
    internal IReadOnlyList<int> singleSourceMap;
    internal Pair<int, int>[] map;

    /// <summary>
    /// Means that no mapping is available for the specified field index.
    /// </summary>
    public const int NoMapping = int.MinValue;

    /// <summary>
    /// Gets the count of source <see cref="Tuples"/> this transform maps to the target one.
    /// </summary>
    public int SourceCount => sourceCount;

    /// <summary>
    /// Gets or sets destination-to-source field map for the first source only.
    /// </summary>
    public IReadOnlyList<int> SingleSourceMap => singleSourceMap;

    /// <summary>
    /// Gets or sets destination-to-source field map.
    /// </summary>
    public IReadOnlyList<Pair<int, int>> Map => map;

    /// <summary>
    /// Applies the transformation.
    /// </summary>
    /// <param name="transformType">The type of transformation to perform.</param>
    /// <param name="sources">Transformation sources.</param>
    /// <returns>Transformation result - 
    /// either <see cref="TransformedTuple"/> or <see cref="Tuple"/> descendant,
    /// dependently on specified <paramref name="transformType"/>.</returns>
    public Tuple Apply(TupleTransformType transformType, params Tuple[] sources)
    {
      ArgumentValidator.EnsureArgumentNotNull(sources, nameof(sources));
      if (sourceCount > sources.Length) {
        throw new InvalidOperationException(string.Format(Strings.ExTheNumberOfSourcesIsTooSmallExpected, sourceCount));
      }
      switch (sourceCount) {
        case 1:
          return Apply(transformType, sources[0]);
        case 2:
          return Apply(transformType, sources[0], sources[1]);
        case 3:
          return Apply(transformType, sources[0], sources[1], sources[2]);
        default:
          switch (transformType) {
            case TupleTransformType.Auto:
              foreach (Tuple tuple in sources)
                if (tuple is TransformedTuple)
                  goto case TupleTransformType.Tuple;
              goto case TupleTransformType.TransformedTuple;
            case TupleTransformType.TransformedTuple:
              return new MapTransformTuple(this, sources);
            case TupleTransformType.Tuple:
              Tuple result = Tuple.Create(Descriptor);
              sources.CopyTo(result, map);
              return result;
            default:
              throw new ArgumentOutOfRangeException(nameof(transformType));
          }
      }
    }

    /// <summary>
    /// Applies the transformation.
    /// </summary>
    /// <param name="transformType">The type of transformation to perform.</param>
    /// <param name="source">Transformation source.</param>
    /// <returns>Transformation result - 
    /// either <see cref="TransformedTuple"/> or <see cref="Tuple"/> descendant,
    /// dependently on specified <paramref name="transformType"/>.</returns>
    public Tuple Apply(TupleTransformType transformType, Tuple source)
    {
      if (sourceCount > 1) {
        throw new InvalidOperationException(string.Format(Strings.ExTheNumberOfSourcesIsTooSmallExpected, sourceCount));
      }
      switch (transformType) {
        case TupleTransformType.Auto:
          if (source is TransformedTuple)
            goto case TupleTransformType.Tuple;
          goto case TupleTransformType.TransformedTuple;
        case TupleTransformType.TransformedTuple:
          return new MapTransformTuple1(this, source);
        case TupleTransformType.Tuple:
          Tuple result = Tuple.Create(Descriptor);
          source.CopyTo(result, singleSourceMap);
          return result;
        default:
          throw new ArgumentOutOfRangeException(nameof(transformType));
      }
    }

    /// <summary>
    /// Applies the transformation.
    /// </summary>
    /// <param name="transformType">The type of transformation to perform.</param>
    /// <param name="source1">First transformation source.</param>
    /// <param name="source2">Second transformation source.</param>
    /// <returns>Transformation result - 
    /// either <see cref="TransformedTuple"/> or <see cref="Tuple"/> descendant,
    /// dependently on specified <paramref name="transformType"/>.</returns>
    public Tuple Apply(TupleTransformType transformType, Tuple source1, Tuple source2)
    {
      if (sourceCount > 2) {
        throw new InvalidOperationException(string.Format(Strings.ExTheNumberOfSourcesIsTooSmallExpected, sourceCount));
      }
      switch (transformType) {
        case TupleTransformType.Auto:
          if (source1 is TransformedTuple)
            goto case TupleTransformType.Tuple;
          if (source2 is TransformedTuple)
            goto case TupleTransformType.Tuple;
          goto case TupleTransformType.TransformedTuple;
        case TupleTransformType.TransformedTuple:
          return new MapTransformTuple3(this, source1, source2);
        case TupleTransformType.Tuple:
          FixedList3<Tuple> sources = new FixedList3<Tuple>(source1, source2);
          Tuple result = Tuple.Create(Descriptor);
          sources.CopyTo(result, map);
          return result;
        default:
          throw new ArgumentOutOfRangeException(nameof(transformType));
      }
    }

    /// <summary>
    /// Applies the transformation.
    /// </summary>
    /// <param name="transformType">The type of transformation to perform.</param>
    /// <param name="source1">First transformation source.</param>
    /// <param name="source2">Second transformation source.</param>
    /// <param name="source3">Third transformation source.</param>
    /// <returns>Transformation result - 
    /// either <see cref="TransformedTuple"/> or <see cref="Tuple"/> descendant,
    /// dependently on specified <paramref name="transformType"/>.</returns>
    public Tuple Apply(TupleTransformType transformType, Tuple source1, Tuple source2, Tuple source3)
    {
      if (sourceCount > 3) {
        throw new InvalidOperationException(string.Format(Strings.ExTheNumberOfSourcesIsTooSmallExpected, sourceCount));
      }
      switch (transformType) {
        case TupleTransformType.Auto:
          if (source1 is TransformedTuple)
            goto case TupleTransformType.Tuple;
          if (source2 is TransformedTuple)
            goto case TupleTransformType.Tuple;
          if (source3 is TransformedTuple)
            goto case TupleTransformType.Tuple;
          goto case TupleTransformType.TransformedTuple;
        case TupleTransformType.TransformedTuple:
          return new MapTransformTuple3(this, source1, source2, source3);
        case TupleTransformType.Tuple:
          FixedList3<Tuple> sources = new FixedList3<Tuple>(source1, source2, source3);
          Tuple result = Tuple.Create(Descriptor);
          sources.CopyTo(result, map);
          return result;
        default:
          throw new ArgumentOutOfRangeException(nameof(transformType));
      }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      string description = $"{SourceCount}: {(SourceCount == 1 ? singleSourceMap.ToCommaDelimitedString() : map.ToCommaDelimitedString())}, {(IsReadOnly ? Strings.ReadOnlyShort : Strings.ReadWriteShort)}";
      return string.Format(Strings.TupleTransformFormat,
        GetType().GetShortName(),
        description);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="isReadOnly"><see cref="TupleTransformBase.IsReadOnly"/> property value.</param>
    /// <param name="descriptor">Initial <see cref="TupleTransformBase.Descriptor"/> property value.</param>
    /// <param name="map"><see cref="Map"/> property value.</param>
    public MapTransform(bool isReadOnly, TupleDescriptor descriptor, Pair<int, int>[] map)
      : base(descriptor, isReadOnly)
    {
      ArgumentValidator.EnsureArgumentNotNull(map, nameof(map));
      var newFirstSourceMap = new int[map.Length];
      var index = 0;
      var newSourceCount = -1;
      foreach (var mappedTo in map) {
        if (mappedTo.First > newSourceCount) {
          newSourceCount = mappedTo.First;
        }
        newFirstSourceMap[index++] = mappedTo.First == 0 ? mappedTo.Second : -1;
      }
      newSourceCount++;

      this.map = map;
      singleSourceMap = newSourceCount == 1 ? newFirstSourceMap : null;
      sourceCount = newSourceCount;
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="isReadOnly"><see cref="TupleTransformBase.IsReadOnly"/> property value.</param>
    /// <param name="descriptor">Initial <see cref="TupleTransformBase.Descriptor"/> property value.</param>
    /// <param name="singleSourceMap"><see cref="SingleSourceMap"/> property value.</param>
    public MapTransform(bool isReadOnly, TupleDescriptor descriptor, IReadOnlyList<int> singleSourceMap)
      : base(descriptor, isReadOnly)
    {
      ArgumentValidator.EnsureArgumentNotNull(singleSourceMap, nameof(singleSourceMap));
      var newMap = new Pair<int, int>[Descriptor.Count];
      var index = 0;
      for (; index < newMap.Length && index < singleSourceMap.Count; index++) {
        newMap[index] = new Pair<int, int>(0, singleSourceMap[index]);
      }
      while (index < newMap.Length) {
        newMap[index++] = new Pair<int, int>(0, NoMapping);
      }

      map = newMap;
      this.singleSourceMap = singleSourceMap;
      sourceCount = 1;
    }
  }
}