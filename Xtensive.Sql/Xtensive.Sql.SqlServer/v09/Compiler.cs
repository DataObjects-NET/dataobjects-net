// Copyright (C) 2003-2010 Xtensive LLC.
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
using Xtensive.Sql.SqlServer.Resources;

namespace Xtensive.Sql.SqlServer.v09
{
  internal class Compiler : SqlCompiler
  {
    protected static readonly long NanosecondsPerDay = TimeSpan.FromDays(1).Ticks*100;
    protected static readonly long NanosecondsPerSecond = 1000000000;
    protected static readonly long NanosecondsPerMillisecond = 1000000;
    protected static readonly long MillisecondsPerDay = (long) TimeSpan.FromDays(1).TotalMilliseconds;
    protected static readonly long MillisecondsPerSecond = 1000L;
    protected static readonly SqlExpression DateFirst = SqlDml.Native("@@DATEFIRST");
    
    public override void Visit(SqlSelect node)
    {
      using (context.EnterScope(node)) {
        context.Output.AppendText(translator.Translate(context, node, SelectSection.Entry));
        VisitSelectLimitOffset(node);
        VisitSelectHints(node);
        VisitSelectColumns(node);
        VisitSelectFrom(node);
        VisitSelectWhere(node);
        VisitSelectGroupBy(node);
        VisitSelectOrderBy(node);
        VisitSelectLock(node);
        context.Output.AppendText(translator.Translate(context, node, SelectSection.Exit));
      }
    }

    public override void VisitSelectLock(SqlSelect node)
    {
      return;
    }

    /// <inheritdoc/>
    public override void Visit(SqlAlterTable node)
    {
      var renameColumnAction = node.Action as SqlRenameColumn;
      if (renameColumnAction!=null)
        context.Output.AppendText(((Translator) translator).Translate(context, renameColumnAction));
      else
        base.Visit(node);
    }

