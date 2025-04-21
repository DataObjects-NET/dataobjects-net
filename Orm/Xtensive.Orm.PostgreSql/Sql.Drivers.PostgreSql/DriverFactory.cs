// Copyright (C) 2009-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using System.Data.Common;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Xtensive.Orm;
using Xtensive.Sql.Info;
using Xtensive.Sql.Drivers.PostgreSql.Resources;

namespace Xtensive.Sql.Drivers.PostgreSql
{
  /// <summary>
  /// A <see cref="SqlDriver"/> factory for PostgreSQL.
  /// </summary>
  public class DriverFactory : SqlDriverFactory
  {
    private const string DatabaseAndSchemaQuery = "select current_database(), current_schema()";

    private readonly static bool InfinityAliasForDatesEnabled;
    private readonly static bool LegacyTimestamptBehaviorEnabled;

    /// <inheritdoc/>
    [SecuritySafeCritical]
    protected override string BuildConnectionString(UrlInfo url)
    {
      SqlHelper.ValidateConnectionUrl(url);

      var builder = new NpgsqlConnectionStringBuilder();

      // host, port, database
      builder.Host = url.Host;
      if (url.Port != 0) {
        builder.Port = url.Port;
      }

      builder.Database = url.Resource ?? string.Empty;

      // user, password
      if (!string.IsNullOrEmpty(url.User)) {
        builder.Username = url.User;
        builder.Password = url.Password;
      }

      // custom options
      foreach (var param in url.Params) {
        builder[param.Key] = param.Value;
      }

      return builder.ToString();
    }

    /// <inheritdoc/>
    [SecuritySafeCritical]
    protected override SqlDriver CreateDriver(string connectionString, SqlDriverConfiguration configuration)
    {
      using var connection = new NpgsqlConnection(connectionString);
      if (configuration.DbConnectionAccessors.Count > 0)
        OpenConnectionWithNotification(connection, configuration, false).GetAwaiter().GetResult();
      else
        OpenConnectionFast(connection, configuration, false).GetAwaiter().GetResult();
      var version = GetVersion(configuration, connection);
      var defaultSchema = GetDefaultSchema(connection);
      var defaultTimeZoneInfo = PostgreSqlHelper.GetTimeZoneInfoForServerTimeZone(connection.Timezone);
      return CreateDriverInstance(connectionString, version, defaultSchema, defaultTimeZoneInfo);
    }

    /// <inheritdoc/>
    protected override async Task<SqlDriver> CreateDriverAsync(
      string connectionString, SqlDriverConfiguration configuration, CancellationToken token)
    {
      // these settings needed to be read before any connection happens
      var useInfinityAliasForDateTime = InfinityAliasForDatesEnabled;
      var legacyTimestampBehavior = LegacyTimestamptBehaviorEnabled;

      var connection = new NpgsqlConnection(connectionString);
      await using (connection.ConfigureAwait(false)) {
        if (configuration.DbConnectionAccessors.Count > 0)
          await OpenConnectionWithNotification(connection, configuration, true, token).ConfigureAwait(false);
        else
          await OpenConnectionFast(connection, configuration, true, token).ConfigureAwait(false);
        var version = GetVersion(configuration, connection);
        var defaultSchema = await GetDefaultSchemaAsync(connection, token: token).ConfigureAwait(false);
        var defaultTimeZoneInfo = PostgreSqlHelper.GetTimeZoneInfoForServerTimeZone(connection.Timezone);
        return CreateDriverInstance(connectionString, version, defaultSchema, defaultTimeZoneInfo);
      }
    }

    private static Version GetVersion(SqlDriverConfiguration configuration, NpgsqlConnection connection)
    {
      var version = string.IsNullOrEmpty(configuration.ForcedServerVersion)
        ? connection.PostgreSqlVersion
        : new Version(configuration.ForcedServerVersion);
      return version;
    }

