// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.12

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Collections
{
  /// <summary>
  /// Base class for dictionary mapping keys to values.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TValue">The type of the value.</typeparam>
  // The code is taken from: http://blogs.msdn.com/nicholg/archive/2006/06/04/616787.aspx
  [DebuggerDisplay("Count = {Count}")]
  [DebuggerTypeProxy(PREFIX + "DictionaryDebugView`2" + SUFFIX)]
  public abstract partial class DictionaryBaseSlim<TKey, TValue> : 
    IDictionary<TKey, TValue>,
    ICountable<KeyValuePair<TKey, TValue>>
  {
    internal const string PREFIX = "System.Collections.Generic.Mscorlib_";
    internal const string SUFFIX = ",mscorlib,Version=2.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089";
    private KeyCollection keys;
    private ValueCollection values;

    // Abstract methods (to implement in descendants)

    /// <inheritdoc/>
    public abstract int Count { get; }

    /// <inheritdoc/>
    long ICountable.Count
    {
      [DebuggerStepThrough]
      get { return Count; }
    }

    /// <inheritdoc/>
    public abstract void Clear();

    /// <inheritdoc/>
    public abstract void Add(TKey key, TValue value);

    /// <inheritdoc/>
    protected abstract void SetValue(TKey key, TValue value);

    /// <inheritdoc/>
    public abstract bool ContainsKey(TKey key);

    /// <inheritdoc/>
    public abstract bool Remove(TKey key);

    /// <inheritdoc/>
    public abstract bool TryGetValue(TKey key, out TValue value);

    /// <inheritdoc/>
    public abstract IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();

    #region Non-abstract methods (IDictionary, etc. implementation)

    /// <inheritdoc/>
    public bool IsReadOnly
    {
      [DebuggerStepThrough]
      get { return false; }
    }

    /// <inheritdoc/>
    public ICollection<TKey> Keys {
      [DebuggerStepThrough]
      get {
        if (keys==null)
          keys = new KeyCollection(this);
        return keys;
      }
    }

    /// <inheritdoc/>
    public ICollection<TValue> Values {
      [DebuggerStepThrough]
      get {
        if (values==null)
          values = new ValueCollection(this);
        return values;
      }
    }

    /// <inheritdoc/>
    public TValue this[TKey key] {
      get {
        TValue value;
        if (!TryGetValue(key, out value))
          throw new KeyNotFoundException();

        return value;
      }
      set { SetValue(key, value); }
    }

    /// <inheritdoc/>
    public void Add(KeyValuePair<TKey, TValue> item)
    {
      Add(item.Key, item.Value);
    }

    /// <inheritdoc/>
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
      TValue value;
      if (!TryGetValue(item.Key, out value))
        return false;

      return AdvancedComparerStruct<TValue>.System.Equals(value, item.Value);
    }

    /// <inheritdoc/>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      this.Copy(array, arrayIndex);
    }

    /// <inheritdoc/>
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
      if (!Contains(item))
        return false;

      return Remove(item.Key);
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
    protected DictionaryBaseSlim()
    {
    }
  }
}