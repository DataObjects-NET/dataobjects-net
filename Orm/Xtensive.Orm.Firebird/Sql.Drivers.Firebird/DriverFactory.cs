// Copyright (C) 2011-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Csaba Beer
// Created:    2011.01.08

using System;
using System.Data.Common;
using System.Text.RegularExpressions;
using FirebirdSql.Data.FirebirdClient;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Sql.Drivers.Firebird.Resources;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.Firebird
{
  /// <summary>
  /// A <see cref="SqlDriver"/> factory for Firebird.
  /// </summary>
  public class DriverFactory : SqlDriverFactory
  {
    private const int DefaultPort = 3050;

    private const string DataSourceFormat =
      "server={0};port={1};database={2};";

    private const string DatabaseAndSchemaQuery =
      "select mon$database_name, '" + Constants.DefaultSchemaName + "' from mon$database";

    private const string ServerVersionParser = @"\d{1,3}\.\d{1,3}(?:\.\d{1,6})+";

    /// <inheritdoc/>
    protected override SqlDriver CreateDriver(string connectionString, SqlDriverConfiguration configuration)
    {
      using (var connection = new FbConnection(connectionString)) {
        if (configuration.DbConnectionAccessors.Count > 0)
          OpenConnectionWithNotification(connection, configuration);
        else
          OpenConnectionFast(connection, configuration);
        var defaultSchema = GetDefaultSchema(connection);
        return CreateDriverInstance(
          connectionString, GetVersionFromServerVersionString(connection.ServerVersion), defaultSchema);
      }
    }

    private static SqlDriver CreateDriverInstance(
      string connectionString, Version version, DefaultSchemaInfo defaultSchema)
    {
      var coreServerInfo = new CoreServerInfo {
        ServerVersion = version,
        ConnectionString = connectionString,
        MultipleActiveResultSets = true,
        DatabaseName = defaultSchema.Database,
        DefaultSchemaName = defaultSchema.Schema,
      };

      if (coreServerInfo.ServerVersion < new Version(2, 5)) {
        throw new NotSupportedException(Strings.ExFirebirdBelow25IsNotSupported);
      }

      if (coreServerInfo.ServerVersion.Major == 2 && coreServerInfo.ServerVersion.Minor == 5) {
        return new v3_0.Driver(coreServerInfo);
      }
      if (coreServerInfo.ServerVersion.Major == 3)
        return new v3_0.Driver(coreServerInfo);

      return null;
    }

    /// <inheritdoc/>
    protected override string BuildConnectionString(UrlInfo connectionUrl)
    {
      SqlHelper.ValidateConnectionUrl(connectionUrl);
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(connectionUrl.Resource, "connectionUrl.Resource");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(connectionUrl.Host, "connectionUrl.Host");

      var builder = new FbConnectionStringBuilder();

      // host, port, database
      if (!string.IsNullOrEmpty(connectionUrl.Host)) {
        int port = connectionUrl.Port!=0 ? connectionUrl.Port : DefaultPort;
        //                builder.DataSource = string.Format(DataSourceFormat, connectionUrl.Host, port, connectionUrl.Resource);
        builder.Database = connectionUrl.Resource;
        builder.DataSource = connectionUrl.Host;
        builder.Dialect = 3;
        builder.Pooling = false;
        builder.Port = port;
        builder.ReturnRecordsAffected = true;
      }

      // user, password
      if (!string.IsNullOrEmpty(connectionUrl.User)) {
        builder.UserID = connectionUrl.User;
        builder.Password = connectionUrl.Password;
      }

      // custom options
      foreach (var parameter in connectionUrl.Params)
        builder.Add(parameter.Key, parameter.Value);

      return builder.ToString();
    }

    /// <inheritdoc/>
    protected override DefaultSchemaInfo ReadDefaultSchema(DbConnection connection, DbTransaction transaction)
    {
      return SqlHelper.ReadDatabaseAndSchema(DatabaseAndSchemaQuery, connection, transaction);
    }

    private void OpenConnectionFast(FbConnection connection, SqlDriverConfiguration configuration)
    {
      connection.Open();
      SqlHelper.ExecuteInitializationSql(connection, configuration);
    }

    private void OpenConnectionWithNotification(FbConnection connection, SqlDriverConfiguration configuration)
    {
      var accessors = configuration.DbConnectionAccessors;
      SqlHelper.NotifyConnectionOpening(accessors, connection);
      try {
        connection.Open();
        if (!string.IsNullOrEmpty(configuration.ConnectionInitializationSql))
          SqlHelper.NotifyConnectionInitializing(accessors, connection, configuration.ConnectionInitializationSql);
        SqlHelper.ExecuteInitializationSql(connection, configuration);
        SqlHelper.NotifyConnectionOpened(accessors, connection);
      }
      catch (Exception ex) {
        SqlHelper.NotifyConnectionOpeningFailed(accessors, connection, ex);
        throw;
      }
    }

    private Version GetVersionFromServerVersionString(string serverVersionString)
    {
      var matcher = new Regex(ServerVersionParser);
      var match = matcher.Match(serverVersionString);
      if (!match.Success)
        throw new InvalidOperationException("Unable to parse server version");
      return new Version(match.Value);
    }
  }
}