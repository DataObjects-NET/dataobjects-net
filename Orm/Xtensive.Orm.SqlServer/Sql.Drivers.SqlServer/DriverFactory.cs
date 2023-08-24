// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Sql.Info;
using Xtensive.SqlServer.Resources;
using SqlServerConnection = Microsoft.Data.SqlClient.SqlConnection;

namespace Xtensive.Sql.Drivers.SqlServer
{
  /// <summary>
  /// A <see cref="SqlDriver"/> factory for Microsoft SQL Server.
  /// </summary>
  public class DriverFactory : SqlDriverFactory
  {
    private const string CheckConnectionQuery = "SELECT TOP(0) 0;";
    private const string PoolingOffCommand = "pooling = false";

    private const string DatabaseAndSchemaQuery = "SELECT DB_NAME(), COALESCE(SCHEMA_NAME(), 'dbo')";

    private const string LangIdQuery = "SELECT @@LANGID";
    private const string MessagesQuery = @"Declare @MSGLANGID int; 
      Select @MSGLANGID = msglangid FROM [master].[sys].[syslanguages] lang
      WHERE lang.langid = @@LANGID;
      SELECT msg.error , msg.description 
      FROM [master].[sys].[sysmessages] msg
      WHERE   msg.msglangid = @MSGLANGID AND msg.error IN ( 2627, 2601, 515, 547 )";

    private const string VersionQuery = "SELECT @@VERSION";

    private const string ForcedAzureVersion = "12.0.0.0";

    private static ErrorMessageParser CreateMessageParser(SqlServerConnection connection)
    {
      bool isEnglish;
      using (var command = connection.CreateCommand()) {
        command.CommandText = LangIdQuery;
        var langId = (short) command.ExecuteScalar();
        isEnglish = langId == 0 || langId == 23;
      }

      var templates = new Dictionary<int, string>();
      using (var command = connection.CreateCommand()) {
        command.CommandText = MessagesQuery;
        using (var reader = command.ExecuteReader()) {
          while (reader.Read()) {
            ReadMessageTemplate(reader, templates);
          }
        }
      }
      return new ErrorMessageParser(templates, isEnglish);
    }

    private static async Task<ErrorMessageParser> CreateMessageParserAsync(
      SqlServerConnection connection, CancellationToken token)
    {
      bool isEnglish;
      var command = connection.CreateCommand();
      await using (command.ConfigureAwaitFalse()) {
        command.CommandText = LangIdQuery;
        isEnglish = (await command.ExecuteScalarAsync(token).ConfigureAwaitFalse()).ToString()=="0";
      }

      var templates = new Dictionary<int, string>();
      command = connection.CreateCommand();
      await using (command.ConfigureAwaitFalse()) {
        command.CommandText = MessagesQuery;
        var reader = await command.ExecuteReaderAsync(token).ConfigureAwaitFalse();
        await using (reader.ConfigureAwaitFalse()) {
          while (await reader.ReadAsync(token).ConfigureAwaitFalse()) {
            ReadMessageTemplate(reader, templates);
          }
        }
      }
      return new ErrorMessageParser(templates, isEnglish);
    }

    private static void ReadMessageTemplate(SqlDataReader reader, Dictionary<int, string> templates) =>
      templates.Add(reader.GetInt32(0), reader.GetString(1));

    private static bool IsAzure(SqlServerConnection connection)
    {
      using var command = connection.CreateCommand();
      command.CommandText = VersionQuery;
      return ((string) command.ExecuteScalar()).IndexOf("Azure", StringComparison.Ordinal) >= 0;
    }

    private static async Task<bool> IsAzureAsync(SqlServerConnection connection, CancellationToken token)
    {
      var command = connection.CreateCommand();
      await using (command.ConfigureAwaitFalse()) {
        command.CommandText = VersionQuery;
        return ((string) await command.ExecuteScalarAsync(token).ConfigureAwaitFalse())
          .IndexOf("Azure", StringComparison.Ordinal) >= 0;
      }
    }

