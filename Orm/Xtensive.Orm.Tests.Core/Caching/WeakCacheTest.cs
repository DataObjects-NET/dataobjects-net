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
    private readonly Random random = RandomManager.CreateRandom((int)DateTime.Now.Ticks);
    private WeakCache<string, TestClass> globalCache;

    [TearDown]
    public void TearDown()
    {
      if (globalCache is not null) {
        globalCache.Dispose();
      }
    }

    [Test]
    public void ConstructorsTest()
    {
      using (var cache = new WeakCache<string, TestClass>(
        false, value => value.Text)) {

        Assert.That(cache.KeyExtractor, Is.Not.Null);

        var item = new TestClass("100");
        cache.Add(item);

        Assert.That(cache.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void ConstructorDenyTest()
    {
      _ = Assert.Throws<ArgumentNullException>(() => { var cache = new WeakCache<string, TestClass>(false, null); });
    }

    [Test]
    public void AddRemoveTest()
    {
      using (var cache = new WeakCache<string, TestClass>(false, value => value.Text)) {

        TestClass item = new TestClass("1");
        cache.Add(item);
        Assert.That(cache.Count, Is.EqualTo(1));

        item = new TestClass("2");
        cache.Add(item);
        Assert.That(cache.Count, Is.EqualTo(2));
        Assert.That(cache[item.Text, false], Is.EqualTo(item));

        ICache<string, TestClass> icache = cache;
        Assert.That(icache[item.Text, false], Is.EqualTo(item));
        Assert.That(icache["3", false], Is.EqualTo(null));

        cache.Remove(item);
        Assert.That(cache.Count, Is.EqualTo(1));

        cache.Clear();
        Assert.That(cache.Count, Is.EqualTo(0));
      }
    }

    [Test]
    public void AddDenyTest()
    {
      var cache = new WeakCache<string, TestClass>(false, value => value.Text);
      _ = Assert.Throws<ArgumentNullException>(() => cache.Add(null));
    }

    [Test]
    public void RemoveDenyTest()
    {
      var cache = new WeakCache<string, TestClass>(false, value => value.Text);
      _ = Assert.Throws<ArgumentNullException>(() => cache.Remove((TestClass)null));
    }

    [Test]
    public void RemoveDenyTest1()
    {
      var cache = new WeakCache<string, TestClass>(false, value => value.Text);
      _ = Assert.Throws<ArgumentNullException>(() => cache.Remove((TestClass)null));
    }

    [Test]
    public void IEnumerableTest()
    {
      using (var cache = new WeakCache<string, TestClass>(false, value => value.Text)) {

        for (int i = 0; i < 100; i++)
          cache.Add(new TestClass("item " + i));

        Assert.That(cache.Count >= 0, Is.True);
        Assert.That(cache.Count <= 100, Is.True);

        int itemsCount = 0;
        foreach (TestClass testClass in cache) {
          Assert.That(testClass == null || testClass.Text.StartsWith("item"), Is.True, "Line 182");
          itemsCount++;
        }
        Assert.That(itemsCount >= 0, Is.True);
        Assert.That(itemsCount <= 100, Is.True);

        TestHelper.CollectGarbage(true);

        itemsCount = 0;
        foreach (TestClass testClass in cache) {
          Assert.That(testClass == null || testClass.Text.StartsWith("item"), Is.True, "Line 196");
          itemsCount++;
        }
        Assert.That(itemsCount >= 0, Is.True);
        Assert.That(itemsCount <= 100, Is.True);
      }
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
      Assert.That(globalCache.Count >= 0, Is.True);

      globalCache.Dispose();
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
