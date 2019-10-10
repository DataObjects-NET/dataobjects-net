// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2019.10.10

using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.PostgreSql.v9_1
{
  internal class ServerInfoProvider : v9_0.ServerInfoProvider
  {
    // Constructors

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}