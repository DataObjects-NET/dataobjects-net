// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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