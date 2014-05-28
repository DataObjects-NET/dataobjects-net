// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.17

using System;
using System.Linq;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.Firebird.v2_5
{
  internal class Compiler : SqlCompiler
  {
    protected static readonly long NanosecondsPerDay = TimeSpan.FromDays(1).Ticks * 100;
    protected static readonly long NanosecondsPerSecond = 1000000000;
    protected static readonly long NanosecondsPerMillisecond = 1000000;
    protected static readonly long MillisecondsPerDay = (long) TimeSpan.FromDays(1).TotalMilliseconds;
    protected static readonly long MillisecondsPerSecond = 1000L;
    private bool case_SqlDateTimePart_DayOfYear;
    private bool case_SqlDateTimePart_Second;


    /// <inheritdoc/>
    public override void Visit(SqlSelect node)
    {
      using (context.EnterScope(node)) {
        context.Output.AppendText(translator.Translate(context, node, SelectSection.Entry));
        VisitSelectLimitOffset(node);
        VisitSelectColumns(node);
        VisitSelectFrom(node);
        VisitSelectWhere(node);
        VisitSelectGroupBy(node);
        VisitSelectOrderBy(node);
        VisitSelectLock(node);
        context.Output.AppendText(translator.Translate(context, node, SelectSection.Exit));
      }
    }

    /// <inheritdoc/>
    public override void VisitSelectFrom(SqlSelect node)
    {
      if (node.From!=null)
        base.VisitSelectFrom(node);
      else
        context.Output.AppendText("FROM RDB$DATABASE");
    }

    /// <inheritdoc/>
    public override void Visit(SqlQueryExpression node)
    {
      using (context.EnterScope(node)) {
        bool needOpeningParenthesis = false;
        bool needClosingParenthesis = false;
        context.Output.AppendText(translator.Translate(context, node, QueryExpressionSection.Entry));
        if (needOpeningParenthesis)
          context.Output.AppendText("(");
        node.Left.AcceptVisitor(this);
        if (needClosingParenthesis)
          context.Output.AppendText(")");
        context.Output.AppendText(translator.Translate(node.NodeType));
        context.Output.AppendText(translator.Translate(context, node, QueryExpressionSection.All));
        if (needOpeningParenthesis)
          context.Output.AppendText("(");
        node.Right.AcceptVisitor(this);
        if (needClosingParenthesis)
          context.Output.AppendText(")");
        context.Output.AppendText(translator.Translate(context, node, QueryExpressionSection.Exit));
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
      switch (node.DateTimePart) {
        case SqlDateTimePart.DayOfYear:
          if (!case_SqlDateTimePart_DayOfYear) {
            case_SqlDateTimePart_DayOfYear = true;
            Visit(SqlDml.Add(node, SqlDml.Literal(1)));
            case_SqlDateTimePart_DayOfYear = false;
          }
          else
            base.Visit(node);
          return;
        case SqlDateTimePart.Second:
          if (!case_SqlDateTimePart_Second) {
            case_SqlDateTimePart_Second = true;
            Visit(SqlDml.Truncate(node));
            case_SqlDateTimePart_Second = false;
          }
          else
            base.Visit(node);
          return;
      }
      base.Visit(node);
    }

    /// <inheritdoc/>
    public override void Visit(SqlUnary node)
    {
      if (node.NodeType==SqlNodeType.BitNot) {
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
        case SqlNodeType.Modulo:
          Visit(SqlDml.FunctionCall(translator.Translate(SqlNodeType.Modulo), node.Left, node.Right));
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
      switch (node.FunctionType) {
        case SqlFunctionType.Concat:
          var exprs = new SqlExpression[node.Arguments.Count];
          node.Arguments.CopyTo(exprs, 0);
          Visit(SqlDml.Concat(exprs));
          return;
        case SqlFunctionType.DateTimeTruncate:
          Visit(SqlDml.Cast(node.Arguments[0], new SqlValueType("Date")));
          return;
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
        case SqlFunctionType.DateTimeConstruct:
          Visit(DateAddDay(DateAddMonth(DateAddYear(SqlDml.Cast(SqlDml.Literal(new DateTime(2001, 1, 1)), SqlType.DateTime),
            node.Arguments[0] - 2001),
            node.Arguments[1] - 1),
            node.Arguments[2] - 1));
          return;
        case SqlFunctionType.DateTimeToStringIso:
          Visit(DateTimeToStringIso(node.Arguments[0]));
          return;
      }

      base.Visit(node);
    }

    public override void Visit(SqlRenameTable node)
    {
      throw new NotSupportedException();
    }

    public override void Visit(SqlAlterSequence node)
    {
      context.Output.AppendText(translator.Translate(context, node, NodeSection.Entry));
      context.Output.AppendText(translator.Translate(context, node, NodeSection.Exit));
    }

    #region Static helpers

    protected static SqlExpression DateTimeSubtractDateTime(SqlExpression date1, SqlExpression date2)
    {
      return CastToLong(DateDiffDay(date2, date1)) * NanosecondsPerDay
        +
        CastToLong(DateDiffMillisecond(DateAddDay(date2, DateDiffDay(date2, date1)), date1)) *
          NanosecondsPerMillisecond;
    }

    protected static SqlExpression DateTimeAddInterval(SqlExpression date, SqlExpression interval)
    {
      return DateAddMillisecond(
        DateAddDay(date, interval / NanosecondsPerDay),
        (interval / NanosecondsPerMillisecond) % (MillisecondsPerDay));
    }

    protected static SqlCast CastToLong(SqlExpression arg)
    {
      return SqlDml.Cast(arg, SqlType.Int64);
    }

    protected static SqlUserFunctionCall DateDiffDay(SqlExpression date1, SqlExpression date2)
    {
      return SqlDml.FunctionCall("DATEDIFF", SqlDml.Native("DAY"), date1, date2);
    }

    protected static SqlUserFunctionCall DateDiffMillisecond(SqlExpression date1, SqlExpression date2)
    {
      return SqlDml.FunctionCall("DATEDIFF", SqlDml.Native("MILLISECOND"), date1, date2);
    }

    protected static SqlUserFunctionCall DateAddYear(SqlExpression date, SqlExpression years)
    {
      return SqlDml.FunctionCall("DATEADD", SqlDml.Native("YEAR"), years, date);
    }

    protected static SqlUserFunctionCall DateAddMonth(SqlExpression date, SqlExpression months)
    {
      return SqlDml.FunctionCall("DATEADD", SqlDml.Native("MONTH"), months, date);
    }

    protected static SqlUserFunctionCall DateAddDay(SqlExpression date, SqlExpression days)
    {
      return SqlDml.FunctionCall("DATEADD", SqlDml.Native("DAY"), days, date);
    }

    protected static SqlUserFunctionCall DateAddHour(SqlExpression date, SqlExpression hours)
    {
      return SqlDml.FunctionCall("DATEADD", SqlDml.Native("HOUR"), hours, date);
    }

    protected static SqlUserFunctionCall DateAddMinute(SqlExpression date, SqlExpression minutes)
    {
      return SqlDml.FunctionCall("DATEADD", SqlDml.Native("MINUTE"), minutes, date);
    }

    protected static SqlUserFunctionCall DateAddSecond(SqlExpression date, SqlExpression seconds)
    {
      return SqlDml.FunctionCall("DATEADD", SqlDml.Native("SECOND"), seconds, date);
    }

    protected static SqlUserFunctionCall DateAddMillisecond(SqlExpression date, SqlExpression milliseconds)
    {
      return SqlDml.FunctionCall("DATEADD", SqlDml.Native("MILLISECOND"), milliseconds, date);
    }

    protected static SqlUserFunctionCall BitAnd(SqlExpression left, SqlExpression right)
    {
      return SqlDml.FunctionCall("BIN_AND", left, right);
    }

    protected static SqlUserFunctionCall BitOr(SqlExpression left, SqlExpression right)
    {
      return SqlDml.FunctionCall("BIN_OR", left, right);
    }

    protected static SqlUserFunctionCall BitXor(SqlExpression left, SqlExpression right)
    {
      return SqlDml.FunctionCall("BIN_XOR", left, right);
    }

    protected static SqlUserFunctionCall BitNot(SqlExpression operand)
    {
      return SqlDml.FunctionCall("BIN_NOT", operand);
    }

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