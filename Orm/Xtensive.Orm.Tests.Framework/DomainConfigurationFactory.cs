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

      var configuration = typeof(DomainConfigurationFactory).Assembly.GetAssemblyConfiguration();
      var domainConfiguration = DomainConfiguration.Load(configuration, storage);
      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      var customConnectionInfo = TestConfiguration.Instance.GetConnectionInfo(storage);
      if (customConnectionInfo!=null)
        domainConfiguration.ConnectionInfo = customConnectionInfo;
      if (addSessionConfiguration) {
        var defaultConfiguration = new SessionConfiguration(
          WellKnown.Sessions.Default, SessionOptions.ServerProfile | SessionOptions.AutoActivation);
        domainConfiguration.Sessions.Add(defaultConfiguration);
      }
      return domainConfiguration;
    }
  }
}
