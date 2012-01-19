// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Enumerates possible Data Definition Language(DDL) statements
  /// in accordance with SQL92 standard.
  /// </summary>
  [Flags]
  public enum DdlStatements
  {
    /// <summary>
    /// Indicates that RDBMS does not support any DDL statement
    /// for the mentioned database entity.
    /// </summary>
    /// <value>0x0</value>
    None = 0x0,

    /// <summary>
    /// Indicates that RDBMS supports <see cref="Create"/> statement
    /// for the mentioned database entity.
    /// For constraints this indicates that RDBMS supports ADD statement.
    /// </summary>
    /// <value>0x1</value>
    Create = 0x1,

    /// <summary>
    /// Indicates that RDBMS supports <see cref="Alter"/> statement
    /// for the mentioned database entity.
    /// </summary>
    /// <value>0x2</value>
    Alter = 0x2,

    /// <summary>
    /// Indicates that RDBMS supports <see cref="Drop"/> statement
    /// for the mentioned database entity.
    /// </summary>
    /// <value>0x4</value>
    Drop = 0x4,

    /// <summary>
    /// Indicates that RDBMS supports <see cref="Rename"/> statement
    /// for the mentioned database entity.
    /// </summary>
    /// <value>0x4</value>
    Rename = 0x8,

    /// <summary>
    /// Indicates that RDBMS supports all DDL statements
    /// for the mentioned database entity.
    /// </summary>
    /// <value>0x7</value>
    All = Create | Alter | Drop | Rename
  }
}
