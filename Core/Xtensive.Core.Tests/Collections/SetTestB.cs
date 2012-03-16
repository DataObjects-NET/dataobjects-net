// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Anton U. Rogozhin
// Created:    2007.06.29

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core.Collections;

namespace Xtensive.Core.Tests.Performance
{
  [TestFixture]
  public class SetTestB
  {
    /// <summary>
    /// Comparer that compares ints, sorting odds before evens.
    /// </summary>
    private class GOddEvenComparer: IComparer<int>
    {
      public override bool Equals(object obj)
      {
        return (obj is GOddEvenComparer);
      }

      public override int GetHashCode()
      {
        return 123569;
      }

      #region IComparer<int> Members

      public int Compare(int e1, int e2)
      {
        if ((e1 & 1) == 1 && (e2 & 1) == 0)
          return -1;
        else if ((e1 & 1) == 0 && (e2 & 1) == 1)
          return 1;
        else if (e1 < e2)
          return -1;
        else if (e1 > e2)
          return 1;
        else
          return 0;
      }

      #endregion
    }

    /// <summary>
    /// Comparer that compares ints, sorting odds before evens.
    /// </summary>
    private class GOddEvenEqualityComparer: IEqualityComparer<int>
    {
      public override bool Equals(object obj)
      {
        return (obj is GOddEvenComparer);
      }

      public override int GetHashCode()
      {
        return 123569;
      }

      #region IEqualityComparer<int> Members

      public bool Equals(int e1, int e2)
      {
        return ((e1 & 1) == (e2 & 1));
      }

      public int GetHashCode(int i)
      {
        return i;
      }

      #endregion
    }

    private class A
    {
      private string text;

      public string Text
      {
        get { return text; }
        set { text = value; }
      }
    }

    private class B : A
    {
      
    }

    [Test]
    public void CountAndClear()
    {
      Set<string> set1 = new Set<string>(StringComparer.InvariantCultureIgnoreCase);
      Assert.AreEqual(0, set1.Count);
      set1.Add("hello");
      Assert.AreEqual(1, set1.Count);
      set1.Add("foo");
      Assert.AreEqual(2, set1.Count);
      set1.Add("");
      Assert.AreEqual(3, set1.Count);
      set1.Add("HELLO");
      Assert.AreEqual(3, set1.Count);
      set1.Add("foo");
      Assert.AreEqual(3, set1.Count);
      set1.Add("Hello");
      Assert.AreEqual(3, set1.Count);
      set1.Add("Eric");
      Assert.AreEqual(4, set1.Count);
      set1.Clear();
      Assert.AreEqual(0, set1.Count);
      bool found = false;
      foreach (string s in set1)
        found = true;
      Assert.IsFalse(found);
    }

    [Test]
    public void Contains()
    {
      ISet<string> set1 = new Set<string>(StringComparer.InvariantCultureIgnoreCase);
      set1.Add("hello");
      Assert.IsTrue(set1.Contains("Hello"));
      set1.Remove("Hello");
      Assert.IsFalse(set1.Contains("hello"));
      set1.Add("");
      Assert.IsTrue(set1.Contains(""));
    }

    [Test]
    public void RandomAddDelete()
    {
      const int size = 50000;
      bool[] present = new bool[size];
      Random rand = new Random();
      ISet<int> set1 = new Set<int>();
      // Add and delete values at random.
      for (int i = 0; i < size*10; ++i) {
        int v = rand.Next(size);
        bool b;
        if (present[v]) {
          Assert.IsTrue(set1.Contains(v));
          b = set1.Remove(v);
          Assert.IsTrue(b);
          present[v] = false;
        }
        else {
          Assert.IsFalse(set1.Contains(v));
          b = set1.Add(v);
          Assert.IsTrue(b);
          present[v] = true;
        }
      }
      int count = 0;
      foreach (bool x in present)
        if (x)
          ++count;
      Assert.AreEqual(count, set1.Count);
      // Make sure the set has all the correct values, not in order.
      foreach (int v in set1) {
        Assert.IsTrue(present[v]);
        present[v] = false;
      }
      // Make sure all were found.
      count = 0;
      foreach (bool x in present)
        if (x)
          ++count;
      Assert.AreEqual(0, count);
    }

