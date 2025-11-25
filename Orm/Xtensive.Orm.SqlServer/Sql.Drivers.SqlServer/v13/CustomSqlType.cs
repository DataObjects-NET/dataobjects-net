// Copyright (C) 2014-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alena Mikshina
// Created:    2014.04.01

namespace Xtensive.Sql.Drivers.SqlServer.v13
{
  /// <summary>
  /// Contains additional SQL column types supported by the MS SQL Server RDBMS.
  /// </summary>
  public static class CustomSqlType
  {
    /// <summary>
    /// Geometry, like in Microsoft.SqlServer.Types
    /// </summary>
    public static readonly SqlType Geometry = new SqlType("Geometry");

    /// <summary>
    /// Geography, like in Microsoft.SqlServer.Types
    /// </summary>
    public static readonly SqlType Geography = new SqlType("Geography");
  }
}
