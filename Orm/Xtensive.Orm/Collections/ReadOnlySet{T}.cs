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


namespace Xtensive.Collections
{
  /// <summary>
  /// Read-only set (<see cref="ISet{T}"/>) wrapper.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("Count = {Count}")]
  public class ReadOnlySet<T>: ISet<T>, IReadOnly
  {
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private readonly ISet<T> innerSet;

    /// <inheritdoc/>
    public T this[T item]
    {
      get { return innerSet[item]; }
    }

    /// <inheritdoc/>
    public int Count
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
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="set">The set to copy or wrap.</param>
    /// <param name="copy">Indicates whether <paramref name="set"/> must be copied or wrapped.</param> 
    public ReadOnlySet(ISet<T> set, bool copy)
    {
      ArgumentValidator.EnsureArgumentNotNull(set, "set");
      innerSet = copy ? new SetSlim<T>(set) : set;
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="set">The set to wrap.</param>
    public ReadOnlySet(ISet<T> set)
      : this(set, false)
    {
    }
  }
}