// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.17

namespace Xtensive.Orm.Tests.Sql
{
  public static class TestConnectionString
  {
    // Copied from TestUrl, keep in sync

    public const string PostgreSql84 =
      "HOST=127.0.0.1;PORT=8432;DATABASE=do40test;USER ID=do4test;PASSWORD=do4testpwd";

    public const string Oracle11 =
      "DATA SOURCE=\"(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=5511))(CONNECT_DATA=(SERVICE_NAME=ora11)))\";USER ID=test;PASSWORD=test";

    public const string SqlServer2005 =
      "Data Source=localhost;Initial Catalog=DO40-Tests;Integrated Security=True;Persist Security Info=False";

    public const string MySql50 =
      "Server=localhost;Database=do40test;Uid=root;Pwd=admin;";

    public const string Firebird25 =
      @"firebird://do4:do4@127.0.0.1:3050/do4test1"; // proto://[[user[:password]@]host[:port]]/resource

  }
}