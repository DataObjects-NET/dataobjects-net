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
      return Create(false);
    }

    public static DomainConfiguration Create(bool useConnectionString)
    {
      var testConfiguration = TestConfiguration.Instance;
      var storageType = testConfiguration.Storage;
      if (useConnectionString)
        storageType += "cs";
      var config = DomainConfiguration.Load(storageType);
      var customConnectionInfo = testConfiguration.GetConnectionInfo(storageType);
      if (customConnectionInfo!=null)
        config.ConnectionInfo = customConnectionInfo;
      var defaultConfiguration = new SessionConfiguration(
        WellKnown.Sessions.Default, SessionOptions.ServerProfile | SessionOptions.AutoActivation);
      config.Sessions.Add(defaultConfiguration);
      return config;
    }

    /// <summary>
    /// Do not use for regular tests! Use Require.ProviderIs to require specific storage.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <returns>Configuration.</returns>
    public static DomainConfiguration CreateForCrudTest(string provider)
    {
      return DomainConfiguration.Load(provider);
    }
  }
}
