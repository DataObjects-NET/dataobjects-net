// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.07

using System;
using System.Collections.Generic;
using System.Linq;
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
  public class MapTransform : ITupleTransform
  {
    private int sourceCount;
    internal Pair<int, int>[] map;

    /// <summary>
    /// Gets <see cref="TupleDescriptor"/> describing the tuples
    /// this transform may produce.
    /// </summary>
    public TupleDescriptor Descriptor { get; }

    /// <summary>
    /// Indicates whether transform always produces read-only tuples or not.
    /// </summary>>
    public bool IsReadOnly { get; }
    
    /// <summary>
    /// Means that no mapping is available for the specified field index.
    /// </summary>
    public const int NoMapping = int.MinValue;

    /// <summary>
    /// Gets or sets destination-to-source field map.
    /// </summary>
    public IReadOnlyList<Pair<int, int>> Map => map;

    /// <summary>
    /// Applies the transformation.
    /// </summary>
    /// <param name="transformType">The type of transformation to perform.</param>
    /// <param name="source1">First transformation source.</param>
    /// <param name="source2">Second transformation source.</param>
    /// <returns>Transformation result - 
    /// either <see cref="TransformedTuple{TTupleTransform}"/> or <see cref="Tuple"/> descendant,
    /// dependently on specified <paramref name="transformType"/>.</returns>
    public Tuple Apply(TupleTransformType transformType, Tuple source1, Tuple source2)
    {
      if (sourceCount > 2) {
        throw new InvalidOperationException(string.Format(Strings.ExTheNumberOfSourcesIsTooSmallExpected, sourceCount));
      }
      switch (transformType) {
        case TupleTransformType.Auto:
          if (source1 is ITransformedTuple)
            goto case TupleTransformType.Tuple;
          if (source2 is ITransformedTuple)
            goto case TupleTransformType.Tuple;
          goto case TupleTransformType.TransformedTuple;
        case TupleTransformType.TransformedTuple:
          return new MapTransformTuple3(this, source1, source2);
        case TupleTransformType.Tuple:
          var sources = new FixedReadOnlyList3<Tuple>(source1, source2);
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
      string description = $"{sourceCount}: {map.ToCommaDelimitedString()}, {(IsReadOnly ? Strings.ReadOnlyShort : Strings.ReadWriteShort)}";
      return string.Format(Strings.TupleTransformFormat,
        GetType().GetShortName(),
        description);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="isReadOnly"><see cref="ITupleTransform.IsReadOnly"/> property value.</param>
    /// <param name="descriptor">Initial <see cref="ITupleTransform.Descriptor"/> property value.</param>
    /// <param name="map"><see cref="Map"/> property value.</param>
    public MapTransform(bool isReadOnly, TupleDescriptor descriptor, Pair<int, int>[] map)
    {
      ArgumentValidator.EnsureArgumentNotNull(descriptor, nameof(descriptor));
      ArgumentValidator.EnsureArgumentNotNull(map, nameof(map));
      
      IsReadOnly = isReadOnly;
      Descriptor = descriptor;

      this.map = map;
      sourceCount = map.Max(item => item.First) + 1;
    }
  }
}