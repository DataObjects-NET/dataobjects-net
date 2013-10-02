// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ustinov
// Created:    2007.05.28

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;


namespace Xtensive.Collections
{
  /// <summary>
  /// Default <see cref="ITopDeque{K,V}"/> implementation.
  /// </summary>
  /// <typeparam name="K">Type of the key.</typeparam>
  /// <typeparam name="V">Type of the value.</typeparam>
  [Serializable]
  [DebuggerDisplay("Count = {Count}")]
  public class TopDeque<K, V> : ITopDeque<K, V>
  {
    private readonly System.Collections.Generic.LinkedList<Pair<K,V>> list;
    private readonly Dictionary<K, LinkedListNode<Pair<K,V>>> map;

    /// <inheritdoc/>
    public int Count
    {
      [DebuggerStepThrough]
      get { return list.Count; }
    }

    /// <inheritdoc/>
    /// <exception cref="KeyNotFoundException">There is no specified key.</exception>
    public V this[K key] {
      get {
        LinkedListNode<Pair<K, V>> valueContainer;
        if (map.TryGetValue(key, out valueContainer))
          return valueContainer.Value.Second;
        throw new KeyNotFoundException(Strings.ExNoObjectWithSpecifiedKey);
      }
      set {
        LinkedListNode<Pair<K, V>> valueContainer;
        if (map.TryGetValue(key, out valueContainer))
          valueContainer.Value = new Pair<K, V>(key, value);
        throw new KeyNotFoundException(Strings.ExNoObjectWithSpecifiedKey);
      }
    }

    /// <inheritdoc/>
    public bool TryGetValue(K key, out V value)
    {
      LinkedListNode<Pair<K, V>> valueContainer;
      if (map.TryGetValue(key, out valueContainer)) {
        value = valueContainer.Value.Second;
        return true;
      }
      value = default(V);
      return false;
    }

    /// <inheritdoc/>
    public bool TryGetValue(K key, bool moveToTop, out V value)
    {
      LinkedListNode<Pair<K, V>> valueContainer;
      if (map.TryGetValue(key, out valueContainer)) {
        if (moveToTop) {
          list.Remove(valueContainer);
          list.AddFirst(valueContainer);
        }
        value = valueContainer.Value.Second;
        return true;
      }
      value = default(V);
      return false;
    }

    /// <inheritdoc/>
    public bool TryChangeValue(K key, V value, bool moveToTop, bool replaceIfExists, out V oldValue)
    {
      LinkedListNode<Pair<K, V>> valueContainer;
      if (map.TryGetValue(key, out valueContainer)) {
        oldValue = valueContainer.Value.Second;
        if (moveToTop) {
          list.Remove(valueContainer);
          list.AddFirst(valueContainer);
        }
        if (replaceIfExists)
          valueContainer.Value = new Pair<K, V>(key, value);
        return true;
      }
      else {
        oldValue = default(V);
        valueContainer = list.AddFirst(new Pair<K, V>(key, value));
        try {
          map.Add(key, valueContainer);
        }
        catch (Exception e) {
          list.RemoveFirst();
          throw;
        }
        return false;
      }
    }

    /// <inheritdoc/>
    public bool Contains(K key)
    {
      return map.ContainsKey(key);
    }

