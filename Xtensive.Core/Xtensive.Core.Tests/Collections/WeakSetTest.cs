// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.07

using System;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Core.Testing;

namespace Xtensive.Core.Tests.Collections
{
  [TestFixture]
  public class WeakSetTest
  {
    private class TestClass : IEquatable<TestClass>
    {
      public readonly string Text;

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
    private Set<string> populateValues = new Set<string>();

    #region Weak-specific test
    [Test]
    public void WeakAddRemoveTest()
    {
      TestClass value1 = new TestClass("1");
      TestClass value2 = new TestClass("2");
      WeakSetSlim<TestClass> set = new WeakSetSlim<TestClass>();
      set.Add(value1);
      set.Add(value2);
      Assert.IsTrue(set.Contains(value1));
      Assert.IsTrue(set.Contains(value2));
      Assert.AreEqual(set.Count, 2);
      
      TestHelper.CollectGarbage(true);
      Assert.IsTrue(set.Contains(value1));
      Assert.IsTrue(set.Contains(value2));
      Assert.IsTrue(set.Contains(new TestClass("1")));
      Assert.IsTrue(set.Contains(new TestClass("2")));
      Assert.AreEqual(2, set.Count);
      
      value2 = null;
      TestHelper.CollectGarbage(true);
      Assert.IsTrue(set.Contains(value1));
      Assert.IsFalse(set.Contains(value2));
      Assert.IsTrue(set.Contains(new TestClass("1")));
      Assert.IsFalse(set.Contains(new TestClass("2")));
      set.Cleanup();
      Assert.AreEqual(1, set.Count);
    }
    #endregion

    #region Set-specific tests
    [Test]
    public void AddRemoveTest()
    {
      string[] values = new string[] {"0", "1"};
      ISet<string> set = new WeakSetSlim<string>();
      Assert.IsTrue(set.Add(values[0]));
      Assert.IsFalse(set.Add(values[0]));
      Assert.IsTrue(set.Contains(values[0]));
      Assert.AreEqual(set.Count, 1);
      Assert.AreEqual(set[values[0]], values[0]);
      Assert.IsTrue(set.Remove(values[0]));
      Assert.AreEqual(set.Count, 0);
    }

    [Test]
    public void ExceptTest()
    {
      string[] items1 = new string[] {"1", "2", "3", "4", "5"};
      string[] items2 = new string[] {"2", "3"};
      WeakSetSlim<string> set1 = new WeakSetSlim<string>(items1);
      WeakSetSlim<string> set2 = new WeakSetSlim<string>(items2);

      WeakSetSlim<string> set3 = set1.Except<string, WeakSetSlim<string>>(set2);
      Assert.AreEqual(3, set3.Count);
      set1.ExceptWith(set2);
      Assert.AreEqual(3, set1.Count);
    }

    [Test]
    public void IntersectTest()
    {
      string[] items1 = new string[] {"1", "2", "3", "4", "5"};
      string[] items2 = new string[] {"2", "3"};
      WeakSetSlim<string> set1 = new WeakSetSlim<string>(items1);
      WeakSetSlim<string> set2 = new WeakSetSlim<string>(items2);

      WeakSetSlim<string> set3 = set1.Intersect<string, WeakSetSlim<string>>(set2);
      Assert.AreEqual(2, set3.Count);
      set1.IntersectWith(set2);
      Assert.AreEqual(2, set1.Count);
    }

    [Test]
    public void SymmetricExceptTest()
    {
      string[] items1 = new string[] {"1", "2", "3", "4", "5"};
      string[] items2 = new string[] {"2", "3", "4", "5", "6"};
      WeakSetSlim<string> set1 = new WeakSetSlim<string>(items1);
      WeakSetSlim<string> set2 = new WeakSetSlim<string>(items2);

      WeakSetSlim<string> set3 = set1.SymmetricExcept<string, WeakSetSlim<string>>(set2);
      Assert.AreEqual(2, set3.Count);
      set1.SymmetricExceptWith(set2);
      Assert.AreEqual(2, set1.Count);
    }

    [Test]
    public void UnionTest()
    {
      string[] items1 = new string[] {"1", "2"};
      string[] items2 = new string[] {"2", "3"};
      WeakSetSlim<string> set1 = new WeakSetSlim<string>(items1);
      WeakSetSlim<string> set2 = new WeakSetSlim<string>(items2);

      ISet<string> set3 = set1.Union<string, WeakSetSlim<string>>(set2);
      Assert.AreEqual(3, set3.Count);
      set1.UnionWith(set2);
      Assert.AreEqual(3, set1.Count);
    }


