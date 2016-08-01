// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.07.29

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset
{
  namespace Model
  {
    [HierarchyRoot]
    public class SingleDateTimeEntity : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public DateTime DateTime { get; set; }

      [Field]
      public DateTime MillisecondDateTime { get; set; }

      [Field]
      public DateTime? NullableDateTime { get; set; }
    }

    [HierarchyRoot]
    public class SingleDateTimeOffsetEntity : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public DateTimeOffset DateTimeOffset { get; set; }

      [Field]
      public DateTimeOffset MillisecondDateTimeOffset { get; set; }

      [Field]
      public DateTimeOffset? NullableDateTimeOffset { get; set; }
    }

    [HierarchyRoot]
    public class DateTimeEntity : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public DateTime DateTime { get; set; }
    }

    [HierarchyRoot]
    public class MillisecondDateTimeEntity : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public DateTime DateTime { get; set; }

      public MillisecondDateTimeEntity()
      {
      }

      public MillisecondDateTimeEntity(DateTimeEntity dateTimeEntity, int milliseconds)
      {
        DateTime = dateTimeEntity.DateTime.AddMilliseconds(milliseconds);
      }
    }

    [HierarchyRoot]
    public class NullableDateTimeEntity : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public DateTime? DateTime { get; set; }

      public NullableDateTimeEntity()
      {
      }

      public NullableDateTimeEntity(DateTimeEntity dateTimeEntity)
      {
        DateTime = dateTimeEntity.DateTime;
      }
    }

    [HierarchyRoot]
    public class DateTimeOffsetEntity : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public DateTimeOffset DateTimeOffset { get; set; }

      public DateTimeOffsetEntity()
      {
      }

      public DateTimeOffsetEntity(DateTimeEntity entity, TimeSpan offset)
      {
        DateTimeOffset = new DateTimeOffset(entity.DateTime, offset);
      }
    }

    [HierarchyRoot]
    public class MillisecondDateTimeOffsetEntity : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public DateTimeOffset DateTimeOffset { get; set; }

      public MillisecondDateTimeOffsetEntity()
      {
      }

      public MillisecondDateTimeOffsetEntity(MillisecondDateTimeEntity dateTimeEntity, TimeSpan offset)
      {
        DateTimeOffset = new DateTimeOffset(dateTimeEntity.DateTime, offset);
      }
    }

    [HierarchyRoot]
    public class NullableDateTimeOffsetEntity : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public DateTimeOffset? DateTimeOffset { get; set; }

      public NullableDateTimeOffsetEntity()
      {
      }

      public NullableDateTimeOffsetEntity(DateTimeOffsetEntity dateTimeOffsetEntity)
      {
        DateTimeOffset = dateTimeOffsetEntity.DateTimeOffset;
      }
    }
  }

  public abstract class BaseDateTimeAndDateTimeOffsetTest
    : AutoBuildTest
  {
    #region Consts && wellknowns

    protected static readonly DateTime FirstDateTime = new DateTime(2016, 01, 02, 03, 04, 05, DateTimeKind.Unspecified);
    protected static readonly DateTime SecondDateTime = new DateTime(2019, 11, 23, 22, 58, 57, DateTimeKind.Unspecified);
    protected static readonly DateTime WrongDateTime = new DateTime(2017, 02, 03, 04, 05, 06, DateTimeKind.Unspecified);
    protected static readonly DateTime NullableDateTime = SecondDateTime;

    protected static readonly TimeSpan FirstOffset = new TimeSpan(2, 45, 0); // +02:45
    protected static readonly TimeSpan SecondOffset = new TimeSpan(-11, 10, 0); // -11:10
    protected static readonly TimeSpan WrongOffset = new TimeSpan(4, 35, 0); // +04:35

    protected static readonly DateTime FirstMillisecondDateTime = FirstDateTime.AddMilliseconds(321);
    protected static readonly DateTime SecondMillisecondDateTime = SecondDateTime.AddMilliseconds(987);
    protected static readonly DateTime WrongMillisecondDateTime = WrongDateTime.AddMilliseconds(654);

    protected static readonly DateTimeOffset FirstDateTimeOffset = new DateTimeOffset(FirstDateTime, FirstOffset);
    protected static readonly DateTimeOffset SecondDateTimeOffset = new DateTimeOffset(SecondDateTime, SecondOffset);
    protected static readonly DateTimeOffset WrongDateTimeOffset = new DateTimeOffset(WrongDateTime, WrongOffset);
    protected static readonly DateTimeOffset NullableDateTimeOffset = SecondDateTimeOffset;

    protected static readonly DateTimeOffset FirstMillisecondDateTimeOffset = new DateTimeOffset(FirstMillisecondDateTime, FirstOffset);
    protected static readonly DateTimeOffset SecondMillisecondDateTimeOffset = new DateTimeOffset(SecondMillisecondDateTime, SecondOffset);
    protected static readonly DateTimeOffset WrongMillisecondDateTimeOffset = new DateTimeOffset(WrongMillisecondDateTime, WrongOffset);

    #endregion

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (SingleDateTimeEntity));
      config.Types.Register(typeof (DateTimeEntity));
      config.Types.Register(typeof (MillisecondDateTimeEntity));
      config.Types.Register(typeof (NullableDateTimeEntity));
      if (StorageProviderInfo.Instance.CheckAnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation)) {
        config.Types.Register(typeof (SingleDateTimeOffsetEntity));
        config.Types.Register(typeof (DateTimeOffsetEntity));
        config.Types.Register(typeof (MillisecondDateTimeOffsetEntity));
        config.Types.Register(typeof (NullableDateTimeOffsetEntity));
      }
      return config;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        #region SingleEntities

        new SingleDateTimeEntity {
          DateTime = FirstDateTime,
          MillisecondDateTime = FirstMillisecondDateTime,
          NullableDateTime = NullableDateTime
        };

        if (StorageProviderInfo.Instance.CheckAnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation)) {
          new SingleDateTimeOffsetEntity {
            DateTimeOffset = FirstDateTimeOffset,
            MillisecondDateTimeOffset = FirstMillisecondDateTimeOffset,
            NullableDateTimeOffset = NullableDateTimeOffset
          };
        }

        #endregion

        #region DateTimeEntities

        var i = 0;

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
        for (i = 0; i < 60; ++i)
          new DateTimeEntity { DateTime = dateTime.AddSeconds(i) };

        #endregion

        #region MillisecondDateTimeEntities

        new MillisecondDateTimeEntity { DateTime = FirstMillisecondDateTime };
        new MillisecondDateTimeEntity { DateTime = FirstMillisecondDateTime };
        new MillisecondDateTimeEntity { DateTime = FirstMillisecondDateTime.Date };
        new MillisecondDateTimeEntity { DateTime = SecondMillisecondDateTime };
        new MillisecondDateTimeEntity { DateTime = SecondMillisecondDateTime.Date };
        new MillisecondDateTimeEntity { DateTime = new DateTime(FirstMillisecondDateTime.Year, FirstMillisecondDateTime.Month, FirstMillisecondDateTime.Day, FirstMillisecondDateTime.Hour, FirstMillisecondDateTime.Minute, 0) };
        new MillisecondDateTimeEntity { DateTime = new DateTime(FirstMillisecondDateTime.Year, FirstMillisecondDateTime.Month, FirstMillisecondDateTime.Day, FirstMillisecondDateTime.Hour, FirstMillisecondDateTime.Minute, FirstMillisecondDateTime.Second, 0) };
        new MillisecondDateTimeEntity { DateTime = new DateTime(FirstMillisecondDateTime.Ticks, DateTimeKind.Local) };
        new MillisecondDateTimeEntity { DateTime = FirstMillisecondDateTime.Add(new TimeSpan(987, 23, 34, 45)) };

        i = 0;
        foreach (var dateTimeEntity1 in Query.All<DateTimeEntity>())
          new MillisecondDateTimeEntity(dateTimeEntity1, ++i % 3==0 ? FirstMillisecondDateTime.Millisecond : SecondMillisecondDateTime.Millisecond);

        dateTime = FirstMillisecondDateTime.AddYears(10);
        for (i = 0; i < 1000; ++i)
          new MillisecondDateTimeEntity { DateTime = dateTime.AddMilliseconds(i) };

        #endregion

        #region NullableDateTimeEntities

        foreach (var dateTimeEntity in Query.All<DateTimeEntity>())
          new NullableDateTimeEntity(dateTimeEntity);

        new NullableDateTimeEntity { DateTime = null };
        new NullableDateTimeEntity { DateTime = null };

        #endregion

        if (StorageProviderInfo.Instance.CheckAnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation)) {
          #region DateTimeOffsetEntities

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

          i = 0;
          foreach (var dateTimeEntity in Query.All<DateTimeEntity>())
            new DateTimeOffsetEntity(dateTimeEntity, ++i % 3==0 ? FirstOffset : SecondOffset);

          #endregion

          #region MillisecondDateTimeOffsetEntities

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

          i = 0;
          foreach (var dateTimeEntity in Query.All<MillisecondDateTimeEntity>())
            new MillisecondDateTimeOffsetEntity(dateTimeEntity, ++i % 3==0 ? FirstOffset : SecondOffset);

          var dateTimeOffset = FirstMillisecondDateTimeOffset.AddYears(10);
          for (i = 0; i < 1000; ++i)
            new MillisecondDateTimeOffsetEntity { DateTimeOffset = dateTimeOffset.AddMilliseconds(i) };

          #endregion

          #region NullableDateTimeOffsetEntities

          foreach (var dateTimeEntity in Query.All<DateTimeOffsetEntity>())
            new NullableDateTimeOffsetEntity(dateTimeEntity);

          new NullableDateTimeOffsetEntity { DateTimeOffset = null };
          new NullableDateTimeOffsetEntity { DateTimeOffset = null };

          #endregion
        }

        tx.Complete();
      }
    }

    protected void OpenSessionAndAction(Action action)
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        action();
      }
    }

    protected void RunTest<T>(Expression<Func<T, bool>> filter, int rightCount = 1)
      where T : Entity
    {
      var count = Query.All<T>().Count(filter);
      Assert.AreEqual(count, rightCount);
    }

    protected void RunWrongTest<T>(Expression<Func<T, bool>> filter)
      where T : Entity
    {
      RunTest(filter, 0);
    }
  }
}
