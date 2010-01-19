// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.11

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlServerCe;

namespace Xtensive.Sql.SqlServerCe
{
  internal class Connection : SqlConnection
  {
    private SqlCeConnection underlyingConnection;
    private SqlCeTransaction activeTransaction;

    /// <inheritdoc/>
    public override DbConnection UnderlyingConnection { get { return underlyingConnection; } }

    /// <inheritdoc/>
    public override DbTransaction ActiveTransaction { get { return activeTransaction; } }

    /// <inheritdoc/>
    public override DbParameter CreateParameter()
    {
      return new SqlCeParameter();
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
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public override void RollbackToSavepoint(string name)
    {
      throw new NotSupportedException();
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

    public Connection(SqlDriver driver)
      : base(driver)
    {
      underlyingConnection = new SqlCeConnection(driver.CoreServerInfo.ConnectionString);
    }
  }
}