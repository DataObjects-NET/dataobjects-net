// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.11

using Npgsql;
using System.Data;
using System.Data.Common;
using Xtensive.Core;

namespace Xtensive.Sql.PostgreSql
{
  internal class Connection : SqlConnection
  {
    private NpgsqlConnection underlyingConnection;
    private NpgsqlTransaction activeTransaction;
    
    /// <inheritdoc/>
    public override DbConnection UnderlyingConnection { get { return underlyingConnection; } }

    /// <inheritdoc/>
    public override DbTransaction ActiveTransaction { get { return activeTransaction; } }
    
    /// <inheritdoc/>
    public override DbParameter CreateParameter()
    {
      return new NpgsqlParameter();
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
      var commandText = string.Format("SAVEPOINT {0}", name);
      using (var command = CreateCommand(commandText))
        command.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public override void RollbackToSavepoint(string name)
    {
      EnsureTransactionIsActive();
      var commandText = string.Format("ROLLBACK TO SAVEPOINT {0}; RELEASE SAVEPOINT {0};", name);
 	    using (var command = CreateCommand(commandText))
 	      command.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public override void ReleaseSavepoint(string name)
    {
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


    // Constructors

    public Connection(SqlDriver driver, UrlInfo url)
      : base(driver, url)
    {
      underlyingConnection = ConnectionFactory.CreateConnection(url);
    }
  }
}