    [Test]
    public void RemoveAt()
    {
      int count = 10;
      int removeCount = 5;
      Assert.LessOrEqual(removeCount, count);
      ISet<int> set = new Set<int>();
      for (int i = 0; i < count; i++)
        set.Add(i);
      Assert.AreEqual(count, set.Count);
      int removedCount = set.RemoveWhere(delegate(int match) { return match < removeCount; });
      Assert.AreEqual(removeCount, removedCount);
    }


    [Test]
    public void CombinedTest()
    {
      WeakSetSlim<string> A = new WeakSetSlim<string>();
      WeakSetSlim<string> B = new WeakSetSlim<string>();

      A.Add("a");
      A.Add("b");
      A.Add("c");
      A.Add("d");

      B.Add("c");
      B.Add("d");
      B.Add("e");
      B.Add("f");

      WeakSetSlim<string> C = A.Intersect<string, WeakSetSlim<string>>(B);
      WeakSetSlim<string> D = A.SymmetricExcept<string, WeakSetSlim<string>>(B);

      if (C.Count!=2) {
        throw new Exception("SetBase<T>.Intersection");
      }

      if (D.Count!=4) {
        throw new Exception("SetBase<T>.SymmetricDifference");
      }

      WeakSetSlim<string> AB = A.Union<string, WeakSetSlim<string>>(B);
      WeakSetSlim<string> CD = C.Union<string, WeakSetSlim<string>>(D);

      if (!AB.IsEqualTo(CD)) {
        throw new Exception("SetBase<T>.Union");
      }

      if (!C.IsDisjointWith(D)) {
        throw new Exception("SetBase<T>.IsDisjointWith");
      }

      if (!C.IsProperSubsetOf(A)) {
        throw new Exception("SetBase<T>.IsProperSubsetOf");
      }

      if (D.IsProperSubsetOf(A)) {
        throw new Exception("SetBase<T>.IsProperSubsetOf");
      }

      if (!A.IsProperSupersetOf(C)) {
        throw new Exception("SetBase<T>.IsProperSupersetOf");
      }

      if (A.IsProperSupersetOf(D)) {
        throw new Exception("SetBase<T>.IsProperSupersetOf");
      }

      if (A.IsProperSubsetOf(A) || A.IsProperSupersetOf(A)) {
        throw new Exception("SetBase<T>.IsProper...Of");
      }

      WeakSetSlim<string> A1 = A.Except<string, WeakSetSlim<string>>(B);
      WeakSetSlim<string> B1 = B.Except<string, WeakSetSlim<string>>(A);

      if (!D.IsEqualTo(A1.Union<string, WeakSetSlim<string>>(B1))) {
        throw new Exception("SetBase<T>.Difference");
      }
    }

    [Test]
    public void Add()
    {
      WeakSetSlim<string> A = new WeakSetSlim<string>();
      string[] strings = {"abc", "dfg", "ag", "abc"};
      A.UnionWith(strings);

      WeakSetSlim<string> C = new WeakSetSlim<string>(strings);

      string[] values = {"1", "2", "2", "3"};
      WeakSetSlim<string> B = new WeakSetSlim<string>();
      B.Add(values[0]);
      B.Add(values[1]);
      B.Add(values[2]);
      B.Add(values[3]);

      if (A.Count!=3 || B.Count!=3 || C.Count!=3) {
        throw new Exception("Set.AddRange");
      }
    }

    public void PopulateSet(WeakSetSlim<string> set, int count)
    {
      Guid g;
      for (int i = 0; i < count; i++) {
        g = Guid.NewGuid();
        string value = g.ToString();
        set.Add(value);
        populateValues.Add(value);
      }
    }

    [Test]
    public void Difference()
    {
      WeakSetSlim<string> setA = new WeakSetSlim<string>();
      PopulateSet(setA, 5);
      WeakSetSlim<string> setB = new WeakSetSlim<string>();
      PopulateSet(setB, 5);
      WeakSetSlim<string> setC = new WeakSetSlim<string>();
      PopulateSet(setC, 5);

      setA.UnionWith(setC);
      setB.UnionWith(setC);

      setA.ExceptWith(setC);
      if (setA.Count!=5) {
        throw new Exception("Set.ExceptWith");
      }

      setB = setB.Except<string, WeakSetSlim<string>>(setC);
      if (setB.Count!=5) {
        throw new Exception("Set.Difference");
      }
    }

