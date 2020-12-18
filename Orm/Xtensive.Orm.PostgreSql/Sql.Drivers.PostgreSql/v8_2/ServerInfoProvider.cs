// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_2
{
  internal class ServerInfoProvider : v8_1.ServerInfoProvider
  {
    protected override IndexFeatures GetIndexFeatures() => base.GetIndexFeatures() | IndexFeatures.FillFactor;


    // Constructors

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}