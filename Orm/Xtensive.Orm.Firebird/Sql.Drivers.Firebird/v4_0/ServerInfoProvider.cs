// Copyright (C) 2021-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.Firebird.v4_0
{
  internal class ServerInfoProvider : v2_5.ServerInfoProvider
  {
    public override QueryInfo GetQueryInfo()
    {
      var info = base.GetQueryInfo();
      info.Features |= QueryFeatures.CrossApplyForSubqueriesOnly;
      return info;
    }

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
