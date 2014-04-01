// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.09

using System;
using System.Data.Common;
using Xtensive.Core;

namespace Xtensive.Orm.Providers
{
  public partial class SqlSessionHandler
  {
    // Implementation of IDirectSqlService

    ConnectionInfo IDirectSqlService.ConnectionInfo {
      get { return connection.ConnectionInfo; }
      set { connection.ConnectionInfo = value; }
    }

    /// <inheritdoc/>
    DbConnection IDirectSqlService.Connection {
      get {
        Prepare();
        return connection.UnderlyingConnection;
      }
    }

    /// <inheritdoc/>
    DbTransaction IDirectSqlService.Transaction {
      get {
        Prepare();
        return connection.ActiveTransaction;
      }
    }

    void IDirectSqlService.RegisterInitializationSql(string sql)
    {
      ArgumentValidator.EnsureArgumentNotNull(sql, "sql");
      initializationSqlScripts.Add(sql);
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Connection is not open.</exception>
    DbCommand IDirectSqlService.CreateCommand()
    {
      Prepare();
      return connection.CreateCommand();
    }
  }
}