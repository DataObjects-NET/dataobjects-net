using System;
using System.Threading;
using NUnit.Framework;
using Xtensive.Caching;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Core.Caching
{
  public class BadTestClass
  {
  }

  [TestFixture]
  public class WeakCacheTest
  {
    private ICache<string, TestClass> globalCache;
    private Random random = RandomManager.CreateRandom((int)DateTime.Now.Ticks);

    [Test]
    public void ConstructorsTest()
    {
      ICache<string, TestClass> cache = new WeakCache<string, TestClass>(
        false, value => value.Text);

      Assert.IsNotNull(cache.KeyExtractor);

      var item = new TestClass("100");
      cache.Add(item);

      Assert.AreEqual(1, cache.Count);
    }

    [Test]
    public void ConstructorDenyTest()
    {
      Assert.Throws<ArgumentNullException>(() => { var cache = new WeakCache<string, TestClass>(false, null); });
    }

    [Test]
    public void AddRemoveTest()
    {
      ICache<string, TestClass> cache = new WeakCache<string, TestClass>(false, value => value.Text);

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
    public void AddDenyTest()
    {
      ICache<string, TestClass> cache = new WeakCache<string, TestClass>(false, value => value.Text);
      Assert.Throws<ArgumentNullException>(() => cache.Add(null));
    }

    [Test]
    public void RemoveDenyTest()
    {
      ICache<string, TestClass> cache = new WeakCache<string, TestClass>(false, value => value.Text);
      Assert.Throws<ArgumentNullException>(() => cache.Remove((TestClass) null));
    }

    [Test]
    public void RemoveDenyTest1()
    {
      ICache<string, TestClass> cache = new WeakCache<string, TestClass>(false, value => value.Text);
      Assert.Throws<ArgumentNullException>(() => cache.Remove((TestClass)null));
    }

    [Test]
    public void IEnumerableTest()
    {
      ICache<string, TestClass> cache = new WeakCache<string, TestClass>(false, value => value.Text);

      for (int i = 0; i < 100; i++)
        cache.Add(new TestClass("item " + i));

      Assert.IsTrue(cache.Count >= 0);
      Assert.IsTrue(cache.Count <= 100);

      int itemsCount = 0;
      foreach (TestClass testClass in cache) {
        Assert.IsTrue(testClass == null || testClass.Text.StartsWith("item"), "Line 182");
        itemsCount++;
      }
      Assert.IsTrue(itemsCount >= 0);
      Assert.IsTrue(itemsCount <= 100);

      TestHelper.CollectGarbage(true);

      itemsCount = 0;
      foreach (TestClass testClass in cache) {
        Assert.IsTrue(testClass == null || testClass.Text.StartsWith("item"), "Line 196");
        itemsCount++;
      }
      Assert.IsTrue(itemsCount >= 0);
      Assert.IsTrue(itemsCount <= 100);
    }

    private static bool canFinish = true;

    [Test]
    public void SynchronizationTest()
    {
      globalCache = new WeakCache<string, TestClass>(false, value => value.Text);
      
      var addThreads = new Thread[10];
      var removeThreads = new Thread[10];
      for (int i = 0; i < 10; i++) {
        addThreads[i] = new Thread(AddItem);
        removeThreads[i] = new Thread(RemoveItem);
      }

      try {
        for (int i = 0; i < 10; i++)
          addThreads[i].Start();
        Thread.Sleep(10);
        for (int i = 0; i < 10; i++)
          removeThreads[i].Start();
        Thread.Sleep(200);
      }
      finally {
        canFinish = true;
        for (int i = 0; i < 10; i++) {
          addThreads[i].Join();
          removeThreads[i].Join();
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
        lock (globalCache) {
          globalCache.Add(new TestClass("Item " + count));
        }
        count++;
      }
    }

    private void RemoveItem()
    {
      while (!canFinish) {
        TestClass test = null;
        lock (globalCache) {
          foreach (TestClass testClass in globalCache) {
            test = testClass;
            break;
          }
        }
        if (test != null) lock (globalCache) {
          globalCache.Remove(test);
        }
      }
    }
  }
}
