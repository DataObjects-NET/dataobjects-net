// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using Xtensive.Core;
using Xtensive.Sql.SqlServer.Resources;

namespace Xtensive.Sql.SqlServer
{
  /// <summary>
  /// A <see cref="SqlDriver"/> factory for Microsoft SQL Server.
  /// </summary>
  public class DriverFactory : SqlDriverFactory
  {
    /// <inheritdoc/>
    public override SqlDriver CreateDriver(UrlInfo url)
    {
      using (var connection = ConnectionFactory.CreateConnection(url)) {
        connection.Open();
        var version = new Version(connection.ServerVersion);
        SqlDriver driver;
        switch (version.Major) {
          case 9:
            driver = new v09.Driver(connection, version);
            break;
          case 10:
            using (var command = connection.CreateCommand()) {
              command.CommandText = "SELECT @@VERSION";
              string result = command.ExecuteScalar() as string;
              if (result!=null && result.Contains("Azure"))
                driver = new Azure.Driver(connection, version);
              else
                driver = new v10.Driver(connection, version);
            }
            break;
          default:
            throw new NotSupportedException(Strings.ExMicrosoftSqlServerBelow2005IsNotSupported);
        }
        connection.Close();
        return driver;
      }
    }
  }
}