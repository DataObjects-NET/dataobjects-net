// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.11

using System;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Orm.Model;
using Xtensive.Sql.Tests;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests
{
  public static class Require
  {
    private static bool isInitialized;
    private static StorageProvider activeProvider;
    private static ProviderInfo activeProviderInfo;

    public static void ProviderIs(StorageProvider allowedProviders, string reason = null)
    {
      EnsureIsInitialized();
      if ((activeProvider & allowedProviders)==0)
        IgnoreMe(
          "This test requires one of the following providers: {0}",
          allowedProviders.ToString().ToLowerInvariant(), reason);
    }

    public static void ProviderIsNot(StorageProvider disallowedProviders, string reason = null)
    {
      EnsureIsInitialized();
      if ((activeProvider & ~disallowedProviders)==0)
        IgnoreMe(
          "This test requires any provider except the following: {0}",
          disallowedProviders.ToString().ToLowerInvariant(), reason);
    }

    public static void ProviderVersionAtLeast(Version minimalVersion)
    {
      EnsureIsInitialized();
      if (activeProviderInfo.StorageVersion < minimalVersion)
        IgnoreMe("This test requires at least '{0}' version", minimalVersion);
    }

    public static void ProviderVersionAtMost(Version maximalVersion)
    {
      EnsureIsInitialized();
      if (activeProviderInfo.StorageVersion > maximalVersion)
        IgnoreMe("This test requires at most '{0}' version", maximalVersion);
    }
    
    public static void AllFeaturesSupported(ProviderFeatures requiredFeatures)
    {
      EnsureIsInitialized();
      if ((requiredFeatures & activeProviderInfo.ProviderFeatures)!=requiredFeatures)
        IgnoreMe("This test requires storage that supports '{0}'", requiredFeatures);
    }

    public static void AllFeaturesNotSupported(ProviderFeatures disallowedFeatures)
    {
      EnsureIsInitialized();
      if ((disallowedFeatures & activeProviderInfo.ProviderFeatures)!=0)
        IgnoreMe("This test requires storage that does not support '{0}'", disallowedFeatures);
    }

    public static void AnyFeatureSupported(ProviderFeatures requiredFeatures)
    {
      EnsureIsInitialized();
      if ((requiredFeatures & activeProviderInfo.ProviderFeatures)==0)
        IgnoreMe("This test requires storage that supports at least one of the '{0}' features", requiredFeatures);
    }

    public static void AnyFeatureNotSupported(ProviderFeatures disallowedFeatures)
    {
      EnsureIsInitialized();
      if ((disallowedFeatures & activeProviderInfo.ProviderFeatures)==disallowedFeatures)
        IgnoreMe("This test requires storage that does not support at least one of the '{0}' features", disallowedFeatures);
    }

    private static void IgnoreMe(string format, object argument, string reason = null)
    {
      var message = string.Format(format, argument);
      if (!string.IsNullOrEmpty(reason))
        message = string.Format("{0}. Reason: {1}", message, reason);
      throw new IgnoreException(message);
    }

    private static void EnsureIsInitialized()
    {
      if (isInitialized)
        return;
      isInitialized = true;

      var config = DomainConfigurationFactory.Create();
      var providerName = config.ConnectionInfo.Provider;
      activeProvider = ParseProvider(providerName);

      activeProviderInfo = ProviderInfoBuilder.Build(providerName, TestSqlDriver.Create(config.ConnectionInfo));
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