// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.16

using System;
using System.Linq;
using Oracle.DataAccess.Client;
using Xtensive.Core;
using Xtensive.Sql.Info;
using Xtensive.Sql.Oracle.Resources;

namespace Xtensive.Sql.Oracle
{
  /// <summary>
  /// A <see cref="SqlDriver"/> factory for Oracle.
  /// </summary>
  public class DriverFactory : SqlDriverFactory
  {
    private const string DatabaseAndSchemaQuery =
      "select sys_context('USERENV', 'DB_NAME'), sys_context('USERENV', 'CURRENT_SCHEMA') from dual";

    public override SqlDriver CreateDriver(ConnectionInfo connectionInfo)
    {
      var connectionString = ConnectionStringBuilder.Build(connectionInfo);
      using (var connection = new OracleConnection(connectionString)) {
        connection.Open();
        var version = ParseVersion(connection.ServerVersion);
        var coreServerInfo = new CoreServerInfo {
          ConnectionString = connectionString,
          ServerVersion = version,
          MultipleActiveResultSets = true,
        };
        SqlHelper.ReadDatabaseAndSchema(connection, DatabaseAndSchemaQuery, coreServerInfo);
        if (version.Major < 9 || version.Major==9 && version.Minor < 2)
          throw new NotSupportedException(Strings.ExOracleBelow9i2IsNotSupported);
        if (version.Major==9)
          return new v09.Driver(coreServerInfo);
        if (version.Major==10)
          return new v10.Driver(coreServerInfo);
        return new v11.Driver(coreServerInfo);
      }
    }

    private static Version ParseVersion(string version)
    {
      var items = version.Split('.').Take(4).Select(item => int.Parse(item)).ToArray();
      return new Version(items[0], items[1], items[2], items[3]);
    }
  }
}