// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kryuchkov
// Created:    2009.07.07

using SqlServerConnection = System.Data.SqlClient.SqlConnection;

namespace Xtensive.Sql.SqlServer.v2008
{
  internal class ServerInfoProvider : v2005.ServerInfoProvider
  {
    // Constructors

    public ServerInfoProvider(SqlServerConnection connection)
      : base(connection)
    {
    }
  }
}