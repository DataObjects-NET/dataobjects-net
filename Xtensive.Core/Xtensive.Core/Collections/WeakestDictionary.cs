// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.10.30

using System.Collections.Generic;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// A <see cref="IDictionary{TKey,TValue}"/> internally storing weak references to both <typeparamref name="TKey"/> and <typeparamref name="TValue"/> items.
  /// Automatically cleaned up (see <see cref="Cleanup"/>) when items are collected by garbage collector.
  /// </summary>
  /// <typeparam name="TKey">The type of key.</typeparam>
  /// <typeparam name="TValue">The type of value.</typeparam>
  public class WeakestDictionary<TKey, TValue>
    : DictionaryBaseSlim<TKey, TValue>
    where TKey : class
    where TValue : class
  {
    private const int MinIterations = 10000;
    private readonly Dictionary<object, WeakReference<TValue>> dictionary;

    private readonly WeakKeyComparer<TKey> comparer;
    private int iteration;

    /// <inheritdoc/>
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
      return dictionary.ContainsKey(key);
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
      dictionary.Add(new WeakKeyReference<TKey>(key, comparer), WeakReference<TValue>.Create(value));
    }

    /// <inheritdoc/>
    public override bool Remove(TKey key)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      IterationalCleanup();
      return dictionary.Remove(key);
    }

    protected override void SetValue(TKey key, TValue value)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      IterationalCleanup();
      WeakReference<TKey> weakKey = new WeakKeyReference<TKey>(key, comparer);
      dictionary[weakKey] = WeakReference<TValue>.Create(value);
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      dictionary.Clear();
      iteration = 0;
    }

    #endregion
    
    #region GetEnumerator<...> methods

    /// <inheritdoc/>
    public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      IterationalCleanup();
      List<WeakReference<TKey>> toRemove = null;
      foreach (KeyValuePair<object, WeakReference<TValue>> kvp in dictionary) {
        WeakReference<TKey> weakKey = (WeakReference<TKey>)(kvp.Key);
        WeakReference<TValue> weakValue = kvp.Value;
        TKey key = weakKey.Target;
        TValue value = weakValue.Target;
        if (weakKey.IsAlive && weakValue.IsAlive)
          yield return new KeyValuePair<TKey, TValue>(key, value);
        else {
          if (toRemove==null)
            toRemove = new List<WeakReference<TKey>>();
          toRemove.Add(weakKey);
        }
      }
      if (toRemove!=null)
        foreach (WeakReference<TKey> key in toRemove)
          dictionary.Remove(key);
    }

    #endregion

    /// <summary>
    /// Removes dead references from the internal dictionary.
    /// </summary>
    public void Cleanup()
    {
      List<object> toRemove = null;
      foreach (KeyValuePair<object, WeakReference<TValue>> pair in dictionary) {
        WeakReference<TKey> weakKey = (WeakReference<TKey>)(pair.Key);
        WeakReference<TValue> weakValue = pair.Value;

        if (!weakKey.IsAlive || !weakValue.IsAlive) {
          if (toRemove==null)
            toRemove = new List<object>();
          toRemove.Add(weakKey);
        }
      }

      if (toRemove!=null) {
        foreach (object key in toRemove)
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
    public WeakestDictionary()
      : this(0, null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="capacity">Initial capacity value.</param>
    public WeakestDictionary(int capacity)
      : this(capacity, null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="comparer">Initial comparer value.</param>
    public WeakestDictionary(IEqualityComparer<TKey> comparer)
      : this(0, comparer)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="capacity">Initial capacity value.</param>
    /// <param name="comparer">Initial comparer value.</param>
    public WeakestDictionary(int capacity, IEqualityComparer<TKey> comparer)
    {
      this.comparer = new WeakKeyComparer<TKey>(comparer);
      dictionary = new Dictionary<object, WeakReference<TValue>>(capacity, this.comparer);
    }
  }
}