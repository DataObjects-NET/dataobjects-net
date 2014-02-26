// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.02.25

namespace Xtensive.Orm.Tests.Sql
{
  public sealed class TestConnectionInfoProvider
  {
    public static string GetCurrentConnectionUrl()
    {
      return GetCurrentConnectionInfo().ConnectionUrl.Url;
    }

    public static string GetCurrentConnectionProvider()
    {
      return GetCurrentConnectionInfo().Provider;
    }

    public static string GetCurrentConnectionString()
    {
      return TestConfiguration.Instance.GetConnectionInfo(TestConfiguration.Instance.Storage+"cs").ConnectionString; 
    }

    public static ConnectionInfo GetCurrentConnectionInfo()
    {
      return TestConfiguration.Instance.GetConnectionInfo(TestConfiguration.Instance.Storage);
    }
  }
}
