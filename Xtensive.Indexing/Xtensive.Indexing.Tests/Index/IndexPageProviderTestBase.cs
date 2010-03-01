// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.01.05

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Testing;
using Xtensive.Indexing;
using Xtensive.Indexing.Providers;

namespace Xtensive.Indexing.Tests.Index
{
  public abstract class IndexPageProviderTestBase
  {
    protected void BaseTest()
    {
      Random rand = RandomManager.CreateRandom();
      try {
        for (int i = 0; i<2000; i++) {
          AddTest(i);
          AddRemoveTest(i, Math.Min(i, 10));
          AddRemoveTest(i, i);
          RangeTest(i, rand.Next(10));
          EnumerableTest(i);
          if (i == 40)
            i = 110;
          if (i == 130)
            i = 510;
          if (i == 515)
            i = 1020;
          if (i == 1030)
            i = 1998;
        }
      }
      finally {
        Cleanup();
      }
    }

    protected virtual void Cleanup()
    {
    }

    // Abstract methods
    protected abstract bool IsSerializable { get; }
    protected abstract Stream Serialize<TKey, TValue>(ref Index<TKey, TValue> tree);

    private void AddTest(int itemCount)
    {
      InnerAddTest<int, int>(IntPairGenerator, itemCount);
      InnerAddTest<string, string>(StringPairGenerator, itemCount);
      InnerAddTest<int, string>(IntStringPairGenerator, itemCount);
    }

    private void AddRemoveTest(int itemCount, int itemsToRemoveCount)
    {
      InnerAddRemoveTest<int, int>(IntPairGenerator, itemCount, itemsToRemoveCount);
      InnerAddRemoveTest<string, string>(StringPairGenerator, itemCount, itemsToRemoveCount);
      InnerAddRemoveTest<int, string>(IntStringPairGenerator, itemCount, itemsToRemoveCount);
    }

    protected void RangeTest(int itemCount, int rangeCount)
    {
      InnerRangeTest<int, int>(IntPairGenerator, itemCount, rangeCount);
      InnerRangeTest<string, string>(StringPairGenerator, itemCount, rangeCount);
    }

    protected void EnumerableTest(int itemCount)
    {
      InnerEnumerableTest<int, int>(IntPairGenerator, itemCount);
      InnerEnumerableTest<string, string>(StringPairGenerator, itemCount);
    }
    // Inner methods

    private void InnerAddTest<TKey, TValue>(Func<Random, KeyValuePair<TKey, TValue>> fillFunct, int itemCount)
    {
      using (new Measurement(String.Format("AddTest<{0}, {1}>", typeof(TKey).Name, typeof(TValue).Name), itemCount)) {
        Index<TKey, TValue> tree = new Index<TKey, TValue>(true);
        Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
        TreeTestUtils<TKey, TValue>.FillUniqueTree(tree, dictionary, fillFunct, itemCount);
        using (TrySerialize(ref tree)) {
          AssertEx.AreEqual<KeyValuePair<TKey, TValue>>(tree, dictionary);
        }
      }
    }

    private void InnerAddRemoveTest<TKey, TValue>(Func<Random, KeyValuePair<TKey, TValue>> fillFunct, int itemCount, int removeCount)
    {
      using (new Measurement(String.Format("AddRemoveTest<{0}, {1}>", typeof(TKey).Name, typeof(TValue).Name), itemCount)) {
        Index<TKey, TValue> tree = new Index<TKey, TValue>(new MemoryPageProvider<TKey, TValue>(true, 16, null, null));
        Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
        TreeTestUtils<TKey, TValue>.FillUniqueTree(tree, dictionary, fillFunct, itemCount);
        TreeTestUtils<TKey, TValue>.DeleteFromTree(tree, dictionary, fillFunct, removeCount);
        using (TrySerialize(ref tree)) {
          AssertEx.AreEqual<KeyValuePair<TKey, TValue>>(tree, dictionary);
        }
      }
    }

