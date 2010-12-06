// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.12.03

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;
using System.Linq;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class ClientProfileTest : NorthwindDOModelTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Sessions.Default.Options = SessionOptions.ClientProfile | SessionOptions.AutoActivation;
      return config;
    }

    [Test]
    public void SaveChangesTest()
    {
      using (var session = Domain.OpenSession()) {
        var customer1 = new Customer("IDKFA") { CompanyName = "Id Software" };
        using (var t = session.OpenTransaction()) {
          var customer2 = new Customer("IDDQD") { CompanyName = "Id Software" };
          session.SaveChanges();
          var customer3 = new Customer("KILLS") { CompanyName = "Id Software" };
        }
        Assert.IsNull(session.Query.SingleOrDefault<Customer>("IDDQD"));
        Assert.IsNull(session.Query.SingleOrDefault<Customer>("KILLS"));
        Assert.IsNotNull(session.Query.SingleOrDefault<Customer>("IDKFA"));
        Assert.IsNull(session.Query.All<Customer>().SingleOrDefault(c => c.Id == "IDKFA"));
        Assert.AreEqual(0, session.Query.All<Customer>().Count(c => c.CompanyName == "Id Software"));

        using (var t = session.OpenTransaction()) {
          var customer2 = new Customer("IDDQD") { CompanyName = "Id Software" };
          session.SaveChanges();
          var customer3 = new Customer("KILLS") { CompanyName = "Id Software" };
          t.Complete();
        }
        Assert.IsNotNull(session.Query.SingleOrDefault<Customer>("IDDQD"));
        Assert.IsNotNull(session.Query.SingleOrDefault<Customer>("IDKFA"));
        Assert.IsNotNull(session.Query.SingleOrDefault<Customer>("KILLS"));
        Assert.IsNull(session.Query.All<Customer>().SingleOrDefault(c => c.Id == "KILLS"));
      }

      using (var session = Domain.OpenSession()) {
        session.Query.All<Customer>().Where(c => c.CompanyName == "Id Software").Remove();
        session.SaveChanges();
      }
    }
  }
}