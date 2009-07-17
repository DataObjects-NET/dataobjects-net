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

    public const string PostgreSql80 = "postgresql://do4test:do4testpwd@127.0.0.1:8032/do40test?Encoding=ASCII&Pooling=on&MinPoolSize=1&MaxPoolSize=5";
    public const string PostgreSql81 = "postgresql://do4test:do4testpwd@127.0.0.1:8132/do40test?Encoding=ASCII&Pooling=on&MinPoolSize=1&MaxPoolSize=5";
    public const string PostgreSql82 = "postgresql://do4test:do4testpwd@127.0.0.1:8232/do40test?Encoding=ASCII&Pooling=on&MinPoolSize=1&MaxPoolSize=5";
    public const string PostgreSql83 = "postgresql://do4test:do4testpwd@127.0.0.1:8332/do40test?Encoding=ASCII&Pooling=on&MinPoolSize=1&MaxPoolSize=5";
    public const string PostgreSql84 = "postgresql://do4test:do4testpwd@127.0.0.1:8432/do40test?Encoding=ASCII&Pooling=on&MinPoolSize=1&MaxPoolSize=5";

    public const string VistaDb = "vistadb://localhost/VistaDb/VDBTests.vdb3";
    public const string VistaDbAw = "vistadb://localhost/VistaDb/AdventureWorks.vdb3";

    public const string Oracle9  = "oracle://test:test@localhost:5509/ora09";
    public const string Oracle10 = "oracle://test:test@localhost:5510/ora10";
    public const string Oracle11 = "oracle://test:test@localhost:5511/ora11";
  }
}
