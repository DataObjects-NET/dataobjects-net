// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.01.30

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.TransactionsTestModel;

namespace Xtensive.Orm.Tests.Storage
{
  public class NonTransactionalEntityAccess : TransactionsTestBase
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      var defaultSession = configuration.Sessions.Default;
      defaultSession.Options = defaultSession.Options | SessionOptions.NonTransactionalReads;
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      int kwanza;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var h = new Hexagon();
        h.IncreaseKwanza();
        kwanza = h.Kwanza;
        tx.Complete();
      }

      using (var session = Domain.OpenSession()) {
        var hexagon = session.Query.All<Hexagon>().Single();
        Assert.That(hexagon.Kwanza, Is.EqualTo(kwanza));
      }
    }

    [Test, ExpectedException(typeof (InvalidOperationException))]
    public void InvalidOperationTest()
    {
      using (var session = Domain.OpenSession()) {
        new Hexagon();
      }
    }
  }
}