// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.02

namespace Xtensive.Orm.Tests.Sql
{
  public static class TestUrl
  {
    public const string SqlServer2005Aw = "sqlserver://appserver/AdventureWorks?Connection Timeout=5";
    
    public const string SqlServerCe35Northwind = "sqlserverce://localhost/SqlServerCe/Northwind.sdf";

    public const string MySql50 = "mysql://root:admin@127.0.0.1:3306/sakila?Connection Timeout=5";

    public const string Firebird25_AgentThompson =
      @"firebird://SYSDBA:masterkey@agentthompson/C:\Program Files\Firebird\Firebird_2_5\do40test.fdb?Port=3050&Dialect=3&Charset=NONE&Role=&Connection lifetime=15&Pooling=true&MinPoolSize=0&MaxPoolSize=50&Packet Size=8192&ServerType=0";
    public const string Sqlite3 = @"sqlite://localhost/Sqlite\Northwind.sl3";
  }
}
