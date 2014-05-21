// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.05.17

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Default schema information for a particular connection.
  /// </summary>
  public sealed class DefaultSchemaInfo
  {
    /// <summary>
    /// Gets default database.
    /// </summary>
    public string Database { get; private set; }

    /// <summary>
    /// Gets default schema.
    /// </summary>
    public string Schema { get; private set; }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="database">Value for <see cref="Database"/>.</param>
    /// <param name="schema">Value for <see cref="Schema"/>.</param>
    public DefaultSchemaInfo(string database, string schema)
    {
      Database = database;
      Schema = schema;
    }
  }
}