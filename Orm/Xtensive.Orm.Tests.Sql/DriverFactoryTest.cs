// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Orm.Building.Builders;
using Xtensive.Sql;

namespace Xtensive.Orm.Tests.Sql
{
  [TestFixture]
  public class DriverFactoryTest
  {
    private string provider = TestConnectionInfoProvider.GetProvider();
    protected string Url = TestConnectionInfoProvider.GetConnectionUrl();
    protected string ConnectionString = TestConnectionInfoProvider.GetConnectionString();

    [Test]
    public void ConnectionUrlTest()
    {
      var url = UrlInfo.Parse("sqlserver://appserver/AdventureWorks?Connection Timeout=5");
      Assert.AreEqual(url.Protocol, "sqlserver");
      Assert.AreEqual(url.Host, "appserver");
      Assert.AreEqual(url.Resource, "AdventureWorks");

      url = UrlInfo.Parse("sqlserver://localhost/database");
      Assert.AreEqual("database", url.GetDatabase());
      Assert.AreEqual("default schema", url.GetSchema("default schema"));

      url = UrlInfo.Parse("sqlserver://localhost/database/");
      Assert.AreEqual("database", url.GetDatabase());
      Assert.AreEqual("default schema", url.GetSchema("default schema"));

      url = UrlInfo.Parse("sqlserver://localhost/database/schema");
      Assert.AreEqual("database", url.GetDatabase());
      Assert.AreEqual("schema", url.GetSchema(string.Empty));

      url = UrlInfo.Parse("sqlserver://localhost/database/schema/");
      Assert.AreEqual("database", url.GetDatabase());
      Assert.AreEqual("schema", url.GetSchema(string.Empty));
    }

    [Test]
    public void ServerInfoTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      Require.ProviderVersionAtLeast(StorageProviderVersion.SqlServer2005);
      var driver = TestSqlDriver.Create(Url);
      Assert.Greater(driver.CoreServerInfo.ServerVersion.Major, 8);
    }

    [Test]
    public void ProviderTest()
    {
      TestProvider(provider, ConnectionString, Url);
    }

    [Test]
    [Explicit("Manual debugging needed. No way to make it automated test yet")]
    public void SqlServerConnectionCheckTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      var descriptor = ProviderDescriptor.Get(provider);
      var factory = (SqlDriverFactory) Activator.CreateInstance(descriptor.DriverFactory);

      var configuration = new SqlDriverConfiguration() {EnsureConnectionIsAlive = false};
      factory.GetDriver(new ConnectionInfo(Url), configuration);
      Assert.That(configuration.EnsureConnectionIsAlive, Is.False);

      configuration = configuration.Clone();
      configuration.EnsureConnectionIsAlive = true;
      factory.GetDriver(new ConnectionInfo(Url), configuration);
      Assert.That(configuration.EnsureConnectionIsAlive, Is.True);

      configuration = configuration.Clone();
      configuration.EnsureConnectionIsAlive = true;
      factory.GetDriver(new ConnectionInfo(provider, ConnectionString + ";pooling=false"), configuration);
      Assert.That(configuration.EnsureConnectionIsAlive, Is.False);

      configuration = configuration.Clone();
      configuration.EnsureConnectionIsAlive = true;
      factory.GetDriver(new ConnectionInfo(provider, ConnectionString + ";Pooling=False"), configuration);
      Assert.That(configuration.EnsureConnectionIsAlive, Is.False);

      configuration = configuration.Clone();
      configuration.EnsureConnectionIsAlive = true;
      factory.GetDriver(new ConnectionInfo(provider, ConnectionString + ";pooling = false"), configuration);
      Assert.That(configuration.EnsureConnectionIsAlive, Is.False);

      configuration = configuration.Clone();
      configuration.EnsureConnectionIsAlive = true;
      factory.GetDriver(new ConnectionInfo(provider, ConnectionString + ";Pooling = False"), configuration);
      Assert.That(configuration.EnsureConnectionIsAlive, Is.False);
    }

    private static void TestProvider(string providerName, string connectionString, string connectionUrl)
    {
      Assert.IsNotNull(TestSqlDriver.Create(connectionUrl));
      Assert.IsNotNull(TestSqlDriver.Create(providerName, connectionString));
    }
  }
}