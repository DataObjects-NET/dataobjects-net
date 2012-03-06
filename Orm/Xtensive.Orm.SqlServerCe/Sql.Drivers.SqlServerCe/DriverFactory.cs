// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Xtensive.Core;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.SqlServerCe
{
  /// <summary>
  /// A <see cref="SqlDriver"/> factory for Microsoft SQL Server.
  /// </summary>
  public class DriverFactory : SqlDriverFactory
  {
    private static readonly Regex DataSourceExtractor = new Regex(
      @"(.*Data Source *= *)(.*)($|;.*)",
      RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

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
    protected override SqlDriver CreateDriver(string connectionString, string forcedVersion)
    {
      var version = new Version(3, 5, 1, 0);
      var dataSource = GetDataSource(connectionString);
      var coreServerInfo = new CoreServerInfo {
        ServerVersion = version,
        ConnectionString = connectionString,
        DatabaseName = string.Empty,
        DefaultSchemaName = string.Empty,
        MultipleActiveResultSets = true
      };
      return new v3_5.Driver(coreServerInfo);
    }

    /// <inheritdoc/>
    public override string BuildConnectionString(UrlInfo url)
    {
      SqlHelper.ValidateConnectionUrl(url);

      string result = string.Format("Data Source = '{0}'", url.Resource);
      
      if (!String.IsNullOrEmpty(url.Password))
        result += String.Format("; Password = '{0}'", url.Password);
      
      return result;
    }
  }
}