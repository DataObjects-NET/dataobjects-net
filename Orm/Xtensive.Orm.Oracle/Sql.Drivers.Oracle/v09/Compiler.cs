// Copyright (C) 2009-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using Xtensive.Core;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Sql.Drivers.Oracle.Resources;
using System.Collections.Generic;

namespace Xtensive.Sql.Drivers.Oracle.v09
{
  internal class Compiler : SqlCompiler
  {
    protected const string DayPartFormat = "DD";
    protected const string HourPartFormat = "HH24";
    protected const string MillisecondPartFormat = "FF3";
    protected const string NanosecondPartFormat = "FF9";
    protected const string MinutePartFormat = "MI";
    protected const string MonthPartFormat = "MM";
    protected const string SecondPartFormat = "SS";
    protected const string YearPartFormat = "YYYY";
    protected const string TimeZoneHourPartFormat = "TZH";
    protected const string TimeZoneMinutePartFormat = "TZM";
    protected const string DayOfYearPartFormat = "DDD";
    protected const string DayOfWeekPartFormat = "D";

    protected const string YearIntervalPart = "YEAR";
    protected const string MonthIntervalPart = "MONTH";
    protected const string DayIntervalPart = "DAY";
    protected const string HourIntervalPart = "HOUR";
    protected const string MinuteIntervalPart = "MINUTE";
    protected const string SecondIntervalPart = "SECOND";

    protected const string ToCharFunctionName = "TO_CHAR";
    protected const string NumToDSIntervalFunctionName = "NUMTODSINTERVAL";
    protected const string NumToYMIntervalFunctionName = "NUMTOYMINTERVAL";

    protected const string ToDSIntervalFunctionName = "TO_DSINTERVAL";
    protected const string TimeFormat = "HH24:MI:SS.FF7";

    protected const long NanosecondsPerHour = 3600000000000;
    protected const long NanosecondsPerMinute = 60000000000;
    protected const long NanosecondsPerSecond = 1000000000;
    protected const long NanosecondsPerMillisecond = 1000000;

    private static readonly SqlExpression SundayNumber = SqlDml.Native(
      "TO_NUMBER(TO_CHAR(TIMESTAMP '2009-07-26 00:00:00.000', 'D'))");
    private static readonly SqlNative RefTimestamp = SqlDml.Native("timestamp '2009-01-01 00:00:00.0000000'");

