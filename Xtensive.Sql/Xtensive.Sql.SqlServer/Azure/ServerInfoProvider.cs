// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.11.09

using System;
using SqlServerConnection = System.Data.SqlClient.SqlConnection;

namespace Xtensive.Sql.SqlServer.Azure
{
  internal class ServerInfoProvider : v10.ServerInfoProvider
  {
    public override bool GetMultipleActiveResultSets()
    {
      return false;
    }

    // Constructors

    public ServerInfoProvider(SqlServerConnection connection, Version version)
      : base(connection, version)
    {
    }
  }
}