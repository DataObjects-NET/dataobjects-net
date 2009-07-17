// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using System.Data.Common;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Oracle
{
  internal abstract class Driver : SqlDriver
  {
    protected override DbConnection CreateNativeConnection(UrlInfo url)
    {
      return ConnectionFactory.CreateConnection(url);
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