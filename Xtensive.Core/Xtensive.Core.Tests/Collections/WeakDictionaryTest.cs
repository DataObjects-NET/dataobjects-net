using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Testing;

namespace Xtensive.Core.Tests.Collections
{
  [TestFixture]
  public class WeakDictionaryTest
  {
    Dictionary<int, TestClass> dictionary = new Dictionary<int, TestClass>();
    List<Pair<int, TestClass>> list = new List<Pair<int, TestClass>>();

    public class TestClass
    {
      public string Text;

      public TestClass(string text)
      {
        Text = text;
      }
    }

    [Test]
    public void RandomTest()
    {
      const int count = 100000;
      WeakDictionary<int, TestClass> dictionary =
        new WeakDictionary<int, TestClass>(count, EqualityComparer<int>.Default);
      List<TestClass> references = new List<TestClass>();
      List<TestClass> tempReferences = new List<TestClass>();
      for (int i = 0; i < count; i++) {
        TestClass value = new TestClass(Guid.NewGuid().ToString());
        if (i%2==0)
          references.Add(value);
        else
          tempReferences.Add(value);
        dictionary.Add(i, value);
        value = null;
      }
      Assert.AreEqual(count, dictionary.Count);

      tempReferences = null;
      TestHelper.CollectGarbage(true);
      Assert.AreEqual(count, dictionary.Count);
      int enumeratorCount = 0;
      foreach (TestClass reference in references)
        enumeratorCount++;
      Assert.AreEqual(enumeratorCount, count/2);
      dictionary.Cleanup();
      Assert.AreEqual(dictionary.Count, count/2);
//      Assert.AreEqual(dictionary.Keys.Count, count/2);
//      Assert.AreEqual(dictionary.Values.Count, count/2);
    }

    [Test]
    public void MethodsAndPropertiesTest()
    {
      WeakDictionary<int, TestClass> dictionary = new WeakDictionary<int, TestClass>();
      Assert.AreEqual(dictionary.Count, 0);
      Assert.AreEqual(false, dictionary.ContainsKey(123));
      
      TestClass value1 = new TestClass("test");
      dictionary.Add(1, value1);
      Assert.AreEqual(dictionary.Count, 1);
      Assert.AreEqual(true, dictionary.ContainsKey(1));
      
      TestClass value1Copy = dictionary[1];
      Assert.AreEqual(value1Copy.Text, value1.Text);
      Assert.AreEqual(value1Copy, value1);
      
      TestClass value2 = new TestClass("test2");
      dictionary.Add(2, value2);
      Assert.AreEqual(dictionary.Count, 2);
      Assert.AreEqual(true, dictionary.ContainsKey(2));
      
      value1Copy = value1 = null;
      TestHelper.CollectGarbage(true);
      dictionary.Cleanup();
      Assert.AreEqual(1, dictionary.Count);
      Assert.AreEqual(false, dictionary.ContainsKey(1));
      Assert.AreEqual(true, dictionary.ContainsKey(2));

      Assert.IsNotNull(value2);
      value2 = null;
      TestHelper.CollectGarbage(true);
      TestClass result;
      Assert.IsFalse(dictionary.TryGetValue(2, out result));
      TestClass newValue2 = new TestClass("New value 2");
      dictionary.Add(2, newValue2);
      Assert.AreEqual(dictionary.Count, 1);
      Assert.AreEqual(true, dictionary.ContainsKey(2));
      dictionary.Clear();
      Assert.AreEqual(dictionary.Count, 0);
      Assert.AreEqual(false, dictionary.ContainsKey(2));

    }

    [Test]
    public void DisposeAndFinalize()
    {
      WeakDictionary<int, TestClass> dictionary = new WeakDictionary<int, TestClass>();
      
      dictionary = null;
      TestHelper.CollectGarbage(true);
      dictionary = new WeakDictionary<int, TestClass>();
    }


