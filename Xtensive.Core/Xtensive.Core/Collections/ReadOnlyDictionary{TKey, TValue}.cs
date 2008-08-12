// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;
using Xtensive.Core.Threading;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// Read-only generic dictionary (<see cref="IDictionary{TKey, TValue}"/>) wrapper.
  /// </summary>
  [Serializable]
  public class ReadOnlyDictionary<TKey, TValue> :
    IDictionary<TKey, TValue>,
    IDictionary,
    ICountable<KeyValuePair<TKey, TValue>>,
    ISynchronizable,
    IReadOnly
  {
    private readonly IDictionary<TKey, TValue> innerDictionary;
    private readonly bool isFixedSize;
    private ReadOnlyCollection<TKey> innerKeyDictionary;
    private ReadOnlyCollection<TValue> innerValueDictionary;

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public int Count
    {
      get { return innerDictionary.Count; }
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    long ICountable.Count
    {
      get { return Count; }
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public object SyncRoot {
      get { return this; }
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown by "set" accessor (setter).</exception>
    public TValue this[TKey key]
    {
      get { return innerDictionary[key]; }
      set { throw Exceptions.CollectionIsReadOnly(null); }
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Always thrown by "set" accessor (setter).</exception>
    public object this[object key] {
      get {
        ArgumentValidator.EnsureArgumentIs<TKey>(key, "key");
        return innerDictionary[(TKey)key];
      }
      set { throw Exceptions.CollectionIsReadOnly(null); }
    }

    #region Keys, Values properties

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public ICollection<TKey> Keys
    {
      get { return innerDictionary.Keys; }
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public ICollection<TValue> Values
    {
      get { return innerDictionary.Values; }
    }

    ICollection IDictionary.Keys
    {
      get
      {
        ICollection<TKey> keys = innerDictionary.Keys;
        if (innerKeyDictionary == null) {
          innerKeyDictionary = new ReadOnlyCollection<TKey>(keys);
          return innerKeyDictionary;
        }
        else {
          // Potentialy innerDictionary.Keys collection can change, that is why validation should be performed.
          if (!innerKeyDictionary.IsWrapperOf(keys))
            innerKeyDictionary = new ReadOnlyCollection<TKey>(keys);
          return innerKeyDictionary;
        }
      }
    }

    ICollection IDictionary.Values
    {
      get
      {
        ICollection<TValue> values = innerDictionary.Values;
        if (innerValueDictionary == null) {
          innerValueDictionary = new ReadOnlyCollection<TValue>(values);
          return innerValueDictionary;
        }
        else {
          // Potentialy innerDictionary.Values collection can change, that is why validation should be performed.
          if (!innerValueDictionary.IsWrapperOf(values))
            innerValueDictionary = new ReadOnlyCollection<TValue>(values);
          return innerValueDictionary;
        }
      }
    }

    #endregion

    #region IsXxx properties

    /// <summary> 
    /// Always returns <see langword="true"/>.
    /// </summary>
    /// <returns><see langword="True"/>. </returns>
    [DebuggerStepThrough]
    public bool IsReadOnly
    {
      get { return true; }
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public bool IsFixedSize
    {
      get { return isFixedSize; }
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public virtual bool IsSynchronized
    {
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

    /// <inheritdoc/>
    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
      return new DictionaryEnumerator<TKey, TValue>(
        innerDictionary.GetEnumerator(),
        delegate(KeyValuePair<TKey, TValue> value) { return value; });
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
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
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="dictionary">The dictionary to wrap.</param>
    public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
      : this(dictionary, false)
    {
    }
  }
}