// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.06.06

using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.PostgreSql.v9_0
{
  internal class ServerInfoProvider : v8_4.ServerInfoProvider
  {
    // Constructors

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}