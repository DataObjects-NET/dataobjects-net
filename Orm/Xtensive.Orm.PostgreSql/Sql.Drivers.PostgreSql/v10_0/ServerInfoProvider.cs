// Copyright (C) 2019-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.09.25

using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.PostgreSql.v10_0
{
  internal class ServerInfoProvider : v9_0.ServerInfoProvider
  {
    public override QueryInfo GetQueryInfo()
    {
      var info = base.GetQueryInfo();
      info.Features |= QueryFeatures.CrossApplyForSubqueriesOnly;
      return info;
    }

    // Constructors

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}