// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.05.26

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimeTestModels;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset
{
  public class DateTimeMillisecondAccuracyTest : DateTimeTest
  {
    public override DateTime DefaultDateTime
    {
      get { return new DateTime(2016, 01, 02, 03, 04, 05, 333); }
    }

    public override DateTime WrongDateTime
    {
      get { return base.WrongDateTime.AddMilliseconds(444); }
    }

    public override DateTime DateTimeForSubstract
    {
      get { return base.DateTimeForSubstract.AddMilliseconds(456); }
    }

    public override TimeSpan DefaultTimeSpan
    {
      get { return base.DefaultTimeSpan.Add(TimeSpan.FromMilliseconds(345)); }
    }

    public override TimeSpan MultipleEntityDateStep
    {
      get { return TimeSpan.FromMilliseconds(1); }
    }

    protected override void ExtractDateTimeProtected()
    {
      base.ExtractDateTimeProtected();
      RunTest(c => c.DateTime.Millisecond==DefaultDateTime.Millisecond);
      RunWrongTest(c => c.DateTime.Millisecond==WrongDateTime.Millisecond);
    }

    protected override void DateTimeOperationsProtected()
    {
      base.DateTimeOperationsProtected();

      // Sometimes add/substract millisecond don't work for specify value (accuracy error)
      for (var millisecond = 300; millisecond < 320; ++millisecond) {
        var millisecond1 = millisecond;
        RunTest(c => c.DateTime.AddMilliseconds(millisecond1)==DefaultDateTime.AddMilliseconds(millisecond1));
        RunWrongTest(c => c.DateTime.AddMilliseconds(millisecond1)==WrongDateTime.AddMilliseconds(millisecond1));
      }
    }

    protected virtual void MillisecondAccuracyProtected()
    {
      // Sometimes add/substract millisecond don't work for specify value (accuracy error)
      const int totalMilliseconds = 10;
      var startDateTime = DefaultDateTime.AddYears(11);
      for (var millisecond = 0; millisecond < totalMilliseconds; ++millisecond)
        new SingleEntity { DateTime = startDateTime.AddMilliseconds(millisecond) };

      // sqlite example: strftime('%Y:%m:%d %H:%M:%f', '2016-01-01 01:01:01.001', '+0 seconds') == '2016-01-01 01:01:01.000'
      for (var millisecond = 0; millisecond < totalMilliseconds; ++millisecond) {
        var millisecond1 = millisecond;
        RunTest(c => c.DateTime==startDateTime.AddMilliseconds(millisecond1));
        RunTest(c => c.DateTime.AddYears(1)==startDateTime.AddMilliseconds(millisecond1).AddYears(1));
        RunTest(c => c.DateTime.AddSeconds(0)==startDateTime.AddMilliseconds(millisecond1).AddSeconds(0));
        RunTest(c => c.DateTime.AddSeconds(-3)==startDateTime.AddMilliseconds(millisecond1).AddSeconds(-3));
        RunTest(c => c.DateTime.AddMilliseconds(12345)==startDateTime.AddMilliseconds(millisecond1).AddMilliseconds(12345));
      }
    }

    [Test]
    public void MillisecondAccuracyTest()
    {
      OpenSessionAndAction(MillisecondAccuracyProtected);
    }
  }
}