    public override void Visit(SqlFunctionCall node)
    {
      var arguments = node.Arguments;

      switch (node.FunctionType) {
        case SqlFunctionType.PadLeft:
        case SqlFunctionType.PadRight:
          SqlHelper.GenericPad(node).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetAddYears:
        case SqlFunctionType.DateTimeAddYears:
          DateTimeAddYMInterval(arguments[0], arguments[1], YearIntervalPart).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetAddMonths:
        case SqlFunctionType.DateTimeAddMonths:
          DateTimeAddYMInterval(arguments[0], arguments[1], MonthIntervalPart).AcceptVisitor(this);
          return;
        case SqlFunctionType.IntervalConstruct:
          IntervalConstruct(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeConstruct:
          DateTimeConstruct(arguments[0], arguments[1], arguments[2]).AcceptVisitor(this);
          return;
#if NET6_0_OR_GREATER
        case SqlFunctionType.DateConstruct:
          DateConstruct(arguments[0], arguments[1], arguments[2]).AcceptVisitor(this);
          return;
        case SqlFunctionType.TimeConstruct:
          TimeConstruct(arguments).AcceptVisitor(this);
          return;
        case SqlFunctionType.TimeToNanoseconds:
          TimeToNanoseconds(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateAddYears:
          DateTimeAddYMInterval(arguments[0], arguments[1], YearIntervalPart).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateAddMonths:
          DateTimeAddYMInterval(arguments[0], arguments[1], MonthIntervalPart).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateAddDays:
          DateTimeAddDSInterval(arguments[0], arguments[1], DayIntervalPart).AcceptVisitor(this);
          return;
        case SqlFunctionType.TimeAddHours:
          TimeAddHourOrMinute(arguments[0], arguments[1], true).AcceptVisitor(this);
          return;
        case SqlFunctionType.TimeAddMinutes:
          TimeAddHourOrMinute(arguments[0], arguments[1], false).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateToString:
          DateToString(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.TimeToString:
          TimeToString(arguments[0]).AcceptVisitor(this);
          return;
#endif
        case SqlFunctionType.IntervalAbs:
          SqlHelper.IntervalAbs(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.IntervalToMilliseconds:
          SqlHelper.IntervalToMilliseconds(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.IntervalToNanoseconds:
          SqlHelper.IntervalToNanoseconds(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.Position:
          Position(arguments[0], arguments[1]).AcceptVisitor(this);
          return;
        case SqlFunctionType.CharLength:
          SqlDml.Coalesce(SqlDml.FunctionCall("LENGTH", arguments[0]), 0).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeToStringIso:
          DateTimeToStringIso(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetConstruct:
          DateTimeOffsetConstruct(arguments[0], arguments[1]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetTimeOfDay:
          DateTimeOffsetTimeOfDay(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetToLocalTime:
          DateTimeOffsetToLocalTime(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeToDateTimeOffset:
          DateTimeToDateTimeOffset(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetToDateTime:
          DateTimeOffsetToDateTime(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetToUtcTime:
          DateTimeOffsetToUtcTime(arguments[0]).AcceptVisitor(this);
          return;
#if NET6_0_OR_GREATER
        case SqlFunctionType.DateTimeToDate:
          DateTimeToDate(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateToDateTime:
          DateToDateTime(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeToTime:
          DateTimeToTime(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.TimeToDateTime:
          TimeToDateTime(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetToDate:
          DateTimeOffsetToDate(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetToTime:
          DateTimeOffsetToTime(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateToDateTimeOffset:
          DateToDateTimeOffset(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.TimeToDateTimeOffset:
          TimeToDateTimeOffset(arguments[0]).AcceptVisitor(this);
          return;
#endif
        default:
          base.Visit(node);
          return;
      }
    }

    public override void Visit(SqlCreateTable node)
    {
      if (node.Table is TemporaryTable table && !table.IsGlobal) {
        throw new NotSupportedException(Strings.ExOracleDoesNotSupportLocalTemporaryTables);
      }
      base.Visit(node);
    }

    public override void Visit(SqlTrim node)
    {
      if (node.TrimCharacters != null && node.TrimCharacters.Length > 1) {
        throw new NotSupportedException(Strings.ExOracleDoesNotSupportTrimmingMoreThatOneCharacterAtOnce);
      }
      base.Visit(node);
    }

    public override void Visit(SqlExtract node)
    {
      switch (node.DateTimeOffsetPart) {
        case SqlDateTimeOffsetPart.Day:
          DateTimeOffsetExtractPart(node.Operand, DayPartFormat).AcceptVisitor(this);
          return;
        case SqlDateTimeOffsetPart.Hour:
          DateTimeOffsetExtractPart(node.Operand, HourPartFormat).AcceptVisitor(this);
          return;
        case SqlDateTimeOffsetPart.Millisecond:
          DateTimeOffsetExtractPart(node.Operand, MillisecondPartFormat).AcceptVisitor(this);
          return;
        case SqlDateTimeOffsetPart.Nanosecond:
          DateTimeOffsetExtractPart(node.Operand, NanosecondPartFormat).AcceptVisitor(this);
          return;
        case SqlDateTimeOffsetPart.Minute:
          DateTimeOffsetExtractPart(node.Operand, MinutePartFormat).AcceptVisitor(this);
          return;
        case SqlDateTimeOffsetPart.Month:
          DateTimeOffsetExtractPart(node.Operand, MonthPartFormat).AcceptVisitor(this);
          return;
        case SqlDateTimeOffsetPart.Second:
          DateTimeOffsetExtractPart(node.Operand, SecondPartFormat).AcceptVisitor(this);
          return;
        case SqlDateTimeOffsetPart.Year:
          DateTimeOffsetExtractPart(node.Operand, YearPartFormat).AcceptVisitor(this);
          return;
        case SqlDateTimeOffsetPart.TimeZoneHour:
          DateTimeOffsetExtractPart(node.Operand, TimeZoneHourPartFormat).AcceptVisitor(this);
          return;
        case SqlDateTimeOffsetPart.TimeZoneMinute:
          DateTimeOffsetExtractPart(node.Operand, TimeZoneMinutePartFormat).AcceptVisitor(this);
          return;
        case SqlDateTimeOffsetPart.DayOfWeek:
          DateTimeExtractDayOfWeek(node.Operand).AcceptVisitor(this);
          return;
        case SqlDateTimeOffsetPart.DayOfYear:
          DateTimeOffsetExtractPart(node.Operand, DayOfYearPartFormat).AcceptVisitor(this);
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
          DateTimeOffsetToUtcDateTime(node.Operand).AcceptVisitor(this);
          return;
        case SqlDateTimeOffsetPart.Offset:
          DateTimeOffsetPartOffset(node.Operand).AcceptVisitor(this);
          return;
      }
      switch (node.DateTimePart) {
        case SqlDateTimePart.DayOfYear:
          DateTimeExtractDayOfYear(node.Operand).AcceptVisitor(this);
          return;
        case SqlDateTimePart.DayOfWeek:
          DateTimeExtractDayOfWeek(node.Operand).AcceptVisitor(this);
          return;
      }
#if NET6_0_OR_GREATER
      switch (node.DatePart) {
        case SqlDatePart.DayOfYear:
          DateTimeExtractDayOfYear(node.Operand).AcceptVisitor(this);
          return;
        case SqlDatePart.DayOfWeek:
          DateTimeExtractDayOfWeek(node.Operand).AcceptVisitor(this);
          return;
      }
#endif
      base.Visit(node);
    }

    protected override void VisitSelectFrom(SqlSelect node)
    {
      if (node.From != null) {
        base.VisitSelectFrom(node);
      }
      else {
        _ = context.Output.Append(" FROM DUAL");
      }
    }

    public override void Visit(SqlJoinHint node)
    {
      var method = translator.Translate(node.Method);
      if (string.IsNullOrEmpty(method)) {
        return;
      }

      _ = context.Output.Append(method);
      _ = context.Output.AppendOpeningPunctuation("(");
      node.Table.AcceptVisitor(this);
      _ = context.Output.AppendClosingPunctuation(")");
    }

    public override void Visit(SqlFastFirstRowsHint node) => 
      context.Output.Append(string.Format("FIRST_ROWS({0})", node.Amount));

    public override void Visit(SqlNativeHint node) => context.Output.Append(node.HintText);
    

    public override void Visit(SqlForceJoinOrderHint node)
    {
      if (node.Tables.IsNullOrEmpty()) {
        _ = context.Output.Append("ORDERED");
      }
      else {
        _  = context.Output.AppendOpeningPunctuation("LEADING(");
        using (context.EnterCollectionScope()) {
          foreach (var table in node.Tables) {
            table.AcceptVisitor(this);
          }
        }
        _ = context.Output.AppendClosingPunctuation(")");
      }
    }

    public override void Visit(SqlUpdate node)
    {
      if (node.From != null) {
        throw new NotSupportedException(Strings.ExOracleDoesNotSupportUpdateFromStatements);
      }
      base.Visit(node);
    }

    public override void Visit(SqlUnary node)
    {
      switch (node.NodeType) {
      case SqlNodeType.BitNot:
        BitNot(node.Operand).AcceptVisitor(this);
        return;
      default:
        base.Visit(node);
        return;
      }
    }

    public override void Visit(SqlBinary node)
    {
      switch (node.NodeType) {
        case SqlNodeType.Modulo:
          SqlDml.FunctionCall("MOD", node.Left, node.Right).AcceptVisitor(this);
          return;
        case SqlNodeType.BitAnd:
          BitAnd(node.Left, node.Right).AcceptVisitor(this);
          return;
        case SqlNodeType.BitOr:
          BitOr(node.Left, node.Right).AcceptVisitor(this);
          return;
        case SqlNodeType.BitXor:
          BitXor(node.Left, node.Right).AcceptVisitor(this);
          return;
#if NET6_0_OR_GREATER
        case SqlNodeType.TimePlusInterval:
          TimeAddInterval(node.Left, node.Right).AcceptVisitor(this);
          return;
        case SqlNodeType.TimeMinusTime:
          TimeAddInterval(node.Left, node.Right, true).AcceptVisitor(this);
          return;
#endif
        default:
          base.Visit(node);
          return;
      }
    }

    private static SqlExpression DateTimeAddYMInterval(SqlExpression dateTime, SqlExpression units, in string component) =>
      dateTime + SqlDml.FunctionCall(NumToYMIntervalFunctionName, units, AnsiString(component));

    private static SqlExpression DateTimeAddDSInterval(SqlExpression dateTime, SqlExpression units, in string component) =>
      dateTime + SqlDml.FunctionCall(NumToDSIntervalFunctionName, units, AnsiString(component));

    private static SqlExpression IntervalConstruct(SqlExpression nanoseconds)
    {
      const long nanosecondsPerSecond = 1000000000;
      return SqlDml.FunctionCall(NumToDSIntervalFunctionName,
        nanoseconds / SqlDml.Literal(nanosecondsPerSecond), AnsiString(SecondIntervalPart));
    }

    private static SqlExpression DateTimeConstruct(SqlExpression years, SqlExpression months, SqlExpression days) =>
      SqlDml.FunctionCall("TO_TIMESTAMP",
        SqlDml.FunctionCall(ToCharFunctionName, ((years * 100) + months) * 100 + days),
        AnsiString("YYYYMMDD"));
#if NET6_0_OR_GREATER

    private static SqlExpression DateConstruct(SqlExpression years, SqlExpression months, SqlExpression days) =>
      SqlDml.FunctionCall("TO_DATE",
        SqlDml.FunctionCall(ToCharFunctionName, ((years * 100) + months) * 100 + days),
        AnsiString("YYYYMMDD"));

    private static SqlExpression TimeConstruct(IReadOnlyList<SqlExpression> arguments)
    {
      SqlExpression hour, minute, second, microsecond;
      if (arguments.Count == 4) {
        hour = arguments[0];
        minute = arguments[1];
        second = arguments[2];
        microsecond = arguments[3] * 10000;
      }
      else if (arguments.Count == 1) {
        var ticks = arguments[0];
        if (SqlHelper.IsTimeSpanTicks(ticks, out var sourceExpression)) {
          // try to optimize and reduce calculations when TimeSpan.Ticks where used for TimeOnly(ticks) ctor
          var days = SqlDml.Extract(SqlIntervalPart.Day, sourceExpression);
          var hours = days * 24 + SqlDml.Extract(SqlIntervalPart.Hour, sourceExpression);

          var hourString1 = SqlDml.Cast(hours, new SqlValueType(SqlType.VarChar, 3));
          var sourceExpressionAsString = SqlDml.FunctionCall(ToCharFunctionName, sourceExpression);
          var minuteToSecondsSubstring = SqlDml.Substring(sourceExpressionAsString, SqlDml.FunctionCall("INSTR", sourceExpressionAsString, AnsiString(":")) - 1 , 16);
          var composedTimeString1 = SqlDml.Concat(AnsiString("0 "), hourString1, minuteToSecondsSubstring);
          return SqlDml.FunctionCall(ToDSIntervalFunctionName, new[] { composedTimeString1 });
        }
        else {
          hour = SqlDml.Cast(ticks / 36000000000, new SqlValueType(SqlType.Decimal, 10, 0));
          minute = SqlDml.Cast((ticks / 600000000) % 60, new SqlValueType(SqlType.Decimal, 10, 0));
          second = SqlDml.Cast((ticks / 10000000) % 60, new SqlValueType(SqlType.Decimal, 10, 0));
          microsecond = SqlDml.Cast(ticks % 10000000, new SqlValueType(SqlType.Decimal, 10, 0));
        }
      }
      else {
        throw new InvalidOperationException("Unsupported count of parameters");
      }

      // using string version of time allows to control hours overflow
      // we cannot add hours, minutes and other parts to 00:00:00.000 time
      // because hours might step over 24 hours and start counting from 0.
      var hourString = SqlDml.Cast(hour, new SqlValueType(SqlType.VarChar, 3));
      var minuteString = SqlDml.Cast(minute, new SqlValueType(SqlType.VarChar, 2));
      var secondString = SqlDml.Cast(second, new SqlValueType(SqlType.VarChar, 2));
      var microsecondString = SqlDml.Cast(microsecond, new SqlValueType(SqlType.VarChar, 7));
      var composedTimeString = SqlDml.Concat(AnsiString("0 "), hourString, SqlDml.Literal(":"), minuteString, SqlDml.Literal(":"), secondString, SqlDml.Literal("."), microsecondString);
      return SqlDml.FunctionCall(ToDSIntervalFunctionName, new[] { composedTimeString });
    }

    private static SqlExpression TimeToNanoseconds(SqlExpression time)
    {
      var nPerHour = SqlDml.Extract(SqlTimePart.Hour, time) * NanosecondsPerHour;
      var nPerMinute = SqlDml.Extract(SqlTimePart.Minute, time) * NanosecondsPerMinute;
      var nPerSecond = SqlDml.Extract(SqlTimePart.Second, time) * NanosecondsPerSecond;
      var nPerMillisecond = SqlDml.Extract(SqlTimePart.Millisecond, time) * NanosecondsPerMillisecond;

      return nPerHour + nPerMinute + nPerSecond + nPerMillisecond;
    }

    private static SqlExpression TimeAddHourOrMinute(SqlExpression time, SqlExpression hourOrMinute, bool isHour)
    {
      var intervalLiteral = isHour ? "INTERVAL '1' HOUR" : "INTERVAL '1' MINUTE";
      return TimeAddInterval(time, hourOrMinute * SqlDml.Native(intervalLiteral));
    }

    private static SqlExpression TimeAddInterval(SqlExpression time, SqlExpression intervalToAdd, bool negateInterval = false)
    {
      var baseOp = (negateInterval) ? RefTimestamp + time - intervalToAdd
        : RefTimestamp + time + intervalToAdd;
      var getTimeOnly = SqlDml.FunctionCall(ToCharFunctionName, baseOp, AnsiString(TimeFormat));
      var pretendZeroDays = (SqlExpression) SqlDml.Concat(AnsiString("0 "), getTimeOnly);
      var dsInterval = SqlDml.FunctionCall(ToDSIntervalFunctionName, pretendZeroDays);
      var castToCorrectInterval = SqlDml.Cast(dsInterval, SqlType.Time);
      return castToCorrectInterval;
    }

    private static SqlExpression DateToString(SqlExpression date) =>
      SqlDml.FunctionCall(ToCharFunctionName, date, "YYYY-MM-DD");

    private static SqlExpression TimeToString(SqlExpression time) =>
      SqlDml.FunctionCall(ToCharFunctionName, RefTimestamp + time, AnsiString("HH24:MI:SS.FF7"));
#endif

    private static SqlExpression DateTimeExtractDayOfWeek(SqlExpression dateTime)
    {
      // TO_CHAR with 'D' returns values depending on NLS_TERRITORY setting,
      // so sunday can be 1 or 7
      // there is no equivalent for sqlserver's @@DATEFIRST function
      // so we need to emulate it with very stupid code
      return (SqlDml.FunctionCall("TO_NUMBER", SqlDml.FunctionCall(ToCharFunctionName, dateTime, AnsiString(DayOfWeekPartFormat))) + 7 - SundayNumber) % 7;
    }

    private static SqlExpression DateTimeExtractDayOfYear(SqlExpression dateTime) =>
      SqlDml.FunctionCall("TO_NUMBER", SqlDml.FunctionCall(ToCharFunctionName, dateTime, AnsiString(DayOfYearPartFormat)));

    private static SqlExpression BitAnd(SqlExpression left, SqlExpression right) =>
      SqlDml.FunctionCall("BITAND", left, right);

    private static SqlExpression BitOr(SqlExpression left, SqlExpression right) =>
      left + right - BitAnd(left, right);

    private static SqlExpression BitXor(SqlExpression left, SqlExpression right) =>
      BitOr(left, right) - BitAnd(left, right);

    private static SqlExpression BitNot(SqlExpression operand) => -1 - operand;

    private static SqlExpression Position(SqlExpression substring, SqlExpression _string) => //TODO : look into this (Malisa)
      SqlDml.FunctionCall("INSTR", _string, substring) - 1;

    private static SqlExpression DateTimeToStringIso(SqlExpression dateTime) =>
      SqlDml.FunctionCall(ToCharFunctionName, dateTime, "YYYY-MM-DD\"T\"HH24:MI:SS");

    private static SqlExpression DateTimeOffsetConstruct(SqlExpression dateTime, SqlExpression offset)
    {
      if (offset is not SqlLiteral<int> intOffset) {
        throw new InvalidOperationException();
      }

      var offsetValue = intOffset.Value;
      return SqlDml.FunctionCall("FROM_TZ",
        dateTime,
        AnsiString($"{((offsetValue < 0) ? "-" : "+")}{offsetValue / 60}:{offsetValue % 60}")
        );
    }

    private static SqlExpression DateTimeOffsetPartOffset(SqlExpression dateTimeOffset) =>
      SqlDml.Cast(dateTimeOffset, SqlType.DateTime)
        - SqlDml.Cast(DateTimeOffsetToUtcDateTime(dateTimeOffset), SqlType.DateTime);

    private static SqlExpression DateTimeOffsetTimeOfDay(SqlExpression dateTimeOffset) =>
      SqlDml.Cast(dateTimeOffset, SqlType.DateTime)
        - SqlDml.Truncate(SqlDml.Cast(dateTimeOffset, SqlType.DateTime));

    private static SqlExpression DateTimeOffsetTruncateOffset(SqlExpression dateTimeOffset) =>
      SqlDml.Cast(dateTimeOffset, SqlType.DateTime);

    private static SqlExpression DateTimeOffsetTruncate(SqlExpression dateTimeOffset) =>
      SqlDml.Truncate(dateTimeOffset);

    private static SqlExpression DateTimeOffsetToUtcDateTime(SqlExpression dateTimeOffset) =>
      SqlDml.FunctionCall("SYS_EXTRACT_UTC", dateTimeOffset);

    private static SqlExpression DateTimeOffsetExtractPart(SqlExpression dateTimeOffset, string dateTimeOffsetPart) =>
      SqlDml.FunctionCall(ToCharFunctionName, dateTimeOffset, AnsiString(dateTimeOffsetPart));

    private static SqlExpression DateTimeOffsetToLocalDateTime(SqlExpression dateTimeOffset) =>
      SqlDml.Cast(DateTimeOffsetToLocalTime(dateTimeOffset), SqlType.DateTime);

    private static SqlExpression DateTimeOffsetToLocalTime(SqlExpression dateTimeOffset) =>
      SqlDml.RawConcat(dateTimeOffset, SqlDml.Native(" AT LOCAL"));

    private static SqlExpression DateTimeToDateTimeOffset(SqlExpression dateTime) =>
      SqlDml.Cast(dateTime, SqlType.DateTimeOffset);

    private static SqlExpression DateTimeOffsetToDateTime(SqlExpression dateTimeOffset) =>
      SqlDml.Cast(dateTimeOffset, SqlType.DateTime);

    private static SqlExpression DateTimeOffsetToUtcTime(SqlExpression dateTimeOffset) =>
      SqlDml.RawConcat(dateTimeOffset, SqlDml.Native(" at time zone 'UTC'"));

    private static SqlExpression DateToDateTimeOffset(SqlExpression date) =>
      SqlDml.Cast(date, SqlType.DateTimeOffset);
#if NET6_0_OR_GREATER

    private static SqlExpression DateTimeToDate(SqlExpression dateTime) =>
      SqlDml.Cast(dateTime, SqlType.Date);

    private static SqlExpression DateToDateTime(SqlExpression date) =>
      SqlDml.Cast(date, SqlType.DateTime);

    private static SqlExpression DateTimeToTime(SqlExpression dateTime) =>
     SqlDml.Cast(SqlDml.FunctionCall(ToDSIntervalFunctionName,
        (SqlExpression) SqlDml.Concat(
          AnsiString("0 "),
          SqlDml.FunctionCall(ToCharFunctionName,
            dateTime,
            AnsiString(TimeFormat)
            )
          )
        ), SqlType.Time);

    private static SqlExpression TimeToDateTime(SqlExpression time) =>
      SqlDml.Cast(
        SqlDml.Literal(DateTime.MinValue) + time,
        SqlType.DateTime);

    private static SqlExpression DateTimeOffsetToDate(SqlExpression dateTimeOffset) =>
      SqlDml.Cast(dateTimeOffset, SqlType.Date);

    private static SqlExpression DateTimeOffsetToTime(SqlExpression dateTimeOffset) =>
      SqlDml.Cast(SqlDml.FunctionCall(ToDSIntervalFunctionName,
        (SqlExpression)SqlDml.Concat(
          AnsiString("0 "),
          SqlDml.FunctionCall(ToCharFunctionName,
            dateTimeOffset,
            AnsiString(TimeFormat)
            )
          )
        ), SqlType.Time);

    private static SqlExpression TimeToDateTimeOffset(SqlExpression time) =>
      SqlDml.Cast(
        SqlDml.Literal(DateTime.MinValue) + time,
        SqlType.DateTimeOffset);
#endif

    private static SqlExpression AnsiString(string value) => SqlDml.Native("'" + value + "'");

    // Constructors

    protected internal Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}