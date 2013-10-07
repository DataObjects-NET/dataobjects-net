// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.01.17

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Collections;
using System.Linq;

namespace Xtensive.Orm.Tests.Core.Collections
{
  [TestFixture]
  public class LinkedListTest
  {
    [Test]
    public void AddTest()
    {
      var list = new SinglyLinkedList<int>(0);
      list = list.Add(1);
      list = list.Add(2);
      list = list.Add(3);
      Assert.AreEqual(4, list.Count);
      Assert.IsTrue(list.SequenceEqual(new []{3,2,1,0}));
    }

    [Test]
    public void SequenceTest()
    {
      var list = new SinglyLinkedList<int>(new[] {0, 1, 2, 3});
      Assert.AreEqual(4, list.Count);
      Assert.IsTrue(list.SequenceEqual(new[] {0, 1, 2, 3}));
    }
  }
}