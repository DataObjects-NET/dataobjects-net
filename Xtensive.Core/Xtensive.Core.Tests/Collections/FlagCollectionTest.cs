// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.10.01

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Conversion;
using Xtensive.Testing;

namespace Xtensive.Tests.Collections
{
  [TestFixture]
  public class FlagCollectionTest
  {
    private const int ItemCount = 32;
    private static readonly IntConverter intConverter = new IntConverter();
    private static readonly Biconverter<int, bool> converter = new Biconverter<int, bool>(intConverter, intConverter);

    internal class IntConverter: 
      IAdvancedConverter<int, bool>, 
      IAdvancedConverter<bool, int>
    {
      public bool Convert(int value)
      {
        if (value >= 0)
          return true;
        return false;
      }

      public int Convert(bool value)
      {
        return value ? 1 : -1;
      }

      public bool IsRough
      {
        get { return false; }
      }

      public IAdvancedConverterProvider Provider
      {
        get { return null; }
      }
    }

    [Test]
    public void AddTest()
    {
      Random random = RandomManager.CreateRandom((int)DateTime.Now.Ticks);
      FlagCollection<object, int> flagCollection = new FlagCollection<object, int>(converter);
      for (int i = 0; i < ItemCount; i++) {
        flagCollection.Add(new object(), random.Next(-100,100));
      }
      Assert.AreEqual(ItemCount, flagCollection.Count );

      FlagCollection<object, int> flagCollection1 = new FlagCollection<object, int>(converter, flagCollection);
      Assert.AreEqual(flagCollection, flagCollection1);
    }

    [Test]
    public void BitVectorTest()
    {
      FlagCollection<object, int> flagCollection = new FlagCollection<object, int>(converter);
      flagCollection.Add(new object(), -100);
      flagCollection.Add(new object(), 100);
      IList<int> list = new List<int>(flagCollection.Values);
      Assert.AreEqual(-1, list[0]);
      Assert.AreEqual(1, list[1]);
    }

    [Test]
    public void AddRemoveTest()
    {
      Random random = RandomManager.CreateRandom((int)DateTime.Now.Ticks);
      List<KeyValuePair<object, int>> collection = new List<KeyValuePair<object, int>>();
      for (int i = 0; i < ItemCount; i++) {
        collection.Add(new KeyValuePair<object, int>(new object(), random.Next(-100, 100))); 
      }
      FlagCollection<object, int> flagCollection = new FlagCollection<object, int>(converter, collection);
      for (int i = 0; i < ItemCount; i++) {
        KeyValuePair<object, int> pair = collection[i];
        Assert.AreEqual(pair.Key, flagCollection[i].Key);
        Assert.AreEqual(pair.Value >= 0, flagCollection[pair.Key] >= 0);
      }

      foreach (KeyValuePair<object, int> pair in collection) {
        flagCollection.Remove(pair.Key);
      }
      Assert.AreEqual(0, flagCollection.Count);
    }
  }
}