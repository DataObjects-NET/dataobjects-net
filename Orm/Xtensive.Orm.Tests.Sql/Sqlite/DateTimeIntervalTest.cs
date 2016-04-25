// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2006.04.21

using System;
using NUnit.Framework;
using Xtensive.Sql;

namespace Xtensive.Orm.Tests.Sql.Sqlite
{
  [TestFixture]
  public class DateTimeIntervalTest : Sql.DateTimeIntervalTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Sqlite);
    }

    [Test(Description = "sqlite operation restriction: add interval works only for even milliseconds, for odd milliseconds - one millisecond error")]
    public override void DateTimeAddIntervalTest()
    {
      // sqlite operation restriction:
      // SELECT strftime('%Y-%m-%d %H:%M:%f', '2001-01-01 01:01:01.001', '1 seconds') = '2001-01-01 01:01:02.000'
      // SELECT strftime('%Y-%m-%d %H:%M:%f', '2001-01-01 01:01:01.002', '1 seconds') = '2001-01-01 01:01:02.002'
      CheckEquality(
        SqlDml.DateTimePlusInterval(new DateTime(2001, 1, 1, 1, 1, 1, 2), new TimeSpan(10, 10, 10, 10, 10)),
        new DateTime(2001, 1, 11, 11, 11, 11, 12));
    }
  }
}