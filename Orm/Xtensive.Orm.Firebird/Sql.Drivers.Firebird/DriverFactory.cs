// Copyright (C) 2011-2021 Xtensive LLC.
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
      if (configuration.DbConnectionAccessors.Count > 0)
        OpenConnectionWithNotification(connection, configuration, false).GetAwaiter().GetResult();
      else
        OpenConnectionFast(connection, configuration, false).GetAwaiter().GetResult();
      var defaultSchema = GetDefaultSchema(connection);
      return CreateDriverInstance(
        connectionString, GetVersionFromServerVersionString(connection.ServerVersion), defaultSchema);
    }

    /// <inheritdoc/>
    protected override async Task<SqlDriver> CreateDriverAsync(
      string connectionString, SqlDriverConfiguration configuration, CancellationToken token)
    {
      var connection = new FbConnection(connectionString);
      await using (connection.ConfigureAwait(false)) {
        if (configuration.DbConnectionAccessors.Count > 0)
          await OpenConnectionWithNotification(connection, configuration, true, token).ConfigureAwait(false);
        else
          await OpenConnectionFast(connection, configuration, true, token).ConfigureAwait(false);
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

      return coreServerInfo.ServerVersion switch {
        ({ Major: 2 } and { Minor: < 5 }) or { Major: < 2 } => throw new NotSupportedException(Strings.ExFirebirdBelow25IsNotSupported),
        { Major: 2 } and { Minor: 5 } => new v2_5.Driver(coreServerInfo),
        { Major: 4 }                  => new v4_0.Driver(coreServerInfo),
        _ => throw new NotSupportedException()
      };
    }

    /// <inheritdoc/>
    protected override string BuildConnectionString(UrlInfo connectionUrl)
    {
      SqlHelper.ValidateConnectionUrl(connectionUrl);
      ArgumentException.ThrowIfNullOrEmpty(connectionUrl.Resource);
      ArgumentException.ThrowIfNullOrEmpty(connectionUrl.Host);

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

    private static async ValueTask OpenConnectionFast(FbConnection connection,
      SqlDriverConfiguration configuration, bool isAsync, CancellationToken cancellationToken = default)
    {
      if (!isAsync) {
        connection.Open();
        SqlHelper.ExecuteInitializationSql(connection, configuration);
      }
      else {
        await connection.OpenAsync().ConfigureAwait(false);
        await SqlHelper.ExecuteInitializationSqlAsync(connection, configuration, cancellationToken).ConfigureAwait(false);
      }
    }

    private static async ValueTask OpenConnectionWithNotification(FbConnection connection,
      SqlDriverConfiguration configuration, bool isAsync, CancellationToken cancellationToken = default)
    {
      var accessors = configuration.DbConnectionAccessors;
      if (!isAsync) {
        SqlHelper.NotifyConnectionOpening(accessors, connection);
        try {
          connection.Open();
          if (!string.IsNullOrEmpty(configuration.ConnectionInitializationSql))
            SqlHelper.NotifyConnectionInitializing(accessors, connection, configuration.ConnectionInitializationSql);
          SqlHelper.ExecuteInitializationSql(connection, configuration);
          SqlHelper.NotifyConnectionOpened(accessors, connection);
        }
        catch(Exception ex) {
          SqlHelper.NotifyConnectionOpeningFailed(accessors, connection, ex);
          throw;
        }
      }
      else {
        await SqlHelper.NotifyConnectionOpeningAsync(accessors, connection, false, cancellationToken).ConfigureAwait(false);
        try {
          await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

          if (!string.IsNullOrEmpty(configuration.ConnectionInitializationSql)) {
            await SqlHelper.NotifyConnectionInitializingAsync(accessors,
                connection, configuration.ConnectionInitializationSql, false, cancellationToken)
              .ConfigureAwait(false);
          }

          await SqlHelper.ExecuteInitializationSqlAsync(connection, configuration, cancellationToken).ConfigureAwait(false);
          await SqlHelper.NotifyConnectionOpenedAsync(accessors, connection, false, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) {
          await SqlHelper.NotifyConnectionOpeningFailedAsync(accessors, connection, ex, false, cancellationToken).ConfigureAwait(false);
          throw;
        }
      }
    }

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
