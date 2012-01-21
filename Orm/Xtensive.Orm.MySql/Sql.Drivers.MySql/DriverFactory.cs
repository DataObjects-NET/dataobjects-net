// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.02.25

using System;
using System.Security;
using MySql.Data.MySqlClient;
using Xtensive.Core;
using Xtensive.Sql.Drivers.MySql.Resources;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.MySql
{
  /// <summary>
  /// A <see cref="SqlDriver"/> factory for MySQL.
  /// </summary>
  public class DriverFactory : SqlDriverFactory
  {
    private const string DataSourceFormat = "{0}:{1}/{2}";
    private const string DatabaseAndSchemaQuery = "select database(), schema()";

    /// <inheritdoc/>
#if NET40
    [SecuritySafeCritical]
#endif
    public override string BuildConnectionString(UrlInfo url)
    {
      SqlHelper.ValidateConnectionUrl(url);

      var builder = new MySqlConnectionStringBuilder();

      // host, port, database
      builder.Server = url.Host;
      if (url.Port != 0)
        builder.Port = (uint) url.Port;
      builder.Database = url.Resource ?? string.Empty;

      // user, password
      if (!String.IsNullOrEmpty(url.User)) {
        builder.UserID = url.User;
        builder.Password = url.Password;
      }
      else
        throw new Exception(Strings.ExUserNameRequired);

      // custom options
      foreach (var param in url.Params)
        builder[param.Key] = param.Value;

      return builder.ToString();
    }

    /// <inheritdoc/>
#if NET40
    [SecuritySafeCritical]
#endif
    protected override SqlDriver CreateDriver(string connectionString)
    {
      using (var connection = new MySqlConnection(connectionString)) {
        connection.Open();
        var version = new Version(connection.ServerVersion);

        var builder = new MySqlConnectionStringBuilder(connectionString);
        string dataSource = string.Format(DataSourceFormat, builder.Server, builder.Port, builder.Database);
        var coreServerInfo = new CoreServerInfo
                               {
                                 ServerVersion = version,
                                 ConnectionString = connectionString,
                                 MultipleActiveResultSets = false,
                               };

        SqlHelper.ReadDatabaseAndSchema(connection, DatabaseAndSchemaQuery, coreServerInfo);
        if (version.Major < 5)
          throw new NotSupportedException(Strings.ExMySqlBelow50IsNotSupported);
        if (version.Major == 5 && version.Minor == 0)
          return new v5_0.Driver(coreServerInfo);
        if (version.Major == 5 && version.Minor == 1)
          return new v5_1.Driver(coreServerInfo);
        if (version.Major == 5 && version.Minor == 5)
          return new v5_5.Driver(coreServerInfo);
        return new v5_0.Driver(coreServerInfo);
      }
    }
  }
}