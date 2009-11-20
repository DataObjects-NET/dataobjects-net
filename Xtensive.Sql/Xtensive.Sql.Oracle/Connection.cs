// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.11

using Oracle.DataAccess.Client;
using System.Data;
using System.Data.Common;
using Xtensive.Core;

namespace Xtensive.Sql.Oracle
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
      return new BinaryLargeObject(underlyingConnection);
    }

    /// <inheritdoc/>
    public override ICharacterLargeObject CreateCharacterLargeObject()
    {
      return new CharacterLargeObject(underlyingConnection);
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
      activeTransaction = underlyingConnection.BeginTransaction(SqlHelper.ReduceIsolationLevel(isolationLevel));
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
    protected override void ClearActiveTransaction()
    {
      activeTransaction = null;
    }

    /// <inheritdoc/>
    protected override DbCommand CreateNativeCommand()
    {
      return new OracleCommand {Connection = underlyingConnection, BindByName = true};
    }


    // Constructors

    public Connection(SqlDriver driver, UrlInfo url)
      : base(driver, url)
    {
      underlyingConnection = ConnectionFactory.CreateConnection(url);
    }
  }
}