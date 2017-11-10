using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Orm.Tests;


namespace Xtensive.Orm.Tests.Core.Collections
{

  [TestFixture]
  public class CollectionBaseTest
  {
    private IntegerCollection globalCollection;
    private readonly Random random = RandomManager.CreateRandom((int)DateTime.Now.Ticks);

    [Serializable]
    public class IntegerCollection : CollectionBase<int>
    {
      public IntegerCollection()
      {
      }
    }

    [Test, Ignore("")]
    public void SynchronizationTest()
    {
      globalCollection = new IntegerCollection();

      
      Thread[] addThreads = new Thread[10];
      Thread[] removeThreads = new Thread[10];
      Thread[] getThreads = new Thread[10];
      Thread[] containsThreads = new Thread[10];
      for (int i = 0; i < 10; i++)
      {
        addThreads[i] = new Thread(AddItem);
        removeThreads[i] = new Thread(RemoveItem);
        getThreads[i] = new Thread(GetItem);
        containsThreads[i] = new Thread(Contains);
      }

      for (int i = 0; i < 10; i++) {
        addThreads[i].Start();
        getThreads[i].Start();
        containsThreads[i].Start();
      }
      Thread.Sleep(10);

      for (int i = 0; i < 10; i++) {
        removeThreads[i].Start();
      }
      Thread.Sleep(1000);

      for (int i = 0; i < 10; i++) {
        removeThreads[i].Abort();
        addThreads[i].Abort();
      }

      Assert.IsTrue(globalCollection.Count > 0);
      globalCollection = null;
    }

    private void AddItem()
    {
      int count = random.Next(100000);
      while (true) {
        globalCollection.Add(count);
        count++;
      }
    }

    private void GetItem()
    {
      int value = globalCollection[globalCollection.Count/2];
    }

    private void Contains()
    {
      int item = random.Next(100000);
      globalCollection.Contains(item);
    }

    private void RemoveItem()
    {
      while (true) {
        int test=0;
        foreach (int n in globalCollection)
        {
          test = n;
          break;
        }
        globalCollection.Remove(test);
      }
    }

    [Test]
    public void SerializationTest()
    {
      globalCollection = new IntegerCollection();
      MemoryStream ms = new MemoryStream();
      BinaryFormatter bf = new BinaryFormatter();
      bf.Serialize(ms, globalCollection);

      ms.Seek(0, SeekOrigin.Begin);
      globalCollection = (IntegerCollection)bf.Deserialize(ms);
    }
  }
}
