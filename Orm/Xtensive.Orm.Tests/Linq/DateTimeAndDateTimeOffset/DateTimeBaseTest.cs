// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.09.15

using System;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset
{
  public abstract class DateTimeBaseTest : BaseTest
  {
    protected static readonly TimeSpan FirstOffset = new TimeSpan(2, 45, 0); // +02:45
    protected static readonly TimeSpan SecondOffset = new TimeSpan(-11, 10, 0); // -11:10
    protected static readonly TimeSpan WrongOffset = new TimeSpan(4, 35, 0); // +04:35

    protected static readonly DateTime FirstDateTime = new DateTime(2016, 01, 02, 03, 04, 05, DateTimeKind.Unspecified);
    protected static readonly DateTime SecondDateTime = new DateTime(2019, 11, 23, 22, 58, 57, DateTimeKind.Unspecified);
    protected static readonly DateTime WrongDateTime = new DateTime(2017, 02, 03, 04, 05, 06, DateTimeKind.Unspecified);
    protected static readonly DateTime NullableDateTime = SecondDateTime;

    protected static readonly DateTime FirstMillisecondDateTime = FirstDateTime.AddMilliseconds(321);
    protected static readonly DateTime SecondMillisecondDateTime = SecondDateTime.AddMilliseconds(987);
    protected static readonly DateTime WrongMillisecondDateTime = WrongDateTime.AddMilliseconds(654);

    protected override void RegisterTypes(DomainConfiguration configuration)
    {
      configuration.Types.Register(typeof (SingleDateTimeEntity));
      configuration.Types.Register(typeof (DateTimeEntity));
      configuration.Types.Register(typeof (MillisecondDateTimeEntity));
      configuration.Types.Register(typeof (NullableDateTimeEntity));
    }

    protected override void PopulateEntities(Session session)
    {
      new SingleDateTimeEntity {
        DateTime = FirstDateTime,
        MillisecondDateTime = FirstMillisecondDateTime,
        NullableDateTime = NullableDateTime
      };

      new DateTimeEntity { DateTime = FirstDateTime };
      new DateTimeEntity { DateTime = FirstDateTime };
      new DateTimeEntity { DateTime = FirstDateTime.Date };
      new DateTimeEntity { DateTime = SecondDateTime };
      new DateTimeEntity { DateTime = SecondDateTime.Date };
      new DateTimeEntity { DateTime = new DateTime(FirstDateTime.Year, FirstDateTime.Month, FirstDateTime.Day, FirstDateTime.Hour, FirstDateTime.Minute, 0) };
      new DateTimeEntity { DateTime = new DateTime(FirstDateTime.Ticks, DateTimeKind.Local) };
      new DateTimeEntity { DateTime = FirstDateTime.Add(new TimeSpan(987, 23, 34, 45)) };
      new DateTimeEntity { DateTime = FirstDateTime.AddYears(1) };
      new DateTimeEntity { DateTime = FirstDateTime.AddYears(-2) };
      new DateTimeEntity { DateTime = FirstDateTime.AddMonths(44) };
      new DateTimeEntity { DateTime = FirstDateTime.AddMonths(-55) };
      new DateTimeEntity { DateTime = SecondDateTime.AddHours(5) };
      new DateTimeEntity { DateTime = SecondDateTime.AddHours(-15) };
      new DateTimeEntity { DateTime = SecondDateTime.AddMinutes(59) };
      new DateTimeEntity { DateTime = SecondDateTime.AddMinutes(-49) };
      new DateTimeEntity { DateTime = SecondDateTime.AddSeconds(57) };
      new DateTimeEntity { DateTime = SecondDateTime.AddSeconds(-5) };

      var dateTime = FirstDateTime.AddYears(10);
      for (var i = 0; i < 60; ++i)
        new DateTimeEntity { DateTime = dateTime.AddSeconds(i) };

      new MillisecondDateTimeEntity { DateTime = FirstMillisecondDateTime };
      new MillisecondDateTimeEntity { DateTime = FirstMillisecondDateTime };
      new MillisecondDateTimeEntity { DateTime = FirstMillisecondDateTime.Date };
      new MillisecondDateTimeEntity { DateTime = SecondMillisecondDateTime };
      new MillisecondDateTimeEntity { DateTime = SecondMillisecondDateTime.Date };
      new MillisecondDateTimeEntity { DateTime = new DateTime(FirstMillisecondDateTime.Year, FirstMillisecondDateTime.Month, FirstMillisecondDateTime.Day, FirstMillisecondDateTime.Hour, FirstMillisecondDateTime.Minute, 0) };
      new MillisecondDateTimeEntity { DateTime = new DateTime(FirstMillisecondDateTime.Year, FirstMillisecondDateTime.Month, FirstMillisecondDateTime.Day, FirstMillisecondDateTime.Hour, FirstMillisecondDateTime.Minute, FirstMillisecondDateTime.Second, 0) };
      new MillisecondDateTimeEntity { DateTime = new DateTime(FirstMillisecondDateTime.Ticks, DateTimeKind.Local) };
      new MillisecondDateTimeEntity { DateTime = FirstMillisecondDateTime.Add(new TimeSpan(987, 23, 34, 45)) };

      var index = 0;
      foreach (var dateTimeEntity1 in Query.All<DateTimeEntity>())
        new MillisecondDateTimeEntity(dateTimeEntity1, ++index % 3 == 0 ? FirstMillisecondDateTime.Millisecond : SecondMillisecondDateTime.Millisecond);

      dateTime = FirstMillisecondDateTime.AddYears(10);
      for (var i = 0; i < 1000; ++i)
        new MillisecondDateTimeEntity { DateTime = dateTime.AddMilliseconds(i) };

      foreach (var dateTimeEntity in Query.All<DateTimeEntity>())
        new NullableDateTimeEntity(dateTimeEntity);

      new NullableDateTimeEntity { DateTime = null };
      new NullableDateTimeEntity { DateTime = null };
    }
  }
}