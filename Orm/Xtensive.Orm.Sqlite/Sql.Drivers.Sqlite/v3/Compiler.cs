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
    private const long NanosecondsPerMillisecond = 1000000L;
    private static readonly long NanosecondsPerDay = (long) TimeSpan.FromDays(1).TotalMilliseconds * NanosecondsPerMillisecond;
    private static readonly long NanosecondsPerHour = (long) TimeSpan.FromHours(1).TotalMilliseconds * NanosecondsPerMillisecond;
    private static readonly long NanosecondsPerSecond = (long) TimeSpan.FromSeconds(1).TotalMilliseconds * NanosecondsPerMillisecond;
    private static readonly long MillisecondsPerSecond = (long) TimeSpan.FromSeconds(1).TotalMilliseconds;

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
      case SqlNodeType.DateTimeOffsetMinusDateTimeOffset:
        DateTimeSubtractDateTime(node.Left, node.Right).AcceptVisitor(this);
        return;
      case SqlNodeType.DateTimeOffsetPlusInterval:
        SqlDml.Concat(
          DateTimeAddInterval(DateTimeOffsetExtractDateTimeAsString(node.Left), node.Right),
          DateTimeOffsetExtractOffsetAsString(node.Left))
          .AcceptVisitor(this);
        return;
      case SqlNodeType.DateTimeOffsetMinusInterval:
        SqlDml.Concat(
          DateTimeAddInterval(DateTimeOffsetExtractDateTimeAsString(node.Left), -node.Right),
          DateTimeOffsetExtractOffsetAsString(node.Left))
          .AcceptVisitor(this);
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
      else if (node.Action is SqlDropConstraint) {
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
      }
      else
        base.Visit(node);
    }

    public override void Visit(SqlExtract node)
    {
      if (node.IntervalPart!=SqlIntervalPart.Nothing) {
        VisitInterval(node);
        return;
      }
      if (node.DateTimePart!=SqlDateTimePart.Nothing) {
        VisitDateTime(node);
        return;
      }
      if (node.DateTimeOffsetPart!=SqlDateTimeOffsetPart.Nothing) {
        VisitDateTimeOffset(node);
        return;
      }
      base.Visit(node);
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
      case SqlFunctionType.IntervalConstruct:
        Visit(CastToLong(node.Arguments[0]));
        return;
      case SqlFunctionType.IntervalToNanoseconds:
        Visit(CastToLong(node.Arguments[0]));
        return;
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
      case SqlFunctionType.DateTimeToStringIso:
        DateTimeToStringIso(node.Arguments[0]).AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeOffsetAddMonths:
        SqlDml.Concat(DateAddMonth(DateTimeOffsetExtractDateTimeAsString(node.Arguments[0]), node.Arguments[1]), DateTimeOffsetExtractOffsetAsString(node.Arguments[0])).AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeOffsetAddYears:
        SqlDml.Concat(DateAddYear(DateTimeOffsetExtractDateTimeAsString(node.Arguments[0]), node.Arguments[1]), DateTimeOffsetExtractOffsetAsString(node.Arguments[0])).AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeOffsetConstruct:
        SqlDml.Concat(node.Arguments[0], OffsetToOffsetAsString(node.Arguments[1])).AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeOffsetToLocalTime:
        SqlDml.Concat(DateTimeOffsetToLocalDateTime(node.Arguments[0]), ServerOffsetAsString()).AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeOffsetToUtcTime:
        SqlDml.Concat(DateTimeOffsetToUtcDateTime(node.Arguments[0]), "+00:00").AcceptVisitor(this);
        return;
      case SqlFunctionType.DateTimeOffsetTimeOfDay:
        SqlDml.DateTimeMinusDateTime(
          DateTimeOffsetExtractDateTimeAsString(node.Arguments[0]),
          DateTimeTruncate(DateTimeOffsetExtractDateTimeAsString(node.Arguments[0])))
          .AcceptVisitor(this);
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

    private void VisitInterval(SqlExtract node)
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
        Visit(CastToLong(node.Operand % NanosecondsPerMillisecond));
        return;
      }
    }

    private void VisitDateTime(SqlExtract node)
    {
      if (node.DateTimePart==SqlDateTimePart.Millisecond) {
        Visit(CastToLong(DateGetMilliseconds(node.Operand)));
        return;
      }
      base.Visit(node);
    }

    private void VisitDateTimeOffset(SqlExtract node)
    {
      switch (node.DateTimeOffsetPart) {
      case SqlDateTimeOffsetPart.Date:
        DateTimeTruncate(DateTimeOffsetExtractDateTimeAsString(node.Operand)).AcceptVisitor(this);
        return;
      case SqlDateTimeOffsetPart.DateTime:
        DateTime(DateTimeOffsetExtractDateTimeAsString(node.Operand)).AcceptVisitor(this);
        return;
      case SqlDateTimeOffsetPart.LocalDateTime:
        DateTimeOffsetToLocalDateTime(node.Operand).AcceptVisitor(this);
        return;
      case SqlDateTimeOffsetPart.UtcDateTime:
        DateTimeOffsetToUtcDateTime(node.Operand).AcceptVisitor(this);
        return;
      case SqlDateTimeOffsetPart.Offset:
        (DateTimeOffsetExtractOffsetAsTotalNanoseconds(node.Operand)).AcceptVisitor(this);
        return;
      case SqlDateTimeOffsetPart.TimeZoneHour:
        (DateTimeOffsetExtractOffsetAsTotalNanoseconds(node.Operand) / NanosecondsPerHour).AcceptVisitor(this);
        return;
      case SqlDateTimeOffsetPart.TimeZoneMinute:
        (((DateTimeOffsetExtractOffsetAsTotalNanoseconds(node.Operand)) % NanosecondsPerHour) / (60 * NanosecondsPerSecond)).AcceptVisitor(this);
        return;
      }
      Visit(SqlDml.Extract(ConvertDateTimeOffsetPartToDateTimePart(node.DateTimeOffsetPart), DateTimeOffsetExtractDateTimeAsString(node.Operand)));
    }

    private static SqlExpression DateTimeAddInterval(SqlExpression date, SqlExpression interval)
    {
      return DateAddSeconds(date, interval / Convert.ToDouble(NanosecondsPerSecond));
    }

    private static SqlExpression DateTimeTruncate(SqlExpression date)
    {
      return DateTime(SqlDml.FunctionCall("DATE", date));
    }

    private static SqlExpression DateTime(SqlExpression date)
    {
      return SqlDml.FunctionCall("DATETIME", date);
    }

    private static SqlCast CastToInt(SqlExpression arg)
    {
      return SqlDml.Cast(arg, SqlType.Int32);
    }

    private static SqlCast CastToLong(SqlExpression arg)
    {
      return SqlDml.Cast(arg, SqlType.Int64);
    }

    private static SqlExpression OffsetToOffsetAsString(SqlExpression offset)
    {
      var sign = '+';
      var offsetAsInt = offset as SqlLiteral<int>;
      var offsetAsDouble = offset as SqlLiteral<double>;
      if (offsetAsInt!=null) {
        if (offsetAsInt.Value < 0) {
          sign = '-';
          offset = -offset;
        }
      }
      else if (offsetAsDouble!=null) {
        if (offsetAsDouble.Value < 0) {
          sign = '-';
          offset = -offset;
        }
      }
      return SqlDml.Concat(sign, ToStringWithLeadZero(CastToInt(offset / 60), 2), ':', ToStringWithLeadZero(CastToInt(offset % 60), 2));
    }

    /// Truncate string from start, if length larger resultStringLength; Add lead zero, if length less resultStringLength
    /// (2, 3) => "002"; (41, 3) => "041", (4321, 3) => "321"
    private static SqlExpression ToStringWithLeadZero(SqlExpression expression, int resultStringLength)
    {
      return SqlDml.Substring(SqlDml.Concat(new String('0', resultStringLength), expression), -resultStringLength - 1, resultStringLength);
    }

    private static SqlExpression DateTimeOffsetExtractDateTimeAsString(SqlExpression dateTimeOffset)
    {
      return SqlDml.Substring(dateTimeOffset, 0, 19);
    }

    private static SqlExpression DateTimeOffsetExtractOffsetAsString(SqlExpression dateTimeOffset)
    {
      return SqlDml.Substring(dateTimeOffset, 19);
    }

    private static SqlExpression DateTimeOffsetExtractOffsetAsTotalNanoseconds(SqlExpression dateTimeOffset)
    {
      return DateTimeSubtractDateTime(DateTimeOffsetExtractDateTimeAsString(dateTimeOffset), dateTimeOffset);
    }

    private static SqlExpression DateTimeOffsetToUtcDateTime(SqlExpression dateTimeOffset)
    {
      return SqlDml.FunctionCall("DATETIME", dateTimeOffset, "UTC");
    }

    private static SqlExpression DateTimeOffsetToLocalDateTime(SqlExpression dateTimeOffset)
    {
      return SqlDml.FunctionCall("DATETIME", dateTimeOffset, "LOCALTIME");
    }

    private static SqlExpression DateTimeToStringIso(SqlExpression dateTime)
    {
      return SqlDml.FunctionCall("STRFTIME", "%Y-%m-%dT%H:%M:%S", dateTime);
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

    private static SqlExpression DateAddSeconds(SqlExpression date, SqlExpression seconds)
    {
      return SqlDml.FunctionCall("DATETIME", date, SqlDml.Concat(seconds, " ", "SECONDS"));
    }

    private static SqlExpression DateGetMilliseconds(SqlExpression date)
    {
      return CastToLong(SqlDml.FunctionCall("STRFTIME", "%f", date) * MillisecondsPerSecond) -
             CastToLong(SqlDml.FunctionCall("STRFTIME", "%S", date) * MillisecondsPerSecond);
    }

    private static SqlExpression DateGetTotalSeconds(SqlExpression date)
    {
      return SqlDml.FunctionCall("STRFTIME", "%s", date);
    }

    private static SqlExpression DateTimeSubtractDateTime(SqlExpression date1, SqlExpression date2)
    {
      return (DateGetTotalSeconds(date1) - DateGetTotalSeconds(date2)) * NanosecondsPerSecond;
      //             + DateGetMilliseconds(date1)
      //             - DateGetMilliseconds(date2);
    }

    private static SqlExpression ServerOffsetAsString()
    {
      const string constDateTime = "2016-01-01 12:00:00";
      return OffsetToOffsetAsString((SqlDml.FunctionCall("STRFTIME", "%s", constDateTime) - SqlDml.FunctionCall("STRFTIME", "%s", constDateTime, "UTC")) / 60);
    }

    private static SqlDateTimePart ConvertDateTimeOffsetPartToDateTimePart(SqlDateTimeOffsetPart dateTimeOffsetPart)
    {
      switch (dateTimeOffsetPart) {
      case SqlDateTimeOffsetPart.Year:
        return SqlDateTimePart.Year;
      case SqlDateTimeOffsetPart.Month:
        return SqlDateTimePart.Month;
      case SqlDateTimeOffsetPart.Day:
        return SqlDateTimePart.Day;
      case SqlDateTimeOffsetPart.Hour:
        return SqlDateTimePart.Hour;
      case SqlDateTimeOffsetPart.Minute:
        return SqlDateTimePart.Minute;
      case SqlDateTimeOffsetPart.Second:
        return SqlDateTimePart.Second;
      case SqlDateTimeOffsetPart.Millisecond:
        return SqlDateTimePart.Millisecond;
      case SqlDateTimeOffsetPart.Nanosecond:
        return SqlDateTimePart.Nanosecond;
      case SqlDateTimeOffsetPart.DayOfYear:
        return SqlDateTimePart.DayOfYear;
      case SqlDateTimeOffsetPart.DayOfWeek:
        return SqlDateTimePart.DayOfWeek;
      case SqlDateTimeOffsetPart.TimeZoneHour:
        return SqlDateTimePart.TimeZoneHour;
      case SqlDateTimeOffsetPart.TimeZoneMinute:
        return SqlDateTimePart.TimeZoneMinute;
      }
      throw SqlHelper.NotSupported(string.Format("Converting {0} to SqlDateTimePart", dateTimeOffsetPart));
    }

    // Constructors

    /// <param name="driver">The driver.</param>
    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
