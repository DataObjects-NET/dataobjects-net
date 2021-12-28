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
  internal sealed class ConcatTransformTuple : TransformedTuple<ConcatTransform>
  {
    private readonly Tuple source1;
    private readonly Tuple source2;
    private readonly int totalCount;
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
      var (source, index) = GetSourceAndFieldIndex(fieldIndex);
      return index == MapTransform.NoMapping ? TupleFieldState.Default : source.GetFieldState(index);
    }

    protected internal override void SetFieldState(int fieldIndex, TupleFieldState fieldState)
    {
      var (source, index) = GetSourceAndFieldIndex(fieldIndex);
      if (index == MapTransform.NoMapping) {
        return;
      }
      source.SetFieldState(index, fieldState);
    }

    /// <inheritdoc/>
    public override object GetValue(int fieldIndex, out TupleFieldState fieldState)
    {
      var (source, index) = GetSourceAndFieldIndex(fieldIndex);
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
      var (source, index) = GetSourceAndFieldIndex(fieldIndex);
      source.SetValue(index, fieldValue);
    }

    #endregion

    private (Tuple source, int index) GetSourceAndFieldIndex(int fieldIndex)
    {
      if (fieldIndex < 0 || fieldIndex > totalCount) {
        return (null, MapTransform.NoMapping);
      }
      var source2Index = fieldIndex - source1.Count;
      return source2Index < 0 ? (source1, fieldIndex) : (source2, source2Index);
    }

    // Constructors
    public ConcatTransformTuple(ConcatTransform transform, Tuple source1, Tuple source2)
      : base(transform)
    {
      this.source1 = source1;
      this.source2 = source2;
      totalCount = source1.Count + source2.Count;
    }
  }
}