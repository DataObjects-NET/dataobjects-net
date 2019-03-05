// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2018.10.30

using System;
using NUnit.Framework;
using Xtensive.Orm.Building.Builders;
using Xtensive.Sql;

namespace Xtensive.Orm.Tests.Sql.SqlServer
{
  [TestFixture]
  public class ConnectionTest
  {
    private string provider = TestConnectionInfoProvider.GetProvider();
    protected string Url = TestConnectionInfoProvider.GetConnectionUrl();
    protected string ConnectionString = TestConnectionInfoProvider.GetConnectionString();

    [OneTimeSetUp]
    public void SetUp()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
    }


    [Test]
    [Explicit("Manual sql profiling needed")]
    public void ConntectionTest()
    {
      var descriptor = ProviderDescriptor.Get(provider);
      var factory = (SqlDriverFactory) Activator.CreateInstance(descriptor.DriverFactory);

      var configuration = new SqlDriverConfiguration {EnsureConnectionIsAlive = true};
      var driver = factory.GetDriver(new ConnectionInfo(Url), configuration);

      var connection = driver.CreateConnection();
      connection.Open();

      var anotherConnection = driver.CreateConnection();
      anotherConnection.OpenAndInitialize("Use [DO-Tests-1]");
    }
  }
}
