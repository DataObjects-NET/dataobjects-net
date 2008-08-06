// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;
using Xtensive.Core.Threading;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// Read-only list (<see cref="IList"/>) wrapper.
  /// </summary>
  [Serializable]
  public class ReadOnlyList<T>: 
    IList<T>, 
    ICountable<T>,
    ISynchronizable,
    IReadOnly
  {
    private readonly IList<T> innerList;
    private readonly bool isFixedSize;

    /// <inheritdoc/>
    [DebuggerHidden]
    public int Count {
      get { return innerList.Count; }
    }

    /// <inheritdoc/>
    [DebuggerHidden]
    long ICountable.Count
    {
      get { return Count; }
    }

    /// <inheritdoc/>
    [DebuggerHidden]
    public object SyncRoot {
      get { return this; }
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown by "set" accessor (setter).</exception>
    public T this[int index]
    {
      get { return innerList[index]; }
      set { throw Exceptions.CollectionIsReadOnly(null);; }
    }

    #region IsXxx properties

    /// <inheritdoc/>
    [DebuggerHidden]
    public virtual bool IsSynchronized
    {
      get { return false; }
    }

    /// <inheritdoc/>
    [DebuggerHidden]
    public bool IsReadOnly
    {
      get { return true; }
    }

    /// <inheritdoc/>
    [DebuggerHidden]
    public bool IsFixedSize
    {
      get { return isFixedSize; }
    }

    #endregion

    #region Contains, IndexOf, CopyTo methods

    /// <inheritdoc/>
    public bool Contains(object value)
    {
      return innerList.Contains((T)value);
    }

    /// <inheritdoc/>
    public bool Contains(T value)
    {
      return innerList.Contains(value);
    }

    /// <inheritdoc/>
    public int IndexOf(object value)
    {
      return innerList.IndexOf((T)value);
    }

    /// <inheritdoc/>
    public int IndexOf(T value)
    {
      return innerList.IndexOf(value);
    }

    /// <inheritdoc/>
    public void CopyTo(T[] array, int index)
    {
      innerList.CopyTo(array, index);
    }

    /// <inheritdoc/>
    public void CopyTo(Array array, int index)
    {
      innerList.Copy(array, index);
    }

    #endregion

    #region Exceptions on: Add, Insert, Remove, RemoveAt, Clear methods

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    public void Add(T value)
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    public int Add(object value)
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    public void Insert(int index, object value)
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    public void Insert(int index, T value)
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    public void Remove(object value)
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    public bool Remove(T value)
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    public void RemoveAt(int index)
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    public void Clear()
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    #endregion

    #region GetEnumerator methods

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
      return innerList.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return innerList.GetEnumerator();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="list">The list to copy or wrap.</param>
    /// <param name="copy">Indicates whether <paramref name="list"/> must be copied or wrapped.</param> 
    public ReadOnlyList(IList<T> list, bool copy)
    {
      ArgumentValidator.EnsureArgumentNotNull(list, "list");
      if (!copy)
        innerList = list;
      else
        innerList = new List<T>(list);
      isFixedSize = copy;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="list">The list to wrap.</param>
    public ReadOnlyList(IList<T> list)
      : this(list, false)
    {
    }
  }
}