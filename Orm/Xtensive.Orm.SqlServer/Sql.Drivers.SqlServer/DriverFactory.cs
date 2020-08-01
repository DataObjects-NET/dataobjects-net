// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Orm;
using Xtensive.Sql.Info;
using SqlServerConnection = System.Data.SqlClient.SqlConnection;

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

    private static ErrorMessageParser CreateMessageParser(SqlServerConnection connection)
    {
      bool isEnglish;
      using (var command = connection.CreateCommand()) {
        command.CommandText = LangIdQuery;
        isEnglish = command.ExecuteScalar().ToString()=="0";
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
      await using (command.ConfigureAwait(false)) {
        command.CommandText = LangIdQuery;
        isEnglish = (await command.ExecuteScalarAsync(token).ConfigureAwait(false)).ToString()=="0";
      }

      var templates = new Dictionary<int, string>();
      command = connection.CreateCommand();
      await using (command.ConfigureAwait(false)) {
        command.CommandText = MessagesQuery;
        await using (var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false)) {
          while (await reader.ReadAsync(token).ConfigureAwait(false)) {
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
      await using (command.ConfigureAwait(false)) {
        command.CommandText = VersionQuery;
        return ((string) await command.ExecuteScalarAsync(token).ConfigureAwait(false))
          .IndexOf("Azure", StringComparison.Ordinal) >= 0;
      }
    }

    /// <inheritdoc/>
    protected override string BuildConnectionString(UrlInfo url)
    {
      SqlHelper.ValidateConnectionUrl(url);

      var builder = new SqlConnectionStringBuilder();

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
        ? isForcedAzure ? "10.0.0.0" : forcedServerVersion
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

      var connection = await CreateAndOpenConnectionAsync(connectionString, configuration, token).ConfigureAwait(false);
      await using (connection.ConfigureAwait(false)) {
        var isEnsureAlive = configuration.EnsureConnectionIsAlive;
        var forcedServerVersion = configuration.ForcedServerVersion;
        var isForcedVersion = !string.IsNullOrEmpty(forcedServerVersion);
        var isForcedAzure = isForcedVersion && forcedServerVersion.Equals("azure", StringComparison.OrdinalIgnoreCase);
        var isAzure = isForcedAzure
          || (!isForcedVersion && await IsAzureAsync(connection, token).ConfigureAwait(false));
        var parser = isAzure
          ? new ErrorMessageParser()
          : await CreateMessageParserAsync(connection, token).ConfigureAwait(false);

        var versionString = isForcedVersion
          ? isForcedAzure ? "10.0.0.0" : forcedServerVersion
          : connection.ServerVersion ?? string.Empty;
        var version = new Version(versionString);
        var defaultSchema = await GetDefaultSchemaAsync(connection, token: token).ConfigureAwait(false);

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
      if (!configuration.EnsureConnectionIsAlive) {
        connection.Open();
        SqlHelper.ExecuteInitializationSql(connection, configuration);
        return connection;
      }

      var testQuery = string.IsNullOrEmpty(configuration.ConnectionInitializationSql)
        ? CheckConnectionQuery
        : configuration.ConnectionInitializationSql;
      return EnsureConnectionIsAlive(connection, testQuery);
    }

    private static async Task<SqlServerConnection> CreateAndOpenConnectionAsync(
      string connectionString, SqlDriverConfiguration configuration, CancellationToken token)
    {
      var connection = new SqlServerConnection(connectionString);
      if (!configuration.EnsureConnectionIsAlive) {
        await connection.OpenAsync(token).ConfigureAwait(false);
        await SqlHelper.ExecuteInitializationSqlAsync(connection, configuration, token).ConfigureAwait(false);
        return connection;
      }

      var testQuery = string.IsNullOrEmpty(configuration.ConnectionInitializationSql)
        ? CheckConnectionQuery
        : configuration.ConnectionInitializationSql;
      return await EnsureConnectionIsAliveAsync(connection, testQuery, token).ConfigureAwait(false);
    }

    private static SqlServerConnection EnsureConnectionIsAlive(SqlServerConnection connection, string query)
    {
      try {
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = query;
        command.ExecuteNonQuery();

        return connection;
      }
      catch (Exception exception) {
        var connectionString = connection.ConnectionString;
        try {
          connection.Close();
          connection.Dispose();
        }
        catch {
          // ignored
        }

        if (InternalHelpers.ShouldRetryOn(exception)) {
          var (isReconnected, newConnection) = TryReconnect(connectionString, query);
          if (isReconnected) {
            return newConnection;
          }
        }
        throw;
      }
    }

    private static async Task<SqlServerConnection> EnsureConnectionIsAliveAsync(
      SqlServerConnection connection, string query, CancellationToken token)
    {
      try {
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        await using (command.ConfigureAwait(false)) {
          command.CommandText = query;
          await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }

        return connection;
      }
      catch (Exception exception) {
        var connectionString = connection.ConnectionString;
        try {
          await connection.CloseAsync().ConfigureAwait(false);
          await connection.DisposeAsync().ConfigureAwait(false);
        }
        catch {
          // ignored
        }

        if (InternalHelpers.ShouldRetryOn(exception)) {
          var (isReconnected, newConnection) =
            await TryReconnectAsync(connectionString, query, token).ConfigureAwait(false);
          if (isReconnected) {
            return newConnection;
          }
        }
        throw;
      }
    }

    private static (bool isReconnected, SqlServerConnection connection) TryReconnect(
      string connectionString, string query)
    {
      try {
        var connection = new SqlServerConnection(connectionString);
        connection.Open();
        using (var command = connection.CreateCommand()) {
          command.CommandText = query;
          command.ExecuteNonQuery();
        }
        return (true, connection);
      }
      catch {
        return (false, null);
      }
    }

    private static async Task<(bool isReconnected, SqlServerConnection connection)> TryReconnectAsync(
      string connectionString, string query, CancellationToken token)
    {
      try {
        var connection = new SqlServerConnection(connectionString);
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        await using (command.ConfigureAwait(false)) {
          command.CommandText = query;
          await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }
        return (true, connection);
      }
      catch {
        return (false, null);
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