    [Test]
    public void GenericICollectionInterface()
    {
      string[] s_array = {"Foo", "Eric", "Clapton", "hello", "goodbye", "C#", "Java"};
      Set<string> set1 = new Set<string>();
      foreach (string s in s_array)
        set1.Add(s);
      Array.Sort(s_array);
      InterfaceTests.TestReadWriteCollectionGeneric(set1, s_array, false);
    }

    [Test]
    public void Add()
    {
      ISet<string> set1 = new Set<string>(StringComparer.InvariantCultureIgnoreCase);
      bool b;
      b = set1.Add("hello");
      Assert.IsTrue(b);
      b = set1.Add("foo");
      Assert.IsTrue(b);
      b = set1.Add("");
      Assert.IsTrue(b);
      b = set1.Add("HELLO");
      Assert.IsFalse(b);
      b = set1.Add("foo");
      Assert.IsFalse(b);
      b = set1.Add("Hello");
      Assert.IsFalse(b);
      b = set1.Add("Eric");
      Assert.IsTrue(b);
      InterfaceTests.TestCollectionGeneric(set1, new string[] {"", "Eric", "foo", "hello"}, false, null);
    }

    [Test]
    public void Remove()
    {
      ISet<string> set1 = new Set<string>(StringComparer.InvariantCultureIgnoreCase);
      bool b;
      b = set1.Remove("Eric");
      Assert.IsFalse(b);
      b = set1.Add("hello");
      Assert.IsTrue(b);
      b = set1.Add("foo");
      Assert.IsTrue(b);
      b = set1.Add("");
      Assert.IsTrue(b);
      b = set1.Remove("HELLO");
      Assert.IsTrue(b);
      b = set1.Remove("hello");
      Assert.IsFalse(b);
      b = set1.Add("Hello");
      Assert.IsTrue(b);
      b = set1.Add("Eric");
      Assert.IsTrue(b);
      b = set1.Remove(null);
      Assert.IsFalse(b);
      b = set1.Add("Eric");
      Assert.IsFalse(b);
      b = set1.Remove("eRic");
      Assert.IsTrue(b);
      b = set1.Remove("eRic");
      Assert.IsFalse(b);
      set1.Clear();
      b = set1.Remove("");
      Assert.IsFalse(b);
    }

    [Test]
    public void GetItem()
    {
      ISet<string> set1 = new Set<string>(StringComparer.InvariantCultureIgnoreCase);
      set1.Add("hello");
      set1.Add("foo");
      string s = set1["hello"];
      Assert.IsNotNull(s);
      Assert.AreEqual(s, "hello");
      s = set1["eric"];
      Assert.IsNull(s);
      set1.Remove("foo");
      s = set1["foo"];
      Assert.IsNull(s);
      set1.Add("");
      s = set1[""];
      Assert.IsNotNull(s);
      Assert.AreEqual(s, "");
    }

    [Test]
    public void Subset()
    {
      ISet<int> set1 = new Set<int>(new int[] {1, 3, 6, 7, 8, 9, 10});
      ISet<int> set2 = new Set<int>();
      ISet<int> set3 = new Set<int>(new int[] {3, 8, 9});
      ISet<int> set4 = new Set<int>(new int[] {3, 8, 9});
      ISet<int> set5 = new Set<int>(new int[] {1, 2, 6, 8, 9, 10});
      ISet<int> set6 = new Set<int>();
      int[] set7 = new int[] {3, 3, 8, 8};
      int[] set8 = new int[] {3, 3, 8, 8, 9, 9, 10};

      Assert.IsTrue(set1.IsSupersetOf(set2));
      Assert.IsTrue(set2.IsSubsetOf(set1));
      Assert.IsTrue(set1.IsProperSupersetOf(set2));
      Assert.IsTrue(set2.IsProperSubsetOf(set1));

      Assert.IsTrue(set1.IsSupersetOf(set3));
      Assert.IsTrue(set3.IsSubsetOf(set1));
      Assert.IsTrue(set1.IsProperSupersetOf(set3));
      Assert.IsTrue(set3.IsProperSubsetOf(set1));

      Assert.IsFalse(set3.IsSupersetOf(set1));
      Assert.IsFalse(set1.IsSubsetOf(set3));
      Assert.IsFalse(set3.IsProperSupersetOf(set1));
      Assert.IsFalse(set1.IsProperSubsetOf(set3));

      Assert.IsFalse(set1.IsSupersetOf(set5));
      Assert.IsFalse(set5.IsSupersetOf(set1));
      Assert.IsFalse(set1.IsSubsetOf(set5));
      Assert.IsFalse(set5.IsSubsetOf(set1));
      Assert.IsFalse(set1.IsProperSupersetOf(set5));
      Assert.IsFalse(set5.IsProperSupersetOf(set1));
      Assert.IsFalse(set1.IsProperSubsetOf(set5));
      Assert.IsFalse(set5.IsProperSubsetOf(set1));

      Assert.IsTrue(set3.IsSupersetOf(set4));
      Assert.IsTrue(set3.IsSubsetOf(set4));
      Assert.IsFalse(set3.IsProperSupersetOf(set4));
      Assert.IsFalse(set3.IsProperSubsetOf(set4));

      Assert.IsTrue(set1.IsSupersetOf(set1));
      Assert.IsTrue(set1.IsSubsetOf(set1));
      Assert.IsFalse(set1.IsProperSupersetOf(set1));
      Assert.IsFalse(set1.IsProperSubsetOf(set1));

      Assert.IsTrue(set2.IsSupersetOf(set6));
      Assert.IsTrue(set6.IsSubsetOf(set2));
      Assert.IsFalse(set2.IsProperSupersetOf(set6));
      Assert.IsFalse(set6.IsProperSubsetOf(set2));

      Assert.IsTrue(set3.IsSupersetOf(set7));
      Assert.IsTrue(set3.IsProperSupersetOf(set7));
      Assert.IsTrue(set3.IsSubsetOf(set8));
      Assert.IsTrue(set3.IsProperSubsetOf(set8));

    }

