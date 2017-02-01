// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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

namespace Xtensive.Sql.Drivers.SqlServer.v09
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

    public override void VisitUpdateLimit(SqlUpdate node)
    {
      if (!node.Limit.IsNullReference()) {
        if (!Driver.ServerInfo.Query.Features.Supports(QueryFeatures.UpdateLimit))
          throw new NotSupportedException(Strings.ExStorageIsNotSupportedLimitationOfRowCountToUpdate);
        context.Output.AppendText(translator.Translate(context, node, UpdateSection.Limit));
        context.Output.AppendText("(");
        node.Limit.AcceptVisitor(this);
        context.Output.AppendText(")");
      }
    }

    public override void Visit(SqlDelete node)
    {
      using (context.EnterScope(node)) {
        VisitDeleteEntry(node);
        VisitDeleteLimit(node);
        context.Output.AppendText(translator.Translate(context, node, DeleteSection.From));
        VisitDeleteDelete(node);
        VisitDeleteFrom(node);
        VisitDeleteWhere(node);
        VisitDeleteExit(node);
      }
    }

    public override void VisitDeleteLimit(SqlDelete node)
    {
      if (!node.Limit.IsNullReference()) {
        if (!Driver.ServerInfo.Query.Features.Supports(QueryFeatures.DeleteLimit))
          throw new NotSupportedException(Strings.ExStorageIsNotSupportedLimitationOfRowCountToDelete);
        context.Output.AppendText(translator.Translate(context, node, DeleteSection.Limit));
        context.Output.AppendText("(");
        node.Limit.AcceptVisitor(this);
        context.Output.AppendText(")");
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
      if (renameColumnAction!=null) {
        context.Output.AppendText(((Translator) translator).Translate(context, renameColumnAction));
        return;
      }
      var dropConstrainAction = node.Action as SqlDropConstraint;
      if (dropConstrainAction!=null) {
        if (dropConstrainAction.Constraint is DefaultConstraint) {
          var constraint = dropConstrainAction.Constraint as DefaultConstraint;
          if (constraint.NameIsStale) {
            //We must know name of default constraint for drop it.
            //But MS SQL creates name of default constrain by itself.
            //And if we moved table to another schema or database or renamed table by recreation during upgrade,
            //we doesn't know real name of default constraint.
            //Because of this we should find name of constraint in system views.
            //And we able to drop default constraint after that.
            context.Output.AppendText(((Translator)translator).Translate(context, node, constraint));
            return;
          }
        }
      }
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
      case SqlFunctionType.DateTimeToStringIso:
        Visit(DateTimeToStringIso(node.Arguments[0]));
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

    public override void Visit(SqlRound node)
    {
      SqlExpression result;
      var shouldCastToDecimal = node.Type==TypeCode.Decimal;
      switch (node.Mode) {
      case MidpointRounding.ToEven:
        result = node.Length.IsNullReference()
          ? BankersRound(node.Argument, shouldCastToDecimal)
          : BankersRound(node.Argument, node.Length, shouldCastToDecimal);
        break;
      case MidpointRounding.AwayFromZero:
        result = node.Length.IsNullReference()
          ? RegularRound(node.Argument, shouldCastToDecimal)
          : RegularRound(node.Argument, node.Length, shouldCastToDecimal);
        break;
      default:
        throw new ArgumentOutOfRangeException();
      }
      result.AcceptVisitor(this);
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
      
      return CastToDecimal(DateDiffDay(date2, date1), 18, 0) * NanosecondsPerDay
          + CastToDecimal(DateDiffMillisecond(DateAddDay(date2, DateDiffDay(date2, date1)), date1),18 , 0) * NanosecondsPerMillisecond;
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

    public override void Visit(SqlContainsTable node)
    {
      string columns;
      if (node.TargetColumns.Count==1) {
        columns = node.TargetColumns[0]==node.Asterisk 
          ? node.TargetColumns[0].Name 
          : translator.QuoteIdentifier(node.TargetColumns[0].Name);
      }
      else
        columns = "(" + string.Join(", ", node.TargetColumns.Select(c => translator.QuoteIdentifier(c.Name)).ToArray()) + ")";
      context.Output.AppendText(string.Format(
        "CONTAINSTABLE({0}, {1}, ", translator.Translate(context, node.TargetTable.DataTable), columns));
      node.SearchCondition.AcceptVisitor(this);
      if (node.TopNByRank!=null) {
        context.Output.AppendText(", ");
        node.TopNByRank.AcceptVisitor(this);
      }
      context.Output.AppendText(") ");
    }

    public override void Visit(SqlFreeTextTable node)
    {
      string columns;
      if (node.TargetColumns.Count==1)
        columns = node.TargetColumns[0]==node.Asterisk
          ? node.TargetColumns[0].Name
          : translator.QuoteIdentifier(node.TargetColumns[0].Name);
      else
        columns = string.Join(", ", node.TargetColumns.Select(c => translator.QuoteIdentifier(c.Name)).ToArray());
      context.Output.AppendText(string.Format(
        "FREETEXTTABLE({0}, {1}, ", translator.Translate(context, node.TargetTable.DataTable), columns));
      node.FreeText.AcceptVisitor(this);
      if (node.TopNByRank!=null) {
        context.Output.AppendText(", ");
        node.TopNByRank.AcceptVisitor(this);
      }
      context.Output.AppendText(") ");
    }

    public override void Visit(SqlCreateIndex node, IndexColumn item)
    {
      base.Visit(node, item);

      if (item.TypeColumn!=null)
        context.Output.AppendText(string.Format("TYPE COLUMN {0} ", translator.QuoteIdentifier(item.TypeColumn.Name)));
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

    private static SqlCast CastToDecimal(SqlExpression arg, short precision, short scale)
    {
      return SqlDml.Cast(arg, SqlType.Decimal, precision, scale);
    }

    protected static SqlUserFunctionCall DatePartWeekDay(SqlExpression date)
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

    protected static SqlUserFunctionCall DateAddYear(SqlExpression date, SqlExpression years)
    {
      return SqlDml.FunctionCall("DATEADD", SqlDml.Native("YEAR"),years, date);
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
      return SqlDml.FunctionCall("DATEADD", SqlDml.Native("MS"), milliseconds, date);
    }

    protected static SqlUserFunctionCall DateTimeToStringIso(SqlExpression dateTime)
    {
      return SqlDml.FunctionCall("CONVERT", SqlDml.Native("NVARCHAR(19)"), dateTime, SqlDml.Native("126"));
    }

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