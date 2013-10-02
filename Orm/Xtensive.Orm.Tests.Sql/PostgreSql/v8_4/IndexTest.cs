// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.09.26

namespace Xtensive.Orm.Tests.Sql.PostgreSql.v8_4
{
  public class IndexTest : v8_3.IndexTest
  {
    protected override string Url { get { return TestUrl.PostgreSql84; } }
  }
}