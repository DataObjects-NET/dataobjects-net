// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using Npgsql;
using Xtensive.Core;
using Xtensive.Sql.Info;
using Xtensive.Sql.PostgreSql.Resources;

namespace Xtensive.Sql.PostgreSql
{
  /// <summary>
  /// A <see cref="SqlDriver"/> factory for PostgreSQL.
  /// </summary>
  public class DriverFactory : SqlDriverFactory
  {
    private const string DatabaseAndSchemaQuery = "select current_database(), current_schema()";

    /// <inheritdoc/>
    public override SqlDriver CreateDriver(ConnectionInfo connectionInfo)
    {
      var connectionString = ConnectionStringBuilder.Build(connectionInfo);
      using (var connection = new NpgsqlConnection(connectionString)) {
        connection.Open();
        var version = connection.PostgreSqlVersion;
        var coreServerInfo = new CoreServerInfo {
          ConnectionString = connectionString,
          ServerVersion = version,
          MultipleActiveResultSets = false,
        };
        SqlHelper.ReadDatabaseAndSchema(connection, DatabaseAndSchemaQuery, coreServerInfo);
        if (version.Major < 8)
          throw new NotSupportedException(Strings.ExPostgreSqlBelow80IsNotSupported);
        if (version.Major==8 && version.Minor==0)
          return new v8_0.Driver(coreServerInfo);
        if (version.Major==8 && version.Minor==1)
          return new v8_1.Driver(coreServerInfo);
        if (version.Major==8 && version.Minor==2)
          return new v8_2.Driver(coreServerInfo);
        if (version.Major==8 && version.Minor==3)
          return new v8_3.Driver(coreServerInfo);
        return new v8_4.Driver(coreServerInfo);
      }
    }
  }
}