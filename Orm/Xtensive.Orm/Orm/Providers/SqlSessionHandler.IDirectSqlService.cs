// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    void IDirectSqlService.RegisterInitializationSql(string sql)
    {
      ArgumentNullException.ThrowIfNull(sql, "sql");
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