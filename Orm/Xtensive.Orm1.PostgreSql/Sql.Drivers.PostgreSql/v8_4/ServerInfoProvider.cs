// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kryuchkov
// Created:    2009.07.07

using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_4
{
  internal class ServerInfoProvider : v8_3.ServerInfoProvider
  {
    public override QueryInfo GetQueryInfo()
    {
      var queryInfo = base.GetQueryInfo();
      queryInfo.Features |= QueryFeatures.RowNumber;
      return queryInfo;
    }

    // Constructors

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}