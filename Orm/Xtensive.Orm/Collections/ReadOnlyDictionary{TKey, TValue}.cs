// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using Xtensive.Core;



namespace Xtensive.Collections
{
  /// <summary>
  /// Read-only generic dictionary (<see cref="IDictionary{TKey, TValue}"/>) wrapper.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("Count = {Count}")]
  public class ReadOnlyDictionary<TKey, TValue> :
    IDictionary<TKey, TValue>
  {
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private readonly IDictionary<TKey, TValue> innerDictionary;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly bool isFixedSize;

    /// <inheritdoc/>
    public int Count {
      [DebuggerStepThrough]
      get { return innerDictionary.Count; }
    }

    /// <inheritdoc/>
    public TValue this[TKey key]     {
      get { return innerDictionary[key]; }
      set { throw Exceptions.CollectionIsReadOnly(null); }
    }

    #region Keys, Values properties

    /// <inheritdoc/>
    public ICollection<TKey> Keys
    {
      [DebuggerStepThrough]
      get { return innerDictionary.Keys; }
    }

    /// <inheritdoc/>
    public ICollection<TValue> Values
    {
      [DebuggerStepThrough]
      get { return innerDictionary.Values; }
    }

    #endregion

    #region IsXxx properties

    /// <summary> 
    /// Always returns <see langword="true"/>.
    /// </summary>
    /// <returns><see langword="True"/>. </returns>
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

    /// <inheritdoc/>
    public virtual bool IsSynchronized
    {
      [DebuggerStepThrough]
      get { return false; }
    }

    #endregion

    #region ContainsKey, Contains, TryGetValue, CopyTo methods

    /// <inheritdoc/>
    public bool ContainsKey(TKey key)
    {
      return innerDictionary.ContainsKey(key);
    }

    /// <inheritdoc/>
    public bool Contains(KeyValuePair<TKey, TValue> keyValuePair)
    {
      return innerDictionary.Contains(keyValuePair);
    }

    /// <inheritdoc/>
    public bool Contains(object key)
    {
      ArgumentValidator.EnsureArgumentIs<TKey>(key, "key");
      return ContainsKey((TKey)key);
    }

    /// <inheritdoc/>
    public bool TryGetValue(TKey key, out TValue value)
    {
      return innerDictionary.TryGetValue(key, out value);
    }

    /// <inheritdoc/>
    public void CopyTo(Array array, int index)
    {
      if (innerDictionary is ICollection)
        ((ICollection)innerDictionary).CopyTo(array, index);
      else
        innerDictionary.Copy(array, index);
    }

    /// <inheritdoc/>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      innerDictionary.CopyTo(array, arrayIndex);
    }

    #endregion

    #region Exceptions on: Add, Remove, Clear methods

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    public void Add(TKey key, TValue value)
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    public void Add(KeyValuePair<TKey, TValue> item)
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    public void Add(object key, object value)
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    public bool Remove(TKey key)
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    public void Remove(object key)
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
    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable)innerDictionary).GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      return innerDictionary.GetEnumerator();
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="dictionary">The dictionary to copy or wrap.</param>
    /// <param name="copy">Indicates whether <paramref name="dictionary"/> must be copied or wrapped.</param> 
    public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary, bool copy)
    {
      ArgumentValidator.EnsureArgumentNotNull(dictionary, "dictionary");
      if (!copy)
        innerDictionary = dictionary;
      else
        innerDictionary = new Dictionary<TKey, TValue>(dictionary);
      isFixedSize = copy;
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="dictionary">The dictionary to wrap.</param>
    public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
      : this(dictionary, false)
    {
    }
  }
}