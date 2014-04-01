// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.03

using NUnit.Framework;
using Xtensive.Orm.Metadata;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class OverrideConnectionInfoTest : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.ConnectionInfo = GetAlternativeConnectionInfo();
        var typeCount = session.Query.All<Type>().Count();
        Assert.That(typeCount, Is.GreaterThan(0));
        tx.Complete();
      }
    }

    private ConnectionInfo GetAlternativeConnectionInfo()
    {
      return DomainConfigurationFactory.CreateForConnectionStringTest().ConnectionInfo;
    }
  }
}