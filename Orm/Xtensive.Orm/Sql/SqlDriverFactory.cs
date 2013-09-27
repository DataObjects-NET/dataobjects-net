// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using Xtensive.Core;
using Xtensive.Orm;

namespace Xtensive.Sql
{
  /// <summary>
  /// Creates drivers from the specified connection info.
  /// </summary>
  public abstract class SqlDriverFactory
  {
    /// <summary>
    /// Gets driver for the specified <see cref="ConnectionInfo"/>.
    /// </summary>
    /// <param name="connectionInfo">Connection information to use.</param>
    /// <returns>Driver for <paramref name="connectionInfo"/>.</returns>
    public SqlDriver GetDriver(ConnectionInfo connectionInfo)
    {
      return GetDriver(connectionInfo, new SqlDriverConfiguration());
    }

    /// <summary>
    /// Creates driver from the specified <paramref name="connectionInfo"/>.
    /// </summary>
    /// <param name="connectionInfo">The connection info to create driver from.</param>
    /// <param name="forcedVersion">Forced server version.</param>
    /// <returns>Created driver.</returns>
    public SqlDriver GetDriver(ConnectionInfo connectionInfo, string forcedVersion)
    {
      return GetDriver(connectionInfo, new SqlDriverConfiguration {ForcedServerVersion = forcedVersion});
    }

    /// <summary>
    /// Create driver from the specified <paramref name="connectionInfo"/>
    /// and <paramref name="configuration"/>.
    /// </summary>
    /// <param name="connectionInfo">The connection info to create driver from.</param>
    /// <param name="configuration">Additional configuration options for the driver.</param>
    /// <returns>Created driver.</returns>
    public SqlDriver GetDriver(ConnectionInfo connectionInfo, SqlDriverConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(connectionInfo, "connectionInfo");
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");

      var connectionString = GetConnectionString(connectionInfo);
      configuration = configuration.Clone();
      var driver = CreateDriver(connectionString, configuration);
      driver.Initialize(this);
      return driver;
    }

    /// <summary>
    /// Gets connection string for the specified <see cref="ConnectionInfo"/>.
    /// </summary>
    /// <param name="connectionInfo">Connection information to process.</param>
    /// <returns>Connection string for <paramref name="connectionInfo"/>.</returns>
    public string GetConnectionString(ConnectionInfo connectionInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(connectionInfo, "connectionInfo");
      return connectionInfo.ConnectionString
        ?? BuildConnectionString(connectionInfo.ConnectionUrl);
    }

    /// <summary>
    /// Creates the driver from the specified <paramref name="connectionString"/>.
    /// </summary>
    /// <param name="connectionString">The connection string to create driver from.</param>
    /// <param name="configuration">Additional configuration for the driver.</param>
    /// <returns>Created driver.</returns>
    protected abstract SqlDriver CreateDriver(string connectionString, SqlDriverConfiguration configuration);

    /// <summary>
    /// Builds the connection string from the specified URL.
    /// </summary>
    /// <param name="connectionUrl">The connection URL.</param>
    /// <returns>Built connection string</returns>
    protected abstract string BuildConnectionString(UrlInfo connectionUrl);
  }
}