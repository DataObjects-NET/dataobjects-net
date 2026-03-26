// Copyright (C) 2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2022.02.03

namespace Xtensive.Sql.Drivers.MySql.v8_0
{
  internal class ServerInfoProvider : v5_7.ServerInfoProvider
  {
    // Constructors

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}