// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.10

using NUnit.Framework;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public class ConnectionStringSupportTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      // fancy check of a SQL storage
      var checkerConfig = DomainConfigurationFactory.Create();
      var provider = checkerConfig.ConnectionInfo.Provider;
      StorageTestHelper.EnsureProviderIs(provider, StorageProvider.Sql);

      return DomainConfigurationFactory.Create(true);
    }

    [Test]
    public void CheckConfigTest()
    {
      var connectionInfo = Domain.Configuration.ConnectionInfo;
      Assert.IsNotNull(connectionInfo.Provider);
      Assert.IsNotNull(connectionInfo.ConnectionString);
    }
  }
}