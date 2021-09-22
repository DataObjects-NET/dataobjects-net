// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.11

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Orm.Building.Definitions
{
  public sealed class HierarchyDefCollectionChangedEventArgs: EventArgs
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
  public class HierarchyDefCollection : LockableBase, ICollection<HierarchyDef>, IReadOnlyList<HierarchyDef>
  {
    private readonly List<HierarchyDef> items = new List<HierarchyDef>();

    public event EventHandler<HierarchyDefCollectionChangedEventArgs> Added;

    public event EventHandler<HierarchyDefCollectionChangedEventArgs> Removed;

    public int Count => items.Count;

    public bool IsReadOnly => IsLocked;

    public HierarchyDef this[int index] => items[index];

    /// <inheritdoc/>
    public bool Contains(HierarchyDef item)
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
      get
      {
        HierarchyDef result = TryGetValue(key);
        if (result!=null)
          return result;
          throw new ArgumentException(String.Format(Strings.ExItemByKeyXWasNotFound, key), "key");
      }
    }

    public void CopyTo(HierarchyDef[] array, int arrayIndex) => items.CopyTo(array, arrayIndex);

    public List<HierarchyDef>.Enumerator GetEnumerator() => items.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator<HierarchyDef> IEnumerable<HierarchyDef>.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(HierarchyDef item)
    {
      this.EnsureNotLocked();
      items.Add(item);
      Added(this, new HierarchyDefCollectionChangedEventArgs(item));
    }
    public virtual bool Remove(HierarchyDef item)
    {
      this.EnsureNotLocked();
      if (items.Remove(item)) {
        Removed(this, new HierarchyDefCollectionChangedEventArgs(item));
        return true;
      }
      return false;
    }

    public virtual void Clear()
    {
      this.EnsureNotLocked();
      items.Clear();
    }
  }
}