// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Orm.Building.Builders;
using Xtensive.Sql;
using Xtensive.Orm.Tests.Sql.DriverFactoryTestTypes;

namespace Xtensive.Orm.Tests.Sql.DriverFactoryTestTypes
{
  public class TestConnectionHandler : IConnectionHandler
  {
    public int OpeningCounter = 0;
    public int OpenedCounter = 0;
    public int OpeningInitCounter = 0;
    public int OpeningFailedCounter = 0;

    public void ConnectionOpening(ConnectionEventData eventData)
    {
      OpeningCounter++;
    }

    public void ConnectionOpened(ConnectionEventData eventData)
    {
      OpenedCounter++;
    }

    public void ConnectionInitialization(ConnectionInitEventData eventData)
    {
      OpeningInitCounter++;
    }

    public void ConnectionOpeningFailed(ConnectionErrorEventData eventData)
    {
      OpeningFailedCounter++;
    }
  }

  public static class StaticCounter
  {
    public static int OpeningReached;
    public static int OpenedReached;
  }
}

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
    public void SqlServerConnectionCheckTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      var descriptor = ProviderDescriptor.Get(provider);
      var factory = (SqlDriverFactory) Activator.CreateInstance(descriptor.DriverFactory);

      var configuration = new SqlDriverConfiguration() { EnsureConnectionIsAlive = false };
      var driver = factory.GetDriver(new ConnectionInfo(Url), configuration);
      Assert.That(GetCheckConnectionIsAliveFlag(driver), Is.False);

      configuration = configuration.Clone();
      configuration.EnsureConnectionIsAlive = true;
      driver = factory.GetDriver(new ConnectionInfo(Url), configuration);
      Assert.That(GetCheckConnectionIsAliveFlag(driver), Is.True);

      configuration = configuration.Clone();
      configuration.EnsureConnectionIsAlive = true;
      driver = factory.GetDriver(new ConnectionInfo(provider, ConnectionString + ";pooling=false"), configuration);
      Assert.That(GetCheckConnectionIsAliveFlag(driver), Is.False);

      configuration = configuration.Clone();
      configuration.EnsureConnectionIsAlive = true;
      driver = factory.GetDriver(new ConnectionInfo(provider, ConnectionString + ";Pooling=False"), configuration);
      Assert.That(GetCheckConnectionIsAliveFlag(driver), Is.False);

      configuration = configuration.Clone();
      configuration.EnsureConnectionIsAlive = true;
      driver = factory.GetDriver(new ConnectionInfo(provider, ConnectionString + ";pooling = false"), configuration);
      Assert.That(GetCheckConnectionIsAliveFlag(driver), Is.False);

      configuration = configuration.Clone();
      configuration.EnsureConnectionIsAlive = true;
      driver = factory.GetDriver(new ConnectionInfo(provider, ConnectionString + ";Pooling = False"), configuration);
      Assert.That(GetCheckConnectionIsAliveFlag(driver), Is.False);
    }

    [Test]
    public void ConnectionHandlerTest()
    {
      var handlerInstance = new TestConnectionHandler();
      var handlersArray = new[] { handlerInstance };
      var descriptor = ProviderDescriptor.Get(provider);
      var factory = (SqlDriverFactory) Activator.CreateInstance(descriptor.DriverFactory);

      Assert.That(handlerInstance.OpeningCounter, Is.EqualTo(0));
      Assert.That(handlerInstance.OpeningInitCounter, Is.EqualTo(0));
      Assert.That(handlerInstance.OpenedCounter, Is.EqualTo(0));
      Assert.That(handlerInstance.OpeningFailedCounter, Is.EqualTo(0));

      var configuration = new SqlDriverConfiguration(handlersArray);
      _ = factory.GetDriver(new ConnectionInfo(Url), configuration);
      Assert.That(handlerInstance.OpeningCounter, Is.EqualTo(1));
      Assert.That(handlerInstance.OpeningInitCounter, Is.EqualTo(0));
      Assert.That(handlerInstance.OpenedCounter, Is.EqualTo(1));
      Assert.That(handlerInstance.OpeningFailedCounter, Is.EqualTo(0));

      configuration = new SqlDriverConfiguration(handlersArray) { EnsureConnectionIsAlive = true };
      _ = factory.GetDriver(new ConnectionInfo(Url), configuration);
      Assert.That(handlerInstance.OpeningCounter, Is.EqualTo(2));
      if (provider == WellKnown.Provider.SqlServer)
        Assert.That(handlerInstance.OpeningInitCounter, Is.EqualTo(1));
      else
        Assert.That(handlerInstance.OpeningInitCounter, Is.EqualTo(0));
      Assert.That(handlerInstance.OpenedCounter, Is.EqualTo(2));
      Assert.That(handlerInstance.OpeningFailedCounter, Is.EqualTo(0));

      configuration = new SqlDriverConfiguration(handlersArray) { ConnectionInitializationSql = InitQueryPerProvider(provider) };
      _ = factory.GetDriver(new ConnectionInfo(Url), configuration);
      Assert.That(handlerInstance.OpeningCounter, Is.EqualTo(3));
      if (provider == WellKnown.Provider.SqlServer)
        Assert.That(handlerInstance.OpeningInitCounter, Is.EqualTo(2));
      else
        Assert.That(handlerInstance.OpeningInitCounter, Is.EqualTo(1));
      Assert.That(handlerInstance.OpenedCounter, Is.EqualTo(3));
      Assert.That(handlerInstance.OpeningFailedCounter, Is.EqualTo(0));

      configuration = new SqlDriverConfiguration(handlersArray) { ConnectionInitializationSql = "dummy string to trigger error" };
      try {
        _ = factory.GetDriver(new ConnectionInfo(Url), configuration);
      }
      catch {
        //skip it
      }
      Assert.That(handlerInstance.OpeningCounter, Is.EqualTo(4));
      if (provider == WellKnown.Provider.SqlServer)
        Assert.That(handlerInstance.OpeningInitCounter, Is.EqualTo(3));
      else
        Assert.That(handlerInstance.OpeningInitCounter, Is.EqualTo(2));
      Assert.That(handlerInstance.OpenedCounter, Is.EqualTo(3));
      Assert.That(handlerInstance.OpeningFailedCounter, Is.EqualTo(1));
    }

    private static void TestProvider(string providerName, string connectionString, string connectionUrl)
    {
      Assert.IsNotNull(TestSqlDriver.Create(connectionUrl));
      Assert.IsNotNull(TestSqlDriver.Create(providerName, connectionString));
    }

    private static bool GetCheckConnectionIsAliveFlag(SqlDriver driver)
    {
      const string fieldName = "checkConnectionIsAlive";
      var type = typeof (Xtensive.Sql.Drivers.SqlServer.Driver);
      return (bool) type.GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        .GetValue(driver);
    }

    private static string InitQueryPerProvider(string currentProvider)
    {
      switch (currentProvider) {
        case WellKnown.Provider.Firebird:
          return "select current_timestamp from RDB$DATABASE;";
        case WellKnown.Provider.MySql:
          return "SELECT 0";
        case WellKnown.Provider.Oracle:
          return "select current_timestamp from DUAL";
        case WellKnown.Provider.PostgreSql:
          return "SELECT 0";
        case WellKnown.Provider.SqlServer:
          return "SELECT 0";
        case WellKnown.Provider.Sqlite:
          return "SELECT 0";
        default:
          throw new ArgumentOutOfRangeException(currentProvider);
      }
    }
  }
}