// Copyright (C) 2012-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.06.06

using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.PostgreSql.v9_0
{
  internal class ServerInfoProvider : v8_4.ServerInfoProvider
  {
    public override QueryInfo GetQueryInfo()
    {
      var queryInfo = base.GetQueryInfo();
      queryInfo.MaxQueryParameterCount = 65535;
      return queryInfo;
    }

    // Constructors

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}