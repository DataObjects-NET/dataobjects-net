// Copyright (C) 2003-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Providers.PostgreSql;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;
using SqlCompiler = Xtensive.Sql.Compiler.SqlCompiler;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_0
{
  internal class Compiler : SqlCompiler
  {
    private const string DateTimeIsoFormat = "YYYY-MM-DD\"T\"HH24:MI:SS";
    private const string DateFormat = "YYYY-MM-DD";
    private const string TimeFormat = "HH24:MI:SS.US0";

    private const long NanosecondsPerHour = 3600000000000;
    private const long NanosecondsPerMinute = 60000000000;
    private const long NanosecondsPerSecond = 1000000000;
    private const long NanosecondsPerMillisecond = 1000000;

    private static readonly Type SqlPlaceholderType = typeof(SqlPlaceholder);

    private static readonly SqlNative OneYearInterval = SqlDml.Native("interval '1 year'");
    private static readonly SqlNative OneMonthInterval = SqlDml.Native("interval '1 month'");
    private static readonly SqlNative OneDayInterval = SqlDml.Native("interval '1 day'");

    private static readonly SqlNative OneHourInterval = SqlDml.Native("interval '1 hour'");
    private static readonly SqlNative OneMinuteInterval = SqlDml.Native("interval '1 minute'");
    private static readonly SqlNative OneSecondInterval = SqlDml.Native("interval '1 second'");

    private static readonly SqlLiteral ReferenceDateTimeLiteral = SqlDml.Literal(new DateTime(2001, 1, 1));
    private static readonly SqlLiteral EpochLiteral = SqlDml.Literal(new DateTime(1970, 1, 1));
    private static readonly SqlLiteral ReferenceDateLiteral = SqlDml.Literal(new DateOnly(2001, 1, 1));

    private static readonly SqlNative ZeroTimeLiteral = SqlDml.Native("'00:00:00.000000'::time(6)");
    private static readonly SqlNative MaxTimeLiteral = SqlDml.Native("'23:59:59.999999'::time(6)");

    private static readonly SqlNative DateMinValue = SqlDml.Native("'0001-01-01'::timestamp");
    private static readonly SqlNative DateMaxValue = SqlDml.Native("'9999-12-31'::timestamp");

    private static readonly SqlNative DateTimeMinValue = SqlDml.Native("'0001-01-01 00:00:00.000000'::timestamp(6)");
    private static readonly SqlNative DateTimeMaxValue = SqlDml.Native("'9999-12-31 23:59:59.999999'::timestamp(6)");

    private static readonly SqlNative DateTimeOffsetMinValue = SqlDml.Native("'0001-01-01 00:00:00.000000+00:00'::timestamp(6) with time zone");
    private static readonly SqlNative DateTimeOffsetMaxValue = SqlDml.Native("'9999-12-31 23:59:59.999999+00:00'::timestamp(6)");

    private static readonly SqlNative Infinity = SqlDml.Native("'Infinity'");
    private static readonly SqlNative NegativeInfinity = SqlDml.Native("'-Infinity'");


    protected readonly bool infinityAliasForDatesEnabled;

    /// <inheritdoc/>
    public override void Visit(SqlDeclareCursor node)
    {
    }

    /// <inheritdoc/>
    public override void Visit(SqlOpenCursor node) => base.Visit(node.Cursor.Declare());

    /// <inheritdoc/>
    public override void Visit(SqlBinary node)
    {
      var right = node.Right as SqlArray;
      if (right is not null && (node.NodeType==SqlNodeType.In || node.NodeType==SqlNodeType.NotIn)) {
        if (right.ItemType == SqlPlaceholderType) {
          using (context.EnterScope(node)) {
            AppendTranslatedEntry(node);
            node.Left.AcceptVisitor(this);
            translator.Translate(context.Output, node.NodeType);
            _ = context.Output.AppendOpeningPunctuation("(");
            var items = right.GetValues();
            for (var i = 0; i < items.Length - 1; i++) {
              Visit((SqlPlaceholder) items[i]);
              _ = context.Output.Append(translator.RowItemDelimiter);
            }
            Visit((SqlPlaceholder) items[^1]);
            _ = context.Output.Append(")");
            AppendTranslatedExit(node);
          }
        }
        else {
          var row = SqlDml.Row(right.GetValues().Select(value => SqlDml.Literal(value)).ToArray());
          base.Visit(node.NodeType == SqlNodeType.In ? SqlDml.In(node.Left, row) : SqlDml.NotIn(node.Left, row));
        }
        return;
      }
      switch (node.NodeType) {
        case SqlNodeType.DateTimeOffsetMinusDateTimeOffset:
          (node.Left - node.Right).AcceptVisitor(this);
          return;
        case SqlNodeType.DateTimeOffsetMinusInterval:
          (node.Left - node.Right).AcceptVisitor(this);
          return;
        case SqlNodeType.DateTimeOffsetPlusInterval:
          (node.Left + node.Right).AcceptVisitor(this);
          return;
        case SqlNodeType.TimeMinusTime:
          SqlDml.Cast(
            SqlDml.Cast(
              (ReferenceDateLiteral + node.Left) - (ReferenceDateLiteral + node.Right),
              SqlType.Time),
            SqlType.Interval).AcceptVisitor(this);
          return;
        default:
          base.Visit(node);
          return;
      }
    }

    /// <inheritdoc/>
    public override void Visit(SqlFunctionCall node)
    {
      const double nanosecondsPerSecond = 1000000000.0;

      switch (node.FunctionType) {
        case SqlFunctionType.PadLeft:
        case SqlFunctionType.PadRight:
          SqlHelper.GenericPad(node).AcceptVisitor(this);
          return;
        case SqlFunctionType.Rand:
          SqlDml.FunctionCall(translator.TranslateToString(SqlFunctionType.Rand)).AcceptVisitor(this);
          return;
        case SqlFunctionType.Square:
          SqlDml.Power(node.Arguments[0], 2).AcceptVisitor(this);
          return;
        case SqlFunctionType.IntervalConstruct:
          ((node.Arguments[0] / SqlDml.Literal(nanosecondsPerSecond)) * OneSecondInterval).AcceptVisitor(this);
          return;
        case SqlFunctionType.IntervalToMilliseconds:
          VisitIntervalToMilliseconds(node);
          return;
        case SqlFunctionType.IntervalToNanoseconds:
          SqlHelper.IntervalToNanoseconds(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.IntervalAbs:
          SqlHelper.IntervalAbs(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeConstruct:
          ConstructDateTime(node.Arguments).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateConstruct:
          ConstructDate(node.Arguments).AcceptVisitor(this);
          return;
        case SqlFunctionType.TimeConstruct:
          ConstructTime(node.Arguments).AcceptVisitor(this);
          return;
        case SqlFunctionType.TimeToNanoseconds:
          TimeToNanoseconds(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeTruncate:
          DateTimeTruncate(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeAddMonths:
          DateTimeAddXxx(node.Arguments[0], node.Arguments[1] * OneMonthInterval).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeAddYears:
          DateTimeAddXxx(node.Arguments[0], node.Arguments[1] * OneYearInterval).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateAddYears:
          DateAddXxx(node.Arguments[0], node.Arguments[1] * OneYearInterval).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateAddMonths:
          DateAddXxx(node.Arguments[0], node.Arguments[1] * OneMonthInterval).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateAddDays:
          DateAddXxx(node.Arguments[0], node.Arguments[1] * OneDayInterval).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateToString:
          DateTimeToStringIso(node.Arguments[0], DateFormat, infinityAliasForDatesEnabled).AcceptVisitor(this);
          return;
        case SqlFunctionType.TimeAddHours:
          (node.Arguments[0] + node.Arguments[1] * OneHourInterval).AcceptVisitor(this);
          return;
        case SqlFunctionType.TimeAddMinutes:
          (node.Arguments[0] + node.Arguments[1] * OneMinuteInterval).AcceptVisitor(this);
          return;
        case SqlFunctionType.TimeToString:
          DateTimeToStringIso(node.Arguments[0], TimeFormat, false).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeToStringIso:
          DateTimeToStringIso(node.Arguments[0], DateTimeIsoFormat, infinityAliasForDatesEnabled).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetTimeOfDay:
          DateTimeOffsetTimeOfDay(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetAddMonths:
          SqlDml.Cast(node.Arguments[0] + node.Arguments[1] * OneMonthInterval, SqlType.DateTimeOffset).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetAddYears:
          SqlDml.Cast(node.Arguments[0] + node.Arguments[1] * OneYearInterval, SqlType.DateTimeOffset).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetConstruct:
          ConstructDateTimeOffset(node.Arguments[0], node.Arguments[1]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeToDateTimeOffset:
          DateTimeToDateTimeOffset(node.Arguments[0], infinityAliasForDatesEnabled).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetToDateTime:
          DateTimeOffsetToDateTime(node.Arguments[0], infinityAliasForDatesEnabled).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeToDate:
          DateTimeToDate(node.Arguments[0], infinityAliasForDatesEnabled).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateToDateTime:
          DateToDateTime(node.Arguments[0], infinityAliasForDatesEnabled).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeToTime:
          DateTimeToTime(node.Arguments[0], infinityAliasForDatesEnabled).AcceptVisitor(this);
          return;
        case SqlFunctionType.TimeToDateTime:
          TimeToDateTime(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetToDate:
          DateTimeOffsetToDate(node.Arguments[0], infinityAliasForDatesEnabled).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateToDateTimeOffset:
          DateToDateTimeOffset(node.Arguments[0], infinityAliasForDatesEnabled).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetToTime:
          DateTimeOffsetToTime(node.Arguments[0], infinityAliasForDatesEnabled).AcceptVisitor(this);
          return;
        case SqlFunctionType.TimeToDateTimeOffset:
          TimeToDateTimeOffset(node.Arguments[0]).AcceptVisitor(this);
          return;
      }
      base.Visit(node);
    }

    /// <inheritdoc/>
    public override void Visit(SqlCustomFunctionCall node)
    {
      if (node.FunctionType == PostgresqlSqlFunctionType.NpgsqlPointExtractX) {
        NpgsqlPointExtractPart(node.Arguments[0], 0).AcceptVisitor(this);
        return;
      }
      if (node.FunctionType == PostgresqlSqlFunctionType.NpgsqlPointExtractY) {
        NpgsqlPointExtractPart(node.Arguments[0], 1).AcceptVisitor(this);
        return;
      }
      if (node.FunctionType == PostgresqlSqlFunctionType.NpgsqlTypeExtractPoint) {
        NpgsqlTypeExtractPoint(node.Arguments[0], node.Arguments[1]).AcceptVisitor(this);
        return;
      }
      if (node.FunctionType == PostgresqlSqlFunctionType.NpgsqlBoxExtractHeight) {
        NpgsqlBoxExtractHeight(node.Arguments[0]).AcceptVisitor(this);
        return;
      }
      if (node.FunctionType == PostgresqlSqlFunctionType.NpgsqlBoxExtractWidth) {
        NpgsqlBoxExtractWidth(node.Arguments[0]).AcceptVisitor(this);
        return;
      }
      if (node.FunctionType == PostgresqlSqlFunctionType.NpgsqlCircleExtractCenter) {
        NpgsqlCircleExtractCenter(node.Arguments[0]).AcceptVisitor(this);
        return;
      }
      if (node.FunctionType == PostgresqlSqlFunctionType.NpgsqlCircleExtractRadius) {
        NpgsqlCircleExtractRadius(node.Arguments[0]).AcceptVisitor(this);
        return;
      }
      if (node.FunctionType == PostgresqlSqlFunctionType.NpgsqlPathAndPolygonCount) {
        NpgsqlPathAndPolygonCount(node.Arguments[0]).AcceptVisitor(this);
        return;
      }
      if (node.FunctionType == PostgresqlSqlFunctionType.NpgsqlPathAndPolygonOpen) {
        NpgsqlPathAndPolygonOpen(node.Arguments[0]).AcceptVisitor(this);
        return;
      }
      if (node.FunctionType == PostgresqlSqlFunctionType.NpgsqlPathAndPolygonContains) {
        NpgsqlPathAndPolygonContains(node.Arguments[0], node.Arguments[1]).AcceptVisitor(this);
        return;
      }
      if (node.FunctionType == PostgresqlSqlFunctionType.NpgsqlTypeOperatorEquality) {
        NpgsqlTypeOperatorEquality(node.Arguments[0], node.Arguments[1]).AcceptVisitor(this);
        return;
      }
      if (node.FunctionType == PostgresqlSqlFunctionType.NpgsqlPointConstructor) {
        var newNode = SqlDml.RawConcat(
          NpgsqlTypeConstructor(node.Arguments[0], node.Arguments[1], "point'"),
          SqlDml.Native("'"));
        newNode.AcceptVisitor(this);
        return;
      }
      if (node.FunctionType == PostgresqlSqlFunctionType.NpgsqlBoxConstructor) {
        NpgsqlTypeConstructor(node.Arguments[0], node.Arguments[1], "box").AcceptVisitor(this);
        return;
      }
      if (node.FunctionType == PostgresqlSqlFunctionType.NpgsqlCircleConstructor) {
        NpgsqlTypeConstructor(node.Arguments[0], node.Arguments[1], "circle").AcceptVisitor(this);
        return;
      }
      if (node.FunctionType == PostgresqlSqlFunctionType.NpgsqlLSegConstructor) {
        NpgsqlTypeConstructor(node.Arguments[0], node.Arguments[1], "lseg").AcceptVisitor(this);
        return;
      }
      base.Visit(node);
    }

    protected virtual void VisitIntervalToMilliseconds(SqlFunctionCall node)
    {
      SqlHelper.IntervalToMilliseconds(node.Arguments[0]).AcceptVisitor(this);
    }

    private static SqlExpression DateTimeToStringIso(SqlExpression dateTime, in string isoFormat, bool infinityEnabled)
    {
      var conversionExpression = SqlDml.FunctionCall("TO_CHAR", dateTime, isoFormat);
      if (infinityEnabled) {
        var @case = SqlDml.Case();
        @case[dateTime == Infinity]         = SqlDml.FunctionCall("TO_CHAR", DateTimeMaxValue, isoFormat);
        @case[dateTime == NegativeInfinity] = SqlDml.FunctionCall("TO_CHAR", DateTimeMinValue, isoFormat);
        @case.Else = conversionExpression;
        return @case;
      }
      return conversionExpression;
    }

    private static SqlExpression IntervalToIsoString(SqlExpression interval, bool signed)
    {
      if (!signed) {
        return SqlDml.FunctionCall("TO_CHAR", interval, "HH24:MI");
      }
      var hours = SqlDml.FunctionCall("TO_CHAR", SqlDml.Extract(SqlIntervalPart.Hour, interval), "SG09");
      var minutes = SqlDml.FunctionCall("TO_CHAR", SqlDml.Extract(SqlIntervalPart.Minute, interval), "FM09");
      return SqlDml.Concat(hours, ":", minutes);
    }

    protected static SqlExpression NpgsqlPointExtractPart(SqlExpression expression, int part) =>
      SqlDml.RawConcat(expression, SqlDml.Native($"[{part}]"));

    protected static SqlExpression NpgsqlTypeExtractPoint(SqlExpression expression, SqlExpression numberPoint)
    {
      var numberPointAsInt = numberPoint as SqlLiteral<int>;
      var valueNumberPoint = numberPointAsInt != null ? numberPointAsInt.Value : 0;

      return SqlDml.RawConcat(
        SqlDml.Native("("),
        SqlDml.RawConcat(
          expression,
          SqlDml.Native($"[{valueNumberPoint}])")));
    }

    protected static SqlExpression NpgsqlBoxExtractHeight(SqlExpression expression) =>
      SqlDml.FunctionCall("HEIGHT", expression);

    protected static SqlExpression NpgsqlBoxExtractWidth(SqlExpression expression) =>
      SqlDml.FunctionCall("WIDTH", expression);

    protected static SqlExpression NpgsqlCircleExtractCenter(SqlExpression expression) =>
      SqlDml.RawConcat(SqlDml.Native("@@"), expression);

    protected static SqlExpression NpgsqlCircleExtractRadius(SqlExpression expression) =>
      SqlDml.FunctionCall("RADIUS", expression);

    protected static SqlExpression NpgsqlPathAndPolygonCount(SqlExpression expression) =>
      SqlDml.FunctionCall("NPOINTS", expression);

    protected static SqlExpression NpgsqlPathAndPolygonOpen(SqlExpression expression) =>
      SqlDml.FunctionCall("ISOPEN", expression);

    protected static SqlExpression NpgsqlPathAndPolygonContains(SqlExpression expression, SqlExpression point)
    {
      return SqlDml.RawConcat(
        expression,
        SqlDml.RawConcat(
          SqlDml.Native("@>"),
          point));
    }

    protected static SqlExpression NpgsqlTypeOperatorEquality(SqlExpression left, SqlExpression right)
    {
      return SqlDml.RawConcat(left,
        SqlDml.RawConcat(
          SqlDml.Native("~="),
          right));
    }

    private static SqlExpression NpgsqlTypeConstructor(SqlExpression left, SqlExpression right, string type)
    {
      return SqlDml.RawConcat(
        SqlDml.Native($"{type}("),
        SqlDml.RawConcat(left,
          SqlDml.RawConcat(
            SqlDml.Native(","),
            SqlDml.RawConcat(
              right,
              SqlDml.Native(")")))));
    }

    // it might be moved to context as some kind of extension
    private readonly HashSet<SqlExtract> delayedExtractOperations = new();

    private (SqlExpression min, SqlExpression max) ExtractMinMaxValuesForPart(SqlExtract node)
    {
      var actualPart = (SqlDateTimeOffsetPart) (node.IsDateTimePart
        ? (int) node.DateTimePart
        : node.IsDatePart
          ? (int) node.DatePart
          : node.IsDateTimeOffsetPart
            ? (int) node.DateTimeOffsetPart
            : throw new ArgumentOutOfRangeException());

      return actualPart switch {
        SqlDateTimeOffsetPart.Year => (SqlDml.Literal(1), SqlDml.Literal(9999)),
        SqlDateTimeOffsetPart.Month => (SqlDml.Literal(1), SqlDml.Literal(12)),
        SqlDateTimeOffsetPart.Day => (SqlDml.Literal(1), SqlDml.Literal(31)),
        SqlDateTimeOffsetPart.DayOfWeek => (SqlDml.Literal(1), SqlDml.Literal(5)), // Monday and Friday
        SqlDateTimeOffsetPart.DayOfYear => (SqlDml.Literal(1), SqlDml.Literal(365)),
        SqlDateTimeOffsetPart.Hour => (SqlDml.Literal(0), SqlDml.Literal(23)),
        SqlDateTimeOffsetPart.Minute or SqlDateTimeOffsetPart.Second => (SqlDml.Literal(0), SqlDml.Literal(59)),
        SqlDateTimeOffsetPart.Millisecond => (SqlDml.Literal(0), SqlDml.Literal(999)),
        SqlDateTimeOffsetPart.Nanosecond => (SqlDml.Literal(0), SqlDml.Literal(900)),
        SqlDateTimeOffsetPart.TimeZoneHour or SqlDateTimeOffsetPart.TimeZoneMinute => (SqlDml.Literal(0), SqlDml.Literal(0)),
        _ => throw new ArgumentOutOfRangeException()
      };
    }

    public override void Visit(SqlExtract node)
    {
      if (node.IsDateTimeOffsetPart) {
        switch (node.DateTimeOffsetPart) {
          case SqlDateTimeOffsetPart.Date:
            DateTimeOffsetExtractDate(node.Operand).AcceptVisitor(this);
            return;
          case SqlDateTimeOffsetPart.DateTime:
            DateTimeOffsetExtractDateTime(node.Operand).AcceptVisitor(this);
            return;

          case SqlDateTimeOffsetPart.UtcDateTime:
            DateTimeOffsetToUtcDateTime(node.Operand).AcceptVisitor(this);
            return;
          case SqlDateTimeOffsetPart.LocalDateTime:
            DateTimeOffsetToLocalDateTime(node.Operand).AcceptVisitor(this);
            return;
          case SqlDateTimeOffsetPart.Offset:
            DateTimeOffsetExtractOffset(node);
            return;
        }
      }

      if (infinityAliasForDatesEnabled
        && (node.IsDatePart || node.IsDateTimePart || node.IsDateTimeOffsetPart)
        && !delayedExtractOperations.Remove(node)) {
        // If DateTime.MinValue => -Infinity and DateTime.MaxValue => Infinity conversion happens
        // in Npgsql driver then we have to use SQL case statement to return correct values to the user.
        // In this case we use original expression in ELSE part of CASE statement and postpone visiting

        _ = delayedExtractOperations.Add(node);
        (SqlExpression min, SqlExpression max) minMaxValues = ExtractMinMaxValuesForPart(node);
        var @case = SqlDml.Case();
        @case[node.Operand == Infinity] = minMaxValues.min;
        @case[node.Operand == NegativeInfinity] = minMaxValues.max;
        @case.Else = node;
        @case.AcceptVisitor(this);
      }
      else {
        base.Visit(node);
      }
    }

    public override void Visit(SqlLiteral node)
    {
      if (!infinityAliasForDatesEnabled) {
        base.Visit(node);
      }
      else {
        // to keep constants and parameters work the same way we have to make this check
        var value = node.GetValue();
        var infinityExpression = value switch {
          DateTime dtValue => dtValue == DateTime.MinValue
            ? NegativeInfinity
            : dtValue == DateTime.MaxValue
              ? Infinity
              : null,
          DateOnly dtValue => dtValue == DateOnly.MinValue
            ? NegativeInfinity
            : dtValue == DateOnly.MaxValue
              ? Infinity
              : null,
          DateTimeOffset dtValue => dtValue == DateTimeOffset.MinValue
            ? NegativeInfinity
            : dtValue == DateTimeOffset.MaxValue
              ? Infinity
              : null,
          _ => null
        };
        
        if (infinityExpression is null) {
          base.Visit(node);
        }
        else {
          infinityExpression.AcceptVisitor(this);
        }

      }
    }

    protected virtual SqlExpression ConstructDateTime(IReadOnlyList<SqlExpression> arguments)
    {
      return ReferenceDateTimeLiteral
        + (OneYearInterval * (arguments[0] - 2001))
        + (OneMonthInterval * (arguments[1] - 1))
        + (OneDayInterval * (arguments[2] - 1));
    }

    protected virtual SqlExpression ConstructDate(IReadOnlyList<SqlExpression> arguments)
    {
      return ReferenceDateLiteral
        + (OneYearInterval * (arguments[0] - 2001))
        + (OneMonthInterval * (arguments[1] - 1))
        + (OneDayInterval * (arguments[2] - 1));
    }

    protected virtual SqlExpression ConstructTime(IReadOnlyList<SqlExpression> arguments)
    {
      SqlExpression hour, minute, second, microsecond;
      if (arguments.Count == 4) {
        hour = arguments[0];
        minute = arguments[1];
        second = arguments[2];
        microsecond = arguments[3] * 1000;
      }
      else if (arguments.Count == 1) {
        var ticks = arguments[0];
        if (SqlHelper.IsTimeSpanTicks(ticks, out var sourceInterval)) {
          // try to optimize and reduce calculations when TimeSpan.Ticks where used for TimeOnly(ticks) ctor
          return SqlDml.Cast(SqlDml.Cast(sourceInterval, SqlType.VarChar), SqlType.Time);
        }
        else {
          hour = SqlDml.Cast(ticks / 36000000000, SqlType.Int32);
          minute = SqlDml.Cast((ticks / 600000000) % 60, SqlType.Int32);
          second = SqlDml.Cast((ticks / 10000000) % 60, SqlType.Int32);
          microsecond = SqlDml.Cast((ticks % 10000000) / 10, SqlType.Int32);
        }
      }
      else {
        throw new InvalidOperationException("Unsupported count of parameters");
      }

      // Using string version of time allows to control hours overflow
      // we cannot add hours, minutes and other parts to 00:00:00.000000 time
      // because hours might step over 24 hours and start counting from 0.
      // Starting from v10 new function is in use, which controlls overflow
      var hourString = SqlDml.Cast(hour, new SqlValueType(SqlType.VarChar, 3));
      var minuteString = SqlDml.Cast(minute, new SqlValueType(SqlType.VarChar, 2));
      var secondString = SqlDml.Cast(second, new SqlValueType(SqlType.VarChar, 2));
      var microsecondString = SqlDml.Cast(microsecond, new SqlValueType(SqlType.VarChar, 7));
      var composedTimeString = SqlDml.Concat(hourString, SqlDml.Literal(":"), minuteString, SqlDml.Literal(":"), secondString, SqlDml.Literal("."), microsecondString);
      return SqlDml.Cast(composedTimeString, SqlType.Time);
    }

    protected virtual SqlExpression TimeToNanoseconds(SqlExpression time)
    {
      var nPerHour = SqlDml.Extract(SqlTimePart.Hour, time) * NanosecondsPerHour;
      var nPerMinute = SqlDml.Extract(SqlTimePart.Minute, time) * NanosecondsPerMinute;
      var nPerSecond = SqlDml.Extract(SqlTimePart.Second, time) * NanosecondsPerSecond;
      var nPerMillisecond = SqlDml.Extract(SqlTimePart.Millisecond, time) * NanosecondsPerMillisecond;

      return nPerHour + nPerMinute + nPerSecond + nPerMillisecond;
    }


    protected SqlExpression DateTimeAddXxx(SqlExpression dateTime, SqlExpression addPart)
    {
      var resultExpression = (dateTime + addPart);
      if (infinityAliasForDatesEnabled) {
        var @case = SqlDml.Case();
        @case[dateTime == Infinity]         = DateTimeMaxValue + addPart;
        @case[dateTime == NegativeInfinity] = DateTimeMinValue + addPart;
        @case.Else = resultExpression;
        return @case;
      }
      return resultExpression;
    }

    protected SqlExpression DateTimeTruncate(SqlExpression dateTime)
    {
      var truncateExpression = SqlDml.FunctionCall("date_trunc", "day", dateTime);
      if (infinityAliasForDatesEnabled) {
        var @case = SqlDml.Case();
        @case[dateTime == Infinity]         = SqlDml.Cast(DateMaxValue, SqlType.DateTime);
        @case[dateTime == NegativeInfinity] = SqlDml.Cast(DateMinValue, SqlType.DateTime);
        @case.Else = truncateExpression;
        return @case;
      }
      return truncateExpression;
    }

    protected SqlExpression DateAddXxx(SqlExpression date, SqlExpression addPart)
    {
      var resultExpression = (date + addPart);
      if (infinityAliasForDatesEnabled) {
        var @case = SqlDml.Case();
        @case[date == Infinity]         = DateMaxValue + addPart;
        @case[date == NegativeInfinity] = DateMinValue + addPart;
        @case.Else = resultExpression;
        return @case;
      }
      return (date + addPart);
    }

    protected SqlExpression DateTimeOffsetExtractDate(SqlExpression timestamp)
    {
      var extractExpression = SqlDml.FunctionCall("DATE", timestamp);
      if (infinityAliasForDatesEnabled) {
        var @case = SqlDml.Case();
        @case[timestamp == Infinity]         = DateMaxValue;
        @case[timestamp == NegativeInfinity] = DateMinValue;
        @case.Else = extractExpression;
        return @case;
      }
      return extractExpression;
    }

    protected SqlExpression DateTimeOffsetExtractDateTime(SqlExpression timestamp)
    {
      var extractExpression = SqlDml.Cast(timestamp, SqlType.DateTime);
      if (infinityAliasForDatesEnabled) {
        var @case = SqlDml.Case();
        @case[timestamp == Infinity]         = DateTimeMaxValue;
        @case[timestamp == NegativeInfinity] = DateTimeMinValue;
        @case.Else = extractExpression;
        return @case;
      }

      return extractExpression;
    }

    protected SqlExpression DateTimeOffsetToUtcDateTime(SqlExpression timestamp)
    {
      var extractExpression = GetDateTimeInTimeZone(timestamp, TimeSpan.Zero);
      if (infinityAliasForDatesEnabled) {
        var @case = SqlDml.Case();
        @case[timestamp == Infinity]         = DateTimeMaxValue;
        @case[timestamp == NegativeInfinity] = DateTimeMinValue;
        @case.Else = extractExpression;
        return @case;
      }
      return extractExpression;
    }

    protected SqlExpression DateTimeOffsetToLocalDateTime(SqlExpression timestamp)
    {
      var extractExpression = SqlDml.Cast(timestamp, SqlType.DateTime);
      if (infinityAliasForDatesEnabled) {
        var @case = SqlDml.Case();
        @case[timestamp == Infinity]         = DateTimeMaxValue;
        @case[timestamp == NegativeInfinity] = DateTimeMinValue;
        @case.Else = extractExpression;
        return @case;
      }

      return extractExpression;
    }

    protected void DateTimeOffsetExtractOffset(SqlExtract node)
    {
      if (infinityAliasForDatesEnabled && !delayedExtractOperations.Remove(node)) {
        // If DateTime.MinValue => -Infinity and DateTime.MaxValue => Infinity conversion happens
        // in Npgsql driver then we have to use SQL case statement to return correct values to the user.
        // In this case we use original expression in ELSE part of CASE statement and postpone visiting

        _ = delayedExtractOperations.Add(node);
        var @case = SqlDml.Case();
        @case[node.Operand == Infinity]         = SqlDml.Native("'00:00'");
        @case[node.Operand == NegativeInfinity] = SqlDml.Native("'00:00'");
        @case.Else = node;
        @case.AcceptVisitor(this);
      }
      else {
        using (context.EnterScope(node)) {
          AppendTranslatedEntry(node);
          translator.Translate(context.Output, node.DateTimeOffsetPart);
          AppendTranslated(node, ExtractSection.From);
          node.Operand.AcceptVisitor(this);
          AppendSpace();
          AppendTranslatedExit(node);
          AppendTranslated(SqlNodeType.Multiply);
          OneSecondInterval.AcceptVisitor(this);
        }
      }
    }

    protected SqlExpression DateTimeOffsetTimeOfDay(SqlExpression timestamp)
    {
      var resultExpression = DateTimeOffsetSubstract(timestamp, SqlDml.DateTimeTruncate(timestamp));
      if (infinityAliasForDatesEnabled) {
        var @case = SqlDml.Case();
        @case[timestamp == Infinity]         = SqlDml.Cast(MaxTimeLiteral, SqlType.Interval);
        @case[timestamp == NegativeInfinity] = SqlDml.Cast(ZeroTimeLiteral, SqlType.Interval);
        @case.Else = resultExpression;
        return @case;
      }
      return resultExpression;
    }

    protected SqlExpression DateTimeOffsetSubstract(SqlExpression timestamp1, SqlExpression timestamp2)
    {
      return timestamp1 - timestamp2;
    }

    protected SqlExpression ConstructDateTimeOffset(SqlExpression dateTimeExpression, SqlExpression offsetInMinutes)
    {
      var dateTimeAsStringExpression = GetDateTimeAsStringExpression(dateTimeExpression);
      var offsetAsStringExpression = GetOffsetAsStringExpression(offsetInMinutes);
      return ConstructDateTimeOffsetFromExpressions(dateTimeAsStringExpression, offsetAsStringExpression);
    }

    protected SqlExpression ConstructDateTimeOffsetFromExpressions(SqlExpression datetimeStringExpression, SqlExpression offsetStringExpression) =>
      SqlDml.Cast(SqlDml.Concat(datetimeStringExpression, " ", offsetStringExpression), SqlType.DateTimeOffset);

    protected SqlExpression GetDateTimeAsStringExpression(SqlExpression dateTimeExpression) =>
      SqlDml.FunctionCall("To_Char", dateTimeExpression, "YYYY-MM-DD\"T\"HH24:MI:SS.MS");

    protected SqlExpression GetOffsetAsStringExpression(SqlExpression offsetInMinutes)
    {
      var hours = 0;
      var minutes = 0;
      //if something simple as double or int or even timespan can be separated into hours and minutes parts
      if (TryDivideOffsetIntoParts(offsetInMinutes, ref hours, ref minutes))
        return SqlDml.Native($"'{ZoneStringFromParts(hours, minutes)}'");

      var intervalExpression = offsetInMinutes * OneMinuteInterval;
      return IntervalToIsoString(intervalExpression, true);
    }

    private static SqlExpression DateTimeToDateTimeOffset(SqlExpression dateTime, bool infinityAliasEnabled)
    {
      var convertExpression = SqlDml.Cast(dateTime, SqlType.DateTimeOffset);
      if (infinityAliasEnabled) {
        var @case = SqlDml.Case();
        @case[dateTime == Infinity]         = DateTimeOffsetMaxValue;
        @case[dateTime == NegativeInfinity] = DateTimeOffsetMinValue;
        @case.Else = convertExpression;
        return @case;
      }
      return convertExpression;
    }

    private static SqlExpression DateTimeOffsetToDateTime(SqlExpression dateTimeOffset, bool infinityAliasEnabled)
    {
      var convertExpression = SqlDml.Cast(dateTimeOffset, SqlType.DateTime);
      if (infinityAliasEnabled) {
        var @case = SqlDml.Case();
        @case[dateTimeOffset == Infinity]         = DateTimeMaxValue;
        @case[dateTimeOffset == NegativeInfinity] = DateTimeMinValue;
        @case.Else = convertExpression;
        return @case;
      }
      return convertExpression;
    }

    private static SqlExpression DateTimeToDate(SqlExpression dateTime, bool infinityAliasEnabled)
    {
      var convertExpression = SqlDml.Cast(dateTime, SqlType.Date);
      if (infinityAliasEnabled) {
        var @case = SqlDml.Case();
        @case[dateTime == Infinity]         = DateMaxValue;
        @case[dateTime == NegativeInfinity] = DateMinValue;
        @case.Else = convertExpression;
        return @case;
      }
      return convertExpression;
    }

    private static SqlExpression DateToDateTime(SqlExpression date, bool infinityAliasEnabled)
    {
      var convertExpression = SqlDml.Cast(date, SqlType.DateTime);
      if (infinityAliasEnabled) {
        var @case = SqlDml.Case();
        @case[date == Infinity]         = MaxTimeLiteral;
        @case[date == NegativeInfinity] = ZeroTimeLiteral;
        @case.Else = convertExpression;
        return @case;
      }
      return convertExpression;
    }

    private static SqlExpression DateTimeToTime(SqlExpression dateTime, bool infinityAliasEnabled)
    {
      var convertExpression = SqlDml.Cast(dateTime, SqlType.Time);
      if (infinityAliasEnabled) {
        var @case = SqlDml.Case();
        @case[dateTime == Infinity]         = MaxTimeLiteral;
        @case[dateTime == NegativeInfinity] = ZeroTimeLiteral;
        @case.Else = convertExpression;
        return @case;
      }
      return convertExpression;
    }

    private static SqlExpression TimeToDateTime(SqlExpression time) =>
      SqlDml.Cast(EpochLiteral + time, SqlType.DateTime);

    private static SqlExpression DateTimeOffsetToDate(SqlExpression dateTimeOffset, bool infinityAliasEnabled)
    {
      var convertExpression = SqlDml.Cast(dateTimeOffset, SqlType.Date);
      if (infinityAliasEnabled) {
        var @case = SqlDml.Case();
        @case[dateTimeOffset == Infinity]         = DateMaxValue;
        @case[dateTimeOffset == NegativeInfinity] = DateMinValue;
        @case.Else = convertExpression;
        return @case;
      }
      return convertExpression;
    }

    private static SqlExpression DateToDateTimeOffset(SqlExpression date, bool infinityAliasEnabled)
    {
      var convertExpression = SqlDml.Cast(date, SqlType.DateTimeOffset);
      if (infinityAliasEnabled) {
        var @case = SqlDml.Case();
        @case[date == Infinity]         = DateTimeOffsetMaxValue;
        @case[date == NegativeInfinity] = DateTimeOffsetMinValue;
        @case.Else = convertExpression;
        return @case;
      }
      return convertExpression;
    }

    private static SqlExpression DateTimeOffsetToTime(SqlExpression dateTimeOffset, bool infinityAliasEnabled)
    {
      var convertExpression = SqlDml.Cast(dateTimeOffset, SqlType.Time);
      if (infinityAliasEnabled) {
        var @case = SqlDml.Case();
        @case[dateTimeOffset == Infinity]         = MaxTimeLiteral;
        @case[dateTimeOffset == NegativeInfinity] = ZeroTimeLiteral;
        @case.Else = convertExpression;
        return @case;
      }
      return convertExpression;
    }

    private static SqlExpression TimeToDateTimeOffset(SqlExpression time) =>
      SqlDml.Cast(EpochLiteral + time, SqlType.DateTimeOffset);

    private string ZoneStringFromParts(int hours, int minutes) =>
      $"{(hours < 0 ? "-" : "+")}{Math.Abs(hours):00}:{Math.Abs(minutes):00}";

    private SqlExpression GetDateTimeInTimeZone(SqlExpression expression, SqlExpression zone) =>
      SqlDml.FunctionCall("TIMEZONE", zone, expression);

    private SqlExpression GetServerTimeZone() =>
      SqlDml.FunctionCall("CURRENT_SETTING", SqlDml.Literal("TIMEZONE"));

    private bool TryDivideOffsetIntoParts(SqlExpression offsetInMinutes, ref int hours , ref int minutes)
    {
      var offsetToDouble = offsetInMinutes as SqlLiteral<double>;
      if (offsetToDouble != null) {
        hours = (int) offsetToDouble.Value / 60;
        minutes = Math.Abs((int) offsetToDouble.Value % 60);
        return true;
      }
      var offsetToInt = offsetInMinutes as SqlLiteral<int>;
      if (offsetToInt != null) {
        hours = offsetToInt.Value / 60;
        minutes = Math.Abs(offsetToInt.Value % 60);
        return true;
      }
      var offsetToTimeSpan = offsetInMinutes as SqlLiteral<TimeSpan>;
      if (offsetToTimeSpan != null) {
        var totalMinutes = offsetToTimeSpan.Value.TotalMinutes;
        hours = (int) totalMinutes / 60;
        minutes = Math.Abs((int)totalMinutes % 60);
        return true;
      }
      return false;
    }

    // Constructors

    protected internal Compiler(PostgreSql.Driver driver)
      : base(driver)
    {
      infinityAliasForDatesEnabled = driver.PostgreServerInfo.InfinityAliasForDatesEnabled;
    }
  }
}
