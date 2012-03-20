// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using System.Security;
using Npgsql;
using Xtensive.Core;
using Xtensive.Sql.Info;
using Xtensive.Sql.Drivers.PostgreSql.Resources;

namespace Xtensive.Sql.Drivers.PostgreSql
{
  /// <summary>
  /// A <see cref="SqlDriver"/> factory for PostgreSQL.
  /// </summary>
  public class DriverFactory : SqlDriverFactory
  {
    private const string DataSourceFormat = "{0}:{1}/{2}";
    private const string DatabaseAndSchemaQuery = "select current_database(), current_schema()";

    /// <inheritdoc/>
#if NET40
    [SecuritySafeCritical]
#endif
    protected override string BuildConnectionString(UrlInfo url)
    {
      SqlHelper.ValidateConnectionUrl(url);

      var builder = new NpgsqlConnectionStringBuilder();
      
      // host, port, database
      builder.Host = url.Host;
      if (url.Port!=0)
        builder.Port = url.Port;
      builder.Database = url.Resource ?? string.Empty;

      // user, password
      if (!String.IsNullOrEmpty(url.User)) {
        builder.UserName = url.User;
        builder.Password = url.Password;
      }
      else
        builder.IntegratedSecurity = true;

      // custom options
      foreach (var param in url.Params)
        builder[param.Key] = param.Value;

      return builder.ToString();
    }

    /// <inheritdoc/>
#if NET40
    [SecuritySafeCritical]
#endif
    protected override SqlDriver CreateDriver(string connectionString, string forcedVersion)
    {
      using (var connection = new NpgsqlConnection(connectionString)) {
        connection.Open();
        var version = forcedVersion!=null ? new Version(forcedVersion) : connection.PostgreSqlVersion;
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var dataSource = string.Format(DataSourceFormat, builder.Host, builder.Port, builder.Database);
        var coreServerInfo = new CoreServerInfo {
          ServerVersion = version,
          ConnectionString = connectionString,
          MultipleActiveResultSets = false,
        };
        SqlHelper.ReadDatabaseAndSchema(connection, DatabaseAndSchemaQuery, coreServerInfo);
        if (version.Major < 8 || version.Major==8 && version.Minor < 3)
          throw new NotSupportedException(Strings.ExPostgreSqlBelow83IsNotSupported);
        if (version.Major==8 && version.Minor==3)
          return new v8_3.Driver(coreServerInfo);
        return new v8_4.Driver(coreServerInfo);
      }
    }
  }
}