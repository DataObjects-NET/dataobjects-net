// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System.Data.Common;
using Xtensive.Core;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.SqlServer
{
  internal abstract class Driver : SqlDriver
  {
    protected override DbConnection CreateNativeConnection(UrlInfo sqlConnectionUrl)
    {
      return ConnectionFactory.CreateConnection(sqlConnectionUrl);
    }

    protected override ValueTypeMapping.DataAccessHandler CreateDataAccessHandler()
    {
      return new DataAccessHandler(this);
    }

    // Constructors

    protected Driver(ServerInfoProvider serverInfoProvider)
      : base(serverInfoProvider)
    {
    }
  }
}