    private static SqlDriver CreateDriverInstance(
      string connectionString, Version version, DefaultSchemaInfo defaultSchema,
      TimeZoneInfo defaultTimeZone)
    {
      var coreServerInfo = new CoreServerInfo {
        ServerVersion = version,
        ConnectionString = connectionString,
        MultipleActiveResultSets = false,
        DatabaseName = defaultSchema.Database,
        DefaultSchemaName = defaultSchema.Schema,
      };

      var pgsqlServerInfo = new PostgreServerInfo() {
        InfinityAliasForDatesEnabled = InfinityAliasForDatesEnabled,
        LegacyTimestampBehavior = LegacyTimestamptBehaviorEnabled,
        DefaultTimeZone = defaultTimeZone
      };

      if (version.Major < 8 || (version.Major == 8 && version.Minor < 3)) {
        throw new NotSupportedException(Strings.ExPostgreSqlBelow83IsNotSupported);
      }

      // We support 8.3, 8.4 and any 9.0+

      return version.Major switch {
        8 when version.Minor == 3 => new v8_3.Driver(coreServerInfo, pgsqlServerInfo),
        8 when version.Minor > 3  => new v8_4.Driver(coreServerInfo, pgsqlServerInfo),
        9 when version.Minor == 0 => new v9_0.Driver(coreServerInfo, pgsqlServerInfo),
        9 when version.Minor > 0 => new v9_1.Driver(coreServerInfo, pgsqlServerInfo),
        10 => new v10_0.Driver(coreServerInfo, pgsqlServerInfo),
        11 => new v10_0.Driver(coreServerInfo, pgsqlServerInfo),
        _ => new v12_0.Driver(coreServerInfo, pgsqlServerInfo)
      };
    }

    /// <inheritdoc/>
    protected override DefaultSchemaInfo ReadDefaultSchema(DbConnection connection, DbTransaction transaction) =>
      SqlHelper.ReadDatabaseAndSchema(DatabaseAndSchemaQuery, connection, transaction);

    /// <inheritdoc/>
    protected override Task<DefaultSchemaInfo> ReadDefaultSchemaAsync(
      DbConnection connection, DbTransaction transaction, CancellationToken token) =>
      SqlHelper.ReadDatabaseAndSchemaAsync(DatabaseAndSchemaQuery, connection, transaction, token);

    private async ValueTask OpenConnectionFast(NpgsqlConnection connection,
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

    private async ValueTask OpenConnectionWithNotification(NpgsqlConnection connection,
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

    #region Helpers

    private static bool SetOrGetExistingDisableInfinityAliasForDatesSwitch(bool valueToSet) =>
      GetSwitchValueOrSet(Orm.PostgreSql.WellKnown.DateTimeToInfinityConversionSwitchName, valueToSet);

    private static bool SetOrGetExistingLegacyTimeStampBehaviorSwitch(bool valueToSet) =>
      GetSwitchValueOrSet(Orm.PostgreSql.WellKnown.LegacyTimestampBehaviorSwitchName, valueToSet);

    private static bool GetSwitchValueOrSet(string switchName, bool valueToSet)
    {
      if (!AppContext.TryGetSwitch(switchName, out var currentValue)) {
        AppContext.SetSwitch(switchName, valueToSet);
        return valueToSet;
      }
      else {
        return currentValue;
      }
    }

    #endregion

    static DriverFactory()
    {
      // Starging from Npgsql 6.0 they broke compatibility by forcefully replacing
      // DateTime.MinValue/MaxValue of parameters with -Infinity and Infinity values.
      // This new "feature", though doesn't affect reading/writing of values and equality/inequality
      // filters, breaks some of operations such as parts extraction, default values for columns
      // (which are constants and declared on high levels of abstraction) and some others.

      // We turn it off to make current code work as before and make current data of
      // the user be compatible with algorighms as long as possible.
      // But if the user sets the switch then we work with what we have.
      // Usage of such aliases makes us to create extra statements in SQL queries to provide
      // the same results the queries which are already written, which may make queries a bit slower.

      // DO NOT REPLACE method call with constant value when debugging, CHANGE THE PARAMETER VALUE.
      InfinityAliasForDatesEnabled = !SetOrGetExistingDisableInfinityAliasForDatesSwitch(valueToSet: true);

      // Legacy timestamp behavoir turns off certain parameter value binding requirements
      // and makes Npgsql work like v4 or older.
      // Current behavior require manual specification of unspecified kind for DateTime values,
      // because Local or Utc kind now meand that underlying type of value to Timestamp without time zone 
      // and Timestamp with time zone respectively.
      // It also affects DateTimeOffsets, now there is a requirement to move timezone of value to Utc
      // this forces us to use only local timezone when reading values, which basically ignores
      // Postgre's setting SET TIME ZONE for database session.

      // We have to use current mode, not the legacy one, because there is a chance of legacy mode elimination.

      // DO NOT REPLACE method call with constant value when debugging, CHANGE THE PARAMETER VALUE.
      LegacyTimestamptBehaviorEnabled = SetOrGetExistingLegacyTimeStampBehaviorSwitch(valueToSet: false);
    }
  }
}
