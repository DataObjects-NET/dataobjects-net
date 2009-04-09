// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.09

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Arithmetic;
using Xtensive.Core.Comparison;

namespace Xtensive.Indexing.Statistics
{
  [Serializable]
  internal class Histogram<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
  {
    public static readonly int DefaultMaxKeyCount = 1000;

    private readonly SortedList<TKey, TValue> data;
    private readonly Arithmetic<TValue> arithmetic;
    private readonly AdvancedComparer<TKey> keyComparer;
    private readonly AdvancedComparer<TValue> valueComparer;

    public readonly int MaxKeyCount;

    public void AddOrReplace(TKey key, TValue value)
    {
      if (data.ContainsKey(key)) {
        data[key] = value;
        return;
      }
      data.Add(key, value);
      AdjustValueOfNewFirstKey(key);
      if (data.Count > MaxKeyCount)
        Shrink();
    }

    public void Merge(Histogram<TKey, TValue> other)
    {
      bool isFirst = true;
      foreach (var pair in other)
        if(isFirst) {
          isFirst = false;
          var firstKey = data.Keys[0];
          if(keyComparer.Compare(pair.Key, firstKey) < 0) {
            data.Remove(firstKey);
            data.Add(pair.Key, pair.Value);
          }
        }
        else if (data.ContainsKey(pair.Key))
          data[pair.Key] = arithmetic.Divide(arithmetic.Add(data[pair.Key], pair.Value), 2);
        else {
          data.Add(pair.Key, pair.Value);
          AdjustValueOfNewFirstKey(pair.Key);
        }
      while(data.Count > MaxKeyCount)
        Shrink();
    }

    private void AdjustValueOfNewFirstKey(TKey newKey)
    {
      if (keyComparer.Compare(data.Keys[0], newKey) == 0) {
        var zeroValue = data[data.Keys[1]];
        data[data.Keys[1]] = data[newKey];
        data[newKey] = zeroValue;
      }
    }

    private void Shrink()
    {
      Pair<KeyValuePair<TKey, TValue>> minimalPair = FindNeighboringKeysWithMinimalValueDiff();
      TValue avgValue = arithmetic.Divide(
        arithmetic.Add(minimalPair.First.Value, minimalPair.Second.Value), 2);
      data.Remove(minimalPair.First.Key);
      data[minimalPair.Second.Key] = avgValue;
    }

    private Pair<KeyValuePair<TKey, TValue>> FindNeighboringKeysWithMinimalValueDiff()
    {
      var firstMinimal = new KeyValuePair<TKey, TValue>();
      var secondMinimal = new KeyValuePair<TKey, TValue>();
      KeyValuePair<TKey, TValue>? previous = null;
      TValue minimalDiff = default(TValue);
      var minimalSpecified = false;
      var isFirst = false;
      foreach (var pair in data)
      {
        if (!isFirst) {
          isFirst = true;
          continue;
        }
        if (previous == null) {
          previous = pair;
          continue;
        }
        var currentDiff = arithmetic.Subtract(previous.Value.Value, pair.Value);
        if (valueComparer.Compare(currentDiff, arithmetic.Zero) < 0)
          currentDiff = arithmetic.Negation(currentDiff);
        if (!minimalSpecified) {
          minimalDiff = currentDiff;
          minimalSpecified = true;
          firstMinimal = previous.Value;
          secondMinimal = pair;
        } else if (valueComparer.Compare(currentDiff, minimalDiff) < 0) {
          minimalDiff = currentDiff;
          firstMinimal = previous.Value;
          secondMinimal = pair;
        }
        previous = pair;
      }
      return new Pair<KeyValuePair<TKey, TValue>>(firstMinimal, secondMinimal);
    }

    #region Implementation of IEnumerable

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      return data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    // Constructors

    public Histogram(AdvancedComparer<TKey> keyComparer, AdvancedComparer<TValue> valueComparer,
      int maxKeyCount, TKey firstKey)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyComparer, "keyComparer");
      ArgumentValidator.EnsureArgumentNotNull(valueComparer, "valueComparer");
      ArgumentValidator.EnsureArgumentNotNull(firstKey, "firstKey");
      ArgumentValidator.EnsureArgumentIsInRange(maxKeyCount, 2, int.MaxValue, "maxKeyCount");
      this.valueComparer = valueComparer;
      this.keyComparer = keyComparer;
      MaxKeyCount = maxKeyCount;
      arithmetic = ArithmeticProvider.Default.GetArithmetic<TValue>();
      data = new SortedList<TKey, TValue>(keyComparer.ComparerImplementation)
             {
               {firstKey, arithmetic.Zero}
             };
    }
  }
}