// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.04.26

using System;
using NUnit.Framework;
using Xtensive.Sql;

namespace Xtensive.Orm.Tests.Sql.Sqlite
{
  [TestFixture]
  public class DateTimeOffsetTest : Sql.DateTimeOffsetTest
  {
    protected override bool IsNanosecondSupported
    {
      get { return false; }
    }

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Sqlite);
    }

    public override void DateTimeOffsetMinusDateTimeOffsetTest()
    {
      var now = DateTimeOffset.Now;
      now = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Offset); //milliseconds part is not supported for sqlite
      CheckEquality(SqlDml.DateTimeOffsetMinusDateTimeOffset(DefaultDateTimeOffset, now), DefaultDateTimeOffset - now);
    }
  }
}
