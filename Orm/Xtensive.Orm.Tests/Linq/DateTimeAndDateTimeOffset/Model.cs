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


#if NET6_0_OR_GREATER //DO_DATEONLY
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
      :base(session)
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

#endif
}
