// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// <para>Defines a list of possible transaction isolation levels.</para>
  /// </summary>
  [Flags]
  public enum IsolationLevels
  {
    /// <summary>
    /// Indicates that RDBMS does not support transaction isolation.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Indicates that RDBMS allows to execute transactions without isolation.
    /// </summary>
    ReadUncommitted = 0x1,

    /// <summary>
    /// Indicates that RDBMS allows to execute transactions with the
    /// <see cref="ReadCommitted"/> isolation level.
    /// </summary>
    ReadCommitted = 0x2,

    /// <summary>
    /// Indicates that RDBMS allows to execute transactions with the
    /// <see cref="RepeatableRead"/> isolation level.
    /// </summary>
    RepeatableRead = 0x4,

    /// <summary>
    /// Indicates that RDBMS allows to execute transactions with the
    /// <see cref="Serializable"/> isolation level.
    /// </summary>
    Serializable = 0x8,

    /// <summary>
    /// Indicates that RDBMS allows to execute transactions with
    /// <see cref="Snapshot"/> isolation level.
    /// </summary>
    Snapshot = 0x10,
  }
}
