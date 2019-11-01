// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.01

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class PersistTest : AutoBuildTest
  {
    protected override Xtensive.Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Customer).Assembly, typeof (Customer).Namespace);
      return configuration;
    }

    [Test]
    public void RemoveCreateTest()
    {
      using (var session = Domain.OpenSession()) {
        Customer customer;
        using (var t = session.OpenTransaction()) {
          customer = new Customer(){FirstName = "Anton", LastName = "Chehov"};
          t.Complete();
        }
        using (var t = session.OpenTransaction()) {
          customer.Remove();
          new Customer();
          t.Complete();
        }
      }
    }

    [Test]
    public void UpdateRemoveTest()
    {
      using (var session = Domain.OpenSession()) {
        Customer customer;
        using (var t = session.OpenTransaction()) {
          customer = new Customer() {FirstName = "Ivan", LastName = "Ivanov"};
          t.Complete();
        }
        using (var t = session.OpenTransaction()) {
          customer.CompanyName = "Xtensive";
          customer.Remove();
          t.Complete();
        }
      }
    }

    [Test]
    public void CreateRemoveTest()
    {
      using (var session = Domain.OpenSession()) {
        Customer customer;
        using (var t = session.OpenTransaction()) {
          customer = new Customer() {FirstName = "Arthur", LastName = "Clarke" };
          customer.Remove();
          t.Complete();
        }
      }
    }
  }
}