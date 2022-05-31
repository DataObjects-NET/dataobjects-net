// Copyright (C) 2011-2022 Xtensive LLC.
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
      switch (node.FunctionType) {
        case SqlFunctionType.Truncate:
          var argument = node.Arguments[0];
          SqlDml.FunctionCall("TRUNCATE", argument, SqlDml.Literal(0)).AcceptVisitor(this);
          return;
        case SqlFunctionType.Concat:
          Visit(SqlDml.Concat(node.Arguments.ToArray(node.Arguments.Count)));
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
        case SqlFunctionType.DateTimeToStringIso:
          Visit(DateTimeToStringIso(node.Arguments[0]));
          return;
      }

      base.Visit(node);
    }

    /// <inheritdoc/>
    protected override void VisitSelectLimitOffset(SqlSelect node)
    {
      if (!node.Limit.IsNullReference()) {
        AppendTranslated(node, SelectSection.Limit);
        node.Limit.AcceptVisitor(this);
      }
      if (!node.Offset.IsNullReference()) {
        if (node.Limit.IsNullReference()) {
          AppendTranslated(node, SelectSection.Limit);
          _ = context.Output.Append(" 18446744073709551615 "); // magic number from http://dev.mysql.com/doc/refman/5.0/en/select.html
        }
        AppendTranslated(node, SelectSection.Offset);
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
      base.Visit(node);
    }

    protected virtual SqlExpression DateTimeSubtractDateTime(SqlExpression date1, SqlExpression date2)
    {
      return (CastToDecimal(DateDiffDay(date1, date2), 18, 0) * NanosecondsPerDay)
        +
        (CastToDecimal(DateDiffMicrosecond(DateAddDay(date2, DateDiffDay(date1, date2)), date1), 18, 0) * NanosecondsPerMicrosecond);
    }

    protected virtual SqlExpression DateTimeAddInterval(SqlExpression date, SqlExpression interval)
    {
      return DateAddMicrosecond(
        DateAddDay(date, ((interval - (interval % NanosecondsPerDay)) + ((interval % NanosecondsPerDay) > (NanosecondsPerDay / 2) ? 0 : 1)) / NanosecondsPerDay),
        (interval / NanosecondsPerMillisecond * NanosecondsPerMicrosecond) % (MillisecondsPerDay * NanosecondsPerMicrosecond));
    }

    #region Static helpers

    private static SqlCast CastToLong(SqlExpression arg) => SqlDml.Cast(arg, SqlType.Int64);

    private static SqlCast CastToDecimal(SqlExpression arg, short precision, short scale) =>
      SqlDml.Cast(arg, SqlType.Decimal, precision, scale);

    private static SqlUserFunctionCall DateDiffDay(SqlExpression date1, SqlExpression date2) =>
      SqlDml.FunctionCall("DATEDIFF", date1, date2);

    private static SqlUserFunctionCall DateDiffMicrosecond(SqlExpression date1, SqlExpression date2) =>
      SqlDml.FunctionCall("TIMESTAMPDIFF", SqlDml.Native("MICROSECOND"), date1, date2);

    private static SqlUserFunctionCall DateAddYear(SqlExpression date, SqlExpression years) =>
      SqlDml.FunctionCall("TIMESTAMPADD", SqlDml.Native("YEAR"), years, date);

    private static SqlUserFunctionCall DateAddMonth(SqlExpression date, SqlExpression months) =>
      SqlDml.FunctionCall("TIMESTAMPADD", SqlDml.Native("MONTH"), months, date);

    private static SqlUserFunctionCall DateAddDay(SqlExpression date, SqlExpression days) =>
      SqlDml.FunctionCall("TIMESTAMPADD", SqlDml.Native("DAY"), days, date);

    private static SqlUserFunctionCall DateAddMicrosecond(SqlExpression date, SqlExpression microseconds) =>
      SqlDml.FunctionCall("TIMESTAMPADD", SqlDml.Native("MICROSECOND"), microseconds, date);

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