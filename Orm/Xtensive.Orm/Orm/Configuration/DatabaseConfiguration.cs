// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.08

using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Database configuration entry.
  /// </summary>
  public sealed class DatabaseConfiguration : LockableBase
  {
    private string name;
    private string realName;

    private int minTypeId;
    private int maxTypeId;

    /// <summary>
    /// Gets or sets logical database name.
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
    /// Gets or sets physical database name.
    /// If this value is not set <see cref="Name"/>
    /// is used as physical database name.
    /// </summary>
    public string RealName
    {
      get { return realName; }
      set
      {
        this.EnsureNotLocked();
        realName = value;
      }
    }

    /// <summary>
    /// Gets or sets type ID minimal value
    /// for types mapped to this database.
    /// Default value is 100.
    /// </summary>
    public int MinTypeId
    {
      get { return minTypeId; }
      set
      {
        ArgumentValidator.EnsureArgumentIsGreaterThanOrEqual(value, TypeInfo.MinTypeId, "value");
        this.EnsureNotLocked();
        minTypeId = value;
      }
    }

    /// <summary>
    /// Gets or sets type ID maximal value
    /// for types mapped to this database.
    /// Default value is <see cref="int.MaxValue"/>.
    /// </summary>
    public int MaxTypeId
    {
      get { return maxTypeId; }
      set
      {
        ArgumentValidator.EnsureArgumentIsGreaterThanOrEqual(value, TypeInfo.MinTypeId, "value");
        maxTypeId = value;
      }
    }

    /// <inheritdoc />
    public override string ToString()
    {
      if (string.IsNullOrEmpty(realName))
        return name;
      return $"{name} -> {realName}";
    }

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns>Cloned instance.</returns>
    public DatabaseConfiguration Clone()
    {
      return new DatabaseConfiguration(name) {
        realName = realName,
        minTypeId = minTypeId,
        maxTypeId = maxTypeId,
      };
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
      minTypeId = TypeInfo.MinTypeId;
      maxTypeId = int.MaxValue;
    }
  }
}