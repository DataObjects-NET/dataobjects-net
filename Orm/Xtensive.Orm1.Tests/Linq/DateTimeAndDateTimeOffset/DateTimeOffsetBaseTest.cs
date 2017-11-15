// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.09.15

using System;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset
{
  public abstract class DateTimeOffsetBaseTest : BaseTest
  {
    protected static readonly DateTime FirstDateTime = new DateTime(2016, 01, 02, 03, 04, 05, DateTimeKind.Unspecified);
    protected static readonly DateTime SecondDateTime = new DateTime(2019, 11, 23, 22, 58, 57, DateTimeKind.Unspecified);
    protected static readonly DateTime WrongDateTime = new DateTime(2017, 02, 03, 04, 05, 06, DateTimeKind.Unspecified);
    protected static readonly DateTime NullableDateTime = SecondDateTime;

    protected static readonly DateTime FirstMillisecondDateTime = FirstDateTime.AddMilliseconds(321);
    protected static readonly DateTime SecondMillisecondDateTime = SecondDateTime.AddMilliseconds(987);
    protected static readonly DateTime WrongMillisecondDateTime = WrongDateTime.AddMilliseconds(654);

    protected static readonly TimeSpan FirstOffset = new TimeSpan(2, 45, 0); // +02:45
    protected static readonly TimeSpan SecondOffset = new TimeSpan(-11, 10, 0); // -11:10
    protected static readonly TimeSpan WrongOffset = new TimeSpan(4, 35, 0); // +04:35

    protected static readonly DateTimeOffset FirstDateTimeOffset = new DateTimeOffset(FirstDateTime, FirstOffset);
    protected static readonly DateTimeOffset SecondDateTimeOffset = new DateTimeOffset(SecondDateTime, SecondOffset);
    protected static readonly DateTimeOffset WrongDateTimeOffset = new DateTimeOffset(WrongDateTime, WrongOffset);
    protected static readonly DateTimeOffset NullableDateTimeOffset = SecondDateTimeOffset;

    protected static readonly DateTimeOffset FirstMillisecondDateTimeOffset = new DateTimeOffset(FirstMillisecondDateTime, FirstOffset);
    protected static readonly DateTimeOffset SecondMillisecondDateTimeOffset = new DateTimeOffset(SecondMillisecondDateTime, SecondOffset);
    protected static readonly DateTimeOffset WrongMillisecondDateTimeOffset = new DateTimeOffset(WrongMillisecondDateTime, WrongOffset);

    protected override void RegisterTypes(DomainConfiguration configuration)
    {
      configuration.Types.Register(typeof (SingleDateTimeOffsetEntity));
      configuration.Types.Register(typeof (DateTimeOffsetEntity));
      configuration.Types.Register(typeof (MillisecondDateTimeOffsetEntity));
      configuration.Types.Register(typeof (NullableDateTimeOffsetEntity));
      configuration.Types.Register(typeof (DateTimeEntity));
    }

    protected override void InitializeCustomSettings(DomainConfiguration configuration)
    {
      var providerInfo = StorageProviderInfo.Instance.Info;
      if (providerInfo.ProviderName==WellKnown.Provider.PostgreSql) {
        var localZone = DateTimeOffset.Now.ToLocalTime().Offset;
        var localZoneString = ((localZone < TimeSpan.Zero) ? "-" : "+") + localZone.ToString(@"hh\:mm");
        configuration.ConnectionInitializationSql = string.Format("SET TIME ZONE INTERVAL '{0}' HOUR TO MINUTE", localZoneString);
      }
    }

    protected override void CheckRequirements()
    {
      Require.AnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);
    }

    protected override void PopulateEntities(Session session)
    {
      new SingleDateTimeOffsetEntity {
        DateTimeOffset = FirstDateTimeOffset,
        MillisecondDateTimeOffset = FirstMillisecondDateTimeOffset,
        NullableDateTimeOffset = NullableDateTimeOffset
      };

      DateTime[] dateTimes = new[] {
        FirstDateTime,
        FirstDateTime,
        FirstDateTime.Date,
        SecondDateTime,
        SecondDateTime.Date,
        new DateTime(FirstDateTime.Year, FirstDateTime.Month, FirstDateTime.Day, FirstDateTime.Hour, FirstDateTime.Minute, 0),
        new DateTime(FirstDateTime.Ticks, DateTimeKind.Unspecified),
        FirstDateTime.Add(new TimeSpan(987, 23, 34, 45)),FirstDateTime.AddYears(1),FirstDateTime.AddYears(-2),
        FirstDateTime.AddMonths(44),
        FirstDateTime.AddMonths(-55),
        SecondDateTime.AddHours(5),
        SecondDateTime.AddHours(-15),
        SecondDateTime.AddMinutes(59),
        SecondDateTime.AddMinutes(-49),
        SecondDateTime.AddSeconds(57),
        SecondDateTime.AddSeconds(-5),
      };

      DateTime[] dateTimesWithMilliseconds = new[] {
        FirstMillisecondDateTime,
        FirstMillisecondDateTime,
        FirstMillisecondDateTime.Date,
        SecondMillisecondDateTime,
        SecondMillisecondDateTime.Date,
        new DateTime(FirstMillisecondDateTime.Year, FirstMillisecondDateTime.Month, FirstMillisecondDateTime.Day, FirstMillisecondDateTime.Hour, FirstMillisecondDateTime.Minute, 0),
        new DateTime(FirstMillisecondDateTime.Year, FirstMillisecondDateTime.Month, FirstMillisecondDateTime.Day, FirstMillisecondDateTime.Hour, FirstMillisecondDateTime.Minute, FirstMillisecondDateTime.Second, 0),
        new DateTime(FirstMillisecondDateTime.Ticks, DateTimeKind.Unspecified),
        FirstMillisecondDateTime.Add(new TimeSpan(987, 23, 34, 45)),
      };

      new DateTimeOffsetEntity { DateTimeOffset = FirstDateTimeOffset };
      new DateTimeOffsetEntity { DateTimeOffset = FirstDateTimeOffset };
      new DateTimeOffsetEntity { DateTimeOffset = FirstDateTimeOffset.ToOffset(FirstOffset) };
      new DateTimeOffsetEntity { DateTimeOffset = FirstDateTimeOffset.ToOffset(SecondOffset) };
      new DateTimeOffsetEntity { DateTimeOffset = FirstDateTimeOffset.ToOffset(TimeSpan.Zero) };
      new DateTimeOffsetEntity { DateTimeOffset = FirstDateTimeOffset.Date };
      new DateTimeOffsetEntity { DateTimeOffset = SecondDateTimeOffset };
      new DateTimeOffsetEntity { DateTimeOffset = SecondDateTimeOffset.ToOffset(FirstOffset) };
      new DateTimeOffsetEntity { DateTimeOffset = SecondDateTimeOffset.ToOffset(SecondOffset) };
      new DateTimeOffsetEntity { DateTimeOffset = SecondDateTimeOffset.Date };
      new DateTimeOffsetEntity { DateTimeOffset = FirstDateTime };
      new DateTimeOffsetEntity { DateTimeOffset = new DateTimeOffset(FirstDateTime, TimeSpan.Zero) };

      var index = 0;
      foreach (var dateTime in dateTimes)
        new DateTimeOffsetEntity(dateTime, ++index % 3==0 ? FirstOffset : SecondOffset);
      
      new MillisecondDateTimeOffsetEntity { DateTimeOffset = FirstMillisecondDateTimeOffset };
      new MillisecondDateTimeOffsetEntity { DateTimeOffset = FirstMillisecondDateTimeOffset };
      new MillisecondDateTimeOffsetEntity { DateTimeOffset = FirstMillisecondDateTimeOffset.ToOffset(FirstOffset) };
      new MillisecondDateTimeOffsetEntity { DateTimeOffset = FirstMillisecondDateTimeOffset.ToOffset(SecondOffset) };
      new MillisecondDateTimeOffsetEntity { DateTimeOffset = FirstMillisecondDateTimeOffset.ToOffset(TimeSpan.Zero) };
      new MillisecondDateTimeOffsetEntity { DateTimeOffset = FirstMillisecondDateTimeOffset.Date };
      new MillisecondDateTimeOffsetEntity { DateTimeOffset = SecondMillisecondDateTimeOffset };
      new MillisecondDateTimeOffsetEntity { DateTimeOffset = SecondMillisecondDateTimeOffset.ToOffset(FirstOffset) };
      new MillisecondDateTimeOffsetEntity { DateTimeOffset = SecondMillisecondDateTimeOffset.ToOffset(SecondOffset) };
      new MillisecondDateTimeOffsetEntity { DateTimeOffset = SecondMillisecondDateTimeOffset.Date };
      new MillisecondDateTimeOffsetEntity { DateTimeOffset = SecondDateTimeOffset };
      new MillisecondDateTimeOffsetEntity { DateTimeOffset = SecondDateTime };
      new MillisecondDateTimeOffsetEntity { DateTimeOffset = new DateTimeOffset(SecondDateTime, TimeSpan.Zero) };

      index = 0;
      foreach (var dateTime in dateTimesWithMilliseconds)
        new MillisecondDateTimeOffsetEntity(dateTime, ++index % 3==0 ? FirstOffset : SecondOffset);

      var dateTimeOffset = FirstMillisecondDateTimeOffset.AddYears(10);
      for (var i = 0; i < 1000; ++i)
        new MillisecondDateTimeOffsetEntity { DateTimeOffset = dateTimeOffset.AddMilliseconds(i) };

      foreach (var dateTimeEntity in Query.All<DateTimeOffsetEntity>())
        new NullableDateTimeOffsetEntity(dateTimeEntity);

      new NullableDateTimeOffsetEntity { DateTimeOffset = null };
      new NullableDateTimeOffsetEntity { DateTimeOffset = null };
    }

    protected DateTimeOffset TryMoveToLocalTimeZone(DateTimeOffset dateTimeOffset)
    {
      if (ProviderInfo.ProviderName==WellKnown.Provider.PostgreSql)
        return dateTimeOffset.ToLocalTime();
      return dateTimeOffset;
    }
  }

  
}