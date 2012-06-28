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
using Xtensive.Internals.DocTemplates;


namespace Xtensive.Collections
{
  /// <summary>
  /// Lightweight base class for any collection.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("Count = {Count}")]
  public class CollectionBaseSlim<TItem>: LockableBase,
    IList<TItem>,
    ICollection
  {
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private readonly List<TItem> items;

    #region Properties: IsReadOnly, SyncRoot, IsSyncronized

    /// <inheritdoc/>
    public virtual bool IsReadOnly
    {
      [DebuggerStepThrough]
      get { return IsLocked; }
    }

    /// <inheritdoc/>
    public virtual object SyncRoot
    {
      [DebuggerStepThrough]
      get { return this; }
    }

    /// <inheritdoc/>
    public virtual bool IsSynchronized
    {
      [DebuggerStepThrough]
      get { return false; }
    }

    #endregion

    /// <summary>
    /// Gets the items.
    /// </summary>
    /// <value>The items.</value>
    protected List<TItem> Items
    {
      [DebuggerStepThrough]
      get { return items; }
    }

    /// <inheritdoc/>
    public int Count
    {
      [DebuggerStepThrough]
      get { return Items.Count; }
    }

    /// <inheritdoc/>
    int ICollection<TItem>.Count
    {
      [DebuggerStepThrough]
      get { return Count; }
    }

    /// <inheritdoc/>
    public virtual TItem this[int index]
    {
      get { return Items[index]; }
      set
      {
        this.EnsureNotLocked();
        Items[index] = value;
      }
    }

    /// <inheritdoc/>
    public virtual int IndexOf(TItem item)
    {
      return Items.IndexOf(item);
    }

    /// <inheritdoc/>
    public virtual bool Contains(TItem item)
    {
      return Items.Contains(item);
    }

    #region Modification methods: Add, Remove, etc.

    /// <inheritdoc/>
    public virtual void Add(TItem item)
    {
      this.EnsureNotLocked();
      Items.Add(item);
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of the <see cref="CollectionBaseSlim{TItem}"/>.
    /// </summary>
    /// <param name="collection">The collection whose elements should be added to the end of the <see cref="CollectionBaseSlim{TItem}"/>. The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>
    /// <exception cref="T:System.ArgumentNullException">collection is null.</exception>
    public virtual void AddRange(IEnumerable<TItem> collection)
    {
      this.EnsureNotLocked();
      items.AddRange(collection);
    }

    /// <inheritdoc/>
    public virtual void Insert(int index, TItem item)
    {
      this.EnsureNotLocked();
      Items.Insert(index, item);
    }

    /// <inheritdoc/>
    public virtual bool Remove(TItem item)
    {
      this.EnsureNotLocked();
      return Items.Remove(item);
    }

    /// <inheritdoc/>
    public virtual void RemoveAt(int index)
    {
      this.EnsureNotLocked();
      Items.RemoveAt(index);
    }

    /// <inheritdoc/>
    public virtual void Clear()
    {
      this.EnsureNotLocked();
      Items.Clear();
    }

    #endregion

    #region CopyTo methods

    /// <inheritdoc/>
    public virtual void CopyTo(Array array, int index)
    {
      ((ICollection)Items).CopyTo(array, index);
    }

    /// <inheritdoc/>
    public virtual void CopyTo(TItem[] array, int arrayIndex)
    {
      Items.CopyTo(array, arrayIndex);
    }

    #endregion

    #region GetEnumerator<...> methods

    /// <inheritdoc/>
    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public virtual IEnumerator<TItem> GetEnumerator()
    {
      return Items.GetEnumerator();
    }

    #endregion

   
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public CollectionBaseSlim()
      : this(0)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="capacity">The capacity.</param>
    public CollectionBaseSlim(int capacity)
    {
      items = new List<TItem>(capacity);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="collection">The collection.</param>
    public CollectionBaseSlim(IEnumerable<TItem> collection)
    {
      items = new List<TItem>(collection);
    }
  }
}