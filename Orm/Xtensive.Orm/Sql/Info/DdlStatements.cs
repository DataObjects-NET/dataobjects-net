// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

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
    /// <value>0x8</value>
    Rename = 0x8,

    /// <summary>
    /// Indicates that RDBMS supports <see cref="Truncate"/> statement
    /// </summary>
    /// <value>0x10</value>
    Truncate = 0x10,

    /// <summary>
    /// Indicates that RDBMS supports all DDL statements
    /// for the mentioned database entity.
    /// </summary>
    /// <value>0x1F</value>
    All = Create | Alter | Drop | Rename | Truncate
  }
}
