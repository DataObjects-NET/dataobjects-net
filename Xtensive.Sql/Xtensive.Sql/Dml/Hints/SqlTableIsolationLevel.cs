// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public enum SqlTableIsolationLevel
  {
    /// <summary>
    /// Specifies that table scan isolation level is equal to the statement transaction isolation level.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Is equivalent to ReadUncommitted.
    /// </summary>
    NoLock = 1,

    /// <summary>
    /// Specifies that dirty reads are allowed. No shared locks are issued to prevent other transactions 
    /// from modifying data read by the current transaction, and exclusive locks set by other transactions 
    /// do not block the current transaction from reading the locked data. Allowing dirty reads can cause 
    /// higher concurrency, but at the cost of reading data modifications that then are rolled back by other 
    /// transactions. Reading modifications that are then rolled back may generate errors for your transaction, 
    /// or present users that have data that was never committed.
    /// ReadUncommitted, and NoLock, cannot be specified for tables modified by insert, update, or delete operations.
    /// The SQL Server query optimizer ignores the ReadUncommitted and NoLock hints in the FROM clause that apply 
    /// to the target table of an UPDATE or DELETE statement.
    /// </summary>
    ReadUncommitted = NoLock,

    /// <summary>
    /// Specifies that read operations comply with the rules for the READ COMMITTED isolation level by using 
    /// either locking or row versioning. If the database option READ_COMITTED_SNAPSHOT is OFF, the Database Engine 
    /// acquires shared locks as data is read and releases those locks when the read operation is completed. 
    /// If the database option READ_COMMITTED_SNAPSHOT is ON, the Database Engine does not acquire locks and uses 
    /// row versioning. 
    /// </summary>
    ReadCommitted = 2,

    /// <summary>
    /// Specifies that read operations comply with the rules for the READ COMMITTED isolation level by using locking. 
    /// The Database Engine acquires shared locks as data is read and releases those locks when the read operation is completed, 
    /// regardless of the setting of the READ_COMMITTED_SNAPSHOT database option.
    /// </summary>
    ReadCommittedLock = 3,

    /// <summary>
    /// Specifies that a scan is performed with the same locking semantics as a transaction running at 
    /// REPEATABLE READ isolation level.
    /// </summary>
    RepeatableRead = 4,

    /// <summary>
    /// Is equivalent to HoldLock. Makes shared locks more restrictive by holding them until a transaction is completed, 
    /// instead of releasing the shared lock as soon as the required table or data page is no longer needed, 
    /// whether the transaction has been completed or not. The scan is performed with the same semantics 
    /// as a transaction running at the SERIALIZABLE isolation level. 
    /// </summary>
    Serializable = 5,

    /// <summary>
    /// Is equivalent to Serializable. HoldLock applies only to the table or view for which it is specified and 
    /// only for the duration of the transaction defined by the statement that it is used in. 
    /// HoldLock cannot be used in a SELECT statement that includes the FOR BROWSE option.
    /// </summary>
    HoldLock = Serializable,
  }
}
