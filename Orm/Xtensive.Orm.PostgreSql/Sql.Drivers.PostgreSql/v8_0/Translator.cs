// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Diagnostics;
using System.Text;
using NpgsqlTypes;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Model;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Drivers.PostgreSql.Resources;
using Xtensive.Reflection.PostgreSql;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_0
{
  internal class Translator : SqlTranslator
  {
    public override string DateTimeFormatString { get { return @"\'yyyyMMdd HHmmss.ffffff\''::timestamp(6)'"; } }
    public override string TimeSpanFormatString { get { return "'{0}{1} days {0}{2}:{3}:{4}.{5:000}'::interval"; } }

    public override string FloatFormatString { get { return base.FloatFormatString + "'::float4'"; } }
    public override string DoubleFormatString { get { return base.DoubleFormatString + "'::float8'"; } }

    public string DateTimeOffsetFormatString { get { return @"\'yyyyMMdd HHmmss.ffffff zzz\''::timestamp(6) with time zone'"; } }


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

    public override string DdlStatementDelimiter { get { return ";"; } }
    public override string BatchItemDelimiter { get { return ";\r\n"; } }

    [DebuggerStepThrough]
    public override string QuoteIdentifier(params string[] names)
    {
      return SqlHelper.QuoteIdentifierWithQuotes(names);
    }

    [DebuggerStepThrough]
    public override string QuoteString(string str)
    {
     return "'" + str.Replace("'", "''").Replace(@"\", @"\\").Replace("\0", string.Empty) + "'";
    }

    public override string Translate(SqlFunctionType type)
    {
      switch (type) {
        case SqlFunctionType.SystemUser:
          return string.Empty;
        case SqlFunctionType.User:
        case SqlFunctionType.CurrentUser:
          return "current_user";
        case SqlFunctionType.SessionUser:
          return "session_user";
        case SqlFunctionType.NullIf:
          return "nullif";
        case SqlFunctionType.Coalesce:
          return "coalesce";
        case SqlFunctionType.BinaryLength:
          return "length";

        //datetime/timespan

        case SqlFunctionType.CurrentDate:
          return "date_trunc('day', current_timestamp)";
        case SqlFunctionType.CurrentTimeStamp:
          return "current_timestamp";
        case SqlFunctionType.IntervalNegate:
          return "-";

        //string

        case SqlFunctionType.CharLength:
          return "char_length";
        case SqlFunctionType.Lower:
          return "lower";
        case SqlFunctionType.Position:
          return "position";
        case SqlFunctionType.Substring:
          return "substring";
        case SqlFunctionType.Upper:
          return "upper";
        case SqlFunctionType.Concat:
          return "textcat";

        //math

        case SqlFunctionType.Abs:
          return "abs";
        case SqlFunctionType.Acos:
          return "acos";
        case SqlFunctionType.Asin:
          return "asin";
        case SqlFunctionType.Atan:
          return "atan";
        case SqlFunctionType.Atan2:
          return "atan2";
        case SqlFunctionType.Ceiling:
          return "ceil";
        case SqlFunctionType.Cos:
          return "cos";
        case SqlFunctionType.Cot:
          return "cot";
        case SqlFunctionType.Degrees:
          return "degrees";
        case SqlFunctionType.Exp:
          return "exp";
        case SqlFunctionType.Floor:
          return "floor";
        case SqlFunctionType.Log:
          return "ln";
        case SqlFunctionType.Log10:
          return "log";
        case SqlFunctionType.Pi:
          return "pi";
        case SqlFunctionType.Power:
          return "power";
        case SqlFunctionType.Radians:
          return "radians";
        case SqlFunctionType.Rand:
          return "random";
        case SqlFunctionType.Round:
          return "round";
        case SqlFunctionType.Truncate:
          return "trunc";
        case SqlFunctionType.Sign:
          return "sign";
        case SqlFunctionType.Sqrt:
          return "sqrt";
        case SqlFunctionType.Tan:
          return "tan";

        default:
          return base.Translate(type);
      }
    }

    public override string Translate(ReferentialAction action)
    {
      switch (action) {
        case ReferentialAction.Cascade:
          return "CASCADE";
        case ReferentialAction.NoAction:
          return "NO ACTION";
        case ReferentialAction.Restrict:
          return "RESTRICT";
        case ReferentialAction.SetDefault:
          return "SET DEFAULT";
        case ReferentialAction.SetNull:
          return "SET NULL";
      }
      return string.Empty;
    }

    public override string Translate(SqlNodeType type)
    {
      switch (type) {
        case SqlNodeType.BitXor:
          return "#";
        case SqlNodeType.Modulo:
          return "%";
        case SqlNodeType.Overlaps:
          return "OVERLAPS";
        case SqlNodeType.DateTimePlusInterval:
          return "+";
        case SqlNodeType.DateTimeMinusInterval:
        case SqlNodeType.DateTimeMinusDateTime:
          return "-";
        default:
          return base.Translate(type);
      }
    }

    public override string Translate(SqlMatchType mt)
    {
      switch (mt) {
        case SqlMatchType.Full:
          return "FULL";
        default:
          return "SIMPLE";
      }
    }

    public override string Translate(SqlCompilerContext context, SchemaNode node)
    {
      //temporary tables need no schema qualifier
      if (!(node is TemporaryTable) && node.Schema!=null) {
        return context == null
          ? QuoteIdentifier(new[] {node.Schema.Name, node.Name})
          : QuoteIdentifier(new[] {context.SqlNodeActualizer.Actualize(node.Schema), node.Name});

      }
      return QuoteIdentifier(new[] {node.Name});
    }

    public override string Translate(SqlCompilerContext context, SqlCreateTable node, CreateTableSection section)
    {
      switch (section) {
        case CreateTableSection.Exit:
          return "WITHOUT OIDS " + base.Translate(context, node, section);
        default:
          return base.Translate(context, node, section);
      }
    }

    public override string Translate(SqlCompilerContext context, TableColumn column, TableColumnSection section)
    {
      switch (section) {
        case TableColumnSection.Exit:
        case TableColumnSection.SetIdentityInfoElement:
        case TableColumnSection.GenerationExpressionExit:
        case TableColumnSection.Collate:
          return string.Empty;
        default:
          return base.Translate(context, column, section);
      }
    }

    public override string Translate(SqlCompilerContext context, SqlCreateIndex node, CreateIndexSection section)
    {
      Index index = node.Index;
      switch (section) {
        case CreateIndexSection.Entry:
          return string.Format("CREATE {0}INDEX {1} ON {2} {3}("
            , index.IsUnique ? "UNIQUE " : string.Empty
            , QuoteIdentifier(index.Name)
            , Translate(context, index.DataTable)
            , index.IsSpatial ? "USING GIST" : string.Empty);
        case CreateIndexSection.StorageOptions:
          var builder = new StringBuilder(")");
          AppendIndexStorageParameters(builder, index);
          if (!string.IsNullOrEmpty(index.Filegroup))
            _ = builder.Append(" TABLESPACE " + QuoteIdentifier(index.Filegroup));
          return builder.ToString();
        case CreateIndexSection.Exit:
          return string.Empty;
        case CreateIndexSection.Where:
          return " WHERE ";
        default:
          return string.Empty;
      }
    }

    protected virtual void AppendIndexStorageParameters(StringBuilder builder, Index index)
    {
    }

    public override string Translate(SqlCompilerContext context, SqlDropIndex node)
    {
      var indexName = context == null
        ? QuoteIdentifier(node.Index.DataTable.Schema.Name, node.Index.Name)
        : QuoteIdentifier(context.SqlNodeActualizer.Actualize(node.Index.DataTable.Schema), node.Index.Name);
      return "DROP INDEX " + indexName;
    }

    public override string Translate(SqlCompilerContext context, SqlBreak node) => "EXIT";

    public override string Translate(SqlCompilerContext context, SqlContinue node) => string.Empty;

    public override string Translate(SqlCompilerContext context, SqlDeclareVariable node) => string.Empty;

    public override string Translate(SqlCompilerContext context, SqlAssignment node, NodeSection section)
    {
      return string.Empty;
    }
    
    public override string Translate(SqlCompilerContext context, object literalValue)
    {
      var literalType = literalValue.GetType();
      if (literalType == WellKnownTypes.ByteArrayType) {
        return TranslateByteArrayLiteral((byte[]) literalValue);
      }
      if (literalType == WellKnownTypes.GuidType) {
        return QuoteString(SqlHelper.GuidToString((Guid) literalValue));
      }
      if (literalType == WellKnownTypes.DateTimeOffsetType) {
        return ((DateTimeOffset) literalValue).ToString(DateTimeOffsetFormatString);
      }
      if (literalType == WellKnownTypes.NpgsqlPointType) {
        var point = (NpgsqlPoint) literalValue;
        return string.Format("point'({0},{1})'", point.X, point.Y);
      }
      if (literalType == WellKnownTypes.NpgsqlLSegType) {
        var lSeg = (NpgsqlLSeg) literalValue;
        return string.Format("lseg'[({0},{1}),({2},{3})]'", lSeg.Start.X, lSeg.Start.Y, lSeg.End.X, lSeg.End.Y);
      }
      if (literalType == WellKnownTypes.NpgsqlBoxType) {
        var box = (NpgsqlBox) literalValue;
        return string.Format("box'({0},{1}),({2},{3})'", box.LowerLeft.X, box.LowerLeft.Y, box.UpperRight.X, box.UpperRight.Y);
      }
      if (literalType == WellKnownTypes.NpgsqlPathType) {
        return string.Format("path'(({0},{1}))'", 0, 0);
      }
      if (literalType == WellKnownTypes.NpgsqlPolygonType) {
        return "polygon'((0,0))'";
      }
      if (literalType == WellKnownTypes.NpgsqlCircleType) {
        var circle = (NpgsqlCircle) literalValue;
        return string.Format("circle'<({0},{1}),{2}>'", circle.Center.X, circle.Center.Y, circle.Radius);
      }
      return base.Translate(context, literalValue);
    }

    public override string Translate(SqlCompilerContext context, SqlArray node, ArraySection section)
    {
      switch (section) {
        case ArraySection.Entry:
          return "ARRAY[";
        case ArraySection.Exit:
          return "]";
        case ArraySection.EmptyArray:
          return string.Format("'{{}}'::{0}[]", TranslateClrType(node.ItemType));
        default:
          throw new ArgumentOutOfRangeException("section");
      }
    }

    public override string Translate(SqlCompilerContext context, SqlExtract node, ExtractSection section)
    {
      var isSecond = node.DateTimePart == SqlDateTimePart.Second
        || node.IntervalPart == SqlIntervalPart.Second
        || node.DateTimeOffsetPart == SqlDateTimeOffsetPart.Second;
      var isMillisecond = node.DateTimePart == SqlDateTimePart.Millisecond
        || node.IntervalPart == SqlIntervalPart.Millisecond
        || node.DateTimeOffsetPart == SqlDateTimeOffsetPart.Millisecond;
      if (!(isSecond || isMillisecond)) {
        return base.Translate(context, node, section);
      }
      switch (section) {
        case ExtractSection.Entry:
          return isSecond ? "(trunc(extract(" : "(extract(";
        case ExtractSection.Exit:
          return isMillisecond
           ?  ")::int8 % 1000)"
           : isSecond ? ")))" : ")::int8)";
        default:
          return base.Translate(context, node, section);
      }
    }

    public override string Translate(SqlCompilerContext context, SqlDeclareCursor node, DeclareCursorSection section)
    {
      switch (section) {
        case DeclareCursorSection.Entry:
          return "DECLARE " + QuoteIdentifier(node.Cursor.Name);
        case DeclareCursorSection.Sensivity:
          return "";
        case DeclareCursorSection.Scrollability:
          return node.Cursor.Scroll ? "SCROLL" : "NO SCROLL";
        case DeclareCursorSection.Cursor:
          return "CURSOR";
        case DeclareCursorSection.Holdability:
          return node.Cursor.WithHold ? "WITH HOLD" : "";
        case DeclareCursorSection.Returnability:
        case DeclareCursorSection.Updatability:
        case DeclareCursorSection.Exit:
          return "";
        case DeclareCursorSection.For:
          return "FOR";
      }
      return base.Translate(context, node, section);
    }

    public override string Translate(SqlCompilerContext context, SqlFetch node, FetchSection section)
    {
      switch (section) {
        case FetchSection.Entry:
          return "FETCH " + node.Option.ToString().ToUpper();
        case FetchSection.Targets:
          return "FROM " + QuoteIdentifier(node.Cursor.Name);
        case FetchSection.Exit:
          break;
      }
      return base.Translate(context, node, section);
    }

    public override string Translate(SqlCompilerContext context, SqlOpenCursor node)
    {
      // DECLARE CURSOR already opens it
      return string.Empty;
    }

    public override string Translate(SqlCompilerContext context, SqlMatch node, MatchSection section)
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
            return "EXISTS(SELECT '";
          }
          else {
            throw new InvalidOperationException(Strings.ExSqlMatchValueMustBeAnSqlRowInstance);
          }
        case MatchSection.Specification:
          return "' WHERE EXISTS";
        case MatchSection.Exit:
          return ")";
      }
      return string.Empty;
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
      bool noMoreWhenNeeded = false;
      bool allNull = true;
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
          for (int i = 0; i < columns.Count; i++) {
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
      bool allNull = true;
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
            SqlCase c3 = SqlDml.Case();
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

    public override string Translate(SqlCompilerContext context, SqlFunctionCall node, FunctionCallSection section, int position)
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
              return Translate(node.FunctionType);
          }
          break;
      }

      return base.Translate(context, node, section, position);
    }

    public override string Translate(SqlCompilerContext context, SqlUnary node, NodeSection section)
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
      return base.Translate(context, node, section);
    }

    public override string Translate(SqlCompilerContext context, SqlNextValue node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          return "nextval('";
        case NodeSection.Exit:
          return "')";
      }
      return string.Empty;
    }

    public override string Translate(SqlCompilerContext context, SqlCast node, NodeSection section)
    {
      // casting this way behaves differently: -32768::int2 is out of range ! We need (-32768)::int2
      switch (section) {
        case NodeSection.Entry:
          return "(";
        case NodeSection.Exit:
          return ")::" + Translate(node.Type);
      }
      return string.Empty;
    }

    public override string Translate(SqlDateTimePart part)
    {
      switch (part) {
      case SqlDateTimePart.Millisecond:
        return "MILLISECONDS";
      case SqlDateTimePart.DayOfYear:
        return "DOY";
      case SqlDateTimePart.DayOfWeek:
        return "DOW";
      }

      return base.Translate(part);
    }

    public override string Translate(SqlDateTimeOffsetPart part)
    {
      switch (part) {
        case SqlDateTimeOffsetPart.Millisecond:
          return "MILLISECONDS";
        case SqlDateTimeOffsetPart.DayOfYear:
          return "DOY";
        case SqlDateTimeOffsetPart.DayOfWeek:
          return "DOW";
        case SqlDateTimeOffsetPart.Offset:
          return "TIMEZONE";
        case SqlDateTimeOffsetPart.TimeZoneHour:
          return "TIMEZONE_HOUR";
        case SqlDateTimeOffsetPart.TimeZoneMinute:
          return "TIMEZONE_MINUTE";
      }

      return base.Translate(part);
    }

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
      chars[chars.Length - 1] = 'a';
      chars[chars.Length - 2] = 'e';
      chars[chars.Length - 3] = 't';
      chars[chars.Length - 4] = 'y';
      chars[chars.Length - 5] = 'b';
      chars[chars.Length - 6] = ':';
      chars[chars.Length - 7] = ':';
      chars[chars.Length - 8] = '\'';

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
    }
  }
}