    [Test]
    public void AreEqual()
    {
      ISet<int> set1 = new Set<int>(new int[] {6, 7, 1, 11, 9, 3, 8});
      ISet<int> set2 = new Set<int>();
      ISet<int> set3 = new Set<int>();
      ISet<int> set4 = new Set<int>(new int[] {9, 11, 1, 3, 6, 7, 8, 14});
      ISet<int> set5 = new Set<int>(new int[] {3, 6, 7, 11, 14, 8, 9});
      ISet<int> set6 = new Set<int>(new int[] {1, 3, 6, 7, 8, 10, 11});
      ISet<int> set7 = new Set<int>(new int[] {9, 1, 8, 3, 7, 6, 11});
      int[] set8 = new int[] {6, 7, 1, 11, 9, 3, 8};
      int[] set9 = new int[] {6, 7, 1, 11, 9, 3, 8, 8, 8};
      Assert.IsTrue(set1.SetEquals(set1));
      Assert.IsTrue(set2.SetEquals(set2));
      Assert.IsTrue(set2.SetEquals(set3));
      Assert.IsTrue(set3.SetEquals(set2));
      Assert.IsTrue(set1.SetEquals(set7));
      Assert.IsTrue(set7.SetEquals(set1));
      Assert.IsFalse(set1.SetEquals(set2));
      Assert.IsFalse(set2.SetEquals(set1));
      Assert.IsFalse(set1.SetEquals(set4));
      Assert.IsFalse(set4.SetEquals(set1));
      Assert.IsFalse(set1.SetEquals(set5));
      Assert.IsFalse(set5.SetEquals(set1));
      Assert.IsFalse(set1.SetEquals(set6));
      Assert.IsFalse(set6.SetEquals(set1));
      Assert.IsFalse(set5.SetEquals(set6));
      Assert.IsFalse(set6.SetEquals(set5));
      Assert.IsFalse(set5.SetEquals(set7));
      Assert.IsFalse(set7.SetEquals(set5));
      Assert.IsTrue(set1.SetEquals(set8));
      Assert.IsTrue(set1.SetEquals(set9));
    }

    [Test]
    public void DisjointSets()
    {
      ISet<int> set1 = new Set<int>(new int[] {6, 7, 1, 11, 9, 3, 8});
      ISet<int> set2 = new Set<int>();
      ISet<int> set3 = new Set<int>();
      ISet<int> set4 = new Set<int>(new int[] {9, 1, 8, 3, 7, 6, 11});
      ISet<int> set5 = new Set<int>(new int[] {17, 3, 12, 10});
      ISet<int> set6 = new Set<int>(new int[] {19, 14, 0, 2});

      Assert.IsFalse(set1.IsDisjointWith(set1));
      Assert.IsTrue(set2.IsDisjointWith(set2));

      Assert.IsTrue(set1.IsDisjointWith(set2));
      Assert.IsTrue(set2.IsDisjointWith(set1));

      Assert.IsTrue(set2.IsDisjointWith(set3));
      Assert.IsTrue(set3.IsDisjointWith(set2));

      Assert.IsFalse(set1.IsDisjointWith(set4));
      Assert.IsFalse(set4.IsDisjointWith(set1));

      Assert.IsFalse(set1.IsDisjointWith(set5));
      Assert.IsFalse(set5.IsDisjointWith(set1));

      Assert.IsTrue(set1.IsDisjointWith(set6));
      Assert.IsTrue(set6.IsDisjointWith(set1));

      Assert.IsTrue(set5.IsDisjointWith(set6));
      Assert.IsTrue(set6.IsDisjointWith(set5));
    }

