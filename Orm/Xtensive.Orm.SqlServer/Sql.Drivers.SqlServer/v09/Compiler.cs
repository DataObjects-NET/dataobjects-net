// Copyright (C) 2009-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.03.11

using System;
using System.Linq;
using System.Collections.Generic;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.SqlServer.Resources;

namespace Xtensive.Sql.Drivers.SqlServer.v09
{
  internal class Compiler : SqlCompiler
  {
    #region Date parts
    protected const string NanosecondPart = "NS";
    protected const string MillisecondPart = "MS";
    protected const string SecondPart = "SECOND";
    protected const string MinutePart = "MINUTE";
    protected const string HourPart = "HOUR";
    protected const string DayPart = "DAY";
    protected const string MonthPart = "MONTH";
    protected const string YearPart = "YEAR";
    protected const string WeekdayPart = "WEEKDAY";
    #endregion

    protected const long NanosecondsPerDay = 86400000000000;
    protected const long NanosecondsPerHour = 3600000000000;
    protected const long NanosecondsPerMinute = 60000000000;
    protected const long NanosecondsPerSecond = 1000000000;
    protected const long NanosecondsPerMillisecond = 1000000;
    protected const long MillisecondsPerDay = 86400000;
    protected const long MillisecondsPerSecond = 1000L;

    protected static readonly SqlExpression DateFirst = SqlDml.Native("@@DATEFIRST");

