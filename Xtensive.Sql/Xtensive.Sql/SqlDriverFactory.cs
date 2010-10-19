// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using Xtensive;

namespace Xtensive.Sql
{
  /// <summary>
  /// Creates drivers from the specified connection info.
  /// </summary>
  public abstract class SqlDriverFactory
  {
    /// <summary>
    /// Creates the driver from the specified <see cref="ConnectionInfo"/>.
    /// </summary>
    /// <param name="connectionString">The connection string to create driver from.</param>
    /// <returns>Created driver.</returns>
    public abstract SqlDriver CreateDriver(string connectionString);

    /// <summary>
    /// Builds the connection string from the specified URL.
    /// </summary>
    /// <param name="connectionUrl">The connection URL.</param>
    /// <returns>Built connection string</returns>
    public abstract string BuildConnectionString(UrlInfo connectionUrl);
  }
}