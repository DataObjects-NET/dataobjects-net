// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.22

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Resources;

namespace Xtensive.Collections
{
  /// <summary>
  /// The fast dictionary-like collection using keys of type <see cref="int"/>.
  /// </summary>
  /// <typeparam name="TValue">The type of a value.</typeparam>
  [Serializable]
  [DebuggerDisplay("Count = {Count}")]
  public sealed class IntDictionary<TValue> : IEnumerable<KeyValuePair<int, TValue>>
  {
    private const int NonExistingKeyIndex = int.MinValue;
    private const int ExistingKeyIndex = int.MinValue + 1;
    private const int MinCapacity = 8; // Must be the power of 2!
    private const double MinFillFactor = 0.75 * 0.5 * 0.5;
    private const double MaxFillFactor = 0.75;

    private Triplet<int, TValue, KeyValuePair<int, TValue>[]>[] items;
    private int mask;
    private int count;

    /// <summary>
    /// Gets the number of elements contained in a collection.
    /// </summary>
    public int Count {
      get { return count; }
    }

    /// <summary>
    /// Gets a value by the specified key.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Specified key not found.</exception>
    public TValue this[int key]
    {
      get {
        var index = key & mask;
        var triplet = items[index];
        if (triplet.First==key)
          return triplet.Second;
        TValue value;
        if (triplet.First==ExistingKeyIndex)
          if(TryGetValueInBucket(triplet.Third, key, out value) >= 0)
            return value;
        throw new KeyNotFoundException();
      }
      set {
        TValue tmpValue;
        if (TryGetValue(key, out tmpValue))
          Remove(key);
        Add(key, value);
      }
    }

    /// <summary>
    /// Tries to get a value by the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns><see langword="true" /> if the key was found; 
    /// otherwise <see langword="false" />.</returns>
    public bool TryGetValue(int key, out TValue value)
    {
      var index = key & mask;
      var triplet = items[index];
      if (triplet.First==key) {
        value = triplet.Second;
        return true;
      }
      if (triplet.First==ExistingKeyIndex)
        return TryGetValueInBucket(triplet.Third, key, out value) >= 0;
      value = default(TValue);
      return false;
    }

    /// <summary>
    /// Adds the item to the collection.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <exception cref="InvalidOperationException">Key already exists.</exception>
    public void Add(int key, TValue value)
    {
      if ((count+1) >= (items.Length * MaxFillFactor))
        Resize(checked (items.Length * 2));
      var index = key & mask;
      var triplet = items[index];
      if (triplet.First==key)
        throw new InvalidOperationException(Strings.ExKeyAlreadyExists);
      else if (triplet.First==ExistingKeyIndex) {
        var bucket = InsertIntoBucket(triplet.Third, new KeyValuePair<int, TValue>(key, value));
        items[index] = new Triplet<int, TValue, KeyValuePair<int, TValue>[]>(
          ExistingKeyIndex, default(TValue), bucket);
        count++;
      }
      else if (triplet.First==NonExistingKeyIndex) {
        items[index] = new Triplet<int, TValue, KeyValuePair<int, TValue>[]>(
          key, value, null);
        count++;
      }
      else {
        items[index] = new Triplet<int, TValue, KeyValuePair<int, TValue>[]>(
          ExistingKeyIndex, default(TValue), new[] {
            new KeyValuePair<int, TValue>(triplet.First, triplet.Second)
            });
        Add(key, value);
      }
    }

