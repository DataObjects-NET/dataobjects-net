// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.17

using System;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Sql
{
    partial class SqlDriver
    {
        private const string DriverAssemblyFormat = "Xtensive.Sql.{0}";
        private static Dictionary<string, Type> factoryRegistry = new Dictionary<string, Type> {
      {"sqlserver", typeof(SqlServer.DriverFactory)},
      {"sqlserverce", typeof(SqlServerCe.DriverFactory)},
      {"oracle", typeof(Oracle.DriverFactory)},
      {"postgresql", typeof(PostgreSql.DriverFactory)},
      {"mysql", typeof(MySql.DriverFactory)}
    };

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
            var factory = (SqlDriverFactory)Activator.CreateInstance(factoryType);
            var connectionString = connectionInfo.ConnectionString
              ?? factory.BuildConnectionString(connectionInfo.ConnectionUrl);
            var driver = factory.CreateDriver(connectionString);
            driver.factory = factory;
            driver.Initialize();
            return driver;
        }
    }
}