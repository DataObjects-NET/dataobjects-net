// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kochetov
// Created:    2008.04.30

using System;
using Xtensive.Reflection;
using Xtensive.Tuples.Transform.Internals;

namespace Xtensive.Tuples.Transform
{
  /// <summary>
  /// This class is used for concatenation of two <see cref="Tuple"/>s.
  /// </summary>
  [Serializable]
  public sealed class ConcatTransform
  {
    private readonly (TupleDescriptor first, TupleDescriptor second) sources;

    /// <inheritdoc/>
    public TupleDescriptor Descriptor { get; }

    /// <inheritdoc/>
    public bool IsReadOnly { get; }

    /// <summary>
    /// Applies the transformation.
    /// </summary>
    /// <param name="transformType">The type of transformation to perform.</param>
    /// <param name="source1">First transformation source.</param>
    /// <param name="source2">Second transformation source.</param>
    /// <returns>Transformation result - 
    /// either <see cref="ConcatTransformTuple"/> instance or the <see cref="RegularTuple"/> descendant,
    /// dependently on specified <paramref name="transformType"/>.</returns>
    public Tuple Apply(TupleTransformType transformType, Tuple source1, Tuple source2) {
      return transformType switch {
        TupleTransformType.Auto when source1 is ITransformedTuple || source2 is ITransformedTuple => CopySourceTuples(source1, source2),
        TupleTransformType.Auto => new ConcatTransformTuple(this, source1, source2),
        TupleTransformType.Tuple => CopySourceTuples(source1, source2),
        TupleTransformType.TransformedTuple => new ConcatTransformTuple(this, source1, source2),
        _ => throw new ArgumentOutOfRangeException(nameof(transformType))
      };

      Tuple CopySourceTuples(Tuple source1, Tuple source2) {
        var result = Tuple.Create(Descriptor);
        source1.CopyTo(result);
        source2.CopyTo(result, 0, source1.Count, source2.Count);
        return result;
      }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      string description = $"{sources.first} + {sources.second}, {(IsReadOnly ? Strings.ReadOnlyShort : Strings.ReadWriteShort)}";
      return string.Format(Strings.TupleTransformFormat,
        GetType().GetShortName(),
        description);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="isReadOnly">Indicates whethere the transformed <see cref="Tuple"/> is read only.</param>
    /// <param name="first">The <see cref="TupleDescriptor"/> of the first source <see cref="Tuple"/>.</param>
    /// <param name="second">The <see cref="TupleDescriptor"/> of the second source <see cref="Tuple"/>.</param>
    public ConcatTransform(bool isReadOnly, TupleDescriptor first, TupleDescriptor second)
    {
      var (firstCount, secondCount) = (first.Count, second.Count);
      var types = new Type[firstCount + secondCount];
      Array.Copy(first.FieldTypes, types, firstCount);
      Array.Copy(second.FieldTypes, 0, types, firstCount, secondCount);

      IsReadOnly = isReadOnly;
      Descriptor = TupleDescriptor.Create(types);
      this.sources = (first, second);
    }
  }
}