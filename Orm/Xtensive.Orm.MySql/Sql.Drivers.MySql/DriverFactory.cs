// Copyright (C) 2011-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Malisa Ncube
// Created:    2011.02.25

using System;
using System.Data.Common;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Xtensive.Orm;
using Xtensive.Sql.Drivers.MySql.Resources;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.MySql
{
  /// <summary>
  /// A <see cref="SqlDriver"/> factory for MySQL.
  /// </summary>
  public class DriverFactory : SqlDriverFactory
  {
    private const string DatabaseAndSchemaQuery = "select database(), schema()";

    /// <inheritdoc/>
    [SecuritySafeCritical]
    protected override string BuildConnectionString(UrlInfo url)
    {
      SqlHelper.ValidateConnectionUrl(url);

      var builder = new MySqlConnectionStringBuilder();

      // host, port, database
      builder.Server = url.Host;
      if (url.Port != 0) {
        builder.Port = (uint) url.Port;
      }

      builder.Database = url.Resource ?? string.Empty;

      // user, password
      if (string.IsNullOrEmpty(url.User)) {
        throw new Exception(Strings.ExUserNameRequired);
      }

      builder.UserID = url.User;
      builder.Password = url.Password;

      // custom options
      foreach (var param in url.Params) {
        builder[param.Key] = param.Value;
      }

      return builder.ToString();
    }

    private static Version ParseVersion(string version)
    {
      // For pre-release versions characters might be used in addition to digits
      // Skip any non-supported chars before creating Version

      var fixedVersion = new string(version.Where(ch => char.IsDigit(ch) || ch=='.').ToArray());
      return new Version(fixedVersion);
    }

    /// <inheritdoc/>
    [SecuritySafeCritical]
    protected override SqlDriver CreateDriver(string connectionString, SqlDriverConfiguration configuration)
    {
      using (var connection = new MySqlConnection(connectionString)) {
        if (configuration.DbConnectionAccessors.Count > 0)
          OpenConnectionWithNotification(connection, configuration, false).GetAwaiter().GetResult();
        else
          OpenConnectionFast(connection, configuration, false).GetAwaiter().GetResult();
        var versionString = string.IsNullOrEmpty(configuration.ForcedServerVersion)
          ? connection.ServerVersion
          : configuration.ForcedServerVersion;
        var version = ParseVersion(versionString);

        var defaultSchema = GetDefaultSchema(connection);
        return CreateDriverInstance(connectionString, version, defaultSchema);
      }
    }

    /// <inheritdoc/>
    protected override async Task<SqlDriver> CreateDriverAsync(
      string connectionString, SqlDriverConfiguration configuration, CancellationToken token)
    {
      var connection = new MySqlConnection(connectionString);
      await using (connection.ConfigureAwait(false)) {
        if (configuration.DbConnectionAccessors.Count > 0)
          await OpenConnectionWithNotification(connection, configuration, true, token).ConfigureAwait(false);
        else
          await OpenConnectionFast(connection, configuration, true, token).ConfigureAwait(false);
        var versionString = string.IsNullOrEmpty(configuration.ForcedServerVersion)
          ? connection.ServerVersion
          : configuration.ForcedServerVersion;
        var version = ParseVersion(versionString);

        var defaultSchema = await GetDefaultSchemaAsync(connection, token: token).ConfigureAwait(false);
        return CreateDriverInstance(connectionString, version, defaultSchema);
      }
    }
    private static SqlDriver CreateDriverInstance(string connectionString, Version version, DefaultSchemaInfo defaultSchema)
    {
      var coreServerInfo = new CoreServerInfo {
        ServerVersion = version,
        ConnectionString = connectionString,
        MultipleActiveResultSets = false,
        DatabaseName = defaultSchema.Database,
        DefaultSchemaName = defaultSchema.Schema,
      };

      if (version.Major < 5) {
        throw new NotSupportedException(Strings.ExMySqlBelow50IsNotSupported);
      }

      return version.Major switch {
        5 when version.Minor == 0 => new v5_0.Driver(coreServerInfo),
        5 when version.Minor == 1 => new v5_1.Driver(coreServerInfo),
        5 when version.Minor == 5 => new v5_5.Driver(coreServerInfo),
        5 when version.Minor == 6 => new v5_6.Driver(coreServerInfo),
        5 when version.Minor == 7 => new v5_7.Driver(coreServerInfo),
        6 or 7 => throw new NotSupportedException(string.Format(Strings.ExVersionXOfMySQLIsNotSupported, version)),
        8 => new v8_0.Driver(coreServerInfo),
        _ => new v8_0.Driver(coreServerInfo)
      };
    }

    /// <inheritdoc/>
    protected override DefaultSchemaInfo ReadDefaultSchema(DbConnection connection, DbTransaction transaction) =>
      SqlHelper.ReadDatabaseAndSchema(DatabaseAndSchemaQuery, connection, transaction);

    /// <inheritdoc/>
    protected override Task<DefaultSchemaInfo> ReadDefaultSchemaAsync(
      DbConnection connection, DbTransaction transaction, CancellationToken token) =>
      SqlHelper.ReadDatabaseAndSchemaAsync(DatabaseAndSchemaQuery, connection, transaction, token);

    private async ValueTask OpenConnectionFast(MySqlConnection connection,
      SqlDriverConfiguration configuration,
      bool isAsync,
      CancellationToken cancellationToken = default)
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

    private async ValueTask OpenConnectionWithNotification(MySqlConnection connection,
      SqlDriverConfiguration configuration,
      bool isAsync,
      CancellationToken cancellationToken = default)
    {
      var acessors = configuration.DbConnectionAccessors;
      if (!isAsync) {
        SqlHelper.NotifyConnectionOpening(acessors, connection);
        try {
          connection.Open();
          if (!string.IsNullOrEmpty(configuration.ConnectionInitializationSql))
            SqlHelper.NotifyConnectionInitializing(acessors, connection, configuration.ConnectionInitializationSql);
          SqlHelper.ExecuteInitializationSql(connection, configuration);
          SqlHelper.NotifyConnectionOpened(acessors, connection);
        }
        catch (Exception ex) {
          SqlHelper.NotifyConnectionOpeningFailed(acessors, connection, ex);
          throw;
        }
      }
      else {
        await SqlHelper.NotifyConnectionOpeningAsync(acessors, connection, false, cancellationToken).ConfigureAwait(false);
        try {
          await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

          if (!string.IsNullOrEmpty(configuration.ConnectionInitializationSql)) {
            await SqlHelper.NotifyConnectionInitializingAsync(acessors,
                connection, configuration.ConnectionInitializationSql, false, cancellationToken)
              .ConfigureAwait(false);
          }

          await SqlHelper.ExecuteInitializationSqlAsync(connection, configuration, cancellationToken).ConfigureAwait(false);
          await SqlHelper.NotifyConnectionOpenedAsync(acessors, connection, false, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) {
          await SqlHelper.NotifyConnectionOpeningFailedAsync(acessors, connection, ex, false, cancellationToken).ConfigureAwait(false);
          throw;
        }
      }
    }

    private void OpenConnectionFast(MySqlConnection connection, SqlDriverConfiguration configuration)
    {
      connection.Open();
      SqlHelper.ExecuteInitializationSql(connection, configuration);
    }

    private void OpenConnectionWithNotification(MySqlConnection connection, SqlDriverConfiguration configuration)
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
  }
}
