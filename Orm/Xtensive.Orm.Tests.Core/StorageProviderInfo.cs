// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.07.23

using System;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Sql;

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

    public bool CheckProviderIs(StorageProvider requiredProviders)
    {
      return (Provider & requiredProviders)!=0;
    }

    public bool CheckProviderIsNot(StorageProvider disallowedProviders)
    {
      return (Provider & disallowedProviders)==0;
    }

    public bool CheckProviderVersionIsAtLeast(Version minimalVersion)
    {
      return Info.StorageVersion >= minimalVersion;
    }

    public bool CheckProviderVersionIsAtMost(Version maximalVersion)
    {
      return Info.StorageVersion <= maximalVersion;
    }

    public bool CheckAllFeaturesSupported(ProviderFeatures requiredFeatures)
    {
      return (Info.ProviderFeatures & requiredFeatures)==requiredFeatures;
    }

    public bool CheckAllFeaturesNotSupported(ProviderFeatures disallowedFeatures)
    {
      return (Info.ProviderFeatures & disallowedFeatures)==0;
    }

    public bool CheckAnyFeatureSupported(ProviderFeatures requiredFeatures)
    {
      return (Info.ProviderFeatures & requiredFeatures)!=0;
    }

    public bool CheckAnyFeatureNotSupported(ProviderFeatures disallowedFeatures)
    {
      return (Info.ProviderFeatures & disallowedFeatures)!=disallowedFeatures;
    }

    private StorageProviderInfo()
    {
      var config = DomainConfigurationFactory.Create();
      var providerName = config.ConnectionInfo.Provider;

      Provider = ParseProvider(providerName);
      Info = ProviderInfoBuilder.Build(providerName, TestSqlDriver.Create(config.ConnectionInfo));
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
  }
}