    [Test]
    public void Intersection()
    {
      WeakSetSlim<string> setA = new WeakSetSlim<string>();
      PopulateSet(setA, 5);
      WeakSetSlim<string> setB = new WeakSetSlim<string>();
      PopulateSet(setB, 5);
      WeakSetSlim<string> setC = new WeakSetSlim<string>();
      PopulateSet(setC, 5);

      setA.UnionWith(setC);
      setB.UnionWith(setC);

      if (!setA.Intersect<string, WeakSetSlim<string>>(setB).IsEqualTo(setC)) {
        throw new Exception("Set.Intersection");
      }

      setA.IntersectWith(setB);
      if (!setA.IsEqualTo(setC)) {
        throw new Exception("Set.Intersection");
      }
    }

    [Test]
    public void DisjointFrom()
    {
      WeakSetSlim<string> setA = new WeakSetSlim<string>();
      PopulateSet(setA, 5);
      WeakSetSlim<string> setB = new WeakSetSlim<string>();
      PopulateSet(setB, 5);

      if (!setA.IsDisjointWith(setB)) {
        throw new Exception("Set.IsDisjointWith");
      }
    }

    [Test]
    public void ProperSubsetOf()
    {
      WeakSetSlim<string> setA = new WeakSetSlim<string>();
      PopulateSet(setA, 5);
      WeakSetSlim<string> setC = new WeakSetSlim<string>();
      PopulateSet(setC, 5);

      setA.UnionWith(setC);

      if (setA.IsProperSubsetOf(setA)) {
        throw new Exception("Set.IsProperSubsetOf");
      }

      if (!setC.IsProperSubsetOf(setA)) {
        throw new Exception("Set.IsProperSubsetOf");
      }
    }

    [Test]
    public void ProperSupersetOf()
    {
      WeakSetSlim<string> setA = new WeakSetSlim<string>();
      PopulateSet(setA, 5);
      WeakSetSlim<string> setC = new WeakSetSlim<string>();
      PopulateSet(setC, 5);

      setA.UnionWith(setC);

      if (!setA.IsProperSupersetOf(setC)) {
        throw new Exception("Set.IsProperSupersetOf");
      }

      if (setA.IsProperSupersetOf(setA)) {
        throw new Exception("Set.IsProperSupersetOf");
      }
    }

    [Test]
    public void SubsetOf()
    {
      WeakSetSlim<string> setA = new WeakSetSlim<string>();
      PopulateSet(setA, 5);
      WeakSetSlim<string> setC = new WeakSetSlim<string>();
      PopulateSet(setC, 5);

      setA.UnionWith(setC);

      if (!setA.IsSubsetOf(setA)) {
        throw new Exception("Set.IsSubsetOf");
      }

      if (!setC.IsSubsetOf(setA)) {
        throw new Exception("Set.IsSubsetOf");
      }
    }

    [Test]
    public void SupersetOf()
    {
      WeakSetSlim<string> setA = new WeakSetSlim<string>();
      PopulateSet(setA, 5);
      WeakSetSlim<string> setC = new WeakSetSlim<string>();
      PopulateSet(setC, 5);

      setA.UnionWith(setC);

      if (!setA.IsSupersetOf(setC)) {
        throw new Exception("Set.IsSupersetOf");
      }

      if (!setA.IsSupersetOf(setA)) {
        throw new Exception("Set.IsSupersetOf");
      }
    }

    [Test]
    public void SymmetricDifference()
    {
      WeakSetSlim<string> setA = new WeakSetSlim<string>();
      PopulateSet(setA, 5);
      WeakSetSlim<string> setB = new WeakSetSlim<string>();
      PopulateSet(setB, 5);
      WeakSetSlim<string> setC = new WeakSetSlim<string>();
      PopulateSet(setC, 5);

      setA.UnionWith(setC);
      setB.UnionWith(setC);

      if (!setA.SymmetricExcept<string, WeakSetSlim<string>>(setB).IsDisjointWith(setC)) {
        throw new Exception("Set.SymmetricDifference");
      }

      setA.SymmetricExceptWith(setB);

      if (!setA.IsDisjointWith(setC)) {
        throw new Exception("Set.SymmetricDifference");
      }
    }
    #endregion
  }
}