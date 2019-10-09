// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2012.05.16

using System;
using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Orm.Tracking
{
  internal sealed class TrackingStackFrame : IEnumerable<TrackingItem>
  {
    private readonly Dictionary<Key, TrackingItem> items = new Dictionary<Key, TrackingItem>();

    public int Count { get { return items.Count; } }

    public void Register(TrackingItem item)
    {
      if (item==null)
        throw new ArgumentNullException("item");

      TrackingItem existing;
      if (items.TryGetValue(item.Key, out existing)) {
        if (item==existing)
          return;

        existing.MergeWith(item);
        return;
      }
      items.Add(item.Key, item);
    }

    public void Clear()
    {
      items.Clear();
    }

    public void MergeWith(TrackingStackFrame source)
    {
      if (source==null)
        throw new ArgumentNullException("source");

      foreach (var sourceItem in source) {
        TrackingItem target;
        if (items.TryGetValue(sourceItem.Key, out target))
          target.MergeWith(sourceItem);
        else
          items.Add(sourceItem.Key, sourceItem);
      }
    }

    public IEnumerator<TrackingItem> GetEnumerator()
    {
      return items.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}