// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using System.Data.SqlClient;
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
    private const string DataSourceFormat = "{0}/{1}";
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
    public override string BuildConnectionString(UrlInfo url)
    {
      SqlHelper.ValidateConnectionUrl(url);

      var builder = new SqlConnectionStringBuilder();
      
      // host, port, database
      if (url.Port==0)
        builder.DataSource = url.Host;
      else
        builder.DataSource = url.Host + "," + url.Port;
      builder.InitialCatalog = url.Resource ?? string.Empty;

      // user, password
      if (!String.IsNullOrEmpty(url.User)) {
        builder.UserID = url.User;
        builder.Password = url.Password;
      }
      else {
        builder.IntegratedSecurity = true;
        builder.PersistSecurityInfo = false;
      }

      // custom options
      foreach (var param in url.Params)
        builder[param.Key] = param.Value;

      return builder.ToString();
    }

    /// <inheritdoc/>
    public override SqlDriver CreateDriver(string connectionString)
    {
      using (var connection = new SqlServerConnection(connectionString)) {
        connection.Open();
        var version = new Version(connection.ServerVersion);
        var builder = new SqlConnectionStringBuilder(connectionString);
        var dataSource = string.Format(DataSourceFormat, builder.DataSource, builder.InitialCatalog);
        var coreServerInfo = new CoreServerInfo {
          ServerLocation = new Location("sqlserver", dataSource),
          ServerVersion = version,
          ConnectionString = connectionString,
        };
        SqlHelper.ReadDatabaseAndSchema(connection, DatabaseAndSchemaQuery, coreServerInfo);
        coreServerInfo.MultipleActiveResultSets = builder.MultipleActiveResultSets;
        if (IsAzure(connection))
          return new Azure.Driver(coreServerInfo);
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