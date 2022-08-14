// Copyright (C) 2012-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.12.27

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm;

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
    /// Gets or sets a value indicating that connection should be checked before actual usage.
    /// </summary>
    public bool EnsureConnectionIsAlive { get; set; }

    /// <summary>
    /// Gets connection accessors that should be notified about connection events.
    /// </summary>
    public IReadOnlyCollection<IDbConnectionAccessor> DbConnectionAccessors { get; private set; }

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns>Clone of this instance.</returns>
    public SqlDriverConfiguration Clone()
    {
      // no deep cloning
      var accessors = (DbConnectionAccessors.Count == 0)
        ? Array.Empty<IDbConnectionAccessor>()
        : DbConnectionAccessors.ToArray(DbConnectionAccessors.Count);

      return new SqlDriverConfiguration(accessors) {
        ForcedServerVersion = ForcedServerVersion,
        ConnectionInitializationSql = ConnectionInitializationSql,
        EnsureConnectionIsAlive = EnsureConnectionIsAlive
      };
    }

    /// <summary>
    /// Creates new instance of this type.
    /// </summary>
    public SqlDriverConfiguration()
    {
      DbConnectionAccessors = Array.Empty<IDbConnectionAccessor>();
    }

    /// <summary>
    /// Creates new instance of this type.
    /// </summary>
    public SqlDriverConfiguration(IReadOnlyCollection<IDbConnectionAccessor> connectionAccessors)
    {
      DbConnectionAccessors = connectionAccessors;
    }
  }
}