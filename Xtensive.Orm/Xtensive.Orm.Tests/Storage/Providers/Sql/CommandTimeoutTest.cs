// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.06.10

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Services;

namespace Xtensive.Orm.Tests.Storage.Providers.Sql
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
      var systemSessionConfig = domainConfiguration.Sessions.System;
      if (systemSessionConfig == null) {
        systemSessionConfig = new SessionConfiguration(WellKnown.Sessions.System);
        domainConfiguration.Sessions.Add(systemSessionConfig);
      }
      systemSessionConfig.DefaultCommandTimeout = 6000; // 100 min.

      using (var domain = Domain.Build(domainConfiguration)) {
        using (var session = domain.OpenSession()) {
          Assert.AreEqual(100, session.CommandTimeout);
        }
        var sessionConfiguration = new SessionConfiguration {DefaultCommandTimeout = 100};
        using (var session = domain.OpenSession(sessionConfiguration)) {
          Assert.AreEqual(100, session.CommandTimeout);
        }
      }
    }

    [Test]
    public void OverrideTest()
    {
      using (var domain = Domain.Build(DomainConfigurationFactory.Create())) {
        using (var session = domain.OpenSession()) {
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