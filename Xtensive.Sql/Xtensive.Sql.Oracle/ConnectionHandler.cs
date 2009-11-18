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
  internal class ConnectionHandler : SqlConnectionHandler
  {
    public override DbCommand CreateCommand(DbConnection connection)
    {
      return new OracleCommand {Connection = (OracleConnection) connection, BindByName = true};
    }

    public override DbParameter CreateParameter()
    {
      return new OracleParameter();
    }

    public override DbParameter CreateCursorParameter()
    {
      var result = new OracleParameter {
        OracleDbType = OracleDbType.RefCursor,
        Direction = ParameterDirection.Output
      };
      return result;
    }

    public override IBinaryLargeObject CreateBinaryLargeObject(DbConnection connection)
    {
      return new BinaryLargeObject(connection);
    }

    public override ICharacterLargeObject CreateCharacterLargeObject(DbConnection connection)
    {
      return new CharacterLargeObject(connection);
    }

    public override DbConnection CreateConnection(UrlInfo url)
    {
      return ConnectionFactory.CreateConnection(url);
    }

    public override DbTransaction BeginTransaction(DbConnection connection, System.Data.IsolationLevel isolationLevel)
    {
      return connection.BeginTransaction(SqlHelper.ReduceIsolationLevel(isolationLevel));
    }

    // Constructors

    public ConnectionHandler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}