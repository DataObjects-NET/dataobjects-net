// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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

#if DO_DATEONLY
    [Field]
    public DateOnly DateOnly { get; set; }

    [Field]
    public DateOnly? NullableDateOnly { get; set; }

    [Field]
    public TimeOnly TimeOnly { get; set; }
#endif
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

#if DO_DATEONLY
    [Field]
    public DateOnly DateOnly { get; set; }

    [Field]
    public TimeOnly TimeOnly { get; set; }
#endif

    public DateTimeEntity(DateTime dateTime)
    {
      DateTime = dateTime;
#if DO_DATEONLY
      DateOnly = DateOnly.FromDateTime(dateTime);
      TimeOnly = TimeOnly.FromDateTime(dateTime);
#endif
    }
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
}
