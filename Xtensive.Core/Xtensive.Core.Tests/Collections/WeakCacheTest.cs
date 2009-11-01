using System;
using System.Threading;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Testing;

namespace Xtensive.Core.Tests.Collections
{
  public class BadTestClass
  {
  }

  [TestFixture]
  public class WeakCacheTest
  {
    private WeakCache<string, TestClass> globalCache;
    private Random random = RandomManager.CreateRandom((int)DateTime.Now.Ticks);

    [Test]
    public void ConstructorsTest()
    {
      WeakCache<string, TestClass> cache = new WeakCache<string, TestClass>(
        100, value => value.Text);

      Assert.IsNotNull(cache.KeyExtractor);
      Assert.IsNotNull(cache.SizeExtractor);

      WeakCache<string, TestClass> cache1 = new WeakCache<string, TestClass>(
        100, value => value.Text, value => value.Text.Length);

      Assert.IsNotNull(cache1.KeyExtractor);
      Assert.IsNotNull(cache1.SizeExtractor);

      TestClass item = new TestClass("100");
      cache.Add(item);
      cache1.Add(item);
      Assert.AreEqual(1, cache.Count);
      Assert.AreEqual(1, cache1.Count);
      Assert.AreEqual("100".Length, cache.Size);
      Assert.AreEqual("100".Length, cache1.Size);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorDenyTest()
    {
      var cache = new WeakCache<string, TestClass>(
        100, null);
    }

    [Test]
    public void AddRemoveTest()
    {
      var cache = new WeakCache<string, TestClass>(
        100, value => value.Text);
      TestClass item = new TestClass("1");
      cache.Add(item);
      Assert.AreEqual(1, cache.Count);
      item = new TestClass("2");
      cache.Add(item);
      Assert.AreEqual(2, cache.Count);
      Assert.AreEqual(item, cache[item.Text, false]);
      ICache<string, TestClass> icache = cache;
      Assert.AreEqual(item, icache[item.Text, false]);
      Assert.AreEqual(null, icache["3", false]);
      cache.Remove(item);
      Assert.AreEqual(1, cache.Count);
      cache.Clear();
      Assert.AreEqual(0, cache.Count);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddDenyTest()
    {
      WeakCache<string, TestClass> cache = new WeakCache<string, TestClass>(
        100, value => value.Text);

      cache.Add(null);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddDenyTest1()
    {
      WeakCache<string, TestClass> cache = new WeakCache<string, TestClass>(
        100, value => value.Text);

      cache.Add(null);
    }

//    [Test]
//    [ExpectedException(typeof(ArgumentException))]
//    public void AddDenyTest2()
//    {
//      WeakCache<string, TestClass> cache = new WeakCache<string, TestClass>(
//        100, true, value => value.Text);
//
//      cache.Add(new BadTestClass());
//    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RemoveDenyTest()
    {
      WeakCache<string, TestClass> cache = new WeakCache<string, TestClass>(
        100, value => value.Text);

      cache.Remove((TestClass)null);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RemoveDenyTest1()
    {
      WeakCache<string, TestClass> cache = new WeakCache<string, TestClass>(
        100, value => value.Text);

      cache.Remove((TestClass)null);
    }

//    [Test]
//    [ExpectedException(typeof(ArgumentException))]
//    public void RemoveDenyTest2()
//    {
//      WeakCache<string, TestClass> cache = new WeakCache<string, TestClass>(
//        100, true, value => value.Text);
//
//      cache.Remove(new BadTestClass());
//    }

    [Test]
    public void IEnumerableTest()
    {
      WeakCache<string, TestClass> cache = new WeakCache<string, TestClass>(
        100, value => value.Text);

      for (int i = 0; i < 100; i++) {
        cache.Add(new TestClass("item " + i));
      }
      Assert.IsTrue(cache.Count >= 0, "Line 176");
      Assert.IsTrue(cache.Count <= 100, "Line 177");

      int itemsCount = 0;
      foreach (TestClass testClass in cache) {
        Assert.IsTrue(testClass == null || testClass.Text.StartsWith("item"), "Line 182");
        itemsCount++;
      }
      Assert.IsTrue(itemsCount >= 0, "Line 187");
      Assert.IsTrue(itemsCount <= 100, "Line 188");

      TestHelper.CollectGarbage(true);

      itemsCount = 0;
      foreach (TestClass testClass in cache) {
        Assert.IsTrue(testClass == null || testClass.Text.StartsWith("item"), "Line 196");
        itemsCount++;
      }
      Assert.IsTrue(itemsCount >= 0, "Line 201");
      Assert.IsTrue(itemsCount <= 100, "Line 202");
    }

    private static bool canFinish = true;

    [Test]
    public void SynchronizationTest()
    {
      globalCache = new WeakCache<string, TestClass>(
        100,
        value => value.Text);

      
      Thread[] addThreads = new Thread[10];
      Thread[] removeThreads = new Thread[10];
      for (int i = 0; i < 10; i++)
      {
        addThreads[i] = new Thread(AddItem);
        removeThreads[i] = new Thread(RemoveItem);
      }

      try {
        for (int i = 0; i < 10; i++) {
          addThreads[i].Start();
        }
        Thread.Sleep(10);
        for (int i = 0; i < 10; i++) {
          removeThreads[i].Start();
        }
        Thread.Sleep(200);
      }
      finally {
        canFinish = true;
        for (int i = 0; i < 10; i++)
        {
          removeThreads[i].Abort();
          addThreads[i].Abort();
        }
      }
      Assert.IsTrue(globalCache.Count >= 0);
      
      globalCache = null;
    }

    private void AddItem()
    {
      int count = random.Next(100000);
      while (!canFinish)
      {
        globalCache.Add(new TestClass("item " + count));
        count++;
      }
    }

    private void RemoveItem()
    {
      while (!canFinish) {
        TestClass test = null;
        foreach (TestClass testClass in globalCache)
        {
          test = testClass;
          break;
        }
        if (test != null)
          globalCache.Remove(test);
      }
    }
  }
}
