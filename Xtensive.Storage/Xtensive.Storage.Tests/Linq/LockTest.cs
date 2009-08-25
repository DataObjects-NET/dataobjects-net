// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.08.25

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using Xtensive.Storage.Linq;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  public class LockTest : NorthwindDOModelTest
  {
    [Test]
    public void SimpleTest()
    {
      var actual = Query<Customer>.All.AsEnumerable();
      var expected = Query<Customer>.All.Lock(LockMode.Exclusive, LockBehavior.ThrowIfLocked);
      Assert.IsTrue(actual.SequenceEqual(expected));
      QueryDumper.Dump(expected, true);
    }
  }
}