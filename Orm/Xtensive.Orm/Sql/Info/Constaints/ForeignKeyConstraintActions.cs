// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Defines all possible actions applicable in the case
  /// of foreign key conflict.
  /// </summary>
  [Flags]
  public enum ForeignKeyConstraintActions
  {
    /// <summary>
    /// No actions are supported.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Indicates that RDBMS is capable to do nothing in the case of some
    /// foreign key conflict.
    /// </summary>
    NoAction = 0x1,

    /// <summary>
    /// Indicates that RDBMS is capable to block execution of any instruction
    /// leading to foreign key conflict.
    /// </summary>
    Restrict = 0x2,

    /// <summary>
    /// Indicates that RDBMS is capable to perform some cascading operation
    /// (delete or update) in order to preserve referential integrity if
    /// it violated by some executed instruction.
    /// </summary>
    Cascade = 0x4,

    /// <summary>
    /// Indicates that RDBMS is capable to assign default value to a field
    /// referenced by foreign key constraint in order to preserve referential 
    /// integrity if it violated by some executed instruction.
    /// </summary>
    SetDefault = 0x8,

    /// <summary>
    /// Indicates that RDBMS is capable to assign <b>NULL</b> to a field
    /// referenced by foreign key constraint in order to preserve referential 
    /// integrity if it violated by some executed instruction.
    /// </summary>
    SetNull = 0x10,
  }
}
