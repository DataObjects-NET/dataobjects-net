// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.02.25

using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Sql
{
  public sealed class TestConnectionInfoProvider
  {
    public static string GetConnectionUrl()
    {
      return GetConnectionInfo().ConnectionUrl.Url;
    }

    public static string GetProvider()
    {
      return GetConnectionInfo().Provider;
    }

    public static string GetConnectionString()
    {
      return GetConnectionInfo(TestConfiguration.Instance.Storage, true).ConnectionString;
    }

    public static ConnectionInfo GetConnectionInfo()
    {
      return GetConnectionInfo(TestConfiguration.Instance.Storage, false);
    }

    private static ConnectionInfo GetConnectionInfo(string storage, bool useConnectionInfo)
    {
      if (useConnectionInfo)
        storage += "cs";
      var configuration = typeof(TestConnectionInfoProvider).Assembly.GetAssemblyConfiguration();
      var domainConnectionInfo = DomainConfiguration.Load(configuration,storage).ConnectionInfo;
      var customConnectionInfo = TestConfiguration.Instance.GetConnectionInfo(storage);
      return customConnectionInfo ?? domainConnectionInfo;
    }
  }
}
