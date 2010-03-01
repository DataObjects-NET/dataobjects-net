﻿// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.02

namespace Xtensive.Sql.Tests
{
  public static class TestUrl
  {
    public const string SqlServer2005 = "sqlserver://localhost/DO40-Tests?Connection Timeout=5";
    public const string SqlServer2005Aw = "sqlserver://localhost/AdventureWorks?Connection Timeout=5";
    public const string SqlServer2008 = @"sqlserver://localhost\Sql2008/DO40-Tests?Connection Timeout=5";
    public const string SqlServer2008Aw = @"sqlserver://localhost\Sql2008/AdventureWorks?Connection Timeout=5";

    public const string PostgreSql80 = "postgresql://do4test:do4testpwd@127.0.0.1:8032/do40test?CommandTimeout=5";
    public const string PostgreSql81 = "postgresql://do4test:do4testpwd@127.0.0.1:8132/do40test?CommandTimeout=5";
    public const string PostgreSql82 = "postgresql://do4test:do4testpwd@127.0.0.1:8232/do40test?CommandTimeout=5";
    public const string PostgreSql83 = "postgresql://do4test:do4testpwd@127.0.0.1:8332/do40test?CommandTimeout=5";
    public const string PostgreSql84 = "postgresql://do4test:do4testpwd@127.0.0.1:8432/do40test?CommandTimeout=5";
    
    public const string Oracle9  = "oracle://test:test@localhost:5509/ora09?Connection Timeout=5";
    public const string Oracle10 = "oracle://test:test@localhost:5510/ora10?Connection Timeout=5";
    public const string Oracle11 = "oracle://test:test@localhost:5511/ora11?Connection Timeout=5";

    public const string SqlServerCe35 = "sqlserverce://localhost/SqlServerCe/DO40-Test.sdf";
    public const string SqlServerCe35Northwind = "sqlserverce://localhost/SqlServerCe/Northwind.sdf";
  }
}
