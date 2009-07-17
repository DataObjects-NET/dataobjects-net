// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using Oracle.DataAccess.Client;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Oracle.v10
{
  internal class ServerInfoProvider : v09.ServerInfoProvider
  {
    // Constructors

    public ServerInfoProvider(OracleConnection connection, Version version)
      : base(connection, version)
    {
    }
  }
}