    /// <inheritdoc/>
    protected override string BuildConnectionString(UrlInfo url)
    {
      SqlHelper.ValidateConnectionUrl(url);

      var builder = new SqlConnectionStringBuilder();
      builder.Encrypt = url.Secure;

      // host, port, database
      if (url.Port==0) {
        builder.DataSource = url.Host;
      }
      else {
        builder.DataSource = url.Host + "," + url.Port;
      }

      builder.InitialCatalog = url.Resource ?? string.Empty;

      // user, password
      if (!string.IsNullOrEmpty(url.User)) {
        builder.UserID = url.User;
        builder.Password = url.Password;
      }
      else {
        builder.IntegratedSecurity = true;
        builder.PersistSecurityInfo = false;
      }

      // custom options
      foreach (var param in url.Params) {
        builder[param.Key] = param.Value;
      }

      return builder.ToString();
    }

    /// <inheritdoc/>
    protected override SqlDriver CreateDriver(string connectionString, SqlDriverConfiguration configuration)
    {
      var isPooingOn = !IsPoolingOff(connectionString);
      configuration.EnsureConnectionIsAlive &= isPooingOn;

      using var connection = CreateAndOpenConnection(connectionString, configuration);
      var isEnsureAlive = configuration.EnsureConnectionIsAlive;
      var forcedServerVersion = configuration.ForcedServerVersion;
      var isForcedVersion = !string.IsNullOrEmpty(forcedServerVersion);
      var isForcedAzure = isForcedVersion && forcedServerVersion.Equals("azure", StringComparison.OrdinalIgnoreCase);
      var isAzure = isForcedAzure || (!isForcedVersion && IsAzure(connection));
      var parser = isAzure ? new ErrorMessageParser() : CreateMessageParser(connection);

      var versionString = isForcedVersion
        ? isForcedAzure ? ForcedAzureVersion : forcedServerVersion
        : connection.ServerVersion ?? string.Empty;
      var version = new Version(versionString);
      var defaultSchema = GetDefaultSchema(connection);

      return CreateDriverInstance(connectionString, isAzure, version, defaultSchema, parser, isEnsureAlive);
    }

    /// <inheritdoc/>
    protected override async Task<SqlDriver> CreateDriverAsync(
      string connectionString, SqlDriverConfiguration configuration, CancellationToken token)
    {
      var isPooingOn = !IsPoolingOff(connectionString);
      configuration.EnsureConnectionIsAlive &= isPooingOn;

      var connection = await CreateAndOpenConnectionAsync(connectionString, configuration, token).ConfigureAwaitFalse();
      await using (connection.ConfigureAwaitFalse()) {
        var isEnsureAlive = configuration.EnsureConnectionIsAlive;
        var forcedServerVersion = configuration.ForcedServerVersion;
        var isForcedVersion = !string.IsNullOrEmpty(forcedServerVersion);
        var isForcedAzure = isForcedVersion && forcedServerVersion.Equals("azure", StringComparison.OrdinalIgnoreCase);
        var isAzure = isForcedAzure
          || (!isForcedVersion && await IsAzureAsync(connection, token).ConfigureAwaitFalse());
        var parser = isAzure
          ? new ErrorMessageParser()
          : await CreateMessageParserAsync(connection, token).ConfigureAwaitFalse();

        var versionString = isForcedVersion
          ? isForcedAzure ? "10.0.0.0" : forcedServerVersion
          : connection.ServerVersion ?? string.Empty;
        var version = new Version(versionString);
        var defaultSchema = await GetDefaultSchemaAsync(connection, token: token).ConfigureAwaitFalse();

        return CreateDriverInstance(connectionString, isAzure, version, defaultSchema, parser, isEnsureAlive);
      }
    }

    private static SqlDriver CreateDriverInstance(string connectionString, bool isAzure, Version version,
      DefaultSchemaInfo defaultSchema, ErrorMessageParser parser, bool isEnsureAlive)
    {
      var builder = new SqlConnectionStringBuilder(connectionString);
      var coreServerInfo = new CoreServerInfo {
        ServerVersion = version,
        ConnectionString = connectionString,
        MultipleActiveResultSets = builder.MultipleActiveResultSets,
        DatabaseName = defaultSchema.Database,
        DefaultSchemaName = defaultSchema.Schema,
      };
      if (isAzure) {
        return new Azure.Driver(coreServerInfo, parser, isEnsureAlive);
      }

      if (version.Major < 9) {
        throw new NotSupportedException(Strings.ExSqlServerBelow2005IsNotSupported);
      }
      return version.Major switch {
        9 => new v09.Driver(coreServerInfo, parser, isEnsureAlive),
        10 => new v10.Driver(coreServerInfo, parser, isEnsureAlive),
        11 => new v11.Driver(coreServerInfo, parser, isEnsureAlive),
        12 => new v12.Driver(coreServerInfo, parser, isEnsureAlive),
        13 => new v13.Driver(coreServerInfo, parser, isEnsureAlive),
        _ => new v13.Driver(coreServerInfo, parser, isEnsureAlive)
      };
    }

