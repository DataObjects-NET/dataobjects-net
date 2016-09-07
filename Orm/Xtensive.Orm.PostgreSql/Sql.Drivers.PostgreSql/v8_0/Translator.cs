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
      TemporaryTable tmp = node as TemporaryTable;
      //temporary tables need no schema qualifier
      if (tmp==null && node.Schema!=null) {
        return QuoteIdentifier(new[] {node.Schema.Name, node.Name});
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
            , index.IsUnique ? "UNIQUE " : String.Empty
            , QuoteIdentifier(index.Name)
            , Translate(context, index.DataTable)
            , index.IsSpatial ? "USING GIST" : String.Empty);
        case CreateIndexSection.StorageOptions:
          var builder = new StringBuilder();
          builder.Append(")");
          AppendIndexStorageParameters(builder, index);
          if (!string.IsNullOrEmpty(index.Filegroup))
            builder.Append(" TABLESPACE " + QuoteIdentifier(index.Filegroup));
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
      return "DROP INDEX " + QuoteIdentifier(node.Index.DataTable.Schema.Name, node.Index.Name);
    }

    public override string Translate(SqlCompilerContext context, SqlBreak node)
    {
      return "EXIT";
    }

    public override string Translate(SqlCompilerContext context, SqlContinue node)
    {
      return string.Empty;
    }

    public override string Translate(SqlCompilerContext context, SqlDeclareVariable node)
    {
      return string.Empty;
    }

    public override string Translate(SqlCompilerContext context, SqlAssignment node, NodeSection section)
    {
      return string.Empty;
    }
    
    public override string Translate(SqlCompilerContext context, object literalValue)
    {
      var literalType = literalValue.GetType();
      if (literalType==typeof(byte[]))
        return TranslateByteArrayLiteral((byte[]) literalValue);
      if (literalType==typeof(Guid))
        return QuoteString(SqlHelper.GuidToString((Guid) literalValue));
      if(literalType==typeof(DateTimeOffset))
        return ((DateTimeOffset)literalValue).ToString(DateTimeOffsetFormatString);

      if (literalType==typeof (NpgsqlPoint)) {
        var point = (NpgsqlPoint) literalValue;
        return String.Format("point'({0},{1})'", point.X, point.Y);
      }
      if (literalType==typeof (NpgsqlLSeg)) {
        var lSeg = (NpgsqlLSeg) literalValue;
        return String.Format("lseg'[({0},{1}),({2},{3})]'", lSeg.Start.X, lSeg.Start.Y, lSeg.End.X, lSeg.End.Y);
      }
      if (literalType==typeof (NpgsqlBox)) {
        var box = (NpgsqlBox) literalValue;
        return String.Format("box'({0},{1}),({2},{3})'", box.LowerLeft.X, box.LowerLeft.Y, box.UpperRight.X, box.UpperRight.Y);
      }
      if (literalType==typeof (NpgsqlPath))
        return String.Format("path'(({0},{1}))'", 0, 0);
      if (literalType==typeof (NpgsqlPolygon))
        return String.Format("polygon'((0,0))'");
      if (literalType==typeof (NpgsqlCircle)) {
        var circle = (NpgsqlCircle) literalValue;
        return String.Format("circle'<({0},{1}),{2}>'", circle.Center.X, circle.Center.Y, circle.Radius);
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
      bool isSecond = node.DateTimePart==SqlDateTimePart.Second
        || node.IntervalPart==SqlIntervalPart.Second;
      bool isMillisecond = node.DateTimePart==SqlDateTimePart.Millisecond
        || node.IntervalPart==SqlIntervalPart.Millisecond;
      if (!(isSecond || isMillisecond))
        return base.Translate(context, node, section);
      switch (section) {
      case ExtractSection.Entry:
        return "(extract(";
      case ExtractSection.Exit:
        return isMillisecond ? ")::int8 % 1000)" : ")::int8)";
      default:
        return base.Translate(context, node, section);
      }
    }

    public override string Translate(SqlCompilerContext context, SqlDeclareCursor node, DeclareCursorSection section)
    {
      switch (section) {
      case DeclareCursorSection.Entry:
        return ("DECLARE " + QuoteIdentifier(node.Cursor.Name));
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
        SqlRow row = node.Value as SqlRow;
        if (row!=null) {
          SqlSelect finalQuery = SqlDml.Select();
          finalQuery.Columns.Add(5);
          switch (node.MatchType) {
            #region SIMPLE

          case SqlMatchType.None: {
            bool existsNull = false;
            SqlCase c = SqlDml.Case();
            {
              bool subQueryNeeded = true;
              //if any of the row elements is NULL then true
              if (row.Count > 0) {
                bool allLiteral = true; //if true then there is no NULL element
                SqlExpression when1 = null;
                for (int i = 0; i < row.Count; i++) {
                  bool elementIsNotLiteral = row[i].NodeType!=SqlNodeType.Literal;
                  //if the row element is the NULL value
                  if (row[i].NodeType==SqlNodeType.Null) {
                    existsNull = true;
                    break;
                  }
                  if (allLiteral && elementIsNotLiteral)
                    allLiteral = false;
                  if (elementIsNotLiteral) {
                    if (when1==null)
                      when1 = SqlDml.IsNull(row[i]);
                    else
                      when1 = when1 || SqlDml.IsNull(row[i]);
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
                else //(!whenNotNeeded)
                {
                  //Check if any row element is NULL
                  c.Add(when1==null ? true : when1, true);
                  subQueryNeeded = true;
                }
              }
              //find row in subquery
              if (subQueryNeeded) {
                SqlQueryRef originalQuery = SqlDml.QueryRef(node.SubQuery.Query);
                SqlSelect q1 = SqlDml.Select(originalQuery);
                q1.Columns.Add(1);
                {
                  SqlTableColumnCollection columns = originalQuery.Columns;
                  SqlExpression where = null;
                  for (int i = 0; i < columns.Count; i++) {
                    if (i==0)
                      where = columns[i]==row[i];
                    else
                      where = where && columns[i]==row[i];
                    if (node.Unique)
                      q1.GroupBy.Add(columns[i]);
                  }
                  q1.Where = where;
                  if (node.Unique)
                    q1.Having = SqlDml.Count(SqlDml.Asterisk)==1;
                }
                //c.Add(Sql.Exists(q1), true);
                c.Else = SqlDml.Exists(q1);
              }
            }
            if (c.Else==null)
              c.Else = false;
            if (existsNull)
              finalQuery.Where = null;
            else if (c.Count > 0)
              finalQuery.Where = c;
            else
              finalQuery.Where = c.Else;
            break;
          }

            #endregion

            #region FULL

          case SqlMatchType.Full: {
            SqlCase c1 = SqlDml.Case();
            {
              bool noMoreWhenNeeded = false;
              bool allNull = true;
              SqlExpression when1 = true;
              //if all row elements are null then true
              if (row.Count > 0) {
                bool whenNotNeeded = false;
                for (int i = 0; i < row.Count; i++) {
                  //if any row element is surely not the NULL value
                  if (row[i].NodeType==SqlNodeType.Literal) {
                    whenNotNeeded = true;
                    break;
                  }
                  if (allNull && row[i].NodeType!=SqlNodeType.Null) {
                    allNull = false;
                  }
                  if (i==0)
                    when1 = SqlDml.IsNull(row[i]);
                  else
                    when1 = when1 && SqlDml.IsNull(row[i]);
                }
                if (allNull) {
                  when1 = true;
                }
                if (!whenNotNeeded)
                  c1.Add(when1, true);
              }
              if (!noMoreWhenNeeded) {
                bool whenNotNeeded = false;
                bool allLiteral = true;
                SqlExpression when2 = true;
                //if no row elements are null then subcase
                for (int i = 0; i < row.Count; i++) {
                  if (row[i].NodeType==SqlNodeType.Null) {
                    whenNotNeeded = true;
                    when2 = false;
                    break;
                  }
                  if (allLiteral && row[i].NodeType!=SqlNodeType.Literal)
                    allLiteral = false;
                  if (i==0)
                    when2 = SqlDml.IsNotNull(row[i]);
                  else
                    when2 = when2 && SqlDml.IsNotNull(row[i]);
                }
                if (allLiteral) {
                  when2 = true;
                }
                if (!whenNotNeeded) {
                  //find row in subquery
                  SqlQueryRef originalQuery = SqlDml.QueryRef(node.SubQuery.Query);
                  SqlSelect q1 = SqlDml.Select(originalQuery);
                  q1.Columns.Add(1);
                  {
                    SqlTableColumnCollection columns = originalQuery.Columns;
                    SqlExpression where = null;
                    for (int i = 0; i < columns.Count; i++) {
                      if (i==0)
                        where = columns[i]==row[i];
                      else
                        where = where && columns[i]==row[i];
                      if (node.Unique)
                        q1.GroupBy.Add(columns[i]);
                    }
                    q1.Where = where;
                    if (node.Unique)
                      q1.Having = SqlDml.Count(SqlDml.Asterisk)==1;
                  }
                  c1.Add(when2, SqlDml.Exists(q1));
                }
              }
              //else false
              c1.Else = false;
            }
            if (c1.Count > 0)
              finalQuery.Where = c1;
            else
              finalQuery.Where = false;
            break;
          }

            #endregion

            #region PARTIAL

          case SqlMatchType.Partial: {
            bool allNull = true;
            SqlCase c1 = SqlDml.Case();
            {
              SqlExpression when1 = true;
              //if all row elements are null then true
              if (row.Count > 0) {
                bool whenNotNeeded = false;
                for (int i = 0; i < row.Count; i++) {
                  //if any row element is surely not the NULL value
                  if (row[i].NodeType==SqlNodeType.Literal) {
                    allNull = false;
                    whenNotNeeded = true;
                    break;
                  }
                  if (allNull && row[i].NodeType!=SqlNodeType.Null) {
                    allNull = false;
                  }
                  if (i==0)
                    when1 = SqlDml.IsNull(row[i]);
                  else
                    when1 = when1 && SqlDml.IsNull(row[i]);
                }
                if (allNull) {
                  when1 = true;
                }
                if (!whenNotNeeded)
                  c1.Add(when1, true);
              }
              //otherwise
              if (!allNull) {
                //find row in subquery
                SqlQueryRef originalQuery = SqlDml.QueryRef(node.SubQuery.Query);
                SqlSelect q1 = SqlDml.Select(originalQuery);
                q1.Columns.Add(8);
                {
                  SqlTableColumnCollection columns = originalQuery.Columns;
                  SqlExpression where = null;
                  for (int i = 0; i < columns.Count; i++) {
                    //if row[i] would be NULL then c3 would result in true,
                    if (row[i].NodeType!=SqlNodeType.Null) {
                      SqlCase c3 = SqlDml.Case();
                      c3.Add(SqlDml.IsNull(row[i]), true);
                      c3.Else = row[i]==columns[i];

                      if (where==null)
                        where = c3;
                      else
                        where = where && c3;
                    }
                    if (node.Unique) {
                      SqlCase c4 = SqlDml.Case();
                      c4.Add(SqlDml.IsNull(row[i]), 0);
                      c4.Else = columns[i];
                      q1.GroupBy.Add(c4);
                    }
                  }
                  q1.Where = where;
                  if (node.Unique)
                    q1.Having = SqlDml.Count(SqlDml.Asterisk)==1;
                }
                c1.Else = SqlDml.Exists(q1);
              }
            }
            if (c1.Else==null)
              c1.Else = false;
            if (allNull)
              finalQuery.Where = null;
            else if (c1.Count > 0)
              finalQuery.Where = c1;
            else
              finalQuery.Where = c1.Else;
          }
            break;

            #endregion
          }
          SqlMatch newNode = SqlDml.Match(SqlDml.Row(), SqlDml.SubQuery(finalQuery).Query, node.Unique, node.MatchType);
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

    private SqlSelect CreateSelectForUniqueMatchNoneOrFull(SqlRow row, SqlSelect query)
    {
      /*
      select exists(select 1
              from (original subquery) x 
              where x.a1=r.a1 and x.a2=r.a2 
              group by x.a1, x.a2
              having count(*)=1
              )                    }
       */
      SqlSelect q0 = SqlDml.Select();
      {
        SqlQueryRef originalQuery = SqlDml.QueryRef(query);
        SqlSelect q1 = SqlDml.Select(originalQuery);
        q1.Columns.Add(1);
        q1.Where = true; //initially true
        {
          int index = 0;
          foreach (SqlColumn col in originalQuery.Columns) {
            q1.Where = q1.Where && col==row[index];
            q1.GroupBy.Add(col);
            index++;
          }
          q1.Having = SqlDml.Count(SqlDml.Asterisk)==1;
        }
        q0.Columns.Add(SqlDml.Exists(q1));
      }
      return q0;
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

      if (node.NodeType==SqlNodeType.Unique) {
        var origSubselect = node.Operand as SqlSubQuery;
        if (origSubselect!=null) {
          SqlQueryRef origQuery = SqlDml.QueryRef(origSubselect.Query);
          SqlSelect existsOp = SqlDml.Select(origQuery);
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
      if (section==NodeSection.Entry)
        return "nextval('";
      if (section==NodeSection.Exit)
        return "')";
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

    public override string Translate(SqlLockType lockType)
    {
      if (lockType.Supports(SqlLockType.SkipLocked)
        || lockType.Supports(SqlLockType.Shared)
        || lockType.Supports(SqlLockType.ThrowIfLocked))
        return base.Translate(lockType);
      return "FOR UPDATE";
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
          if (type==typeof (TimeSpan))
            return "interval";
          if (type==typeof(Guid))
            return "bytea";
          return "text";
      }
    }

    private static string TranslateByteArrayLiteral(byte[] array)
    {
      if (array.Length==0)
        return "''::bytea";

      var chars = new char[1 + 5 * array.Length + 8];
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