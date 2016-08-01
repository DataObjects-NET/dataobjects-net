// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.08.01

using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset
{
  public class DateTimeCompareTest : BaseDateTimeAndDateTimeOffsetTest
  {
    [Test]
    public void EqualsTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime==FirstDateTime);
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime==FirstMillisecondDateTime);
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime==NullableDateTime);

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime==WrongDateTime);
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime==WrongMillisecondDateTime);
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime==WrongDateTime);
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime==null);
      });
    }

    [Test]
    public void CompareTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime > FirstDateTime.Date);
        RunTest<SingleDateTimeEntity>(c => c.DateTime > FirstDateTime.AddSeconds(-1));
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime > FirstMillisecondDateTime.AddMilliseconds(-1));

        RunTest<SingleDateTimeEntity>(c => c.DateTime < FirstDateTime.Date.AddDays(1));
        RunTest<SingleDateTimeEntity>(c => c.DateTime < FirstDateTime.AddSeconds(1));
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime < FirstMillisecondDateTime.AddMilliseconds(1));

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime > FirstDateTime);
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime > FirstMillisecondDateTime);
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime < FirstMillisecondDateTime.Date);
      });
    }
  }
}
