// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.11

using System;
using System.Linq;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Model;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.SqlServer.v2005
{
  internal class Compiler : SqlCompiler
  {
    private static readonly int MillisecondsPerDay = (int) TimeSpan.FromDays(1).TotalMilliseconds;
    private static readonly SqlExpression DateFirst = SqlDml.FunctionCall(Translator.DateFirst);

    /// <inheritdoc/>
    public override void Visit(SqlAlterTable node)
    {
      if (!(node.Action is SqlRenameAction)) {
        base.Visit(node);
        return;
      }
      var action = (SqlRenameAction) node.Action;
      var column = action.Node as TableColumn;
      if (column != null)
        context.AppendText(translator.Translate(context, column, action));
      else
        context.AppendText(translator.Translate(context, node.Table, action));
    }

    /// <inheritdoc/>
    public override void Visit(SqlFunctionCall node)
    {
      switch (node.FunctionType) {
      case SqlFunctionType.Round:
        // Round should always be called with 2 arguments
        if (node.Arguments.Count == 1) {
          Visit(SqlDml.FunctionCall(
            translator.Translate(SqlFunctionType.Round),
            node.Arguments[0],
            SqlDml.Literal(0)));
          return;
        }
        break;
      case SqlFunctionType.Truncate:
        // Truncate is implemented as round(arg, 0, 1) call in MSSQL.
        // It's stupid, isn't it?
        Visit(SqlDml.FunctionCall(
          translator.Translate(SqlFunctionType.Round),
          node.Arguments[0],
          SqlDml.Literal(0),
          SqlDml.Literal(1)));
        return;
      case SqlFunctionType.Substring:
        if (node.Arguments.Count == 2) {
          node = SqlDml.Substring(node.Arguments[0], node.Arguments[1]);
          SqlExpression len = SqlDml.Length(node.Arguments[0]);
          node.Arguments.Add(len);
          base.Visit(node);
          return;
        }
        break;
      case SqlFunctionType.IntervalConstruct:
      case SqlFunctionType.IntervalToMilliseconds:
        Visit(CastToLong(node.Arguments[0]));
        return;
      case SqlFunctionType.IntervalExtract:
        IntervalExtract(node.Arguments[0], node.Arguments[1]);
        return;
      case SqlFunctionType.IntervalDuration:
        Visit(SqlDml.Abs(node.Arguments[0]));
        return;
      case SqlFunctionType.DateTimeAddMonths:
        Visit(DateAddMonth(node.Arguments[0], node.Arguments[1]));
        return;
      case SqlFunctionType.DateTimeAddYears:
        Visit(DateAddYear(node.Arguments[0], node.Arguments[1]));
        return;
      case SqlFunctionType.DateTimeAddInterval:
        DateTimeAddInterval(node.Arguments[0], node.Arguments[1]);
        return;
      case SqlFunctionType.DateTimeSubtractInterval:
        DateTimeAddInterval(node.Arguments[0], -node.Arguments[1]);
        return;
      case SqlFunctionType.DateTimeSubtractDateTime:
        DateTimeSubtractDateTime(node.Arguments[0], node.Arguments[1]);
        return;
      case SqlFunctionType.DateTimeTruncate:
        DateTimeTruncate(node.Arguments[0]);
        return;
      case SqlFunctionType.DateTimeConstruct:
        Visit(DateAddDay(DateAddMonth(DateAddYear(SqlDml.Literal(new DateTime(2001, 1, 1)),
          node.Arguments[0] - 2001),
          node.Arguments[1] - 1),
          node.Arguments[2] - 1));
        return;

      case SqlFunctionType.Extract:
        if (((SqlLiteral<SqlDateTimePart>)node.Arguments[0]).Value == SqlDateTimePart.DayOfWeek)
        {
          Visit((DatePartWeekDay(node.Arguments[1]) + DateFirst + 6) % 7);
          return;
        }
        break;
      }

      base.Visit(node);
    }

    public override void Visit(SqlTrim node)
    {
      if (node.TrimCharacters!=null && !node.TrimCharacters.All(_char => _char==' '))
        throw new NotSupportedException("MSSQL supports trimming of space characters only.");
      
      using (context.EnterNode(node)) {
        context.AppendText(translator.Translate(context, node, TrimSection.Entry));
        node.Expression.AcceptVisitor(this);
        context.AppendText(translator.Translate(context, node, TrimSection.Exit));
      }
    }

    private void DateTimeTruncate(SqlExpression date)
    {
      Visit(DateAddMillisecond(DateAddSecond(DateAddMinute(DateAddHour(date,
        -SqlDml.Extract(SqlDateTimePart.Hour, date)),
        -SqlDml.Extract(SqlDateTimePart.Minute, date)),
        -SqlDml.Extract(SqlDateTimePart.Second, date)),
        -SqlDml.Extract(SqlDateTimePart.Millisecond, date)));
    }

    private void DateTimeSubtractDateTime(SqlExpression date1, SqlExpression date2)
    {
      Visit(
        CastToLong(DateDiffDay(date2, date1)) * MillisecondsPerDay
          + DateDiffMillisecond(DateAddDay(date2, DateDiffDay(date2, date1)), date1)
        );
    }

    private void DateTimeAddInterval(SqlExpression date, SqlExpression interval)
    {
      Visit(
        DateAddMillisecond(DateAddDay(date, interval / MillisecondsPerDay), interval % MillisecondsPerDay)
        );
    }

    private void IntervalExtract(SqlExpression partExpression, SqlExpression source)
    {
      var part = ((SqlLiteral<SqlIntervalPart>)partExpression).Value;

      switch (part)
      {
      case SqlIntervalPart.Day:
        Visit(CastToLong(source / MillisecondsPerDay));
        return;
      case SqlIntervalPart.Hour:
        Visit(CastToLong(source / (60 * 60 * 1000)) % 24);
        return;
      case SqlIntervalPart.Minute:
        Visit(CastToLong(source / (60 * 1000)) % 60);
        return;
      case SqlIntervalPart.Second:
        Visit(CastToLong(source / 1000) % 60);
        return;
      case SqlIntervalPart.Millisecond:
        Visit(source % 1000);
        return;
      }
    }

    #region Static helpers

    private static SqlCast CastToLong(SqlExpression arg)
    {
      return SqlDml.Cast(arg, SqlType.Int64);
    }

    private static SqlUserFunctionCall DatePartWeekDay(SqlExpression date)
    {
      return SqlDml.FunctionCall(Translator.DatePartWeekDay, date);
    }

    private static SqlUserFunctionCall DateDiffDay(SqlExpression date1, SqlExpression date2)
    {
      return SqlDml.FunctionCall(Translator.DateDiffDay, date1, date2);
    }

    private static SqlUserFunctionCall DateDiffMillisecond(SqlExpression date1, SqlExpression date2)
    {
      return SqlDml.FunctionCall(Translator.DateDiffMillisecond, date1, date2);
    }

    private static SqlUserFunctionCall DateAddYear(SqlExpression date, SqlExpression years)
    {
      return SqlDml.FunctionCall(Translator.DateAddYear, years, date);
    }

    private static SqlUserFunctionCall DateAddMonth(SqlExpression date, SqlExpression months)
    {
      return SqlDml.FunctionCall(Translator.DateAddMonth, months, date);
    }

    private static SqlUserFunctionCall DateAddDay(SqlExpression date, SqlExpression days)
    {
      return SqlDml.FunctionCall(Translator.DateAddDay, days, date);
    }

    private static SqlUserFunctionCall DateAddHour(SqlExpression date, SqlExpression hours)
    {
      return SqlDml.FunctionCall(Translator.DateAddHour, hours, date);
    }

    private static SqlUserFunctionCall DateAddMinute(SqlExpression date, SqlExpression minutes)
    {
      return SqlDml.FunctionCall(Translator.DateAddMinute, minutes, date);
    }

    private static SqlUserFunctionCall DateAddSecond(SqlExpression date, SqlExpression seconds)
    {
      return SqlDml.FunctionCall(Translator.DateAddSecond, seconds, date);
    }

    private static SqlUserFunctionCall DateAddMillisecond(SqlExpression date, SqlExpression milliseconds)
    {
      return SqlDml.FunctionCall(Translator.DateAddMillisecond, milliseconds, date);
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