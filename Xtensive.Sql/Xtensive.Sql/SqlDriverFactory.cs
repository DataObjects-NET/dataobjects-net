// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using Xtensive.Core;

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
    /// <param name="connectionInfo">The connection info to create driver from.</param>
    /// <returns>Created driver.</returns>
    public abstract SqlDriver CreateDriver(ConnectionInfo connectionInfo);
  }
}