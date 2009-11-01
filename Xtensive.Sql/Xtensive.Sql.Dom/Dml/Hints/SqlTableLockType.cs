// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dom.Dml
{
  [Serializable]
  public enum SqlTableLockType
  {
    /// <summary>
    /// Specifies that ordinarily locks are taken.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Takes page locks either where individual locks are ordinarily taken on rows or keys, 
    /// or where a single table lock is ordinarily taken. 
    /// By default, uses the lock mode appropriate for the operation.
    /// </summary>
    PagLock = 1,

    /// <summary>
    /// Specifies that row locks are taken when page or table locks are ordinarily taken.
    /// </summary>
    RowLock = 2,

    /// <summary>
    /// Specifies that a shared lock is taken on the table held until the end-of-statement. 
    /// If <see cref="SqlTableIsolationLevel.HoldLock"/> is also specified, the shared table lock is held 
    /// until the end of the transaction.
    /// </summary>
    TabLock = 3,

    /// <summary>
    /// Specifies that an exclusive lock is taken on the table. 
    /// If <see cref="SqlTableIsolationLevel.HoldLock"/> is also specified, the lock is held until the transaction completes.
    /// </summary>
    TabLockX = 4,

    /// <summary>
    /// Specifies that update locks are to be taken and held until the transaction completes.
    /// </summary>
    UpdLock = 5,
  }
}
