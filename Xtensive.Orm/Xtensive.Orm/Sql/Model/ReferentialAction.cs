// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql
{
  /// <summary>
  /// Defines all possible actions applicable in the case
  /// of foreign key conflict.
  /// </summary>
  [Serializable]
  public enum ReferentialAction
  {
    /// <summary>
    /// Indicates that RDBMS is capable to do nothing in the case of some
    /// foreign key conflict.
    /// </summary>
    NoAction = 0,

    /// <summary>
    /// Indicates that RDBMS is capable to block execution of any instruction
    /// leading to foreign key conflict.
    /// </summary>
    Restrict = 1,

    /// <summary>
    /// Indicates that RDBMS is capable to perform some cascading operation
    /// (delete or update) in order to preserve referential integrity if
    /// it violated by some executed instruction.
    /// </summary>
    Cascade = 2,

    /// <summary>
    /// Indicates that RDBMS is capable to assign default value to a field
    /// referenced by foreign key constraint in order to preserve referential 
    /// integrity if it violated by some executed instruction.
    /// </summary>
    SetDefault = 3,

    /// <summary>
    /// Indicates that RDBMS is capable to assign <b>NULL</b> to a field
    /// referenced by foreign key constraint in order to preserve referential 
    /// integrity if it violated by some executed instruction.
    /// </summary>
    SetNull = 4,
  }
}
