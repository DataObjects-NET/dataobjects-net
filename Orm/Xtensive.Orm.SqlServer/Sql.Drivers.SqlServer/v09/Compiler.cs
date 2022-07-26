// Copyright (C) 2009-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.03.11

using System;
using System.Linq;
using System.Text;
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

    protected static readonly long NanosecondsPerDay = TimeSpan.FromDays(1).Ticks*100;
    protected static readonly long NanosecondsPerSecond = 1000000000;
    protected static readonly long NanosecondsPerMillisecond = 1000000;
    protected static readonly long MillisecondsPerDay = (long) TimeSpan.FromDays(1).TotalMilliseconds;
    protected static readonly long MillisecondsPerSecond = 1000L;
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
        case SqlFunctionType.DateOnlyAddDays:
          Visit(DateAddDay(arguments[0], arguments[1]));
          return;
        case SqlFunctionType.TimeOnlyAddHours:
          Visit(DateAddHour(arguments[0], arguments[1]));
          return;
        case SqlFunctionType.TimeOnlyAddMinutes:
          Visit(DateAddMinute(arguments[0], arguments[1]));
          return;
        case SqlFunctionType.DateTimeTruncate:
          DateTimeTruncate(arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeConstruct:
          Visit(DateAddDay(DateAddMonth(DateAddYear(SqlDml.Literal(new DateTime(2001, 1, 1)),
            arguments[0] - 2001),
            arguments[1] - 1),
            arguments[2] - 1));
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