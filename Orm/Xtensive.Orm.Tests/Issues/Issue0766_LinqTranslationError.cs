// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.07.23

using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Linq;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Issues
{
  [Serializable]
  public class Issue0766_LinqTranslationError : ChinookDOModelTest
  {
    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
    }

    [Test]
    public void MainTest()
    {
      var result1 =
        from c in Session.Query.All<Customer>()
        from i in Session.Query.All<Invoice>().Where(i => i.Customer==c).Take(1).DefaultIfEmpty()
        where i!=null
        select
          new {
            ID = c.CustomerId,
            Name = c.FirstName,
            InvoiceID = i.InvoiceId
          };
      var list1 = result1.ToList();

      var result2 =
        from c in Session.Query.All<Customer>()
        from i in Session.Query.All<Invoice>().Where(i => i.Customer==c).DefaultIfEmpty()
        where i!=null
        select
          new {
            ID = c.CustomerId,
            Name = c.FirstName,
            InvoiceID = i.InvoiceId
          };
      var list2 = result2.ToList();

      var count = result1.Count();
      Assert.AreEqual(list1.Count, count);
      Assert.Greater(list2.Count, count);
    }
  }
}