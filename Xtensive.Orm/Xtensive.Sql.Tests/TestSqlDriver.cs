// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.11.04

using System;
using System.Collections.Generic;
using System.Data;
using Xtensive.Core;

namespace Xtensive.Sql.Tests
{
  public static class TestSqlDriver
  {
    private static Dictionary<string, Type> factoryRegistry = new Dictionary<string, Type>();
//  {
//      {"sqlserver", typeof(SqlServer.DriverFactory)},
//      {"sqlserverce", typeof(SqlServerCe.DriverFactory)},
//      {"oracle", typeof(Oracle.DriverFactory)},
//      {"postgresql", typeof(PostgreSql.DriverFactory)},
//      {"firebird", typeof(Firebird.DriverFactory)},
//      {"mysql", typeof(MySql.DriverFactory)}
//    };


    /// <summary>
    /// Creates the driver from the specified connection URL.
    /// </summary>
    /// <param name="connectionUrl">The connection url.</param>
    /// <returns>Created driver.</returns>
    public static SqlDriver Create(UrlInfo connectionUrl)
    {
      ArgumentValidator.EnsureArgumentNotNull(connectionUrl, "connectionUrl");
      return BuildDriver(new ConnectionInfo(connectionUrl));
    }

    /// <summary>
    /// Creates the driver from the specified connection URL.
    /// </summary>
    /// <param name="connectionUrl">The connection url.</param>
    /// <returns>Created driver.</returns>
    public static SqlDriver Create(string connectionUrl)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(connectionUrl, "connectionUrl");
      return BuildDriver(new ConnectionInfo(connectionUrl));
    }

    /// <summary>
    /// Creates the driver from the specified connection string and driver name.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>Created driver.</returns>
    public static SqlDriver Create(string provider, string connectionString)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(provider, "provider");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(connectionString, "connectionString");
      return BuildDriver(new ConnectionInfo(provider, connectionString));
    }

    /// <summary>
    /// Creates the driver from the specified connection string and driver name.
    /// </summary>
    /// <param name="connectionInfo">The connection info.</param>
    /// <returns>Created driver.</returns>
    public static SqlDriver Create(ConnectionInfo connectionInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(connectionInfo, "connectionInfo");
      return BuildDriver(connectionInfo);
    }

    private static SqlDriver BuildDriver(ConnectionInfo connectionInfo)
    {
      var factoryType = factoryRegistry[connectionInfo.Provider.ToLower()];
      var factory = (SqlDriverFactory) Activator.CreateInstance(factoryType);
      return factory.CreateDriver(connectionInfo);
    }
  }
}