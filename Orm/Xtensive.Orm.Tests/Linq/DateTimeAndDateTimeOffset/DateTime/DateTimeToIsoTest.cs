// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.08.01

using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimes
{
  public class DateTimeToIsoTest : DateTimeBaseTest
  {
    [Test]
    public void ToIsoStringTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.ToString("s")==FirstDateTime.ToString("s"));
        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.ToString("s")==FirstDateTime.AddMinutes(1).ToString("s"));
      });
    }
  }
}
