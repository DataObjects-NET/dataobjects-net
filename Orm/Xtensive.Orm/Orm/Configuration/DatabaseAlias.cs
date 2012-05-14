// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.08

using Xtensive.Core;
using Xtensive.Helpers;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Database alias entry.
  /// </summary>
  public sealed class DatabaseAlias : LockableBase
  {
    private string name;

    /// <summary>
    /// Gets name of this alias (i.e. logical database name).
    /// </summary>
    public string Name
    {
      get { return name; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(value, "value");
        this.EnsureNotLocked();
        name = value;
      }
    }

    private string database;

    /// <summary>
    /// Gets database this alias targets (i.e. physical database name).
    /// </summary>
    public string Database
    {
      get { return database; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(value, "value");
        this.EnsureNotLocked();
        database = value;
      }
    }

    /// <inheritdoc />
    public override string ToString()
    {
      return string.Format("{0} -> {1}", Name, Database);
    }

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns>Cloned instance.</returns>
    public DatabaseAlias Clone()
    {
      return new DatabaseAlias(name, database);
    }

    // Constructors

    /// <summary>
    /// Creates new instance of this class.
    /// </summary>
    /// <param name="name">Alias name.</param>
    /// <param name="database">Database name (alias target).</param>
    public DatabaseAlias(string name, string database)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(database, "database");

      Name = name;
      Database = database;
    }
  }
}