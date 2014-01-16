// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2013.12.03

using NUnit.Framework;

namespace Xtensive.Orm.Tests.Sql.SqlServer.v10
{
  [TestFixture, Explicit]
  public class DateTimeOffsetTest : Sql.DateTimeOffsetTest
  {
    protected override string Url { get { return TestUrl.SqlServer2008; } }
  }
}
                                                                                                                                                                                                                                                               