// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.11.20

using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// A <see cref="IDictionary{TKey,TValue}"/> internally storing weak references to <typeparamref name="TValue"/> items.
  /// Automatically cleaned up (see <see cref="Cleanup"/>) when items are collected by garbage collector.
  /// </summary>
  /// <typeparam name="TKey">The type of key.</typeparam>
  /// <typeparam name="TValue">The type of value.</typeparam>
  public class WeakDictionary<TKey, TValue>
    : DictionaryBaseSlim<TKey, TValue>
    where TValue : class
  {
    // Constants
    private const int MinIterations = 10000;

    // Private
    private readonly Dictionary<TKey, WeakReference<TValue>> dictionary;
    private int iteration;

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override int Count {
      get {
        IterationalCleanup();
        return dictionary.Count;
      }
    }

    #region ContainsKey, TryGetValue methods

    /// <inheritdoc/>
    public override bool ContainsKey(TKey key)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      IterationalCleanup();
      TValue value;
      return (TryGetValue(key, out value));
    }

    /// <inheritdoc/>
    public override bool TryGetValue(TKey key, out TValue value)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      IterationalCleanup();
      WeakReference<TValue> weakValue;
      if (dictionary.TryGetValue(key, out weakValue)) {
        value = weakValue.Target;
        if (!weakValue.IsAlive)
          dictionary.Remove(key);
        return weakValue.IsAlive;
      }
      value = null;
      return false;
    }

    #endregion

    #region Modification methods: Add, SetValue, Remove, Clear

    /// <inheritdoc/>
    public override void Add(TKey key, TValue value)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      IterationalCleanup();
      WeakReference<TValue> weakValue;
      if (dictionary.TryGetValue(key, out weakValue) && !weakValue.IsAlive)
        SetValue(key, value);
      else
        dictionary.Add(key, WeakReference<TValue>.Create(value));
    }

    protected override void SetValue(TKey key, TValue value)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      IterationalCleanup();
      dictionary[key] = WeakReference<TValue>.Create(value);
    }

    /// <inheritdoc/>
    public override bool Remove(TKey key)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      IterationalCleanup();
      return dictionary.Remove(key);
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      dictionary.Clear();
    }

    #endregion

    #region GetEnumerator<...> methods

    /// <inheritdoc/>
    public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      IterationalCleanup();
      List<TKey> keysToRemove = new List<TKey>();
      foreach (KeyValuePair<TKey, WeakReference<TValue>> pair in dictionary) {
        WeakReference<TValue> weakValue = pair.Value;
        TValue value = weakValue.Target;
        if (weakValue.IsAlive)
          yield return new KeyValuePair<TKey, TValue>(pair.Key, value);
        else 
          keysToRemove.Add(pair.Key);
      }
      foreach (TKey key in keysToRemove) {
        dictionary.Remove(key);
      }
    }

    #endregion

    /// <summary>
    /// Removes dead references from the internal dictionary.
    /// </summary>
    public void Cleanup()
    {
      List<TKey> itemsToDelete = new List<TKey>();
      foreach (KeyValuePair<TKey, WeakReference<TValue>> keyValuePair in dictionary) {
        if (!keyValuePair.Value.IsAlive)
          itemsToDelete.Add(keyValuePair.Key);
      }
      foreach (TKey key in itemsToDelete) {
        dictionary.Remove(key);
      }
    }

    #region Private \ internal methods

    private void IterationalCleanup()
    {
      iteration++;
      if (iteration > MinIterations && iteration > dictionary.Count) {
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
      : this(0, null)
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
      : this(0, comparer)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="capacity">Initial capacity value.</param>
    /// <param name="comparer">Initial comparer value.</param>
    public WeakDictionary(int capacity, IEqualityComparer<TKey> comparer)
    {
      dictionary = new Dictionary<TKey, WeakReference<TValue>>(capacity, comparer);
    }
  }
}