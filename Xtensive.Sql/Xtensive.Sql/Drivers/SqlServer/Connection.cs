// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.11

using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using SqlServerConnection = System.Data.SqlClient.SqlConnection;

namespace Xtensive.Sql.SqlServer
{
  internal class Connection : SqlConnection
  {
    private SqlServerConnection underlyingConnection;
    private SqlTransaction activeTransaction;

    /// <inheritdoc/>
    public override DbConnection UnderlyingConnection { get { return underlyingConnection; } }

    /// <inheritdoc/>
    public override DbTransaction ActiveTransaction { get { return activeTransaction; } }

    /// <inheritdoc/>
    public override DbParameter CreateParameter()
    {
      return new SqlParameter();
    }

    /// <inheritdoc/>
    public override void BeginTransaction()
    {
      EnsureTrasactionIsNotActive();
      activeTransaction = underlyingConnection.BeginTransaction();
    }

    /// <inheritdoc/>
    public override void BeginTransaction(IsolationLevel isolationLevel)
    {
      EnsureTrasactionIsNotActive();
      activeTransaction = underlyingConnection.BeginTransaction(isolationLevel);
    }
    
    /// <inheritdoc/>
    public override void MakeSavepoint(string name)
    {
      EnsureTransactionIsActive();
      activeTransaction.Save(name);
    }

    /// <inheritdoc/>
    public override void RollbackToSavepoint(string name)
    {
      EnsureTransactionIsActive();
      activeTransaction.Rollback(name);
    }

    /// <inheritdoc/>
    public override void ReleaseSavepoint(string name)
    {
      EnsureTransactionIsActive();
      // nothing
    }

    /// <inheritdoc/>
    protected override void ClearActiveTransaction()
    {
      activeTransaction = null;
    }


    // Constructors

    public Connection(SqlDriver driver, string connectionString)
      : base(driver, connectionString)
    {
      underlyingConnection = new SqlServerConnection(connectionString);
    }
  }
}