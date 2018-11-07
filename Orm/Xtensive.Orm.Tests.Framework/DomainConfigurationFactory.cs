// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.05

using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests
{
  public static class DomainConfigurationFactory
  {
    public static DomainConfiguration Create()
    {
      return Create(TestConfiguration.Instance.Storage, false, true);
    }

    public static DomainConfiguration CreateForConnectionStringTest()
    {
      return Create(TestConfiguration.Instance.Storage, true, true);
    }

    public static DomainConfiguration CreateForCrudTest(string provider)
    {
      return Create(provider, false, false);
    }

    public static DomainConfiguration CreateWithoutSessionConfigurations()
    {
      return Create(TestConfiguration.Instance.Storage, false, false);
    }

    private static DomainConfiguration Create(string storage, bool useConnectionString, bool addSessionConfiguration)
    {
      if (useConnectionString)
        storage += "cs";

      var customConnectionInfo = TestConfiguration.Instance.GetConnectionInfo(storage);

      var configuration = (!DomainConfiguration.DomainExists(storage) && customConnectionInfo!=null)
        ? new DomainConfiguration {Name = storage}
        : DomainConfiguration.Load(storage);

      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      if (customConnectionInfo!=null)
        configuration.ConnectionInfo = customConnectionInfo;
      if (addSessionConfiguration) {
        var defaultConfiguration = new SessionConfiguration(
          WellKnown.Sessions.Default,
          SessionOptions.ServerProfile | SessionOptions.AutoActivation);
        configuration.Sessions.Add(defaultConfiguration);
      }

      return configuration;
    }
  }
}
