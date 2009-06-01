// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.01

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  public sealed class InterfaceTest : NorthwindDOModelTest
  {
    [Test]
    public void QueryByInterfaceTest()
    {
      var actual = Query<IHasShippingAddress>.All.ToList();
      var expected = Query<Order>.All.ToList();
      Assert.AreEqual(0, expected.Except(actual.Cast<Order>()).Count());
    }
  }
}