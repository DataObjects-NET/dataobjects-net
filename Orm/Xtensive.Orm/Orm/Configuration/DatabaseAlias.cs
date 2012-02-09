// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.08

using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Database alias entry.
  /// </summary>
  public sealed class DatabaseAlias
  {
    /// <summary>
    /// Gets name of this alias (i.e. logical database name).
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets database this alias targets (i.e. physical database name).
    /// </summary>
    public string Database { get; private set; }

    /// <inheritdoc />
    public override string ToString()
    {
      return string.Format("{0} -> {1}", Name, Database);
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DatabaseAlias(string name, string database)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(database, "database");

      Name = name;
      Database = database;
    }
  }
}