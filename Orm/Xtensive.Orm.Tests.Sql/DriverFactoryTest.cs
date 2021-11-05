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
using System.Threading.Tasks;
using System.Threading;

namespace Xtensive.Orm.Tests.Sql.DriverFactoryTestTypes
{
  public class TestConnectionAccessor : DbConnectionAccessor
  {
    public int OpeningCounter = 0;
    public int OpeningAsyncCounter = 0;

    public int OpeningInitCounter = 0;
    public int OpeningInitAsyncCounter = 0;

    public int OpenedCounter = 0;
    public int OpenedAsyncCounter = 0;

    public int OpeningFailedCounter = 0;
    public int OpeningFailedAsyncCounter = 0;

    public override void ConnectionOpening(ConnectionEventData eventData)
    {
      OpeningCounter++;
    }

    public override Task ConnectionOpeningAsync(ConnectionEventData eventData, CancellationToken cancellationToken)
    {
      OpeningAsyncCounter++;
      return base.ConnectionOpeningAsync(eventData, cancellationToken);
    }

    public override void ConnectionInitialization(ConnectionInitEventData eventData)
    {
      OpeningInitCounter++;
    }

    public override Task ConnectionInitializationAsync(ConnectionInitEventData eventData, CancellationToken cancellationToken)
    {
      OpeningInitAsyncCounter++;
      return base.ConnectionInitializationAsync(eventData, cancellationToken);
    }

    public override void ConnectionOpened(ConnectionEventData eventData)
    {
      OpenedCounter++;
    }

    public override Task ConnectionOpenedAsync(ConnectionEventData eventData, CancellationToken cancellationToken)
    {
      OpenedAsyncCounter++;
      return base.ConnectionOpenedAsync(eventData, cancellationToken);
    }

    public override void ConnectionOpeningFailed(ConnectionErrorEventData eventData)
    {
      OpeningFailedCounter++;
    }

