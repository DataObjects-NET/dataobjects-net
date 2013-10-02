// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.11

using System;
using NUnit.Framework;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests
{
  public static class Require
  {
    public static void ProviderIs(StorageProvider allowedProviders, string reason = null)
    {
      var info = StorageProviderInfo.Instance;
      if (!info.CheckProviderIs(allowedProviders))
        IgnoreMe(
          "This test requires one of the following providers: {0}",
          allowedProviders.ToString().ToLowerInvariant(), reason);
    }

    public static void ProviderIsNot(StorageProvider disallowedProviders, string reason = null)
    {
      var info = StorageProviderInfo.Instance;
      if (!info.CheckProviderIsNot(disallowedProviders))
        IgnoreMe(
          "This test requires any provider except the following: {0}",
          disallowedProviders.ToString().ToLowerInvariant(), reason);
    }

    public static void ProviderVersionAtLeast(Version minimalVersion)
    {
      var info = StorageProviderInfo.Instance;
      if (!info.CheckProviderVersionIsAtLeast(minimalVersion))
        IgnoreMe("This test requires at least '{0}' version", minimalVersion);
    }

    public static void ProviderVersionAtMost(Version maximalVersion)
    {
      var info = StorageProviderInfo.Instance;
      if (!info.CheckProviderVersionIsAtMost(maximalVersion))
        IgnoreMe("This test requires at most '{0}' version", maximalVersion);
    }
    
    public static void AllFeaturesSupported(ProviderFeatures requiredFeatures)
    {
      var info = StorageProviderInfo.Instance;
      if (!info.CheckAllFeaturesSupported(requiredFeatures))
        IgnoreMe("This test requires storage that supports '{0}'", requiredFeatures);
    }

    public static void AllFeaturesNotSupported(ProviderFeatures disallowedFeatures)
    {
      var info = StorageProviderInfo.Instance;
      if (!info.CheckAllFeaturesNotSupported(disallowedFeatures))
        IgnoreMe("This test requires storage that does not support '{0}'", disallowedFeatures);
    }

    public static void AnyFeatureSupported(ProviderFeatures requiredFeatures)
    {
      var info = StorageProviderInfo.Instance;
      if (!info.CheckAnyFeatureSupported(requiredFeatures))
        IgnoreMe("This test requires storage that supports at least one of the '{0}' features", requiredFeatures);
    }

    public static void AnyFeatureNotSupported(ProviderFeatures disallowedFeatures)
    {
      var info = StorageProviderInfo.Instance;
      if (!info.CheckAnyFeatureNotSupported(disallowedFeatures))
        IgnoreMe("This test requires storage that does not support at least one of the '{0}' features", disallowedFeatures);
    }

    private static void IgnoreMe(string format, object argument, string reason = null)
    {
      var message = string.Format(format, argument);
      if (!string.IsNullOrEmpty(reason))
        message = string.Format("{0}. Reason: {1}", message, reason);
      throw new IgnoreException(message);
    }
  }
}