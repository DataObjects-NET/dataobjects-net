// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.SqlServerCe
{
  /// <summary>
  /// A <see cref="SqlDriver"/> factory for Microsoft SQL Server.
  /// </summary>
  public class DriverFactory : SqlDriverFactory
  {
    /// <inheritdoc/>
    protected override SqlDriver CreateDriver(string connectionString, string forcedVersion)
    {
      var version = new Version(3, 5, 1, 0);
      var coreServerInfo = new CoreServerInfo {
        ServerVersion = version,
        ConnectionString = connectionString,
        DatabaseName = string.Empty,
        DefaultSchemaName = "default",
        MultipleActiveResultSets = true
      };
      return new v3_5.Driver(coreServerInfo);
    }

    /// <inheritdoc/>
    protected override string BuildConnectionString(UrlInfo url)
    {
      SqlHelper.ValidateConnectionUrl(url);

      string result = string.Format("Data Source = '{0}'", url.Resource);
      
      if (!String.IsNullOrEmpty(url.Password))
        result += string.Format("; Password = '{0}'", url.Password);
      
      return result;
    }
  }
}