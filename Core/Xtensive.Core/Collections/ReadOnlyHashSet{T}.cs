// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.14

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Collections
{
  /// <summary>
  /// Read-only set (<see cref="ISet{T}"/>) wrapper.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("Count = {Count}")]
  public class ReadOnlyHashSet<T>: ISet<T>, IReadOnly
  {
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private readonly HashSet<T> innerSet;

    /// <inheritdoc/>
    public T this[T item]
    {
      get { return innerSet.Contains(item) ? item : default(T); }
    }

    /// <inheritdoc/>
    public int Count
    {
      [DebuggerStepThrough]
      get { return innerSet.Count; }
    }

    /// <inheritdoc/>
    long ICountable.Count
    {
      [DebuggerStepThrough]
      get { return innerSet.Count; }
    }

    /// <inheritdoc/>
    public bool IsReadOnly
    {
      [DebuggerStepThrough]
      get { return true; }
    }

    /// <inheritdoc/>
    public IEqualityComparer<T> Comparer
    {
      get { return innerSet.Comparer; }
    }

    #region Contains, CopyTo methods

    /// <inheritdoc/>
    public bool Contains(T element)
    {
      return innerSet.Contains(element);
    }

    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex)
    {
      innerSet.CopyTo(array, arrayIndex);
    }

    #endregion

    #region Exceptions on: Add, Remove, RemoveWhere, Clear methods

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    public bool Add(T item)
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    void ICollection<T>.Add(T item)
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    public bool Remove(T item)
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    public int RemoveWhere(Predicate<T> match)
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
      return innerSet.GetEnumerator();
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="set">The set to copy or wrap.</param>
    /// <param name="copy">Indicates whether <paramref name="set"/> must be copied or wrapped.</param> 
    public ReadOnlyHashSet(HashSet<T> set, bool copy)
    {
      ArgumentValidator.EnsureArgumentNotNull(set, "set");
      innerSet = copy ? new HashSet<T>(set) : set;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="set">The set to wrap.</param>
    public ReadOnlyHashSet(HashSet<T> set)
      : this(set, false)
    {
    }
  }
}