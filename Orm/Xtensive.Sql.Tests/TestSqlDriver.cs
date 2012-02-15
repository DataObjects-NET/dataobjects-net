// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.11.04

using System;
using System.Collections.Generic;
using System.Data;
using Xtensive.Core;
using Xtensive.Sql.Compiler;

namespace Xtensive.Sql.Tests
{
  public static class TestSqlDriver
  {
    private static readonly Dictionary<string, Type> FactoryRegistry = new Dictionary<string, Type> {
        {"sqlserver", typeof (Drivers.SqlServer.DriverFactory)},
        {"sqlserverce", typeof (Drivers.SqlServerCe.DriverFactory)},
        {"oracle", typeof (Drivers.Oracle.DriverFactory)},
        {"postgresql", typeof (Drivers.PostgreSql.DriverFactory)},
        {"firebird", typeof (Drivers.Firebird.DriverFactory)},
        {"mysql", typeof (Drivers.MySql.DriverFactory)}
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
      var factoryType = FactoryRegistry[connectionInfo.Provider.ToLower()];
      var factory = (SqlDriverFactory) Activator.CreateInstance(factoryType);
      return factory.GetDriver(connectionInfo);
    }
  }
}