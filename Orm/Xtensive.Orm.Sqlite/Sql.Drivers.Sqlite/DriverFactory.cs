// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.04.29

using System;
using System.Data.Common;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using Xtensive.Orm;
using Xtensive.Sql.Drivers.Sqlite.Resources;
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
      ProviderInitializer.Run(configuration.NativeLibraryCacheFolder);
      return DoCreateDriver(connectionString, configuration);
    }

    private SqlDriver DoCreateDriver(string connectionString, SqlDriverConfiguration configuration)
    {
      using (var connection = new SQLiteConnection(connectionString)) {
        connection.Open();
        SqlHelper.ExecuteInitializationSql(connection, configuration);
        var version = new Version(connection.ServerVersion);
        var defaultSchema = GetDefaultSchema(connection);
        var coreServerInfo = new CoreServerInfo {
          ServerVersion = version,
          ConnectionString = connectionString,
          MultipleActiveResultSets = false,
          DatabaseName = defaultSchema.Database,
          DefaultSchemaName = defaultSchema.Schema,
        };

        if (version.Major < 3)
          throw new NotSupportedException(Strings.ExSqlLiteServerBelow3IsNotSupported);
        return new v3.Driver(coreServerInfo);
      }
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
    public override DefaultSchemaInfo GetDefaultSchema(DbConnection connection)
    {
      return new DefaultSchemaInfo(GetDataSource(connection.ConnectionString), "Main");
    }
  }
}