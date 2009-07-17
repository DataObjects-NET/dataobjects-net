// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.16

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Sql.Oracle.Resources;

namespace Xtensive.Sql.Oracle
{
  /// <summary>
  /// A <see cref="SqlDriverFactory"/> for Oracle.
  /// </summary>
  public class DriverFactory : SqlDriverFactory
  {
    public override SqlDriver CreateDriver(UrlInfo url)
    {
      using (var connection = ConnectionFactory.CreateConnection(url)) {
        connection.Open();
        var version = ParseVersion(connection.ServerVersion);
        if (version.Major < 9 || version.Major==9 && version.Minor < 2)
          throw new NotSupportedException(Strings.ExOracleBelow9i2IsNotSupported);
        SqlDriver result;
        if (version.Major==9)
          result = new v09.Driver(connection, version);
        else if (version.Major==10)
          result = new v10.Driver(connection, version);
        else
          result = new v11.Driver(connection, version);
        connection.Close();
        return result;
      }
    }

    private static Version ParseVersion(string version)
    {
      var items = version.Split('.').Take(4).Select(item => int.Parse(item)).ToArray();
      return new Version(items[0], items[1], items[2], items[3]);
    }
  }
}