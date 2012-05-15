// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.04.29

using System;
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
#if NET40
    [SecuritySafeCritical]
#endif
    public override DbParameter CreateParameter()
    {
      return new SQLiteParameter();
    }

    /// <inheritdoc/>
#if NET40
    [SecuritySafeCritical]
#endif
    public override void BeginTransaction()
    {
      EnsureTrasactionIsNotActive();
      activeTransaction = underlyingConnection.BeginTransaction();
    }

    /// <inheritdoc/>
#if NET40
    [SecuritySafeCritical]
#endif
    public override void BeginTransaction(IsolationLevel isolationLevel)
    {
      EnsureTrasactionIsNotActive();
      activeTransaction = underlyingConnection.BeginTransaction(SqlHelper.ReduceIsolationLevel(isolationLevel));
    }

    /// <inheritdoc/>
    protected override void ClearActiveTransaction()
    {
      activeTransaction = null;
    }


    // Constructors

    /// <inheritdoc/>
#if NET40
    [SecuritySafeCritical]
#endif
    public Connection(SqlDriver driver, string connectionString)
      : base(driver, connectionString)
    {
      underlyingConnection = new SQLiteConnection(connectionString);
    }
  }
}