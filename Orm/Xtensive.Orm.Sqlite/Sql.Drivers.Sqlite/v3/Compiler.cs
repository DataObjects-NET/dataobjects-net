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
      // Bit XOR is not supported by SQLite
      // but it can be easily emulated using remaining bit operators

      if (node.NodeType==SqlNodeType.BitXor) {
        // A ^ B = (A | B) & ~(A & B)
        var replacement = SqlDml.BitAnd(
          SqlDml.BitOr(node.Left, node.Right),
          SqlDml.BitNot(SqlDml.BitAnd(node.Left, node.Right)));
        replacement.AcceptVisitor(this);
        return;
      }

      base.Visit(node);
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
        case SqlFunctionType.IntervalToMilliseconds:
          Visit(CastToLong(node.Arguments[0]) / NanosecondsPerMillisecond);
          return;
        case SqlFunctionType.IntervalConstruct:
        case SqlFunctionType.IntervalToNanoseconds:
          Visit(CastToLong(node.Arguments[0]));
          return;
        case SqlFunctionType.DateTimeAddMonths:
          using (context.EnterScope(node)) {
            context.Output.AppendText(translator.Translate(context, node, FunctionCallSection.Entry, -1));
            if (node.Arguments.Count > 0)
              using (context.EnterCollectionScope()) {
                int argumentPosition = 0;
                foreach (SqlExpression item in node.Arguments) {
                  if (!context.IsEmpty)
                    context.Output.AppendDelimiter(translator.Translate(context, node, FunctionCallSection.ArgumentDelimiter, argumentPosition));
                  context.Output.AppendText(translator.Translate(context, node, FunctionCallSection.ArgumentEntry, argumentPosition));
                  if (argumentPosition==1) {
                    SqlLiteral l = item as SqlLiteral;
                    if (l!=null) {
                      string value = translator.Translate(context, l.GetValue());
                      context.Output.AppendText(string.Format("'{0} MONTH'", value));
                    }
                  }
                  else
                    item.AcceptVisitor(this);
                  context.Output.AppendText(translator.Translate(context, node, FunctionCallSection.ArgumentExit, argumentPosition));
                  argumentPosition++;
                }
              }
            context.Output.AppendText(translator.Translate(context, node, FunctionCallSection.Exit, -1));
          }
          return;
        case SqlFunctionType.DateTimeAddYears:
          using (context.EnterScope(node)) {
            context.Output.AppendText(translator.Translate(context, node, FunctionCallSection.Entry, -1));
            if (node.Arguments.Count > 0)
              using (context.EnterCollectionScope()) {
                int argumentPosition = 0;
                foreach (SqlExpression item in node.Arguments) {
                  if (!context.IsEmpty)
                    context.Output.AppendDelimiter(translator.Translate(context, node, FunctionCallSection.ArgumentDelimiter, argumentPosition));
                  context.Output.AppendText(translator.Translate(context, node, FunctionCallSection.ArgumentEntry, argumentPosition));
                  if (argumentPosition==1) {
                    SqlLiteral l = item as SqlLiteral;
                    if (l!=null) {
                      string value = translator.Translate(context, l.GetValue());
                      context.Output.AppendText(string.Format("'{0} YEARS'", value));
                    }
                  }
                  else
                    item.AcceptVisitor(this);
                  context.Output.AppendText(translator.Translate(context, node, FunctionCallSection.ArgumentExit, argumentPosition));
                  argumentPosition++;
                }
              }
            context.Output.AppendText(translator.Translate(context, node, FunctionCallSection.Exit, -1));
          }
          return;
        case SqlFunctionType.DateTimeTruncate:
          DateTimeTruncate(node.Arguments[0]).AcceptVisitor(this);
          return;
        case SqlFunctionType.DateTimeConstruct:
          DateTimeConstruct(node.Arguments[0], node.Arguments[1], node.Arguments[2]).AcceptVisitor(this);
          return;
      }
      base.Visit(node);
    }

    private SqlExpression DateTimeConstruct(SqlExpression year, SqlExpression month, SqlExpression day)
    {
      // date('0000-01-01',
      //   '+' || cast(year as varchar)        || ' years',
      //   '+' || cast((month - 1) as varchar) || ' months',
      //   '+' || cast((day - 1) as varchar)   || ' days')

      var zeroYear = SqlDml.Literal("0000-01-01");
      var plus = SqlDml.Literal("+");
      var one = SqlDml.Literal(1);

      var yearSuffix = SqlDml.Literal(" years");
      var monthSuffix = SqlDml.Literal(" months");
      var daySuffix = SqlDml.Literal(" days");

      var yearModifier = SqlDml.Concat(plus, year, yearSuffix);
      var monthModifier = SqlDml.Concat(plus, SqlDml.Subtract(month, one), monthSuffix);
      var dayModifier = SqlDml.Concat(plus, SqlDml.Subtract(day, one), daySuffix);    

      return SqlDml.FunctionCall("DATE", zeroYear, yearModifier, monthModifier, dayModifier);
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

    private SqlExpression DateTimeTruncate(SqlExpression date)
    {
      return SqlDml.FunctionCall("DATETIME", SqlDml.FunctionCall("DATE", date));
    }

    private static SqlCast CastToLong(SqlExpression arg)
    {
      return SqlDml.Cast(arg, SqlType.Int64);
    }

    // Constructors

    /// <param name="driver">The driver.</param>
    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}