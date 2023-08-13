// Copyright (C) 2013-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2013.07.23

using System;
using Xtensive.Orm.Providers;
using Xtensive.Sql;

namespace Xtensive.Orm.Tests
{
  public sealed class StorageProviderInfo
  {
    private static readonly object InstanceLock = new object();
    private static StorageProviderInfo InstanceValue;

    public static StorageProviderInfo Instance
    {
      get
      {
        lock (InstanceLock) {
          if (InstanceValue==null)
            InstanceValue = new StorageProviderInfo();
          return InstanceValue;
        }
      }
    }

    public StorageProvider Provider { get; private set; }

    public ProviderInfo Info { get; private set; }

    public IStorageTimeZoneProvider TimeZoneProvider { get; private set; }

    public bool CheckProviderIs(StorageProvider requiredProviders) =>
      (Provider & requiredProviders) != 0;

    public bool CheckProviderIsNot(StorageProvider disallowedProviders) =>
      (Provider & disallowedProviders) == 0;

    public bool CheckProviderVersionIsAtLeast(Version minimalVersion) =>
      Info.StorageVersion >= minimalVersion;

    public bool CheckProviderVersionIsAtMost(Version maximalVersion) =>
      Info.StorageVersion <= maximalVersion;

    public bool CheckAllFeaturesSupported(ProviderFeatures requiredFeatures) =>
      (Info.ProviderFeatures & requiredFeatures) == requiredFeatures;

    public bool CheckAllFeaturesNotSupported(ProviderFeatures disallowedFeatures) =>
      (Info.ProviderFeatures & disallowedFeatures) == 0;

    public bool CheckAnyFeatureSupported(ProviderFeatures requiredFeatures) =>
      (Info.ProviderFeatures & requiredFeatures) != 0;

    public bool CheckAnyFeatureNotSupported(ProviderFeatures disallowedFeatures) =>
      (Info.ProviderFeatures & disallowedFeatures) != disallowedFeatures;

    public IDisposable ReplaceTimeZoneProvider(IStorageTimeZoneProvider newProvder)
    {
      var oldProvider = TimeZoneProvider;
      TimeZoneProvider = newProvder;
      return new Core.Disposable((b) => TimeZoneProvider = oldProvider);
    }

    private StorageProviderInfo()
    {
      var config = DomainConfigurationFactory.Create();
      var providerName = config.ConnectionInfo.Provider;

      Provider = ParseProvider(providerName);
      var sqlDriver = TestSqlDriver.Create(config.ConnectionInfo);
      TimeZoneProvider = GetTimeZoneProvider(Provider, sqlDriver);
      Info = ProviderInfoBuilder.Build(providerName, sqlDriver);
    }

    private static StorageProvider ParseProvider(string provider)
    {
      switch (provider) {
        case WellKnown.Provider.SqlServer:
          return StorageProvider.SqlServer;
        case WellKnown.Provider.SqlServerCe:
          return StorageProvider.SqlServerCe;
        case WellKnown.Provider.PostgreSql:
          return StorageProvider.PostgreSql;
        case WellKnown.Provider.Oracle:
          return StorageProvider.Oracle;
        case WellKnown.Provider.MySql:
          return StorageProvider.MySql;
        case WellKnown.Provider.Firebird:
          return StorageProvider.Firebird;
        case WellKnown.Provider.Sqlite:
          return StorageProvider.Sqlite;
        default:
          throw new ArgumentOutOfRangeException("provider");
      }
    }

    private static IStorageTimeZoneProvider GetTimeZoneProvider(StorageProvider provider, SqlDriver sqlDriver)
    {
      switch (provider) {
        case StorageProvider.SqlServer:
          return new SqlServerTimeZoneProvider(sqlDriver);
        case StorageProvider.PostgreSql:
          return new PgSqlTimeZoneProvider(sqlDriver);
        case StorageProvider.Oracle:
          return new OracleTimeZoneProvider(sqlDriver);
        case StorageProvider.Sqlite:
          return new SqliteTimeZoneProvider(sqlDriver);
        default:
          return new LocalTimeZoneProvider();
      }
    }
  }
}