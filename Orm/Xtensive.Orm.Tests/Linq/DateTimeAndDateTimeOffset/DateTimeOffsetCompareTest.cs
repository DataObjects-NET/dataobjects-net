// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.08.01

using System;
using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset
{
  public class DateTimeOffsetCompareTest : BaseDateTimeAndDateTimeOffsetTest
  {
    protected override void CheckRequirements()
    {
      Require.AnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);
    }

    [Test]
    public void EqualsTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset==FirstDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset==FirstMillisecondDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset==NullableDateTimeOffset);

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset==WrongDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset==WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset==WrongDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset==null);
      });
    }

    [Test]
    public void EqualsToDateTimeOffsetWithAnotherOffset()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset==FirstDateTimeOffset.ToOffset(FirstOffset));
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset==FirstDateTimeOffset.ToOffset(SecondOffset));
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset==FirstDateTimeOffset.ToOffset(TimeSpan.Zero));
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset==FirstDateTimeOffset.ToLocalTime());
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset==FirstDateTimeOffset.ToUniversalTime());

        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset==FirstMillisecondDateTimeOffset.ToOffset(FirstOffset));
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset==FirstMillisecondDateTimeOffset.ToOffset(SecondOffset));
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset==FirstMillisecondDateTimeOffset.ToOffset(TimeSpan.Zero));
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset==FirstMillisecondDateTimeOffset.ToLocalTime());
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset==FirstMillisecondDateTimeOffset.ToUniversalTime());

        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset==NullableDateTimeOffset.ToOffset(FirstOffset));
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset==NullableDateTimeOffset.ToOffset(SecondOffset));
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset==NullableDateTimeOffset.ToOffset(TimeSpan.Zero));
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset==NullableDateTimeOffset.ToLocalTime());
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset==NullableDateTimeOffset.ToUniversalTime());

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset==WrongDateTimeOffset.ToOffset(FirstOffset));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset==WrongDateTimeOffset.ToOffset(SecondOffset));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset==WrongDateTimeOffset.ToOffset(TimeSpan.Zero));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset==WrongDateTimeOffset.ToLocalTime());
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset==WrongDateTimeOffset.ToUniversalTime());

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset==WrongMillisecondDateTimeOffset.ToOffset(FirstOffset));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset==WrongMillisecondDateTimeOffset.ToOffset(SecondOffset));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset==WrongMillisecondDateTimeOffset.ToOffset(TimeSpan.Zero));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset==WrongMillisecondDateTimeOffset.ToLocalTime());
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset==WrongMillisecondDateTimeOffset.ToUniversalTime());

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset==WrongDateTimeOffset.ToOffset(FirstOffset));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset==WrongDateTimeOffset.ToOffset(SecondOffset));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset==WrongDateTimeOffset.ToOffset(TimeSpan.Zero));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset==WrongDateTimeOffset.ToLocalTime());
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset==WrongDateTimeOffset.ToUniversalTime());
      });
    }

    [Test]
    public void EqualsToUtcDateTime()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset==FirstDateTimeOffset.UtcDateTime);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset==FirstMillisecondDateTimeOffset.UtcDateTime);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset==NullableDateTimeOffset.UtcDateTime);

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset==WrongDateTimeOffset.UtcDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset==WrongMillisecondDateTimeOffset.UtcDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset==WrongDateTimeOffset.UtcDateTime);
      });
    }

    [Test]
    public void EqualsToLocalDateTime()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset==FirstDateTimeOffset.LocalDateTime);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset==FirstMillisecondDateTimeOffset.LocalDateTime);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset==NullableDateTimeOffset.LocalDateTime);

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset==WrongDateTimeOffset.LocalDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset==WrongMillisecondDateTimeOffset.LocalDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset==WrongDateTimeOffset.LocalDateTime);
      });
    }

    [Test]
    public void CompareTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset > FirstDateTimeOffset.AddHours(-1));
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset > FirstMillisecondDateTimeOffset.AddMilliseconds(-1));
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset > NullableDateTimeOffset.AddYears(-1));

        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset < FirstDateTimeOffset.AddHours(1));
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset < FirstMillisecondDateTimeOffset.AddMilliseconds(1));
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset < NullableDateTimeOffset.AddYears(1));

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset > FirstDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset < FirstMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset > NullableDateTimeOffset.AddSeconds(11));
      });
    }

    [Test]
    public void CompareToDateTimeTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset > FirstDateTimeOffset.ToLocalTime().AddHours(-1));
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset > FirstMillisecondDateTimeOffset.ToUniversalTime().AddMilliseconds(-1));
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset > NullableDateTimeOffset.ToLocalTime().Date);

        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset < FirstDateTimeOffset.ToLocalTime().AddHours(1));
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset < FirstMillisecondDateTimeOffset.ToUniversalTime().AddMilliseconds(1));
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset < NullableDateTimeOffset.ToUniversalTime().AddYears(1));

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset > FirstDateTimeOffset.ToLocalTime());
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset < FirstMillisecondDateTimeOffset.ToLocalTime());
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset > NullableDateTimeOffset.AddDays(1).ToUniversalTime());
      });
    }

    [Test(Description = "Might be failed by reasons described in issue DO-657")]
    public void CompareToNullableDateTimeOffsetToOffsetTest()
    {
      DateTimeOffset? nullableDateTimeOffset = NullableDateTimeOffset;
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset!=NullableDateTimeOffset.ToOffset(FirstOffset)); // works fine
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset!=nullableDateTimeOffset.Value.ToOffset(FirstOffset)); // failed
      });
    }

    [Test(Description = "Might be failed by reasons described in issue DO-657")]
    public void CompareToNullableDateTimeOffsetGetUtcDateTimeTest()
    {
      DateTimeOffset? nullableDateTimeOffset = NullableDateTimeOffset;
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset!=NullableDateTimeOffset.UtcDateTime);
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset!=nullableDateTimeOffset.Value.UtcDateTime);
      });
    }

    [Test(Description = "Might be failed by reasons described in issue DO-657")]
    public void CompareToNullableDateTimeOffsetGetLocalDateTimeTest()
    {
      DateTimeOffset? nullableDateTimeOffset = NullableDateTimeOffset;
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset!=NullableDateTimeOffset.LocalDateTime);
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset!=nullableDateTimeOffset.Value.LocalDateTime);
      });
    }
  }
}
