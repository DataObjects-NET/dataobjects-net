// Copyright (C) 2012-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2012.05.16

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Orm.Tracking
{
  internal readonly struct TrackingStackFrame : IEnumerable<TrackingItem>
  {
    private readonly Dictionary<Key, TrackingItem> items;

    public int Count => items.Count;

    public void Register(TrackingItem item)
    {
      ArgumentNullException.ThrowIfNull(item);

      var key = item.Key;
      if (!items.TryGetValue(key, out var existing)) {
        items.Add(key, item);
      }
      else if (item != existing) {
        existing.MergeWith(item);
      }
    }

    public void Clear() => items.Clear();

    public void MergeWith(TrackingStackFrame source)
    {
      foreach (var sourceItem in source) {
        var key = sourceItem.Key;
        if (items.TryGetValue(key, out var existing)) {
          existing.MergeWith(sourceItem);
        }
        else {
          items.Add(key, sourceItem);
        }
      }
    }

    public IEnumerator<TrackingItem> GetEnumerator() => items.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // parameterless ctor not allowed in C#9
    //TODO: remove it after moving to C#10
    public TrackingStackFrame(bool _)
    {
      items = new();
    }
  }
}
