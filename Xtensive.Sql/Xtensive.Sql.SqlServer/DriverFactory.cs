// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using System.Diagnostics;
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
    public override SqlDriver CreateDriver(UrlInfo sqlConnectionUrl)
    {
      using (var connection = ConnectionFactory.CreateConnection(sqlConnectionUrl)) {
        connection.Open();
        SqlDriver result;
        var version = new Version(connection.ServerVersion);
        if (version.Major < 9)
          throw new NotSupportedException(Strings.ExMicrosoftSqlServerBelow2005IsNotSupported);
        if (version.Major==9)
          result = new v2005.Driver(connection);
        else
          result = new v2008.Driver(connection);
        connection.Close();
        return result;
      }
    }
  }
}