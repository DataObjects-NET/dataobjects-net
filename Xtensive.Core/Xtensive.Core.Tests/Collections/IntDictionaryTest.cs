// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.23

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Testing;

namespace Xtensive.Core.Tests.Collections
{
  [TestFixture]
  public sealed class IntDictionaryTest
  {
    [Test]
    public void SimpleTest()
    {
      var dictionary = new IntDictionary<double>(3);
      var keys = new[] {0, 1, 2, 0x53};
      var values = new double[] {24, 435, 876, 3};
      for (int i = 0; i < keys.Length; i++)
        dictionary.Add(keys[i], values[i]);
      CheckValues(dictionary, keys, values);
      AssertEx.Throws<KeyNotFoundException>(() => { var t = dictionary[-1];});
      AssertEx.Throws<KeyNotFoundException>(() => { var t = dictionary[4];});
      AssertEx.Throws<KeyNotFoundException>(() => { var t = dictionary[0x54];});
    }

    [Test]
    public void ResizeTest()
    {
      var dictionary = new IntDictionary<int>(3);
      Assert.AreEqual(4, GetBucketCount(dictionary));
      var keys = new[]
                 {
                   0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15
                 };
      var values = GenerateValues(keys);
      for (int i = 0; i < keys.Length; i++)
        dictionary.Add(keys[i], values[i]);
      CheckValues(dictionary, keys, values);
      AssertEx.Throws<KeyNotFoundException>(() => { var t = dictionary[-1];});
      AssertEx.Throws<KeyNotFoundException>(() => { var t = dictionary[0x16];});
      var items = GetItemsCollection(dictionary);
      Assert.AreEqual(32, items.Length);
      for (var i = 0; i < items.Length; i++)
        if (i < 16) {
          Assert.IsNotNull(items[i]);
          Assert.AreEqual(1, items[i].Length);
        }
        else
          Assert.IsNull(items[i]);
    }

    [Test]
    public void CollisionTest()
    {
      var dictionary = new IntDictionary<int>(15);
      var initialBucketCount = GetBucketCount(dictionary);
      var keys = new[]
                 {
                   0, 0xA00000, 1, 0x100001, 0x200001, 0x500002, 0x800002, 0x300003,0xA00003,
                   0x400003, 0x700003, 0xF00004, 0xA00004, 0x900005,0x700005, 0x200005, 0xE00005, 5
                 };
      var values = GenerateValues(keys);
      for (int i = 0; i < keys.Length; i++)
        dictionary.Add(keys[i], values[i]);
      CheckValues(dictionary, keys, values);
      Assert.AreEqual(initialBucketCount, GetBucketCount(dictionary));
      var items = GetItemsCollection(dictionary);
      for (var i = 0; i < items.Length; i++)
        if (i < 6) {
          Assert.IsNotNull(items[i]);
          Assert.Greater(items[i].Length, 1);
        }
        else
          Assert.IsNull(items[i]);
    }

    [Test]
    public void RemoveTest()
    {
      var dictionary = new IntDictionary<int>(15);
      var keys = new[]
                 {
                   0, 0xA00000, 1, 0x100001, 0x200001, 0x500002, 0x800002, 0x300003,0xA00003,
                   0x400003, 0x700003, 0xA00004, 0x900005,0x700005, 0x200005, 0xE00005, 5
                 };
      var values = GenerateValues(keys);
      for (int i = 0; i < keys.Length; i++)
        dictionary.Add(keys[i], values[i]);
      Assert.IsFalse(dictionary.Remove(4));
      Assert.IsFalse(dictionary.Remove(0xB00003));
      CheckValues(dictionary, keys, values);
      var items = GetItemsCollection(dictionary);
      Assert.IsNotNull(items[4]);
      CheckRemoving(ref keys, ref values, 0xA00004, dictionary);
      Assert.IsNull(items[4]);
      CheckRemoving(ref keys, ref values, 1, dictionary);
      CheckRemoving(ref keys, ref values, 0x100001, dictionary);
      CheckRemoving(ref keys, ref values, 5, dictionary);
      CheckRemoving(ref keys, ref values, 0xE00005, dictionary);
    }

    [Test]
    public void ClearTest()
    {
      var dictionary = new IntDictionary<int>(15);
      var keys = new[]
                 {
                   0, 0xA00000, 1, 0x100001, 0x200001, 0x500002, 0x800002, 0x300003,0xA00003,
                   0x400003, 0x700003, 0xA00004, 0x900005,0x700005, 0x200005, 0xE00005, 5
                 };
      var values = GenerateValues(keys);
      for (int i = 0; i < keys.Length; i++)
        dictionary.Add(keys[i], values[i]);
      dictionary.Clear();
      var items = GetItemsCollection(dictionary);
      foreach (var bucket in items)
        Assert.IsNull(bucket);
    }
    
    [Test]
    public void DuplicateKeyInsertTest()
    {
      var dictionary = new IntDictionary<int>(15);
      var keys = new[] {0, 0x900005, 0x700005, 0x200005, 0xE00005, 5};
      var values = GenerateValues(keys);
      for (int i = 0; i < keys.Length; i++)
        dictionary.Add(keys[i], values[i]);
      AssertEx.Throws<ArgumentException>(() => dictionary.Add(0, 1));
      AssertEx.Throws<ArgumentException>(() => dictionary.Add(0x900005, 1));
      AssertEx.Throws<ArgumentException>(() => dictionary.Add(5, 1));
    }

