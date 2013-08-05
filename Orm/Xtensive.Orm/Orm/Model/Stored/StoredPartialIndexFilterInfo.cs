// Copyright (C) 2013 Xtensive LLC
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.07.18

using System.ComponentModel;

namespace Xtensive.Orm.Model.Stored
{
  /// <summary>
  /// Serializable information about partial index.
  /// </summary>
  public sealed class StoredPartialIndexFilterInfo
  {
    /// <summary>
    /// Gets or sets database name (if multi-database mode is enabled).
    /// </summary>
    [DefaultValue("")]
    public string Database;

    /// <summary>
    /// Gets or sets schema name (if multi-schema mode is enabled).
    /// </summary>
    [DefaultValue("")]
    public string Schema;

    /// <summary>
    /// Gets or sets table name.
    /// </summary>
    [DefaultValue("")]
    public string Table;

    /// <summary>
    /// Gets or sets index name.
    /// </summary>
    [DefaultValue("")]
    public string Name;

    /// <summary>
    /// Gets or sets filter expression.
    /// </summary>
    [DefaultValue("")]
    public string Filter;
  }
}