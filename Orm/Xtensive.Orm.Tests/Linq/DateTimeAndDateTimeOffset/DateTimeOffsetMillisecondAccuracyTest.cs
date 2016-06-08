// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.05.30

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimeOffsetTestModels;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset
{
  public class DateTimeOffsetMillisecondAccuracyTest : DateTimeOffsetTest
  {
    #region Default values

    public override DateTimeOffset DefaultDateTimeOffset
    {
      get { return base.DefaultDateTimeOffset.AddMilliseconds(333); }
    }

    public override DateTimeOffset WrongDateTimeOffset
    {
      get { return base.WrongDateTimeOffset.AddMilliseconds(444); }
    }

    public override DateTime DateTimeForSubstract
    {
      get { return base.DateTimeForSubstract.AddMilliseconds(987); }
    }

    public override DateTimeOffset DateTimeOffsetForSubstract
    {
      get { return base.DateTimeOffsetForSubstract.AddMilliseconds(456); }
    }

    public override TimeSpan DefaultTimeSpan
    {
      get { return base.DefaultTimeSpan.Add(TimeSpan.FromMilliseconds(345)); }
    }

    public override TimeSpan MultipleEntityDateStep
    {
      get { return TimeSpan.FromMilliseconds(1); }
    }

    #endregion

    [Test]
    public void MillisecondAccuracyTest()
    {
      OpenSessionAndAction(MillisecondAccuracyProtected);
    }

    #region Implementation of own and ancestor's tests

    protected override void ExtractDateTimeOffsetProtected()
    {
      base.ExtractDateTimeOffsetProtected();
      RunTest(c => c.DateTimeOffset.Millisecond==DefaultDateTimeOffset.Millisecond);
      RunWrongTest(c => c.DateTimeOffset.Millisecond==WrongDateTimeOffset.Millisecond);
    }

    protected override void DateTimeOffsetOperationsProtected()
    {
      base.DateTimeOffsetOperationsProtected();

      // Sometimes add/substract millisecond don't work for specify value (accuracy error)
      for (var millisecond = 300; millisecond < 320; ++millisecond) {
        var millisecond1 = millisecond;
        RunTest(c => c.DateTimeOffset.AddMilliseconds(millisecond1)==DefaultDateTimeOffset.AddMilliseconds(millisecond1));
        RunWrongTest(c => c.DateTimeOffset.AddMilliseconds(millisecond1)==WrongDateTimeOffset.AddMilliseconds(millisecond1));
      }
    }

    protected virtual void MillisecondAccuracyProtected()
    {
      // Sometimes add/substract millisecond don't work for specify value (accuracy error)
      const int totalMilliseconds = 10;
      var startDateTimeOffset = DefaultDateTimeOffset.AddYears(11);
      for (var millisecond = 0; millisecond < totalMilliseconds; ++millisecond)
        new SingleEntity { DateTimeOffset = startDateTimeOffset.AddMilliseconds(millisecond) };

      // sqlite example: strftime('%Y:%m:%d %H:%M:%f', '2016-01-01 01:01:01.001', '+0 seconds') == '2016-01-01 01:01:01.000'
      for (var millisecond = 0; millisecond < totalMilliseconds; ++millisecond) {
        var millisecond1 = millisecond;
        RunTest(c => c.DateTimeOffset==startDateTimeOffset.AddMilliseconds(millisecond1));
        RunTest(c => c.DateTimeOffset.AddYears(1)==startDateTimeOffset.AddMilliseconds(millisecond1).AddYears(1));
        RunTest(c => c.DateTimeOffset.AddSeconds(0)==startDateTimeOffset.AddMilliseconds(millisecond1).AddSeconds(0));
        RunTest(c => c.DateTimeOffset.AddSeconds(-3)==startDateTimeOffset.AddMilliseconds(millisecond1).AddSeconds(-3));
        RunTest(c => c.DateTimeOffset.AddMilliseconds(12345)==startDateTimeOffset.AddMilliseconds(millisecond1).AddMilliseconds(12345));
      }
    }

    #endregion;
  }
}