    /// <inheritdoc/>
    public override void Visit(SqlFunctionCall node)
    {
      switch (node.FunctionType) {
      case SqlFunctionType.CharLength:
        (SqlDml.FunctionCall("DATALENGTH", node.Arguments) / 2).AcceptVisitor(this);
        return;
      case SqlFunctionType.PadLeft:
      case SqlFunctionType.PadRight:
        GenericPad(node).AcceptVisitor(this);
        return;
      case SqlFunctionType.Round:
        // Round should always be called with 2 arguments
        if (node.Arguments.Count==1) {
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
        if (node.Arguments.Count==2) {
          node = SqlDml.Substring(node.Arguments[0], node.Arguments[1]);
          SqlExpression len = SqlDml.CharLength(node.Arguments[0]);
          node.Arguments.Add(len);
          Visit(node);
          return;
        }
        break;
      case SqlFunctionType.IntervalToMilliseconds:
          Visit(CastToLong(node.Arguments[0]) / NanosecondsPerMillisecond);
          return;
      case SqlFunctionType.IntervalConstruct:
      case SqlFunctionType.IntervalToNanoseconds:
        Visit(CastToLong(node.Arguments[0]));
        return;
      case SqlFunctionType.DateTimeAddMonths:
        Visit(DateAddMonth(node.Arguments[0], node.Arguments[1]));
        return;
      case SqlFunctionType.DateTimeAddYears:
        Visit(DateAddYear(node.Arguments[0], node.Arguments[1]));
        return;
      case SqlFunctionType.DateTimeTruncate:
        DateTimeTruncate(node.Arguments[0]).AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeConstruct:
        Visit(DateAddDay(DateAddMonth(DateAddYear(SqlDml.Literal(new DateTime(2001, 1, 1)),
          node.Arguments[0] - 2001),
          node.Arguments[1] - 1),
          node.Arguments[2] - 1));
        return;
      }

      base.Visit(node);
    }

    public override void Visit(SqlTrim node)
    {
      if (node.TrimCharacters!=null && !node.TrimCharacters.All(_char => _char==' '))
        throw new NotSupportedException(Strings.ExSqlServerSupportsTrimmingOfSpaceCharactersOnly);
      
      using (context.EnterScope(node)) {
        context.Output.AppendText(translator.Translate(context, node, TrimSection.Entry));
        node.Expression.AcceptVisitor(this);
        context.Output.AppendText(translator.Translate(context, node, TrimSection.Exit));
      }
    }
    
    public override void Visit(SqlExtract node)
    {
      if (node.DateTimePart==SqlDateTimePart.DayOfWeek) {
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

    protected virtual SqlExpression DateTimeTruncate(SqlExpression date)
    {
      return DateAddMillisecond(DateAddSecond(DateAddMinute(DateAddHour(date,
        -SqlDml.Extract(SqlDateTimePart.Hour, date)),
        -SqlDml.Extract(SqlDateTimePart.Minute, date)),
        -SqlDml.Extract(SqlDateTimePart.Second, date)),
        -SqlDml.Extract(SqlDateTimePart.Millisecond, date));
    }
    
    protected virtual SqlExpression DateTimeSubtractDateTime(SqlExpression date1, SqlExpression date2)
    {
      return CastToLong(DateDiffDay(date2, date1)) * NanosecondsPerDay
          + CastToLong(DateDiffMillisecond(DateAddDay(date2, DateDiffDay(date2, date1)), date1)) * NanosecondsPerMillisecond;
    }

    protected virtual SqlExpression DateTimeAddInterval(SqlExpression date, SqlExpression interval)
    {
        return DateAddMillisecond(
          DateAddDay(date, interval / NanosecondsPerDay),
          (interval/NanosecondsPerMillisecond) % (MillisecondsPerDay));
    }

    private SqlExpression GenericPad(SqlFunctionCall node)
    {
      var operand = node.Arguments[0];
      var actualLength = SqlDml.CharLength(operand);
      var requiredLength = node.Arguments[1];
      var paddingExpression = node.Arguments.Count > 2
        ? SqlDml.FunctionCall("REPLICATE", node.Arguments[2], requiredLength - actualLength)
        : SqlDml.FunctionCall("SPACE", requiredLength - actualLength);
      SqlExpression resultExpression;
      switch (node.FunctionType) {
      case SqlFunctionType.PadLeft:
        resultExpression = paddingExpression + operand;
        break;
      case SqlFunctionType.PadRight:
        resultExpression = operand + paddingExpression;
        break;
      default:
        throw new InvalidOperationException();
      }
      var result = SqlDml.Case();
      result.Add(actualLength < requiredLength, resultExpression);
      result.Else = operand;
      return result;
    }

    public override void Visit(SqlFreeTextTable node)
    {
      string columns;
      if (node.TargetColumns.Count == 1)
        columns = node.TargetColumns[0].Name;
      else
        columns = string.Join(", ", node.TargetColumns.Select(c => c.Name).ToArray());
      context.Output.AppendText(string.Format("FREETEXTTABLE({0}, {1}, ", translator.Translate(node.TargetTable.DataTable), columns));
      node.FreeText.AcceptVisitor(this);
      context.Output.AppendText(") ");
    }
    
  public override void Visit(SqlCreateIndex node, IndexColumn item)
  {
    base.Visit(node, item);
    switch (item.Languages.Count) {
      case 0:
      break;
      case 1:
      if (!string.IsNullOrEmpty(item.Languages[0].Name)) 
        context.Output.AppendText(string.Format("LANGUAGE '{0}'", item.Languages[0].Name));
      break;
      default:
        throw new InvalidOperationException(string.Format(
          Strings.ExMultipleLanguagesNotSupportedForFulltextColumnXOfIndexY, item.Name, item.Index.Name));
    }
  }

    #region Static helpers

    private static SqlCast CastToLong(SqlExpression arg)
    {
      return SqlDml.Cast(arg, SqlType.Int64);
    }

    private static SqlUserFunctionCall DatePartWeekDay(SqlExpression date)
    {
      return SqlDml.FunctionCall("DATEPART", SqlDml.Native("WEEKDAY"), date);
    }

    protected static SqlUserFunctionCall DateDiffDay(SqlExpression date1, SqlExpression date2)
    {
      return SqlDml.FunctionCall("DATEDIFF", SqlDml.Native("DAY"), date1, date2);
    }

    protected static SqlUserFunctionCall DateDiffMillisecond(SqlExpression date1, SqlExpression date2)
    {
      return SqlDml.FunctionCall("DATEDIFF", SqlDml.Native("MS"), date1, date2);
    }

    private static SqlUserFunctionCall DateAddYear(SqlExpression date, SqlExpression years)
    {
      return SqlDml.FunctionCall("DATEADD", SqlDml.Native("YEAR"),years, date);
    }

    private static SqlUserFunctionCall DateAddMonth(SqlExpression date, SqlExpression months)
    {
      return SqlDml.FunctionCall("DATEADD", SqlDml.Native("MONTH"), months, date);
    }

    protected static SqlUserFunctionCall DateAddDay(SqlExpression date, SqlExpression days)
    {
      return SqlDml.FunctionCall("DATEADD", SqlDml.Native("DAY"), days, date);
    }

    private static SqlUserFunctionCall DateAddHour(SqlExpression date, SqlExpression hours)
    {
      return SqlDml.FunctionCall("DATEADD", SqlDml.Native("HOUR"), hours, date);
    }

    private static SqlUserFunctionCall DateAddMinute(SqlExpression date, SqlExpression minutes)
    {
      return SqlDml.FunctionCall("DATEADD", SqlDml.Native("MINUTE"), minutes, date);
    }

    private static SqlUserFunctionCall DateAddSecond(SqlExpression date, SqlExpression seconds)
    {
      return SqlDml.FunctionCall("DATEADD", SqlDml.Native("SECOND"), seconds, date);
    }

    protected static SqlUserFunctionCall DateAddMillisecond(SqlExpression date, SqlExpression milliseconds)
    {
      return SqlDml.FunctionCall("DATEADD", SqlDml.Native("MS"), milliseconds, date);
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