    /// <inheritdoc/>
    public override void Visit(SqlSelect node)
    {
      using (context.EnterScope(node)) {
        var comment = node.Comment;
        VisitCommentIfBefore(comment);
        AppendTranslatedEntry(node);
        VisitCommentIfWithin(comment);
        VisitSelectLimitOffset(node);
        VisitSelectHints(node);
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
    public override void Visit(SqlUpdate node)
    {
      using (context.EnterScope(node)) {
        VisitUpdateEntry(node);
        VisitUpdateLimit(node);
        VisitUpdateUpdate(node);
        VisitUpdateSet(node);
        VisitUpdateFrom(node);
        VisitUpdateWhere(node);
        VisitUpdateExit(node);
      }
    }

    /// <inheritdoc/>
    protected override void VisitUpdateLimit(SqlUpdate node)
    {
      if (node.Limit is not null) {
        if (!Driver.ServerInfo.Query.Features.Supports(QueryFeatures.UpdateLimit)) {
          throw new NotSupportedException(Strings.ExStorageDoesNotSupportLimitationOfRowCountToUpdate);
        }

        AppendTranslated(node, UpdateSection.Limit);
        _ = context.Output.AppendOpeningPunctuation("(");
        node.Limit.AcceptVisitor(this);
        _ = context.Output.Append(")");
      }
    }

    /// <inheritdoc/>
    public override void Visit(SqlDelete node)
    {
      using (context.EnterScope(node)) {
        VisitDeleteEntry(node);
        VisitDeleteLimit(node);
        AppendTranslated(node, DeleteSection.From);
        VisitDeleteDelete(node);
        VisitDeleteFrom(node);
        VisitDeleteWhere(node);
        VisitDeleteExit(node);
      }
    }

    /// <inheritdoc/>
    protected override void VisitDeleteLimit(SqlDelete node)
    {
      if (node.Limit is not null) {
        if (!Driver.ServerInfo.Query.Features.Supports(QueryFeatures.DeleteLimit)) {
          throw new NotSupportedException(Strings.ExStorageDoesNotSupportLimitationOfRowCountToDelete);
        }

        AppendTranslated(node, DeleteSection.Limit);
        _ = context.Output.AppendOpeningPunctuation("(");
        node.Limit.AcceptVisitor(this);
        _ = context.Output.Append(")");
      }
    }

    /// <inheritdoc/>
    protected override void VisitSelectLock(SqlSelect node)
    {
      return;
    }

    /// <inheritdoc/>
    public override void Visit(SqlAlterTable node)
    {
      if (node.Action is SqlRenameColumn renameColumnAction) {
        ((Translator) translator).Translate(context, renameColumnAction);
        return;
      }
      if (node.Action is SqlDropConstraint dropConstrainAction) {
        if (dropConstrainAction.Constraint is DefaultConstraint) {
          var constraint = dropConstrainAction.Constraint as DefaultConstraint;
          if (constraint.NameIsStale) {
            //We must know name of default constraint for drop it.
            //But MS SQL creates name of default constrain by itself.
            //And if we moved table to another schema or database or renamed table by recreation during upgrade,
            //we doesn't know real name of default constraint.
            //Because of this we should find name of constraint in system views.
            //And we able to drop default constraint after that.
            ((Translator) translator).Translate(context, node, constraint);
            return;
          }
        }
      }
      base.Visit(node);
    }

    /// <inheritdoc/>
    public override void Visit(SqlFunctionCall node)
    {
      var arguments = node.Arguments;
      switch (node.FunctionType) {
        case SqlFunctionType.CharLength:
          (SqlDml.FunctionCall("DATALENGTH", arguments) / 2).AcceptVisitor(this);
          return;
        case SqlFunctionType.PadLeft:
        case SqlFunctionType.PadRight:
          GenericPad(node).AcceptVisitor(this);
          return;
        case SqlFunctionType.Round:
          // Round should always be called with 2 arguments
          if (arguments.Count == 1) {
            Visit(SqlDml.FunctionCall(
              translator.TranslateToString(SqlFunctionType.Round),
              arguments[0],
              SqlDml.Literal(0)));
            return;
          }
          break;
        case SqlFunctionType.Truncate:
          // Truncate is implemented as round(arg, 0, 1) call in MSSQL.
          // It's stupid, isn't it?
          Visit(SqlDml.FunctionCall(
            translator.TranslateToString(SqlFunctionType.Round),
            arguments[0],
            SqlDml.Literal(0),
            SqlDml.Literal(1)));
          return;
        case SqlFunctionType.Substring:
          if (arguments.Count == 2) {
            SqlExpression len = SqlDml.CharLength(arguments[0]);
            node = SqlDml.Substring(arguments[0], arguments[1], len);
            Visit(node);
            return;
          }
          break;
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
        case SqlFunctionType.DateAddYears:
          Visit(DateAddYear(arguments[0], arguments[1]));
          return;
        case SqlFunctionType.DateAddMonths:
          Visit(DateAddMonth(arguments[0], arguments[1]));
          return;
        case SqlFunctionType.DateAddDays:
          Visit(DateAddDay(arguments[0], arguments[1]));
          return;
        case SqlFunctionType.TimeAddHours:
          Visit(DateAddHour(arguments[0], arguments[1]));
          return;
        case SqlFunctionType.TimeAddMinutes:
          Visit(DateAddMinute(arguments[0], arguments[1]));
          return;
        case SqlFunctionType.DateTimeTruncate:
          DateTimeTruncate(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeConstruct:
          ConstructDateTime(arguments).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateConstruct:
          ConstructDate(arguments).AcceptVisitor(this);
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
        case SqlFunctionType.DateTimeToStringIso:
          Visit(DateTimeToStringIso(arguments[0]));
          return;
      }

      base.Visit(node);
    }

    /// <inheritdoc/>
    public override void Visit(SqlTrim node)
    {
      if (node.TrimCharacters!=null && !node.TrimCharacters.All(_char => _char==' ')) {
        throw new NotSupportedException(Strings.ExSqlServerSupportsTrimmingOfSpaceCharactersOnly);
      }

      using (context.EnterScope(node)) {
        AppendTranslated(node, TrimSection.Entry);
        node.Expression.AcceptVisitor(this);
        AppendTranslated(node, TrimSection.Exit);
      }
    }

    /// <inheritdoc/>
    public override void Visit(SqlExtract node)
    {
      if (node.DateTimePart == SqlDateTimePart.DayOfWeek) {
        Visit((DatePartWeekDay(node.Operand) + DateFirst + 6) % 7);
        return;
      }
      if (node.DatePart == SqlDatePart.DayOfWeek) {
        Visit((DatePartWeekDay(node.Operand) + DateFirst + 6) % 7);
        return;
      }

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
      base.Visit(node);
    }

    /// <inheritdoc/>
    public override void Visit(SqlBinary node)
    {
      switch (node.NodeType) {
        case SqlNodeType.DateTimePlusInterval:
          DateTimeAddInterval(node.Left, node.Right).AcceptVisitor(this);
          return;
        case SqlNodeType.DateTimeMinusDateTime:
          DateTimeSubtractDateTime(node.Left, node.Right).AcceptVisitor(this);
          return;
        case SqlNodeType.DateTimeMinusInterval:
          DateTimeAddInterval(node.Left, -node.Right).AcceptVisitor(this);
          return;
        case SqlNodeType.TimePlusInterval:
          TimeAddInterval(node.Left, node.Right).AcceptVisitor(this);
          return;
        case SqlNodeType.TimeMinusTime:
          TimeSubtractTime(node.Left, node.Right).AcceptVisitor(this);
          return;
        default:
          base.Visit(node);
          return;
      }
    }

    /// <inheritdoc/>
    public override void Visit(SqlRound node)
    {
      SqlExpression result;
      var shouldCastToDecimal = node.Type==TypeCode.Decimal;
      switch (node.Mode) {
        case MidpointRounding.ToEven:
          result = node.Length is null
            ? BankersRound(node.Argument, shouldCastToDecimal)
            : BankersRound(node.Argument, node.Length, shouldCastToDecimal);
          break;
        case MidpointRounding.AwayFromZero:
          result = node.Length is null
            ? RegularRound(node.Argument, shouldCastToDecimal)
            : RegularRound(node.Argument, node.Length, shouldCastToDecimal);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      result.AcceptVisitor(this);
    }

    /// <inheritdoc/>
    public override void Visit(SqlContainsTable node)
    {
      var output = context.Output;

      _ = output.AppendOpeningPunctuation("CONTAINSTABLE(");
      AppendTranslated(node.TargetTable.DataTable);
      _ = output.Append(", ");

      var targetColumns = node.TargetColumns;
      if (targetColumns.Count == 1) {
        TranslateSingleColumnName(targetColumns[0], node.Asterisk);
      }
      else {
        _ = output.Append('(');
        TranslateJoinedColumnNames(targetColumns);
        _ = output.Append(')');
      }

      _ = output.Append(", ");
      node.SearchCondition.AcceptVisitor(this);
      if (node.TopNByRank!=null) {
        _ = output.Append(", ");
        node.TopNByRank.AcceptVisitor(this);
      }
      _ = output.Append(") ");
    }

    /// <inheritdoc/>
    public override void Visit(SqlFreeTextTable node)
    {
      var output = context.Output;

      _ = output.AppendOpeningPunctuation("FREETEXTTABLE(");
      AppendTranslated(node.TargetTable.DataTable);
      _ = output.Append(", ");

      var targetColumns = node.TargetColumns;
      if (targetColumns.Count == 1) {
        TranslateSingleColumnName(targetColumns[0], node.Asterisk);
      }
      else {
        TranslateJoinedColumnNames(targetColumns);
      }

      _ = output.Append(", ");
      node.FreeText.AcceptVisitor(this);
      if (node.TopNByRank != null) {
        _ = context.Output.Append(", ");
        node.TopNByRank.AcceptVisitor(this);
      }
      _ = output.Append(") ");
    }

    /// <inheritdoc/>
    public override void Visit(SqlCreateIndex node, IndexColumn item)
    {
      base.Visit(node, item);

      if (item.TypeColumn!=null) {
        _ = context.Output.Append("TYPE COLUMN");
        translator.TranslateIdentifier(context.Output, item.TypeColumn.Name);
      }
      switch (item.Languages.Count) {
        case 0:
          break;
        case 1:
          if (!string.IsNullOrEmpty(item.Languages[0].Name))
            _ = context.Output.Append($"LANGUAGE '{item.Languages[0].Name}'");
          break;
        default:
          throw new InvalidOperationException(string.Format(
            Strings.ExMultipleLanguagesNotSupportedForFulltextColumnXOfIndexY, item.Name, item.Index.Name));
      }
    }

    /// <summary>
    /// Creates expression that truncates time part for given datetime
    /// </summary>
    /// <param name="date">Date (datetime datetimeoffset) expression</param>
    /// <returns>Result expression</returns>
    protected virtual SqlExpression DateTimeTruncate(SqlExpression date)
    {
      return DateAddMillisecond(DateAddSecond(DateAddMinute(DateAddHour(date,
        -SqlDml.Extract(SqlDateTimePart.Hour, date)),
        -SqlDml.Extract(SqlDateTimePart.Minute, date)),
        -SqlDml.Extract(SqlDateTimePart.Second, date)),
        -SqlDml.Extract(SqlDateTimePart.Millisecond, date));
    }

    /// <summary>
    /// Creates expression that represents subtraction of two <see cref="DateTime"/> expressions.
    /// </summary>
    /// <param name="date1">First <see cref="DateTime"/> expression.</param>
    /// <param name="date2">Second <see cref="DateTime"/> expression.</param>
    /// <returns>Result expression.</returns>
    /// <returns></returns>
    protected virtual SqlExpression DateTimeSubtractDateTime(SqlExpression date1, SqlExpression date2)
    {
      return CastToDecimal(DateDiffDay(date2, date1), 18, 0) * NanosecondsPerDay
          + CastToDecimal(DateDiffMillisecond(DateAddDay(date2, DateDiffDay(date2, date1)), date1), 18, 0) * NanosecondsPerMillisecond;
    }

    /// <summary>
    /// Creates expression that represents addition <paramref name="interval"/> to the given <paramref name="date"/>.
    /// </summary>
    /// <param name="date">Date (datatime, datetimeoffset) expression.</param>
    /// <param name="interval">Interval expression to add.</param>
    /// <returns></returns>
    protected virtual SqlExpression DateTimeAddInterval(SqlExpression date, SqlExpression interval)
    {
      return DateAddMillisecond(
        DateAddDay(date, interval / NanosecondsPerDay),
        (interval / NanosecondsPerMillisecond) % (MillisecondsPerDay));
    }

    /// <summary>
    /// Creates expression that represents construction of datetime value
    /// from arguments (year, month, day).
    /// </summary>
    /// <param name="arguments">Expressions representing year, month, and day.</param>
    /// <returns>Result expression.</returns>
    protected virtual SqlExpression ConstructDateTime(IReadOnlyList<SqlExpression> arguments)
    {
      return DateAddDay(DateAddMonth(DateAddYear(SqlDml.Literal(new DateTime(2001, 1, 1)),
        arguments[0] - 2001),
        arguments[1] - 1),
        arguments[2] - 1);
    }

    /// <summary>
    /// Creates expression that represents construction of date value
    /// from arguments (year, month, day).
    /// </summary>
    /// <param name="arguments">Expressions representing year, month, and day.</param>
    /// <returns>Result expression.</returns>
    protected virtual SqlExpression ConstructDate(IReadOnlyList<SqlExpression> arguments)
    {
      return SqlDml.Cast(DateAddDay(DateAddMonth(DateAddYear(SqlDml.Literal(new DateOnly(2001, 1, 1)),
        arguments[0] - 2001),
        arguments[1] - 1),
        arguments[2] - 1), SqlType.Date);
    }

    /// <summary>
    /// Creates expression that represents construction of time value from arguments.
    /// </summary>
    /// <param name="arguments">Expressions to construct time from.</param>
    /// <returns>Result expression.</returns>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    protected virtual SqlExpression ConstructTime(IReadOnlyList<SqlExpression> arguments)
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
        // try to optimize and reduce calculations when TimeSpan.Ticks where used for TimeOnly(ticks) ctor
        ticks = SqlHelper.IsTimeSpanTicks(ticks, out var sourceInterval) ? sourceInterval / 100 : ticks;
        hour = SqlDml.Cast(ticks / 36000000000, SqlType.Int32);
        minute = SqlDml.Cast((ticks / 600000000) % 60, SqlType.Int32);
        second = SqlDml.Cast((ticks / 10000000) % 60, SqlType.Int32);
        microsecond = SqlDml.Cast(ticks % 10000000, SqlType.Int32);
      }
      else {
        throw new InvalidOperationException("Unsupported count of parameters");
      }

      // Using string version of time allows to control hours overflow
      // we cannot add hours, minutes and other parts to 00:00:00.000000 time
      // because hours might step over 24 hours and start counting from 0.
      // Starting from v11 built-in function with hour overflow control is used.
      var hourString = SqlDml.Cast(hour, new SqlValueType(SqlType.VarChar, 3));
      var minuteString = SqlDml.Cast(minute, new SqlValueType(SqlType.VarChar, 2));
      var secondString = SqlDml.Cast(second, new SqlValueType(SqlType.VarChar, 2));
      var microsecondString = SqlDml.Cast(microsecond, new SqlValueType(SqlType.VarChar, 7));
      var composedTimeString = SqlDml.Concat(hourString, SqlDml.Literal(":"), minuteString, SqlDml.Literal(":"), secondString, SqlDml.Literal("."), microsecondString);
      return SqlDml.Cast(composedTimeString, SqlType.Time);
    }

    /// <summary>
    /// Creates expression that represents conversion of time value to nanoseconds.
    /// </summary>
    /// <param name="time">Time value to convert.</param>
    /// <returns>Result expression.</returns>
    protected virtual SqlExpression TimeToNanoseconds(SqlExpression time)
    {
      var nPerHour = SqlDml.Extract(SqlTimePart.Hour, time) * NanosecondsPerHour;
      var nPerMinute = SqlDml.Extract(SqlTimePart.Minute, time) * NanosecondsPerMinute;
      var nPerSecond = SqlDml.Extract(SqlTimePart.Second, time) * NanosecondsPerSecond;
      var n = SqlDml.Extract(SqlTimePart.Nanosecond, time);

      return nPerHour + nPerMinute + nPerSecond + n;
    }

    /// <summary>
    /// Creates expression that represents addition <paramref name="interval"/> to the given <paramref name="time"/>.
    /// </summary>
    /// <param name="time">Time expression.</param>
    /// <param name="interval">Interval expression to add.</param>
    /// <returns>Result expression.</returns>
    protected virtual SqlExpression TimeAddInterval(SqlExpression time, SqlExpression interval) =>
      DateAddMillisecond(time, (interval / NanosecondsPerMillisecond) % (MillisecondsPerDay));

    /// <summary>
    /// Creates expression that represents subtraction of two <see cref="TimeOnly"/> expressions.
    /// </summary>
    /// <param name="time1">First <see cref="TimeOnly"/> expression.</param>
    /// <param name="time2">Second <see cref="TimeOnly"/> expression.</param>
    /// <returns>Result expression.</returns>
    protected virtual SqlExpression TimeSubtractTime(SqlExpression time1, SqlExpression time2) =>
      SqlDml.Modulo(
        NanosecondsPerDay + CastToDecimal(DateDiffMillisecond(time2, time1), 18,0) * NanosecondsPerMillisecond,
        NanosecondsPerDay);

    private SqlExpression GenericPad(SqlFunctionCall node)
    {
      var operand = node.Arguments[0];
      var actualLength = SqlDml.CharLength(operand);
      var requiredLength = node.Arguments[1];
      var paddingExpression = node.Arguments.Count > 2
        ? SqlDml.FunctionCall("REPLICATE", node.Arguments[2], requiredLength - actualLength)
        : SqlDml.FunctionCall("SPACE", requiredLength - actualLength);
      SqlExpression resultExpression = node.FunctionType switch {
        SqlFunctionType.PadLeft => paddingExpression + operand,
        SqlFunctionType.PadRight => operand + paddingExpression,
        _ => throw new InvalidOperationException(),
      };
      var result = SqlDml.Case();
      _ = result.Add(actualLength < requiredLength, resultExpression);
      result.Else = operand;
      return result;
    }

    private void TranslateSingleColumnName(SqlTableColumn column, SqlTableColumn asterisk)
    {
      var output = context.Output;
      if (column == asterisk) {
        _ = output.Append(column.Name);
      }
      else {
        translator.TranslateIdentifier(output, column.Name);
      }
    }

    private void TranslateJoinedColumnNames(SqlTableColumnCollection targetColumns)
    {
      var output = context.Output;
      var first = true;
      foreach (var column in targetColumns) {
        if (first) {
          first = false;
        }
        else {
          _ = output.Append(", ");
        }
        translator.TranslateIdentifier(output, column.Name);
      }
    }

    #region Static helpers

    protected static SqlCast CastToLong(SqlExpression arg) => SqlDml.Cast(arg, SqlType.Int64);

    protected static SqlCast CastToDecimal(SqlExpression arg, short precision, short scale) =>
      SqlDml.Cast(arg, SqlType.Decimal, precision, scale);

    protected static SqlUserFunctionCall DatePartWeekDay(SqlExpression date) =>
      SqlDml.FunctionCall("DATEPART", SqlDml.Native(WeekdayPart), date);

    protected static SqlUserFunctionCall DateDiffDay(SqlExpression date1, SqlExpression date2) =>
      SqlDml.FunctionCall("DATEDIFF", SqlDml.Native(DayPart), date1, date2);

    protected static SqlUserFunctionCall DateDiffHour(SqlExpression date1, SqlExpression date2) =>
      SqlDml.FunctionCall("DATEDIFF", SqlDml.Native(HourPart), date1, date2);

    protected static SqlUserFunctionCall DateDiffMillisecond(SqlExpression date1, SqlExpression date2) =>
      SqlDml.FunctionCall("DATEDIFF", SqlDml.Native(MillisecondPart), date1, date2);

    protected static SqlUserFunctionCall DateAddYear(SqlExpression date, SqlExpression years) =>
      SqlDml.FunctionCall("DATEADD", SqlDml.Native(YearPart), years, date);

    protected static SqlUserFunctionCall DateAddMonth(SqlExpression date, SqlExpression months) =>
      SqlDml.FunctionCall("DATEADD", SqlDml.Native(MonthPart), months, date);

    protected static SqlUserFunctionCall DateAddDay(SqlExpression date, SqlExpression days) =>
      SqlDml.FunctionCall("DATEADD", SqlDml.Native(DayPart), days, date);

    protected static SqlUserFunctionCall DateAddHour(SqlExpression date, SqlExpression hours) =>
      SqlDml.FunctionCall("DATEADD", SqlDml.Native(HourPart), hours, date);

    protected static SqlUserFunctionCall DateAddMinute(SqlExpression date, SqlExpression minutes) =>
      SqlDml.FunctionCall("DATEADD", SqlDml.Native(MinutePart), minutes, date);

    protected static SqlUserFunctionCall DateAddSecond(SqlExpression date, SqlExpression seconds) =>
      SqlDml.FunctionCall("DATEADD", SqlDml.Native(SecondPart), seconds, date);

    protected static SqlUserFunctionCall DateAddMillisecond(SqlExpression date, SqlExpression milliseconds) =>
      SqlDml.FunctionCall("DATEADD", SqlDml.Native(MillisecondPart), milliseconds, date);

    protected static SqlUserFunctionCall TimeToString(SqlExpression time) =>
      SqlDml.FunctionCall("CONVERT", SqlDml.Native("NVARCHAR(16)"), time, SqlDml.Native("114"));

    protected static SqlUserFunctionCall DateToString(SqlExpression time) =>
      SqlDml.FunctionCall("CONVERT", SqlDml.Native("NVARCHAR(10)"), time, SqlDml.Native("23"));

    protected static SqlExpression DateTimeToDate(SqlExpression dateTime) =>
      SqlDml.Cast(dateTime, SqlType.Date);

    protected static SqlExpression DateToDateTime(SqlExpression date) =>
      SqlDml.Cast(date, SqlType.DateTime);

    protected static SqlExpression DateTimeToTime(SqlExpression dateTime) =>
      SqlDml.Cast(dateTime, SqlType.Time);

    protected static SqlExpression TimeToDateTime(SqlExpression time) =>
      SqlDml.Cast(time, SqlType.DateTime);

    protected static SqlUserFunctionCall DateTimeToStringIso(SqlExpression dateTime) =>
      SqlDml.FunctionCall("CONVERT", SqlDml.Native("NVARCHAR(19)"), dateTime, SqlDml.Native("126"));

    private static SqlExpression BankersRound(SqlExpression value, bool shouldCastToDecimal)
    {
      var res = SqlHelper.BankersRound(value);
      return shouldCastToDecimal ? SqlDml.Cast(res, SqlType.Decimal) : res;
    }

    private static SqlExpression BankersRound(SqlExpression value, SqlExpression digits, bool shouldCastToDecimal)
    {
      var scale = SqlDml.Power(10, digits);
      return BankersRound(value * scale, shouldCastToDecimal) / scale;
    }

    private static SqlExpression RegularRound(SqlExpression value, bool shouldCastToDecimal)
    {
      var res = SqlHelper.RegularRound(value);
      return shouldCastToDecimal ? SqlDml.Cast(res, SqlType.Decimal) : res;
    }

    private static SqlExpression RegularRound(SqlExpression value, SqlExpression digits, bool shouldCastToDecimal)
    {
      var scale = SqlDml.Power(10, digits);
      return RegularRound(value * scale, shouldCastToDecimal) / scale;
    }

    #endregion

    // Constructors

    /// <param name="driver">The driver.</param>
    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}