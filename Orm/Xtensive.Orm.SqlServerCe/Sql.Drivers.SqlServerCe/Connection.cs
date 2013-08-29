// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.11

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Security;

namespace Xtensive.Sql.Drivers.SqlServerCe
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
    [SecuritySafeCritical]
    public override DbParameter CreateParameter()
    {
      return new SqlCeParameter();
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
      activeTransaction = underlyingConnection.BeginTransaction(isolationLevel);
    }

    /// <inheritdoc/>
    protected override void ClearActiveTransaction()
    {
      activeTransaction = null;
    }


    // Constructors

    /// <inheritdoc/>
    [SecuritySafeCritical]
    public Connection(SqlDriver driver, string connectionString)
      : base(driver, connectionString)
    {
      underlyingConnection = new SqlCeConnection(connectionString);
    }
  }
}