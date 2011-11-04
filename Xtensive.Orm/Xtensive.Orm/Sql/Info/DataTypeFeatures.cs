// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Defines a list of features those are allows to describe
  /// RDBMS capabilities concerning a certain data type.
  /// </summary>
  [Flags]
  public enum DataTypeFeatures
  {
    /// <summary>
    /// Indicates that RDBMS supports nothing additional
    /// for the mentioned data type.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Indicates that RDBMS supports fill factor property
    /// for the mentioned data type.
    /// </summary>
    FillFactor = 0x1,

    /// <summary>
    /// Indicates that RDBMS allows key constraints
    /// for the mentioned data type.
    /// </summary>
    KeyConstraint = 0x2,

    /// <summary>
    /// Indicates that RDBMS allows multiple columns
    /// of the mentioned data type in a single table.
    /// </summary>
    Multiple = 0x4,

    /// <summary>
    /// Indicates that RDBMS allows null values
    /// for the mentioned data type.
    /// </summary>
    Nullable = 0x8,

    /// <summary>
    /// Indicates that RDBMS allows to declare identity
    /// column of the mentioned data type.
    /// </summary>
    Identity = 0x10,

    /// <summary>
    /// Indicates that RDBMS allows to specify default value
    /// for a column of the mentioned data type.
    /// </summary>
    Default = 0x20,

    /// <summary>
    /// Indicates that RDBMS supports grouping operations
    /// by columns of the mentioned data type.
    /// </summary>
    Grouping = 0x40,

    /// <summary>
    /// Indicates that RDBMS supports ordering operations
    /// by columns of the mentioned data type.
    /// </summary>
    Ordering = 0x80,

    /// <summary>
    /// Indicates that RDBMS allows to build clustered index
    /// which includes key columns of the mentioned data type.
    /// </summary>
    Clustering = 0x100,

    /// <summary>
    /// Indicates that RDBMS allows to build index
    /// which includes key columns of the mentioned data type.
    /// </summary>
    Indexing = 0x200,

    /// <summary>
    /// Indicates that RDBMS allows to include column of the 
    /// mentioned data type as non-key part of some index.
    /// </summary>
    NonKeyIndexing = 0x400,

    /// <summary>
    /// Indicates that RDBMS treats zero length value as NULL.
    /// </summary>
    ZeroLengthValueIsNull = 0x800,
  }
}
