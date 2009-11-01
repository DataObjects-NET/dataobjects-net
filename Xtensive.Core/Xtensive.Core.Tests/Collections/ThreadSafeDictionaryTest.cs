// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.20

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;

namespace Xtensive.Core.Tests.Collections
{
  [TestFixture]
  public class ThreadSafeDictionaryTest
  {
    private const int FillCount = 10;
    private Dictionary<int, int> d = new Dictionary<int, int>();
    private ThreadSafeDictionary<int, int> tsd = ThreadSafeDictionary<int, int>.Create();
    

    [Test]
    public void CombinedTest()
    {
      ThreadSafeDictionary<int, int> d1 = ThreadSafeDictionary<int, int>.Create();
      Assert.AreEqual(d1.GetValue(0), 0);
      d1.SetValue(0,1);
      Assert.AreEqual(d1.GetValue(0), 1);

      ThreadSafeDictionary<int, int> d2 = ThreadSafeDictionary<int, int>.Create();
      d2.SetValue(0,2);
      Assert.AreEqual(d2.GetValue(0), 2);

      Assert.AreNotEqual(d1.GetValue(0), d2.GetValue(0));
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PerformanceTest()
    {
      FillThreadSafeDictionary();
      FillDictionary();
      DictionaryReadTest(1000);
      ThreadSafeDictionaryReadTest(1000);
      int count = 10000000;
      int r1, r2;
      using (new Measurement("Reading ThreadSafeDictionary<int, int>", count)) {
        r1 = ThreadSafeDictionaryReadTest(count);
      }
      using (new Measurement("Reading Dictionary", count)) {
        r2 = DictionaryReadTest(count);
      }
      Assert.AreEqual(r1, r2);
    }

    private void FillThreadSafeDictionary()
    {
      for (int i = 0; i<FillCount; i++)
        tsd.SetValue(i, i);
    }

    private void FillDictionary()
    {
      for (int i = 0; i<FillCount; i++)
        d.Add(i, i);
    }

    private int ThreadSafeDictionaryReadTest(int count)
    {
      int j = 0;
      for (int i = 0; i < count; i++)
        j += tsd.GetValue(i%FillCount);
      return j;
    }

    private int DictionaryReadTest(int count)
    {
      int j = 0, value;
      for (int i = 0; i < count; i++) {
        d.TryGetValue(i%FillCount, out value);
        j += value;
      }
      return j;
    }
  }
}