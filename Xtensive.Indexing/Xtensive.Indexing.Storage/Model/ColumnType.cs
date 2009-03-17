// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.16

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Enumerate possible column types.
  /// </summary>
  public enum ColumnType
  {
    /// <summary>
    /// Value column.
    /// </summary>
    Value = 0,
    /// <summary>
    /// Primary key column.
    /// </summary>
    PrimaryKey = 0x3,
    /// <summary>
    /// Secondary key column.
    /// </summary>
    SecondaryKey = 0x5
  }
}