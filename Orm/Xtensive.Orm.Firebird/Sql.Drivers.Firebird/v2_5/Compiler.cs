// Copyright (C) 2011-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Csaba Beer
// Created:    2011.01.17

using System;
using System.Linq;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Core;
using System.Collections.Generic;

namespace Xtensive.Sql.Drivers.Firebird.v2_5
{
  internal class Compiler : SqlCompiler
  {
    protected const long NanosecondsPerDay = 86400000000000;
    protected const long NanosecondsPerHour = 3600000000000;
    protected const long NanosecondsPerMinute = 60000000000;
    protected const long NanosecondsPerSecond = 1000000000;
    protected const long NanosecondsPerMillisecond = 1000000;
    protected const long MillisecondsPerDay = 86400000;
    protected const long MillisecondsPerSecond = 1000L;

    private bool case_SqlDateTimePart_DayOfYear;
    private bool case_SqlDateTimePart_Second;


    /// <inheritdoc/>
    public override void Visit(SqlSelect node)
    {
      using (context.EnterScope(node)) {
        var comment = node.Comment;
        VisitCommentIfBefore(comment);
        AppendTranslatedEntry(node);
        VisitCommentIfWithin(comment);
        VisitSelectLimitOffset(node);
        VisitSelectColumns(node);
        VisitSelectFrom(node);
        VisitSelectWhere(node);
        VisitSelectGroupBy(node);
        VisitSelectOrderBy(node);
        VisitSelectLock(node);
        AppendTranslatedExit(node);
        VisitCommentIfAfter(comment);
      }
    }

    /// <inheritdoc/>
    protected override void VisitSelectFrom(SqlSelect node)
    {
      if (node.From != null) {
        base.VisitSelectFrom(node);
      }
      else {
        _ = context.Output.Append(" FROM RDB$DATABASE");
      }
    }

    /// <inheritdoc/>
    public override void Visit(SqlQueryExpression node)
    {
      using (context.EnterScope(node)) {
        //bool needOpeningParenthesis = false;
        //bool needClosingParenthesis = false;
        AppendTranslated(node, QueryExpressionSection.Entry);
        //if (needOpeningParenthesis)
        //  context.Output.Append("(");
        node.Left.AcceptVisitor(this);
        //if (needClosingParenthesis)
        //  context.Output.Append(")");
        AppendTranslated(node.NodeType);
        AppendTranslated(node, QueryExpressionSection.All);
        //if (needOpeningParenthesis)
        //  context.Output.Append("(");
        node.Right.AcceptVisitor(this);
        //if (needClosingParenthesis)
        //  context.Output.Append(")");
        AppendTranslated(node, QueryExpressionSection.Exit);
      }
    }

    /// <inheritdoc/>
    public override void Visit(SqlExtract node)
    {
      switch (node.IntervalPart) {
        case SqlIntervalPart.Day:
          Visit(CastToLong(node.Operand / NanosecondsPerDay));
          return;
        case SqlIntervalPart.Hour:
          Visit(CastToLong(node.Operand / (60 * 60 * NanosecondsPerSecond)) % 24);
          return;
        case SqlIntervalPart.Minute:
          Visit(CastToLong(node.Operand / (60 * NanosecondsPerSecond)) % 60);
          return;
        case SqlIntervalPart.Second:
          Visit(CastToLong(node.Operand / NanosecondsPerSecond) % 60);
          return;
        case SqlIntervalPart.Millisecond:
          Visit(CastToLong(node.Operand / NanosecondsPerMillisecond) % MillisecondsPerSecond);
          return;
        case SqlIntervalPart.Nanosecond:
          Visit(CastToLong(node.Operand));
          return;
      }
#if NET6_0_OR_GREATER
      if (((node.IsDatePart && node.DatePart == SqlDatePart.DayOfYear)
          || (node.IsDateTimePart && node.DateTimePart == SqlDateTimePart.DayOfYear))) {
        if (!case_SqlDateTimePart_DayOfYear) {
          case_SqlDateTimePart_DayOfYear = true;
          Visit(SqlDml.Add(node, SqlDml.Literal(1)));
          case_SqlDateTimePart_DayOfYear = false;
        }
        else {
          base.Visit(node);
        }
        return;
      }
      else if (node.IsSecondExtraction) {
        if (!case_SqlDateTimePart_Second) {
          case_SqlDateTimePart_Second = true;
          Visit(SqlDml.Truncate(node));
          case_SqlDateTimePart_Second = false;
        }
        else {
          base.Visit(node);
        }
        return;
      }
#else
      switch (node.DateTimePart) {
        case SqlDateTimePart.DayOfYear:
          if (!case_SqlDateTimePart_DayOfYear) {
            case_SqlDateTimePart_DayOfYear = true;
            Visit(SqlDml.Add(node, SqlDml.Literal(1)));
            case_SqlDateTimePart_DayOfYear = false;
          }
          else {
            base.Visit(node);
          }
          return;
        case SqlDateTimePart.Second:
          if (!case_SqlDateTimePart_Second) {
            case_SqlDateTimePart_Second = true;
            Visit(SqlDml.Truncate(node));
            case_SqlDateTimePart_Second = false;
          }
          else {
            base.Visit(node);
          }
          return;
      }
#endif

      base.Visit(node);
    }

