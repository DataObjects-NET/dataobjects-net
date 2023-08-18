// Copyright (C) 2016-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Groznov
// Created:    2016.08.01

using System;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model
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

    public SingleDateTimeEntity(Session session)
      : base(session)
    {
    }
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

    public DateTimeEntity(Session session, DateTime dateTime)
      : base(session)
    {
      DateTime = dateTime;
    }
  }

  [HierarchyRoot]
  public class MillisecondDateTimeEntity : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public DateTime DateTime { get; set; }

    public MillisecondDateTimeEntity(Session session)
    {
    }

    public MillisecondDateTimeEntity(Session session, DateTime dateTime)
    {
      DateTime = dateTime;
    }
  }

  [HierarchyRoot]
  public class NullableDateTimeEntity : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public DateTime? DateTime { get; set; }

    public NullableDateTimeEntity(Session session)
      : base(session)
    {
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

    public DateTimeOffsetEntity(DateTime dateTime, TimeSpan offset)
    {
      DateTimeOffset = new DateTimeOffset(dateTime, offset);
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

    public MillisecondDateTimeOffsetEntity(DateTime dateTimeEntity, TimeSpan offset)
    {
      DateTimeOffset = new DateTimeOffset(dateTimeEntity, offset);
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


  [HierarchyRoot]
  public class AllPossiblePartsEntity : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    [Validation.RangeConstraint(Min = 0, Max = 3000)]
    public int Year { get; set; }

    [Field]
    [Validation.RangeConstraint(Min = 1, Max = 12)]
    public int Month { get; set; }

    [Field]
    [Validation.RangeConstraint(Min = 1, Max = 31)]
    public int Day { get; set; }

    [Field]
    [Validation.RangeConstraint(Min = 0, Max = 23)]
    public int Hour { get; set; }

    [Field]
    [Validation.RangeConstraint(Min = 0, Max = 59)]
    public int Minute { get; set; }

    [Field]
    [Validation.RangeConstraint(Min = 0, Max = 59)]
    public int Second { get; set; }

    [Field]
    [Validation.RangeConstraint(Min = 0, Max = 999)]
    public int Millisecond { get; set; }

    [Field]
    [Validation.RangeConstraint(Min = 0, Max = 999)]
    public int Microsecond { get; set; }

    [Field]
    public long DateTimeTicks { get; set; }

    [Field]
    public long TimeOnlyTicks { get; set; }

    [Field]
    [Validation.RangeConstraint(Min = -23, Max = 23)]
    public int OffsetHour { get; set; }

    [Field]
    [Validation.RangeConstraint(Min = 0, Max = 59)]
    public int OffsetMinute { get; set; }

    [Field]
    public TimeSpan TimeSpan { get; set; }

    public static AllPossiblePartsEntity FromDateTime(Session session, DateTime dateTime, int microsecond)
    {
      return new AllPossiblePartsEntity(session) {
        Year = dateTime.Year,
        Month = dateTime.Month,
        Day = dateTime.Day,
        Hour = dateTime.Hour,
        Minute = dateTime.Minute,
        Second = dateTime.Second,
        Millisecond = dateTime.Millisecond,
        Microsecond = microsecond,
        OffsetHour = 0,
        OffsetMinute = 0,
        DateTimeTicks = dateTime.Ticks,
        TimeOnlyTicks = TimeOnly.FromDateTime(dateTime).Ticks,
        TimeSpan = TimeOnly.FromDateTime(dateTime).ToTimeSpan(),
      };
    }

    public static AllPossiblePartsEntity FromDateTimeOffset(Session session, DateTimeOffset dateTimeOffset, int microsecond)
    {
      return new AllPossiblePartsEntity(session) {
        Year = dateTimeOffset.Year,
        Month = dateTimeOffset.Month,
        Day = dateTimeOffset.Day,
        Hour = dateTimeOffset.Hour,
        Minute = dateTimeOffset.Minute,
        Second = dateTimeOffset.Second,
        Millisecond = dateTimeOffset.Millisecond,
        Microsecond = microsecond,
        OffsetHour = dateTimeOffset.Offset.Hours,
        OffsetMinute = dateTimeOffset.Offset.Minutes,
        DateTimeTicks = dateTimeOffset.Ticks,
        TimeOnlyTicks = TimeOnly.FromDateTime(dateTimeOffset.DateTime).Ticks,
        TimeSpan = TimeOnly.FromDateTime(dateTimeOffset.DateTime).ToTimeSpan(),
      };
    }

    private AllPossiblePartsEntity(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class DateOnlyEntity : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public DateOnly DateOnly { get; set; }

    [Field]
    public DateOnly? NullableDateOnly { get; set; }

    public DateOnlyEntity(Session session)
      : base(session)
    {
    }
  }

  public class SingleDateOnlyEntity : DateOnlyEntity
  {
    public SingleDateOnlyEntity(Session session)
      : base(session)
    {

    }
  }

  [HierarchyRoot]
  public class TimeOnlyEntity : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public TimeOnly TimeOnly { get; set; }

    [Field]
    public TimeOnly MillisecondTimeOnly { get; set; }

    [Field]
    public TimeOnly? NullableTimeOnly { get; set; }

    public TimeOnlyEntity(Session session)
      : base(session)
    {
    }
  }

  public class SingleTimeOnlyEntity : TimeOnlyEntity
  {
    public SingleTimeOnlyEntity(Session session)
      : base(session)
    {
    }
  }
}
