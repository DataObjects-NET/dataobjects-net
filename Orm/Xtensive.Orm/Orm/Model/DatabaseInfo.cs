// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.22

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Database.
  /// </summary>
  public sealed class DatabaseInfo : Node
  {
    /// <summary>
    /// Gets references databases
    /// </summary>
    public IList<DatabaseInfo> ReferencedDatabases { get; private set; }

    /// <summary>
    /// Gets all referenced database, i.e. transitive closure of <see cref="ReferencedDatabases"/>.
    /// </summary>
    /// <returns>All referenced databases.</returns>
    public IEnumerable<DatabaseInfo> GetAllReferencedDatabases()
    {
      return ReferencedDatabases.Flatten(db => db.ReferencedDatabases, null, true).Distinct();
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);

      // NodeCollection it ignores recursive flag in its Lock() method.
      // We shouldn't lock referenced databases here and thus can't use NodeCollection.

      ReferencedDatabases = ReferencedDatabases.ToList().AsReadOnly();
    }

    /// <summary>
    /// Creates new instance of this class.
    /// </summary>
    /// <param name="name">Node name.</param>
    public DatabaseInfo(string name)
      : base(name)
    {
      ReferencedDatabases = new List<DatabaseInfo>();
    }
  }
}