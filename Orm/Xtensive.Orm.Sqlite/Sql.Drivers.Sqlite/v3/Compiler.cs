// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.04.29

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Drivers.Sqlite.v3
{
  internal class Compiler : SqlCompiler
  {
    private static readonly long NanosecondsPerDay = TimeSpan.FromDays(1).Ticks * 100;
    private static readonly long NanosecondsPerSecond = 1000000000;
    private static readonly long NanosecondsPerMillisecond = 1000000;
    private static readonly long MillisecondsPerDay = (long) TimeSpan.FromDays(1).TotalMilliseconds;
    private static readonly long MillisecondsPerSecond = 1000L;

    protected override bool VisitCreateTableConstraints(SqlCreateTable node, IEnumerable<TableConstraint> constraints, bool hasItems)
    {
      // SQLite has special syntax for autoincrement primary keys
      // We write everything when doing translation for column definition
      // and should skip any PK definitions here.
      var hasAutoIncrementColumn = node.Table.TableColumns.Any(c => c.SequenceDescriptor!=null);
      constraints = hasAutoIncrementColumn ? constraints.Where(c => !(c is PrimaryKey)) : constraints;
      return base.VisitCreateTableConstraints(node, constraints, hasItems);
    }

    public override void Visit(SqlBinary node)
    {
      switch (node.NodeType) {
        // Bit XOR is not supported by SQLite
        // but it can be easily emulated using remaining bit operators
      case SqlNodeType.BitXor:
        // A ^ B = (A | B) & ~(A & B)
        var replacement = SqlDml.BitAnd(
          SqlDml.BitOr(node.Left, node.Right),
          SqlDml.BitNot(SqlDml.BitAnd(node.Left, node.Right)));
        replacement.AcceptVisitor(this);
        return;
      case SqlNodeType.DateTimePlusInterval:
        DateTimeAddInterval(node.Left, node.Right).AcceptVisitor(this);
        return;
      case SqlNodeType.DateTimeMinusInterval:
        DateTimeAddInterval(node.Left, -node.Right).AcceptVisitor(this);
        return;
      case SqlNodeType.DateTimeMinusDateTime:
        DateTimeSubtractDateTime(node.Left, node.Right).AcceptVisitor(this);
        return;
      default:
        base.Visit(node);
        return;
      }
    }

    public override void Visit(SqlAlterTable node)
    {
      var renameColumnAction = node.Action as SqlRenameColumn;
      if (renameColumnAction!=null)
        context.Output.AppendText(((Translator) translator).Translate(context, renameColumnAction));
      else if (node.Action is SqlDropConstraint)
        using (context.EnterScope(node)) {
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
      else
        base.Visit(node);
    }

    public override void Visit(SqlExtract node)
    {
      using (context.EnterScope(node)) {
        context.Output.AppendText(translator.Translate(context, node, ExtractSection.Entry));
        var part = node.DateTimePart!=SqlDateTimePart.Nothing ? translator.Translate(node.DateTimePart) : translator.Translate(node.IntervalPart);
        context.Output.AppendText(translator.Translate(context, node, ExtractSection.From));
        context.Output.AppendText(",");
        node.Operand.AcceptVisitor(this);
        context.Output.AppendText(translator.Translate(context, node, ExtractSection.Exit));
      }
    }

    public override void Visit(SqlFreeTextTable node)
    {
      throw SqlHelper.NotSupported("FreeText");
    }

    public override void Visit(SqlFunctionCall node)
    {
      switch (node.FunctionType) {
        case SqlFunctionType.CharLength:
          (SqlDml.FunctionCall("LENGTH", node.Arguments) / 2).AcceptVisitor(this);
          return;
        case SqlFunctionType.PadLeft:
        case SqlFunctionType.PadRight:
          return;
        case SqlFunctionType.Concat:
          var nod = node.Arguments[0];
          return;
        case SqlFunctionType.Round:
          // Round should always be called with 2 arguments
          if (node.Arguments.Count==1) {
            Visit(SqlDml.FunctionCall(translator.Translate(SqlFunctionType.Round), node.Arguments[0], SqlDml.Literal(0)));
            return;
          }
          break;
        case SqlFunctionType.Truncate:
          Visit(CastToLong(node.Arguments[0]));
          return;
        case SqlFunctionType.IntervalToNanoseconds:
          Visit(CastToLong(node.Arguments[0] / NanosecondsPerMillisecond));
          return;
        case SqlFunctionType.IntervalConstruct:
        case SqlFunctionType.IntervalToMilliseconds:
          Visit(CastToLong(node.Arguments[0] / NanosecondsPerMillisecond));
          return;
        case SqlFunctionType.DateTimeAddMonths:
          DateAddMonth(node.Arguments[0], node.Arguments[1]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeAddYears:
          DateAddYear(node.Arguments[0], node.Arguments[1]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeTruncate:
          DateTimeTruncate(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeConstruct:
          DateAddDay(DateAddMonth(DateAddYear(SqlDml.Literal(new DateTime(2001, 1, 1)),
            node.Arguments[0] - 2001),
            node.Arguments[1] - 1),
            node.Arguments[2] - 1).AcceptVisitor(this);
          return;
      }
      base.Visit(node);
    }

    public override void Visit(SqlQueryExpression node)
    {
      using (context.EnterScope(node)) {
        context.Output.AppendText(translator.Translate(context, node, QueryExpressionSection.Entry));
        node.Left.AcceptVisitor(this);
        context.Output.AppendText(translator.Translate(node.NodeType));
        context.Output.AppendText(translator.Translate(context, node, QueryExpressionSection.All));
        node.Right.AcceptVisitor(this);
        context.Output.AppendText(translator.Translate(context, node, QueryExpressionSection.Exit));
      }
    }

    public override void Visit(SqlSelect node)
    {
      // For hinting limitations see http://www.sqlite.org/lang_indexedby.html

      using (context.EnterScope(node)) {
        context.Output.AppendText(translator.Translate(context, node, SelectSection.Entry));
        VisitSelectColumns(node);
        VisitSelectFrom(node);
        VisitSelectWhere(node);
        VisitSelectGroupBy(node);
        VisitSelectOrderBy(node);
        VisitSelectLimitOffset(node);
        context.Output.AppendText(translator.Translate(context, node, SelectSection.Exit));
      }
    }

    public override void Visit(SqlTrim node)
    {
      using (context.EnterScope(node)) {
        context.Output.AppendText(translator.Translate(context, node, TrimSection.Entry));
        context.Output.AppendText(translator.Translate(node.TrimType));
        node.Expression.AcceptVisitor(this);
        if (node.TrimCharacters!=null) {
          context.Output.AppendText(",");
          context.Output.AppendText(translator.Translate(context, node.TrimCharacters));
        }
        context.Output.AppendText(translator.Translate(context, node, TrimSection.Exit));
      }
    }

    /// <inheritdoc/>
    public override void VisitSelectLimitOffset(SqlSelect node)
    {
      // SQLite requires limit to be present if offset it used,
      // luckily negatives value does the job.

      var isSpecialCase = !node.HasLimit && node.HasOffset;

      if (!isSpecialCase) {
        base.VisitSelectLimitOffset(node);
        return;
      }

      context.Output.AppendText(translator.Translate(context, node, SelectSection.Limit));
      SqlDml.Literal(-1).AcceptVisitor(this);
      context.Output.AppendText(translator.Translate(context, node, SelectSection.LimitEnd));

      context.Output.AppendText(translator.Translate(context, node, SelectSection.Offset));
      node.Offset.AcceptVisitor(this);
      context.Output.AppendText(translator.Translate(context, node, SelectSection.OffsetEnd));
    }

    public override void Visit(SqlCreateIndex node, IndexColumn item)
    {
      base.Visit(node, item);

      var column = item.Column as TableColumn;
      if (column!=null && column.Collation!=null)
        context.Output.AppendText(translator.Translate(context, column, TableColumnSection.Collate));
    }

    protected virtual SqlExpression DateTimeAddInterval(SqlExpression date, SqlExpression interval)
    {
      return DateAddDay(date, interval / MillisecondsPerDay);
    }

    private SqlExpression DateTimeTruncate(SqlExpression date)
    {
      return SqlDml.FunctionCall("DATETIME", SqlDml.FunctionCall("DATE", date));
    }

    private static SqlCast CastToLong(SqlExpression arg)
    {
      return SqlDml.Cast(arg, SqlType.Int64);
    }

    private static SqlExpression DateAddYear(SqlExpression date, SqlExpression years)
    {
      return SqlDml.FunctionCall("DATETIME", date, SqlDml.Concat(years, " ", "YEARS"));
    }

    private static SqlExpression DateAddMonth(SqlExpression date, SqlExpression months)
    {
      return SqlDml.FunctionCall("DATETIME", date, SqlDml.Concat(months, " ", "MONTHS"));
    }

    private static SqlExpression DateAddDay(SqlExpression date, SqlExpression days)
    {
      return SqlDml.FunctionCall("DATETIME", date, SqlDml.Concat(days, " ", "DAYS"));
    }

    private static SqlExpression DateDiffDay(SqlExpression date1, SqlExpression date2)
    {
      return SqlDml.FunctionCall("strftime", "%d", date1) - SqlDml.FunctionCall("strftime", "%d", date2);
    }

    private static SqlExpression DateDiffSeconds(SqlExpression date1, SqlExpression date2)
    {
      return SqlDml.FunctionCall("strftime", "%s", date1) - SqlDml.FunctionCall("strftime", "%s", date2);
    }

    protected static SqlExpression DateTimeSubtractDateTime(SqlExpression date1, SqlExpression date2)
    {
      return CastToLong(DateDiffDay(date1, date2)) * NanosecondsPerDay
        +
        CastToLong(DateDiffSeconds(DateAddDay(date1, DateDiffDay(date1, date2)), date2)) *
          NanosecondsPerSecond;
    }

    // Constructors

    /// <param name="driver">The driver.</param>
    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}