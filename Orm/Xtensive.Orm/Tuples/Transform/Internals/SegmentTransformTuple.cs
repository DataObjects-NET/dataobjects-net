// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;

namespace Xtensive.Tuples.Transform.Internals
{
  /// <summary>
  /// A <see cref="MapTransform"/> result tuple mapping 1 source tuple to a single one (this).
  /// </summary>
  [Serializable]
  internal sealed class SegmentTransformTuple : TransformedTuple<SegmentTransform>
  {
    private readonly Tuple source;
    private Tuple defaultResult;

    /// <summary>
    /// Gets the default result tuple.
    /// Can be used to get default values for the result tuple fields.
    /// </summary>
    private Tuple DefaultResult => defaultResult ??= Tuple.Create(TupleTransform.Descriptor);

    #region GetFieldState, GetValue, SetValue methods

    /// <inheritdoc/>
    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      var index = GetSourceFieldIndex(fieldIndex);
      return index == MapTransform.NoMapping ? TupleFieldState.Default : source.GetFieldState(index);
    }

    protected internal override void SetFieldState(int fieldIndex, TupleFieldState fieldState)
    {
      var index = GetSourceFieldIndex(fieldIndex);
      if (index == MapTransform.NoMapping) {
        return;
      }
      source.SetFieldState(index, fieldState);
    }

    /// <inheritdoc/>
    public override object GetValue(int fieldIndex, out TupleFieldState fieldState)
    {
      int index = GetSourceFieldIndex(fieldIndex);
      return index == MapTransform.NoMapping
        ? DefaultResult.GetValue(fieldIndex, out fieldState)
        : source.GetValue(index, out fieldState);
    }

    /// <inheritdoc/>
    public override void SetValue(int fieldIndex, object fieldValue)
    {
      if (TupleTransform.IsReadOnly) {
        throw Exceptions.ObjectIsReadOnly(null);
      }
      source.SetValue(GetSourceFieldIndex(fieldIndex), fieldValue);
    }

    #endregion

    private int GetSourceFieldIndex(int fieldIndex)
    {
      var sourceIndex = TupleTransform.Segment.Offset + fieldIndex;
      return sourceIndex < 0 || sourceIndex >= source.Count ? MapTransform.NoMapping : sourceIndex;
    }

    // Constructors
    public SegmentTransformTuple(SegmentTransform transform, Tuple source)
      : base(transform)
    {
      this.source = source;
    }
  }
}