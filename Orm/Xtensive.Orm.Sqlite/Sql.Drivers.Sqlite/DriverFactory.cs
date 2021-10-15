// Copyright (C) 2011-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Malisa Ncube
// Created:    2011.04.29

using System;
using System.Data.Common;
using System.Data.SQLite;
using System.Text.RegularExpressions;
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

    private string GetDataSource(string connectionString)
    {
      var match = DataSourceExtractor.Match(connectionString);
      if (!match.Success)
        return "<unknown>";
      var dataSource = match.Groups[2].Captures[0].Value;
      if (dataSource.Length > 1 && dataSource.StartsWith("'") && dataSource.EndsWith("'"))
        dataSource = dataSource.Substring(1, dataSource.Length - 2);
      return dataSource;
    }

    /// <inheritdoc/>
    protected override SqlDriver CreateDriver(string connectionString, SqlDriverConfiguration configuration)
    {
      return DoCreateDriver(connectionString, configuration);
    }

    private SqlDriver DoCreateDriver(string connectionString, SqlDriverConfiguration configuration)
    {
      using (var connection = new SQLiteConnection(connectionString)) {
        if (configuration.DbConnectionAccessors.Count > 0)
          OpenConnectionWithNotification(connection, configuration);
        else
          OpenConnectionFast(connection, configuration);
        var version = new Version(connection.ServerVersion);
        var defaultSchema = GetDefaultSchema(connection);
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
      string result = string.Format("Data Source = {0}", url.Resource);

      if (!string.IsNullOrEmpty(url.Password))
        result += String.Format("; Password = '{0}'", url.Password);

      return result;
    }

    /// <inheritdoc/>
    protected override DefaultSchemaInfo ReadDefaultSchema(DbConnection connection, DbTransaction transaction)
    {
      return new DefaultSchemaInfo(GetDataSource(connection.ConnectionString), Extractor.DefaultSchemaName);
    }

    private void OpenConnectionFast(SQLiteConnection connection, SqlDriverConfiguration configuration)
    {
      connection.Open();
      SqlHelper.ExecuteInitializationSql(connection, configuration);
    }

    private void OpenConnectionWithNotification(SQLiteConnection connection, SqlDriverConfiguration configuration)
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