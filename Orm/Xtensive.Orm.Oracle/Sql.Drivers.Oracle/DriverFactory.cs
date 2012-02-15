// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.16

using System;
using System.Linq;
using Oracle.DataAccess.Client;
using Xtensive.Core;
using Xtensive.Sql.Info;
using Xtensive.Sql.Drivers.Oracle.Resources;

namespace Xtensive.Sql.Drivers.Oracle
{
  /// <summary>
  /// A <see cref="SqlDriver"/> factory for Oracle.
  /// </summary>
  public class DriverFactory : SqlDriverFactory
  {
    private const int DefaultPort = 1521;
    private const string DataSourceFormat =
      "(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1}))(CONNECT_DATA=(SERVICE_NAME={2})))";
    private const string DatabaseAndSchemaQuery =
      "select sys_context('USERENV', 'DB_NAME'), sys_context('USERENV', 'CURRENT_SCHEMA') from dual";

    private static Version ParseVersion(string version)
    {
      var items = version.Split('.').Take(4).Select(item => int.Parse(item)).ToArray();
      return new Version(items[0], items[1], items[2], items[3]);
    }

    protected override string BuildConnectionString(UrlInfo url)
    {
      SqlHelper.ValidateConnectionUrl(url);
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(url.Resource, "url.Resource");

      var builder = new OracleConnectionStringBuilder();

      // host, port, database
      if (!string.IsNullOrEmpty(url.Host)) {
        int port = url.Port!=0 ? url.Port : DefaultPort;
        builder.DataSource = string.Format(DataSourceFormat, url.Host, port, url.Resource);
      }
      else
        builder.DataSource = url.Resource; // Plain TNS name

      // user, password
      if (!string.IsNullOrEmpty(url.User)) {
        builder.UserID = url.User;
        builder.Password = url.Password;
      }
      else
        builder.UserID = "/";

      // custom options
      foreach (var parameter in url.Params)
        builder.Add(parameter.Key, parameter.Value);

      return builder.ToString();
    }
    
    /// <inheritdoc/>
    protected override SqlDriver CreateDriver(string connectionString)
    {
      using (var connection = new OracleConnection(connectionString)) {
        connection.Open();
        var version = ParseVersion(connection.ServerVersion);
        var dataSource = new OracleConnectionStringBuilder(connectionString).DataSource;
        var coreServerInfo = new CoreServerInfo {
          ServerVersion = version,
          ConnectionString = connectionString,
          MultipleActiveResultSets = true,
        };
        SqlHelper.ReadDatabaseAndSchema(connection, DatabaseAndSchemaQuery, coreServerInfo);
        if (version.Major < 10)
          throw new NotSupportedException(Strings.ExOracleBelow10gIsNotSupported);
        if (version.Major==10)
          return new v10.Driver(coreServerInfo);
        return new v11.Driver(coreServerInfo);
      }
    }
  }
}