// Copyright (C) 2012-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2012.05.16

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Tracking
{
  [DebuggerDisplay("{Key}")]
  internal sealed class TrackingItem : ITrackingItem
  {
    private IReadOnlyList<ChangedValue> cachedChangedValues;

    public Key Key { get; }

    public DifferentialTuple RawData { get; private set; }

    public TrackingItemState State { get; private set; }

    public IReadOnlyList<ChangedValue> ChangedValues => cachedChangedValues ??= CalculateChangedValues();

    public void MergeWith(TrackingItem source)
    {
      ArgumentNullException.ThrowIfNull(source);

      if (State == TrackingItemState.Deleted && source.State == TrackingItemState.Created) {
        State = TrackingItemState.Changed;
        RawData = source.RawData; // TODO: Check whether a clone is required
        return;
      }

      if (State == TrackingItemState.Created && source.State == TrackingItemState.Changed) {
        State = TrackingItemState.Created;
        MergeWith(source.RawData.Difference);
        return;
      }

      MergeWith(source.RawData.Difference);
      State = source.State;
    }

    private IReadOnlyList<ChangedValue> CalculateChangedValues()
    {
      var originalValues = RawData.Origin;
      var changedValues = RawData.Difference;

      if (State == TrackingItemState.Created) {
        originalValues = null;
        changedValues = RawData.Origin;
      }

      var changedValuesList = new List<ChangedValue>(Key.TypeInfo.Fields.Count);

      foreach (var field in Key.TypeInfo.Fields.Where(f => f.Column != null)) {
        object origValue = null, changedValue = null;
        int fieldIndex = field.MappingInfo.Offset;
        TupleFieldState fieldState;
        if (originalValues != null)
          origValue = originalValues.GetValue(fieldIndex, out fieldState);
        if (changedValues != null) {
          changedValue = changedValues.GetValue(fieldIndex, out fieldState);
          if (!fieldState.IsAvailable())
            continue;
        }
        changedValuesList.Add(new ChangedValue(field, origValue, changedValue));
      }

      return changedValuesList.AsReadOnly();
    }

    private void MergeWith(Tuple difference)
    {
      if (RawData.Difference == null)
        RawData.Difference = difference;
      else
        RawData.Difference.MergeWith(difference, MergeBehavior.PreferDifference);
    }

    public TrackingItem(Key key, TrackingItemState state, DifferentialTuple tuple)
    {
      ArgumentNullException.ThrowIfNull(key);

      if (state != TrackingItemState.Deleted) {
        ArgumentNullException.ThrowIfNull(tuple);
      }

      Key = key;
      if (tuple != null)
        RawData = (DifferentialTuple) tuple.Clone();
      State = state;
    }
  }
}