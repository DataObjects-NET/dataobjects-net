// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.23

using System;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;

namespace Xtensive.Orm.Tests.Core.Collections
{
  [TestFixture]
  public class SetTests
  {
    [Test]
    public void AddRemoveTest()
    {
      ISet<int> set = new SetSlim<int>();
      Assert.IsTrue(set.Add(0));
      Assert.IsFalse(set.Add(0));
      Assert.IsTrue(set.Contains(0));
      Assert.AreEqual(set.Count, 1);
      Assert.AreEqual(set[0], 0);
      Assert.IsTrue(set.Remove(0));
      Assert.AreEqual(set.Count, 0);
    }

    [Test]
    public void ExceptTest()
    {
      SetSlim<int> set1 = new SetSlim<int>(new int[] {1, 2, 3, 4, 5});
      SetSlim<int> set2 = new SetSlim<int>(new int[] {2, 3});

      SetSlim<int> set3 = set1.Except<int, SetSlim<int>>(set2);
      Assert.AreEqual(3, set3.Count);
      set1.ExceptWith(set2);
      Assert.AreEqual(3, set1.Count);
    }

    [Test]
    public void IntersectTest()
    {
      SetSlim<int> set1 = new SetSlim<int>(new int[] {1, 2, 3, 4});
      SetSlim<int> set2 = new SetSlim<int>(new int[] {2, 3});

      SetSlim<int> set3 = set1.Intersect<int, SetSlim<int>>(set2);
      Assert.AreEqual(2, set3.Count);
      set1.IntersectWith(set2);
      Assert.AreEqual(2, set1.Count);
    }

    [Test]
    public void SymmetricExceptTest()
    {
      SetSlim<int> set1 = new SetSlim<int>(new int[] {1, 2, 3, 4, 5});
      SetSlim<int> set2 = new SetSlim<int>(new int[] {2, 3, 4, 5, 6});

      SetSlim<int> set3 = set1.SymmetricExcept<int, SetSlim<int>>(set2);
      Assert.AreEqual(2, set3.Count);
      set1.SymmetricExceptWith(set2);
      Assert.AreEqual(2, set1.Count);
    }

    [Test]
    public void UnionTest()
    {
      SetSlim<int> set1 = new SetSlim<int>(new int[] {1, 2});
      SetSlim<int> set2 = new SetSlim<int>(new int[] {2, 3});

      SetSlim<int> set3 = set1.Union<int, SetSlim<int>>(set2);
      Assert.AreEqual(3, set3.Count);
      set1.UnionWith(set2);
      Assert.AreEqual(3, set1.Count);
    }
  }
}