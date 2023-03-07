// Copyright (C) 2016-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
#if NET6_0_OR_GREATER

    protected static readonly DateOnly FirstDateOnly = DateOnly.FromDateTime(FirstDateTime);
    protected static readonly DateOnly SecondDateOnly = DateOnly.FromDateTime(SecondDateTime);
    protected static readonly DateOnly NullableDateOnly = DateOnly.FromDateTime(SecondDateTime);
    protected static readonly DateOnly WrongDateOnly = DateOnly.FromDateTime(WrongDateTime);

    protected static readonly TimeOnly FirstTimeOnly = TimeOnly.FromDateTime(FirstDateTime);
    protected static readonly TimeOnly SecondTimeOnly = TimeOnly.FromDateTime(SecondDateTime);
    protected static readonly TimeOnly NullableTimeOnly = TimeOnly.FromDateTime(SecondDateTime);
    protected static readonly TimeOnly WrongTimeOnly = TimeOnly.FromDateTime(WrongDateTime);

    protected static readonly TimeOnly FirstMillisecondTimeOnly = TimeOnly.FromDateTime(FirstDateTime.AddMilliseconds(321));
    protected static readonly TimeOnly SecondMillisecondTimeOnly = TimeOnly.FromDateTime(SecondDateTime.AddMilliseconds(987));
    protected static readonly TimeOnly WrongMillisecondTimeOnly = TimeOnly.FromDateTime(WrongDateTime.AddMilliseconds(654));
