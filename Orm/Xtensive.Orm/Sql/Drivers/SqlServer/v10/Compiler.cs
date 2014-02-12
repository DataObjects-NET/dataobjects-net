// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.07

using System;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.SqlServer.v10
{
  internal class Compiler : v09.Compiler
  {
    protected static SqlUserFunctionCall DateAddNanosecond(SqlExpression date, SqlExpression nanoseconds)
    {
      return SqlDml.FunctionCall("DATEADD", SqlDml.Native("NS"), nanoseconds, date);
    }

    protected static SqlUserFunctionCall DateDiffNanosecond(SqlExpression date1, SqlExpression date2)
    {
      return SqlDml.FunctionCall("DATEDIFF", SqlDml.Native("NS"), date1, date2);
    }

    protected override SqlExpression DateTimeTruncate(SqlExpression date)
    {
      return SqlDml.Cast(SqlDml.Cast(date, new SqlValueType("Date")), new SqlValueType("DateTime2"));
    }

    protected override SqlExpression  DateTimeSubtractDateTime(SqlExpression date1, SqlExpression date2)
    {
      return base.DateTimeSubtractDateTime(date1, date2)
        + DateDiffNanosecond(
          DateAddMillisecond(
            DateAddDay(date2, DateDiffDay(date2, date1)),
            DateDiffMillisecond(DateAddDay(date2, DateDiffDay(date2, date1)), date1)),
          date1);
    }

    protected virtual SqlExpression DateTimeOffsetSubtractDateTimeOffset(SqlExpression date1, SqlExpression date2)
    {
      return DateTimeSubtractDateTime(date1, date2);
    }

    protected override SqlExpression DateTimeAddInterval(SqlExpression date, SqlExpression interval)
    {
      return DateAddNanosecond(
        DateAddMillisecond(
          DateAddDay(date, interval / NanosecondsPerDay),
          (interval/NanosecondsPerMillisecond) % (MillisecondsPerDay)),
        (interval/NanosecondsPerSecond) % NanosecondsPerDay/NanosecondsPerSecond);
    }

    public override void Visit(SqlExtract node)
    {
      if (node.DateTimeOffsetPart==SqlDateTimeOffsetPart.DayOfWeek) {
        Visit((DatePartWeekDay(node.Operand) + DateFirst + 6) % 7);
        return;
      }
      switch (node.DateTimeOffsetPart) {
      case SqlDateTimeOffsetPart.TimeZoneHour:
        Visit(DateTimeOffsetTimeZoneInMinutes(node.Operand) / 60);
        return;
      case SqlDateTimeOffsetPart.TimeZoneMinute:
        Visit(DateTimeOffsetTimeZoneInMinutes(node.Operand) % 60);
        return;
      }
      base.Visit(node);
    }

    /// <inheritdoc/>
    public override void Visit(SqlFunctionCall node)
    {
      switch (node.FunctionType) {
      case SqlFunctionType.DateTimeOffsetAddMonths:
        Visit(DateAddMonth(node.Arguments[0], node.Arguments[1]));
        return;
      case SqlFunctionType.DateTimeOffsetAddYears:
        Visit(DateAddYear(node.Arguments[0], node.Arguments[1]));
        return;
      case SqlFunctionType.DateTimeOffsetTruncate:
        DateTimeOffsetTruncate(node.Arguments[0]).AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeOffsetToDateTime:
        DateTimeOffsetTruncateOffset(node.Arguments[0]).AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeOffsetPartOffset:
        DateTimeOffsetPartOffset(node.Arguments[0]).AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeOffsetToUtcDateTime:
        Switchoffset(node.Arguments[0], "+00:00").AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeOffsetToLocalDateTime:
        DateTimeOffsetToLocalDateTime(node.Arguments[0]).AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeOffsetTimeOfDay:
        DateTimeOffsetTimeOfDay(node.Arguments[0]).AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeOffsetConstruct:
        Visit(ToDateTimeOffset(DateAddDay(DateAddMonth(DateAddYear(SqlDml.Literal(new DateTime(2001, 1, 1)),
          node.Arguments[0] - 2001),
          node.Arguments[1] - 1),
          node.Arguments[2] - 1),
          node.Arguments[3]));
        return;
      case SqlFunctionType.DateTimeOffsetToLocalTime:
        DateTimeOffsetToLocalTime(node.Arguments[0]).AcceptVisitor(this);
        return;
      case SqlFunctionType.UpgradeDateTimeToDateTimeOffset:
        UpgradeDateTimeToDateTimeOffset(node.Arguments[0]).AcceptVisitor(this);
        return;
      }

      base.Visit(node);
    }

    public override void Visit(SqlBinary node)
    {
      switch (node.NodeType) {
      case SqlNodeType.DateTimeOffsetPlusInterval:
        DateTimeAddInterval(node.Left, node.Right).AcceptVisitor(this);
        return;
      case SqlNodeType.DateTimeOffsetMinusDateTimeOffset:
        DateTimeOffsetSubtractDateTimeOffset(node.Left, node.Right).AcceptVisitor(this);
        return;
      case SqlNodeType.DateTimeOffsetMinusInterval:
        DateTimeAddInterval(node.Left, -node.Right).AcceptVisitor(this);
        return;
      }
      base.Visit(node);
    }

    #region Static helpers

    private static SqlExpression DateTimeOffsetTruncate(SqlExpression dateTimeOffset)
    {
      return SqlDml.Cast(DateAddMillisecond(DateAddSecond(DateAddMinute(DateAddHour(dateTimeOffset,
        -SqlDml.Extract(SqlDateTimeOffsetPart.Hour, dateTimeOffset)),
        -SqlDml.Extract(SqlDateTimeOffsetPart.Minute, dateTimeOffset)),
        -SqlDml.Extract(SqlDateTimeOffsetPart.Second, dateTimeOffset)),
        -SqlDml.Extract(SqlDateTimeOffsetPart.Millisecond, dateTimeOffset)),
        SqlType.DateTime);
    }

    private static SqlExpression DateTimeOffsetTruncateOffset(SqlExpression dateTimeOffset)
    {
      return SqlDml.Cast(dateTimeOffset, SqlType.DateTime);
    }

    private static SqlExpression DateTimeOffsetPartOffset(SqlExpression dateTimeOffset)
    {
      return SqlDml.DateTimeOffsetMinusDateTimeOffset(DateTimeOffsetTruncateOffset(dateTimeOffset), Switchoffset(dateTimeOffset, "+00:00"));
    }

    private static SqlExpression DateTimeOffsetTimeOfDay(SqlExpression dateTimeOffset)
    {
      return SqlDml.Extract(SqlDateTimeOffsetPart.Hour, dateTimeOffset) * (60 * 60 * NanosecondsPerSecond)
        + SqlDml.Extract(SqlDateTimeOffsetPart.Minute, dateTimeOffset) * (60 * NanosecondsPerSecond)
        + SqlDml.Extract(SqlDateTimeOffsetPart.Second, dateTimeOffset) * NanosecondsPerSecond
        +SqlDml.Extract(SqlDateTimeOffsetPart.Millisecond, dateTimeOffset) * NanosecondsPerMillisecond;
    }

    private static SqlExpression DateTimeOffsetToLocalDateTime(SqlExpression dateTimeOffset)
    {
      return SqlDml.Cast(
        SqlDml.DateTimePlusInterval(
          Switchoffset(dateTimeOffset, "+00:00"),
          SqlDml.DateTimeMinusDateTime(SqlDml.Native("getdate()"), SqlDml.Native("getutcdate()"))),
        SqlType.DateTime);
    }

    private static SqlUserFunctionCall ToDateTimeOffset(SqlExpression dateTime, SqlExpression offsetInMinutes)
    {
      return SqlDml.FunctionCall("TODATETIMEOFFSET", dateTime, offsetInMinutes);
    }

    private static SqlExpression Switchoffset(SqlExpression dateTimeOffset, SqlExpression offset)
    {
      return SqlDml.FunctionCall("SWITCHOFFSET", dateTimeOffset, offset);
    }

    private static SqlUserFunctionCall DateTimeOffsetTimeZoneInMinutes(SqlExpression date)
    {
      return SqlDml.FunctionCall("DATEPART", SqlDml.Native("TZoffset"), date);
    }

    private static SqlExpression DateTimeOffsetToLocalTime(SqlExpression dateTimeOffset)
    {
      return Switchoffset(dateTimeOffset, DateTimeOffsetTimeZoneInMinutes(SqlDml.Native("SYSDATETIMEOFFSET()")));
    }

    private static SqlExpression UpgradeDateTimeToDateTimeOffset(SqlExpression dateTime)
    {
      return SqlDml.FunctionCall("TODATETIMEOFFSET", dateTime, SqlDml.FunctionCall("DATEPART", SqlDml.Native("TZoffset"), SqlDml.Native("SYSDATETIMEOFFSET()")));
    }

    #endregion

    // Constructors

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}