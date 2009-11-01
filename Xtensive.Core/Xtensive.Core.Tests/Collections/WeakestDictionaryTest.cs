using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Core.Testing;

namespace Xtensive.Core.Tests.Collections
{

  [TestFixture]
  public class WeakestDictionaryTest
  {
    public class TestClass : IEquatable<TestClass>
    {
      public string Text;
      public TestClass(string text)
      {
        Text = text;
      }

      #region IEquatable<TestClass> Members

      ///<summary>
      ///Indicates whether the current object is equal to another object of the same type.
      ///</summary>
      ///
      ///<returns>
      ///true if the current object is equal to the other parameter; otherwise, false.
      ///</returns>
      ///
      ///<param name="other">An object to compare with this object.</param>
      public bool Equals(TestClass other)
      {
        if (other == null)
          return false;
        else
          return Equals(Text, other.Text);
      }

      #endregion

      public override int GetHashCode()
      {
        if (Text == null)
          return 0;
        else
          return Text.GetHashCode();
      }
    }

    [Test]
    public void RandomTest()
    {
      const int count = 1000;
      WeakestDictionary<TestClass, TestClass> dictionary =
        new WeakestDictionary<TestClass, TestClass>(count, EqualityComparer<TestClass>.Default);
      List<TestClass> references = new List<TestClass>();
      List<TestClass> tempReferences = new List<TestClass>();
      for (int i = 0; i < count; i++)
      {
        TestClass value = new TestClass(Guid.NewGuid().ToString());
        if (i % 2 == 0)
          references.Add(value);
        else
          tempReferences.Add(value);
        dictionary.Add(new TestClass(i.ToString()), value);
        value = null;
      }
      Assert.AreEqual(count, dictionary.Count);
      tempReferences = null;
      TestHelper.CollectGarbage(true);
      Assert.AreEqual(count, dictionary.Count);
      int enumeratorCount = 0;
      foreach (TestClass reference in references)
        enumeratorCount++;
      Assert.AreEqual(enumeratorCount, count / 2);
      dictionary.Cleanup();
      Assert.AreEqual(dictionary.Count, 0);
      Assert.AreEqual(dictionary.Keys.Count, 0);
      Assert.AreEqual(dictionary.Values.Count, 0);
    }

    [Test]
    public void MethodsAndPropertiesTest()
    {
      WeakestDictionary<TestClass, TestClass> dictionary = new WeakestDictionary<TestClass, TestClass>();
      Assert.AreEqual(dictionary.Count, 0);
      Assert.AreEqual(false, dictionary.ContainsKey(new TestClass("123")));
      TestClass value = new TestClass("test");
      dictionary.Add(new TestClass("1"), value);
      Assert.AreEqual(dictionary.Count, 1);
      Assert.AreEqual(true, dictionary.ContainsKey(new TestClass("1")));
      TestClass newValue = dictionary[new TestClass("1")];
      Assert.AreEqual(newValue.Text, value.Text);
      Assert.AreEqual(newValue, value);
      TestClass value2 = new TestClass("test2");
      TestClass key2 = new TestClass("2");
      dictionary.Add(key2, value2);
      Assert.AreEqual(dictionary.Count, 2);
      Assert.AreEqual(true, dictionary.ContainsKey(key2));

      newValue = value = null;
      TestHelper.CollectGarbage(true);
      dictionary.Cleanup();
      Assert.AreEqual(dictionary.Count, 1);
      Assert.AreEqual(true, dictionary.ContainsKey(key2));
      Assert.AreEqual(false, dictionary.ContainsKey(new TestClass("1")));
      
      value2 = null;
      TestHelper.CollectGarbage(true);
      TestClass result;
      Assert.IsFalse(dictionary.TryGetValue(new TestClass("2"), out result));
      TestClass newValue2 = new TestClass("New value 2");
      dictionary.Add(new TestClass("2"), newValue2);
      Assert.AreEqual(dictionary.Count, 1);
      Assert.AreEqual(true, dictionary.ContainsKey(new TestClass("2")));
      dictionary.Clear();
      Assert.AreEqual(dictionary.Count, 0);
      Assert.AreEqual(false, dictionary.ContainsKey(new TestClass("2")));
    }


    [Test]
    public void MultipleDictionariesTest()
    {
      WeakestDictionary<string, TestClass> dictionary = new WeakestDictionary<string, TestClass>();
      WeakestDictionary<TestClass, string> dictionary1 = new WeakestDictionary<TestClass, string>();
      TestClass test = new TestClass("TestString");
      dictionary.Add("TestString", test);
      dictionary1.Add(test, "TestString");
    }

    [Test]
    public void ReusableReferenceLoadTest()
    {
      int max = 600000;
      Dictionary<int, string> simpleHash = new Dictionary<int, string>();
      WeakestDictionary<string, string> dictionary = new WeakestDictionary<string, string>();

      DateTime startTime = DateTime.Now;
      for (int i = 0; i < max; i++) {
        simpleHash.Add(i, "test" + i);
      }

      int counter = 0;
      for (int i = 0; i < max; i++) {
        if (simpleHash[i] != null)
          counter++;
      }
      Log.Info("Cycle " + DateTime.Now.Subtract(startTime) + " counter:" + counter + " for simple hash");
      simpleHash = null;

      startTime = DateTime.Now;
      for (int i = 0; i < max; i++) {
        string testString = "test" + i;
        dictionary.Add(i.ToString(), testString);
      }
      Log.Info("Cycle set:" + DateTime.Now.Subtract(startTime));

      counter = 0;
      for (int i = 0; i < max; i++) {
        if (dictionary.ContainsKey(i.ToString())) {
          counter++;
        }
      }
      Log.Info("Cycle " + DateTime.Now.Subtract(startTime) + " counter:" + counter);
      Log.Info("Objects left: " + dictionary.Count);
      TestHelper.CollectGarbage(true);
    }

    private WeakestDictionary<string, TestClass> globalDictionary;
    private Random random = RandomManager.CreateRandom((int)DateTime.Now.Ticks);
    private static bool canFinish = true;
   
    private void AddItem()
    {
      int count = random.Next(100000);
      while (true) {
        try {
          globalDictionary.Add(count.ToString(), new TestClass("item " + count));
          count++;
        }
        catch (Exception ex) {
          if (canFinish)
            return;
          Assert.Fail(ex + ex.StackTrace);
        }
      }
    }

    private void RemoveItem()
    {
      while (true) {
        string test = null;
        foreach (KeyValuePair<string, TestClass> pair in globalDictionary) {
          test = pair.Key;
          break;
        }
        if (test != null)
          try {
          globalDictionary.Remove(test);
          }
          catch (Exception ex) {
            if (canFinish)
              return;
            Assert.Fail(ex + ex.StackTrace);
          }
      }
    }
  }
}
