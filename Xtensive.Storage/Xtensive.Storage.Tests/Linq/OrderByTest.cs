// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.01.29

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  public class OrderByTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Supplier).Assembly, typeof(Supplier).Namespace);
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      Domain domain = base.BuildDomain(configuration);
      DataBaseFiller.Fill(domain);
      return domain;
    }

    [Test]
    public void MainTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var contacts = Enumerable.OrderBy(Session.Current.All<Customer>(), s => s.ContactName);
          foreach (var item in contacts)
            Console.WriteLine(item.ContactName);
          foreach (var item in contacts.OrderByDescending(s => s.ContactName))
            Console.WriteLine(item.ContactName);
        }
      }
    }
  }
}