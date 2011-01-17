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

namespace Xtensive.Tests.Collections
{
  [TestFixture]
  public class LinkedListTest
  {
    [Test]
    public void AddTest()
    {
      var list = new LinkedList<int>(0);
      list = list.AppendHead(1);
      list = list.AppendHead(2);
      list = list.AppendHead(3);
      Assert.AreEqual(4, list.Count);
      Assert.IsTrue(list.SequenceEqual(new []{3,2,1,0}));
    }

    [Test]
    public void SequenceTest()
    {
      var list = new LinkedList<int>(new[] {0, 1, 2, 3});
      Assert.AreEqual(4, list.Count);
      Assert.IsTrue(list.SequenceEqual(new[] {0, 1, 2, 3}));
    }
  }
}