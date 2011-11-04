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

namespace Xtensive.Sql.PostgreSql
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
    public override string BuildConnectionString(UrlInfo url)
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
    public override SqlDriver CreateDriver(string connectionString)
    {
      using (var connection = new NpgsqlConnection(connectionString)) {
        connection.Open();
        var version = connection.PostgreSqlVersion;
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var dataSource = string.Format(DataSourceFormat, builder.Host, builder.Port, builder.Database);
        var coreServerInfo = new CoreServerInfo {
          ServerLocation = new Location("postgresql", dataSource),
          ServerVersion = version,
          ConnectionString = connectionString,
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