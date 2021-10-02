using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Caching;
using Xtensive.Conversion;
using Xtensive.Core;
using Xtensive.Orm.Tests;

#pragma warning disable IDE0058

namespace Xtensive.Orm.Tests.Core.Caching
{
  [TestFixture]
  public class FastConcurrentLruCacheTest
  {
    private FastConcurrentLruCache<string, TestClass> globalCache;
    private readonly Random random = RandomManager.CreateRandom((int) DateTime.Now.Ticks);

    private class BadTestClass :
      IIdentified<string>,
      IHasSize
    {
      object IIdentified.Identifier
      {
        get { return Identifier; }
      }

      public string Identifier
      {
        get { return null; }
      }

      public long Size
      {
        get { return 1; }
      }
    }

    [Test]
    public void ConstructorsTest()
    {
      var cache = new FastConcurrentLruCache<string, TestClass>(
        1000,
        value => value.Text);

      var cache1 = new FastConcurrentLruCache<string, TestClass>(
        1000,
        (value) => value.Text
        );


      TestClass item = new TestClass("1");
      cache.Add(item);
      cache1.Add(item);
      Assert.AreEqual(1, cache1.Count);

      for (int i = 0; i < 100000; i++) {
        TestClass test = new TestClass("" + i);
        cache1.Add(test);
      }
    }

    [Test]
    public void ConstructorDenyTest()
    {
      Assert.Throws<ArgumentOutOfRangeException>(() => {
        var cache =
          new FastConcurrentLruCache<string, TestClass>(
            -1,
            value => value.Text
          );
      });
    }

    [Test]
    public void AddRemoveTest()
    {
      var cache = new FastConcurrentLruCache<string, TestClass>(
        100,
        value => value.Text);

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
    public void AddDenyTest1()
    {
      var cache = new FastConcurrentLruCache<string, TestClass>(
        100,
        value => value.Text);
      Assert.Throws<ArgumentNullException>(() => cache.Add(null));
    }

    [Test]
    public void AddDenyTest3()
    {
      var cache =
        new FastConcurrentLruCache<string, BadTestClass>(
          100,
          value => value.Identifier);
      Assert.Throws<ArgumentNullException>(() => cache.Add(new BadTestClass()));
    }

    [Test]
    public void RemoveDenyTest1()
    {
      var cache =
        new FastConcurrentLruCache<string, TestClass>(
          100,
          value => value.Text);
      Assert.Throws<ArgumentNullException>(() => cache.Remove(null));
    }

    [Test]
    public void RemoveDenyTest2()
    {
      var cache =
        new FastConcurrentLruCache<string, TestClass>(
          100,
          value => value.Text);
      Assert.Throws<ArgumentNullException>(() => cache.RemoveKey(null));
    }

    [Test]
    public void RemoveDenyTest3()
    {
      var cache =
        new FastConcurrentLruCache<string, BadTestClass>(
          100,
          value => value.Identifier);
      BadTestClass test1 = new BadTestClass();
      Assert.Throws<ArgumentNullException>(() => cache.Remove(test1));
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

        for (int i = 0; i < 10; i++) {
          addThreads[i] = new Task(() => AddItem(cancellationTokenSource.Token), cancellationTokenSource.Token);
          removeThreads[i] = new Task(() => RemoveItem(cancellationTokenSource.Token), cancellationTokenSource.Token);
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
          cancellationTokenSource.Cancel();
          Thread.Sleep(20);
        }
      }

      Assert.IsTrue(globalCache.Count >= 0);
      globalCache = null;
    }

    private void AddItem(CancellationToken cancellationToken)
    {
      int count = random.Next(100000);
      int counter = 0;
      bool whileCondition = (counter++) < 10 || !cancellationToken.IsCancellationRequested;
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
        if (test != null)
          globalCache.Remove(test);
      }
      cancellationToken.ThrowIfCancellationRequested();
    }

    private class ThreadPoolThreadsIncreaser : Disposable
    {
      private int previousWorkingThreadsCount;
      private int previousIOThreadsCount;

      private static Func<int> A;
      private static Func<int> B;

      private void Increase(int workingThreadsCount, int ioThreadsCount)
      {
        int minWorkingThreads;
        int minIOTheads;
        ThreadPool.GetMinThreads(out minWorkingThreads, out minIOTheads);
        previousWorkingThreadsCount = minWorkingThreads;
        previousIOThreadsCount = minIOTheads;

        ThreadPool.SetMinThreads(workingThreadsCount, ioThreadsCount);
      }

      private static void Decrease(Func<int> workingThreadsCountAcccessor, Func<int> ioThreadsCountAcccessor)
      {
        ThreadPool.SetMinThreads(workingThreadsCountAcccessor(), ioThreadsCountAcccessor());
      }

      private int Aa()
      {
        return previousWorkingThreadsCount;
      }

      private int Bb()
      {
        return previousIOThreadsCount;
      }


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
