// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.09.05

using System;
using NUnit.Framework;
using Xtensive.Sql;

namespace Xtensive.Orm.Tests.Sql.PostgreSql
{
  public class DateTimeOffsetTest : Sql.DateTimeOffsetTest
  {
    protected override bool ShouldTransformToLocalZone
    {
      get { return true; }
    }

    protected override bool IsNanosecondSupported
    {
      get { return false; }
    }

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
    }

    [Test]
    public override void DateTimeOffsetToUtcTimeTest()
    {
      Assert.Throws<NotSupportedException>(() => CheckEquality(SqlDml.DateTimeOffsetToUtcTime(DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).ToUniversalTime()));
    }
  }
}
