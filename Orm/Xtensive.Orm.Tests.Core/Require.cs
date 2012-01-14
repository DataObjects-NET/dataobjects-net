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
using Xtensive.Orm.Providers.Sql;

namespace Xtensive.Orm.Tests
{
  public static class Require
  {
    private static bool isInitialized;
    private static StorageProvider activeProvider;
    private static ProviderInfo activeProviderInfo;

    public static void ProviderIs(StorageProvider allowedProviders)
    {
      EnsureIsInitialized();
      if ((activeProvider & allowedProviders)==0)
        IgnoreMe("This test is not suitable for '{0}' provider", activeProvider.ToString().ToLowerInvariant());
    }

    public static void ProviderIsNot(StorageProvider disallowedProviders)
    {
      ProviderIs(~disallowedProviders);
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

    public static void InheritanceSchemaIs(InheritanceSchema requiredInheritanceSchema)
    {
      EnsureIsInitialized();
      if (EnvironmentConfiguration.InheritanceSchema!=requiredInheritanceSchema)
        IgnoreMe("This test requires '{0}' inheritance schema", requiredInheritanceSchema);
    }

    public static void InheritanceSchemaIsNot(InheritanceSchema disallowedSchema)
    {
      EnsureIsInitialized();
      if (EnvironmentConfiguration.InheritanceSchema==disallowedSchema)
        IgnoreMe("This test requires any inheritance schema except '{0}'", disallowedSchema);
    }

    public static void ForeignKeyModeIs(ForeignKeyMode requiredForeignKeyMode)
    {
      EnsureIsInitialized();
      if (EnvironmentConfiguration.ForeignKeyMode!=requiredForeignKeyMode)
        IgnoreMe("This test requires '{0}' foreign key mode", requiredForeignKeyMode);
    }

    public static void ForeignKeyModeIsNot(ForeignKeyMode disallowedForeignKeyMode)
    {
      EnsureIsInitialized();
      if (EnvironmentConfiguration.ForeignKeyMode==disallowedForeignKeyMode)
        IgnoreMe("This test requires any foreign key mode except '{0}'", disallowedForeignKeyMode);
    }

    public static void TypeIdBehaviorIs(TypeIdBehavior requiredTypeIdBehavior)
    {
      EnsureIsInitialized();
      if (EnvironmentConfiguration.TypeIdBehavior!=requiredTypeIdBehavior)
        IgnoreMe("This test requires '{0}' type id behavior", requiredTypeIdBehavior);
    }

    public static void TypeIdBehaviorIsNot(TypeIdBehavior disallowedTypeIdBehavior)
    {
      EnsureIsInitialized();
      if (EnvironmentConfiguration.TypeIdBehavior==disallowedTypeIdBehavior)
        IgnoreMe("This test requires any type id behavior except '{0}'", disallowedTypeIdBehavior);
    }

    private static void IgnoreMe(string message, object argument)
    {
      throw new IgnoreException(string.Format(message, argument));
    }

    private static void EnsureIsInitialized()
    {
      if (isInitialized)
        return;
      isInitialized = true;

      var config = DomainConfigurationFactory.Create();
      activeProvider = ParseProvider(config.ConnectionInfo.Provider);

      activeProviderInfo = ProviderInfoBuilder.Build(TestSqlDriver.Create(config.ConnectionInfo));
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
        return  StorageProvider.Oracle;
      case WellKnown.Provider.MySql:
        return  StorageProvider.MySql;
      case WellKnown.Provider.Firebird:
        return  StorageProvider.Firebird;
      default:
        throw new ArgumentOutOfRangeException("provider");
      }
    }
  }
}