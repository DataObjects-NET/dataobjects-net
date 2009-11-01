// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ustinov
// Created:    2007.05.28

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Collections
{
  internal class TopDeque<K, V> : 
    IEnumerable<V>
  {
    private readonly LinkedList<Pair<K,V>>                    list = new LinkedList<Pair<K,V>>();
    private readonly Dictionary<K, LinkedListNode<Pair<K,V>>>  map = new Dictionary<K, LinkedListNode<Pair<K, V>>>();

    public int Count
    {
      get { return list.Count; }
    }

    public V this[K key] {
      get {
        LinkedListNode<Pair<K, V>> valueContainer;
        if (map.TryGetValue(key, out valueContainer))
          return valueContainer.Value.Second;
        throw new ArgumentException(Strings.ExNoObjectWithSpecifiedKey, "key");
      }
      set {
        LinkedListNode<Pair<K, V>> valueContainer;
        if (map.TryGetValue(key, out valueContainer))
          valueContainer.Value = new Pair<K, V>(key, value);
        throw new ArgumentException(Strings.ExNoObjectWithSpecifiedKey, "key");
      }
    }

    public bool Contains(K key)
    {
      return map.ContainsKey(key);
    }

    #region Properties: TopXxx, BottomXxx

    public V Top {
      get {
        if (list.Count==0)
          throw new InvalidOperationException(Strings.ExCollectionIsEmpty);
        return list.First.Value.Second;
      }
    }

    public V Bottom {
      get {
        if (list.Count==0)
          throw new InvalidOperationException(Strings.ExCollectionIsEmpty);
        return list.Last.Value.Second;
      }
    }

    public K TopKey {
      get {
        if (list.Count==0)
          throw new InvalidOperationException(Strings.ExCollectionIsEmpty);
        return list.First.Value.First;
      }
    }

    public K BottomKey {
      get {
        if (list.Count==0)
          throw new InvalidOperationException(Strings.ExCollectionIsEmpty);
        return list.Last.Value.First;
      }
    }

    #endregion

    #region PeekXxx methods

    public V PeekTop()
    {
      if (list.Count==0)
        throw new InvalidOperationException(Strings.ExCollectionIsEmpty);

      LinkedListNode<Pair<K, V>> valueContainer = list.First;
      list.Remove(valueContainer);
      Pair<K, V> keyValuePair = valueContainer.Value;
      map.Remove(keyValuePair.First);
      return keyValuePair.Second;
    }

    public V PeekBottom()
    {
      if (list.Count==0)
        throw new InvalidOperationException(Strings.ExCollectionIsEmpty);

      LinkedListNode<Pair<K, V>> valueContainer = list.Last;
      list.Remove(valueContainer);
      Pair<K, V> keyValuePair = valueContainer.Value;
      map.Remove(keyValuePair.First);
      return keyValuePair.Second;
    }

    #endregion

    #region MoveXxx methods

    public void MoveToTop(K key)
    {
      LinkedListNode<Pair<K, V>> valueContainer;
      if (!map.TryGetValue(key, out valueContainer))
        throw new ArgumentException(Strings.ExNoObjectWithSpecifiedKey, "key");
      list.Remove(valueContainer);
      list.AddFirst(valueContainer);
    }

    public void MoveToBottom(K key)
    {
      LinkedListNode<Pair<K, V>> valueContainer;
      if (!map.TryGetValue(key, out valueContainer))
        throw new ArgumentException(Strings.ExNoObjectWithSpecifiedKey, "key");
      list.Remove(valueContainer);
      list.AddLast(valueContainer);
    }

    #endregion

    #region AddXxx, Remove, Clear methods

    public void AddToTop(K key, V value)
    {
      if (map.ContainsKey(key))
        throw new InvalidOperationException(Strings.ExCollectionAlreadyContainsItemWithSpecifiedKey);

      Pair<K, V> keyValuePair = new Pair<K, V>(key, value);
      LinkedListNode<Pair<K, V>> valueContainer = list.AddFirst(keyValuePair);
      map.Add(key, valueContainer);
    }

    public void AddToBottom(K key, V value)
    {
      if (map.ContainsKey(key))
        throw new InvalidOperationException(Strings.ExCollectionAlreadyContainsItemWithSpecifiedKey);

      Pair<K, V> entry = new Pair<K, V>(key, value);
      LinkedListNode<Pair<K, V>> valueContainer = list.AddLast(entry);
      map.Add(key, valueContainer);
    }

    public void Remove(K key)
    {
      LinkedListNode<Pair<K, V>> valueContainer;
      if (map.TryGetValue(key, out valueContainer)) {
        list.Remove(valueContainer);
        map.Remove(key);
      }
    }

    public void Clear()
    {
      list.Clear();
      map.Clear();
    }

    #endregion

    #region IEnumerable methods

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<V> GetEnumerator()
    {
      foreach (Pair<K, V> keyValuePair in list)
        yield return keyValuePair.Second;
    }

    #endregion
  }
}