// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.26

namespace Xtensive.Core.Collections
{
  public interface ITopDeque<K,V> : ICountable<V>
  {
    int Count { get; }

    V this[K key] { get; set; }
    bool TryGetValue(K key, out V value);
    bool TryGetValue(K key, bool moveToTop, out V value);
    bool TryChangeValue(K key, V value, bool moveToTop, out V oldValue);
    bool Contains(K key);

    V Top { get; }
    V Bottom { get; }
    K TopKey { get; }
    K BottomKey { get; }

    V PeekTop();
    V PeekBottom();

    void MoveToTop(K key);
    void MoveToBottom(K key);

    void AddToTop(K key, V value);
    void AddToBottom(K key, V value);

    void Remove(K key);

    void Clear();
  }
}