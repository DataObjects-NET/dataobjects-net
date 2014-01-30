// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.01.30

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.TransactionsTestModel;

namespace Xtensive.Orm.Tests.Storage
{
  public class CrossTransactionEntityAccessTest : TransactionsTestBase
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration =  base.BuildConfiguration();
      var defaultSession = configuration.Sessions.Default;
      defaultSession.Options = defaultSession.Options | SessionOptions.PreserveStateOnCommit;
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        Hexagon hexagon;
        int kwanza;
        StateLifetimeToken token;
        using (var tx = session.OpenTransaction()) {
          hexagon = new Hexagon();
          hexagon.IncreaseKwanza();
          kwanza = hexagon.Kwanza;
          token = hexagon.State.LifetimeToken;
          tx.Complete();
        }

        using (var tx = session.OpenTransaction()) {
          AssertStateIsValid(hexagon);
          Assert.That(hexagon.Kwanza, Is.EqualTo(kwanza));
          Assert.That(hexagon.State.LifetimeToken, Is.EqualTo(token));
        }
      }
    }
  }
}