    [Test]
    public void Intersect()
    {
      Set<int> setOdds = new Set<int>(new int[] {1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25});
      Set<int> setDigits = new Set<int>(new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9});
      Set<int> set1, set2, set3;

      // Algorithms work different depending on sizes, so try both ways.
      set1 = new Set<int>(setOdds);
      set2 = new Set<int>(setDigits);
      set1.IntersectWith(set2);
      InterfaceTests.TestCollectionGeneric(set1, new int[] {1, 3, 5, 7, 9}, false, null);

      set1 = new Set<int>(setOdds);
      set2 = new Set<int>(setDigits);
      set2.IntersectWith(set1);
      InterfaceTests.TestCollectionGeneric(set2, new int[] {1, 3, 5, 7, 9}, false, null);

      set1 = new Set<int>(setOdds);
      set2 = new Set<int>(setDigits);
      set3 = Set<int>.Intersect(set1, set2);
      InterfaceTests.TestCollectionGeneric(set3, new int[] {1, 3, 5, 7, 9}, false, null);

      set1 = new Set<int>(setOdds);
      set2 = new Set<int>(setDigits);
      set3 = Set<int>.Intersect(set2, set1);
      InterfaceTests.TestCollectionGeneric(set3, new int[] {1, 3, 5, 7, 9}, false, null);

      // Make sure intersection with itself works.
      set1 = new Set<int>(setDigits);
      set1.IntersectWith(set1);
      InterfaceTests.TestCollectionGeneric(set1, new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9}, false, null);

