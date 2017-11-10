using System;
using System.Threading;
using NUnit.Framework;
using Xtensive.Caching;
using Xtensive.Conversion;
using Xtensive.Core;
using Xtensive.Orm.Tests;


namespace Xtensive.Orm.Tests.Core.Caching
{
  internal class TestClass : 
    IIdentified<string>,
    IHasSize
  {
    public string Text { get; set; }

    object IIdentified.Identifier
    {
      get { return Identifier; }
    }

    public string Identifier
    {
      get { return Text; }
    }

    public long Size
    {
      get { return Text.Length; }
    }

    public TestClass(string text)
    {
      Text = text;
    }
  }

  [Serializable]
  internal class TestClassAdvancedConverter :
    AdvancedConverterBase,
    IAdvancedConverter<TestClass, string>
  {
    public TestClassAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }

    #region IAdvancedConverter<TestClass,string> Members

    public string Convert(TestClass value)
    {
      return value.Text;
    }

    public bool IsRough
    {
      get { return false; }
    }

    #endregion
  }

  [TestFixture]
  public class LruCacheTest
  {
    private LruCache<string, TestClass, TestClass> globalCache;
    private Random random = RandomManager.CreateRandom((int)DateTime.Now.Ticks);
    
    class BadTestClass: 
      IIdentified<String>,
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
      var cache = new LruCache<string, TestClass, TestClass>(
        1000,
        value => value.Text);

      var cache1 = new LruCache<string, TestClass, TestClass>(
        1000,
        (value) => value.Text,
        new Biconverter<TestClass, TestClass>(value => value, value => value));


      TestClass item = new TestClass("1");
      cache.Add(item);
      cache1.Add(item);
      Assert.AreEqual(1, cache.Size);
      Assert.AreEqual(1, cache1.Count);
      
      for (int i=0;i<100000;i++) {
        TestClass test = new TestClass(""+i);
        cache1.Add(test);
      }
    }

    [Test]
    public void ConstructorDenyTest()
    {
      Assert.Throws<ArgumentOutOfRangeException>(() => {
        var cache =
          new LruCache<string, TestClass, TestClass>(
            -1,
            value => value.Text
          );
      });
    }

    [Test]
    public void AddRemoveTest()
    {
      var cache = new LruCache<string, TestClass, TestClass>(
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
      var cache = new LruCache<string, TestClass, TestClass>(
        100,
        value => value.Text);
      Assert.Throws<ArgumentNullException>(() => cache.Add(null));
    }

    [Test]
    public void AddDenyTest3()
    {
      var cache =
        new LruCache<string, BadTestClass, BadTestClass>(
          100,
          value => value.Identifier);
      Assert.Throws<ArgumentNullException>(() => cache.Add(new BadTestClass()));
    }

    [Test]
    public void RemoveDenyTest1()
    {
      var cache =
        new LruCache<string, TestClass, TestClass>(
          100,
          value => value.Text);
      Assert.Throws<ArgumentNullException>(() => cache.Remove(null));
    }

    [Test]
    public void RemoveDenyTest2()
    {
      var cache =
        new LruCache<string, TestClass, TestClass>(
          100,
          value => value.Text);
      Assert.Throws<ArgumentNullException>(() => cache.RemoveKey(null));
    }

    [Test]
    public void RemoveDenyTest3()
    {
      var cache =
        new LruCache<string, BadTestClass, BadTestClass>(
          100,
          value => value.Identifier);
      BadTestClass test1 = new BadTestClass();
      Assert.Throws<ArgumentNullException>(() => cache.Remove(test1));
    }
    
    [Test]
    public void IEnumerableTest()
    {
      var cache =
        new LruCache<string, TestClass, TestClass>(
          10000,
          value => value.Text);
      for (int i = 0; i < 100; i++)
      {
        cache.Add(new TestClass("item " + i));
      }
      Assert.AreEqual(100, cache.Count);

      int itemsCount = 0;
      foreach (TestClass testClass in cache)
      {
        Assert.IsTrue(testClass.Text.StartsWith("item"));
        itemsCount++;
      }
      Assert.AreEqual(100, itemsCount);
    }

    private static bool canFinish = true;
    [Test]
    public void SynchronizationTest()
    {
      globalCache =
        new LruCache<string, TestClass, TestClass>(
          1000,
          value => value.Text);

      
      var addThreads = new Thread[10];
      var removeThreads = new Thread[10];
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
        for (int i = 0; i < 10; i++) {
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
