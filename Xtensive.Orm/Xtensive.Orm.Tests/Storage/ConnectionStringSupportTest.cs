// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.10

using NUnit.Framework;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class ConnectionStringSupportTest : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Sql);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
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