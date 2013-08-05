// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.11.04

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm;

namespace Xtensive.Sql.Tests
{
  public static class TestSqlDriver
  {
    private static readonly Dictionary<ConnectionInfo, SqlDriver> DriverCache = new Dictionary<ConnectionInfo, SqlDriver>();

    private static readonly Dictionary<string, Type> FactoryRegistry = new Dictionary<string, Type> {
        {WellKnown.Provider.SqlServer, typeof (Drivers.SqlServer.DriverFactory)},
        {WellKnown.Provider.SqlServerCe, typeof (Drivers.SqlServerCe.DriverFactory)},
        {WellKnown.Provider.Oracle, typeof (Drivers.Oracle.DriverFactory)},
        {WellKnown.Provider.PostgreSql, typeof (Drivers.PostgreSql.DriverFactory)},
        {WellKnown.Provider.Firebird, typeof (Drivers.Firebird.DriverFactory)},
        {WellKnown.Provider.MySql, typeof (Drivers.MySql.DriverFactory)},
        {WellKnown.Provider.Sqlite, typeof (Drivers.Sqlite.DriverFactory)},
      };

    public static SqlDriver Create(UrlInfo connectionUrl)
    {
      ArgumentValidator.EnsureArgumentNotNull(connectionUrl, "connectionUrl");
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
      ArgumentValidator.EnsureArgumentNotNull(connectionInfo, "connectionInfo");
      return BuildDriver(connectionInfo);
    }

    private static SqlDriver BuildDriver(ConnectionInfo connectionInfo)
    {
      lock (DriverCache) {
        SqlDriver driver;
        if (!DriverCache.TryGetValue(connectionInfo, out driver)) {
          var factoryType = FactoryRegistry[connectionInfo.Provider];
          var factory = (SqlDriverFactory) Activator.CreateInstance(factoryType);
          driver = factory.GetDriver(connectionInfo);
          DriverCache.Add(connectionInfo, driver);
        }
        return driver;
      }
    }
  }
}