    /// <inheritdoc/>
    public override void Visit(SqlUnary node)
    {
      if (node.NodeType == SqlNodeType.BitNot) {
        Visit(BitNot(node.Operand));
        return;
      }
      base.Visit(node);
    }

    /// <inheritdoc/>
    public override void Visit(SqlBinary node)
    {
      switch (node.NodeType) {
        case SqlNodeType.DateTimePlusInterval:
          DateTimeAddInterval(node.Left, node.Right).AcceptVisitor(this);
          return;
        case SqlNodeType.DateTimeMinusInterval:
          DateTimeAddInterval(node.Left, -node.Right).AcceptVisitor(this);
          return;
        case SqlNodeType.DateTimeMinusDateTime:
          DateTimeSubtractDateTime(node.Left, node.Right).AcceptVisitor(this);
          return;
#if NET6_0_OR_GREATER
        case SqlNodeType.TimePlusInterval:
          TimeAddInterval(node.Left, node.Right).AcceptVisitor(this);
          return;
        case SqlNodeType.TimeMinusTime:
          TimeSubtractTime(node.Left, node.Right).AcceptVisitor(this);
          return;
#endif
        case SqlNodeType.Modulo:
          Visit(SqlDml.FunctionCall(translator.TranslateToString(SqlNodeType.Modulo), node.Left, node.Right));
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
        default:
          base.Visit(node);
          return;
      }
    }

