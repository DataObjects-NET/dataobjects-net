// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.11

using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Data.Common;
using Xtensive.Orm;

namespace Xtensive.Sql.Drivers.Oracle
{
  internal class Connection : SqlConnection
  {
    private OracleConnection underlyingConnection;
    private OracleTransaction activeTransaction;

    /// <inheritdoc/>
    public override DbConnection UnderlyingConnection { get { return underlyingConnection; } }

    /// <inheritdoc/>
    public override DbTransaction ActiveTransaction { get { return activeTransaction; } }

    /// <inheritdoc/>
    public override DbParameter CreateParameter()
    {
      return new OracleParameter();
    }

    /// <inheritdoc/>
    public override DbParameter CreateCursorParameter()
    {
      var result = new OracleParameter {
        OracleDbType = OracleDbType.RefCursor,
        Direction = ParameterDirection.Output
      };
      return result;
    }

    /// <inheritdoc/>
    public override IBinaryLargeObject CreateBinaryLargeObject()
    {
      EnsureIsNotDisposed();
      return new BinaryLargeObject(underlyingConnection);
    }

    /// <inheritdoc/>
    public override ICharacterLargeObject CreateCharacterLargeObject()
    {
      EnsureIsNotDisposed();
      return new CharacterLargeObject(underlyingConnection);
    }

    /// <inheritdoc/>
    public override void BeginTransaction()
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsNotActive();
      activeTransaction = underlyingConnection.BeginTransaction();
    }

    /// <inheritdoc/>
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
      activeTransaction.Save(name);
    }

    /// <inheritdoc/>
    public override void RollbackToSavepoint(string name)
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsActive();
      activeTransaction.Rollback(name);
    }

    /// <inheritdoc/>
    public override void ReleaseSavepoint(string name)
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsActive();
      // nothing
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

    /// <inheritdoc/>
    protected override DbCommand CreateNativeCommand()
    {
      return new OracleCommand {Connection = underlyingConnection, BindByName = true};
    }

    // Constructors

    public Connection(SqlDriver driver)
      : base(driver)
    {
      underlyingConnection = new OracleConnection();
    }
  }
}