// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.20

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Collections;

namespace Xtensive.Orm.Tests.Core.Collections
{
  [TestFixture]
  public class ThreadSafeDictionaryTest
  {
    private const int FillCount = 10;
    private Dictionary<int, int> d = new Dictionary<int, int>();
    private ThreadSafeDictionary<int, int> tsd = ThreadSafeDictionary<int, int>.Create(new object());
    

    [Test]
    public void CombinedTest()
    {      
      var dictionary = ThreadSafeDictionary<int, string>.Create(new object());

      // SetValue
      string one;
      Assert.IsFalse(
        dictionary.TryGetValue(1, out one));
      Assert.IsNull(one);

      dictionary.SetValue(1, "One");

      Assert.IsTrue(
        dictionary.TryGetValue(1, out one));

      Assert.AreEqual("One", one);

      // GetValue with generaors
      string two = dictionary.GetValue(2,
        delegate { return "Two"; });

      Assert.AreEqual("Two", two);

      bool twoRecalculated = false;

      two = dictionary.GetValue(2,
        delegate 
        { 
          twoRecalculated = true;
          return "Two";          
        });

      Assert.AreEqual("Two", two);
      Assert.IsFalse(twoRecalculated); 

      // Null value
      string zero = dictionary.GetValue(0,
        delegate { return null; });

      Assert.AreEqual(null, zero);

      bool zeroRecalculated = false;

      zero = dictionary.GetValue(0,
        delegate 
        { 
          zeroRecalculated = true;
          return null;
        });

      Assert.IsNull(zero);
      Assert.IsFalse(zeroRecalculated); 


      var dictionary2 = ThreadSafeDictionary<int, int>.Create(new object());

      int i;
      dictionary2.TryGetValue(0, out i);
      Assert.AreEqual(0, i);
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
      int j = 0, value;
      for (int i = 0; i < count; i++) {        
        if (tsd.TryGetValue(i%FillCount, out value))
          j += value;
      }
        
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