    /// <inheritdoc/>
    protected override DefaultSchemaInfo ReadDefaultSchema(DbConnection connection, DbTransaction transaction) =>
      SqlHelper.ReadDatabaseAndSchema(DatabaseAndSchemaQuery, connection, transaction);

    /// <inheritdoc/>
    protected override Task<DefaultSchemaInfo> ReadDefaultSchemaAsync(
      DbConnection connection, DbTransaction transaction, CancellationToken token) =>
      SqlHelper.ReadDatabaseAndSchemaAsync(DatabaseAndSchemaQuery, connection, transaction, token);

    private static SqlServerConnection CreateAndOpenConnection(
      string connectionString, SqlDriverConfiguration configuration)
    {
      var connection = new SqlServerConnection(connectionString);
      var initScript = configuration.ConnectionInitializationSql;

      if (!configuration.EnsureConnectionIsAlive) {
        if (configuration.DbConnectionAccessors.Count == 0)
          OpenConnectionFast(connection, initScript, false).GetAwaiter().GetResult();
        else
          OpenConnectionWithNotification(connection, configuration, false).GetAwaiter().GetResult();
        return connection;
      }

      var testQuery = string.IsNullOrEmpty(initScript)
        ? CheckConnectionQuery
        : initScript;
      if (configuration.DbConnectionAccessors.Count == 0)
        return EnsureConnectionIsAliveFast(connection, testQuery, false).GetAwaiter().GetResult();
      else
        return EnsureConnectionIsAliveWithNotification(connection, testQuery, configuration.DbConnectionAccessors, false)
          .GetAwaiter().GetResult();
    }

    private static async Task<SqlServerConnection> CreateAndOpenConnectionAsync(
      string connectionString, SqlDriverConfiguration configuration, CancellationToken token)
    {
      var connection = new SqlServerConnection(connectionString);
      var initScript = configuration.ConnectionInitializationSql;

      if (!configuration.EnsureConnectionIsAlive) {
        if (configuration.DbConnectionAccessors.Count == 0)
          await OpenConnectionFast(connection, initScript, true, token).ConfigureAwaitFalse();
        else
          await OpenConnectionWithNotification(connection, configuration, true, token).ConfigureAwaitFalse();
        return connection;
      }

      var testQuery = string.IsNullOrEmpty(initScript)
        ? CheckConnectionQuery
        : initScript;
      if (configuration.DbConnectionAccessors.Count == 0)
        return await EnsureConnectionIsAliveFast(connection, testQuery, true, token).ConfigureAwaitFalse();
      else
        return await EnsureConnectionIsAliveWithNotification(connection, testQuery, configuration.DbConnectionAccessors, true, token)
          .ConfigureAwaitFalse();
    }

    private static async ValueTask OpenConnectionFast(SqlServerConnection connection,
      string sqlScript, bool isAsync, CancellationToken token = default)
    {
      if (!isAsync) {
        connection.Open();
        SqlHelper.ExecuteInitializationSql(connection, sqlScript);
      }
      else {
        await connection.OpenAsync(token).ConfigureAwaitFalse();
        await SqlHelper.ExecuteInitializationSqlAsync(connection, sqlScript, token).ConfigureAwaitFalse();
      }
    }

