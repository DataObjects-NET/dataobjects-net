// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.05.06

using DataObjects.NET;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using Xtensive.Storage.Linq;
using System.Linq;

namespace Xtensive.Storage.Tests.Linq
{
  [Ignore("Not implemented")]
  [TestFixture, Category("Linq")]
  public class ExpandTest: NorthwindDOModelTest
  {
    [Test]
    public void EntitySimpleSetTest()
    {
      var result = Query<Customer>.All.Prefetch(c=>c.Orders);
      QueryDumper.Dump(result);
    }

    [Test]
    public void EntitySimpleTest()
    {
      var result = Query<Order>.All.Prefetch(o=>o.ShipVia);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubquerySimpleTest()
    {
      var result = Query<Order>.All.Prefetch(o=>Query<Shipper>.All.Where(s=>s==o.ShipVia));
      QueryDumper.Dump(result);
    }
  }
}