    /// <inheritdoc/>
    public override void Visit(SqlFunctionCall node)
    {
      var arguments = node.Arguments;

      switch (node.FunctionType) {
        case SqlFunctionType.Concat:
          Visit(SqlDml.Concat(arguments.ToArray(node.Arguments.Count)));
          return;
        case SqlFunctionType.DateTimeTruncate:
          Visit(SqlDml.Cast(arguments[0], new SqlValueType("Date")));
          return;
        case SqlFunctionType.IntervalToMilliseconds:
          Visit(CastToLong(arguments[0]) / NanosecondsPerMillisecond);
          return;
        case SqlFunctionType.IntervalConstruct:
        case SqlFunctionType.IntervalToNanoseconds:
          Visit(CastToLong(arguments[0]));
          return;
        case SqlFunctionType.DateTimeAddMonths:
          Visit(DateAddMonth(arguments[0], arguments[1]));
          return;
        case SqlFunctionType.DateTimeAddYears:
          Visit(DateAddYear(arguments[0], arguments[1]));
          return;
        case SqlFunctionType.DateTimeConstruct:
          ConstructDateTime(arguments).AcceptVisitor(this);
          return;
#if NET6_0_OR_GREATER
        case SqlFunctionType.DateAddYears:
          Visit(DateAddYear(arguments[0], arguments[1]));
          return;
        case SqlFunctionType.DateAddMonths:
          Visit(DateAddMonth(arguments[0], arguments[1]));
          return;
        case SqlFunctionType.DateAddDays:
          Visit(DateAddDay(arguments[0], arguments[1]));
          return;
        case SqlFunctionType.DateConstruct:
          ConstructDate(arguments).AcceptVisitor(this);
          return;
        case SqlFunctionType.TimeAddHours:
          Visit(DateAddHour(node.Arguments[0], node.Arguments[1]));
          return;
        case SqlFunctionType.TimeAddMinutes:
          Visit(DateAddMinute(node.Arguments[0], node.Arguments[1]));
          return;
        case SqlFunctionType.TimeConstruct:
          ConstructTime(arguments).AcceptVisitor(this);
          return;
        case SqlFunctionType.TimeToNanoseconds:
          TimeToNanoseconds(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateToString:
          Visit(DateToString(arguments[0]));
          return;
        case SqlFunctionType.TimeToString:
          Visit(TimeToString(arguments[0]));
          return;
        case SqlFunctionType.DateToDateTime:
          DateToDateTime(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeToDate:
          DateTimeToDate(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeToTime:
          DateTimeToTime(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.TimeToDateTime:
          TimeToDateTime(arguments[0]).AcceptVisitor(this);
          return;
#endif
        case SqlFunctionType.DateTimeToStringIso:
          Visit(DateTimeToStringIso(arguments[0]));
          return;
      }

      base.Visit(node);
    }

    /// <inheritdoc/>
    public override void Visit(SqlRenameTable node) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void Visit(SqlAlterSequence node)
    {
      translator.Translate(context, node, NodeSection.Entry);
      translator.Translate(context, node, NodeSection.Exit);
    }

    protected virtual SqlExpression ConstructDateTime(IReadOnlyList<SqlExpression> arguments)
    {
      return DateAddDay(
        DateAddMonth(
          DateAddYear(
            SqlDml.Cast(SqlDml.Literal(new DateTime(2001, 1, 1)), SqlType.DateTime),
            arguments[0] - 2001),
          arguments[1] - 1),
        arguments[2] - 1);
    }
#if NET6_0_OR_GREATER

    protected virtual SqlExpression ConstructDate(IReadOnlyList<SqlExpression> arguments)
    {
      return DateAddDay(
        DateAddMonth(
          DateAddYear(
            SqlDml.Cast(SqlDml.Literal(new DateOnly(2001, 1, 1)), SqlType.Date),
            arguments[0] - 2001),
          arguments[1] - 1),
        arguments[2] - 1);
    }

    protected virtual SqlExpression ConstructTime(IReadOnlyList<SqlExpression> arguments)
    {
      SqlExpression hour, minute, second, millisecond;
      if (arguments.Count == 4) {
        hour = arguments[0];
        minute = arguments[1];
        second = arguments[2];
        millisecond = arguments[3] * 10;
      }
      else if (arguments.Count == 1) {
        var ticks = arguments[0];
        // try to optimize and reduce calculations when TimeSpan.Ticks where used for TimeOnly(ticks) ctor
        ticks = SqlHelper.IsTimeSpanTicks(ticks, out var sourceInterval) ? sourceInterval / 100 : ticks;
        hour = SqlDml.Cast(ticks / 36000000000, SqlType.Int32);
        minute = SqlDml.Cast((ticks / 600000000) % 60, SqlType.Int32);
        second = SqlDml.Cast((ticks / 10000000) % 60, SqlType.Int32);
        millisecond = SqlDml.Cast((ticks % 10000000) / 1000, SqlType.Int32);
      }
      else {
        throw new InvalidOperationException("Unsupported count of parameters");
      }

      // using string version of time allows to control hours overflow
      // we cannot add hours, minutes and other parts to 00:00:00.0000 time
      // because hours might step over 24 hours and start counting from 0.
      var hourString = SqlDml.Cast(hour, new SqlValueType(SqlType.VarChar, 3));
      var minuteString = SqlDml.Cast(minute, new SqlValueType(SqlType.VarChar, 2));
      var secondString = SqlDml.Cast(second, new SqlValueType(SqlType.VarChar, 2));
      var millisecondString = SqlDml.Cast(millisecond, new SqlValueType(SqlType.VarChar, 4));
      var composedTimeString = SqlDml.Concat(hourString, SqlDml.Literal(":"), minuteString, SqlDml.Literal(":"), secondString, SqlDml.Literal("."), millisecondString);
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
#endif

    #region Static helpers

    protected static SqlExpression DateTimeSubtractDateTime(SqlExpression date1, SqlExpression date2)
    {
      return (CastToLong(DateDiffDay(date2, date1)) * NanosecondsPerDay)
        +
        (CastToLong(DateDiffMillisecond(DateAddDay(date2, DateDiffDay(date2, date1)), date1)) *
          NanosecondsPerMillisecond);
    }
#if NET6_0_OR_GREATER

    protected static SqlExpression TimeSubtractTime(SqlExpression time1, SqlExpression time2)
    {
      return SqlDml.Modulo(
        NanosecondsPerDay + CastToLong(DateDiffMillisecond(time2, time1)) * NanosecondsPerMillisecond,
        NanosecondsPerDay);
    }
#endif

    protected static SqlExpression DateTimeAddInterval(SqlExpression date, SqlExpression interval)
    {
      return DateAddMillisecond(
        DateAddDay(date, interval / NanosecondsPerDay),
        (interval / NanosecondsPerMillisecond) % (MillisecondsPerDay));
    }

    protected static SqlExpression TimeAddInterval(SqlExpression time, SqlExpression interval) =>
      DateAddMillisecond(time, (interval / NanosecondsPerMillisecond) % (MillisecondsPerDay));

    protected static SqlCast CastToLong(SqlExpression arg) => SqlDml.Cast(arg, SqlType.Int64);

    protected static SqlUserFunctionCall DateDiffDay(SqlExpression date1, SqlExpression date2) =>
      SqlDml.FunctionCall("DATEDIFF", SqlDml.Native("DAY"), date1, date2);

    protected static SqlUserFunctionCall DateDiffMillisecond(SqlExpression date1, SqlExpression date2) =>
      SqlDml.FunctionCall("DATEDIFF", SqlDml.Native("MILLISECOND"), date1, date2);

    protected static SqlUserFunctionCall DateAddYear(SqlExpression date, SqlExpression years) =>
      SqlDml.FunctionCall("DATEADD", SqlDml.Native("YEAR"), years, date);

    protected static SqlUserFunctionCall DateAddMonth(SqlExpression date, SqlExpression months) =>
      SqlDml.FunctionCall("DATEADD", SqlDml.Native("MONTH"), months, date);

    protected static SqlUserFunctionCall DateAddDay(SqlExpression date, SqlExpression days) =>
      SqlDml.FunctionCall("DATEADD", SqlDml.Native("DAY"), days, date);

    protected static SqlUserFunctionCall DateAddHour(SqlExpression date, SqlExpression hours) =>
      SqlDml.FunctionCall("DATEADD", SqlDml.Native("HOUR"), hours, date);

    protected static SqlUserFunctionCall DateAddMinute(SqlExpression date, SqlExpression minutes) =>
      SqlDml.FunctionCall("DATEADD", SqlDml.Native("MINUTE"), minutes, date);

    protected static SqlUserFunctionCall DateAddSecond(SqlExpression date, SqlExpression seconds) =>
      SqlDml.FunctionCall("DATEADD", SqlDml.Native("SECOND"), seconds, date);

    protected static SqlUserFunctionCall DateAddMillisecond(SqlExpression date, SqlExpression milliseconds) =>
      SqlDml.FunctionCall("DATEADD", SqlDml.Native("MILLISECOND"), milliseconds, date);

    protected static SqlUserFunctionCall BitAnd(SqlExpression left, SqlExpression right) =>
      SqlDml.FunctionCall("BIN_AND", left, right);

    protected static SqlUserFunctionCall BitOr(SqlExpression left, SqlExpression right) =>
      SqlDml.FunctionCall("BIN_OR", left, right);

    protected static SqlUserFunctionCall BitXor(SqlExpression left, SqlExpression right) =>
      SqlDml.FunctionCall("BIN_XOR", left, right);

    protected static SqlUserFunctionCall BitNot(SqlExpression operand) =>
      SqlDml.FunctionCall("BIN_NOT", operand);
#if NET6_0_OR_GREATER

    protected static SqlExpression TimeToDateTime(SqlExpression time) =>
      SqlDml.Cast(time, SqlType.DateTime);

    protected static SqlExpression DateTimeToTime(SqlExpression dateTime) =>
      SqlDml.Cast(dateTime, SqlType.Time);

    protected static SqlExpression DateToDateTime(SqlExpression date) =>
      SqlDml.Cast(date, SqlType.DateTime);

    protected static SqlExpression DateTimeToDate(SqlExpression dateTime) =>
      SqlDml.Cast(dateTime, SqlType.Date);

    protected static SqlFunctionCall DateToString(SqlExpression date) =>
      SqlDml.Substring(date, 0, 10);

    protected static SqlConcat TimeToString(SqlExpression time) =>
      SqlDml.Concat(SqlDml.Substring(time, 0, 12), SqlDml.Literal("0000"));
#endif

    protected static SqlConcat DateTimeToStringIso(SqlExpression dateTime)
    {
      var date = SqlDml.Substring(dateTime, 0, 10);
      var time = SqlDml.Substring(dateTime, 11, 8);

      return SqlDml.Concat(date, SqlDml.Literal("T"), time);
    }

#endregion

    protected internal Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}