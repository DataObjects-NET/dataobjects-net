// Copyright (C) 2011-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Csaba Beer
// Created:    2011.01.08

using System;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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

    private const string DatabaseAndSchemaQuery =
      "select mon$database_name, '" + Constants.DefaultSchemaName + "' from mon$database";

    private const string ServerVersionParser = @"\d{1,3}\.\d{1,3}(?:\.\d{1,6})+";

    /// <inheritdoc/>
    protected override SqlDriver CreateDriver(string connectionString, SqlDriverConfiguration configuration)
    {
      using var connection = new FbConnection(connectionString);
      connection.Open();
      SqlHelper.ExecuteInitializationSql(connection, configuration);
      var defaultSchema = GetDefaultSchema(connection);
      return CreateDriverInstance(
        connectionString, GetVersionFromServerVersionString(connection.ServerVersion), defaultSchema);
    }

    protected override async Task<SqlDriver> CreateDriverAsync(
      string connectionString, SqlDriverConfiguration configuration, CancellationToken token)
    {
      var connection = new FbConnection(connectionString);
      await using (connection.ConfigureAwait(false)) {
        await connection.OpenAsync(token).ConfigureAwait(false);
        await SqlHelper.ExecuteInitializationSqlAsync(connection, configuration, token).ConfigureAwait(false);
        var defaultSchema = await GetDefaultSchemaAsync(connection, token: token).ConfigureAwait(false);
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
        return new v2_5.Driver(coreServerInfo);
      }

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
        var port = connectionUrl.Port!=0 ? connectionUrl.Port : DefaultPort;
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
      foreach (var parameter in connectionUrl.Params) {
        builder.Add(parameter.Key, parameter.Value);
      }

      return builder.ToString();
    }

    /// <inheritdoc/>
    protected override DefaultSchemaInfo ReadDefaultSchema(DbConnection connection, DbTransaction transaction) =>
      SqlHelper.ReadDatabaseAndSchema(DatabaseAndSchemaQuery, connection, transaction);

    /// <inheritdoc/>
    protected override  Task<DefaultSchemaInfo> ReadDefaultSchemaAsync(
      DbConnection connection, DbTransaction transaction, CancellationToken token) =>
      SqlHelper.ReadDatabaseAndSchemaAsync(DatabaseAndSchemaQuery, connection, transaction, token);

    private static Version GetVersionFromServerVersionString(string serverVersionString)
    {
      var matcher = new Regex(ServerVersionParser);
      var match = matcher.Match(serverVersionString);
      if (!match.Success) {
        throw new InvalidOperationException("Unable to parse server version");
      }

      return new Version(match.Value);
    }
  }
}
