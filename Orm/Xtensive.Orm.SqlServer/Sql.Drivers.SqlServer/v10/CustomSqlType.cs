// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.04.01

namespace Xtensive.Sql.Drivers.SqlServer.v10
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
