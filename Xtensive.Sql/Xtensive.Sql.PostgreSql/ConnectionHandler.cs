// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.11

using System;
using System.Data;
using System.Data.Common;
using Xtensive.Core;
using Xtensive.Sql.PostgreSql.Resources;

namespace Xtensive.Sql.PostgreSql
{
  internal class ConnectionHandler : SqlConnectionHandler
  {
    public override DbConnection CreateConnection(UrlInfo url)
    {
      return ConnectionFactory.CreateConnection(url);
    }

    public override DbTransaction BeginTransaction(DbConnection connection, IsolationLevel isolationLevel)
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