// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;

namespace Xtensive.Tuples.Transform.Internals
{
  /// <summary>
  /// A <see cref="SegmentTransform"/> result tuple mapping 1 source tuple to a single one (this).
  /// </summary>
  [Serializable]
  internal sealed class SegmentTransformTuple : Tuple, ITransformedTuple
  {
    private readonly SegmentTransform transform;
    private readonly Tuple source;
    private Tuple defaultResult;

    /// <summary>
    /// Gets the default result tuple.
    /// Can be used to get default values for the result tuple fields.
    /// </summary>
    private Tuple DefaultResult => defaultResult ??= Tuple.Create(transform.Descriptor);

    /// <inheritdoc/>
    public override TupleDescriptor Descriptor => transform.Descriptor;

    #region GetFieldState, GetValue, SetValue methods

    /// <inheritdoc/>
    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      var index = GetSourceFieldIndex(fieldIndex);
      return index == TransformUtil.NoMapping ? TupleFieldState.Default : source.GetFieldState(index);
    }

    protected internal override void SetFieldState(int fieldIndex, TupleFieldState fieldState)
    {
      var index = GetSourceFieldIndex(fieldIndex);
      if (index == TransformUtil.NoMapping) {
        return;
      }
      source.SetFieldState(index, fieldState);
    }

    /// <inheritdoc/>
    public override object GetValue(int fieldIndex, out TupleFieldState fieldState)
    {
      int index = GetSourceFieldIndex(fieldIndex);
      return index == TransformUtil.NoMapping
        ? DefaultResult.GetValue(fieldIndex, out fieldState)
        : source.GetValue(index, out fieldState);
    }

    /// <inheritdoc/>
    public override void SetValue(int fieldIndex, object fieldValue)
    {
      if (transform.IsReadOnly) {
        throw Exceptions.ObjectIsReadOnly(null);
      }
      source.SetValue(GetSourceFieldIndex(fieldIndex), fieldValue);
    }

    #endregion

    private int GetSourceFieldIndex(int fieldIndex)
    {
      var sourceIndex = transform.Segment.Offset + fieldIndex;
      return sourceIndex < 0 || sourceIndex >= source.Count ? TransformUtil.NoMapping : sourceIndex;
    }

    /// <inheritdoc/>
    public override string ToString() =>
      string.Format(Strings.TransformedTupleFormat, base.ToString(), transform, source);


    // Constructors

    public SegmentTransformTuple(SegmentTransform transform, Tuple source)
    {
      this.transform = transform;
      this.source = source;
    }
  }
}