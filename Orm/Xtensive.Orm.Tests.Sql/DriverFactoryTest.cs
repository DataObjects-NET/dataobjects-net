// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Sql;

namespace Xtensive.Orm.Tests.Sql
{
  [TestFixture]
  public class DriverFactoryTest
  {
    private string provider = TestConnectionInfoProvider.GetCurrentConnectionProvider();
    protected string Url = TestConnectionInfoProvider.GetCurrentConnectionUrl();
    protected string ConnectionString = TestConnectionInfoProvider.GetCurrentConnectionString();

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

    private static void TestProvider(string providerName, string connectionString, string connectionUrl)
    {
      Assert.IsNotNull(TestSqlDriver.Create(connectionUrl));
      Assert.IsNotNull(TestSqlDriver.Create(providerName, connectionString));
    }
  }
}