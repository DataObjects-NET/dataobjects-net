// Copyright (C) 2011-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Malisa Ncube
// Created:    2011.04.29

using System;
using System.Data.Common;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Orm;
using Xtensive.Sql.Drivers.Sqlite.Resources;
using Xtensive.Sql.Drivers.Sqlite.v3;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.Sqlite
{
  /// <summary>
  /// A <see cref="SqlDriver"/> factory for SQLite.
  /// </summary>
  public class DriverFactory : SqlDriverFactory
  {
    private static readonly Regex DataSourceExtractor = new Regex(@"(.*Data Source *= *)(.*)($|;.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static string GetDataSource(string connectionString)
    {
      var match = DataSourceExtractor.Match(connectionString);
      if (!match.Success) {
        return "<unknown>";
      }

      var dataSource = match.Groups[2].Captures[0].Value;
      if (dataSource.Length > 1 && dataSource.StartsWith("'") && dataSource.EndsWith("'")) {
        dataSource = dataSource.Substring(1, dataSource.Length - 2);
      }

      return dataSource;
    }

    /// <inheritdoc/>
    protected override SqlDriver CreateDriver(string connectionString, SqlDriverConfiguration configuration)
    {
      using var connection = new SQLiteConnection(connectionString);
      if (configuration.DbConnectionAccessors.Count > 0)
        OpenConnectionWithNotification(connection, configuration, false).GetAwaiter().GetResult();
      else
        OpenConnectionFast(connection, configuration, false).GetAwaiter().GetResult();
      var defaultSchema = GetDefaultSchema(connection);
      var version = new Version(connection.ServerVersion ?? string.Empty);
      return CreateDriverInstance(connectionString, version, defaultSchema);
    }

    /// <inheritdoc/>
    protected override async Task<SqlDriver> CreateDriverAsync(
      string connectionString, SqlDriverConfiguration configuration, CancellationToken token)
    {
      var connection = new SQLiteConnection(connectionString);
      await using (connection.ConfigureAwait(false)) {
        if (configuration.DbConnectionAccessors.Count > 0)
          await OpenConnectionWithNotification(connection, configuration, true, token).ConfigureAwait(false);
        else
          await OpenConnectionFast(connection, configuration, true, token).ConfigureAwait(false);
        var defaultSchema = await GetDefaultSchemaAsync(connection, token: token).ConfigureAwait(false);
        var version = new Version(connection.ServerVersion ?? string.Empty);
        return CreateDriverInstance(connectionString, version, defaultSchema);
      }
    }

    private static SqlDriver CreateDriverInstance(string connectionString, Version version,
      DefaultSchemaInfo defaultSchema)
    {
      var coreServerInfo = new CoreServerInfo {
        ServerVersion = version,
        ConnectionString = connectionString,
        MultipleActiveResultSets = false,
        DatabaseName = defaultSchema.Database,
        DefaultSchemaName = defaultSchema.Schema,
      };

      if (version.Major < 3) {
        throw new NotSupportedException(Strings.ExSqlLiteServerBelow3IsNotSupported);
      }

      return new v3.Driver(coreServerInfo);
    }

    /// <inheritdoc/>
    protected override string BuildConnectionString(UrlInfo url)
    {
      SqlHelper.ValidateConnectionUrl(url);
      var result = $"Data Source = {url.Resource}";

      if (!string.IsNullOrEmpty(url.Password)) {
        result += $"; Password = '{url.Password}'";
      }

      return result;
    }

    /// <inheritdoc/>
    protected override DefaultSchemaInfo ReadDefaultSchema(DbConnection connection, DbTransaction transaction) =>
      new DefaultSchemaInfo(GetDataSource(connection.ConnectionString), Extractor.DefaultSchemaName);

    /// <inheritdoc/>
    protected override Task<DefaultSchemaInfo> ReadDefaultSchemaAsync(
      DbConnection connection, DbTransaction transaction, CancellationToken token) =>
      Task.FromResult(new DefaultSchemaInfo(GetDataSource(connection.ConnectionString), Extractor.DefaultSchemaName));

    private async ValueTask OpenConnectionFast(SQLiteConnection connection,
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

    private async ValueTask OpenConnectionWithNotification(SQLiteConnection connection,
      SqlDriverConfiguration configuration,
      bool isAsync,
      CancellationToken cancellationToken = default)
    {
      var accessors = configuration.DbConnectionAccessors;
      if (!isAsync) {
        SqlHelper.NotifyConnectionOpening(accessors, connection);
        try {
          connection.Open();
          if (!string.IsNullOrEmpty(configuration.ConnectionInitializationSql)) {
            SqlHelper.NotifyConnectionInitializing(accessors, connection, configuration.ConnectionInitializationSql);
          }

          SqlHelper.ExecuteInitializationSql(connection, configuration);
          SqlHelper.NotifyConnectionOpened(accessors, connection);
        }
        catch (Exception ex) {
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
  }
}