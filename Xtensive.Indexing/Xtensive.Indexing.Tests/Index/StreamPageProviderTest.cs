// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.11.23

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Indexing;
using Xtensive.Indexing.Providers;

namespace Xtensive.Indexing.Tests.Index
{
  [TestFixture]
  public class StreamPageProviderTest : IndexPageProviderTestBase
  {
    public static int Seed = 1;

    protected override bool IsSerializable
    {
      get { return true; }
    }

    protected override Stream Serialize<TKey, TValue>(ref Index<TKey, TValue> tree)
    {
      Stream stream = new MemoryStream();
      StreamPageProvider<TKey, TValue> streamPageProvider = new StreamPageProvider<TKey, TValue>(stream);
      Index<TKey, TValue> storedTree = new Index<TKey, TValue>(streamPageProvider);
      storedTree.Serialize(tree);
      
      // Full deserialization.
      stream.Seek(0, SeekOrigin.Begin);
      tree = new Index<TKey, TValue>(new StreamPageProvider<TKey, TValue>(stream));
      return stream;
    }

    [Test]
    public void Test()
    {
      BaseTest();
    }

    [Test]
    public void GetMeasureTest()
    {
      Random rand = RandomManager.CreateRandom();
      Index<int, int> tree = new Index<int, int>(true);
      tree.AddMeasureDefinition(CountMeasure<object, long>.CommonName, 
        new CountMeasure<ChangedItem<KeyValuePair<int,int>>>());
      int count = 10000;
      for (int i = 0; i < count; i++) {
        tree.Add(i, i);
        Assert.AreEqual(i, tree[i]);
      }
      
      Stream stream = new MemoryStream();
      StreamPageProvider<int, int> streamPageProvider = new StreamPageProvider<int, int>(stream);
      streamPageProvider.DescriptorPage.AddMeasureDefinition(CountMeasure<object, long>.CommonName, 
        new CountMeasure<ChangedItem<KeyValuePair<int,int>>>());
      Index<int, int> storedTree = new Index<int, int>(streamPageProvider);
      storedTree.Serialize(tree);
      try {
        streamPageProvider = new StreamPageProvider<int, int>(stream);
        Index<int, int> restoredTree = new Index<int, int>(streamPageProvider);

        for (int i = 0; i < 100; i++) {
          int min = rand.Next(count);
          int max = rand.Next(min, count);
          Assert.AreEqual(max - min + 1,
            restoredTree.GetMeasure<CountMeasure<ChangedItem<KeyValuePair<int,int>>>>(CountMeasure<object, long>.CommonName, 
              new Range<int>(min, max)).Value,
            "at step " + i + " from " + min + " to " + max);
        }
      }
      finally {
        stream.Close();
      }
    }

    private static long Summ(ChangedItem<KeyValuePair<int,int>> mi)
    {
      return mi.Item.Value;
    }

    private static readonly Func<ChangedItem<KeyValuePair<int,int>>, long> SummDelegate = Summ;

    private static long ExpectedSumm(long min, long max)
    {
      return ((min + max) * (max - min + 1)) / 2;
    }

    [Test]
    public void SummMeasureTest()
    {
      Random rand = RandomManager.CreateRandom();
      int count = 10000;
      Index<int, int> tree = new Index<int, int>(true);
      tree.AddMeasureDefinition("Summ", 
        new SummMeasure<ChangedItem<KeyValuePair<int,int>>, long>(SummDelegate));
      tree.AddMeasureDefinition(CountMeasure<object, long>.CommonName, 
        new CountMeasure<ChangedItem<KeyValuePair<int,int>>>());
      for (int i = 0; i < count; i++) {
        tree.Add(i, i);
        Assert.AreEqual(i, tree[i]);
      }

      Stream stream = new MemoryStream();
      StreamPageProvider<int, int> streamPageProvider = new StreamPageProvider<int, int>(stream);
      streamPageProvider.DescriptorPage.AddMeasureDefinition("Summ", 
        new SummMeasure<ChangedItem<KeyValuePair<int,int>>, long>(SummDelegate));
      streamPageProvider.DescriptorPage.AddMeasureDefinition(CountMeasure<object, long>.CommonName, 
        new CountMeasure<ChangedItem<KeyValuePair<int,int>>>());
      Index<int, int> storedTree = new Index<int, int>(streamPageProvider);
      storedTree.Serialize(tree);

      for (int i = 0; i < 100; i++) {
        int min = rand.Next(count);
        int max = rand.Next(min, count);
        Assert.AreEqual(max - min + 1,
          storedTree.GetMeasure<CountMeasure<ChangedItem<KeyValuePair<int,int>>>>(CountMeasure<object, long>.CommonName, 
            new Range<int>(min, max)).Value,
          "count from " + min + " to " + max);
        Assert.AreEqual(ExpectedSumm(min, max), storedTree.GetMeasure<SummMeasure<ChangedItem<KeyValuePair<int,int>>, long>>("Summ", 
            new Range<int>(min, max)).Value,
          "summ from " + min + " to " + max);
      }
      stream.Close();
    }

    [Test]
    public void EnumeratorTest()
    {
      Index<int, int> tree = new Index<int, int>(true);
      int max = 10000;
      int i = max - 1;
      for (; i >= 0; i--) {
        tree.Add(i, i);
        Assert.AreEqual(i, tree[i]);
      }

      Stream stream = new MemoryStream();
      StreamPageProvider<int, int> streamPageProvider = new StreamPageProvider<int, int>(stream);
      Index<int, int> storedTree = new Index<int, int>(streamPageProvider);
      storedTree.Serialize(tree);

      i = 0;
      foreach (KeyValuePair<int, int> pair in storedTree) {
        Assert.AreEqual(i++, pair.Key);
      }
      Assert.AreEqual(max, i);
    }
  }
}
