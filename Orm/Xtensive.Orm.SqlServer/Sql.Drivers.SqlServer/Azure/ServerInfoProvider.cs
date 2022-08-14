// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2009.11.09

using SqlServerConnection = Microsoft.Data.SqlClient.SqlConnection;

namespace Xtensive.Sql.Drivers.SqlServer.Azure
{
  internal class ServerInfoProvider : v12.ServerInfoProvider
  {
    // Constructors

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}