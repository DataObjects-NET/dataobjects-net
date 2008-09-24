// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.11.20

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Xtensive.Core.Internals.DocTemplates;
using System.Linq;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// A <see cref="IDictionary{TKey,TValue}"/> internally storing weak references to <typeparamref name="TValue"/> items.
  /// Automatically cleaned up (see <see cref="Cleanup"/>) when items are collected by garbage collector.
  /// </summary>
  /// <typeparam name="TKey">The type of key.</typeparam>
  /// <typeparam name="TValue">The type of value.</typeparam>
  public class WeakDictionary<TKey, TValue> : IDictionary<TKey,TValue>,
    ICountable<KeyValuePair<TKey, TValue>>
    where TValue : class
  {
    // Constants
    private const int MinIterations = 10000;

    // Private
    private Dictionary<TKey, GCHandle> dictionary;
    private int iteration;

    /// <inheritdoc/>
    public TValue this[TKey key]
    {
      get {
        TValue value;
        if (!TryGetValue(key, out value))
          throw new KeyNotFoundException();

        return value;
      }
      set { SetValue(key, value); }
    }

    /// <inheritdoc/>
    long ICountable.Count
    {
      [DebuggerStepThrough]
      get { return Count; }
    }

    /// <inheritdoc/>
    public bool IsReadOnly
    {
      [DebuggerStepThrough]
      get { return false; }
    }

    /// <inheritdoc/>
    public int Count {
      [DebuggerStepThrough]
      get
      {
        IterationalCleanup();
        return dictionary.Count;
      }
    }

    #region ContainsKey, TryGetValue methods

    /// <inheritdoc/>
    public bool ContainsKey(TKey key)
    {
      TValue value;
      return (TryGetValue(key, out value));
    }

    /// <inheritdoc/>
    public bool TryGetValue(TKey key, out TValue value)
    {
      IterationalCleanup();
      GCHandle gcHandle;
      if (dictionary.TryGetValue(key, out gcHandle)) {
        value = (TValue)gcHandle.Target;
        return value!=null;
      }
      value = null;
      return false;
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
      throw new System.NotSupportedException();
    }

    ICollection<TKey> IDictionary<TKey, TValue>.Keys
    {
      get { throw new System.NotSupportedException(); }
    }

    ICollection<TValue> IDictionary<TKey, TValue>.Values
    {
      get { throw new System.NotSupportedException(); }
    }

    #endregion

    #region Modification methods: Add, SetValue, Remove, Clear

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
      Add(item.Key, item.Value);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
      return Remove(item.Key);
    }

    /// <inheritdoc/>
    public void Add(TKey key, TValue value)
    {
      IterationalCleanup();
      GCHandle weakValue;
      if (dictionary.TryGetValue(key, out weakValue) && weakValue.Target == null)
        SetValue(key, value);
      else
        dictionary.Add(key, GCHandle.Alloc(value, GCHandleType.Weak));
    }

    protected void SetValue(TKey key, TValue value)
    {
      IterationalCleanup();
      dictionary[key] = GCHandle.Alloc(value, GCHandleType.Weak);
    }

    /// <inheritdoc/>
    public bool Remove(TKey key)
    {
      IterationalCleanup();
      return dictionary.Remove(key);
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public void Clear()
    {
      dictionary.Clear();
    }

    #endregion

    #region GetEnumerator<...> methods

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      foreach (KeyValuePair<TKey, GCHandle> pair in dictionary) {
        var value = (TValue)pair.Value.Target;
        if (value != null)
          yield return new KeyValuePair<TKey, TValue>(pair.Key, value);
      }
    }

    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      this.Copy(array, arrayIndex);
    }

    /// <summary>
    /// Removes dead references from the internal dictionary.
    /// </summary>
    public void Cleanup()
    {
      var newCopy = new Dictionary<TKey, GCHandle>(dictionary.Count >> 1);
      foreach (var pair in dictionary) {
        if (pair.Value.Target != null)
          newCopy.Add(pair.Key, pair.Value);
      }
      dictionary = newCopy;
    }

    #region Private \ internal methods

    private void IterationalCleanup()
    {
      iteration++;
      if (iteration > MinIterations && iteration > (dictionary.Count << 1)) {
        Cleanup();
        iteration = 0;
      }
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public WeakDictionary()
      : this(32, null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="capacity">Initial capacity value.</param>
    public WeakDictionary(int capacity)
      : this(capacity, null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="comparer">Initial comparer value.</param>
    public WeakDictionary(IEqualityComparer<TKey> comparer)
      : this(32, comparer)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="capacity">Initial capacity value.</param>
    /// <param name="comparer">Initial comparer value.</param>
    public WeakDictionary(int capacity, IEqualityComparer<TKey> comparer)
    {
      dictionary = new Dictionary<TKey, GCHandle>(capacity, comparer);
    }
  }
}