    private static async ValueTask OpenConnectionWithNotification(SqlServerConnection connection,
      SqlDriverConfiguration configuration, bool isAsync, CancellationToken token = default)
    {
      var accessors = configuration.DbConnectionAccessors;
      var initSql = configuration.ConnectionInitializationSql;

      if (!isAsync) {
        SqlHelper.NotifyConnectionOpening(accessors, connection);
        try {
          connection.Open();
          if (!string.IsNullOrEmpty(initSql)) {
            SqlHelper.NotifyConnectionInitializing(accessors, connection, initSql);
            SqlHelper.ExecuteInitializationSql(connection, initSql);
          }
          SqlHelper.NotifyConnectionOpened(accessors, connection);
        }
        catch (Exception ex) {
          SqlHelper.NotifyConnectionOpeningFailed(accessors, connection, ex);
          throw;
        }
      }
      else {
        await SqlHelper.NotifyConnectionOpeningAsync(accessors, connection, false, token);
        try {
          await connection.OpenAsync(token);
          if (!string.IsNullOrEmpty(initSql)) {
            await SqlHelper.NotifyConnectionInitializingAsync(accessors, connection, initSql, false, token);
            await SqlHelper.ExecuteInitializationSqlAsync(connection, initSql, token);
          }
          await SqlHelper.NotifyConnectionOpenedAsync(accessors, connection, false, token);
        }
        catch (Exception ex) {
          await SqlHelper.NotifyConnectionOpeningFailedAsync(accessors, connection, ex, false, token);
          throw;
        }
      }
    }

    private static async ValueTask<SqlServerConnection> EnsureConnectionIsAliveFast(SqlServerConnection connection,
      string query, bool isAsync, CancellationToken token = default)
    {
      if (!isAsync) {
        try {
          connection.Open();
          
          using (var command = connection.CreateCommand()) {
            command.CommandText = query;
            _ = command.ExecuteNonQuery();
          }

          return connection;
        }
        catch (Exception exception) {
          try {
            connection.Close();
            connection.Dispose();
          }
          catch {
            // ignored
          }

          if (InternalHelpers.ShouldRetryOn(exception)) {
            var (isReconnected, newConnection) =
              TryReconnectFast(connection.ConnectionString, query, isAsync).GetAwaiter().GetResult();
            if (isReconnected)
              return newConnection;
          }
          throw;
        }
      }
      else {
        try {
          await connection.OpenAsync(token).ConfigureAwaitFalse();

          var command = connection.CreateCommand();
          await using (command.ConfigureAwaitFalse()) {
            command.CommandText = query;
            _ = await command.ExecuteNonQueryAsync(token).ConfigureAwaitFalse();
          }

          return connection;
        }
        catch (Exception exception) {
          try {
            await connection.CloseAsync().ConfigureAwaitFalse();
            await connection.DisposeAsync().ConfigureAwaitFalse();
          }
          catch {
            // ignored
          }

          if (InternalHelpers.ShouldRetryOn(exception)) {
            var (isReconnected, newConnection) =
              await TryReconnectFast(connection.ConnectionString, query, isAsync, token).ConfigureAwaitFalse();
            if (isReconnected) {
              return newConnection;
            }
          }
          throw;
        }
      }
    }

    private static async ValueTask<SqlServerConnection> EnsureConnectionIsAliveWithNotification(SqlServerConnection connection,
      string query, IReadOnlyCollection<IDbConnectionAccessor> connectionAccessos, bool isAsync, CancellationToken token = default)
    {
      if (!isAsync) {
        SqlHelper.NotifyConnectionOpening(connectionAccessos, connection);
        try {
          connection.Open();

          SqlHelper.NotifyConnectionInitializing(connectionAccessos, connection, query);

          using (var command = connection.CreateCommand()) {
            command.CommandText = query;
            _ = command.ExecuteNonQuery();
          }

          SqlHelper.NotifyConnectionOpened(connectionAccessos, connection);
          return connection;
        }
        catch (Exception exception) {
          var retryToConnect = InternalHelpers.ShouldRetryOn(exception);
          if (!retryToConnect)
            SqlHelper.NotifyConnectionOpeningFailed(connectionAccessos, connection, exception);
          try {
            connection.Close();
            connection.Dispose();
          }
          catch {
            // ignored
          }

          if (retryToConnect) {
            var (isReconnected, newConnection) = TryReconnectWithNotification(connection.ConnectionString, query, connectionAccessos, isAsync)
              .GetAwaiter().GetResult();
            if (isReconnected) {
              return newConnection;
            }
          }
          throw;
        }
      }
      else {
        await SqlHelper.NotifyConnectionOpeningAsync(connectionAccessos, connection, false, token).ConfigureAwaitFalse();

        try {
          await connection.OpenAsync(token).ConfigureAwaitFalse();

          await SqlHelper.NotifyConnectionInitializingAsync(connectionAccessos, connection, query, false, token).ConfigureAwaitFalse();

          var command = connection.CreateCommand();
          await using (command.ConfigureAwaitFalse()) {
            command.CommandText = query;
            _ = await command.ExecuteNonQueryAsync(token).ConfigureAwaitFalse();
          }

          await SqlHelper.NotifyConnectionOpenedAsync(connectionAccessos, connection, false, token).ConfigureAwaitFalse();
          return connection;
        }
        catch (Exception exception) {
          var retryToConnect = InternalHelpers.ShouldRetryOn(exception);
          if (!retryToConnect) {
            await SqlHelper.NotifyConnectionOpeningFailedAsync(connectionAccessos, connection, exception, false, token).ConfigureAwaitFalse();
          }

          var connectionString = connection.ConnectionString;
          try {
            await connection.CloseAsync().ConfigureAwaitFalse();
            await connection.DisposeAsync().ConfigureAwaitFalse();
          }
          catch {
            // ignored
          }

          if (retryToConnect) {
            var (isReconnected, newConnection) =
              await TryReconnectWithNotification(connectionString, query, connectionAccessos, isAsync, token).ConfigureAwaitFalse();
            if (isReconnected) {
              return newConnection;
            }
          }
          throw;
        }
      }
    }

