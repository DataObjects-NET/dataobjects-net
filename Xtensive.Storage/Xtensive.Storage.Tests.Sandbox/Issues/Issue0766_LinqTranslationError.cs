// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.07.23

using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Linq;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Issues
{
  [Serializable]
  public class Issue0766_LinqTranslationError : NorthwindDOModelTest
  {
    [Test]
    public void MainTest()
    {
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var result1 = 
          from c in Query.All<Customer>()
          from o in Query.All<Order>().Where(o => o.Customer == c).Take(1).DefaultIfEmpty()
          where o != null
          select
            new {
              ID = c.Id,
              Name = c.ContactName,
              OrderID = o.Id
            };
        var list1 = result1.ToList();

        var result2 =
          from c in Query.All<Customer>()
          from o in Query.All<Order>().Where(o => o.Customer == c).DefaultIfEmpty()
          where o != null
          select
            new
            {
              ID = c.Id,
              Name = c.ContactName,
              OrderID = o.Id
            };
        var list2 = result2.ToList();

        var count = result1.Count();
        Assert.AreEqual(list1.Count, count);
        Assert.Greater(list2.Count, count);

        t.Complete();
      }
    }
  }
}