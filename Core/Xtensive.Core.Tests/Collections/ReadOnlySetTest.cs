// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using System;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;

namespace Xtensive.Tests.Collections
{
  [TestFixture]
  public class ReadOnlySetTest
  {
    private ISet<string> set;
    private ReadOnlySet<string> readOnlySet;

    [TestFixtureSetUp]
    public void Init()
    {
      set = new SetSlim<string>();
      readOnlySet = new ReadOnlySet<string>(set);
    }

    [Test]
    [ExpectedException(typeof (NotSupportedException))]
    public void Test_Add()
    {
      int count = readOnlySet.Count;
      try {
        readOnlySet.Add("Element");
      }
      finally {
        Assert.AreEqual(count, readOnlySet.Count);
      }
    }

    [Test]
    [ExpectedException(typeof (NotSupportedException))]
    public void Test_Remove()
    {
      set.Add("Element");
      int count = readOnlySet.Count;
      try {
        readOnlySet.Remove("Element");
      }
      finally {
        Assert.AreEqual(count, readOnlySet.Count);
      }
    }

    [Test]
    [ExpectedException(typeof (NotSupportedException))]
    public void Test_IntersectionWith()
    {
      set.Add("Element");
      int count = readOnlySet.Count;
      try {
        readOnlySet.IntersectWith(new SetSlim<string>());
      }
      finally {
        Assert.AreEqual(count, readOnlySet.Count);
      }
    }

    [Test]
    [ExpectedException(typeof (NotSupportedException))]
    public void Test_UnionWith()
    {
      set.Add("Element");
      int count = readOnlySet.Count;
      try {
        SetSlim<string> newSet = new SetSlim<string>();
        newSet.Add("Element 2");
        readOnlySet.UnionWith(newSet);
      }
      finally {
        Assert.AreEqual(count, readOnlySet.Count);
      }
    }

    [Test]
    [ExpectedException(typeof (NotSupportedException))]
    public void Test_DifferenceWith()
    {
      set.Add("Element");
      int count = readOnlySet.Count;
      try {
        SetSlim<string> newSet = new SetSlim<string>();
        newSet.Add("Element");
        readOnlySet.ExceptWith(newSet);
      }
      finally {
        Assert.AreEqual(count, readOnlySet.Count);
      }
    }

    [Test]
    [ExpectedException(typeof (NotSupportedException))]
    public void Test_SymmetricDifferenceWith()
    {
      set.Add("Element");
      int count = readOnlySet.Count;
      try {
        SetSlim<string> newSet = new SetSlim<string>();
        newSet.Add("Element 2");
        readOnlySet.SymmetricExceptWith(newSet);
      }
      finally {
        Assert.AreEqual(count, readOnlySet.Count);
      }
    }

    [Test]
    public void CopyToTest()
    {
      set.Clear();
      set.Add("Element");
      set.Add("Element");
      set.Add("Element 2");
      string[] array = new string[3];
      readOnlySet.CopyTo(array, 1);
      Assert.IsTrue(array[0]==null);
      if (array[1]=="Element")
        Assert.IsTrue(array[2]=="Element 2");
      else
        Assert.IsTrue(array[2]=="Element");
    }

    [Test]
    public void SerializationTest()
    {
      var deserialized = Cloner.Clone(readOnlySet);
      Assert.AreEqual(deserialized.Count, readOnlySet.Count);
      foreach (string s in readOnlySet)
        Assert.IsTrue(deserialized.Contains(s));
      foreach (string s in deserialized)
        Assert.IsTrue(readOnlySet.Contains(s));
    }
  }
}