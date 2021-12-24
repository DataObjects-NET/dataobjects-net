// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kochetov
// Created:    2008.04.30

using System;
using Xtensive.Core;
using Xtensive.Reflection;

namespace Xtensive.Tuples.Transform
{
  /// <summary>
  /// This class is used for source <see cref="Tuple"/>s combining.
  /// </summary>
  [Serializable]
  public sealed class CombineTransform : ITupleTransform
  {
    private MapTransform mapTransform;
    private readonly (TupleDescriptor first, TupleDescriptor second) sources;

    /// <inheritdoc/>
    public TupleDescriptor Descriptor => mapTransform.Descriptor;

    /// <inheritdoc/>
    public bool IsReadOnly => mapTransform.IsReadOnly;

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
      => mapTransform.Apply(transformType, source1, source2);

    /// <inheritdoc/>
    public override string ToString()
    {
      string description = $"{sources.first} + {sources.second}, {(IsReadOnly ? Strings.ReadOnlyShort : Strings.ReadWriteShort)}";
      return string.Format(Strings.TupleTransformFormat,
        GetType().GetShortName(),
        description);
    }

    // Constructors

    private static TupleDescriptor CreateDescriptorAndMap(in (TupleDescriptor first, TupleDescriptor second) sources, out Pair<int, int>[] map)
    {
      int totalLength = sources.first.Count + sources.second.Count;
      var types = new Type[totalLength];
      map = new Pair<int, int>[totalLength];
      int index = 0;
      for (int i = 0; i < 2; i++) {
        var currentDescriptor = i == 0 ? sources.first : sources.second;
        int currentCount = currentDescriptor.Count;
        for (int j = 0; j < currentCount; j++) {
          types[index] = currentDescriptor[j];
          map[index++] = new Pair<int, int>(i, j);
        }
      }
      return TupleDescriptor.Create(types);
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="isReadOnly"><see cref="ITupleTransform.IsReadOnly"/> property value.</param>
    /// <param name="first">First tuple descriptor to combine.</param>
    /// <param name="second">Second tuple descriptor to combine.</param>
    public CombineTransform(bool isReadOnly, TupleDescriptor first, TupleDescriptor second)
    {
      mapTransform = new MapTransform(isReadOnly, CreateDescriptorAndMap((first, second), out var map), map);
      this.sources = (first, second);
    }
  }
}