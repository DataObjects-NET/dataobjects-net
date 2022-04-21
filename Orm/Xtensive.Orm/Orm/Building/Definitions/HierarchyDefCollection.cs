// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.01.11

using System;
using Xtensive.Core;
using Xtensive.Collections;
using System.Collections.Generic;

namespace Xtensive.Orm.Building.Definitions
{
  public readonly struct HierarchyDefCollectionChangedEventArgs
  {
    public HierarchyDef Item { get; }

    public HierarchyDefCollectionChangedEventArgs(HierarchyDef item)
    {
      Item = item;
    }
  }

  /// <summary>
  /// A collection of <see cref="HierarchyDef"/> items.
  /// </summary>
  public class HierarchyDefCollection : CollectionBaseSlim<HierarchyDef>
  {
    public event EventHandler<HierarchyDefCollectionChangedEventArgs> Added;
    public event EventHandler<HierarchyDefCollectionChangedEventArgs> Removed;

    /// <inheritdoc/>
    public override bool Contains(HierarchyDef item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      return TryGetValue(item.Root) != null;
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value associated with the specified <paramref name="key"/> or <see langword="null"/> 
    /// if item was not found.</returns>
    public HierarchyDef TryGetValue(TypeDef key)
    {
      return TryGetValue(key.UnderlyingType);
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value associated with the specified <paramref name="key"/> or <see langword="null"/> 
    /// if item was not found.</returns>
    public HierarchyDef TryGetValue(Type key)
    {
      foreach (HierarchyDef item in this)
        if (item.Root.UnderlyingType==key)
          return item;
      return null;
    }

    /// <summary>
    /// An indexer that provides access to collection items.
    /// </summary>
    /// <exception cref="ArgumentException"> when item was not found.</exception>
    public HierarchyDef this[Type key]
    {
      get {
        HierarchyDef result = TryGetValue(key);
        if (result != null)
          return result;
        throw new ArgumentException(String.Format(Strings.ExItemByKeyXWasNotFound, key), "key");
      }
    }

    /// <inheritdoc/>
    public override void Add(HierarchyDef item)
    {
      base.Add(item);
      Added?.Invoke(this, new HierarchyDefCollectionChangedEventArgs(item));
    }

    /// <inheritdoc/>
    public override void AddRange(IEnumerable<HierarchyDef> items)
    {
      foreach (var item in items) {
        Add(item);
      }
    }

    /// <inheritdoc/>
    public override bool Remove(HierarchyDef item)
    {
      if (base.Remove(item)) {
        Removed?.Invoke(this, new HierarchyDefCollectionChangedEventArgs(item));
        return true;
      }
      return false;
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      foreach (var item in this) {
        Removed?.Invoke(this, new HierarchyDefCollectionChangedEventArgs(item));
      }
      base.Clear();
    }
  }
}