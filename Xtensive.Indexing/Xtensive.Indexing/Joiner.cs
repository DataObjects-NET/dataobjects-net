// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.10.23

using System;
using System.Collections.Generic;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Indexing
{
  /// <summary>
  /// A class providing various join algorithms.
  /// </summary>
  public static class Joiner
  {
    private const int SCAN_ITERATIONS = 4;

    /// <summary>
    /// Merge join implementation.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TRightValue">The type of the right value.</typeparam>
    /// <param name="left">The left sequence.</param>
    /// <param name="right">The right sequence.</param>
    /// <returns>Join operation result.</returns>
    public static IEnumerable<Pair<TValue, TRightValue>> MergeJoin<TKey, TValue, TRightValue>(
      this IOrderedEnumerable<TKey, TValue> left, 
      IOrderedEnumerable<TKey, TRightValue> right)
    {
      var comparer = left.KeyComparer;
      var lReader = left.CreateReader(left.GetFullRange());
      var rReader = right.CreateReader(right.GetFullRange());

      var rList = new LinkedList<KeyValuePair<TKey, TRightValue>>();

      int lScanCount = 0;
      int rScanCount = 0;

      bool haveValues = lReader.MoveNext() && rReader.MoveNext();
      while (haveValues) {
        TValue lValue = lReader.Current;
        TRightValue rValue = rReader.Current;
        TKey lKey = left.KeyExtractor(lValue);
        TKey rKey = right.KeyExtractor(rValue);

        int result = comparer.Compare(lKey, rKey);
        if (result == 0) {
          yield return new Pair<TValue, TRightValue>(lValue, rValue);
          rList.AddLast(new KeyValuePair<TKey, TRightValue>(rKey, rValue));
          haveValues &= rReader.MoveNext();
          rScanCount = 0;
          lScanCount = 0;
        }
        else if (result < 0) {
          rScanCount = 0;
          if (lScanCount > SCAN_ITERATIONS && rList.Count == 0) {
            lReader.MoveTo(new Entire<TKey>(rKey));
            haveValues &= lReader.MoveNext();
            lScanCount = 0;
          }
          else {
            haveValues &= lReader.MoveNext();
            lScanCount++;
          }
          if (haveValues && rList.Count > 0) {
            lValue = lReader.Current;
            lKey = left.KeyExtractor(lValue);
            while (rList.Count > 0 && comparer.Compare(lKey, rList.First.Value.Key) != 0)
              rList.RemoveFirst();
            foreach (KeyValuePair<TKey, TRightValue> pair in rList)
              yield return new Pair<TValue, TRightValue>(lValue, pair.Value);
          }
        }
        else {
          lScanCount = 0;
          rList.Clear();
          if (rScanCount > SCAN_ITERATIONS) {
            rReader.MoveTo(new Entire<TKey>(lKey));
            haveValues &= rReader.MoveNext();
            rScanCount = 0;
          }
          else {
            haveValues &= rReader.MoveNext();
            rScanCount++;
          }
        }
      }
    }

    /// <summary>
    /// "Merge join left" implementation.
    /// "Left" implies any item from the left sequence will be kept in the output sequence,
    /// even if there is no corresponding item in the right sequence; if so,
    /// default value for <typeparamref name="TRightValue"/> will be used as matching one there.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TRightValue">The type of the right value.</typeparam>
    /// <param name="left">The left sequence.</param>
    /// <param name="right">The right sequence.</param>
    /// <param name="keyExtractorLeft">The key extractor for the left sequence.</param>
    /// <param name="keyExtractorRight">The key extractor for the right sequence.</param>
    /// <param name="comparer">The comparer.</param>
    /// <returns>Join operation result.</returns>
    public static IEnumerable<Pair<TValue, TRightValue>> MergeJoinLeft<TKey, TValue, TRightValue>(
      this IEnumerable<TValue> left, 
      IEnumerable<TRightValue> right,
      Converter<TValue, TKey> keyExtractorLeft,
      Converter<TRightValue, TKey> keyExtractorRight,
      AdvancedComparer<TKey> comparer)
    {
      IEnumerator<TValue> lEnum = left.GetEnumerator();
      bool haveLValues = lEnum.MoveNext();

      if (haveLValues) {
        LinkedList<KeyValuePair<TKey, TRightValue>> rList = new LinkedList<KeyValuePair<TKey, TRightValue>>();

        IEnumerator<TRightValue> rEnum = right.GetEnumerator();
        bool haveRValues = rEnum.MoveNext();
        bool match = false;

        while (haveLValues) {
          if (!haveRValues) {
            yield return new Pair<TValue, TRightValue>(lEnum.Current, default(TRightValue));
            haveLValues = lEnum.MoveNext();
          }
          else {
            TValue lValue = lEnum.Current;
            TKey lKey = keyExtractorLeft(lValue);
            TRightValue rValue = rEnum.Current;
            TKey rKey = keyExtractorRight(rValue);
            int result = comparer.Compare(lKey, rKey);
            if (result == 0) {
              yield return new Pair<TValue, TRightValue>(lValue, rValue);
              rList.AddLast(new KeyValuePair<TKey, TRightValue>(rKey, rValue));
              haveRValues &= rEnum.MoveNext();
              if (!haveRValues)
                haveLValues = lEnum.MoveNext();
              match = true;
            }
            else if (result < 0) {
              if (!match)
                yield return new Pair<TValue, TRightValue>(lValue, default(TRightValue));

              haveLValues = lEnum.MoveNext();
              match = false;
              if (haveLValues) {
                lValue = lEnum.Current;
                lKey = keyExtractorLeft(lValue);
                if (rList.Count > 0) {
                  while (rList.Count > 0 && comparer.Compare(lKey, rList.First.Value.Key) != 0)
                    rList.RemoveFirst();
                  foreach (KeyValuePair<TKey, TRightValue> pair in rList) {
                    yield return new Pair<TValue, TRightValue>(lValue, pair.Value);
                    match = true;
                  }
                }
              }
            }
            else {
              rList.Clear();
              haveRValues = rEnum.MoveNext();
            }
          }
        }
      }
    }

    /// <summary>
    /// "Merge join left" implementation.
    /// "Left" implies any item from the left sequence will be kept in the output sequence,
    /// even if there is no corresponding item in the right sequence; if so,
    /// default value for <typeparamref name="TRightValue"/> will be used as matching one there.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TRightValue">The type of the right value.</typeparam>
    /// <param name="left">The left sequence.</param>
    /// <param name="right">The right sequence.</param>
    /// <returns>Join operation result.</returns>
    public static IEnumerable<Pair<TValue, TRightValue>> MergeJoinLeft<TKey, TValue, TRightValue>(
      this IOrderedEnumerable<TKey, TValue> left, 
      IOrderedEnumerable<TKey, TRightValue> right)
    {
      AdvancedComparer<TKey> comparer = left.KeyComparer;
      IEnumerator<TValue> lEnum = left.GetEnumerator();
      bool haveLValues = lEnum.MoveNext();

      if (haveLValues) {
        LinkedList<KeyValuePair<TKey, TRightValue>> rList = new LinkedList<KeyValuePair<TKey, TRightValue>>();

        int rScanCount = 0;
        IIndexReader<TKey, TRightValue> rReader = right.CreateReader(right.GetFullRange());
        bool haveRValues = rReader.MoveNext();
        bool match = false;

        while (haveLValues) {
          if (!haveRValues) {
            yield return new Pair<TValue, TRightValue>(lEnum.Current, default(TRightValue));
            haveLValues = lEnum.MoveNext();
          }
          else {
            TValue lValue = lEnum.Current;
            TKey lKey = left.KeyExtractor(lValue);
            TRightValue rValue = rReader.Current;
            TKey rKey = right.KeyExtractor(rValue);
            int result = comparer.Compare(lKey, rKey);
            if (result == 0) {
              yield return new Pair<TValue, TRightValue>(lValue, rValue);
              rList.AddLast(new KeyValuePair<TKey, TRightValue>(rKey, rValue));
              haveRValues &= rReader.MoveNext();
              if (!haveRValues)
                haveLValues = lEnum.MoveNext();
              match = true;
            }
            else if (result < 0) {
              if (!match)
                yield return new Pair<TValue, TRightValue>(lValue, default(TRightValue));

              rScanCount = 0;
              haveLValues = lEnum.MoveNext();
              match = false;
              if (haveLValues) {
                lValue = lEnum.Current;
                lKey = left.KeyExtractor(lValue);
                if (rList.Count > 0) {
                  while (rList.Count > 0 && comparer.Compare(lKey, rList.First.Value.Key) != 0)
                    rList.RemoveFirst();
                  foreach (KeyValuePair<TKey, TRightValue> pair in rList) {
                    yield return new Pair<TValue, TRightValue>(lValue, pair.Value);
                    match = true;
                  }
                }
              }
            }
            else {
              rList.Clear();
              if (rScanCount > SCAN_ITERATIONS) {
                rReader.MoveTo(new Entire<TKey>(lKey));
                haveRValues = rReader.MoveNext();
                rScanCount = 0;
              }
              else {
                haveRValues = rReader.MoveNext();
                rScanCount++;
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Loop join implementation.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TRightValue">The type of the right value.</typeparam>
    /// <param name="left">The left sequence.</param>
    /// <param name="right">The right sequence.</param>
    /// <param name="keyExtractor">The key extractor.</param>
    /// <returns>Join operation result.</returns>
    public static IEnumerable<Pair<TValue, TRightValue>> LoopJoin<TKey, TValue, TRightValue>(
      this IEnumerable<TValue> left,
      IOrderedEnumerable<TKey, TRightValue> right,
      Converter<TValue, TKey> keyExtractor)
    {
      foreach (TValue lValue in left) {
        TKey key = keyExtractor(lValue);
        foreach (TRightValue rValue in right.GetItems(new Range<Entire<TKey>>(new Entire<TKey>(key, Direction.Negative), new Entire<TKey>(key, Direction.Positive))))
          yield return new Pair<TValue, TRightValue>(lValue, rValue);
      }
    }

    /// <summary>
    /// "Loop join left" implementation.
    /// "Left" implies any item from the left sequence will be kept in the output sequence,
    /// even if there is no corresponding item in the right sequence; if so,
    /// default value for <typeparamref name="TRightValue"/> will be used as matching one there.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TRightValue">The type of the right value.</typeparam>
    /// <param name="left">The left sequence.</param>
    /// <param name="right">The right sequence.</param>
    /// <param name="keyExtractor">The key extractor.</param>
    /// <returns>Join operation result.</returns>
    public static IEnumerable<Pair<TValue, TRightValue>> LoopJoinLeft<TKey, TValue, TRightValue>(
      this IEnumerable<TValue> left, 
      IOrderedEnumerable<TKey, TRightValue> right, 
      Converter<TValue, TKey> keyExtractor)
    {
      foreach (TValue lValue in left) {
        TKey key = keyExtractor(lValue);
        bool match = false;
        foreach (TRightValue rValue in right.GetItems(new Range<Entire<TKey>>(new Entire<TKey>(key, Direction.Negative), new Entire<TKey>(key, Direction.Positive)))) {
          yield return new Pair<TValue, TRightValue>(lValue, rValue);
          match = true;
        }
        if (!match)
          yield return new Pair<TValue, TRightValue>(lValue, default(TRightValue));
      }
    }

    /// <summary>
    /// Nested loop join implementation.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TRightValue">The type of the right value.</typeparam>
    /// <param name="left">The left sequence.</param>
    /// <param name="right">The right sequence.</param>
    /// <param name="keyExtractorLeft">The key extractor for the left sequence.</param>
    /// <param name="keyExtractorRight">The key extractor for the right sequence.</param>
    /// <param name="comparer">The comparer.</param>
    /// <returns>Join operation result.</returns>
    public static IEnumerable<Pair<TValue, TRightValue>> NestedLoopJoin<TKey, TValue, TRightValue>(
      this IEnumerable<TValue> left, 
      IEnumerable<TRightValue> right, 
      Converter<TValue, TKey> keyExtractorLeft,
      Converter<TRightValue, TKey> keyExtractorRight, 
      AdvancedComparer<TKey> comparer)
    {
      foreach (TValue lValue in left) {
        TKey lKey = keyExtractorLeft(lValue);
        foreach (TRightValue rValue in right) {
          TKey rKey = keyExtractorRight(rValue);
          if (comparer.Compare(lKey, rKey) == 0)
            yield return new Pair<TValue, TRightValue>(lValue, rValue);
        }
      }
    }

    /// <summary>
    /// "Nested loop join left" implementation.
    /// "Left" implies any item from the left sequence will be kept in the output sequence,
    /// even if there is no corresponding item in the right sequence; if so,
    /// default value for <typeparamref name="TRightValue"/> will be used as matching one there.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TRightValue">The type of the right value.</typeparam>
    /// <param name="left">The left sequence.</param>
    /// <param name="right">The right sequence.</param>
    /// <param name="keyExtractorLeft">The key extractor for the left sequence.</param>
    /// <param name="keyExtractorRight">The key extractor for the right sequence.</param>
    /// <param name="comparer">The comparer.</param>
    /// <returns>Join operation result.</returns>
    public static IEnumerable<Pair<TValue, TRightValue>> NestedLoopJoinLeft<TKey, TValue, TRightValue>(
      this IEnumerable<TValue> left, 
      IEnumerable<TRightValue> right, 
      Converter<TValue, TKey> keyExtractorLeft,
      Converter<TRightValue, TKey> keyExtractorRight, 
      AdvancedComparer<TKey> comparer)
    {
      foreach (TValue lValue in left) {
        TKey lKey = keyExtractorLeft(lValue);
        bool found = false;
        foreach (TRightValue rValue in right) {
          TKey rKey = keyExtractorRight(rValue);
          if (comparer.Compare(lKey, rKey) == 0) {
            found = true;
            yield return new Pair<TValue, TRightValue>(lValue, rValue);
          }
        }
        if (!found)
          yield return new Pair<TValue, TRightValue>(lValue, default(TRightValue));
      }
    }
  }
}