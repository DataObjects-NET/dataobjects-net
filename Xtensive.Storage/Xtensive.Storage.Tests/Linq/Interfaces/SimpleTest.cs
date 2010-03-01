// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.01

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq.Interfaces
{
  [TestFixture, Category("Linq")]
  public sealed class SimpleTest : NorthwindDOModelTest
  {
    [Test]
    public void QueryTest()
    {
      var result = Query.All<IHasFreight>();
      var list = result.ToList();
      Assert.AreEqual(830, list.Count);
      Assert.IsTrue(list.All(i => i != null));
    }

    [Test]
    public void QueryByInterfaceTest()
    {
      var actual = Query.All<IHasFreight>().ToList();
      var expected = Query.All<Order>().ToList();
      Assert.AreEqual(0, expected.Except(actual.Cast<Order>()).Count());
    }
  }
}