#endif

    protected override void RegisterTypes(DomainConfiguration configuration)
    {
      configuration.Types.Register(typeof(SingleDateTimeEntity));
      configuration.Types.Register(typeof(DateTimeEntity));
      configuration.Types.Register(typeof(MillisecondDateTimeEntity));
      configuration.Types.Register(typeof(NullableDateTimeEntity));
      configuration.Types.Register(typeof(AllPossiblePartsEntity));
#if NET6_0_OR_GREATER
      configuration.Types.Register(typeof(DateOnlyEntity));
      configuration.Types.Register(typeof(SingleDateOnlyEntity));
      configuration.Types.Register(typeof(TimeOnlyEntity));
      configuration.Types.Register(typeof(SingleTimeOnlyEntity));
#endif
    }

    protected override void PopulateEntities(Session session)
    {
      _ = new SingleDateTimeEntity(session) {
        DateTime = FirstDateTime,
        MillisecondDateTime = FirstMillisecondDateTime,
        NullableDateTime = NullableDateTime
      };
#if NET6_0_OR_GREATER

      _ = new SingleDateOnlyEntity(session) {
        DateOnly = FirstDateOnly,
        NullableDateOnly = NullableDateOnly,
      };

      _ = new DateOnlyEntity(session) { DateOnly = FirstDateOnly, NullableDateOnly = FirstDateOnly };
      _ = new DateOnlyEntity(session) { DateOnly = FirstDateOnly, NullableDateOnly = FirstDateOnly };
      _ = new DateOnlyEntity(session) { DateOnly = FirstDateOnly, NullableDateOnly = NullableDateOnly };
      _ = new DateOnlyEntity(session) { DateOnly = SecondDateOnly, NullableDateOnly = NullableDateOnly };
      _ = new DateOnlyEntity(session) { DateOnly = FirstDateOnly, NullableDateOnly = null };
      _ = new DateOnlyEntity(session) { DateOnly = SecondDateOnly, NullableDateOnly = null };
      _ = new DateOnlyEntity(session) { DateOnly = FirstDateOnly.AddYears(1), NullableDateOnly = FirstDateOnly.AddYears(1) };
      _ = new DateOnlyEntity(session) { DateOnly = FirstDateOnly.AddYears(-2), NullableDateOnly = FirstDateOnly.AddYears(-2) };
      _ = new DateOnlyEntity(session) { DateOnly = FirstDateOnly.AddMonths(44), NullableDateOnly = FirstDateOnly.AddMonths(44) };
      _ = new DateOnlyEntity(session) { DateOnly = FirstDateOnly.AddMonths(-55), NullableDateOnly = FirstDateOnly.AddMonths(-55) };
      _ = new DateOnlyEntity(session) { DateOnly = FirstDateOnly.AddDays(444), NullableDateOnly = FirstDateOnly.AddDays(444) };
      _ = new DateOnlyEntity(session) { DateOnly = FirstDateOnly.AddDays(-555), NullableDateOnly = FirstDateOnly.AddDays(-555) };

      _ = new SingleTimeOnlyEntity(session) {
        TimeOnly = FirstTimeOnly,
        NullableTimeOnly = NullableTimeOnly,
        MillisecondTimeOnly = FirstMillisecondTimeOnly
      };

      _ = new TimeOnlyEntity(session) {
        TimeOnly = FirstTimeOnly,
        NullableTimeOnly = FirstTimeOnly,
        MillisecondTimeOnly = FirstTimeOnly
      };
      _ = new TimeOnlyEntity(session) {
        TimeOnly = FirstTimeOnly,
        NullableTimeOnly = FirstTimeOnly,
        MillisecondTimeOnly = FirstTimeOnly
      };
      _ = new TimeOnlyEntity(session) {
        TimeOnly = FirstTimeOnly,
        NullableTimeOnly = NullableTimeOnly,
        MillisecondTimeOnly = FirstMillisecondTimeOnly
      };
      _ = new TimeOnlyEntity(session) {
        TimeOnly = FirstTimeOnly,
        NullableTimeOnly = NullableTimeOnly,
        MillisecondTimeOnly = FirstMillisecondTimeOnly
      };
      _ = new TimeOnlyEntity(session) {
        TimeOnly = FirstTimeOnly,
        NullableTimeOnly = null,
        MillisecondTimeOnly = FirstMillisecondTimeOnly
      };
      _ = new TimeOnlyEntity(session) {
        TimeOnly = FirstTimeOnly,
        NullableTimeOnly = null,
        MillisecondTimeOnly = FirstMillisecondTimeOnly
      };
      _ = new TimeOnlyEntity(session) {
        TimeOnly = FirstTimeOnly.AddHours(5),
        MillisecondTimeOnly = FirstMillisecondTimeOnly.AddHours(5),
        NullableTimeOnly = NullableTimeOnly.AddHours(5)
      };
      _ = new TimeOnlyEntity(session) {
        TimeOnly = FirstTimeOnly.AddHours(-15),
        MillisecondTimeOnly = FirstMillisecondTimeOnly.AddHours(-15),
        NullableTimeOnly = NullableTimeOnly.AddHours(-15),
      };
      _ = new TimeOnlyEntity(session) {
        TimeOnly = FirstTimeOnly.AddMinutes(59),
        MillisecondTimeOnly = FirstMillisecondTimeOnly.AddMinutes(59),
        NullableTimeOnly = NullableTimeOnly.AddMinutes(59),
      };
      _ = new TimeOnlyEntity(session) {
        TimeOnly = FirstTimeOnly.AddMinutes(-49),
        MillisecondTimeOnly = FirstMillisecondTimeOnly.AddMinutes(-49),
        NullableTimeOnly = NullableTimeOnly.AddMinutes(-49),
      };
      _ = new TimeOnlyEntity(session) {
        TimeOnly = FirstTimeOnly.Add(new TimeSpan(0, 0, 57)),
        MillisecondTimeOnly = FirstMillisecondTimeOnly.Add(new TimeSpan(0, 0, 57)),
        NullableTimeOnly = NullableTimeOnly.Add(new TimeSpan(0, 0, 57)),
      };
      _ = new TimeOnlyEntity(session) {
        TimeOnly = FirstTimeOnly.Add(new TimeSpan(0, 0,-5)),
        MillisecondTimeOnly = FirstMillisecondTimeOnly.Add(new TimeSpan(0, 0,-5)),
        NullableTimeOnly = NullableTimeOnly.Add(new TimeSpan(0, 0,-5)),
      };
#endif

      _ = new DateTimeEntity(session, FirstDateTime);
      _ = new DateTimeEntity(session, FirstDateTime);
      _ = new DateTimeEntity(session, FirstDateTime.Date);
      _ = new DateTimeEntity(session, SecondDateTime);
      _ = new DateTimeEntity(session, SecondDateTime.Date);
      _ = new DateTimeEntity(session, new DateTime(FirstDateTime.Year, FirstDateTime.Month, FirstDateTime.Day, FirstDateTime.Hour, FirstDateTime.Minute, 0));
      _ = new DateTimeEntity(session, new DateTime(FirstDateTime.Ticks, DateTimeKind.Local));
      _ = new DateTimeEntity(session, FirstDateTime.Add(new TimeSpan(987, 23, 34, 45)));
      _ = new DateTimeEntity(session, FirstDateTime.AddYears(1));
      _ = new DateTimeEntity(session, FirstDateTime.AddYears(-2));
      _ = new DateTimeEntity(session, FirstDateTime.AddMonths(44));
      _ = new DateTimeEntity(session, FirstDateTime.AddMonths(-55));
      _ = new DateTimeEntity(session, SecondDateTime.AddHours(5));
      _ = new DateTimeEntity(session, SecondDateTime.AddHours(-15));
      _ = new DateTimeEntity(session, SecondDateTime.AddMinutes(59));
      _ = new DateTimeEntity(session, SecondDateTime.AddMinutes(-49));
      _ = new DateTimeEntity(session, SecondDateTime.AddSeconds(57));
      _ = new DateTimeEntity(session, SecondDateTime.AddSeconds(-5));

      var dateTime = FirstDateTime.AddYears(10);
      for (var i = 0; i < 60; ++i) {
        _ = new DateTimeEntity(session, dateTime.AddSeconds(i));
      }

      _ = new MillisecondDateTimeEntity(session) { DateTime = FirstMillisecondDateTime };
      _ = new MillisecondDateTimeEntity(session) { DateTime = FirstMillisecondDateTime };
      _ = new MillisecondDateTimeEntity(session) { DateTime = FirstMillisecondDateTime.Date };
      _ = new MillisecondDateTimeEntity(session) { DateTime = SecondMillisecondDateTime };
      _ = new MillisecondDateTimeEntity(session) { DateTime = SecondMillisecondDateTime.Date };
      _ = new MillisecondDateTimeEntity(session) { DateTime = new DateTime(FirstMillisecondDateTime.Year, FirstMillisecondDateTime.Month, FirstMillisecondDateTime.Day, FirstMillisecondDateTime.Hour, FirstMillisecondDateTime.Minute, 0) };
      _ = new MillisecondDateTimeEntity(session) { DateTime = new DateTime(FirstMillisecondDateTime.Year, FirstMillisecondDateTime.Month, FirstMillisecondDateTime.Day, FirstMillisecondDateTime.Hour, FirstMillisecondDateTime.Minute, FirstMillisecondDateTime.Second, 0) };
      _ = new MillisecondDateTimeEntity(session) { DateTime = new DateTime(FirstMillisecondDateTime.Ticks, DateTimeKind.Local) };
      _ = new MillisecondDateTimeEntity(session) { DateTime = FirstMillisecondDateTime.Add(new TimeSpan(987, 23, 34, 45)) };

      var index = 0;
      foreach (var dateTimeEntity1 in session.Query.All<DateTimeEntity>()) {
        var dtValue = dateTimeEntity1.DateTime.AddMilliseconds(++index % 3 == 0 ? FirstMillisecondDateTime.Millisecond : SecondMillisecondDateTime.Millisecond);
        _ = new MillisecondDateTimeEntity(session, dtValue);
      }

      dateTime = FirstMillisecondDateTime.AddYears(10);
      for (var i = 0; i < 1000; ++i) {
        _ = new MillisecondDateTimeEntity(session) { DateTime = dateTime.AddMilliseconds(i) };
      }

      foreach (var dateTimeEntity in session.Query.All<DateTimeEntity>()) {
        _ = new NullableDateTimeEntity(session) { DateTime = dateTimeEntity.DateTime };
      }

      _ = new NullableDateTimeEntity(session) { DateTime = null };
      _ = new NullableDateTimeEntity(session) { DateTime = null };

      _ = AllPossiblePartsEntity.FromDateTime(session, FirstMillisecondDateTime, 321);
    }
  }
}