// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.11.04

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Building.Builders;
using Xtensive.Sql;

namespace Xtensive.Orm.Tests
{
  public static class TestSqlDriver
  {
    private static readonly Dictionary<string, SqlDriverFactory> FactoryCache = new Dictionary<string, SqlDriverFactory>();

    public static SqlDriver Create(UrlInfo connectionUrl)
    {
      ArgumentNullException.ThrowIfNull(connectionUrl);
      return BuildDriver(new ConnectionInfo(connectionUrl));
    }

    public static SqlDriver Create(string connectionUrl)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(connectionUrl, "connectionUrl");
      return BuildDriver(new ConnectionInfo(connectionUrl));
    }

    public static SqlDriver Create(string provider, string connectionString)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(provider, "provider");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(connectionString, "connectionString");
      return BuildDriver(new ConnectionInfo(provider, connectionString));
    }

    public static SqlDriver Create(ConnectionInfo connectionInfo)
    {
      ArgumentNullException.ThrowIfNull(connectionInfo);
      return BuildDriver(connectionInfo);
    }

    private static SqlDriver BuildDriver(ConnectionInfo connectionInfo)
    {
      return GetFactory(connectionInfo.Provider).GetDriver(connectionInfo);
    }

    private static SqlDriverFactory GetFactory(string provider)
    {
      lock (FactoryCache) {
        SqlDriverFactory factory;
        if (!FactoryCache.TryGetValue(provider, out factory)) {
          var descriptor = ProviderDescriptor.Get(provider);
          factory = (SqlDriverFactory) Activator.CreateInstance(descriptor.DriverFactory);
          FactoryCache.Add(provider, factory);
        }
        return factory;
      }
    }
  }
}