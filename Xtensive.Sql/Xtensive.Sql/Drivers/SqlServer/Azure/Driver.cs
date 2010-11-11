// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.07

using Xtensive.Sql.Info;
using SqlServerConnection = System.Data.SqlClient.SqlConnection;

namespace Xtensive.Sql.SqlServer.Azure
{
  internal class Driver : v10.Driver
  {
    protected override Model.Extractor CreateExtractor()
    {
      return new Extractor(this);
    }

    protected override Info.ServerInfoProvider CreateServerInfoProvider()
    {
      return new ServerInfoProvider(this);
    }

    // Constructors

    public Driver(CoreServerInfo coreServerInfo)
      : base(coreServerInfo)
    {
    }
  }
}