// Copyright (C) 2010-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2010.02.11

using System;

namespace Xtensive.Orm.Tests
{
  public static class StorageProviderVersion
  {
    public static Version SqlServer2005 = new Version(9, 0);
    public static Version SqlServer2008 = new Version(10, 0);
    public static Version SqlServer2008R2 = new Version(10, 50);
    public static Version SqlServer2012 = new Version(11, 0);
    public static Version SqlServer2014 = new Version(12, 0);
    public static Version SqlServer2016 = new Version(13, 0);
    public static Version SqlServer2017 = new Version(14, 0);
    public static Version SqlServer2019 = new Version(15, 0);

    public static Version Oracle09 = new Version(9, 0);
    public static Version Oracle10 = new Version(10, 0);
    public static Version Oracle11 = new Version(11, 0);

    public static Version PostgreSql80 = new Version(8, 0);
    public static Version PostgreSql81 = new Version(8, 1);
    public static Version PostgreSql82 = new Version(8, 2);
    public static Version PostgreSql83 = new Version(8, 3);
    public static Version PostgreSql84 = new Version(8, 4);
    public static Version PostgreSql90 = new Version(9, 0);
    public static Version PostgreSql91 = new Version(9, 1);
    public static Version PostgreSql92 = new Version(9, 2);
    public static Version PostgreSql100 = new Version(10, 0);
    public static Version PostgreSql110 = new Version(11, 0);
    public static Version PostgreSql120 = new Version(12, 0);

    public static Version MySql55 = new Version(5, 5);
    public static Version MySql56 = new Version(5, 6);
    public static Version MySql57 = new Version(5, 7);
    public static Version Mysql80 = new Version(8, 0);
  }
}