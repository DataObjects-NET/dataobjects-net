// Copyright (C) 2011-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    private static readonly long NanosecondsPerDay = (long) TimeSpan.FromDays(1).TotalMilliseconds * NanosecondsPerMillisecond;
    private static readonly long NanosecondsPerHour = (long) TimeSpan.FromHours(1).TotalMilliseconds * NanosecondsPerMillisecond;
    private static readonly long NanosecondsPerSecond = (long) TimeSpan.FromSeconds(1).TotalMilliseconds * NanosecondsPerMillisecond;
    private static readonly long MillisecondsPerSecond = (long) TimeSpan.FromSeconds(1).TotalMilliseconds;
    private static readonly int StartOffsetIndex = DateTimeOffsetExampleString.IndexOf('+');

    private const long NanosecondsPerMillisecond = 1000000L;
    private const string DateFormat = "%Y-%m-%d 00:00:00.000";
    private const string DateTimeFormat = "%Y-%m-%d %H:%M:%f";
    private const string DateTimeIsoFormat = "%Y-%m-%dT%H:%M:%S";
    private const string DateTimeOffsetExampleString = "2001-02-03 04:05:06.789+02.45";

    protected override bool VisitCreateTableConstraints(SqlCreateTable node, IEnumerable<TableConstraint> constraints, bool hasItems)
    {
      // SQLite has special syntax for autoincrement primary keys
      // We write everything when doing translation for column definition
      // and should skip any PK definitions here.
      var hasAutoIncrementColumn = node.Table.TableColumns.Any(c => c.SequenceDescriptor!=null);
      constraints = hasAutoIncrementColumn ? constraints.Where(c => !(c is PrimaryKey)) : constraints;
      return base.VisitCreateTableConstraints(node, constraints, hasItems);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override void Visit(SqlAlterTable node)
    {
      if (node.Action is SqlRenameColumn renameColumnAction) {
        _ = context.Output.Append(((Translator) translator).Translate(context, renameColumnAction));
      }
      else if (node.Action is SqlDropConstraint) {
        using (context.EnterScope(node)) {
          AppendTranslatedEntry(node);

          var action = node.Action as SqlDropConstraint;
          var constraint = action.Constraint as TableConstraint;
          AppendTranslated(node, AlterTableSection.DropConstraint);
          if (constraint is ForeignKey) {
            _ = context.Output.Append("REFERENCES ");
            translator.TranslateIdentifier(context.Output, constraint.DbName);
          }
          else {
            AppendTranslated(constraint, ConstraintSection.Entry);
          }
          AppendTranslated(node, AlterTableSection.DropBehavior);
          AppendTranslatedExit(node);
        }
      }
      else
        base.Visit(node);
    }

    /// <inheritdoc/>
    public override void Visit(SqlExtract node)
    {
      if (node.IntervalPart != SqlIntervalPart.Nothing) {
        VisitInterval(node);
        return;
      }
      if (node.DateTimePart != SqlDateTimePart.Nothing) {
        VisitDateTime(node);
        return;
      }
      if (node.DateTimeOffsetPart != SqlDateTimeOffsetPart.Nothing) {
        VisitDateTimeOffset(node);
        return;
      }
      base.Visit(node);
    }

    /// <inheritdoc/>
    public override void Visit(SqlFreeTextTable node) => throw SqlHelper.NotSupported("FreeText");

    /// <inheritdoc/>
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
          if (node.Arguments.Count == 1) {
            Visit(SqlDml.FunctionCall(translator.TranslateToString(SqlFunctionType.Round), node.Arguments[0], SqlDml.Literal(0)));
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
        case SqlFunctionType.DateTimeToDateTimeOffset:
          SqlDml.Concat(DateTime(node.Arguments[0]), ServerOffsetAsString()).AcceptVisitor(this);
          return;
      }
      base.Visit(node);
    }

    /// <inheritdoc/>
    public override void Visit(SqlQueryExpression node)
    {
      using (context.EnterScope(node)) {
        AppendTranslatedEntry(node);
        node.Left.AcceptVisitor(this);
        AppendTranslated(node.NodeType);
        AppendTranslated(node, QueryExpressionSection.All);
        node.Right.AcceptVisitor(this);
        AppendTranslatedExit(node);
      }
    }

    /// <inheritdoc/>
    public override void Visit(SqlSelect node)
    {
      // For hinting limitations see http://www.sqlite.org/lang_indexedby.html

      using (context.EnterScope(node)) {
        var comment = node.Comment;
        VisitCommentIfBefore(comment);
        AppendTranslatedEntry(node);
        VisitCommentIfWithin(comment);
        VisitSelectColumns(node);
        VisitSelectFrom(node);
        VisitSelectWhere(node);
        VisitSelectGroupBy(node);
        VisitSelectOrderBy(node);
        VisitSelectLimitOffset(node);
        AppendTranslatedExit(node);
        VisitCommentIfAfter(comment);
      }
    }

    /// <inheritdoc/>
    public override void Visit(SqlTrim node)
    {
      using (context.EnterScope(node)) {
        var output = context.Output;

        AppendTranslated(node, TrimSection.Entry);
        translator.Translate(output, node.TrimType);
        node.Expression.AcceptVisitor(this);
        if (node.TrimCharacters!=null) {
          _ = output.Append(",");
          AppendTranslatedLiteral(node.TrimCharacters);
        }
        AppendTranslated(node, TrimSection.Exit);
      }
    }

    /// <inheritdoc/>
    protected override void VisitSelectLimitOffset(SqlSelect node)
    {
      // SQLite requires limit to be present if offset it used,
      // luckily negatives value does the job.

      var isSpecialCase = !node.HasLimit && node.HasOffset;

      if (!isSpecialCase) {
        base.VisitSelectLimitOffset(node);
        return;
      }

      AppendTranslated(node, SelectSection.Limit);
      SqlDml.Literal(-1).AcceptVisitor(this);
      AppendTranslated(node, SelectSection.LimitEnd);

      AppendTranslated(node, SelectSection.Offset);
      node.Offset.AcceptVisitor(this);
      AppendTranslated(node, SelectSection.OffsetEnd);
    }

    /// <inheritdoc/>
    public override void Visit(SqlCreateIndex node, IndexColumn item)
    {
      base.Visit(node, item);

      if (item.Column is TableColumn column && column.Collation != null) {
        AppendTranslated(column, TableColumnSection.Collate);
      }
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
          DateTimeOffsetExtractOffsetAsTotalNanoseconds(node.Operand).AcceptVisitor(this);
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

    private static SqlExpression DateTimeAddInterval(SqlExpression date, SqlExpression interval) =>
      DateAddSeconds(date, interval / Convert.ToDouble(NanosecondsPerSecond));

    private static SqlExpression DateTimeTruncate(SqlExpression date) =>
      DateTime(SqlDml.FunctionCall("STRFTIME", DateFormat, date));

    private static SqlExpression DateTime(SqlExpression date) => SqlDml.FunctionCall("STRFTIME", DateTimeFormat, date);

    private static SqlCast CastToInt(SqlExpression arg) => SqlDml.Cast(arg, SqlType.Int32);

    private static SqlCast CastToLong(SqlExpression arg) => SqlDml.Cast(arg, SqlType.Int64);

    private static SqlExpression OffsetToOffsetAsString(SqlExpression offset)
    {
      var sign = '+';
      var offsetAsInt = offset as SqlLiteral<int>;
      var offsetAsDouble = offset as SqlLiteral<double>;
      if (offsetAsInt != null) {
        if (offsetAsInt.Value < 0) {
          sign = '-';
          offset = -offset;
        }
      }
      else if (offsetAsDouble != null) {
        if (offsetAsDouble.Value < 0) {
          sign = '-';
          offset = -offset;
        }
      }
      return SqlDml.Concat(sign, ToStringWithLeadZero(CastToInt(offset / 60), 2), ':', ToStringWithLeadZero(CastToInt(offset % 60), 2));
    }

    /// Truncate string from start, if length larger resultStringLength; Add lead zero, if length less resultStringLength
    /// (2, 3) => "002"; (41, 3) => "041", (4321, 3) => "321"
    private static SqlExpression ToStringWithLeadZero(SqlExpression expression, int resultStringLength) =>
      SqlDml.Substring(SqlDml.Concat(new string('0', resultStringLength), expression), -resultStringLength - 1, resultStringLength);

    private static SqlExpression DateTimeOffsetExtractDateTimeAsString(SqlExpression dateTimeOffset) =>
      SqlDml.Substring(dateTimeOffset, 0, StartOffsetIndex);

    private static SqlExpression DateTimeOffsetExtractOffsetAsString(SqlExpression dateTimeOffset) =>
      SqlDml.Substring(dateTimeOffset, StartOffsetIndex);

    private static SqlExpression DateTimeOffsetExtractOffsetAsTotalNanoseconds(SqlExpression dateTimeOffset) =>
      DateTimeSubtractDateTime(DateTimeOffsetExtractDateTimeAsString(dateTimeOffset), dateTimeOffset);

    private static SqlExpression DateTimeOffsetToUtcDateTime(SqlExpression dateTimeOffset) =>
      SqlDml.FunctionCall("STRFTIME", DateTimeFormat, dateTimeOffset, "LOCALTIME", "UTC");

    private static SqlExpression DateTimeOffsetToLocalDateTime(SqlExpression dateTimeOffset) =>
      SqlDml.FunctionCall("STRFTIME", DateTimeFormat, dateTimeOffset, "LOCALTIME");

    private static SqlExpression DateTimeToStringIso(SqlExpression dateTime) =>
      SqlDml.FunctionCall("STRFTIME", DateTimeIsoFormat, dateTime);

    private static SqlExpression DateAddYear(SqlExpression date, SqlExpression years) =>
      SqlDml.FunctionCall("STRFTIME", DateTimeFormat, date, SqlDml.Concat(years, " ", "YEARS"));


    private static SqlExpression DateAddMonth(SqlExpression date, SqlExpression months) =>
      SqlDml.FunctionCall("STRFTIME", DateTimeFormat, date, SqlDml.Concat(months, " ", "MONTHS"));

    private static SqlExpression DateAddDay(SqlExpression date, SqlExpression days) =>
      SqlDml.FunctionCall("STRFTIME", DateTimeFormat, date, SqlDml.Concat(days, " ", "DAYS"));


    private static SqlExpression DateAddSeconds(SqlExpression date, SqlExpression seconds) =>
      SqlDml.FunctionCall("STRFTIME", DateTimeFormat, date, SqlDml.Concat(seconds, " ", "SECONDS"));

    private static SqlExpression DateGetMilliseconds(SqlExpression date) =>
      CastToLong(SqlDml.FunctionCall("STRFTIME", "%f", date) * MillisecondsPerSecond) -
        CastToLong(SqlDml.FunctionCall("STRFTIME", "%S", date) * MillisecondsPerSecond);


    private static SqlExpression DateGetTotalSeconds(SqlExpression date) =>
      SqlDml.FunctionCall("STRFTIME", "%s", date);

    private static SqlExpression DateTimeSubtractDateTime(SqlExpression date1, SqlExpression date2) =>
      (((DateGetTotalSeconds(date1) - DateGetTotalSeconds(date2)) * MillisecondsPerSecond)
        + DateGetMilliseconds(date1) - DateGetMilliseconds(date2)) * NanosecondsPerMillisecond;


    private static SqlExpression ServerOffsetAsString()
    {
      const string constDateTime = "2016-01-01 12:00:00";
      return OffsetToOffsetAsString((SqlDml.FunctionCall("STRFTIME", "%s", constDateTime) - SqlDml.FunctionCall("STRFTIME", "%s", constDateTime, "UTC")) / 60);
    }

    private static SqlDateTimePart ConvertDateTimeOffsetPartToDateTimePart(SqlDateTimeOffsetPart dateTimeOffsetPart)
    {
      return dateTimeOffsetPart switch {
        SqlDateTimeOffsetPart.Year => SqlDateTimePart.Year,
        SqlDateTimeOffsetPart.Month => SqlDateTimePart.Month,
        SqlDateTimeOffsetPart.Day => SqlDateTimePart.Day,
        SqlDateTimeOffsetPart.Hour => SqlDateTimePart.Hour,
        SqlDateTimeOffsetPart.Minute => SqlDateTimePart.Minute,
        SqlDateTimeOffsetPart.Second => SqlDateTimePart.Second,
        SqlDateTimeOffsetPart.Millisecond => SqlDateTimePart.Millisecond,
        SqlDateTimeOffsetPart.Nanosecond => SqlDateTimePart.Nanosecond,
        SqlDateTimeOffsetPart.DayOfYear => SqlDateTimePart.DayOfYear,
        SqlDateTimeOffsetPart.DayOfWeek => SqlDateTimePart.DayOfWeek,
        SqlDateTimeOffsetPart.TimeZoneHour => SqlDateTimePart.TimeZoneHour,
        SqlDateTimeOffsetPart.TimeZoneMinute => SqlDateTimePart.TimeZoneMinute,
        _ => throw SqlHelper.NotSupported($"Converting {dateTimeOffsetPart} to SqlDateTimePart"),
      };
    }

    // Constructors

    /// <param name="driver">The driver.</param>
    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
