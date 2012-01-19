// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.09

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Arithmetic;
using Xtensive.Comparison;
using Xtensive.Core;

namespace Xtensive.Indexing.Optimization
{
  [Serializable]
  internal class Histogram<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
  {
    public static readonly int DefaultMaxKeyCount = 1000;
    public static readonly double DefaultMinFillFactor = 0.4;

    private readonly SortedList<TKey, TValue> data;
    private readonly Arithmetic<TValue> arithmetic;
    private readonly AdvancedComparer<TKey> keyComparer;
    private readonly AdvancedComparer<TValue> valueComparer;

    public readonly int MaxKeyCount;

    public void AddOrReplace(TKey key, TValue value)
    {
      if (data.ContainsKey(key)) {
        data[key] = CalculateMergedValue(data[key], value);
        return;
      }
      data.Add(key, value);
      if (data.Count > MaxKeyCount)
        Shrink();
    }

    public void Merge(Histogram<TKey, TValue> other)
    {
      foreach (var pair in other)
        if (data.ContainsKey(pair.Key))
          MergeExistingKey(pair);
        else {
          data.Add(pair.Key, pair.Value);
        }
      while(data.Count > MaxKeyCount)
        Shrink();
    }

    private void MergeExistingKey(KeyValuePair<TKey, TValue> pair)
    {
      data[pair.Key] = CalculateMergedValue(data[pair.Key], pair.Value);
    }

    private TValue CalculateMergedValue(TValue oldValue, TValue newValue)
    {
      return arithmetic.Add(oldValue, newValue);
    }

    private void Shrink()
    {
      Pair<KeyValuePair<TKey, TValue>> minimalPair = FindNeighboringKeysWithMinimalValueDiff();
      TValue mergedValue = CalculateMergedValue(minimalPair.First.Value, minimalPair.Second.Value);
      data.Remove(minimalPair.Second.Key);
      data[minimalPair.First.Key] = mergedValue;
    }

    private Pair<KeyValuePair<TKey, TValue>> FindNeighboringKeysWithMinimalValueDiff()
    {
      var firstMinimal = new KeyValuePair<TKey, TValue>();
      var secondMinimal = new KeyValuePair<TKey, TValue>();
      KeyValuePair<TKey, TValue>? previous = null;
      TValue minimalDiff = default(TValue);
      var minimalSpecified = false;
      foreach (var pair in data) {
        if (previous==null) {
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
        }
        else if (valueComparer.Compare(currentDiff, minimalDiff) < 0) {
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
      int maxKeyCount)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyComparer, "keyComparer");
      ArgumentValidator.EnsureArgumentNotNull(valueComparer, "valueComparer");
      ArgumentValidator.EnsureArgumentIsInRange(maxKeyCount, 2, int.MaxValue, "maxKeyCount");
      this.valueComparer = valueComparer;
      this.keyComparer = keyComparer;
      MaxKeyCount = maxKeyCount;
      arithmetic = ArithmeticProvider.Default.GetArithmetic<TValue>();
      data = new SortedList<TKey, TValue>(keyComparer.ComparerImplementation);
    }
  }
}