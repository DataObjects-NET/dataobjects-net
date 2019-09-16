// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
    private IList<ChangedValue> cachedChangedValues;

    public Key Key { get; private set; }

    public DifferentialTuple RawData { get; private set; }

    public TrackingItemState State { get; private set; }

    public IList<ChangedValue> ChangedValues
    {
      get
      {
        if (cachedChangedValues==null)
          cachedChangedValues = CalculateChangedValues().ToList().AsReadOnly();
        return cachedChangedValues;
      }
    }

    public void MergeWith(TrackingItem source)
    {
      if (source==null)
        throw new ArgumentNullException("source");

      if (State==TrackingItemState.Deleted && source.State==TrackingItemState.Created) {
        State = TrackingItemState.Changed;
        RawData = source.RawData; // TODO: Check whether a clone is required
        return;
      }

      if (State==TrackingItemState.Created && source.State==TrackingItemState.Changed) {
        State = TrackingItemState.Created;
        MergeWith(source.RawData.Difference);
        return;
      }

      MergeWith(source.RawData.Difference);
      State = source.State;
    }

    private IEnumerable<ChangedValue> CalculateChangedValues()
    {
      var originalValues = RawData.Origin;
      var changedValues = RawData.Difference;

      if (State==TrackingItemState.Created) {
        originalValues = null;
        changedValues = RawData.Origin;
      }

      foreach (var field in Key.TypeInfo.Fields.Where(f => f.Column!=null)) {
        object origValue = null, changedValue = null;
        int fieldIndex = field.MappingInfo.Offset;
        TupleFieldState fieldState;
        if (originalValues!=null)
          origValue = originalValues.GetValue(fieldIndex, out fieldState);
        if (changedValues!=null) {
          changedValue = changedValues.GetValue(fieldIndex, out fieldState);
          if (!fieldState.IsAvailable())
            continue;
        }
        yield return new ChangedValue(field, origValue, changedValue);
      }
    }

    private void MergeWith(Tuple difference)
    {
      if (RawData.Difference==null)
        RawData.Difference = difference;
      else
        RawData.Difference.MergeWith(difference, MergeBehavior.PreferDifference);
    }

    public TrackingItem(Key key, TrackingItemState state, DifferentialTuple tuple)
    {
      if (key==null)
        throw new ArgumentNullException("key");
      if (state!=TrackingItemState.Deleted && tuple==null)
        throw new ArgumentNullException("tuple");

      Key = key;
      if (tuple!=null)
        RawData = (DifferentialTuple) tuple.Clone();
      State = state;
    }
  }
}