// Copyright (C) 2020-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.PostgreSql.v12
{
  internal class ServerInfoProvider : v11.ServerInfoProvider
  {
    protected override IndexFeatures GetIndexFeatures() => base.GetIndexFeatures() | IndexFeatures.NonKeyColumns;

    // Constructors

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}