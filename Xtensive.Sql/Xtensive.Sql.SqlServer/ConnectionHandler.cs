// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.11

using System;
using System.Data.Common;
using System.Data.SqlClient;
using Xtensive.Core;

namespace Xtensive.Sql.SqlServer
{
  internal class ConnectionHandler : SqlConnectionHandler
  {
    public override DbConnection CreateConnection(UrlInfo url)
    {
      return ConnectionFactory.CreateConnection(Driver, url);
    }

    public override DbParameter CreateParameter()
    {
      return new SqlParameter();
    }

    // Constructors

    public ConnectionHandler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}