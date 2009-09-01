// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kryuchkov
// Created:    2009.07.07

using System;
using Xtensive.Sql.Info;
using SqlServerConnection = System.Data.SqlClient.SqlConnection;

namespace Xtensive.Sql.SqlServer.v2008
{
  internal class ServerInfoProvider : v2005.ServerInfoProvider
  {
    public override IndexInfo GetIndexInfo()
    {
      var result = base.GetIndexInfo();
      result.Features |= IndexFeatures.Filtered;
      return result;
    }


    // Constructors

    public ServerInfoProvider(SqlServerConnection connection, Version version)
      : base(connection, version)
    {
    }
  }
}