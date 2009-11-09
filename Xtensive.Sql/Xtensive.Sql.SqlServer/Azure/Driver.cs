// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.07

using System;
using SqlServerConnection = System.Data.SqlClient.SqlConnection;

namespace Xtensive.Sql.SqlServer.Azure
{
  internal class Driver : v10.Driver
  {

    // Constructors

    public Driver(SqlServerConnection connection, Version version)
      : base(new ServerInfoProvider(connection, version))
    {
    }
  }
}