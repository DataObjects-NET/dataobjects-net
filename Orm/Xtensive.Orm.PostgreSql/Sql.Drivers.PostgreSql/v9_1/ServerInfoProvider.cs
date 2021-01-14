// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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