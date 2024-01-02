// Copyright (C) 2011-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Malisa Ncube
// Created:    2011.02.25

using System;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Core;
using System.Collections.Generic;

namespace Xtensive.Sql.Drivers.MySql.v5_0
{
  internal class Compiler : SqlCompiler
  {
    protected const long NanosecondsPerDay = 86400000000000;
    protected const long NanosecondsPerHour = 3600000000000;
    protected const long NanosecondsPerMinute = 60000000000;
    protected const long NanosecondsPerSecond = 1000000000;
    protected const long NanosecondsPerMillisecond = 1000000;
    protected const long NanosecondsPerMicrosecond = 1000;
    protected const long MillisecondsPerDay = 86400000;

    /// <inheritdoc/>
    public override void Visit(SqlSelect node)
    {
      using (context.EnterScope(node)) {
        var comment = node.Comment;
        VisitCommentIfBefore(comment);
        AppendTranslatedEntry(node);
        VisitCommentIfWithin(comment);
        VisitSelectColumns(node);
        VisitSelectFrom(node);
        VisitSelectHints(node);
        VisitSelectWhere(node);
        VisitSelectGroupBy(node);
        VisitSelectOrderBy(node);
        VisitSelectLimitOffset(node);
        VisitSelectLock(node);
        AppendTranslatedExit(node);
        VisitCommentIfAfter(comment);
      }
    }

    /// <inheritdoc/>
    public override void Visit(SqlAlterTable node)
    {
      if (node.Action is SqlRenameColumn renameColumnAction)
        ((Translator) translator).Translate(context, renameColumnAction);
      else if (node.Action is SqlDropConstraint) {
        using (context.EnterScope(node)) {
          AppendTranslatedEntry(node);

          var action = node.Action as SqlDropConstraint;
          var constraint = action.Constraint as TableConstraint;
          AppendTranslated(node, AlterTableSection.DropConstraint);
          if (constraint is ForeignKey) {
            _ = context.Output.Append("FOREIGN KEY ");
            translator.TranslateIdentifier(context.Output, constraint.DbName);
          }
          else if (constraint is PrimaryKey)
            _ = context.Output.Append("PRIMARY KEY ");
          else {
            AppendTranslated(constraint, ConstraintSection.Entry);
          }

          AppendTranslated(node, AlterTableSection.DropBehavior);
          AppendTranslatedExit(node);
        }
      }
      else {
        base.Visit(node);
      }
    }

