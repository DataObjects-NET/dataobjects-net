// Copyright (C) 2003-2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.12.27

namespace Xtensive.Sql
{
  /// <summary>
  /// Configuration for <see cref="SqlDriver"/>.
  /// </summary>
  public sealed class SqlDriverConfiguration
  {
    /// <summary>
    /// Gets or sets forced server version.
    /// </summary>
    public string ForcedServerVersion { get; set; }

    /// <summary>
    /// Gets or sets connection initialization SQL script.
    /// </summary>
    public string ConnectionInitializationSql { get; set; }

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns>Clone of this instance.</returns>
    public SqlDriverConfiguration Clone()
    {
      return new SqlDriverConfiguration {
        ForcedServerVersion = ForcedServerVersion,
        ConnectionInitializationSql = ConnectionInitializationSql,
      };
    }

    /// <summary>
    /// Creates new instance of this type.
    /// </summary>
    public SqlDriverConfiguration()
    {
    }
  }
}