// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using System.Text.RegularExpressions;
using Xtensive.Core;
using Xtensive.Sql.Info;
using Xtensive.Sql.SqlServer.Resources;
using SqlServerConnection = System.Data.SqlClient.SqlConnection;

namespace Xtensive.Sql.SqlServer
{
  /// <summary>
  /// A <see cref="SqlDriver"/> factory for Microsoft SQL Server.
  /// </summary>
  public class DriverFactory : SqlDriverFactory
  {
    // Some people, when confronted with a problem, think
    // "I know, I'll use regular expressions." Now they have two problems.
    private static readonly Regex MarsParameterChecker = new Regex(
      @"MultipleActiveResultSets\ *=\ *True",
      RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private const string DatabaseAndSchemaQuery =
      "select db_name(), default_schema_name from sys.database_principals where name=user";

    private static bool IsAzure(SqlServerConnection connection)
    {
      using (var command = connection.CreateCommand()) {
        command.CommandText = "SELECT @@VERSION";
        return ((string) command.ExecuteScalar()).Contains("Azure");
      }
    }

    /// <inheritdoc/>
    public override SqlDriver CreateDriver(ConnectionInfo connectionInfo)
    {
      var connectionString = ConnectionStringBuilder.Build(connectionInfo);
      using (var connection = new SqlServerConnection(connectionString)) {
        connection.Open();
        var version = new Version(connection.ServerVersion);
        var coreServerInfo = new CoreServerInfo {
          ConnectionString = connectionString,
          ServerVersion = version
        };
        SqlHelper.ReadDatabaseAndSchema(connection, DatabaseAndSchemaQuery, coreServerInfo);
        if (IsAzure(connection)) {
          coreServerInfo.MultipleActiveResultSets = false;
          return new Azure.Driver(coreServerInfo);
        }
        coreServerInfo.MultipleActiveResultSets = MarsParameterChecker.IsMatch(connectionString);
        switch (version.Major) {
        case 9:
          return new v09.Driver(coreServerInfo);
        case 10:
          return new v10.Driver(coreServerInfo);
        default:
          throw new NotSupportedException(Strings.ExSqlServerBelow2005IsNotSupported);
        }
      }
    }
  }
}