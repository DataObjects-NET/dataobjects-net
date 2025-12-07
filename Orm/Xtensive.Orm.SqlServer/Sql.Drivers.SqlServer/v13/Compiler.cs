// Copyright (C) 2009-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.07.07

using System;
using System.Linq;
using System.Collections.Generic;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.SqlServer.Resources;

namespace Xtensive.Sql.Drivers.SqlServer.v13
{
  internal class Compiler : SqlCompiler
  {
    #region Date parts
    protected const string NanosecondPart = "NS";
    protected const string MicrosecondPart = "MCS";
    protected const string MillisecondPart = "MS";
    protected const string SecondPart = "SECOND";
    protected const string MinutePart = "MINUTE";
    protected const string HourPart = "HOUR";
    protected const string DayPart = "DAY";
    protected const string MonthPart = "MONTH";
    protected const string YearPart = "YEAR";
    protected const string WeekdayPart = "WEEKDAY";
    protected const string OffsetPart = "TZoffset";
    #endregion

    protected const long NanosecondsPerDay = 86400000000000;
    protected const long NanosecondsPerHour = 3600000000000;
    protected const long NanosecondsPerMinute = 60000000000;
    protected const long NanosecondsPerSecond = 1000000000;
    protected const long NanosecondsPerMillisecond = 1000000;
    protected const long MillisecondsPerDay = 86400000;
    protected const long MillisecondsPerSecond = 1000L;
    protected const long NanosecondsPerMicrosecond = 1000;

    protected const string UtcTimeZone = "+00:00";
    protected const string ZeroTime = "'00:00:00.0000000'";
    protected const string SqlDateTypeName = "date";
    protected const string SqlDateTime2TypeName = "datetime2";

    protected static readonly SqlExpression DateFirst = SqlDml.Native("@@DATEFIRST");