    /// <summary>
    /// Removes the item with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns><see langword="true" /> if an item was found and removed; 
    /// otherwise <see langword="false" />.</returns>
    public bool Remove(int key)
    {
      if (items.Length>MinCapacity && ((count-1) < (items.Length * MinFillFactor)))
        Resize(checked (items.Length / 2));
      var index = key & mask;
      var triplet = items[index];
      if (triplet.First==key) {
        items[index] = new Triplet<int, TValue, KeyValuePair<int, TValue>[]>(
          NonExistingKeyIndex, default(TValue), null);
        count--;
        return true;
      }
      else if (triplet.First==ExistingKeyIndex) {
        var bucket = triplet.Third;
        if(!RemoveFromBucket(ref bucket, key))
          return false;
        if (bucket.Length==1)
          items[index] = new Triplet<int, TValue, KeyValuePair<int, TValue>[]>(
            bucket[0].Key, bucket[0].Value, null);
        else
          items[index] = new Triplet<int, TValue, KeyValuePair<int, TValue>[]>(
            ExistingKeyIndex, default(TValue), bucket);
        count--;
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Removes all items.
    /// </summary>
    public void Clear()
    {
      Initialize(MinCapacity);
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<int, TValue>> GetEnumerator()
    {
      foreach (var triplet in items) {
        if (triplet.First>=0)
          yield return new KeyValuePair<int, TValue>(triplet.First, triplet.Second);
        else if (triplet.First==ExistingKeyIndex)
          foreach (var pair in triplet.Third)
            yield return pair;
      }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #region Private / internal members

    private static int TryGetValueInBucket(KeyValuePair<int, TValue>[] bucket, int key, out TValue value)
    {
      var start = 0;
      var end = bucket.Length - 1;
      KeyValuePair<int, TValue> current;
      while (end >= start) {
        if (end==start) {
          current = bucket[start];
          if (key==current.Key) {
            value = current.Value;
            return start;
          }
          else {
            value = default(TValue);
            return ~start;
          }
        }
        var middle = ((end - start) / 2) + start;
        current = bucket[middle];
        if (key==current.Key) {
          value = current.Value;
          return middle;
        }
        else if (key > current.Key)
          start = middle + 1;
        else
          end = middle - 1;
      }
      value = default(TValue);
      return ~start;
    }

    private void Resize(int newCapacity)
    {
      var pairs = this.ToList();
      Initialize(newCapacity);
      foreach (var pair in pairs)
        Add(pair.Key, pair.Value);
    }

    /// <exception cref="InvalidOperationException">Key already exists.</exception>
    private static KeyValuePair<int, TValue>[] InsertIntoBucket(KeyValuePair<int, TValue>[] bucket, KeyValuePair<int, TValue> newPair)
    {
      TValue tmpValue;
      int index = TryGetValueInBucket(bucket, newPair.Key, out tmpValue);
      if (index>=0)
        throw new InvalidOperationException(Strings.ExKeyAlreadyExists);
      index = ~index;
      if (newPair.Key>bucket[index].Key)
        index++;
      Array.Resize(ref bucket, bucket.Length + 1);
      Array.Copy(bucket, index, bucket, index + 1, bucket.Length - index - 1);
      bucket[index] = newPair;
      return bucket;
    }

    /// <exception cref="KeyNotFoundException">Specified key not found.</exception>
    private static bool RemoveFromBucket(ref KeyValuePair<int, TValue>[] bucket, int key)
    {
      TValue tmpValue;
      int index = TryGetValueInBucket(bucket, key, out tmpValue);
      if (index<0)
        return false;
      Array.Copy(bucket, index + 1, bucket, index, bucket.Length - index - 1);
      Array.Resize(ref bucket, bucket.Length - 1);
      return true;
    }

    /// <exception cref="OverflowException">Capacity limit is reached.</exception>
    private static int GetCapacity(int requestedCapacity)
    {
      int capacity = MinCapacity;
      while (capacity < requestedCapacity) {
        capacity = capacity << 1;
        if (capacity<=0)
          throw new OverflowException();
      }
      return capacity;
    }

    private void Initialize(int capacity)
    {
      capacity = GetCapacity(capacity);
      mask = capacity - 1;
      count = 0;
      items = new Triplet<int, TValue, KeyValuePair<int, TValue>[]>[capacity];
      for (int i = 0; i < items.Length; i++)
        items[i] = new Triplet<int, TValue, KeyValuePair<int, TValue>[]>(
          NonExistingKeyIndex, default(TValue), null);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public IntDictionary()
      : this(MinCapacity)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="capacity">The initial capacity.</param>
    public IntDictionary(int capacity)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThan(capacity, 0, "capacity");
      Initialize(capacity);
    }
  }
}