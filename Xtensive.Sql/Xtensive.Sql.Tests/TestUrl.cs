// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.02

namespace Xtensive.Sql.Tests
{
  public static class TestUrl
  {
    public const string SqlServer2005 = "sqlserver://localhost/DO40-Tests";
    public const string SqlServer2005Aw = "sqlserver://localhost/AdventureWorks";
    public const string SqlServer2008 = @"sqlserver://localhost\Sql2008/DO40-Tests";
    public const string SqlServer2008Aw = @"sqlserver://localhost\Sql2008/AdventureWorks";

    public const string PostgreSql80 = "postgresql://do4test:do4testpwd@127.0.0.1:8032/do40test";
    public const string PostgreSql81 = "postgresql://do4test:do4testpwd@127.0.0.1:8132/do40test";
    public const string PostgreSql82 = "postgresql://do4test:do4testpwd@127.0.0.1:8232/do40test";
    public const string PostgreSql83 = "postgresql://do4test:do4testpwd@127.0.0.1:8332/do40test";
    public const string PostgreSql84 = "postgresql://do4test:do4testpwd@127.0.0.1:8432/do40test";

    public const string PostgreSql80LowTimeout = "postgresql://do4test:do4testpwd@127.0.0.1:8032/do40test?CommandTimeout=3";
    public const string PostgreSql81LowTimeout = "postgresql://do4test:do4testpwd@127.0.0.1:8132/do40test?CommandTimeout=3";
    public const string PostgreSql82LowTimeout = "postgresql://do4test:do4testpwd@127.0.0.1:8232/do40test?CommandTimeout=3";
    public const string PostgreSql83LowTimeout = "postgresql://do4test:do4testpwd@127.0.0.1:8332/do40test?CommandTimeout=3";
    public const string PostgreSql84LowTimeout = "postgresql://do4test:do4testpwd@127.0.0.1:8432/do40test?CommandTimeout=3";
    
    public const string Oracle9  = "oracle://test:test@localhost:5509/ora09";
    public const string Oracle10 = "oracle://test:test@localhost:5510/ora10";
    public const string Oracle11 = "oracle://test:test@localhost:5511/ora11";

    public const string SqlServerCe35 = "sqlserverce://localhost/SqlServerCe/DO40-Test.sdf";
    public const string SqlServerCe35Northwind = "sqlserverce://localhost/SqlServerCe/Northwind.sdf";
  }
}