    private void InnerRangeTest<TKey, TValue>(Func<Random, KeyValuePair<TKey, TValue>> fillFunct, int itemCount, int rangeCount)
    {
      using (new Measurement(String.Format("RangeTest<{0}, {1}>", typeof(TKey).Name, typeof(TValue).Name), itemCount)) {
        Index<TKey, TValue> tree = new Index<TKey, TValue>();
        SortedCollection<TKey> sortedCollection = new SortedCollection<TKey>();
        SortedDictionary<TKey, IList<TValue>> sortedDictionary = new SortedDictionary<TKey, IList<TValue>>();
        TreeTestUtils<TKey, TValue>.FillNonUniqueTree(tree, sortedCollection, sortedDictionary, fillFunct, itemCount);
        using (TrySerialize(ref tree)) {
          AssertEx.AreEqual(tree.GetKeys(), sortedDictionary.Keys);

          TreeTestUtils<TKey, TValue>.TestRanges(tree, sortedCollection, sortedDictionary, fillFunct, rangeCount);
        }
      }
    }

    private void InnerEnumerableTest<TKey, TValue>(Func<Random, KeyValuePair<TKey, TValue>> fillFunct, int itemCount)
    {
      using (new Measurement(String.Format("RangeTest<{0}, {1}>", typeof(TKey).Name, typeof(TValue).Name), itemCount)) {
        Index<TKey, TValue> tree = new Index<TKey, TValue>();
        SortedCollection<TKey> sortedCollection = new SortedCollection<TKey>();
        SortedDictionary<TKey, IList<TValue>> sortedDictionary = new SortedDictionary<TKey, IList<TValue>>();
        TreeTestUtils<TKey, TValue>.FillNonUniqueTree(tree, sortedCollection, sortedDictionary, fillFunct, itemCount);
        using (TrySerialize(ref tree)) {
          AssertEx.AreEqual(tree.GetKeys(), sortedDictionary.Keys);
          AssertEx.AreEqual<KeyValuePair<TKey, TValue>>(tree, TreeTestUtils<TKey, TValue>.GetKeyValuePairs(sortedDictionary));
          AssertEx.AreEqual(tree.GetKeys(), sortedDictionary.Keys);
          AssertEx.AreEqual(((IEnumerable<TValue>)tree), TreeTestUtils<TKey, TValue>.GetValues(sortedDictionary));
          AssertEx.AreEqual(tree.GetValues(Ray<TKey>.MinusInfinity), TreeTestUtils<TKey, TValue>.GetValues(sortedDictionary));
          AssertEx.AreEqual(((IEnumerable<ChangedItem<KeyValuePair<TKey, TValue>>>)tree), TreeTestUtils<TKey, TValue>.GetMeasuredItems(sortedDictionary));
        }
      }
    }

    // Private methods

    private IDisposable TrySerialize<TKey, TValue>(ref Index<TKey, TValue> tree)
    {
      if (IsSerializable) {
        Stream serializedStream = Serialize(ref tree);
        return new Disposable<Stream>(serializedStream, delegate(bool disposing, Stream stream) {
          string name = null;
          FileStream fileStream = stream as FileStream;
          if (fileStream!=null)
            name = fileStream.Name;
          if (stream!=null)
            stream.Close();
          if (!String.IsNullOrEmpty(name))
            File.Delete(name);
        });
      }
      else
        return null;
    }

    private static KeyValuePair<int, int> IntPairGenerator(Random rand)
    {
      return new KeyValuePair<int, int>(rand.Next(), rand.Next());
    }

    private static KeyValuePair<string, string> StringPairGenerator(Random rand)
    {
      return new KeyValuePair<string, string>(rand.Next(100000).ToString(), rand.Next(100000).ToString());
    }

    private static KeyValuePair<int, string> IntStringPairGenerator(Random rand)
    {
      return new KeyValuePair<int, string>(rand.Next(), rand.Next().ToString());
    }
  }
}
