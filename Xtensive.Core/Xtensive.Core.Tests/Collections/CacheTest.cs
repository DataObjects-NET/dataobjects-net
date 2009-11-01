using System;
using System.Threading;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Collections;
using System.Collections;
using Xtensive.Core.Conversion;
using Xtensive.Core.Testing;


namespace Xtensive.Core.Tests.Collections
{
  internal class TestClass : 
    IIdentified<string>,
    IHasSize
  {
    private string text;

    public string Text
    {
      get { return text; }
      set { text = value; }
    }

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
      this.text = text;
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
  public class CacheTest
  {
    private Cache<string, TestClass, TestClass> globalCache;
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
      Cache<string, TestClass, TestClass> cache = new Cache<string, TestClass, TestClass>(
        1000,
        value => value.Text);

      Cache<string, TestClass, TestClass> cache1 = new Cache<string, TestClass, TestClass>(
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
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ConstructorDenyTest()
    {
      Cache<string, TestClass, TestClass> cache =
        new Cache<string, TestClass, TestClass>(
          -1,
          value => value.Text
          );
    }

    [Test]
    public void AddRemoveTest()
    {
      Cache<string, TestClass, TestClass> cache = new Cache<string, TestClass, TestClass>(
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
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddDenyTest1()
    {
      Cache<string, TestClass, TestClass> cache = new Cache<string, TestClass, TestClass>(
        100,
        value => value.Text);
      cache.Add(null);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddDenyTest3()
    {
      Cache<string, BadTestClass, BadTestClass> cache =
        new Cache<string, BadTestClass, BadTestClass>(
          100,
          value => value.Identifier);

      cache.Add(new BadTestClass());
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RemoveDenyTest1()
    {
      Cache<string, TestClass, TestClass> cache =
        new Cache<string, TestClass, TestClass>(
          100,
          value => value.Text);
      cache.Remove((TestClass)null);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RemoveDenyTest2()
    {
      Cache<string, TestClass, TestClass> cache =
        new Cache<string, TestClass, TestClass>(
          100,
          value => value.Text);
      cache.Remove((string)null);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RemoveDenyTest3()
    {
      Cache<string, BadTestClass, BadTestClass> cache =
        new Cache<string, BadTestClass, BadTestClass>(
          100,
          value => value.Identifier);
      BadTestClass test1 = new BadTestClass();
      cache.Remove(test1);
    }
    
    [Test]
    public void IEnumerableTest()
    {
      Cache<string, TestClass, TestClass> cache =
        new Cache<string, TestClass, TestClass>(
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
        new Cache<string, TestClass, TestClass>(
          1000,
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
