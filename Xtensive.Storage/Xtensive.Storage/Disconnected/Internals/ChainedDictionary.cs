// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.23

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Disconnected
{
  /// <summary>
  /// The chained dictionary.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TValue">The type of the value.</typeparam>
  public class ChainedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
  {
    private readonly IDictionary<TKey, TValue> origin;
    private Dictionary<TKey, TValue> addedItems;
    private Dictionary<TKey, TValue> removedItems;

    /// <summary>
    /// Commits changes to underlying dictionary.
    /// </summary>
    public void Commit()
    {
      foreach (var item in removedItems)
        origin.Remove(item.Key);
      foreach (var item in addedItems)
        origin.Add(item.Key, item.Value);
    }

    /// <summary>
    /// Rollbacks all changes.
    /// </summary>
    public void Rollback()
    {
      addedItems = new Dictionary<TKey, TValue>();
      removedItems = new Dictionary<TKey, TValue>();
    }

    /// <inheritdoc/>
    public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      var set = origin
        .Where(pair => !removedItems.ContainsKey(pair.Key))
        .Concat(addedItems);
      foreach (var pair in set)
        yield return pair;
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public void Add(KeyValuePair<TKey, TValue> item)
    {
      Add(item.Key, item.Value);
    }

    /// <inheritdoc/>
    public void Clear()
    {
      foreach (var pair in addedItems)
        removedItems.Add(pair.Key, pair.Value);
      addedItems.Clear();
      foreach (var pair in origin)
        if (!removedItems.ContainsKey(pair.Key))
          removedItems.Add(pair.Key, pair.Value);
    }

    /// <inheritdoc/>
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
      return ContainsKey(item.Key);
    }

    /// <inheritdoc/>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
      return Remove(item.Key);
    }

    /// <inheritdoc/>
    public int Count
    {
      get
      {
        return origin.Keys
          .Except(removedItems.Keys)
          .Concat(addedItems.Keys)
          .Count();
      }
    }

    /// <inheritdoc/>
    public bool IsReadOnly
    {
      get { return false; }
    }

    /// <inheritdoc/>
    public bool ContainsKey(TKey key)
    {
      return addedItems.ContainsKey(key)
        || (!removedItems.ContainsKey(key)
          && origin.ContainsKey(key));
    }

    /// <inheritdoc/>
    public void Add(TKey key, TValue value)
    {
      if (origin.ContainsKey(key))
        return;
      addedItems.Add(key, value);
      removedItems.Remove(key);
    }

    /// <inheritdoc/>
    public bool Remove(TKey key)
    {
      TValue value;
      if (TryGetValue(key, out value)) {
        removedItems.Add(key, value);
        addedItems.Remove(key);
        return true;
      }
      return false;
    }

    /// <inheritdoc/>
    public virtual bool TryGetValue(TKey key, out TValue value)
    {
      if (removedItems.ContainsKey(key)) {
        value = default (TValue);
        return false;
      }
      if (addedItems.TryGetValue(key, out value))
        return true;
      return origin.TryGetValue(key, out value);
    }

    /// <summary>
    /// Gets the <see cref="TValue"/> with the specified key.
    /// </summary>
    public virtual TValue this[TKey key]
    {
      get 
      {
        TValue value;
        if (TryGetValue(key, out value))
          return value;
        return default (TValue);
      }
      set { throw new NotImplementedException(); }
    }

    /// <inheritdoc/>
    public virtual ICollection<TKey> Keys
    {
      get { return origin.Keys.Except(removedItems.Keys).Concat(addedItems.Keys).ToList(); }
    }

    /// <inheritdoc/>
    public virtual ICollection<TValue> Values
    {
      get
      {
        return
          origin
            .Where(pair => !removedItems.ContainsKey(pair.Key))
            .Concat(addedItems)
            .Select(pair => pair.Value)
            .ToList();
      }
    }

    /// <summary>
    /// Gets the added items.
    /// </summary>
    public virtual IEnumerable<KeyValuePair<TKey, TValue>> AddedItems
    {
      get { return addedItems;}
    }

    /// <summary>
    /// Gets the removed items.
    /// </summary>
    public virtual IEnumerable<KeyValuePair<TKey, TValue>> RemovedItems
    {
      get { return removedItems;}
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The original dictionary.</param>
    public ChainedDictionary(IDictionary<TKey, TValue> origin)
    {
      ArgumentValidator.EnsureArgumentNotNull(origin, "origin");

      this.origin = origin;
      addedItems = new Dictionary<TKey, TValue>();
      removedItems = new Dictionary<TKey, TValue>();
    }

  }
}