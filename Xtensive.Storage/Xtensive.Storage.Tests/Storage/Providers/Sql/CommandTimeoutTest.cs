// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.06.10

using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Services;

namespace Xtensive.Storage.Tests.Storage.Providers.Sql
{
  [TestFixture]
  public class CommandTimeoutTest
  {
    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Require.ProviderIs(StorageProvider.Sql);
    }

    [Test]
    public void ConfigTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      var domainConfiguration = DomainConfiguration.Load(
        "AppConfigTest", "DomainWithCustomCommandTimeout");
      using (var domain = Domain.Build(domainConfiguration)) {
        using (var session = Session.Open(domain)) {
          Assert.AreEqual(100, session.CommandTimeout);
        }
        var sessionConfiguration = new SessionConfiguration {DefaultCommandTimeout = 100};
        using (var session = Session.Open(domain, sessionConfiguration)) {
          Assert.AreEqual(100, session.CommandTimeout);
        }
      }
    }

    [Test]
    public void OverrideTest()
    {
      using (var domain = Domain.Build(DomainConfigurationFactory.Create())) {
        using (var session = Session.Open(domain)) {
          var oldValue = session.CommandTimeout;
          const int newValue = 50;
          const int newValue2 = 100;
          session.CommandTimeout = newValue;
          var sqlAccessor = session.Services.Get<DirectSqlAccessor>();
          Assert.AreEqual(newValue, sqlAccessor.CreateCommand().CommandTimeout);
          using (session.OverrideCommandTimeout(newValue2))
            Assert.AreEqual(newValue2, sqlAccessor.CreateCommand().CommandTimeout);
        }
      }
    }
  }
}