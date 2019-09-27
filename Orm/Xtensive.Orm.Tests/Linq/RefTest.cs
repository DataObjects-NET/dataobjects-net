// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.16

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;
using System.Linq;

namespace Xtensive.Orm.Tests.Linq
{
  [TestFixture]
  [Ignore("")]
  public class RefTest : ChinookDOModelTest
  {
    private class X
    {
      public Ref<Invoice> InvoiceRef;
      public int SomeInt;
    }

    [Test]
    public void GetEntityTest()
    {
      var xs = Session.Query.All<Invoice>().OrderBy(i => i.InvoiceId).Take(10).Select((invoice, index) =>
        new X {
          InvoiceRef = (Ref<Invoice>) invoice.Key,
          SomeInt = index
        })
        .ToList();

      var query =
        from i in Session.Query.All<Invoice>()
        from x in Session.Query.Store(xs)
        where i==x.InvoiceRef.Value
        select i;

      var expected = 
        from i in Session.Query.All<Invoice>().AsEnumerable()
        from x in xs
        where i==x.InvoiceRef.Value
        select i;

      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0 , expected.Except(query).Count());
    }

    [Test]
    public void GetEntity2Test()
    {
      var xs = Session.Query.All<Invoice>().OrderBy(i => i.InvoiceId).Take(10).Select((invoice, index) =>
        new X {
          InvoiceRef = (Ref<Invoice>) invoice.Key,
          SomeInt = index
        })
        .ToList();

      var query =
        from i in Session.Query.All<Invoice>()
        from x in xs
        where i==x.InvoiceRef.Value
        select i;

      var expected = 
        from i in Session.Query.All<Invoice>().AsEnumerable()
        from x in xs
        where i==x.InvoiceRef.Value
        select i;

      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0 , expected.Except(query).Count());
    }

    [Test]
    public void KeyTest()
    {
      var refs = Session.Query.All<Invoice>().Take(10).Select(invoice => (Ref<Invoice>) invoice).ToList();
      var query = Session.Query.All<Invoice>()
        .Join(refs, invoice => invoice.Key, @ref => @ref.Key, (invoice, key) => new {invoice, key});

      Assert.That(query, Is.Not.Empty);
      QueryDumper.Dump(query);

      var expectedQuery = Session.Query.All<Invoice>().AsEnumerable()
        .Join(refs, invoice => invoice.Key, @ref => @ref.Key, (invoice, key) => new {invoice, key});

      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }
  }
}