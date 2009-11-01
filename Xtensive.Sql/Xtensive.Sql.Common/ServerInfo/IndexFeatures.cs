// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// <para>Defines a list of possible index features.</para>
  /// <para>You can combine this features to describe certain RDBMS capabilities.</para>
  /// </summary>
  [Flags]
  public enum IndexFeatures
  {
    /// <summary>
    /// Indicates that RDBMS does not support any feature in the list.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Indicates that RDBMS supports clustered indexes.
    /// </summary>
    Clustered = 0x1,

    /// <summary>
    /// Indicates that RDBMS allows to include some columns as
    /// non key part of an index.
    /// </summary>
    NonKeyColumns = 0x2,

    /// <summary>
    /// Indicates that RDBMS allows to specify fill factor for an index.
    /// </summary>
    FillFactor = 0x4,

    /// <summary>
    /// Indicates that RDBMS supports unique indexes.
    /// </summary>
    Unique = 0x8,

    /// <summary>
    /// Indicates that RDBMS allows to specify max degree of parallelism
    /// parameter for an index.
    /// </summary>
    MaxDop = 0x10,

    /// <summary>
    /// Indicates that RDBMS supports full-text indexes.
    /// </summary>
    FullText = 0x20
  }
}
