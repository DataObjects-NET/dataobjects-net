// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.07

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Reflection;


using Xtensive.Tuples.Transform.Internals;

namespace Xtensive.Tuples.Transform
{
  /// <summary>
  /// Maps fields of a destination tuple to the specified fields of of the source tuple.
  /// </summary>
  [Serializable]
  public class MapTransform
  {
    private IReadOnlyList<int> map;

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
    /// Gets or sets destination-to-source field map for the first source only.
    /// </summary>
    public IReadOnlyList<int> Map => map;

    /// <summary>
    /// Applies the transformation.
    /// </summary>
    /// <param name="transformType">The type of transformation to perform.</param>
    /// <param name="source">Transformation source.</param>
    /// <returns>Transformation result - 
    /// either <see cref="MapTransformTuple"/> instance or the <see cref="RegularTuple"/> descendant,
    /// dependently on specified <paramref name="transformType"/>.</returns>
    public Tuple Apply(TupleTransformType transformType, Tuple source)
    {
      switch (transformType) {
        case TupleTransformType.Auto:
          if (source is ITransformedTuple)
            goto case TupleTransformType.Tuple;
          goto case TupleTransformType.TransformedTuple;
        case TupleTransformType.TransformedTuple:
          return new MapTransformTuple(this, source);
        case TupleTransformType.Tuple:
          Tuple result = Tuple.Create(Descriptor);
          source.CopyTo(result, map);
          return result;
        default:
          throw new ArgumentOutOfRangeException(nameof(transformType));
      }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      string description = $"[{map.ToCommaDelimitedString()}], {(IsReadOnly ? Strings.ReadOnlyShort : Strings.ReadWriteShort)}";
      return string.Format(Strings.TupleTransformFormat,
        GetType().GetShortName(),
        description);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="isReadOnly">Indicates whethere the transformed <see cref="Tuple"/> is read only.</param>
    /// <param name="descriptor">The <see cref="TupleDescriptor"/> of the target <see cref="Tuple"/>.</param>
    /// <param name="map"><see cref="Map"/> property value.</param>
    public MapTransform(bool isReadOnly, TupleDescriptor descriptor, IReadOnlyList<int> map)
    {
      ArgumentValidator.EnsureArgumentNotNull(descriptor, nameof(descriptor));
      ArgumentValidator.EnsureArgumentNotNull(map, nameof(map));
      
      IsReadOnly = isReadOnly;
      Descriptor = descriptor;

      this.map = map;
    }
  }
}