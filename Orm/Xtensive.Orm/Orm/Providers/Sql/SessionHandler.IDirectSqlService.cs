// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.09

using System;
using System.Data.Common;

namespace Xtensive.Orm.Providers.Sql
{
  public partial class SessionHandler
  {
    // Implementation of IDirectSqlService

    
    DbConnection IDirectSqlService.Connection {
      get {
        var sqlConnection = Connection;
        return sqlConnection==null ? null : sqlConnection.UnderlyingConnection;
      }
    }

    
    DbTransaction IDirectSqlService.Transaction {
      get {
        var sqlConnection = Connection;
        return sqlConnection==null ? null : sqlConnection.ActiveTransaction;
      }
    }

    
    /// <exception cref="InvalidOperationException">Connection is not open.</exception>
    DbCommand IDirectSqlService.CreateCommand()
    {
      var sqlConnection = Connection;
      if (sqlConnection==null)
        throw new InvalidOperationException(Strings.ExConnectionIsNotOpen);
      var dbCommand = sqlConnection.CreateCommand();
      if (Session.CommandTimeout!=null)
        dbCommand.CommandTimeout = Session.CommandTimeout.Value;
      return dbCommand;
    }
  }
}