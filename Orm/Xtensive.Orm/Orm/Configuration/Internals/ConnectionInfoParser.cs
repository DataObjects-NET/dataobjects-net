// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.09.27

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration
{
  internal static class ConnectionInfoParser
  {
    private readonly struct UnifiedAccessor
    {
      private readonly IDictionary<string, string> connectionStringsAsDict;
      private readonly ConnectionStringSettingsCollection connectionStrings;

      public string this[string key] {
        get {
          if (connectionStrings != null)
            return connectionStrings[key]?.ConnectionString;
          else if(connectionStringsAsDict!=null)
            return connectionStringsAsDict[key];
          return null;
        }
      }

      public UnifiedAccessor(ConnectionStringSettingsCollection oldConnectionStrings)
      {
        connectionStringsAsDict = null;
        connectionStrings = oldConnectionStrings;
      }
      public UnifiedAccessor(IDictionary<string, string> connectionStrings)
      {
        connectionStringsAsDict = connectionStrings;
        this.connectionStrings = null;
      }
    }

    public static ConnectionInfo GetConnectionInfo(System.Configuration.Configuration configuration,string connectionUrl, string provider, string connectionString)
    {
      var accessor = new UnifiedAccessor(configuration.ConnectionStrings.ConnectionStrings);
      return GetConnectionInfoInternal(accessor, connectionUrl, provider, connectionString);
    }

    public static ConnectionInfo GetConnectionInfo(IDictionary<string,string> connectionStrings, string connectionUrl, string provider, string connectionString)
    {
      var accessor = new UnifiedAccessor(connectionStrings);
      return GetConnectionInfoInternal(accessor, connectionUrl, provider, connectionString);
    }

    private static ConnectionInfo GetConnectionInfoInternal(in UnifiedAccessor connectionStringAccessor,
      string connectionUrl, string provider, string connectionString)
    {
      var connectionUrlSpecified = !string.IsNullOrEmpty(connectionUrl);
      var connectionStringSpecified = !string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(provider);

      if (connectionUrlSpecified && connectionStringSpecified)
        throw new InvalidOperationException(Strings.ExConnectionInfoIsWrongYouShouldSetEitherConnectionUrlElementOrProviderAndConnectionStringElements);

      if (connectionUrlSpecified)
        return new ConnectionInfo(connectionUrl);

      if (connectionStringSpecified)
        return new ConnectionInfo(provider,
          ExpandConnectionString(connectionStringAccessor, connectionString));

      // Neither connection string, nor connection url specified.
      // Leave connection information undefined.
      return null;
    }

    private static string ExpandConnectionString(in UnifiedAccessor connectionStrings, string connectionString)
    {
      const string prefix = "#";

      if (!connectionString.StartsWith(prefix, StringComparison.Ordinal))
        return connectionString;

      var connectionStringName = connectionString[prefix.Length..];

      var connectionStringSetting = connectionStrings[connectionStringName];
      if (connectionStringSetting == null)
        throw new InvalidOperationException(string.Format(Strings.ExConnectionStringWithNameXIsNotFound, connectionStringName));

      if (string.IsNullOrEmpty(connectionStringSetting))
        throw new InvalidOperationException(string.Format(Strings.ExConnectionStringWithNameXIsNullOrEmpty, connectionStringName));

      return connectionStringSetting;
    }
  }
}