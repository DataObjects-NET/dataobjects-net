// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kochetov
// Created:    2008.04.30

using System;
using Xtensive.Core;
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
    private readonly (int first, int second) sourceParts;

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
      var sb = new ValueStringBuilder(stackalloc char[4096]);
      for (int i = 0, count = sourceParts.first; i < count; i++) {
        if (i > 0)
          sb.Append(", ");
        sb.Append(Descriptor[i].GetShortName());
      }
      var sourceOne = string.Format(Strings.TupleDescriptorFormat, sb.ToString());

      sb = new ValueStringBuilder(stackalloc char[4096]);
      for (int i = sourceParts.first, count = Descriptor.Count; i < count; i++) {
        if (i > sourceParts.first)
          sb.Append(", ");
        sb.Append(Descriptor[i].GetShortName());
      }
      var sourceTwo = string.Format(Strings.TupleDescriptorFormat, sb.ToString());

      var description = $"{sourceOne} + {sourceTwo}, {(IsReadOnly ? Strings.ReadOnlyShort : Strings.ReadWriteShort)}";
      return string.Format(Strings.TupleTransformFormat,
        nameof(ConcatTransform),
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
      if (first == default)
        throw new ArgumentException("Argument is default instance.", nameof(first));

      if (second == default)
        throw new ArgumentException("Argument is default instance.", nameof(second));

      IsReadOnly = isReadOnly;
      Descriptor = first.ConcatWith(second);
      this.sourceParts = (first.Count, second.Count);
    }


    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="first">The <see cref="TupleDescriptor"/> of the first source <see cref="Tuple"/>.</param>
    /// <param name="second">The <see cref="TupleDescriptor"/> of the second source <see cref="Tuple"/>.</param>
    // WARNING !!!!! NO CHECKS FOR DEFAULT VALUES FOR THE SAKE OF PEFRORMANCE
    internal ConcatTransform(TupleDescriptor first, TupleDescriptor second)
    {
      IsReadOnly = false;
      Descriptor = first.ConcatWith(second);
      this.sourceParts = (first.Count, second.Count);
    }
  }
}