// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.22

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// The fast dictionary-like collection using keys of type <see cref="int"/>.
  /// </summary>
  /// <typeparam name="TValue">The type of a value.</typeparam>
  [Serializable]
  public sealed class IntDictionary<TValue> : IEnumerable<KeyValuePair<int, TValue>>
  {
    private const int maxMaskPattern = int.MinValue;
    private const int indexOfAbsenceKey = -1;
    private const double maxFillFactor = 0.7;

    private KeyValuePair<int, TValue>[][] items;
    private int mask;
    private int keyOffset;
    private int occupiedBucketCount;

    /// <summary>
    /// Gets a value by the specified key.
    /// </summary>
    public TValue this[int key]
    {
      get {
        var hashCode = CalculateHashCode(key);
        var bucket = items[hashCode];
        if (bucket==null)
          throw new KeyNotFoundException();
        if (bucket.Length==1) {
          var pair = bucket[0];
          if (pair.Key==key)
            return pair.Value;
          throw new KeyNotFoundException();
        }
        var keyIndex = FindKeyIndexInBucket(bucket, key);
        if (keyIndex==indexOfAbsenceKey)
          throw new KeyNotFoundException();
        return bucket[keyIndex].Value;
      }
    }

    /// <summary>
    /// Adds the item to the collection.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public void Add(int key, TValue value)
    {
      var newPair = new KeyValuePair<int, TValue>(key, value);
      if (AddPair(newPair, items, CalculateHashCode(newPair.Key), ref occupiedBucketCount))
        IncreaseSizeIfNecessary();
    }

    /// <summary>
    /// Removes the item with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns><see langword="true" /> if an item was found and removed; 
    /// otherwise <see langword="false" />.</returns>
    public bool Remove(int key)
    {
      var hashCode = CalculateHashCode(key);
      var bucket = items[hashCode];
      if (bucket==null)
        return false;
      if (bucket.Length==1) {
        if (bucket[0].Key==key) {
          items[hashCode] = null;
          return true;
        }
        return false;
      }
      var keyIndex = FindKeyIndexInBucket(bucket, key);
      if (keyIndex==indexOfAbsenceKey)
        return false;
      RemoveFromBucket(bucket, hashCode, keyIndex);
      return true;
    }

    /// <summary>
    /// Removes all items.
    /// </summary>
    public void Clear()
    {
      for (int i = 0; i < items.Length; i++)
        items[i] = null;
    }

    /// <summary>
    /// Tries to get a value by the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns><see langword="bool" /> if the key was found; 
    /// otherwise <see langword="false" />.</returns>
    public bool TryGetValue(int key, out TValue value)
    {
      var hashCode = CalculateHashCode(key);
      var bucket = items[hashCode];
      if (bucket==null) {
        value = default(TValue);
        return false;
      }
      if (bucket.Length==1) {
        var pair = bucket[0];
        if (pair.Key==key) {
          value = pair.Value;
          return true;
        }
        value = default(TValue);
        return false;
      }
      var keyIndex = FindKeyIndexInBucket(bucket, key);
      if (keyIndex==indexOfAbsenceKey) {
        value = default(TValue);
        return false;
      }
      value = bucket[keyIndex].Value;
      return true;
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<int, TValue>> GetEnumerator()
    {
      foreach (var bucket in items)
        if (bucket!=null)
          foreach (var pair in bucket)
            yield return pair;
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #region Private / internal members

    private int CalculateBucketCount()
    {
      var tempMask = mask;
      keyOffset = 0;
      while ((tempMask & 1)!=1) {
        keyOffset++;
        tempMask = tempMask >> 1;
      }
      return tempMask + 1;
    }

    private int CalculateHashCode(int key)
    {
      var result = key & mask;
      var currentOffset = keyOffset;
      while (currentOffset > 0) {
        result = result >> 1;
        currentOffset--;
      }
      return result;
    }

    private static bool AddPair(KeyValuePair<int, TValue> newPair, KeyValuePair<int, TValue>[][] items,
      int hashCode, ref int occupiedBucketCount)
    {
      var bucket = items[hashCode];
      if (bucket==null) {
        occupiedBucketCount++;
        items[hashCode] = new KeyValuePair<int, TValue>[1];
        items[hashCode][0] = newPair;
        return true;
      }
      var bucketSize = bucket.Length;
      Array.Resize(ref bucket, bucketSize + 1);
      items[hashCode] = bucket;
      InsertIntoBucket(newPair, bucket);
      return false;
    }

    private static void InsertIntoBucket(KeyValuePair<int, TValue> newPair,
      KeyValuePair<int, TValue>[] bucket)
    {
      var index = 0;
      var endIndex = bucket.Length - 1;
      while (bucket[index].Key < newPair.Key && index < endIndex)
        index++;
      if (bucket[index].Key==newPair.Key)
        throw new ArgumentException(Strings.ExItemWithTheSameKeyHasBeenAdded);
      for (int i = bucket.Length - 1; i > index; i--)
        bucket[i] = bucket[i - 1];
      bucket[index] = newPair;
    }

    private void IncreaseSizeIfNecessary()
    {
      if ((double) occupiedBucketCount / items.Length < maxFillFactor)
        return;
      if ((mask & maxMaskPattern)!=0)
        return;
      ExtendMask();
      var bucketCount = CalculateBucketCount();
      var newCollection = new KeyValuePair<int, TValue>[bucketCount][];
      ResizeBucketCollection(newCollection);
    }

    private void ExtendMask()
    {
      var leftOffset = 0;
      var tempMask = (uint) mask;
      while ((tempMask & maxMaskPattern >> 1)==0) {
        tempMask = tempMask << 1;
        leftOffset++;
      }
      tempMask = (uint) (tempMask | maxMaskPattern);
      mask = (int) (tempMask >> leftOffset);
    }

    private void ResizeBucketCollection(KeyValuePair<int, TValue>[][] target)
    {
      var newOccupiedBucketCount = 0;
      foreach (var pair in this) {
        var hashCode = CalculateHashCode(pair.Key);
        AddPair(pair, target, hashCode, ref newOccupiedBucketCount);
      }
      occupiedBucketCount = newOccupiedBucketCount;
      items = target;
    }

    private void RemoveFromBucket(KeyValuePair<int, TValue>[] bucket, int hashCode, int keyIndex)
    {
      var endIndex = bucket.Length - 2;
      for (int i = keyIndex; i <= endIndex; i++)
        bucket[i] = bucket[i + 1];
      Array.Resize(ref bucket, bucket.Length - 1);
      items[hashCode] = bucket;
    }

    private static int FindKeyIndexInBucket(KeyValuePair<int, TValue>[] bucket, int key)
    {
      var start = 0;
      var end = bucket.Length - 1;
      while (end >= start) {
        if (end==start)
          if (key==bucket[start].Key)
            return start;
          else
            return indexOfAbsenceKey;
        var middle = ((end - start) / 2) + start;
        var middleKey = bucket[middle].Key;
        if (key==bucket[middle].Key)
          return middle;
        if (key > middleKey) {
          start = middle + 1;
          continue;
        }
        end = middle - 1;
      }
      return indexOfAbsenceKey;
    }

    #endregion

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="mask">The mask to calculate the hash code of a key.</param>
    public IntDictionary(int mask)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThan(mask, 0, "mask");
      this.mask = mask;
      var bucketCount = CalculateBucketCount();
      items = new KeyValuePair<int, TValue>[bucketCount][];
    }
  }
}