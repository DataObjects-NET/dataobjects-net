// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.07.07

using System;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.SqlServer.v10
{
  internal class Compiler : v09.Compiler
  {
    protected const string UtcTimeZone = "+00:00";
    protected const string SqlDateTypeName = "date";
    protected const string SqlDateTime2TypeName = "datetime2";

    protected static SqlUserFunctionCall DateAddNanosecond(SqlExpression date, SqlExpression nanoseconds) =>
      SqlDml.FunctionCall("DATEADD", SqlDml.Native("NS"), nanoseconds, date);

    protected static SqlUserFunctionCall DateDiffNanosecond(SqlExpression date1, SqlExpression date2) =>
      SqlDml.FunctionCall("DATEDIFF", SqlDml.Native("NS"), date1, date2);

    protected override SqlExpression DateTimeTruncate(SqlExpression date) =>
      SqlDml.Cast(
        SqlDml.Cast(date, new SqlValueType(SqlDateTypeName)),
        new SqlValueType(SqlDateTime2TypeName));

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
      switch (node.DateTimeOffsetPart) {
        case SqlDateTimeOffsetPart.DayOfWeek:
          Visit((DatePartWeekDay(node.Operand) + DateFirst + 6) % 7);
          return;
        case SqlDateTimeOffsetPart.TimeZoneHour:
          Visit(DateTimeOffsetTimeZoneInMinutes(node.Operand) / 60);
          return;
        case SqlDateTimeOffsetPart.TimeZoneMinute:
          Visit(DateTimeOffsetTimeZoneInMinutes(node.Operand) % 60);
          return;
        case SqlDateTimeOffsetPart.Date:
          DateTimeOffsetTruncate(node.Operand).AcceptVisitor(this);
          return;
        case SqlDateTimeOffsetPart.DateTime:
          DateTimeOffsetTruncateOffset(node.Operand).AcceptVisitor(this);
          return;
        case SqlDateTimeOffsetPart.LocalDateTime:
          DateTimeOffsetToLocalDateTime(node.Operand).AcceptVisitor(this);
          return;
        case SqlDateTimeOffsetPart.UtcDateTime:
          SqlDml.Cast(Switchoffset(node.Operand, UtcTimeZone), SqlType.DateTime).AcceptVisitor(this);
          return;
        case SqlDateTimeOffsetPart.Offset:
          DateTimeOffsetPartOffset(node.Operand).AcceptVisitor(this);
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
        case SqlFunctionType.DateTimeOffsetTimeOfDay:
          DateTimeOffsetTimeOfDay(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetConstruct:
          Visit(ToDateTimeOffset(node.Arguments[0], node.Arguments[1]));
          return;
        case SqlFunctionType.DateTimeOffsetToLocalTime:
          DateTimeOffsetToLocalTime(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetToUtcTime:
          DateTimeOffsetToUtcTime(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeToDateTimeOffset:
          DateTimeToDateTimeOffset(node.Arguments[0]).AcceptVisitor(this);
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

    private static SqlExpression DateTimeOffsetTruncate(SqlExpression dateTimeOffset) =>
      SqlDml.Cast(
        SqlDml.Cast(dateTimeOffset, new SqlValueType(SqlDateTypeName)),
        new SqlValueType(SqlDateTime2TypeName));

    private static SqlExpression DateTimeOffsetTruncateOffset(SqlExpression dateTimeOffset) =>
      SqlDml.Cast(dateTimeOffset, SqlType.DateTime);

    private static SqlExpression DateTimeOffsetPartOffset(SqlExpression dateTimeOffset) =>
      SqlDml.DateTimeOffsetMinusDateTimeOffset(
        DateTimeOffsetTruncateOffset(dateTimeOffset),
        Switchoffset(dateTimeOffset, UtcTimeZone));

    private static SqlExpression DateTimeOffsetTimeOfDay(SqlExpression dateTimeOffset) =>
      SqlDml.Extract(SqlDateTimeOffsetPart.Hour, dateTimeOffset) * (60 * 60 * NanosecondsPerSecond)
        + SqlDml.Extract(SqlDateTimeOffsetPart.Minute, dateTimeOffset) * (60 * NanosecondsPerSecond)
        + SqlDml.Extract(SqlDateTimeOffsetPart.Second, dateTimeOffset) * NanosecondsPerSecond
        + SqlDml.Extract(SqlDateTimeOffsetPart.Millisecond, dateTimeOffset) * NanosecondsPerMillisecond;

    private static SqlExpression DateTimeOffsetToLocalDateTime(SqlExpression dateTimeOffset) =>
      SqlDml.Cast(
        SqlDml.DateTimePlusInterval(
          Switchoffset(dateTimeOffset, UtcTimeZone),
          SqlDml.DateTimeMinusDateTime(SqlDml.Native("getdate()"), SqlDml.Native("getutcdate()"))),
        SqlType.DateTime);

    private static SqlUserFunctionCall ToDateTimeOffset(SqlExpression dateTime, SqlExpression offsetInMinutes) =>
      SqlDml.FunctionCall("TODATETIMEOFFSET", dateTime, offsetInMinutes);

    private static SqlExpression Switchoffset(SqlExpression dateTimeOffset, SqlExpression offset) =>
      SqlDml.FunctionCall("SWITCHOFFSET", dateTimeOffset, offset);

    private static SqlUserFunctionCall DateTimeOffsetTimeZoneInMinutes(SqlExpression date) =>
      SqlDml.FunctionCall("DATEPART", SqlDml.Native("TZoffset"), date);

    private static SqlExpression DateTimeOffsetToLocalTime(SqlExpression dateTimeOffset) =>
      Switchoffset(dateTimeOffset, DateTimeOffsetTimeZoneInMinutes(SqlDml.Native("SYSDATETIMEOFFSET()")));

    private static SqlExpression DateTimeOffsetToUtcTime(SqlExpression dateTimeOffset) =>
      Switchoffset(dateTimeOffset, UtcTimeZone);

    private static SqlExpression DateTimeToDateTimeOffset(SqlExpression dateTime) =>
      SqlDml.FunctionCall("TODATETIMEOFFSET",
        dateTime,
        SqlDml.FunctionCall("DATEPART",
          SqlDml.Native("TZoffset"),
          SqlDml.Native("SYSDATETIMEOFFSET()")));

    #endregion

    // Constructors

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}