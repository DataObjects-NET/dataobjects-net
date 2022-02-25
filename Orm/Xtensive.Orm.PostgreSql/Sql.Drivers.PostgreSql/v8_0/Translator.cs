// Copyright (C) 2012-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Diagnostics;
using System.Text;
using NpgsqlTypes;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Model;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Drivers.PostgreSql.Resources;
using Index = Xtensive.Sql.Model.Index;
using Xtensive.Reflection.PostgreSql;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_0
{
  internal class Translator : SqlTranslator
  {
    /// <inheritdoc/>
    public override string DateTimeFormatString => @"\'yyyyMMdd HHmmss.ffffff\''::timestamp(6)'";

    /// <inheritdoc/>
    public override string TimeSpanFormatString => "'{0}{1} days {0}{2}:{3}:{4}.{5:000}'::interval";

    /// <inheritdoc/>
    public override string DdlStatementDelimiter => ";";

    public string DateTimeOffsetFormatString => @"\'yyyyMMdd HHmmss.ffffff zzz\''::timestamp(6) with time zone'";

    public override void Initialize()
    {
      base.Initialize();
      FloatNumberFormat.NumberDecimalSeparator = ".";
      FloatNumberFormat.NumberGroupSeparator = "";
      FloatNumberFormat.NaNSymbol = "'Nan'::float4";
      FloatNumberFormat.NegativeInfinitySymbol = "'-Infinity'::float4";
      FloatNumberFormat.PositiveInfinitySymbol = "'Infinity'::float4";

      DoubleNumberFormat.NumberDecimalSeparator = ".";
      DoubleNumberFormat.NumberGroupSeparator = "";
      DoubleNumberFormat.NaNSymbol = "'Nan'::float8";
      DoubleNumberFormat.NegativeInfinitySymbol = "'-Infinity'::float8";
      DoubleNumberFormat.PositiveInfinitySymbol = "'Infinity'::float8";
    }

    /// <inheritdoc/>
    public override SqlHelper.EscapeSetup EscapeSetup => SqlHelper.EscapeSetup.WithQuotes;

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override string QuoteString(string str) =>
      "'" + str.Replace("'", "''").Replace(@"\", @"\\").Replace("\0", string.Empty) + "'";

    /// <inheritdoc/>
    protected override void TranslateChar(IOutput output, char ch)
    {
      switch (ch) {
        case '\\':
          _ = output.AppendLiteral("\\\\");
          break;
        default:
          base.TranslateChar(output, ch);
          break;
      }
    }

    /// <inheritdoc/>
    public override string Translate(SqlFunctionType type)
    {
      return type switch {
        SqlFunctionType.SystemUser => string.Empty,
        SqlFunctionType.User or SqlFunctionType.CurrentUser => "current_user",
        SqlFunctionType.SessionUser => "session_user",
        SqlFunctionType.NullIf => "nullif",
        SqlFunctionType.Coalesce => "coalesce",
        SqlFunctionType.BinaryLength => "length",
        //datetime/timespan
        SqlFunctionType.CurrentDate => "date_trunc('day', current_timestamp)",
        SqlFunctionType.CurrentTimeStamp => "current_timestamp",
        SqlFunctionType.IntervalNegate => "-",
        //string
        SqlFunctionType.CharLength => "char_length",
        SqlFunctionType.Lower => "lower",
        SqlFunctionType.Position => "position",
        SqlFunctionType.Substring => "substring",
        SqlFunctionType.Upper => "upper",
        SqlFunctionType.Concat => "textcat",
        //math
        SqlFunctionType.Abs => "abs",
        SqlFunctionType.Acos => "acos",
        SqlFunctionType.Asin => "asin",
        SqlFunctionType.Atan => "atan",
        SqlFunctionType.Atan2 => "atan2",
        SqlFunctionType.Ceiling => "ceil",
        SqlFunctionType.Cos => "cos",
        SqlFunctionType.Cot => "cot",
        SqlFunctionType.Degrees => "degrees",
        SqlFunctionType.Exp => "exp",
        SqlFunctionType.Floor => "floor",
        SqlFunctionType.Log => "ln",
        SqlFunctionType.Log10 => "log",
        SqlFunctionType.Pi => "pi",
        SqlFunctionType.Power => "power",
        SqlFunctionType.Radians => "radians",
        SqlFunctionType.Rand => "random",
        SqlFunctionType.Round => "round",
        SqlFunctionType.Truncate => "trunc",
        SqlFunctionType.Sign => "sign",
        SqlFunctionType.Sqrt => "sqrt",
        SqlFunctionType.Tan => "tan",
        _ => base.Translate(type),
      };
    }

    /// <inheritdoc/>
    public override string Translate(ReferentialAction action)
    {
      return action switch {
        ReferentialAction.Cascade => "CASCADE",
        ReferentialAction.NoAction => "NO ACTION",
        ReferentialAction.Restrict => "RESTRICT",
        ReferentialAction.SetDefault => "SET DEFAULT",
        ReferentialAction.SetNull => "SET NULL",
        _ => string.Empty,
      };
    }

    /// <inheritdoc/>
    public override string Translate(SqlNodeType type)
    {
      return type switch {
        SqlNodeType.BitXor => "#",
        SqlNodeType.Modulo => "%",
        SqlNodeType.Overlaps => "OVERLAPS",
        SqlNodeType.DateTimePlusInterval => "+",
        SqlNodeType.DateTimeMinusInterval or SqlNodeType.DateTimeMinusDateTime => "-",
        _ => base.Translate(type),
      };
    }

    /// <inheritdoc/>
    public override string Translate(SqlMatchType mt)
    {
      return mt switch {
        SqlMatchType.Full => "FULL",
        _ => "SIMPLE",
      };
    }

    /// <inheritdoc/>
    public override string TranslateToString(SqlCompilerContext context, SchemaNode node)
    {
      //temporary tables need no schema qualifier
      if (!(node is TemporaryTable) && node.Schema != null) {
        return context == null
          ? QuoteIdentifier(new[] { node.Schema.Name, node.Name })
          : QuoteIdentifier(new[] { context.SqlNodeActualizer.Actualize(node.Schema), node.Name });

      }
      return QuoteIdentifier(new[] { node.Name });
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SchemaNode node) =>
      context.Output.Append(TranslateToString(context, node));

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlCreateTable node, CreateTableSection section)
    {
      switch (section) {
        case CreateTableSection.Exit:
          _ = context.Output.Append("WITHOUT OIDS ");
          break;
      }
      base.Translate(context, node, section);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, TableColumn column, TableColumnSection section)
    {
      switch (section) {
        case TableColumnSection.Exit:
        case TableColumnSection.SetIdentityInfoElement:
        case TableColumnSection.GenerationExpressionExit:
        case TableColumnSection.Collate:
          break;
        default:
          base.Translate(context, column, section);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlCreateIndex node, CreateIndexSection section)
    {
      var output = context.Output;
      var index = node.Index;
      switch (section) {
        case CreateIndexSection.Entry:
          _ = index.IsUnique
            ? output.Append("CREATE UNIQUE INDEX ")
            : output.Append("CREATE INDEX ");
          TranslateIdentifier(output, index.Name);
          _ = output.Append(" ON ");
          Translate(context, index.DataTable);
          if (index.IsSpatial) {
            _ = output.Append(" USING GIST");
          }
          _ = output.Append("(");
          break;
        case CreateIndexSection.StorageOptions:
          _ = output.Append(")");
          AppendIndexStorageParameters(output, index);
          if (!string.IsNullOrEmpty(index.Filegroup)) {
            _ = output.Append(" TABLESPACE ");
            TranslateIdentifier(output, index.Filegroup);
          }

          break;
        case CreateIndexSection.Exit:
          break;
        case CreateIndexSection.Where:
          _ = output.Append(" WHERE ");
          break;
        default:
          break;
          ;
      }
    }
    protected virtual void AppendIndexStorageParameters(IOutput output, Index index)
    {
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlDropIndex node)
    {
      _ = context.Output.Append("DROP INDEX ");
      TranslateIdentifier(context.Output,
        context.SqlNodeActualizer.Actualize(node.Index.DataTable.Schema), node.Index.Name);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlBreak node) => context.Output.Append("EXIT");

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlContinue node) { }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlDeclareVariable node) { }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlAssignment node, NodeSection section)
    {
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, object literalValue)
    {
      var output = context.Output;
      var literalType = literalValue.GetType();
      if (literalType == WellKnownTypes.ByteArrayType) {
        _ = output.Append(TranslateByteArrayLiteral((byte[]) literalValue));
      }
      else if (literalType == WellKnownTypes.GuidType) {
        TranslateString(output, SqlHelper.GuidToString((Guid) literalValue));
      }
      else if (literalType == WellKnownTypes.DateTimeOffsetType) {
        _ = output.Append(((DateTimeOffset) literalValue).ToString(DateTimeOffsetFormatString));
      }
      else if (literalType == WellKnownTypes.NpgsqlPointType) {
        var point = (NpgsqlPoint) literalValue;
        _ = output.Append($"point'({point.X},{point.Y})'");
      }
      else if (literalType == WellKnownTypes.NpgsqlLSegType) {
        var lSeg = (NpgsqlLSeg) literalValue;
        _ = output.Append($"lseg'[({lSeg.Start.X},{lSeg.Start.Y}),({lSeg.End.X},{lSeg.End.Y})]'");
      }
      else if (literalType == WellKnownTypes.NpgsqlBoxType) {
        var box = (NpgsqlBox) literalValue;
        _ = output.Append($"box'({box.LowerLeft.X},{box.LowerLeft.Y}),({box.UpperRight.X},{box.UpperRight.Y})'");
      }
      else if (literalType == WellKnownTypes.NpgsqlPathType) {
        _ = output.Append($"path'((0,0))'");
      }
      else if (literalType == WellKnownTypes.NpgsqlPolygonType) {
        _ = output.Append("polygon'((0,0))'");
      }
      else if (literalType == WellKnownTypes.NpgsqlCircleType) {
        var circle = (NpgsqlCircle) literalValue;
        _ = output.Append($"circle'<({circle.Center.X},{circle.Center.Y}),{circle.Radius}>'");
      }
      else {
        base.Translate(context, literalValue);
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlArray node, ArraySection section)
    {
      _ = context.Output.Append(section switch {
        ArraySection.Entry => "ARRAY[",
        ArraySection.Exit => "]",
        ArraySection.EmptyArray => $"'{{}}'::{TranslateClrType(node.ItemType)}[]",
        _ => throw new ArgumentOutOfRangeException(nameof(section))
      });
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlExtract node, ExtractSection section)
    {
      var isSecond = node.DateTimePart == SqlDateTimePart.Second
        || node.IntervalPart == SqlIntervalPart.Second
        || node.DateTimeOffsetPart == SqlDateTimeOffsetPart.Second;
      var isMillisecond = node.DateTimePart == SqlDateTimePart.Millisecond
        || node.IntervalPart == SqlIntervalPart.Millisecond
        || node.DateTimeOffsetPart == SqlDateTimeOffsetPart.Millisecond;
      if (!(isSecond || isMillisecond)) {
        base.Translate(context, node, section);
        return;
      }
      switch (section) {
        case ExtractSection.Entry:
          _ = context.Output.Append(isSecond ? "(trunc(extract(" : "(extract(");
          break;
        case ExtractSection.Exit:
          _ = context.Output.Append(isMillisecond
           ? ")::int8 % 1000)"
           : isSecond ? ")))" : ")::int8)"
          );
          break;
        default:
          base.Translate(context, node, section);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlDeclareCursor node, DeclareCursorSection section)
    {
      var output = context.Output;
      switch (section) {
        case DeclareCursorSection.Entry:
          _ = output.Append("DECLARE ");
          TranslateIdentifier(output, node.Cursor.Name);
          break;
        case DeclareCursorSection.Sensivity:
          break;
        case DeclareCursorSection.Scrollability:
          _ = output.Append(node.Cursor.Scroll ? "SCROLL" : "NO SCROLL");
          break;
        case DeclareCursorSection.Cursor:
          _ = output.Append("CURSOR");
          break;
        case DeclareCursorSection.Holdability:
          _ = output.Append(node.Cursor.WithHold ? "WITH HOLD" : "");
          break;
        case DeclareCursorSection.Returnability:
        case DeclareCursorSection.Updatability:
        case DeclareCursorSection.Exit:
          break;
        case DeclareCursorSection.For:
          _ = output.Append("FOR");
          break;
        default:
          base.Translate(context, node, section);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlFetch node, FetchSection section)
    {
      switch (section) {
        case FetchSection.Entry:
          _ = context.Output.Append("FETCH ").Append(node.Option.ToString().ToUpper());
          return;
        case FetchSection.Targets:
          var output = context.Output;
          _ = output.Append("FROM ");
          TranslateIdentifier(output, node.Cursor.Name);
          return;
        case FetchSection.Exit:
          break;
      }
      base.Translate(context, node, section);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlOpenCursor node)
    {
      // DECLARE CURSOR already opens it
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlMatch node, MatchSection section)
    {
      switch (section) {
        case MatchSection.Entry:
          //MATCH is not supported by PostgreSQL, we need some workaround
          if (node.Value is SqlRow row) {
            var finalQuery = SqlDml.Select();
            finalQuery.Columns.Add(5);
            switch (node.MatchType) {
              case SqlMatchType.None: {
                BuildSelectForUniqueMatchNone(node, row, finalQuery);
                break;
              }
              case SqlMatchType.Full: {
                BuildSelectForUniqueMatchFull(node, row, finalQuery);
                break;
              }
              case SqlMatchType.Partial: {
                BuildSelectForUniqueMatchPartial(node, row, finalQuery);
                break;
              }
            }
            var newNode = SqlDml.Match(SqlDml.Row(), SqlDml.SubQuery(finalQuery).Query, node.Unique, node.MatchType);
            node.ReplaceWith(newNode);
            _ = context.Output.Append("EXISTS(SELECT '");
            break;
          }
          else {
            throw new InvalidOperationException(Strings.ExSqlMatchValueMustBeAnSqlRowInstance);
          }
        case MatchSection.Specification:
          _ = context.Output.Append("' WHERE EXISTS");
          break;
        case MatchSection.Exit:
          _ = context.Output.Append(")");
          break;
      }
    }

    private void BuildSelectForUniqueMatchNone(SqlMatch node, SqlRow row, SqlSelect finalQuery)
    {
      var existsNull = false;
      var @case = SqlDml.Case();
      var subQueryNeeded = true;
      //if any of the row elements is NULL then true
      if (row.Count > 0) {
        var allLiteral = true; //if true then there is no NULL element
        SqlExpression when = null;
        for (var i = 0; i < row.Count; i++) {
          var elementIsNotLiteral = row[i].NodeType != SqlNodeType.Literal;
          //if the row element is the NULL value
          if (row[i].NodeType == SqlNodeType.Null) {
            existsNull = true;
            break;
          }
          if (allLiteral && elementIsNotLiteral) {
            allLiteral = false;
          }
          if (elementIsNotLiteral) {
            when = when == null
              ? SqlDml.IsNull(row[i])
              : when || SqlDml.IsNull(row[i]);
          }
        }
        if (existsNull) {
          //Some row element is the NULL value, MATCH result is true
          subQueryNeeded = false;
        }
        else if (allLiteral) {
          //No row element is the NULL value
          subQueryNeeded = true;
        }
        else { //(!whenNotNeeded)
               //Check if any row element is NULL
          _ = @case.Add(when == null ? true : when, true);
          subQueryNeeded = true;
        }
      }
      //find row in subquery
      if (subQueryNeeded) {
        var originalQuery = SqlDml.QueryRef(node.SubQuery.Query);
        var subquery = SqlDml.Select(originalQuery);
        subquery.Columns.Add(1);
        var columns = originalQuery.Columns;
        SqlExpression where = null;
        for (var i = 0; i < columns.Count; i++) {
          if (i == 0) {
            where = columns[i] == row[i];
          }
          else {
            where = where && columns[i] == row[i];
          }
          if (node.Unique) {
            subquery.GroupBy.Add(columns[i]);
          }
        }
        subquery.Where = where;
        if (node.Unique) {
          subquery.Having = SqlDml.Count(SqlDml.Asterisk) == 1;
        }
        //c.Add(Sql.Exists(q1), true);
        @case.Else = SqlDml.Exists(subquery);
      }
      if (@case.Else == null) {
        @case.Else = false;
      }
      if (existsNull) {
        finalQuery.Where = null;
      }
      else if (@case.Count > 0) {
        finalQuery.Where = @case;
      }
      else {
        finalQuery.Where = @case.Else;
      }
    }

    private void BuildSelectForUniqueMatchFull(SqlMatch node, SqlRow row, SqlSelect finalQuery)
    {
      var @case = SqlDml.Case();
      var noMoreWhenNeeded = false;
      var allNull = true;
      SqlExpression when1 = true;
      //if all row elements are null then true
      if (row.Count > 0) {
        var whenNotNeeded = false;
        for (var i = 0; i < row.Count; i++) {
          //if any row element is surely not the NULL value
          if (row[i].NodeType == SqlNodeType.Literal) {
            whenNotNeeded = true;
            break;
          }
          if (allNull && row[i].NodeType != SqlNodeType.Null) {
            allNull = false;
          }
          if (i == 0) {
            when1 = SqlDml.IsNull(row[i]);
          }
          else {
            when1 = when1 && SqlDml.IsNull(row[i]);
          }
        }
        if (allNull) {
          when1 = true;
        }
        if (!whenNotNeeded) {
          _ = @case.Add(when1, true);
        }
      }
      if (!noMoreWhenNeeded) {
        var whenNotNeeded = false;
        var allLiteral = true;
        SqlExpression when2 = true;
        //if no row elements are null then subcase
        for (var i = 0; i < row.Count; i++) {
          if (row[i].NodeType == SqlNodeType.Null) {
            whenNotNeeded = true;
            when2 = false;
            break;
          }
          if (allLiteral && row[i].NodeType != SqlNodeType.Literal) {
            allLiteral = false;
          }
          if (i == 0) {
            when2 = SqlDml.IsNotNull(row[i]);
          }
          else {
            when2 = when2 && SqlDml.IsNotNull(row[i]);
          }
        }
        if (allLiteral) {
          when2 = true;
        }
        if (!whenNotNeeded) {
          //find row in subquery
          var originalQuery = SqlDml.QueryRef(node.SubQuery.Query);
          var subQuery = SqlDml.Select(originalQuery);
          subQuery.Columns.Add(1);

          var columns = originalQuery.Columns;
          SqlExpression where = null;
          for (var i = 0; i < columns.Count; i++) {
            if (i == 0) {
              where = columns[i] == row[i];
            }
            else {
              where = where && columns[i] == row[i];
            }
            if (node.Unique) {
              subQuery.GroupBy.Add(columns[i]);
            }
          }
          subQuery.Where = where;
          if (node.Unique) {
            subQuery.Having = SqlDml.Count(SqlDml.Asterisk) == 1;
          }
          _ = @case.Add(when2, SqlDml.Exists(subQuery));
        }
      }
      //else false
      @case.Else = false;
      finalQuery.Where = @case.Count > 0 ? @case : (SqlExpression) false;
    }

    private void BuildSelectForUniqueMatchPartial(SqlMatch node, SqlRow row, SqlSelect finalQuery)
    {
      var allNull = true;
      var @case = SqlDml.Case();
      SqlExpression when = true;
      //if all row elements are null then true
      if (row.Count > 0) {
        var whenNotNeeded = false;
        for (var i = 0; i < row.Count; i++) {
          //if any row element is surely not the NULL value
          if (row[i].NodeType == SqlNodeType.Literal) {
            allNull = false;
            whenNotNeeded = true;
            break;
          }
          if (allNull && row[i].NodeType != SqlNodeType.Null) {
            allNull = false;
          }
          when = i == 0
            ? SqlDml.IsNull(row[i])
            : when && SqlDml.IsNull(row[i]);
        }
        if (allNull) {
          when = true;
        }
        if (!whenNotNeeded) {
          _ = @case.Add(when, true);
        }
      }
      //otherwise
      if (!allNull) {
        //find row in subquery
        var originalQuery = SqlDml.QueryRef(node.SubQuery.Query);
        var subQuery = SqlDml.Select(originalQuery);
        subQuery.Columns.Add(8);
        var columns = originalQuery.Columns;
        SqlExpression where = null;
        for (var i = 0; i < columns.Count; i++) {
          //if row[i] would be NULL then c3 would result in true,
          if (row[i].NodeType != SqlNodeType.Null) {
            var c3 = SqlDml.Case();
            _ = c3.Add(SqlDml.IsNull(row[i]), true);
            c3.Else = row[i] == columns[i];

            where = where == null ? c3 : where && c3;
          }
          if (node.Unique) {
            var c4 = SqlDml.Case();
            _ = c4.Add(SqlDml.IsNull(row[i]), 0);
            c4.Else = columns[i];
            subQuery.GroupBy.Add(c4);
          }
        }
        subQuery.Where = where;
        if (node.Unique) {
          subQuery.Having = SqlDml.Count(SqlDml.Asterisk) == 1;
        }
        @case.Else = SqlDml.Exists(subQuery);
      }
      if (@case.Else == null) {
        @case.Else = false;
      }
      if (allNull) {
        finalQuery.Where = null;
      }
      else if (@case.Count > 0) {
        finalQuery.Where = @case;
      }
      else {
        finalQuery.Where = @case.Else;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlFunctionCall node, FunctionCallSection section, int position)
    {
      switch (section) {
        case FunctionCallSection.Entry:
          switch (node.FunctionType) {
            case SqlFunctionType.CurrentUser:
            case SqlFunctionType.SessionUser:
            case SqlFunctionType.SystemUser:
            case SqlFunctionType.User:
            case SqlFunctionType.CurrentDate:
            case SqlFunctionType.CurrentTimeStamp:
              _ = context.Output.Append(Translate(node.FunctionType));
              return;
          }
          break;
      }

      base.Translate(context, node, section, position);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlUnary node, NodeSection section)
    {
      //substitute UNIQUE predicate with a more complex EXISTS predicate,
      //because UNIQUE is not supported

      if (node.NodeType == SqlNodeType.Unique) {
        if (node.Operand is SqlSubQuery origSubselect) {
          var origQuery = SqlDml.QueryRef(origSubselect.Query);
          var existsOp = SqlDml.Select(origQuery);
          existsOp.Columns.Add(1);
          existsOp.Where = true;
          foreach (SqlColumn col in origQuery.Columns) {
            existsOp.Where = existsOp.Where && SqlDml.IsNotNull(col);
            existsOp.GroupBy.Add(col);
          }
          existsOp.Having = SqlDml.Count(SqlDml.Asterisk) > 1;
          existsOp.Limit = 1;

          node.ReplaceWith(SqlDml.Not(SqlDml.Exists(existsOp)));
        }
      }
      base.Translate(context, node, section);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlNextValue node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          _ = context.Output.Append("nextval('");
          break;
        case NodeSection.Exit:
          _ = context.Output.Append("')");
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlCast node, NodeSection section)
    {
      // casting this way behaves differently: -32768::int2 is out of range ! We need (-32768)::int2
      switch (section) {
        case NodeSection.Entry:
          _ = context.Output.Append("(");
          break;
        case NodeSection.Exit:
          _ = context.Output.Append(")::").Append(Translate(node.Type));
          break;
      }
    }

    /// <inheritdoc/>
    public override string Translate(SqlDateTimePart part)
    {
      return part switch {
        SqlDateTimePart.Millisecond => "MILLISECONDS",
        SqlDateTimePart.DayOfYear => "DOY",
        SqlDateTimePart.DayOfWeek => "DOW",
        _ => base.Translate(part),
      };
    }

    /// <inheritdoc/>
    public override string Translate(SqlDateTimeOffsetPart part)
    {
      return part switch {
        SqlDateTimeOffsetPart.Millisecond => "MILLISECONDS",
        SqlDateTimeOffsetPart.DayOfYear => "DOY",
        SqlDateTimeOffsetPart.DayOfWeek => "DOW",
        SqlDateTimeOffsetPart.Offset => "TIMEZONE",
        SqlDateTimeOffsetPart.TimeZoneHour => "TIMEZONE_HOUR",
        SqlDateTimeOffsetPart.TimeZoneMinute => "TIMEZONE_MINUTE",
        _ => base.Translate(part),
      };
    }

    /// <inheritdoc/>
    public override string Translate(SqlLockType lockType)
    {
      return lockType.Supports(SqlLockType.SkipLocked)
        || lockType.Supports(SqlLockType.Shared)
        || lockType.Supports(SqlLockType.ThrowIfLocked)
        ? base.Translate(lockType)
        : "FOR UPDATE";
    }

    protected virtual string TranslateClrType(Type type)
    {
      switch (Type.GetTypeCode(type)) {
        case TypeCode.Boolean:
          return "bool";
        case TypeCode.Byte:
        case TypeCode.SByte:
        case TypeCode.Int16:
          return "int2";
        case TypeCode.UInt16:
        case TypeCode.Int32:
          return "int4";
        case TypeCode.UInt32:
        case TypeCode.Int64:
          return "int8";
        case TypeCode.UInt64:
        case TypeCode.Decimal:
          return "numeric";
        case TypeCode.Single:
          return "float4";
        case TypeCode.Double:
          return "float8";
        case TypeCode.Char:
        case TypeCode.String:
          return "text";
        case TypeCode.DateTime:
          return "timestamp";
        default:
          if (type == WellKnownTypes.TimeSpanType) {
            return "interval";
          }
          if (type == WellKnownTypes.GuidType) {
            return "bytea";
          }
          return "text";
      }
    }

    private static string TranslateByteArrayLiteral(byte[] array)
    {
      if (array.Length == 0) {
        return "''::bytea";
      }

      var chars = new char[1 + (5 * array.Length) + 8];
      chars[0] = '\'';
      chars[^1] = 'a';
      chars[^2] = 'e';
      chars[^3] = 't';
      chars[^4] = 'y';
      chars[^5] = 'b';
      chars[^6] = ':';
      chars[^7] = ':';
      chars[^8] = '\'';

      for (int n = 1, i = 0; i < array.Length; i++, n += 5) {
        chars[n] = chars[n + 1] = '\\';
        chars[n + 2] = (char) ('0' + (7 & (array[i] >> 6)));
        chars[n + 3] = (char) ('0' + (7 & (array[i] >> 3)));
        chars[n + 4] = (char) ('0' + (7 & (array[i] >> 0)));
      }

      return new string(chars);
    }


    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
      FloatFormatString = base.FloatFormatString + "'::float4'";
      DoubleFormatString = base.DoubleFormatString + "'::float8'";
    }
  }
}
