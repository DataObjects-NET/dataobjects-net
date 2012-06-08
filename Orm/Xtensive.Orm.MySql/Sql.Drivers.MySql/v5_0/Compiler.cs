// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.02.25

using System;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Drivers.MySql.v5_0
{
  internal class Compiler : SqlCompiler
  {
    protected static readonly long NanosecondsPerDay = TimeSpan.FromDays(1).Ticks * 100;
    protected static readonly long NanosecondsPerSecond = 1000000000;
    protected static readonly long NanosecondsPerMillisecond = 1000000;
    protected static readonly long NanosecondsPerMicrosecond = 1000;
    protected static readonly long MillisecondsPerDay = (long) TimeSpan.FromDays(1).TotalMilliseconds;
    protected static readonly long MillisecondsPerSecond = 1000L;

    /// <inheritdoc/>
    public override void Visit(SqlSelect node)
    {
      using (context.EnterScope(node)) {
        context.Output.AppendText(translator.Translate(context, node, SelectSection.Entry));
        VisitSelectColumns(node);
        VisitSelectFrom(node);
        VisitSelectHints(node);
        VisitSelectWhere(node);
        VisitSelectGroupBy(node);
        VisitSelectOrderBy(node);
        VisitSelectLimitOffset(node);
        VisitSelectLock(node);
        context.Output.AppendText(translator.Translate(context, node, SelectSection.Exit));
      }
    }

    /// <inheritdoc/>
    public override void Visit(SqlAlterTable node)
    {
      var renameColumnAction = node.Action as SqlRenameColumn;
      if (renameColumnAction!=null)
        context.Output.AppendText(((Translator) translator).Translate(context, renameColumnAction));
      else if (node.Action is SqlDropConstraint) {
        using (context.EnterScope(node)) {
          context.Output.AppendText(translator.Translate(context, node, AlterTableSection.Entry));

          var action = node.Action as SqlDropConstraint;
          var constraint = action.Constraint as TableConstraint;
          context.Output.AppendText(translator.Translate(context, node, AlterTableSection.DropConstraint));
          if (constraint is ForeignKey)
            context.Output.AppendText("FOREIGN KEY " + translator.QuoteIdentifier(constraint.DbName));
          else if (constraint is PrimaryKey)
            context.Output.AppendText("PRIMARY KEY ");
          else
            context.Output.AppendText(translator.Translate(context, constraint, ConstraintSection.Entry));
          context.Output.AppendText(translator.Translate(context, node, AlterTableSection.DropBehavior));
          context.Output.AppendText(translator.Translate(context, node, AlterTableSection.Exit));
        }
      }
      else
        base.Visit(node);
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
        default:
          base.Visit(node);
          return;
      }
    }

    protected virtual SqlExpression DateTimeSubtractDateTime(SqlExpression date1, SqlExpression date2)
    {
      return CastToLong(DateDiffDay(date2, date1)) * NanosecondsPerDay
        +
        CastToLong(DateDiffMillisecond(DateAddDay(date2, DateDiffDay(date2, date1)), date1)) *
          NanosecondsPerMillisecond;
    }

    protected virtual SqlExpression DateTimeAddInterval(SqlExpression date, SqlExpression interval)
    {
      return DateAddMicrosecond(
        DateAddDay(date, interval / NanosecondsPerDay),
        (interval / NanosecondsPerMillisecond * NanosecondsPerMicrosecond) % (MillisecondsPerDay * NanosecondsPerMicrosecond));
    }

    /// <inheritdoc/>
    /// //Thanks to Csaba Beer.
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
    public override void Visit(SqlFunctionCall node)
    {
      switch (node.FunctionType) {
        case SqlFunctionType.Concat:
          var exprs = new SqlExpression[node.Arguments.Count];
          node.Arguments.CopyTo(exprs, 0);
          Visit(SqlDml.Concat(exprs));
          return;
        case SqlFunctionType.Position:
          Position(node.Arguments[0], node.Arguments[1]).AcceptVisitor(this);
          return;
        case SqlFunctionType.CharLength:
          SqlDml.FunctionCall(translator.Translate(SqlFunctionType.CharLength), node.Arguments[0]).AcceptVisitor(this);
          //          SqlDml.CharLength(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.PadLeft:
        case SqlFunctionType.PadRight:
          SqlHelper.GenericPad(node).AcceptVisitor(this);
          return;
        case SqlFunctionType.Rand:
          SqlDml.FunctionCall(translator.Translate(SqlFunctionType.Rand)).AcceptVisitor(this);
          return;
        case SqlFunctionType.Square:
          SqlDml.Power(node.Arguments[0], 2).AcceptVisitor(this);
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
          Visit(DateAddDay(DateAddMonth(DateAddYear(SqlDml.Literal(new DateTime(2001, 1, 1)),
            node.Arguments[0] - 2001),
            node.Arguments[1] - 1),
            node.Arguments[2] - 1));
          return;
      }
      base.Visit(node);
    }

    public override void VisitSelectLimitOffset(SqlSelect node)
    {
      if (!node.Limit.IsNullReference()) {
        context.Output.AppendText(translator.Translate(context, node, SelectSection.Limit));
        node.Limit.AcceptVisitor(this);
      }
      if (!node.Offset.IsNullReference()) {
        if (node.Limit.IsNullReference()) {
          context.Output.AppendText(translator.Translate(context, node, SelectSection.Limit));
          context.Output.AppendText(" 18446744073709551615 "); // magic number from http://dev.mysql.com/doc/refman/5.0/en/select.html
        }
        context.Output.AppendText(translator.Translate(context, node, SelectSection.Offset));
        node.Offset.AcceptVisitor(this);
      }
    }

    #region Static helpers

    private static SqlExpression Position(SqlExpression substring, SqlExpression _string)
    {
      return SqlDml.FunctionCall("LOCATE", _string, substring) - 1;
    }

    private static SqlCast CastToLong(SqlExpression arg)
    {
      return SqlDml.Cast(arg, SqlType.Int64);
    }

    protected static SqlUserFunctionCall DateDiffDay(SqlExpression date1, SqlExpression date2)
    {
      return SqlDml.FunctionCall("DATEDIFF", date1, date2);
    }

    protected static SqlUserFunctionCall DateDiffMillisecond(SqlExpression date1, SqlExpression date2)
    {
      return SqlDml.FunctionCall("MICROSECOND", SqlDml.FunctionCall("DATEDIFF", date1, date2) * NanosecondsPerMillisecond);
    }

    private static SqlUserFunctionCall DateAddYear(SqlExpression date, SqlExpression years)
    {
      return SqlDml.FunctionCall("TIMESTAMPADD", SqlDml.Native("YEAR"), years, date);
    }

    private static SqlUserFunctionCall DateAddMonth(SqlExpression date, SqlExpression months)
    {
      return SqlDml.FunctionCall("TIMESTAMPADD", SqlDml.Native("MONTH"), months, date);
    }

    protected static SqlUserFunctionCall DateAddDay(SqlExpression date, SqlExpression days)
    {
      return SqlDml.FunctionCall("TIMESTAMPADD", SqlDml.Native("DAY"), days, date);
    }

    private static SqlUserFunctionCall DateAddHour(SqlExpression date, SqlExpression hours)
    {
      return SqlDml.FunctionCall("TIMESTAMPADD", SqlDml.Native("HOUR"), hours, date);
    }

    private static SqlUserFunctionCall DateAddMinute(SqlExpression date, SqlExpression minutes)
    {
      return SqlDml.FunctionCall("TIMESTAMPADD", SqlDml.Native("MINUTE"), minutes, date);
    }

    private static SqlUserFunctionCall DateAddSecond(SqlExpression date, SqlExpression seconds)
    {
      return SqlDml.FunctionCall("TIMESTAMPADD", SqlDml.Native("SECOND"), seconds, date);
    }

    protected static SqlUserFunctionCall DateAddMicrosecond(SqlExpression date, SqlExpression microseconds)
    {
      return SqlDml.FunctionCall("TIMESTAMPADD", SqlDml.Native("MICROSECOND"), microseconds, date);
    }

    #endregion

    // Constructors

    protected internal Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}