    [Test]
    public void MaskRequiringOffsetTest()
    {
      var dictionary = new IntDictionary<int>(252);
      var keyOffsetField = typeof (IntDictionary<int>).GetField("keyOffset",
        BindingFlags.NonPublic | BindingFlags.Instance);
      Assert.Greater((int)keyOffsetField.GetValue(dictionary), 0);
      var keysCount = 155;
      var keys = new int[keysCount];
      for (int i = 0; i < keysCount; i++)
        keys[i] = 100 + i;
      var values = GenerateValues(keys);
      for (int i = 0; i < keys.Length; i++)
        dictionary.Add(keys[i], values[i]);
      CheckValues(dictionary, keys, values);
    }

    [Test]
    public void AddOrReplaceTest()
    {
      var dictionary = new IntDictionary<int>(15);
      var keys = new[] {0, 0x900005, 0x700005, 0x200005, 0xE00005, 5};
      var values = GenerateValues(keys);
      for (int i = 0; i < keys.Length; i++)
        dictionary.Add(keys[i], values[i]);
      var oldValue = dictionary[keys[0]];
      var newValue = -1;
      Assert.AreNotEqual(oldValue, newValue);
      dictionary[keys[0]] = newValue;
      values[0] = newValue;
      Assert.AreEqual(newValue, dictionary[keys[0]]);
      CheckValues(dictionary, keys, values);
      oldValue = dictionary[keys[2]];
      newValue = -2;
      Assert.AreNotEqual(oldValue, newValue);
      dictionary[keys[2]] = newValue;
      values[2] = newValue;
      Assert.AreEqual(newValue, dictionary[keys[2]]);
      CheckValues(dictionary, keys, values);
      var newKey = 0x100005;
      newValue = -3;
      Array.Resize(ref keys, keys.Length + 1);
      Array.Resize(ref values, values.Length + 1);
      keys[keys.Length - 1] = newKey;
      values[values.Length - 1] = newValue;
      dictionary[newKey] = newValue;
      Assert.AreEqual(newValue, dictionary[newKey]);
      CheckValues(dictionary, keys, values);
      newKey = 6;
      newValue = -4;
      Array.Resize(ref keys, keys.Length + 1);
      Array.Resize(ref values, values.Length + 1);
      keys[keys.Length - 1] = newKey;
      values[values.Length - 1] = newValue;
      dictionary[newKey] = newValue;
      Assert.AreEqual(newValue, dictionary[newKey]);
      CheckValues(dictionary, keys, values);
      /*newKey = 0x100006;
      newValue = -5;
      Array.Resize(ref keys, keys.Length + 1);
      Array.Resize(ref values, values.Length + 1);
      keys[keys.Length - 1] = newKey;
      values[values.Length - 1] = newValue;
      dictionary.Add(newKey, newValue);
      Assert.AreEqual(newValue, dictionary[newKey]);
      CheckValues(dictionary, keys, values);*/
    }
    
    [Test]
    [Category("Profile")]
    public void PerformanceTest()
    {
      var dictionary = new IntDictionary<int>(255);
      var keysCount = 355;
      var keys = new int[keysCount];
      for (int i = 0; i < keysCount; i++)
        keys[i] = 100 + i;
      var values = GenerateValues(keys);
      for (int i = 0; i < keys.Length; i++)
        dictionary.Add(keys[i], values[i]);
      int count = 10000000;
      MeasureIntDictionary(keys, dictionary, count);
      MeasureOriginalDictionary(keys, dictionary, count);
    }

    private static void MeasureIntDictionary(int[] keys, IntDictionary<int> dictionary, int count)
    {
      int keysLength = keys.Length;
      int t = 0;
      using (new Measurement("Reading IntDictionary<int>", count))
        for (int i = 0; i < count; i++)
          dictionary.TryGetValue(keys[i % keysLength], out t);
      Assert.Greater(t, int.MinValue);
    }

    private static void MeasureOriginalDictionary(int[] keys, IEnumerable<KeyValuePair<int, int>> dictionary,
      int count)
    {
      var keysLength = keys.Length;
      var t = 0;
      var reference = dictionary.ToDictionary(pair => pair.Key, pair => pair.Value);
      using (new Measurement("Reading Dictionary<int, int>", count))
        for (var i = 0; i < count; i++)
          reference.TryGetValue(keys[i % keysLength], out t);
      Assert.Greater(t, int.MinValue);
    }
    
    private static void CheckValues<T>(IntDictionary<T> dictionary, int[] keys, T[] values)
    {
      for (var i = 0; i < keys.Length; i++)
        Assert.AreEqual(values[i], dictionary[keys[i]]);
    }

    private static int GetBucketCount<T>(IntDictionary<T> dictionary)
    {
      return (GetItemsCollection(dictionary)).Length;
    }

    private static KeyValuePair<int, T>[][] GetItemsCollection<T>(IntDictionary<T> dictionary)
    {
      var itemsField = typeof (IntDictionary<T>)
        .GetField("items", BindingFlags.NonPublic | BindingFlags.Instance);
      return (KeyValuePair<int, T>[][])itemsField.GetValue(dictionary);
    }
    
    private static int[] GenerateValues(IEnumerable<int> keys)
    {
      var rnd = new Random();
      return (from key in keys select (int) (rnd.NextDouble() * 100000)).ToArray();
    }

    private static void CheckRemoving(ref int[] keys, ref int[] values, int keyToRemove,
      IntDictionary<int> dictionary)
    {
      Assert.IsTrue(dictionary.Remove(keyToRemove));
      Remove(ref keys, ref values, keyToRemove);
      AssertEx.Throws<KeyNotFoundException>(() => { var t = dictionary[keyToRemove]; });
      CheckValues(dictionary, keys, values);
    }

    private static void Remove(ref int[] keys, ref int[] values, int removedKey)
    {
      var removedValue = values[keys.IndexOf(removedKey)];
      keys = (from key in keys where key!=removedKey select key).ToArray();
      values = (from value in values where value!=removedValue select value).ToArray();
    }
  }
}