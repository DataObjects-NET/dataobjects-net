// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using Xtensive.Core;
using Xtensive.Threading;

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
      ArgumentValidator.EnsureArgumentNotNull(connectionInfo, "connectionInfo");
      var driver = CreateDriver(GetConnectionString(connectionInfo));
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
    /// <returns>Created driver.</returns>
    protected abstract SqlDriver CreateDriver(string connectionString);

    /// <summary>
    /// Builds the connection string from the specified URL.
    /// </summary>
    /// <param name="connectionUrl">The connection URL.</param>
    /// <returns>Built connection string</returns>
    protected abstract string BuildConnectionString(UrlInfo connectionUrl);
  }
}