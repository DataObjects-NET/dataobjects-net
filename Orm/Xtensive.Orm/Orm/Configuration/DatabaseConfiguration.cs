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
  public sealed class DatabaseConfiguration : LockableBase
  {
    private string name;
    private string alias;
    private int typeIdSeed;

    /// <summary>
    /// Gets or sets database name (i.e. physical database name).
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

    /// <summary>
    /// Gets or sets database alias (i.e. logical database name).
    /// </summary>
    public string Alias
    {
      get { return alias; }
      set
      {
        this.EnsureNotLocked();
        alias = value;
      }
    }

    /// <summary>
    /// Gets or sets type ID initial value
    /// for types mapped to this database.
    /// </summary>
    public int TypeIdSeed
    {
      get { return typeIdSeed; }
      set
      {
        ArgumentValidator.EnsureArgumentIsGreaterThanOrEqual(value, 0, "value");
        this.EnsureNotLocked();
        typeIdSeed = value;
      }
    }

    /// <inheritdoc />
    public override string ToString()
    {
      if (string.IsNullOrEmpty(alias))
        return Name;
      return string.Format("{0} -> {1}", Alias, Name);
    }

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns>Cloned instance.</returns>
    public DatabaseConfiguration Clone()
    {
      return new DatabaseConfiguration(name) {alias = alias};
    }

    // Constructors

    /// <summary>
    /// Creates new instance of this class.
    /// </summary>
    /// <param name="name">Database name.</param>
    public DatabaseConfiguration(string name)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      Name = name;
    }
  }
}