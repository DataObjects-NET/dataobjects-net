// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.07

using System;
using NUnit.Framework;
using Xtensive.Storage.Providers.PgSql;

namespace Xtensive.Storage.Tests.Storage.Providers.PgSql
{
  [TestFixture]
  public class TimeSpanParsingTest
  {
    [Test]
    public void MainTest()
    {
      Check("12:34:56.7890000");
      Check("-12:34:56.7890000");
      Check("-12:34:56.7890000");
      Check("4.11:25:03.2110000");
      Check("-5.12:34:56.7890000");
      Check("-5.11:25:03.2110000");
      Check("00:00:00.2000000");
      Check("-00:00:00.2000000");
      Check("00:00:00.0200000");
      Check("-00:00:00.0200000");
      Check("-00:00:01");
      Check("-00:01:01");
      Check("04:08:01.0230000");
      Check("04:08:01.0230000");
      Check("04:08:01.0240000");
      Check("-04:08:01.0260000");
      Check("-03:51:58.9770000");
      Check("03:52:00.9760000");
      Check("-04:07:59.0260000");
      Check("-00:00:00.0020000");
      Check("00:00:02.9980000");
      Check("00:04:02.9980000");
      Check("05:04:02.9980000");
      Check("-100000.00:00:00.0230000");
    }

    private static void Check(string timeSpan)
    {
      var expected = TimeSpan.Parse(timeSpan);
      var actual = SqlValueTypeMapper.StringToTimeSpan(SqlValueTypeMapper.TimeSpanToString(expected));
      Assert.AreEqual(expected, actual);
    }
  }
}
