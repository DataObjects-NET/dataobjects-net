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
    public override SqlDriver CreateDriver(UrlInfo sqlConnectionUrl)
    {
      using (var connection = ConnectionFactory.CreateConnection(sqlConnectionUrl)) {
        SqlDriver result;
        connection.Open();
        int major = connection.PostgreSqlVersion.Major;
        int minor = connection.PostgreSqlVersion.Minor;
        if (major < 8)
          throw new NotSupportedException(Strings.ExPostgreSqlBelow80IsNotSupported);
        if (major==8 && minor==0)
          result = new v8_0.Driver(connection);
        else if (major==8 && minor==1)
          result = new v8_1.Driver(connection);
        else if (major==8 && minor==2)
          result = new v8_2.Driver(connection);
        else
          result = new v8_3.Driver(connection);
        connection.Close();
        return result;
      }
    }
  }
}