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
    private const string NumToDSIntervalFunctionName = "NUMTODSINTERVAL";
    private static readonly SqlExpression SundayNumber = SqlDml.Native(
      "TO_NUMBER(TO_CHAR(TIMESTAMP '2009-07-26 00:00:00.000', 'D'))");
    private static readonly SqlNative RefTimestamp = SqlDml.Native("timestamp '2009-01-01 00:00:00.000000'");

    public override void Visit(SqlFunctionCall node)
    {
      switch (node.FunctionType) {
        case SqlFunctionType.PadLeft:
        case SqlFunctionType.PadRight:
          SqlHelper.GenericPad(node).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetAddYears:
        case SqlFunctionType.DateTimeAddYears:
          DateTimeAddYMInterval(node.Arguments[0], node.Arguments[1], YearIntervalPart).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetAddMonths:
        case SqlFunctionType.DateTimeAddMonths:
          DateTimeAddYMInterval(node.Arguments[0], node.Arguments[1], MonthIntervalPart).AcceptVisitor(this);
          return;
        case SqlFunctionType.IntervalConstruct:
          IntervalConstruct(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeConstruct:
          DateTimeConstruct(node.Arguments[0], node.Arguments[1], node.Arguments[2]).AcceptVisitor(this);
          return;
#if NET6_0_OR_GREATER //DO_DATEONLY
        case SqlFunctionType.DateConstruct:
          DateConstruct(node.Arguments[0], node.Arguments[1], node.Arguments[2]).AcceptVisitor(this);
          return;
        case SqlFunctionType.TimeConstruct:
          TimeConstruct(node.Arguments[0], node.Arguments[1], node.Arguments[2], node.Arguments[3]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateAddYears: {
          DateTimeAddYMInterval(node.Arguments[0], node.Arguments[1], YearIntervalPart).AcceptVisitor(this);
          return;
        }
        case SqlFunctionType.DateAddMonths: {
          DateTimeAddYMInterval(node.Arguments[0], node.Arguments[1], MonthIntervalPart).AcceptVisitor(this);
          return;
        }
        case SqlFunctionType.DateAddDays: {
          DateTimeAddDSInterval(node.Arguments[0], node.Arguments[1], DayIntervalPart).AcceptVisitor(this);
          return;
        }
        case SqlFunctionType.TimeAddHours: {
          TimeAddHourOrMinute(node.Arguments[0], node.Arguments[1], true).AcceptVisitor(this);
          return;
        }
        case SqlFunctionType.TimeAddMinutes: {
          TimeAddHourOrMinute(node.Arguments[0], node.Arguments[1], false).AcceptVisitor(this);
          return;
        }
        case SqlFunctionType.DateToString: {
          DateToString(node.Arguments[0]).AcceptVisitor(this);
          return;
        }
        case SqlFunctionType.TimeToString: {
          TimeToString(node.Arguments[0]).AcceptVisitor(this);
          return;
        }
#endif
        case SqlFunctionType.IntervalAbs:
          SqlHelper.IntervalAbs(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.IntervalToMilliseconds:
          SqlHelper.IntervalToMilliseconds(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.IntervalToNanoseconds:
          SqlHelper.IntervalToNanoseconds(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.Position:
          Position(node.Arguments[0], node.Arguments[1]).AcceptVisitor(this);
          return;
        case SqlFunctionType.CharLength:
          SqlDml.Coalesce(SqlDml.FunctionCall("LENGTH", node.Arguments[0]), 0).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeToStringIso:
          DateTimeToStringIso(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetConstruct:
          DateTimeOffsetConstruct(node.Arguments[0], node.Arguments[1]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetTimeOfDay:
          DateTimeOffsetTimeOfDay(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetToLocalTime:
          DateTimeOffsetToLocalTime(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeToDateTimeOffset:
          DateTimeToDateTimeOffset(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetToUtcTime:
          DateTimeOffsetToUtcTime(node.Arguments[0]).AcceptVisitor(this);
          return;
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
#if NET6_0_OR_GREATER //DO_DATEONLY
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
#if NET6_0_OR_GREATER //DO_DATEONLY
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
      dateTime + SqlDml.FunctionCall(NumToDSIntervalFunctionName, units, AnsiString(component));

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

#if NET6_0_OR_GREATER //DO_DATEONLY
    private static SqlExpression DateConstruct(SqlExpression years, SqlExpression months, SqlExpression days) =>
      SqlDml.FunctionCall("TO_DATE",
        SqlDml.FunctionCall(ToCharFunctionName, ((years * 100) + months) * 100 + days),
        AnsiString("YYYYMMDD"));

    private static SqlExpression TimeAddHourOrMinute(SqlExpression time, SqlExpression hourOrMinute, bool isHour)
    {
      var intervalLiteral = isHour ? "INTERVAL '1' HOUR" : "INTERVAL '1' MINUTE";
      return TimeAddInterval(time, hourOrMinute * SqlDml.Native(intervalLiteral));
    }

    private static SqlExpression TimeAddInterval(SqlExpression time, SqlExpression intervalToAdd, bool negateInterval = false)
    {
      var baseOp = (negateInterval) ? RefTimestamp + time - intervalToAdd
        : RefTimestamp + time + intervalToAdd;
      var getTimeOnly = SqlDml.FunctionCall(ToCharFunctionName, baseOp, AnsiString("HH24:MI:SS.FF6"));
      var pretendZeroDays = (SqlExpression) SqlDml.Concat(AnsiString("0 "), getTimeOnly);
      var dsInterval = SqlDml.FunctionCall("TO_DSINTERVAL", pretendZeroDays);
      var castToCorrectInterval = SqlDml.Cast(dsInterval, SqlType.Time);
      return castToCorrectInterval;
    }

    private static SqlExpression TimeConstruct(
      SqlExpression hours, SqlExpression minutes, SqlExpression seconds, SqlExpression milliseconds) =>
        SqlDml.FunctionCall(NumToDSIntervalFunctionName,
          seconds + (minutes * 60) + (hours * 3600) + (milliseconds / 1000),
          AnsiString(SecondIntervalPart));

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

    private static SqlExpression DateTimeOffsetToUtcTime(SqlExpression dateTimeOffset) =>
      SqlDml.RawConcat(dateTimeOffset, SqlDml.Native(" at time zone 'UTC'"));

    private static SqlExpression AnsiString(string value) => SqlDml.Native("'" + value + "'");

    // Constructors

    protected internal Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}