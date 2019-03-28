// Copyright (C) 2003-2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2019.03.21

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Collections;

namespace Xtensive.Orm.Tests.Core.Collections
{
  public class CollectionUtilsTest
  {
    [Test]
    public void RangeToArrayTest()
    {
      Assert.That(CollectionUtils.RangeToArray(1, 10).SequenceEqual(Enumerable.Range(1, 10)));
      Assert.That(CollectionUtils.RangeToArray(-1, 10).SequenceEqual(Enumerable.Range(-1, 10)));
      Assert.Throws<ArgumentOutOfRangeException>(() => CollectionUtils.RangeToArray(1, -10));
    }

    [Test]
    public void RangeToListTest()
    {
      Assert.That(CollectionUtils.RangeToList(1, 10).SequenceEqual(Enumerable.Range(1, 10)));
      Assert.That(CollectionUtils.RangeToList(-1, 10).SequenceEqual(Enumerable.Range(-1, 10)));
      Assert.Throws<ArgumentOutOfRangeException>(() => CollectionUtils.RangeToList(1, -10));
    }

    [Test]
    public void RepeatToArrayTest()
    {
      var iList = CollectionUtils.RepeatToArray(1, 10);
      Assert.That(iList.Length, Is.EqualTo(10));
      Assert.That(iList.All(x => x==1));
    }

    [Test]
    public void RepeatToListTest()
    {
      var iList = CollectionUtils.RepeatToList(1, 10);
      Assert.That(iList.Count, Is.EqualTo(10));
      Assert.That(iList.All(x => x==1));
    }
  }
}