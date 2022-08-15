// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.24

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;

namespace Xtensive.Collections
{
  /// <summary>
  /// Lightweight base class for any collection.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("Count = {Count}")]
  public class CollectionBaseSlim<TItem>: LockableBase,
    ICollection<TItem>, IReadOnlyList<TItem>
  {
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private readonly List<TItem> items;

    /// <inheritdoc/>
    public virtual bool IsReadOnly {
      [DebuggerStepThrough]
      get => IsLocked;
    }

    /// <inheritdoc/>
    public int Count
    {
      [DebuggerStepThrough]
      get => items.Count;
    }

    /// <inheritdoc/>
    public virtual TItem this[int index]
    {
      [DebuggerStepThrough]
      get => items[index];
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public virtual bool Contains(TItem item)
      => items.Contains(item);

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public virtual void CopyTo(TItem[] array, int arrayIndex)
      => items.CopyTo(array, arrayIndex);

    #region GetEnumerator<...> methods

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public List<TItem>.Enumerator GetEnumerator()
      => items.GetEnumerator();

    /// <inheritdoc/>
    [DebuggerStepThrough]
    IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator()
      => items.GetEnumerator();
    
    /// <inheritdoc/>
    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
      => items.GetEnumerator();

    #endregion

    #region Modification methods: Add, Remove, etc.

    /// <inheritdoc/>
    public virtual void Add(TItem item)
    {
      EnsureNotLocked();
      items.Add(item);
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of the <see cref="CollectionBaseSlim{TItem}"/>.
    /// </summary>
    /// <param name="collection">The collection whose elements should be added to the end of the <see cref="CollectionBaseSlim{TItem}"/>. The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>
    /// <exception cref="T:System.ArgumentNullException">collection is null.</exception>
    public virtual void AddRange(IEnumerable<TItem> collection)
    {
      EnsureNotLocked();
      items.AddRange(collection);
    }

    /// <inheritdoc/>
    public virtual bool Remove(TItem item)
    {
      EnsureNotLocked();
      return items.Remove(item);
    }

    /// <inheritdoc/>
    public virtual void Clear()
    {
      EnsureNotLocked();
      items.Clear();
    }

    #endregion

    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    public CollectionBaseSlim()
    {
      items = new List<TItem>();
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="capacity">The capacity.</param>
    public CollectionBaseSlim(int capacity)
    {
      items = new List<TItem>(capacity);
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="collection">The collection.</param>
    public CollectionBaseSlim(IEnumerable<TItem> collection)
    {
      items = new List<TItem>(collection);
    }
  }
}