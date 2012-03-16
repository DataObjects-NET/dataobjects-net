// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.11

using System;

namespace Xtensive.Orm.Tests
{
  public static class StorageProviderVersion
  {
    public static Version SqlServer2005 = new Version(9, 0);
    public static Version SqlServer2008 = new Version(10, 0);

    public static Version Oracle09 = new Version(9, 0);
    public static Version Oracle10 = new Version(10, 0);
    public static Version Oracle11 = new Version(11, 0);

    public static Version PostgreSql80 = new Version(8, 0);
    public static Version PostgreSql81 = new Version(8, 1);
    public static Version PostgreSql82 = new Version(8, 2);
    public static Version PostgreSql83 = new Version(8, 3);
    public static Version PostgreSql84 = new Version(8, 4);
  }
}