    private static async Task<(bool isReconnected, SqlServerConnection connection)> TryReconnectFast(
      string connectionString, string query, bool isAsync, CancellationToken token = default)
    {
      var connection = new SqlServerConnection(connectionString);
      if (!isAsync) {
        try {
          connection.Open();

          using (var command = connection.CreateCommand()) {
            command.CommandText = query;
            _ = command.ExecuteNonQuery();
          }

          return (true, connection);
        }
        catch {
          connection.Dispose();
          return (false, null);
        }
      }
      else {
        try {
          await connection.OpenAsync(token).ConfigureAwaitFalse();

          var command = connection.CreateCommand();
          await using (command.ConfigureAwaitFalse()) {
            command.CommandText = query;
            _ = await command.ExecuteNonQueryAsync(token).ConfigureAwaitFalse();
          }

          return (true, connection);
        }
        catch {
          await connection.DisposeAsync();
          return (false, null);
        }
      }
    }

    private static async Task<(bool isReconnected, SqlServerConnection connection)> TryReconnectWithNotification(
      string connectionString, string query, IReadOnlyCollection<IDbConnectionAccessor> connectionAccessors,
      bool isAsync, CancellationToken token = default)
    {
      var connection = new SqlServerConnection(connectionString);
      if (!isAsync) {
        SqlHelper.NotifyConnectionOpening(connectionAccessors, connection, true);

        try {
          connection.Open();
          SqlHelper.NotifyConnectionInitializing(connectionAccessors, connection, query, true);

          using (var command = connection.CreateCommand()) {
            command.CommandText = query;
            _ = command.ExecuteNonQuery();
          }

          SqlHelper.NotifyConnectionOpened(connectionAccessors, connection, true);
          return (true, connection);
        }
        catch (Exception exception) {
          SqlHelper.NotifyConnectionOpeningFailed(connectionAccessors, connection, exception, true);
          connection.Dispose();
          return (false, null);
        }
      }
      else {
        await SqlHelper.NotifyConnectionOpeningAsync(connectionAccessors, connection, true, token).ConfigureAwaitFalse();

        try {
          await connection.OpenAsync(token).ConfigureAwaitFalse();

          await SqlHelper.NotifyConnectionInitializingAsync(connectionAccessors, connection, query, true, token).ConfigureAwaitFalse();

          var command = connection.CreateCommand();
          await using (command.ConfigureAwaitFalse()) {
            command.CommandText = query;
            _ = await command.ExecuteNonQueryAsync(token).ConfigureAwaitFalse();
          }

          await SqlHelper.NotifyConnectionOpenedAsync(connectionAccessors, connection, true, token).ConfigureAwaitFalse();
          return (true, connection);
        }
        catch (Exception exception) {
          await SqlHelper.NotifyConnectionOpeningFailedAsync(connectionAccessors, connection, exception, true, token).ConfigureAwaitFalse();
          await connection.DisposeAsync();
          return (false, null);
        }
      }
    }

    private static bool IsPoolingOff(string connectionString)
    {
      var lowerCaseString = connectionString.ToLower();
      return lowerCaseString.Contains(PoolingOffCommand) ||
             lowerCaseString.Contains(PoolingOffCommand.Replace(" ", ""));
    }
  }
}