    [Test]
    public void OLD_MultipleDictionariesTest()
    {
      WeakDictionary<int, TestClass> dictionary = new WeakDictionary<int, TestClass>();
      WeakDictionary<string, TestClass> dictionary1 = new WeakDictionary<string, TestClass>();
      TestClass test = new TestClass("TestString");
      dictionary.Add(1, test);
      dictionary1.Add("TestString", test);
    }


    [Test]
    public void OLD_ReusableReferenceTest()
    {
      WeakDictionary<int, TestClass> dictionary = new WeakDictionary<int, TestClass>();
      TestClass test = new TestClass("TestString");

      dictionary.Add(1, test);
      TestHelper.CollectGarbage(true);
      Assert.AreEqual(dictionary[1], test);
      Assert.AreEqual(test.Text, "TestString");
      
      test = null;
      TestHelper.CollectGarbage(true);
      dictionary.Cleanup();
      Assert.IsFalse(dictionary.ContainsKey(1));
      test = new TestClass("TestString1");
      dictionary.Add(2, test);
      Assert.AreEqual(dictionary[2], test);
      TestHelper.CollectGarbage(true);
    }


    [Test]
    public void OLD_ReusableReferenceLoadTest()
    {
      int max = 600000;
      Dictionary<int, string> simpleHash = new Dictionary<int, string>();
      WeakDictionary<int, string> dictionary = new WeakDictionary<int, string>();

      DateTime startTime = DateTime.Now;
      for (int i = 0; i < max; i++) {
        simpleHash.Add(i, "test" + i);
      }

      int counter = 0;
      for (int i = 0; i < max; i++) {
        if (simpleHash[i]!=null)
          counter++;
      }
      Log.Info("Cycle " + DateTime.Now.Subtract(startTime) + " counter:" + counter + " for simple hash");
      simpleHash = null;

      startTime = DateTime.Now;
      for (int i = 0; i < max; i++) {
        string testString = "test" + i;
        dictionary.Add(i, testString);
      }
      Log.Info("Cycle set:" + DateTime.Now.Subtract(startTime));

      counter = 0;
      for (int i = 0; i < max; i++) {
        if (dictionary.ContainsKey(i)) {
          counter++;
        }
      }
      Log.Info("Cycle " + DateTime.Now.Subtract(startTime) + " counter:" + counter);
      Log.Info("Objects left: " + dictionary.Count);
      TestHelper.CollectGarbage(true);
    }

    [Test]
    public void PerformanceTest()
    {
      const int count = 100000;
      var weakDictionary = new WeakDictionary<int, TestClass>();
      for (int i = 0; i < count; i++)
        list.Add(new Pair<int, TestClass>(i, new TestClass(i.ToString())));

      using(new Measurement("Inserting into Dictionary", count))
        foreach (var pair in list)
          dictionary.Add(pair.First, pair.Second);

      using (new Measurement("Inserting into WeakDictionary", count))
        foreach (var pair in list)
          weakDictionary.Add(pair.First, pair.Second);

      using (new Measurement("Searching through Dictionary", count*2)){
        foreach (var pair in list) {
          var exists = dictionary.ContainsKey(pair.First);
        }
        foreach (var pair in list) {
          var exists = dictionary.ContainsKey(pair.First);
        }
      }

      using (new Measurement("Searching through WeakDictionary", count*2)) {
        foreach (var pair in list) {
          var exists = weakDictionary.ContainsKey(pair.First);
        }
        foreach (var pair in list) {
          var exists = weakDictionary.ContainsKey(pair.First);
        }
      }

      Assert.AreEqual(count, weakDictionary.Count);
#if !DEBUG
    list.Clear();
    list = null;
    dictionary.Clear();
    dictionary = null;
    TestHelper.CollectGarbage(true);
    foreach (var pair in weakDictionary) {
      var value = pair.Value;
      Assert.IsNull(value);
    }
    Assert.AreEqual(0, weakDictionary.Count);
#endif

    }
  }
}