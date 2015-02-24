// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// <para>Defines a list of typical features of identity columns.</para>
  /// <para>You can combine this features to describe certain RDBMS capabilities.</para>
  /// </summary>
  [Flags]
  public enum IdentityFeatures
  {
    /// <summary>
    /// Indicates that RDBMS does not support any feature in the list.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Indicates that RDBMS allows to specify seed value
    /// for identity columns.
    /// </summary>
    Seed = 0x1,

    /// <summary>
    /// Indicates that RDBMS allows to specify increment value
    /// for identity columns.
    /// </summary>
    Increment = 0x2,

    /// <summary>
    /// Indicates that RDBMS supports automatically incrementing
    /// identity columns.
    /// </summary>
    AutoIncrement = 0x4,

    /// <summary>
    /// Indicates that RDBMS stores state of automatically increment
    /// in memory and restore it according data in identity column.
    /// </summary>
    // 
    // --------------------------------------------------------------------------------------
    // WARNING!!!!!!
    // THIS OPTION CAN BROKE CONCURENT ACCESS FROM FEW DOMAINS TO GENERATOR TABLE.
    // DO NOT USE THIS OPTION WITH ANY RDBMS EXCEPT MYSQL.
    //---------------------------------------------------------------------------------------
    // This option fix dropping settings of auto-increment for generator table in MySQL.
    // It makes DO store last generated value in generator table.
    AutoIncrementSettingsInMemory = 0x8,
  }
}
