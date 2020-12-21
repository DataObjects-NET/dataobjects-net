// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => CollectionUtils.RangeToArray(1, -10));
    }

    [Test]
    public void RangeToListTest()
    {
      Assert.That(CollectionUtils.RangeToList(1, 10).SequenceEqual(Enumerable.Range(1, 10)));
      Assert.That(CollectionUtils.RangeToList(-1, 10).SequenceEqual(Enumerable.Range(-1, 10)));
      _ = Assert.Throws<ArgumentOutOfRangeException>(() => CollectionUtils.RangeToList(1, -10));
    }

    [Test]
    public void RepeatToArrayTest()
    {
      var array = CollectionUtils.RepeatToArray(1, 10);
      Assert.That(array.Length, Is.EqualTo(10));
      Assert.That(array.All(x => x == 1));
    }

    [Test]
    public void RepeatToListTest()
    {
      var list = CollectionUtils.RepeatToList(1, 10);
      Assert.That(list.Count, Is.EqualTo(10));
      Assert.That(list.All(x => x == 1));
    }
  }
}