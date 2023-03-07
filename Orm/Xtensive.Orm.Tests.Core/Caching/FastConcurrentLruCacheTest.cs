// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Caching;
using Xtensive.Conversion;
using Xtensive.Core;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Core.Caching
{
  [TestFixture]
  public class FastConcurrentLruCacheTest
  {
    private class BadTestClass : IIdentified<string>, IHasSize
      {
      object IIdentified.Identifier => Identifier;
      public string Identifier => null;
      public long Size => 1;
      }

    private const int TestCacheCapacity = 6; // divides by 3

    private FastConcurrentLruCache<string, TestClass> globalCache;
    private readonly Random random = RandomManager.CreateRandom((int) DateTime.Now.Ticks);

    [Test]
    public void ConstructorsTest()
    {
      const int maxCount = 1000;

      var cache = CreateCacheInstance(maxCount);

      var cache1 = CreateCacheInstance(maxCount);

      var item = new TestClass("1");
      cache.Add(item);
      cache1.Add(item);
      Assert.AreEqual(1, cache1.Count);

      for (var i = 0; i < 100000; i++) {
        cache1.Add(new TestClass("" + i));
      }
    }

    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void ConstructorDenyTest(int deniedCapacity)
    {
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => {
        _ = CreateCacheInstance(deniedCapacity);
      });
    }

    [Test]
    [Ignore("BitFaster .Clear() implementation does not remove all items")]
    public void AddRemoveTest()
    {
      var cache = CreateCacheInstance(TestCacheCapacity);

      var item = new TestClass("1");
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
    public void AddDenyTest1()
    {
      var cache = CreateCacheInstance(TestCacheCapacity);
      _ = Assert.Throws<ArgumentNullException>(() => cache.Add(null));
    }

    [Test]
    public void AddDenyTest3()
    {
      var cache = new FastConcurrentLruCache<string, BadTestClass>(
          TestCacheCapacity,
          value => value.Identifier);
      _ = Assert.Throws<ArgumentNullException>(() => cache.Add(new BadTestClass()));
    }

    [Test]
    public void RemoveDenyTest1()
    {
      var cache = CreateCacheInstance(TestCacheCapacity);
      _ = Assert.Throws<ArgumentNullException>(() => cache.Remove(null));
    }

    [Test]
    public void RemoveDenyTest2()
    {
      var cache = CreateCacheInstance(TestCacheCapacity);
      _ = Assert.Throws<ArgumentNullException>(() => cache.RemoveKey(null));
    }

    [Test]
    public void RemoveDenyTest3()
    {
      var cache =
        new FastConcurrentLruCache<string, BadTestClass>(
          TestCacheCapacity,
          value => value.Identifier);
      var test1 = new BadTestClass();
      _ = Assert.Throws<ArgumentNullException>(() => cache.Remove(test1));
    }

    [Test]
    public void SynchronizationTest()
    {
      globalCache =
        new FastConcurrentLruCache<string, TestClass>(
          1000,
          value => value.Text);

      using (new ThreadPoolThreadsIncreaser(20, 20)) {
        var addThreads = new Task[10];
        var removeThreads = new Task[10];
        var cancellationTokenSource = new CancellationTokenSource();

        for (var i = 0; i < 10; i++) {
          addThreads[i] = new Task(() => AddItem(cancellationTokenSource.Token), cancellationTokenSource.Token);
          removeThreads[i] = new Task(() => RemoveItem(cancellationTokenSource.Token), cancellationTokenSource.Token);
        }

        try {
          for (var i = 0; i < 10; i++) {
            addThreads[i].Start();
          }
          Thread.Sleep(10);

          for (var i = 0; i < 10; i++) {
            removeThreads[i].Start();
          }
          Thread.Sleep(200);
        }
        finally {
          cancellationTokenSource.Cancel();
          Thread.Sleep(20);
        }
      }

      Assert.IsTrue(globalCache.Count >= 0);
      globalCache = null;
    }

    private void AddItem(CancellationToken cancellationToken)
    {
      var count = random.Next(100000);
      while (!cancellationToken.IsCancellationRequested) {
        globalCache.Add(new TestClass("item " + count));
        count++;
      }
      cancellationToken.ThrowIfCancellationRequested();
    }

    private void RemoveItem(CancellationToken cancellationToken)
    {
      while (!cancellationToken.IsCancellationRequested) {
        TestClass test = null;
        foreach (TestClass testClass in globalCache) {
          test = testClass;
          break;
        }
        if (test != null) {
          globalCache.Remove(test);
      }
      }
      cancellationToken.ThrowIfCancellationRequested();
    }

    private FastConcurrentLruCache<string, TestClass> CreateCacheInstance(int maxCount) =>
      new FastConcurrentLruCache<string, TestClass>(maxCount, value => value.Text);


    private class ThreadPoolThreadsIncreaser : Disposable
    {
      private int previousWorkingThreadsCount;
      private int previousIOThreadsCount;

      private static Func<int> A;
      private static Func<int> B;

      private void Increase(int workingThreadsCount, int ioThreadsCount)
      {
        ThreadPool.GetMinThreads(out var minWorkingThreads, out var minIOTheads);
        previousWorkingThreadsCount = minWorkingThreads;
        previousIOThreadsCount = minIOTheads;

        _ = ThreadPool.SetMinThreads(workingThreadsCount, ioThreadsCount);
      }

      private static void Decrease(Func<int> workingThreadsCountAcccessor, Func<int> ioThreadsCountAcccessor)
      {
        _ = ThreadPool.SetMinThreads(workingThreadsCountAcccessor(), ioThreadsCountAcccessor());
      }

      private int Aa() => previousWorkingThreadsCount;

      private int Bb() => previousIOThreadsCount;


      public ThreadPoolThreadsIncreaser(int workingThreadsCount, int ioThreadsCount)
        : base((disposing) => Decrease(A, B))
      {
        Increase(workingThreadsCount, ioThreadsCount);
        A = Aa;
        B = Bb;
      }
    }
  }
}
