// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.22

using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Database info.
  /// </summary>
  public sealed class DatabaseInfo : Node
  {
    /// <summary>
    /// Gets references databases
    /// </summary>
    public IList<DatabaseInfo> ReferencedDatabases { get; private set; }

    /// <summary>
    /// Gets <see cref="DatabaseConfiguration"/> for this database.
    /// </summary>
    public DatabaseConfiguration Configuration { get; private set; }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);

      // NodeCollection it ignores recursive flag in its Lock() method.
      // We shouldn't lock referenced databases here and thus can't use NodeCollection.

      ReferencedDatabases = ReferencedDatabases.ToList().AsReadOnly();
    }

    // Constructors

    internal DatabaseInfo(string name, DatabaseConfiguration configuration)
      : base(name)
    {
      ReferencedDatabases = new List<DatabaseInfo>();
      Configuration = configuration;
    }
  }
}