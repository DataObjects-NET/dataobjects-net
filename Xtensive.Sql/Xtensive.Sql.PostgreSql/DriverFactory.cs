// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using Xtensive.Core;
using Xtensive.Sql.PostgreSql.Resources;

namespace Xtensive.Sql.PostgreSql
{
  /// <summary>
  /// A <see cref="SqlDriver"/> factory for PostgreSQL.
  /// </summary>
  public class DriverFactory : SqlDriverFactory
  {
    /// <inheritdoc/>
    public override SqlDriver CreateDriver(UrlInfo url)
    {
      using (var connection = ConnectionFactory.CreateConnection(url)) {
        connection.Open();
        var version = connection.PostgreSqlVersion;
        int major = version.Major;
        int minor = version.Minor;
        if (major < 8)
          throw new NotSupportedException(Strings.ExPostgreSqlBelow80IsNotSupported);
        SqlDriver result;
        if (major==8 && minor==0)
          result = new v8_0.Driver(connection, version);
        else if (major==8 && minor==1)
          result = new v8_1.Driver(connection, version);
        else if (major==8 && minor==2)
          result = new v8_2.Driver(connection, version);
        else if (major==8 && minor==3)
          result = new v8_3.Driver(connection, version);
        else
          result = new v8_4.Driver(connection, version);
        connection.Close();
        return result;
      }
    }
  }
}