    public override Task ConnectionOpeningFailedAsync(ConnectionErrorEventData eventData, CancellationToken cancellationToken)
    {
      OpeningFailedAsyncCounter++;
      return base.ConnectionOpeningFailedAsync(eventData, cancellationToken);
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
    public void ConnectionAccessorTest()
    {
      var accessorInstance = new TestConnectionAccessor();
      var accessorsArray = new[] { accessorInstance };
      var descriptor = ProviderDescriptor.Get(provider);
      var factory = (SqlDriverFactory) Activator.CreateInstance(descriptor.DriverFactory);

      Assert.That(accessorInstance.OpeningCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpeningAsyncCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpeningInitCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpeningInitAsyncCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpenedCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpenedAsyncCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpeningFailedCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpeningFailedAsyncCounter, Is.EqualTo(0));

      var configuration = new SqlDriverConfiguration(accessorsArray);
      _ = factory.GetDriver(new ConnectionInfo(Url), configuration);
      Assert.That(accessorInstance.OpeningCounter, Is.EqualTo(1));
      Assert.That(accessorInstance.OpeningAsyncCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpeningInitCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpeningInitAsyncCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpenedCounter, Is.EqualTo(1));
      Assert.That(accessorInstance.OpenedAsyncCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpeningFailedCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpeningFailedAsyncCounter, Is.EqualTo(0));

      configuration = new SqlDriverConfiguration(accessorsArray) { EnsureConnectionIsAlive = true };
      _ = factory.GetDriver(new ConnectionInfo(Url), configuration);
      Assert.That(accessorInstance.OpeningCounter, Is.EqualTo(2));
      Assert.That(accessorInstance.OpeningAsyncCounter, Is.EqualTo(0));

      if (provider == WellKnown.Provider.SqlServer) {
        Assert.That(accessorInstance.OpeningInitCounter, Is.EqualTo(1));
        Assert.That(accessorInstance.OpeningInitAsyncCounter, Is.EqualTo(0));
      }
      else {
        Assert.That(accessorInstance.OpeningInitCounter, Is.EqualTo(0));
        Assert.That(accessorInstance.OpeningInitAsyncCounter, Is.EqualTo(0));
      }

      Assert.That(accessorInstance.OpenedCounter, Is.EqualTo(2));
      Assert.That(accessorInstance.OpenedAsyncCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpeningFailedCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpeningFailedAsyncCounter, Is.EqualTo(0));

      configuration = new SqlDriverConfiguration(accessorsArray) { ConnectionInitializationSql = InitQueryPerProvider(provider) };
      _ = factory.GetDriver(new ConnectionInfo(Url), configuration);
      Assert.That(accessorInstance.OpeningCounter, Is.EqualTo(3));
      Assert.That(accessorInstance.OpeningAsyncCounter, Is.EqualTo(0));

      if (provider == WellKnown.Provider.SqlServer) {
        Assert.That(accessorInstance.OpeningInitCounter, Is.EqualTo(2));
        Assert.That(accessorInstance.OpeningInitAsyncCounter, Is.EqualTo(0));
      }
      else {
        Assert.That(accessorInstance.OpeningInitCounter, Is.EqualTo(1));
        Assert.That(accessorInstance.OpeningInitAsyncCounter, Is.EqualTo(0));
      }

      Assert.That(accessorInstance.OpenedCounter, Is.EqualTo(3));
      Assert.That(accessorInstance.OpenedAsyncCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpeningFailedCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpeningFailedAsyncCounter, Is.EqualTo(0));

      configuration = new SqlDriverConfiguration(accessorsArray) { ConnectionInitializationSql = "dummy string to trigger error" };
      try {
        _ = factory.GetDriver(new ConnectionInfo(Url), configuration);
      }
      catch {
        //skip it
      }
      Assert.That(accessorInstance.OpeningCounter, Is.EqualTo(4));
      Assert.That(accessorInstance.OpeningAsyncCounter, Is.EqualTo(0));

      if (provider == WellKnown.Provider.SqlServer) {
        Assert.That(accessorInstance.OpeningInitCounter, Is.EqualTo(3));
        Assert.That(accessorInstance.OpeningInitAsyncCounter, Is.EqualTo(0));
      }
      else {
        Assert.That(accessorInstance.OpeningInitCounter, Is.EqualTo(2));
        Assert.That(accessorInstance.OpeningInitAsyncCounter, Is.EqualTo(0));
      }

      Assert.That(accessorInstance.OpenedCounter, Is.EqualTo(3));
      Assert.That(accessorInstance.OpenedAsyncCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpeningFailedCounter, Is.EqualTo(1));
      Assert.That(accessorInstance.OpeningFailedAsyncCounter, Is.EqualTo(0));
    }

    [Test]
    public async Task ConnectionAccessorAsyncTest()
    {
      var accessorInstance = new TestConnectionAccessor();
      var accessorsArray = new[] { accessorInstance };
      var descriptor = ProviderDescriptor.Get(provider);
      var factory = (SqlDriverFactory) Activator.CreateInstance(descriptor.DriverFactory);

      Assert.That(accessorInstance.OpeningCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpeningAsyncCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpeningInitCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpeningInitAsyncCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpenedCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpenedAsyncCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpeningFailedCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpeningFailedAsyncCounter, Is.EqualTo(0));
      
      var configuration = new SqlDriverConfiguration(accessorsArray);
      _ = await factory.GetDriverAsync(new ConnectionInfo(Url), configuration, CancellationToken.None);
      Assert.That(accessorInstance.OpeningCounter, Is.EqualTo(1));
      Assert.That(accessorInstance.OpeningAsyncCounter, Is.EqualTo(1));
      Assert.That(accessorInstance.OpeningInitCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpeningInitAsyncCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpenedCounter, Is.EqualTo(1));
      Assert.That(accessorInstance.OpenedAsyncCounter, Is.EqualTo(1));
      Assert.That(accessorInstance.OpeningFailedCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpeningFailedAsyncCounter, Is.EqualTo(0));

      configuration = new SqlDriverConfiguration(accessorsArray) { EnsureConnectionIsAlive = true };
      _ = await factory.GetDriverAsync(new ConnectionInfo(Url), configuration, CancellationToken.None);
      Assert.That(accessorInstance.OpeningCounter, Is.EqualTo(2));
      Assert.That(accessorInstance.OpeningAsyncCounter, Is.EqualTo(2));

      if (provider == WellKnown.Provider.SqlServer) {
        Assert.That(accessorInstance.OpeningInitCounter, Is.EqualTo(1));
        Assert.That(accessorInstance.OpeningInitAsyncCounter, Is.EqualTo(1));
      }
      else {
        Assert.That(accessorInstance.OpeningInitCounter, Is.EqualTo(0));
        Assert.That(accessorInstance.OpeningInitAsyncCounter, Is.EqualTo(0));
      }

      Assert.That(accessorInstance.OpenedCounter, Is.EqualTo(2));
      Assert.That(accessorInstance.OpenedAsyncCounter, Is.EqualTo(2));
      Assert.That(accessorInstance.OpeningFailedCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpeningFailedAsyncCounter, Is.EqualTo(0));

      configuration = new SqlDriverConfiguration(accessorsArray) { ConnectionInitializationSql = InitQueryPerProvider(provider) };
      _ = await factory.GetDriverAsync(new ConnectionInfo(Url), configuration, CancellationToken.None);
      Assert.That(accessorInstance.OpeningCounter, Is.EqualTo(3));
      Assert.That(accessorInstance.OpeningAsyncCounter, Is.EqualTo(3));
      if (provider == WellKnown.Provider.SqlServer) {
        Assert.That(accessorInstance.OpeningInitCounter, Is.EqualTo(2));
        Assert.That(accessorInstance.OpeningInitAsyncCounter, Is.EqualTo(2));
      }
      else {
        Assert.That(accessorInstance.OpeningInitCounter, Is.EqualTo(1));
        Assert.That(accessorInstance.OpeningInitAsyncCounter, Is.EqualTo(1));
      }

      Assert.That(accessorInstance.OpenedCounter, Is.EqualTo(3));
      Assert.That(accessorInstance.OpenedAsyncCounter, Is.EqualTo(3));
      Assert.That(accessorInstance.OpeningFailedCounter, Is.EqualTo(0));
      Assert.That(accessorInstance.OpeningFailedAsyncCounter, Is.EqualTo(0));

      configuration = new SqlDriverConfiguration(accessorsArray) { ConnectionInitializationSql = "dummy string to trigger error" };
      try {
        _ = await factory.GetDriverAsync(new ConnectionInfo(Url), configuration, CancellationToken.None);
      }
      catch {
        //skip it
      }

      Assert.That(accessorInstance.OpeningCounter, Is.EqualTo(4));
      Assert.That(accessorInstance.OpeningAsyncCounter, Is.EqualTo(4));

      if (provider == WellKnown.Provider.SqlServer) {
        Assert.That(accessorInstance.OpeningInitCounter, Is.EqualTo(3));
        Assert.That(accessorInstance.OpeningInitAsyncCounter, Is.EqualTo(3));
      }
      else {
        Assert.That(accessorInstance.OpeningInitCounter, Is.EqualTo(2));
        Assert.That(accessorInstance.OpeningInitAsyncCounter, Is.EqualTo(2));
      }

      Assert.That(accessorInstance.OpenedCounter, Is.EqualTo(3));
      Assert.That(accessorInstance.OpenedAsyncCounter, Is.EqualTo(3));
      Assert.That(accessorInstance.OpeningFailedCounter, Is.EqualTo(1));
      Assert.That(accessorInstance.OpeningFailedAsyncCounter, Is.EqualTo(1));
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
        case WellKnown.Provider.Firebird: return "select current_timestamp from RDB$DATABASE;";
        case WellKnown.Provider.MySql: return "SELECT 0";
        case WellKnown.Provider.Oracle: return "select current_timestamp from DUAL";
        case WellKnown.Provider.PostgreSql: return "SELECT 0";
        case WellKnown.Provider.SqlServer: return "SELECT 0";
        case WellKnown.Provider.Sqlite: return "SELECT 0";
        default: throw new ArgumentOutOfRangeException(currentProvider);
      }
    }
  }
}
