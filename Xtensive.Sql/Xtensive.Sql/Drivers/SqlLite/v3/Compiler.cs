// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.04.29

using System;
using System.Linq;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Model;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Drivers.SQLite.Resources;

namespace Xtensive.Sql.SQLite.v3
{
  internal class Compiler : SqlCompiler
  {
    private static readonly long NanosecondsPerDay = TimeSpan.FromDays(1).Ticks*100;
    private static readonly long NanosecondsPerSecond = 1000000000;
    private static readonly long NanosecondsPerMillisecond = 1000000;
    private static readonly long MillisecondsPerDay = (long) TimeSpan.FromDays(1).TotalMilliseconds;
    private static readonly long MillisecondsPerSecond = 1000L;
    
    public override void Visit(SqlSelect node)
    {
      //For hinting limitations see http://www.sqlite.org/lang_indexedby.html
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
    public override void Visit(SqlAlterTable node)
    {
        var renameColumnAction = node.Action as SqlRenameColumn;
        if (renameColumnAction != null)
            context.Output.AppendText(((Translator)translator).Translate(context, renameColumnAction));
        else if (node.Action is SqlDropConstraint)
        {
            using (context.EnterScope(node))
            {
                context.Output.AppendText(translator.Translate(context, node, AlterTableSection.Entry));

                var action = node.Action as SqlDropConstraint;
                var constraint = action.Constraint as TableConstraint;
                context.Output.AppendText(translator.Translate(context, node, AlterTableSection.DropConstraint));
                if (constraint is ForeignKey)
                    context.Output.AppendText("REFERENCES " + translator.QuoteIdentifier(constraint.DbName));
                else
                    context.Output.AppendText(translator.Translate(context, constraint, ConstraintSection.Entry));
                context.Output.AppendText(translator.Translate(context, node, AlterTableSection.DropBehavior));
                context.Output.AppendText(translator.Translate(context, node, AlterTableSection.Exit));
            }
        }
        else
            base.Visit(node);
    }

    public override void VisitSelectLock(SqlSelect node)
    {
      return;
    }

    /// <inheritdoc/>
    public override void Visit(SqlFunctionCall node)
    {
      switch (node.FunctionType) {
      case SqlFunctionType.CharLength:
         (SqlDml.FunctionCall("length", node.Arguments) / 2).AcceptVisitor(this);
        return;
      case SqlFunctionType.PadLeft:
      case SqlFunctionType.PadRight:
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

    /// <inheritdoc/>
    public override void Visit(SqlFreeTextTable node)
    {
        throw SqlHelper.NotSupported("FreeText");
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

    /// <inheritdoc/>
    /// //Thanks to Csaba Beer.
    public override void Visit(SqlQueryExpression node)
    {
        using (context.EnterScope(node))
        {
            bool needOpeningParenthesis = false;
            bool needClosingParenthesis = false;
            context.Output.AppendText(translator.Translate(context, node, QueryExpressionSection.Entry));
            if (needOpeningParenthesis) context.Output.AppendText("(");
            node.Left.AcceptVisitor(this);
            if (needClosingParenthesis) context.Output.AppendText(")");
            context.Output.AppendText(translator.Translate(node.NodeType));
            context.Output.AppendText(translator.Translate(context, node, QueryExpressionSection.All));
            if (needOpeningParenthesis) context.Output.AppendText("(");
            node.Right.AcceptVisitor(this);
            if (needClosingParenthesis) context.Output.AppendText(")");
            context.Output.AppendText(translator.Translate(context, node, QueryExpressionSection.Exit));
        }
    }
    
    public override void Visit(SqlExtract node)
    {
        switch (node.IntervalPart)
        {
            case SqlIntervalPart.Day:
                Visit(CastToLong(node.Operand/NanosecondsPerDay));
                return;
            case SqlIntervalPart.Hour:
                Visit(CastToLong(node.Operand/(60*60*NanosecondsPerSecond))%24);
                return;
            case SqlIntervalPart.Minute:
                Visit(CastToLong(node.Operand/(60*NanosecondsPerSecond))%60);
                return;
            case SqlIntervalPart.Second:
                Visit(CastToLong(node.Operand/NanosecondsPerSecond)%60);
                return;
            case SqlIntervalPart.Millisecond:
                Visit(CastToLong(node.Operand/NanosecondsPerMillisecond)%MillisecondsPerSecond);
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

    public override void VisitSelectLimitOffset(SqlSelect node)
    {
      if (!node.Limit.IsNullReference()) {
        context.Output.AppendText(translator.Translate(context, node, SelectSection.Limit));
        context.Output.AppendText(" (");
        node.Limit.AcceptVisitor(this);
        context.Output.AppendText(")");
        return;
      }
      base.VisitSelectLimitOffset(node);
    }

    private SqlExpression DateTimeTruncate(SqlExpression date)
    {
      return DateAddMillisecond(DateAddSecond(DateAddMinute(DateAddHour(date,
        -SqlDml.Extract(SqlDateTimePart.Hour, date)),
        -SqlDml.Extract(SqlDateTimePart.Minute, date)),
        -SqlDml.Extract(SqlDateTimePart.Second, date)),
        -SqlDml.Extract(SqlDateTimePart.Millisecond, date));
    }
    
    private SqlExpression DateTimeSubtractDateTime(SqlExpression date1, SqlExpression date2)
    {
      return CastToLong(DateDiffDay(date2, date1)) * NanosecondsPerDay
          + CastToLong(DateDiffMillisecond(DateAddDay(date2, DateDiffDay(date2, date1)), date1)) * NanosecondsPerMillisecond
          + DateDiffNanosecond(
              DateAddMillisecond(
                DateAddDay(date2, DateDiffDay(date2, date1)), 
                DateDiffMillisecond(DateAddDay(date2, DateDiffDay(date2, date1)), date1)),
              date1);
    }

    private SqlExpression DateTimeAddInterval(SqlExpression date, SqlExpression interval)
    {
      return DateAddNanosecond(
        DateAddMillisecond(
          DateAddDay(date, interval / NanosecondsPerDay),
          (interval/NanosecondsPerMillisecond) % (MillisecondsPerDay)),
        (interval/NanosecondsPerSecond) % NanosecondsPerDay/NanosecondsPerSecond);
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

    private static SqlUserFunctionCall DateDiffDay(SqlExpression date1, SqlExpression date2)
    {
      return SqlDml.FunctionCall("DATEDIFF", SqlDml.Native("DAY"), date1, date2);
    }

    private static SqlUserFunctionCall DateDiffMillisecond(SqlExpression date1, SqlExpression date2)
    {
      return SqlDml.FunctionCall("DATEDIFF", SqlDml.Native("MS"), date1, date2);
    }

    private static SqlUserFunctionCall DateDiffNanosecond(SqlExpression date1, SqlExpression date2)
    {
      return SqlDml.FunctionCall("DATEDIFF", SqlDml.Native("NS"), date1, date2);
    }

    private static SqlUserFunctionCall DateAddYear(SqlExpression date, SqlExpression years)
    {
      return SqlDml.FunctionCall("DATEADD", SqlDml.Native("YEAR"),years, date);
    }

    private static SqlUserFunctionCall DateAddMonth(SqlExpression date, SqlExpression months)
    {
      return SqlDml.FunctionCall("DATEADD", SqlDml.Native("MONTH"), months, date);
    }

    private static SqlUserFunctionCall DateAddDay(SqlExpression date, SqlExpression days)
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

    private static SqlUserFunctionCall DateAddMillisecond(SqlExpression date, SqlExpression milliseconds)
    {
      return SqlDml.FunctionCall("DATEADD", SqlDml.Native("MS"), milliseconds, date);
    }

    private static SqlUserFunctionCall DateAddNanosecond(SqlExpression date, SqlExpression nanoseconds)
    {
      return SqlDml.FunctionCall("DATEADD", SqlDml.Native("NS"), nanoseconds, date);
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