// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.06.08

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class Issue0689_WeirdExceptionWhenMissingConnectionInfo
  {
    [Test]
    public void MissingConnectionInfoInCodeTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.ConnectionInfo = null;
      AssertEx.Throws<ArgumentException>(
        () => Domain.Build(configuration));
    }

    [Test]
    public void MissingConnectionInfoInAppConfigTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      // Default behavior was changed
      var config = DomainConfiguration.Load("AppConfigTest", "DomainWithWrongConnectionInfo");
      Assert.That(config.ConnectionInfo.Provider, Is.EqualTo(WellKnown.Provider.SqlServer));
    }
  }
}