    #region Properties: TopXxx, BottomXxx

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Collection is empty.</exception>
    public V Top {
      get {
        if (list.Count==0)
          throw new InvalidOperationException(Strings.ExCollectionIsEmpty);
        return list.First.Value.Second;
      }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Collection is empty.</exception>
    public V Bottom {
      get {
        if (list.Count==0)
          throw new InvalidOperationException(Strings.ExCollectionIsEmpty);
        return list.Last.Value.Second;
      }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Collection is empty.</exception>
    public K TopKey {
      get {
        if (list.Count==0)
          throw new InvalidOperationException(Strings.ExCollectionIsEmpty);
        return list.First.Value.First;
      }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Collection is empty.</exception>
    public K BottomKey {
      get {
        if (list.Count==0)
          throw new InvalidOperationException(Strings.ExCollectionIsEmpty);
        return list.Last.Value.First;
      }
    }

    #endregion

    #region PeekXxx methods

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Collection is empty.</exception>
    public V PopTop()
    {
      if (list.Count==0)
        throw new InvalidOperationException(Strings.ExCollectionIsEmpty);

      var valueContainer = list.First;
      list.Remove(valueContainer);
      var keyValuePair = valueContainer.Value;
      map.Remove(keyValuePair.First);
      return keyValuePair.Second;
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Collection is empty.</exception>
    public V PopBottom()
    {
      if (list.Count==0)
        throw new InvalidOperationException(Strings.ExCollectionIsEmpty);

      var valueContainer = list.Last;
      list.Remove(valueContainer);
      var keyValuePair = valueContainer.Value;
      map.Remove(keyValuePair.First);
      return keyValuePair.Second;
    }

    #endregion

    #region MoveXxx methods

    /// <inheritdoc/>
    /// <exception cref="KeyNotFoundException">There is no specified key.</exception>
    public void MoveToTop(K key)
    {
      LinkedListNode<Pair<K, V>> valueContainer;
      if (!map.TryGetValue(key, out valueContainer))
        throw new KeyNotFoundException(Strings.ExNoObjectWithSpecifiedKey);
      list.Remove(valueContainer);
      list.AddFirst(valueContainer);
    }

    /// <inheritdoc/>
    /// <exception cref="KeyNotFoundException">There is no specified key.</exception>
    public void MoveToBottom(K key)
    {
      LinkedListNode<Pair<K, V>> valueContainer;
      if (!map.TryGetValue(key, out valueContainer))
        throw new KeyNotFoundException(Strings.ExNoObjectWithSpecifiedKey);
      list.Remove(valueContainer);
      list.AddLast(valueContainer);
    }

    #endregion

    #region AddXxx, Remove, Clear methods

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">The key is already added.</exception>
    public void AddToTop(K key, V value)
    {
      var valueContainer = list.AddFirst(new Pair<K, V>(key, value));
      try {
        map.Add(key, valueContainer);
      }
      catch (Exception e) {
        list.RemoveFirst();
        if (e is ArgumentException)
          throw new InvalidOperationException(Strings.ExCollectionAlreadyContainsItemWithSpecifiedKey);
        else
          throw;
      }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">The key is already added.</exception>
    public void AddToBottom(K key, V value)
    {
      var valueContainer = list.AddLast(new Pair<K, V>(key, value));
      try {
        map.Add(key, valueContainer);
      }
      catch (Exception e) {
        list.RemoveLast();
        if (e is ArgumentException)
          throw new InvalidOperationException(Strings.ExCollectionAlreadyContainsItemWithSpecifiedKey);
        else
          throw;
      }
    }

    /// <inheritdoc/>
    public void Remove(K key)
    {
      LinkedListNode<Pair<K, V>> valueContainer;
      if (map.TryGetValue(key, out valueContainer)) {
        list.Remove(valueContainer);
        map.Remove(key);
      }
    }

    /// <inheritdoc/>
    public void Clear()
    {
      list.Clear();
      map.Clear();
    }

    #endregion

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator<V> GetEnumerator()
    {
      foreach (Pair<K, V> keyValuePair in list)
        yield return keyValuePair.Second;
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public TopDeque()
    {
      list = new System.Collections.Generic.LinkedList<Pair<K, V>>();
      map = new Dictionary<K, LinkedListNode<Pair<K, V>>>();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="capacity">The initial capacity.</param>
    public TopDeque(int capacity)
    {
      list = new System.Collections.Generic.LinkedList<Pair<K, V>>();
      map = new Dictionary<K, LinkedListNode<Pair<K, V>>>(capacity);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="keyComparer">The key comparer.</param>
    public TopDeque(IEqualityComparer<K> keyComparer)
    {
      list = new System.Collections.Generic.LinkedList<Pair<K, V>>();
      map = new Dictionary<K, LinkedListNode<Pair<K, V>>>(keyComparer);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="keyComparer">The key comparer.</param>
    /// <param name="capacity">The initial capacity.</param>
    public TopDeque(IEqualityComparer<K> keyComparer, int capacity)
    {
      list = new System.Collections.Generic.LinkedList<Pair<K, V>>();
      map = new Dictionary<K, LinkedListNode<Pair<K, V>>>(capacity, keyComparer);
    }
  }
}