    /// <inheritdoc/>
    protected override void VisitSelectLimitOffset(SqlSelect node)
    {
      // FETCH NEXT n ROWS ONLY does not work without OFFSET n ROWS
      // Provide zero offset if no offset was specified by user.

      if (!node.HasOffset && !node.HasLimit) {
        return; // Nothing to process.
      }

      AppendTranslated(node, SelectSection.Offset);

      if (node.HasOffset) {
        node.Offset.AcceptVisitor(this);
      }
      else {
        _ = context.Output.Append("0");
      }

      AppendSpaceIfNecessary();
      translator.Translate(context, node, SelectSection.OffsetEnd);

      if (node.HasLimit) {
        AppendTranslated(node, SelectSection.Limit);
        node.Limit.AcceptVisitor(this);
        AppendSpaceIfNecessary();
        translator.Translate(context, node, SelectSection.LimitEnd);
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
        case SqlFunctionType.IntervalToMilliseconds: {
          if (arguments[0] is SqlBinary binary
              && (binary.NodeType is SqlNodeType.DateTimeMinusDateTime or SqlNodeType.DateTimeOffsetMinusDateTimeOffset or SqlNodeType.TimeMinusTime)) {
            Visit(DateDiffBigMicrosecond(binary.Right, binary.Left) / CastToLong(1000));
          }
          else {
            Visit(CastToLong(arguments[0]) / NanosecondsPerMillisecond);
          }
          return;
        }
        case SqlFunctionType.IntervalToNanoseconds: {
          if (arguments[0] is SqlBinary binary) {
            if (binary.NodeType is SqlNodeType.DateTimeMinusDateTime or SqlNodeType.DateTimeOffsetMinusDateTimeOffset) {
              // we have to use time consuming algorithm here because
              // DATEDIFF_BIG can throw arithmetic overflow on nanoseconds
              // so we should handle it by this big formula
              Visit(CastToLong(DateTimeSubtractDateTimeExpensive(binary.Left, binary.Right)));
            }
            else if (binary.NodeType is SqlNodeType.TimeMinusTime) {
              //but for time it is OK
              Visit(DateDiffBigMicrosecond(binary.Right, binary.Left));
            }
            else {
              Visit(CastToLong(arguments[0]));
            }
          }
          else {
            Visit(CastToLong(arguments[0]));
          }
          return;
        }
        case SqlFunctionType.DateTimeOffsetAddMonths:
          Visit(DateAddMonth(arguments[0], arguments[1]));
          return;
        case SqlFunctionType.DateTimeOffsetAddYears:
          Visit(DateAddYear(arguments[0], arguments[1]));
          return;
        case SqlFunctionType.DateTimeOffsetTimeOfDay:
          DateTimeOffsetTimeOfDay(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetConstruct:
          Visit(ToDateTimeOffset(arguments[0], arguments[1]));
          return;
        case SqlFunctionType.DateTimeOffsetToLocalTime:
          DateTimeOffsetToLocalTime(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetToUtcTime:
          DateTimeOffsetToUtcTime(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeToDateTimeOffset:
          DateTimeToDateTimeOffset(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetToDateTime:
          DateTimeOffsetToDateTime(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateToDateTimeOffset:
          DateToDateTimeOffset(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.TimeToDateTimeOffset:
          TimeToDateTimeOffset(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetToTime:
          DateTimeOffsetToTime(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeOffsetToDate:
          DateTimeOffsetToDate(arguments[0]).AcceptVisitor(this);
          return;
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
            Visit(SqlDml.Round(arguments[0], SqlDml.Literal(0)));
            return;
          }
          else {
            base.Visit(node);
            return;
          }
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
            Visit(SqlDml.Substring(arguments[0], arguments[1], len));
            return;
          }
          else {
            base.Visit(node);
            return;
          }
        case SqlFunctionType.IntervalConstruct:
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
        default:
          base.Visit(node);
          return;
      }
    }

    /// <inheritdoc/>
    public override void Visit(SqlTrim node)
    {
      if (node.TrimCharacters != null && !node.TrimCharacters.All(_char => _char == ' ')) {
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
        case SqlNodeType.DateTimeOffsetPlusInterval:
          DateTimeAddInterval(node.Left, node.Right).AcceptVisitor(this);
          return;
        case SqlNodeType.DateTimeOffsetMinusDateTimeOffset:
          DateTimeOffsetSubtractDateTimeOffset(node.Left, node.Right).AcceptVisitor(this);
          return;
        case SqlNodeType.DateTimeOffsetMinusInterval:
          DateTimeAddInterval(node.Left, -node.Right).AcceptVisitor(this);
          return;
        default:
          base.Visit(node);
          return;
      }
    }

    /// <inheritdoc/>
    public override void Visit(SqlRound node)
    {
      var shouldCastToDecimal = node.Type == TypeCode.Decimal;
      var result = node.Mode switch {
        MidpointRounding.ToEven when node.Length is null        => BankersRound(node.Argument, shouldCastToDecimal),
        MidpointRounding.ToEven                                 => BankersRound(node.Argument, node.Length, shouldCastToDecimal),
        MidpointRounding.AwayFromZero when node.Length is null  => RegularRound(node.Argument, shouldCastToDecimal),
        MidpointRounding.AwayFromZero                           => RegularRound(node.Argument, node.Length, shouldCastToDecimal),
        _ => throw new ArgumentOutOfRangeException("node.Mode"),
      };
      result.AcceptVisitor(this);
    }

    /// <inheritdoc/>
    public override void Visit(SqlContainsTable node)
    {
      VisitFullTextExpressions(node, node.TargetTable.DataTable, node.TargetColumns, node.SearchCondition, node.TopNByRank, true);
    }

    /// <inheritdoc/>
    public override void Visit(SqlFreeTextTable node)
    {
      VisitFullTextExpressions(node, node.TargetTable.DataTable, node.TargetColumns, node.FreeText, node.TopNByRank, false);
    }

    private void VisitFullTextExpressions(SqlTable node,
      DataTable targetTable,
      SqlTableColumnCollection targetColumns,
      SqlExpression ftExpression,
      SqlExpression topNByRank,
      bool isContainsTable)
    {
      var output = context.Output;

      _ = output.AppendOpeningPunctuation(isContainsTable ? "CONTAINSTABLE(" : "FREETEXTTABLE(");
      AppendTranslated(targetTable);
      _ = output.Append(translator.ArgumentDelimiter);

      if (targetColumns.Count == 0)
        throw new InvalidOperationException(isContainsTable ? "No columns for CONTAINSTABLE function" : "No columns for FREETEXTTABLE function");
      if (targetColumns.Count == 1) {
        TranslateSingleColumnName(targetColumns[0], node.Asterisk);
      }
      else if (isContainsTable) {
        _ = output.Append('(');
        TranslateJoinedColumnNames(targetColumns);
        _ = output.Append(')');
      }
      else {
        TranslateJoinedColumnNames(targetColumns);
      }
        _ = output.Append(translator.ArgumentDelimiter);
      ftExpression.AcceptVisitor(this);
      if (topNByRank is not null) {
        _ = output.Append(translator.ArgumentDelimiter);
        topNByRank.AcceptVisitor(this);
      }
      _ = output.Append(") ");
    }

    /// <inheritdoc/>
    public override void Visit(SqlCreateIndex node, IndexColumn item)
    {
      base.Visit(node, item);

      if (item.TypeColumn is not null) {
        _ = context.Output.Append("TYPE COLUMN");
        translator.TranslateIdentifier(context.Output, item.TypeColumn.Name);
      }
      switch (item.Languages.Count) {
        case 0:
          break;
        case 1:
          var language = item.Languages[0].Name;
          if (!string.IsNullOrEmpty(language)) {
            _ = context.Output.Append($"LANGUAGE '{language}'");
          }
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
    protected virtual SqlExpression DateTimeTruncate(SqlExpression date) =>
      SqlDml.Cast(
        SqlDml.Cast(date, new SqlValueType(SqlDateTypeName)),
        new SqlValueType(SqlDateTime2TypeName));

    /// <summary>
    /// Creates expression that represents subtraction of two <see cref="DateTimeOffset"/> expressions.
    /// </summary>
    /// <param name="date1">First <see cref="DateTimeOffset"/> expressions.</param>
    /// <param name="date2">Second <see cref="DateTimeOffset"/> expressions.</param>
    /// <returns>Result expression.</returns>
    protected virtual SqlExpression DateTimeOffsetSubtractDateTimeOffset(SqlExpression date1, SqlExpression date2) =>
      DateTimeSubtractDateTime(date1, date2);

    /// <summary>
    /// Creates expression that represents subtraction of two <see cref="DateTime"/> expressions.
    /// </summary>
    /// <param name="date1">First <see cref="DateTime"/> expression.</param>
    /// <param name="date2">Second <see cref="DateTime"/> expression.</param>
    /// <returns>Result expression.</returns>
    /// <returns></returns>
    protected virtual SqlExpression DateTimeSubtractDateTime(SqlExpression date1, SqlExpression date2)
    {
      return CastToDecimal(DateDiffBigMicrosecond(date2, date1), 18, 0) * CastToLong(1000);
    }

    private SqlExpression DateTimeSubtractDateTimeExpensive(SqlExpression date1, SqlExpression date2)
    {
      return CastToDecimal(DateDiffBigDay(date2, date1), 18, 0) * NanosecondsPerDay
          + CastToDecimal(DateDiffBigMillisecond(DateAddDay(date2, DateDiffBigDay(date2, date1)), date1), 18, 0) * NanosecondsPerMillisecond;
    }

    /// <summary>
    /// Creates expression that represents addition <paramref name="interval"/> to the given <paramref name="date"/>.
    /// </summary>
    /// <param name="date">Date (datatime, datetimeoffset) expression.</param>
    /// <param name="interval">Interval expression to add.</param>
    /// <returns></returns>
    protected virtual SqlExpression DateTimeAddInterval(SqlExpression date, SqlExpression interval)
    {
      return DateAddNanosecond(
        DateAddMillisecond(
          DateAddDay(date, interval / NanosecondsPerDay),
          (interval / NanosecondsPerMillisecond) % (MillisecondsPerDay)),
        (interval / NanosecondsPerSecond) % NanosecondsPerDay / NanosecondsPerSecond);
    }

    /// <summary>
    /// Creates expression that represents construction of datetime value
    /// from arguments (year, month, day).
    /// </summary>
    /// <param name="arguments">Expressions representing year, month, and day.</param>
    /// <returns>Result expression.</returns>
    protected virtual SqlExpression ConstructDateTime(IReadOnlyList<SqlExpression> arguments) =>
      SqlDml.FunctionCall("DATETIME2FROMPARTS", arguments[0], arguments[1], arguments[2], 0, 0, 0, 0, 7);

    /// <summary>
    /// Creates expression that represents construction of date value
    /// from arguments (year, month, day).
    /// </summary>
    /// <param name="arguments">Expressions representing year, month, and day.</param>
    /// <returns>Result expression.</returns>
    protected virtual SqlExpression ConstructDate(IReadOnlyList<SqlExpression> arguments) =>
      SqlDml.FunctionCall("DATEFROMPARTS", arguments[0], arguments[1], arguments[2]);

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
        // argument[3] * 10000 operation is based on statement that millisaconds use 3 digits
        // default precision of time is 7, and if we use raw argument[3] value the result will be .0000xxx,
        // to prevent this and make fractions part valid .xxx0000 we multiply
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
      return SqlDml.FunctionCall("TIMEFROMPARTS", hour, minute, second, microsecond, 7);
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
        NanosecondsPerDay + CastToDecimal(DateDiffMillisecond(time2, time1), 18, 0) * NanosecondsPerMillisecond,
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
          _ = output.Append(translator.ColumnDelimiter);
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

    protected static SqlUserFunctionCall DateDiffBigDay(SqlExpression date1, SqlExpression date2) =>
      SqlDml.FunctionCall("DATEDIFF_BIG", SqlDml.Native(DayPart), date1, date2);

    protected static SqlUserFunctionCall DateDiffHour(SqlExpression date1, SqlExpression date2) =>
      SqlDml.FunctionCall("DATEDIFF", SqlDml.Native(HourPart), date1, date2);

    protected static SqlUserFunctionCall DateDiffMillisecond(SqlExpression date1, SqlExpression date2) =>
      SqlDml.FunctionCall("DATEDIFF", SqlDml.Native(MillisecondPart), date1, date2);

    private static SqlUserFunctionCall DateDiffBigMillisecond(SqlExpression date1, SqlExpression date2) =>
      SqlDml.FunctionCall("DATEDIFF_BIG", SqlDml.Native(MillisecondPart), date1, date2);

    private static SqlUserFunctionCall DateDiffBigNanosecond(SqlExpression date1, SqlExpression date2) =>
      SqlDml.FunctionCall("DATEDIFF_BIG", SqlDml.Native(NanosecondPart), date1, date2);

    private static SqlUserFunctionCall DateDiffBigMicrosecond(SqlExpression date1, SqlExpression date2) =>
      SqlDml.FunctionCall("DATEDIFF_BIG", SqlDml.Native(MicrosecondPart), date1, date2);

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

    protected static SqlUserFunctionCall DateAddNanosecond(SqlExpression date, SqlExpression nanoseconds) =>
      SqlDml.FunctionCall("DATEADD", SqlDml.Native(NanosecondPart), nanoseconds, date);

    protected static SqlUserFunctionCall TimeToString(SqlExpression time) =>
      SqlDml.FunctionCall("CONVERT", SqlDml.Native("NVARCHAR(16)"), time, SqlDml.Native("114"));

    protected static SqlUserFunctionCall DateToString(SqlExpression date) =>
      SqlDml.FunctionCall("CONVERT", SqlDml.Native("NVARCHAR(10)"), date, SqlDml.Native("23"));

    protected static SqlUserFunctionCall DateTimeToStringIso(SqlExpression dateTime) =>
      SqlDml.FunctionCall("CONVERT", SqlDml.Native("NVARCHAR(19)"), dateTime, SqlDml.Native("126"));

    protected static SqlExpression DateTimeToDate(SqlExpression dateTime) =>
      SqlDml.Cast(dateTime, SqlType.Date);

    protected static SqlExpression DateToDateTime(SqlExpression date) =>
      SqlDml.Cast(date, SqlType.DateTime);

    protected static SqlExpression DateTimeToTime(SqlExpression dateTime) =>
      SqlDml.Cast(dateTime, SqlType.Time);

    protected static SqlExpression TimeToDateTime(SqlExpression time) =>
      SqlDml.Cast(time, SqlType.DateTime);

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

    private static SqlExpression DateTimeOffsetTruncate(SqlExpression dateTimeOffset) =>
      SqlDml.Cast(
        SqlDml.Cast(dateTimeOffset, new SqlValueType(SqlDateTypeName)),
        new SqlValueType(SqlDateTime2TypeName));

    private static SqlExpression DateTimeOffsetPartOffset(SqlExpression dateTimeOffset) =>
      SqlDml.DateTimeOffsetMinusDateTimeOffset(
        DateTimeOffsetTruncateOffset(dateTimeOffset),
        Switchoffset(dateTimeOffset, UtcTimeZone));

    private static SqlExpression DateTimeOffsetTruncateOffset(SqlExpression dateTimeOffset) =>
      SqlDml.Cast(dateTimeOffset, SqlType.DateTime);

    private static SqlExpression DateTimeOffsetTimeOfDay(SqlExpression dateTimeOffset) =>
      DateDiffBigNanosecond(
        SqlDml.Native(ZeroTime),
        SqlDml.Cast(dateTimeOffset, new SqlValueType("time")));

    private static SqlExpression DateTimeOffsetToLocalDateTime(SqlExpression dateTimeOffset) =>
      SqlDml.Cast(DateTimeOffsetToLocalTime(dateTimeOffset), SqlType.DateTime);

    private static SqlUserFunctionCall ToDateTimeOffset(SqlExpression dateTime, SqlExpression offsetInMinutes) =>
      SqlDml.FunctionCall("TODATETIMEOFFSET", dateTime, offsetInMinutes);

    private static SqlExpression DateTimeOffsetToLocalTime(SqlExpression dateTimeOffset) =>
      Switchoffset(dateTimeOffset, DateTimeOffsetTimeZoneInMinutes(SqlDml.Native("SYSDATETIMEOFFSET()")));

    private static SqlExpression DateTimeOffsetToUtcTime(SqlExpression dateTimeOffset) =>
      Switchoffset(dateTimeOffset, UtcTimeZone);

    private static SqlExpression Switchoffset(SqlExpression dateTimeOffset, SqlExpression offset) =>
     SqlDml.FunctionCall("SWITCHOFFSET", dateTimeOffset, offset);

    private static SqlUserFunctionCall DateTimeOffsetTimeZoneInMinutes(SqlExpression date) =>
      SqlDml.FunctionCall("DATEPART", SqlDml.Native(OffsetPart), date);

    private static SqlExpression DateTimeOffsetToDateTime(SqlExpression dateTimeOffset) =>
      SqlDml.Cast(dateTimeOffset, SqlType.DateTime);

    private static SqlExpression DateToDateTimeOffset(SqlExpression date) =>
      DateTimeToDateTimeOffset(SqlDml.Cast(date, SqlType.DateTime));

    private static SqlExpression TimeToDateTimeOffset(SqlExpression time) =>
      DateTimeToDateTimeOffset(SqlDml.Cast(time, SqlType.DateTime));

    private static SqlExpression DateTimeToDateTimeOffset(SqlExpression dateTime) =>
      ToDateTimeOffset(dateTime, DateTimeOffsetTimeZoneInMinutes(SqlDml.Native("SYSDATETIMEOFFSET()")));

    private static SqlExpression DateTimeOffsetToTime(SqlExpression dateTimeOffset) =>
      SqlDml.Cast(dateTimeOffset, SqlType.Time);

    private static SqlExpression DateTimeOffsetToDate(SqlExpression dateTimeOffset) =>
      SqlDml.Cast(dateTimeOffset, SqlType.Date);

    #endregion

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}