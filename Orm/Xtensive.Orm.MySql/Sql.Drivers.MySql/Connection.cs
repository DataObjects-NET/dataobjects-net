// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.02.25

using System.Data;
using System.Data.Common;
using System.Security;
using MySql.Data.MySqlClient;

namespace Xtensive.Sql.Drivers.MySql
{
  internal class Connection : SqlConnection
  {
    private readonly MySqlConnection underlyingConnection;
    private MySqlTransaction activeTransaction;

    /// <inheritdoc/>
    public override DbConnection UnderlyingConnection
    {
      get { return underlyingConnection; }
    }

    /// <inheritdoc/>
    public override DbTransaction ActiveTransaction
    {
      get { return activeTransaction; }
    }

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public override DbParameter CreateParameter()
    {
      return new MySqlParameter();
    }

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public override void BeginTransaction()
    {
      EnsureTrasactionIsNotActive();
      activeTransaction = underlyingConnection.BeginTransaction();
    }

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public override void BeginTransaction(IsolationLevel isolationLevel)
    {
      EnsureTrasactionIsNotActive();
      activeTransaction = underlyingConnection.BeginTransaction(SqlHelper.ReduceIsolationLevel(isolationLevel));
    }

    /// <inheritdoc/>
    public override void MakeSavepoint(string name)
    {
      EnsureTransactionIsActive();
      string commandText = string.Format("SAVEPOINT {0}", name);
      using (DbCommand command = CreateCommand(commandText))
        command.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public override void RollbackToSavepoint(string name)
    {
      EnsureTransactionIsActive();
      string commandText = string.Format("ROLLBACK TO SAVEPOINT {0}; RELEASE SAVEPOINT {0};", name);
      using (DbCommand command = CreateCommand(commandText))
        command.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public override void ReleaseSavepoint(string name)
    {
      EnsureTransactionIsActive();
      string commandText = string.Format("RELEASE SAVEPOINT {0}", name);
      using (DbCommand command = CreateCommand(commandText))
        command.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    protected override void ClearActiveTransaction()
    {
      activeTransaction = null;
    }

    // Constructors

    [SecuritySafeCritical]
    public Connection(SqlDriver driver)
      : base(driver)
    {
      underlyingConnection = new MySqlConnection();
    }
  }
}