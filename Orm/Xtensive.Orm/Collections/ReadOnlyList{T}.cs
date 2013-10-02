// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;


namespace Xtensive.Collections
{
  /// <summary>
  /// Read-only list (<see cref="IList"/>) wrapper.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("Count = {Count}")]
  public class ReadOnlyList<T>: 
    IList<T>, 
    IReadOnly
  {
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private readonly IList<T> innerList;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly bool isFixedSize;

    /// <summary>
    /// Empty <see cref="ReadOnlyList{T}"/> instance.
    /// </summary>
    public static ReadOnlyList<T> Empty;

    /// <inheritdoc/>
    public int Count {
      [DebuggerStepThrough]
      get { return innerList.Count; }
    }

    /// <inheritdoc/>
    public object SyncRoot {
      [DebuggerStepThrough]
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
    public virtual bool IsSynchronized
    {
      [DebuggerStepThrough]
      get { return false; }
    }

    /// <inheritdoc/>
    public bool IsReadOnly
    {
      [DebuggerStepThrough]
      get { return true; }
    }

    /// <inheritdoc/>
    public bool IsFixedSize
    {
      [DebuggerStepThrough]
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
    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion


    // Initializer

    static ReadOnlyList()
    {
      Empty = new ReadOnlyList<T>(new List<T>(0));
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="list">The list to copy or wrap.</param>
    /// <param name="copy">Indicates whether <paramref name="list"/> must be copied or wrapped.</param> 
    public ReadOnlyList(IList<T> list, bool copy)
    {
      ArgumentValidator.EnsureArgumentNotNull(list, "list");
      innerList = copy 
        ? new List<T>(list) 
        : list;
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