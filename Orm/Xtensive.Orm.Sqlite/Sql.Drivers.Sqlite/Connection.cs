// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.04.29

using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Security;

namespace Xtensive.Sql.Drivers.Sqlite
{
  internal class Connection : SqlConnection
  {
    private SQLiteConnection underlyingConnection;
    private SQLiteTransaction activeTransaction;

    /// <inheritdoc/>
    public override DbConnection UnderlyingConnection { get { return underlyingConnection; } }

    /// <inheritdoc/>
    public override DbTransaction ActiveTransaction { get { return activeTransaction; } }

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public override DbParameter CreateParameter()
    {
      return new SQLiteParameter();
    }

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public override void BeginTransaction()
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsNotActive();
      activeTransaction = underlyingConnection.BeginTransaction();
    }

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public override void BeginTransaction(IsolationLevel isolationLevel)
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsNotActive();
      activeTransaction = underlyingConnection.BeginTransaction(SqlHelper.ReduceIsolationLevel(isolationLevel));
    }

    /// <inheritdoc/>
    public override void MakeSavepoint(string name)
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsActive();
      var commandText = string.Format("SAVEPOINT {0}", name);
      using (var command = CreateCommand(commandText))
        command.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public override void RollbackToSavepoint(string name)
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsActive();
      var commandText = string.Format("ROLLBACK TO SAVEPOINT {0}; RELEASE SAVEPOINT {0};", name);
      using (var command = CreateCommand(commandText))
        command.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public override void ReleaseSavepoint(string name)
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsActive();
      var commandText = string.Format("RELEASE SAVEPOINT {0}", name);
      using (var command = CreateCommand(commandText))
        command.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    protected override void ClearActiveTransaction()
    {
      activeTransaction = null;
    }

    /// <inheritdoc/>
    protected override void ClearUnderlyingConnection()
    {
      underlyingConnection = null;
    }

    // Constructors

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public Connection(SqlDriver driver)
      : base(driver)
    {
      underlyingConnection = new SQLiteConnection();
    }
  }
}