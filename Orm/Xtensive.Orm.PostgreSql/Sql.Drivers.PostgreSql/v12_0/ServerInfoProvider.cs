// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.PostgreSql.v12_0
{
  internal class ServerInfoProvider : v10_0.ServerInfoProvider
  {
    protected override IndexFeatures GetIndexFeatures() => base.GetIndexFeatures() | IndexFeatures.NonKeyColumns;

    // Constructors

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}