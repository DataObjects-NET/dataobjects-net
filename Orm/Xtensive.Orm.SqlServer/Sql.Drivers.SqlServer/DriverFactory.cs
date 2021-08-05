// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Sql.Info;
using Xtensive.SqlServer.Resources;
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

    private const string ForcedAzureVersion = "12.0.0.0";

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
        using (var reader = command.ExecuteReader())
          while (reader.Read())
            templates.Add(reader.GetInt32(0), reader.GetString(1));
      }
      return new ErrorMessageParser(templates, isEnglish);
    }

    private static bool IsAzure(SqlServerConnection connection)
    {
      using (var command = connection.CreateCommand()) {
        command.CommandText = VersionQuery;
        return ((string) command.ExecuteScalar()).IndexOf("Azure", StringComparison.Ordinal) >= 0;
      }
    }

    /// <inheritdoc/>
    protected override string BuildConnectionString(UrlInfo url)
    {
      SqlHelper.ValidateConnectionUrl(url);

      var builder = new SqlConnectionStringBuilder();

      // host, port, database
      if (url.Port==0)
        builder.DataSource = url.Host;
      else
        builder.DataSource = url.Host + "," + url.Port;
      builder.InitialCatalog = url.Resource ?? string.Empty;

      // user, password
      if (!String.IsNullOrEmpty(url.User)) {
        builder.UserID = url.User;
        builder.Password = url.Password;
      }
      else {
        builder.IntegratedSecurity = true;
        builder.PersistSecurityInfo = false;
      }

      // custom options
      foreach (var param in url.Params)
        builder[param.Key] = param.Value;

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

    private SqlServerConnection CreateAndOpenConnection(string connectionString, SqlDriverConfiguration configuration)
    {
      var connection = new SqlServerConnection(connectionString);
      var initScript = configuration.ConnectionInitializationSql;

      if (!configuration.EnsureConnectionIsAlive) {
        if (configuration.ConnectionHandlers.Count == 0)
          OpenConnectionFast(connection, initScript);
        else
          OpenConnectionWithNotification(connection, configuration);
        return connection;
      }

      var testQuery = string.IsNullOrEmpty(initScript)
        ? CheckConnectionQuery
        : initScript;
      if (configuration.ConnectionHandlers.Count == 0)
        return EnsureConnectionIsAliveFast(connection, testQuery);
      else
        return EnsureConnectionIsAliveWithNotification(connection, testQuery, configuration.ConnectionHandlers);
    }

    private static void OpenConnectionFast(SqlServerConnection connection, string sqlScript)
    {
      connection.Open();
      SqlHelper.ExecuteInitializationSql(connection, sqlScript);
    }

    private static void OpenConnectionWithNotification(SqlServerConnection connection,
      SqlDriverConfiguration configuration)
    {
      var handlers = configuration.ConnectionHandlers;
      var initSql = configuration.ConnectionInitializationSql;

      SqlHelper.NotifyConnectionOpening(handlers, connection);
      try {
        connection.Open();
        if (!string.IsNullOrEmpty(initSql)) {
          SqlHelper.NotifyConnectionInitializing(handlers, connection, initSql);
          SqlHelper.ExecuteInitializationSql(connection, initSql);
        }
        SqlHelper.NotifyConnectionOpened(handlers, connection);
      }
      catch (Exception ex) {
        SqlHelper.NotifyConnectionOpeningFailed(handlers, connection, ex);
        throw;
      }
    }

    private static SqlServerConnection EnsureConnectionIsAliveFast(SqlServerConnection connection, string query)
    {
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
          var (isReconnected, newConnection) = TryReconnectFast(connection.ConnectionString, query);
          if (isReconnected) {
            return newConnection;
          }
        }
        throw;
      }
    }

    private static SqlServerConnection EnsureConnectionIsAliveWithNotification(SqlServerConnection connection,
      string query, IReadOnlyCollection<IConnectionHandler> handlers)
    {
      SqlHelper.NotifyConnectionOpening(handlers, connection);
      try {
        connection.Open();

        SqlHelper.NotifyConnectionInitializing(handlers, connection, query);

        using (var command = connection.CreateCommand()) {
          command.CommandText = query;
          _ = command.ExecuteNonQuery();
        }

        SqlHelper.NotifyConnectionOpened(handlers, connection);
        return connection;
      }
      catch (Exception exception) {
        var retryToConnect = InternalHelpers.ShouldRetryOn(exception);
        if (!retryToConnect)
          SqlHelper.NotifyConnectionOpeningFailed(handlers, connection, exception);
        try {
          connection.Close();
          connection.Dispose();
        }
        catch {
          // ignored
        }

        if (retryToConnect) {
          var (isReconnected, newConnection) = TryReconnectWithNotification(connection.ConnectionString, query, handlers);
          if (isReconnected) {
            return newConnection;
          }
        }
        throw;
      }
    }

    private static (bool isReconnected, SqlServerConnection connection) TryReconnectFast(
      string connectionString, string query)
    {
      var connection = new SqlServerConnection(connectionString);

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

    private static (bool isReconnected, SqlServerConnection connection) TryReconnectWithNotification(
      string connectionString, string query, IReadOnlyCollection<IConnectionHandler> handlers)
    {
      var connection = new SqlServerConnection(connectionString);

      SqlHelper.NotifyConnectionOpening(handlers, connection, true);
      try {
        connection.Open();
        SqlHelper.NotifyConnectionInitializing(handlers, connection, query, true);

        using (var command = connection.CreateCommand()) {
          command.CommandText = query;
          _ = command.ExecuteNonQuery();
        }

        SqlHelper.NotifyConnectionOpened(handlers, connection, true);
        return (true, connection);
      }
      catch (Exception exception) {
        SqlHelper.NotifyConnectionOpeningFailed(handlers, connection, exception, true);
        connection.Dispose();
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