    /// <inheritdoc/>
    public override void Visit(SqlFreeTextTable node)
    {
      //See Readme.txt point 6.
      throw SqlHelper.NotSupported("FreeText");
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
    public override void Visit(SqlUnary node)
    {
      if (node.NodeType == SqlNodeType.BitNot) {
        Visit(BitNot(node.Operand));
        return;
      }
      base.Visit(node);
    }

    /// <inheritdoc/>
    /// //Thanks to Csaba Beer.
    public override void Visit(SqlQueryExpression node)
    {
      using (context.EnterScope(node)) {
        //bool needOpeningParenthesis = false;
        //bool needClosingParenthesis = false;
        AppendTranslatedEntry(node);
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
        AppendTranslatedExit(node);
      }
    }

    /// <inheritdoc/>
    public override void Visit(SqlFunctionCall node)
    {
      var arguments = node.Arguments;

      switch (node.FunctionType) {
        case SqlFunctionType.Truncate:
          SqlDml.FunctionCall("TRUNCATE", arguments[0], SqlDml.Literal(0)).AcceptVisitor(this);
          return;
        case SqlFunctionType.Concat:
          Visit(SqlDml.Concat(arguments.ToArray(node.Arguments.Count)));
          return;
        case SqlFunctionType.CharLength:
          SqlDml.FunctionCall(translator.TranslateToString(SqlFunctionType.CharLength), node.Arguments[0]).AcceptVisitor(this);
          //          SqlDml.CharLength(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.PadLeft:
        case SqlFunctionType.PadRight:
          SqlHelper.GenericPad(node).AcceptVisitor(this);
          return;
        case SqlFunctionType.Rand:
          SqlDml.FunctionCall(translator.TranslateToString(SqlFunctionType.Rand)).AcceptVisitor(this);
          return;
        case SqlFunctionType.Square:
          SqlDml.Power(arguments[0], 2).AcceptVisitor(this);
          return;
        case SqlFunctionType.IntervalToMilliseconds:
          Visit(CastToLong(arguments[0]) / NanosecondsPerMillisecond);
          return;
        case SqlFunctionType.IntervalConstruct:
        case SqlFunctionType.IntervalToNanoseconds:
          Visit(CastToLong(arguments[0]));
          return;
        case SqlFunctionType.DateTimeAddMonths:
          Visit(DateTimeAddMonth(arguments[0], arguments[1]));
          return;
        case SqlFunctionType.DateTimeAddYears:
          Visit(DateTimeAddYear(arguments[0], arguments[1]));
          return;
        case SqlFunctionType.DateTimeConstruct:
          ConstructDateTime(arguments).AcceptVisitor(this);
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
        case SqlFunctionType.DateConstruct:
          ConstructDate(arguments).AcceptVisitor(this);          
          return;
        case SqlFunctionType.TimeAddHours:
          Visit(SqlDml.FunctionCall("TIME", SqlDml.FunctionCall(
            "DATE_ADD",
            SqlDml.Literal(new DateTime(2001, 1, 1)),
            SqlDml.RawConcat(
              SqlDml.RawConcat(SqlDml.Native("INTERVAL "), SqlDml.FunctionCall("TIME_TO_SEC", arguments[0]) + arguments[1] * 3600),
              SqlDml.Native("SECOND")))));
          return;
        case SqlFunctionType.TimeAddMinutes:
          Visit(SqlDml.FunctionCall("TIME",
            SqlDml.FunctionCall("DATE_ADD",
              SqlDml.Literal(new DateTime(2001, 1, 1)),
              SqlDml.RawConcat(
                SqlDml.RawConcat(SqlDml.Native("INTERVAL "), SqlDml.FunctionCall("TIME_TO_SEC", arguments[0]) + arguments[1] * 60),
                SqlDml.Native("SECOND")))));
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
          DateTimeToDate(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateToDateTime:
          DateToDateTime(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeToTime:
          DateTimeToTime(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.TimeToDateTime:
          TimeToDateTime(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeToStringIso:
          Visit(DateTimeToStringIso(arguments[0]));
          return;
      }

      base.Visit(node);
    }

    public override void Visit(SqlPlaceholder node)
    {
      if (node.Id is Orm.Providers.ParameterBinding qpb
        && qpb.TypeMapping?.Type == typeof(TimeOnly)) {
        _ = context.Output.Append("TIME(");
        base.Visit(node);
        _ = context.Output.Append(")");
      }
      else {
        base.Visit(node);
      }
    }

    /// <inheritdoc/>
    protected override void VisitSelectLimitOffset(SqlSelect node)
    {
      if (node.Limit is not null) {
        translator.SelectLimit(context, node);
        node.Limit.AcceptVisitor(this);
      }
      if (node.Offset is not null) {
        if (node.Limit is null) {
          translator.SelectLimit(context, node);
          _ = context.Output.Append(" 18446744073709551615 "); // magic number from http://dev.mysql.com/doc/refman/5.0/en/select.html
        }
        translator.SelectOffset(context, node);
        node.Offset.AcceptVisitor(this);
      }
    }

    /// <inheritdoc/>
    public override void Visit(SqlExtract node)
    {
      if (node.DateTimePart == SqlDateTimePart.DayOfWeek || node.DateTimePart == SqlDateTimePart.DayOfYear) {
        Visit(SqlDml.FunctionCall(node.DateTimePart.ToString(), node.Operand));
        return;
      }
      if (node.DatePart == SqlDatePart.DayOfWeek || node.DatePart == SqlDatePart.DayOfYear) {
        Visit(SqlDml.FunctionCall(node.DatePart.ToString(), node.Operand));
        return;
      }

      base.Visit(node);
    }

    protected virtual SqlExpression ConstructDateTime(IReadOnlyList<SqlExpression> arguments)
    {
      return DateTimeAddDay(
        DateTimeAddMonth(
          DateTimeAddYear(
            SqlDml.Literal(new DateTime(2001, 1, 1)),
            arguments[0] - 2001),
          arguments[1] - 1),
        arguments[2] - 1);
    }

    protected virtual SqlExpression DateTimeSubtractDateTime(SqlExpression date1, SqlExpression date2)
    {
      return (CastToDecimal(DateDiffDay(date1, date2), 18, 0) * NanosecondsPerDay)
        +
        (CastToDecimal(DateTimeDiffMicrosecond(DateTimeAddDay(date2, DateDiffDay(date1, date2)), date1), 18, 0) * NanosecondsPerMicrosecond);
    }

    protected virtual SqlExpression DateTimeAddInterval(SqlExpression date, SqlExpression interval)
    {
      return DateTimeAddMicrosecond(
        DateTimeAddDay(date, ((interval - (interval % NanosecondsPerDay)) + ((interval % NanosecondsPerDay) > (NanosecondsPerDay / 2) ? 0 : 1)) / NanosecondsPerDay),
        (interval / NanosecondsPerMillisecond * NanosecondsPerMicrosecond) % (MillisecondsPerDay * NanosecondsPerMicrosecond));
    }

    protected virtual SqlExpression ConstructDate(IReadOnlyList<SqlExpression> arguments)
    {
      return DateAddDay(
        DateAddMonth(
          DateAddYear(
            SqlDml.Literal(new DateOnly(2001, 1, 1)),
            arguments[0] - 2001),
          arguments[1] - 1),
        arguments[2] - 1);
    }

    protected virtual SqlExpression TimeToNanoseconds(SqlExpression time)
    {
      var nPerHour = SqlDml.Extract(SqlTimePart.Hour, time) * NanosecondsPerHour;
      var nPerMinute = SqlDml.Extract(SqlTimePart.Minute, time) * NanosecondsPerMinute;
      var nPerSecond = SqlDml.Extract(SqlTimePart.Second, time) * NanosecondsPerSecond;
      var nPerMillisecond = SqlDml.Extract(SqlTimePart.Millisecond, time) * NanosecondsPerMillisecond;

      return nPerHour + nPerMinute + nPerSecond + nPerMillisecond;
    }

    protected virtual SqlExpression TimeSubtractTime(SqlExpression time1, SqlExpression time2) =>
      SqlDml.Modulo(
        NanosecondsPerDay + CastToDecimal(SqlDml.FunctionCall("TIME_TO_SEC", time1) - SqlDml.FunctionCall("TIME_TO_SEC", time2), 18, 0) * NanosecondsPerSecond,
        NanosecondsPerDay);

    protected virtual SqlExpression TimeAddInterval(SqlExpression time, SqlExpression interval) =>
      SqlDml.FunctionCall("TIME",
        SqlDml.FunctionCall(
          "DATE_ADD",
          SqlDml.Literal(new DateTime(2001, 1, 1)),
            SqlDml.RawConcat(
              SqlDml.RawConcat(SqlDml.Native("INTERVAL "),
                SqlDml.FunctionCall("TIME_TO_SEC", time) + interval / NanosecondsPerSecond),
              SqlDml.Native("SECOND"))));

    #region Static helpers

    protected static SqlCast CastToLong(SqlExpression arg) => SqlDml.Cast(arg, SqlType.Int64);

    protected static SqlCast CastToDecimal(SqlExpression arg, short precision, short scale) =>
      SqlDml.Cast(arg, SqlType.Decimal, precision, scale);

    protected static SqlUserFunctionCall DateDiffDay(SqlExpression date1, SqlExpression date2) =>
      SqlDml.FunctionCall("DATEDIFF", date1, date2);

    protected static SqlUserFunctionCall DateTimeDiffMicrosecond(SqlExpression datetime1, SqlExpression datetime2) =>
      SqlDml.FunctionCall("TIMESTAMPDIFF", SqlDml.Native("MICROSECOND"), datetime1, datetime2);

    protected static SqlUserFunctionCall DateTimeDiffSecond(SqlExpression datetime1, SqlExpression datetime2) =>
      SqlDml.FunctionCall("TIMESTAMPDIFF", SqlDml.Native("SECOND"), datetime1, datetime2);

    protected static SqlUserFunctionCall DateTimeAddYear(SqlExpression datetime, SqlExpression years) =>
      SqlDml.FunctionCall("TIMESTAMPADD", SqlDml.Native("YEAR"), years, datetime);

    protected static SqlUserFunctionCall DateTimeAddMonth(SqlExpression datetime, SqlExpression months) =>
      SqlDml.FunctionCall("TIMESTAMPADD", SqlDml.Native("MONTH"), months, datetime);

    protected static SqlUserFunctionCall DateTimeAddDay(SqlExpression datetime, SqlExpression days) =>
      SqlDml.FunctionCall("TIMESTAMPADD", SqlDml.Native("DAY"), days, datetime);

    protected static SqlUserFunctionCall DateTimeAddHour(SqlExpression datetime, SqlExpression days) =>
      SqlDml.FunctionCall("TIMESTAMPADD", SqlDml.Native("HOUR"), days, datetime);

    protected static SqlUserFunctionCall DateTimeAddMicrosecond(SqlExpression datetime, SqlExpression microseconds) =>
      SqlDml.FunctionCall("TIMESTAMPADD", SqlDml.Native("MICROSECOND"), microseconds, datetime);

    protected static SqlUserFunctionCall DateAddYear(SqlExpression date, SqlExpression years) =>
      SqlDml.FunctionCall("DATE_ADD", date, SqlDml.RawConcat(SqlDml.Native("INTERVAL "), SqlDml.RawConcat(years, SqlDml.Native("YEAR"))));

    protected static SqlUserFunctionCall DateAddMonth(SqlExpression date, SqlExpression months) =>
      SqlDml.FunctionCall("DATE_ADD", date, SqlDml.RawConcat(SqlDml.Native("INTERVAL "), SqlDml.RawConcat(months, SqlDml.Native("MONTH"))));

    protected static SqlUserFunctionCall DateAddDay(SqlExpression date, SqlExpression days) =>
      SqlDml.FunctionCall("DATE_ADD", date, SqlDml.RawConcat(SqlDml.Native("INTERVAL "), SqlDml.RawConcat(days, SqlDml.Native("DAY"))));

    protected static SqlUserFunctionCall TimeAddHour(SqlExpression time, SqlExpression hours) =>
      SqlDml.FunctionCall("DATE_ADD", time, SqlDml.RawConcat(SqlDml.Native("INTERVAL "), SqlDml.RawConcat(hours, SqlDml.Native("HOUR"))));

    protected static SqlUserFunctionCall TimeAddMinute(SqlExpression time, SqlExpression minutes) =>
      SqlDml.FunctionCall("DATE_ADD", time, SqlDml.RawConcat(SqlDml.Native("INTERVAL "), SqlDml.RawConcat(minutes, SqlDml.Native("MINUTE"))));

    protected static SqlUserFunctionCall TimeAddSecond(SqlExpression time, SqlExpression seconds) =>
      SqlDml.FunctionCall("DATE_ADD", time, SqlDml.RawConcat(SqlDml.Native("INTERVAL "), SqlDml.RawConcat(seconds, SqlDml.Native("SECOND"))));

    protected static SqlUserFunctionCall TimeAddMillisecond(SqlExpression time, SqlExpression millisecond) =>
      SqlDml.FunctionCall("DATE_ADD", time, SqlDml.RawConcat(SqlDml.Native("INTERVAL "), SqlDml.RawConcat(millisecond * 1000, SqlDml.Native("MICROSECOND"))));

    protected static SqlUserFunctionCall DateToString(SqlExpression dateTime) =>
      SqlDml.FunctionCall("DATE_FORMAT", dateTime, "%Y-%m-%d");

    protected static SqlUserFunctionCall TimeToString(SqlExpression dateTime) =>
      SqlDml.FunctionCall("DATE_FORMAT", dateTime, "%H:%i:%s.%f0");

    private static SqlExpression DateTimeToDate(SqlExpression dateTime) =>
      SqlDml.Cast(dateTime, SqlType.Date);

    private static SqlExpression DateToDateTime(SqlExpression date) =>
      SqlDml.Cast(date, SqlType.DateTime);

    private static SqlExpression DateTimeToTime(SqlExpression dateTime) =>
      SqlDml.Cast(dateTime, SqlType.Time);

    // can't convert via cast, because mysql shots to its head and creates
    // value that it can't read later. This mimics conversion that occurs
    // in newer versions (5.6+) and use current date as a source of year,
    // month and day values :-)
    private static SqlExpression TimeToDateTime(SqlExpression time) =>
      SqlDml.FunctionCall("DATE_ADD",
        SqlDml.Literal(DateTime.Now.Date),
        SqlDml.RawConcat(
          SqlDml.RawConcat(SqlDml.Native("INTERVAL "),
          SqlDml.FunctionCall("TIME_TO_SEC", time)),
          SqlDml.Native("SECOND")));

    protected static SqlUserFunctionCall DateTimeToStringIso(SqlExpression dateTime) =>
      SqlDml.FunctionCall("DATE_FORMAT", dateTime, "%Y-%m-%dT%T");

    protected static SqlUserFunctionCall BitNot(SqlExpression operand) =>
      SqlDml.FunctionCall(
        "CAST",
        SqlDml.RawConcat(
          SqlDml.Native("~"),
          SqlDml.RawConcat(
            operand,
            SqlDml.Native("AS SIGNED"))));


    #endregion

    // Constructors

    protected internal Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}