// Copyright (C) 2016-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    protected static readonly TimeSpan WrongOffset = new TimeSpan(1, 35, 0); // +01:35

    protected static readonly DateTimeOffset FirstDateTimeOffset = new DateTimeOffset(FirstDateTime, FirstOffset);
    protected static readonly DateTimeOffset SecondDateTimeOffset = new DateTimeOffset(SecondDateTime, SecondOffset);
    protected static readonly DateTimeOffset WrongDateTimeOffset = new DateTimeOffset(WrongDateTime, WrongOffset);
    protected static readonly DateTimeOffset NullableDateTimeOffset = SecondDateTimeOffset;

    protected static readonly DateTimeOffset FirstMillisecondDateTimeOffset = new DateTimeOffset(FirstMillisecondDateTime, FirstOffset);
    protected static readonly DateTimeOffset SecondMillisecondDateTimeOffset = new DateTimeOffset(SecondMillisecondDateTime, SecondOffset);
    protected static readonly DateTimeOffset WrongMillisecondDateTimeOffset = new DateTimeOffset(WrongMillisecondDateTime, WrongOffset);

    protected static readonly TimeSpan localTimezone = DateTimeOffset.Now.ToLocalTime().Offset;

    protected override void RegisterTypes(DomainConfiguration configuration)
    {
      configuration.Types.Register(typeof(SingleDateTimeOffsetEntity));
      configuration.Types.Register(typeof(DateTimeOffsetEntity));
      configuration.Types.Register(typeof(MillisecondDateTimeOffsetEntity));
      configuration.Types.Register(typeof(NullableDateTimeOffsetEntity));
      configuration.Types.Register(typeof(DateTimeEntity));
      if (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.PostgreSql)) {
        configuration.Types.Register(typeof(MinMaxDateTimeOffsetEntity));
      }
    }

    protected override void InitializeCustomSettings(DomainConfiguration configuration)
    {
      var providerInfo = StorageProviderInfo.Instance.Info;
      if (providerInfo.ProviderName==WellKnown.Provider.PostgreSql) {
        var localZone = localTimezone;
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
      _ = new SingleDateTimeOffsetEntity(session) {
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
        FirstDateTime.Add(new TimeSpan(987, 23, 34, 45)),FirstDateTime.AddYears(1),FirstDateTime.AddYears(-1),
        FirstDateTime.AddMonths(44),
        FirstDateTime.AddMonths(-5),
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

      _ = new DateTimeOffsetEntity { DateTimeOffset = FirstDateTimeOffset };
      _ = new DateTimeOffsetEntity { DateTimeOffset = FirstDateTimeOffset };
      _ = new DateTimeOffsetEntity { DateTimeOffset = FirstDateTimeOffset.ToOffset(FirstOffset) };
      _ = new DateTimeOffsetEntity { DateTimeOffset = FirstDateTimeOffset.ToOffset(SecondOffset) };
      _ = new DateTimeOffsetEntity { DateTimeOffset = FirstDateTimeOffset.ToOffset(TimeSpan.Zero) };
      _ = new DateTimeOffsetEntity { DateTimeOffset = FirstDateTimeOffset.Date };
      _ = new DateTimeOffsetEntity { DateTimeOffset = SecondDateTimeOffset };
      _ = new DateTimeOffsetEntity { DateTimeOffset = SecondDateTimeOffset.ToOffset(FirstOffset) };
      _ = new DateTimeOffsetEntity { DateTimeOffset = SecondDateTimeOffset.ToOffset(SecondOffset) };
      _ = new DateTimeOffsetEntity { DateTimeOffset = SecondDateTimeOffset.Date };
      _ = new DateTimeOffsetEntity { DateTimeOffset = FirstDateTime };
      _ = new DateTimeOffsetEntity { DateTimeOffset = new DateTimeOffset(FirstDateTime, TimeSpan.Zero) };

      var index = 0;
      foreach (var dateTime in dateTimes) {
        _ = new DateTimeOffsetEntity(dateTime, ++index % 3 == 0 ? FirstOffset : SecondOffset);
      }

      _ = new MillisecondDateTimeOffsetEntity { DateTimeOffset = FirstMillisecondDateTimeOffset };
      _ = new MillisecondDateTimeOffsetEntity { DateTimeOffset = FirstMillisecondDateTimeOffset };
      _ = new MillisecondDateTimeOffsetEntity { DateTimeOffset = FirstMillisecondDateTimeOffset.ToOffset(FirstOffset) };
      _ = new MillisecondDateTimeOffsetEntity { DateTimeOffset = FirstMillisecondDateTimeOffset.ToOffset(SecondOffset) };
      _ = new MillisecondDateTimeOffsetEntity { DateTimeOffset = FirstMillisecondDateTimeOffset.ToOffset(TimeSpan.Zero) };
      _ = new MillisecondDateTimeOffsetEntity { DateTimeOffset = FirstMillisecondDateTimeOffset.Date };
      _ = new MillisecondDateTimeOffsetEntity { DateTimeOffset = SecondMillisecondDateTimeOffset };
      _ = new MillisecondDateTimeOffsetEntity { DateTimeOffset = SecondMillisecondDateTimeOffset.ToOffset(FirstOffset) };
      _ = new MillisecondDateTimeOffsetEntity { DateTimeOffset = SecondMillisecondDateTimeOffset.ToOffset(SecondOffset) };
      _ = new MillisecondDateTimeOffsetEntity { DateTimeOffset = SecondMillisecondDateTimeOffset.Date };
      _ = new MillisecondDateTimeOffsetEntity { DateTimeOffset = SecondDateTimeOffset };
      _ = new MillisecondDateTimeOffsetEntity { DateTimeOffset = SecondDateTime };
      _ = new MillisecondDateTimeOffsetEntity { DateTimeOffset = new DateTimeOffset(SecondDateTime, TimeSpan.Zero) };

      index = 0;
      foreach (var dateTime in dateTimesWithMilliseconds) {
        _ = new MillisecondDateTimeOffsetEntity(dateTime, ++index % 3 == 0 ? FirstOffset : SecondOffset);
      }

      var dateTimeOffset = FirstMillisecondDateTimeOffset.AddYears(10);
      for (var i = 0; i < 1000; ++i) {
        _ = new MillisecondDateTimeOffsetEntity { DateTimeOffset = dateTimeOffset.AddMilliseconds(i) };
      }

      foreach (var dateTimeEntity in Query.All<DateTimeOffsetEntity>()) {
        _ = new NullableDateTimeOffsetEntity(dateTimeEntity);
      }

      _ = new NullableDateTimeOffsetEntity { DateTimeOffset = null };
      _ = new NullableDateTimeOffsetEntity { DateTimeOffset = null };

      if (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.PostgreSql)) {
        _ = new MinMaxDateTimeOffsetEntity(session) { MinValue = DateTimeOffset.MinValue, MaxValue = DateTimeOffset.MaxValue };
      }
    }

    protected DateTimeOffset TryMoveToLocalTimeZone(DateTimeOffset dateTimeOffset)
    {
      if (ProviderInfo.ProviderName==WellKnown.Provider.PostgreSql)
        return dateTimeOffset.ToLocalTime();
      return dateTimeOffset;
    }
  }
}