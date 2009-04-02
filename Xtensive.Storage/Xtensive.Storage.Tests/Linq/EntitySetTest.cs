// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.02

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  public class EntitySetTest : NorthwindDOModelTest
  {
    [Test]
    public void QueryTest()
    {
      var customer = Query<Customer>.All.Where(c => c.Id=="LACOR").Single();
      var result = customer.Orders.Where(o => o.Freight > -1);
      Assert.AreEqual(4, result.ToList().Count);
    }

    [Test]
    public void UnsupportedMethodsTest()
    {
      AssertEx.ThrowsNotSupportedException(() => Query<Customer>.All.Where(c => c.Orders.Add(null)).ToList());
      AssertEx.ThrowsNotSupportedException(() => Query<Customer>.All.Where(c => c.Orders.Remove(null)).ToList());
    }

    [Test]
    public void SandBoxTest()
    {
      var customer = Query<Customer>.All.Where(c => c.Id == "LACOR").Single();
      var expression = customer.Orders.Expression;
      var now = DateTime.Now;
    }
  }
}
