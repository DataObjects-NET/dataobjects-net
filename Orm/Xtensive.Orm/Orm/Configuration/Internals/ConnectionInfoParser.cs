// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.09.27

using System;
using System.Configuration;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration
{
  internal static class ConnectionInfoParser
  {
    public static ConnectionInfo GetConnectionInfo(System.Configuration.Configuration configuration,string connectionUrl, string provider, string connectionString)
    {
      bool connectionUrlSpecified = !string.IsNullOrEmpty(connectionUrl);
      bool connectionStringSpecified = !string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(provider);

      if (connectionUrlSpecified && connectionStringSpecified)
        throw new InvalidOperationException(Strings.ExConnectionInfoIsWrongYouShouldSetEitherConnectionUrlElementOrProviderAndConnectionStringElements);

      if (connectionUrlSpecified)
        return new ConnectionInfo(connectionUrl);

      if (connectionStringSpecified)
        return new ConnectionInfo(provider, ExpandConnectionString(configuration, connectionString));

      // Neither connection string, nor connection url specified.
      // Leave connection information undefined.
      return null;
    }

    private static string ExpandConnectionString(System.Configuration.Configuration configuration, string connectionString)
    {
      const string prefix = "#";

      if (!connectionString.StartsWith(prefix, StringComparison.Ordinal))
        return connectionString;

      string connectionStringName = connectionString.Substring(prefix.Length);

      var connectionStringSetting = configuration.ConnectionStrings.ConnectionStrings[connectionStringName];
      if (connectionStringSetting==null)
        throw new InvalidOperationException(string.Format(Strings.ExConnectionStringWithNameXIsNotFound, connectionStringName));

      if (string.IsNullOrEmpty(connectionStringSetting.ConnectionString))
        throw new InvalidOperationException(string.Format(Strings.ExConnectionStringWithNameXIsNullOrEmpty, connectionStringName));

      return connectionStringSetting.ConnectionString;
    }
  }
}