      set1 = new Set<int>(setDigits);
      set3 = Set<int>.Intersect(set1, set1);
      InterfaceTests.TestCollectionGeneric(set3, new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9}, false, null);
    }

    [Test]
    public void Union()
    {
      Set<int> setOdds = new Set<int>(new int[] {1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25});
      Set<int> setDigits = new Set<int>(new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9});
      Set<int> set1, set2, set3;

      // Algorithms work different depending on sizes, so try both ways.
      set1 = new Set<int>(setOdds);
      set2 = new Set<int>(setDigits);
      set1.UnionWith(set2);
      InterfaceTests.TestCollectionGeneric(set1,
                                           new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 13, 15, 17, 19, 21, 23, 25},
                                           false,
                                           null);

      set1 = new Set<int>(setOdds);
      set2 = new Set<int>(setDigits);
      set2.UnionWith(set1);
      InterfaceTests.TestCollectionGeneric(set2,
                                           new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 13, 15, 17, 19, 21, 23, 25},
                                           false,
                                           null);

      set1 = new Set<int>(setOdds);
      set2 = new Set<int>(setDigits);
      set3 = Set<int>.Union(set1, set2);
      InterfaceTests.TestCollectionGeneric(set3,
                                           new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 13, 15, 17, 19, 21, 23, 25},
                                           false,
                                           null);

      set1 = new Set<int>(setOdds);
      set2 = new Set<int>(setDigits);
      set3 = Set<int>.Union(set2, set1);
      InterfaceTests.TestCollectionGeneric(set3,
                                           new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 13, 15, 17, 19, 21, 23, 25},
                                           false,
                                           null);

      // Make sure intersection with itself works.
      set1 = new Set<int>(setDigits);
      set1.UnionWith(set1);
      InterfaceTests.TestCollectionGeneric(set1, new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9}, false, null);

      set1 = new Set<int>(setDigits);
      set3 = Set<int>.Union(set1, set1);
      InterfaceTests.TestCollectionGeneric(set3, new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9}, false, null);
    }

    [Test]
    public void SymmetricExcept()
    {
      Set<int> setOdds = new Set<int>(new int[] {1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25});
      Set<int> setDigits = new Set<int>(new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9});
      Set<int> set1, set2, set3;

      // Algorithms work different depending on sizes, so try both ways.
      set1 = new Set<int>(setOdds);
      set2 = new Set<int>(setDigits);
      set1.SymmetricExceptWith(set2);
      InterfaceTests.TestCollectionGeneric(set1, new int[] {2, 4, 6, 8, 11, 13, 15, 17, 19, 21, 23, 25}, false, null);

      set1 = new Set<int>(setOdds);
      set2 = new Set<int>(setDigits);
      set2.SymmetricExceptWith(set1);
      InterfaceTests.TestCollectionGeneric(set2, new int[] {2, 4, 6, 8, 11, 13, 15, 17, 19, 21, 23, 25}, false, null);

      set1 = new Set<int>(setOdds);
      set2 = new Set<int>(setDigits);
      set3 = Set<int>.SymmetricExcept(set1, set2);
      InterfaceTests.TestCollectionGeneric(set3, new int[] {2, 4, 6, 8, 11, 13, 15, 17, 19, 21, 23, 25}, false, null);

      set1 = new Set<int>(setOdds);
      set2 = new Set<int>(setDigits);
      set3 = Set<int>.SymmetricExcept(set2, set1);
      InterfaceTests.TestCollectionGeneric(set3, new int[] {2, 4, 6, 8, 11, 13, 15, 17, 19, 21, 23, 25}, false, null);

      // Make sure intersection with itself works.
      set1 = new Set<int>(setDigits);
      set1.SymmetricExceptWith(set1);
      Assert.AreEqual(0, set1.Count);

      set1 = new Set<int>(setDigits);
      set3 = Set<int>.SymmetricExcept(set1, set1);
      Assert.AreEqual(0, set3.Count);
    }

    [Test]
    public void Except()
    {
      Set<int> setOdds = new Set<int>(new int[] {1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25});
      Set<int> setDigits = new Set<int>(new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9});
      Set<int> set1, set2, set3;

      // Algorithms work different depending on sizes, so try both ways.
      set1 = new Set<int>(setOdds);
      set2 = new Set<int>(setDigits);
      set1.ExceptWith(set2);
      InterfaceTests.TestCollectionGeneric(set1, new int[] {11, 13, 15, 17, 19, 21, 23, 25}, false, null);

      set1 = new Set<int>(setOdds);
      set2 = new Set<int>(setDigits);
      set2.ExceptWith(set1);
      InterfaceTests.TestCollectionGeneric(set2, new int[] {2, 4, 6, 8}, false, null);

      set1 = new Set<int>(setOdds);
      set2 = new Set<int>(setDigits);
      set3 = Set<int>.Except(set1, set2);
      InterfaceTests.TestCollectionGeneric(set3, new int[] {11, 13, 15, 17, 19, 21, 23, 25}, false, null);

      set1 = new Set<int>(setOdds);
      set2 = new Set<int>(setDigits);
      set3 = Set<int>.Except(set2, set1);
      InterfaceTests.TestCollectionGeneric(set3, new int[] {2, 4, 6, 8}, false, null);

      // Make sure intersection with itself works.
      set1 = new Set<int>(setDigits);
      set1.ExceptWith(set1);
      Assert.AreEqual(0, set1.Count);

      set1 = new Set<int>(setDigits);
      set3 = Set<int>.Except(set1, set1);
      Assert.AreEqual(0, set3.Count);
    }

    [Test]
    public void ConsistentComparisons()
    {
      Set<string> set1 = new Set<string>(new string[] {"foo", "Bar"}, StringComparer.InvariantCulture);
      Set<string> set2 = new Set<string>(new string[] {"bada", "bing"}, StringComparer.InvariantCulture);
      set1.ExceptWith(set2);
    }

    [Test, ExpectedException(typeof (InvalidOperationException))]
    public void InconsistentComparisons1()
    {
      Set<string> set1 = new Set<string>(new string[] {"foo", "Bar"}, StringComparer.CurrentCulture);
      Set<string> set2 = new Set<string>(new string[] {"bada", "bing"}, StringComparer.InvariantCulture);
      set1.IntersectWith(set2);
    }

    [Test, ExpectedException(typeof (InvalidOperationException))]
    public void InconsistentComparisons2()
    {
      Set<int> setOdds = new Set<int>(new int[] {1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25});
      Set<int> setDigits = new Set<int>(new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9}, new GOddEvenEqualityComparer());
      setOdds.SymmetricExceptWith(setDigits);
    }
  }
}