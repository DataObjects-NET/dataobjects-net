using System;
using System.Diagnostics;
using System.Text;
using Xtensive.Sql.Common;
using Xtensive.Sql.Common.PgSql;
using Xtensive.Sql.Dom.Compiler;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Ddl;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.PgSql.v8_0
{
  public class PgSqlTranslator : SqlTranslator
  {
    internal const string RealExtractDays = "real_extract_days";
    internal const string RealExtractSeconds = "real_extract_seconds";
    internal const string RealExtractMilliseconds = "real_extract_ms";

    public PgSqlTranslator(SqlDriver driver)
      : base(driver)
    {
    }

    public override void Initialize()
    {
      base.Initialize();
      numberFormat.NumberDecimalSeparator = ".";
      numberFormat.NumberGroupSeparator = "";
      numberFormat.NaNSymbol = "'Nan'::float4";
      numberFormat.NegativeInfinitySymbol = "'-Infinity'::float4";
      numberFormat.PositiveInfinitySymbol = "'Infinity'::float4";

      dateTimeFormat.FullDateTimePattern = "yyyyMMdd HHmmss.ffffff";
    }

    protected new PgSqlDriver Driver
    {
      get { return base.Driver as PgSqlDriver; }
    }


    public override string DdlStatementDelimiter
    {
      get { return ";"; }
    }

    public override string BatchStatementDelimiter
    {
      get { return ";"; }
    }


    [DebuggerStepThrough]
    public override string QuoteIdentifier(params string[] names)
    {
      string[] names2 = new string[names.Length];
      for (int i = 0; i < names.Length; i++) {
        names2[i] = names[i].Replace("\"", "\"\"");
      }
      return ("\"" + string.Join("\".\"", names2) + "\"");
    }

    [DebuggerStepThrough]
    public override string QuoteString(string str)
    {
      return "'" + str.Replace("'", "''").Replace(@"\", @"\\") + "'";
    }

    public override string Translate(SqlFunctionType type)
    {
      switch (type) {
      case SqlFunctionType.SystemUser:
        return String.Empty;
      case SqlFunctionType.UserDefined:
        return String.Empty;

      case SqlFunctionType.User:
      case SqlFunctionType.CurrentUser:
        return "current_user";
      case SqlFunctionType.SessionUser:
        return "session_user";
      case SqlFunctionType.NullIf:
        return "nullif";
      case SqlFunctionType.Coalesce:
        return "coalesce";

      //date

      case SqlFunctionType.CurrentDate:
        return "date_trunc('day', current_timestamp)";

      case SqlFunctionType.CurrentTimeStamp:
        return "current_timestamp";

      case SqlFunctionType.Extract:
      case SqlFunctionType.IntervalExtract:
        return "extract";

      //string

      case SqlFunctionType.Length:
        return "length";
      case SqlFunctionType.CharLength:
        return "char_length";

      case SqlFunctionType.OctetLength:
        return "octet_length";

      case SqlFunctionType.Lower:
        return "lower";

      case SqlFunctionType.Position:
        return "position";

      case SqlFunctionType.Substring:
        return "substring";

        //        case SqlFunctionType.Trim:
        //          return "trim";

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

      case SqlFunctionType.Sign:
        return "sign";

      case SqlFunctionType.Sqrt:
        return "sqrt";

      case SqlFunctionType.Tan:
        return "tan";

        //other

      case SqlFunctionType.BitLength:
        return "bit_length";

      default:
        return base.Translate(type);
      }
    }

    public override string Translate(SqlJoinMethod method)
    {
      return string.Empty;
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

      default:
        return base.Translate(type);
      }
    }

    public override string Translate(SqlValueType type)
    {
      string str = "";
      switch (type.DataType) {
      case SqlDataType.Boolean:
        str = "bool";
        break;

      case SqlDataType.SByte:
      case SqlDataType.Byte:
      case SqlDataType.Int16:
        str = "int2";
        break;

      case SqlDataType.UInt16:
      case SqlDataType.Int32:
        str = "int4";
        break;

      case SqlDataType.UInt32:
      case SqlDataType.Int64:
        str = "int8";
        break;

      case SqlDataType.UInt64:
        str = "numeric(20,0)";
        break;

      case SqlDataType.Decimal:
        str = "numeric";
        break;

      case SqlDataType.Float:
        str = "float4";
        break;

      case SqlDataType.Double:
        str = "float8";
        break;

      case SqlDataType.SmallMoney:
      case SqlDataType.Money:
        str = "numeric(19,4)";
        break;

      case SqlDataType.SmallDateTime:
      case SqlDataType.DateTime:
      case SqlDataType.TimeStamp:
        str = "timestamp";
        break;

      case SqlDataType.Interval:
        str = "interval";
        break;

      case SqlDataType.AnsiChar:
      case SqlDataType.Char:
        str = "char";
        break;

      case SqlDataType.AnsiVarChar:
      case SqlDataType.VarChar:
        str = "varchar";
        break;

      case SqlDataType.AnsiText:
      case SqlDataType.Text:
      case SqlDataType.Variant:
      case SqlDataType.Guid:
      case SqlDataType.Xml:
        str = "text";
        break;

      case SqlDataType.Image:
      case SqlDataType.Binary:
      case SqlDataType.VarBinary:
        str = "bytea";
        break;
      }
      if (Util.IsSizableType(type.DataType)) {
        return string.Concat(str, "(", type.Size, ")");
      }
      if ((type.Precision!=0)) {
        short precision = type.Precision;
        if (type.DataType==SqlDataType.DateTime || type.DataType==SqlDataType.TimeStamp || type.DataType==SqlDataType.SmallDateTime)
          precision = Math.Min(precision, Driver.ServerInfoProvider.MaxDateTimePrecision);

        if (type.Scale!=0) {
          return String.Concat(str, "(", precision, ",", type.Scale, ")");
        }
        else {
          return String.Concat(str, "(", precision, ")");
        }
      }
      return str;
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

    public override string Translate(SchemaNode node)
    {
      TemporaryTable tmp = node as TemporaryTable;
      //temporary tables need no schema qualifier
      if (tmp==null && node.Schema!=null) {
        return QuoteIdentifier(new[] {node.Schema.Name, node.Name});
      }
      return QuoteIdentifier(new[] {node.Name});
    }

    public override string Translate(SqlCompilerContext context, SqlCreateSequence node, NodeSection section)
    {
      if (section==NodeSection.Entry) {
        return "CREATE SEQUENCE " + Translate(node.Sequence); // + ((node.Sequence.DataType != null) ? (" AS " + this.Translate(node.Sequence.DataType)) : ""));
      }
      else {
        return base.Translate(context, node, section);
      }
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
        return string.Empty;

      default:
        return base.Translate(context, column, section);
      }
    }

    public override string Translate(SqlCompilerContext context, SqlCreateIndex node)
    {
      Index index = node.Index;
      StringBuilder builder = new StringBuilder();
      builder.AppendFormat("CREATE {0}INDEX {1} ON {2} ("
        , index.IsUnique ? "UNIQUE " : ""
        , QuoteIdentifier(index.Name)
        , Translate(index.DataTable));

      bool first = true;
      foreach (IndexColumn column in index.Columns) {
        if (first) {
          first = false;
        }
        else {
          builder.Append(RowItemDelimiter);
        }
        builder.Append(QuoteIdentifier(column.Name));
      }
      builder.Append(")");
      AppendIndexStorageParameters(builder, index);

      if (!String.IsNullOrEmpty(index.Filegroup)) {
        builder.Append(" TABLESPACE " + QuoteIdentifier(index.Filegroup));
      }

      //cluster in a separate command
      if (index.IsClustered) {
        builder.AppendFormat(BatchStatementDelimiter + " CLUSTER {0} ON {1}"
          , QuoteIdentifier(index.Name)
          , QuoteIdentifier(index.DataTable.Schema.Name, index.DataTable.Name)
          );
      }

      return builder.ToString();
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
      return String.Empty;
    }

    public override string Translate(SqlCompilerContext context, SqlDeclareVariable node)
    {
      return String.Empty;
    }

    public override string Translate(SqlCompilerContext context, SqlAssignment node, NodeSection section)
    {
      return string.Empty;
    }


    public override string Translate<T>(SqlCompilerContext context, SqlLiteral<T> node)
    {
      return TranslateLiteral(node.Value);
    }

    public override string Translate<T>(SqlCompilerContext context, SqlArray<T> node)
    {
      T[] values = node.Values;
      int length = values.Length;
      if (length==0) {
        return "'{}'::" + TranslateDotNetType<T>() + "[]";
      }
      else {
        StringBuilder sb = new StringBuilder("ARRAY[");
        for (int i = 0; i < length; i++) {
          sb.Append(TranslateLiteral(values[i]) + ",");
        }
        sb.Length -= 1;
        sb.Append("]");
        return sb.ToString();
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
      //DECLARE CURSOR already opens it
      return String.Empty;
    }

    public override string Translate(SqlCompilerContext context, SqlSelect node, SelectSection section)
    {
      if (section==SelectSection.Exit) {
        if (node.Top > 0 || node.Offset > 0) {
          StringBuilder sb = new StringBuilder();
          if (node.Offset > 0)
            sb.Append(" OFFSET " + node.Offset);
          if (node.Top > 0)
            sb.Append(" LIMIT " + node.Top);
          return sb.ToString();
        }
        return string.Empty;
      }
      return base.Translate(context, node, section);
    }
    
    public override string Translate(SqlCompilerContext context, SqlMatch node, MatchSection section)
    {
      switch (section) {
      case MatchSection.Entry:
        //MATCH is not supported by PostgreSQL, we need some workaround
        SqlRow row = node.Value as SqlRow;
        if (row!=null) {
          SqlSelect finalQuery = Sql.Select();
          finalQuery.Columns.Add(5);
          switch (node.MatchType) {
            #region SIMPLE

          case SqlMatchType.None: {
            bool existsNull = false;
            SqlCase c = Sql.Case();
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
                      when1 = Sql.IsNull(row[i]);
                    else
                      when1 = when1 || Sql.IsNull(row[i]);
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
                SqlQueryRef originalQuery = Sql.QueryRef(node.SubQuery.Query);
                SqlSelect q1 = Sql.Select(originalQuery);
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
                    q1.Having = Sql.Count(Sql.Asterisk)==1;
                }
                //c.Add(Sql.Exists(q1), true);
                c.Else = Sql.Exists(q1);
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
            SqlCase c1 = Sql.Case();
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
                    when1 = Sql.IsNull(row[i]);
                  else
                    when1 = when1 && Sql.IsNull(row[i]);
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
                    when2 = Sql.IsNotNull(row[i]);
                  else
                    when2 = when2 && Sql.IsNotNull(row[i]);
                }
                if (allLiteral) {
                  when2 = true;
                }
                if (!whenNotNeeded) {
                  //find row in subquery
                  SqlQueryRef originalQuery = Sql.QueryRef(node.SubQuery.Query);
                  SqlSelect q1 = Sql.Select(originalQuery);
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
                      q1.Having = Sql.Count(Sql.Asterisk)==1;
                  }
                  c1.Add(when2, Sql.Exists(q1));
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
            SqlCase c1 = Sql.Case();
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
                    when1 = Sql.IsNull(row[i]);
                  else
                    when1 = when1 && Sql.IsNull(row[i]);
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
                SqlQueryRef originalQuery = Sql.QueryRef(node.SubQuery.Query);
                SqlSelect q1 = Sql.Select(originalQuery);
                q1.Columns.Add(8);
                {
                  SqlTableColumnCollection columns = originalQuery.Columns;
                  SqlExpression where = null;
                  for (int i = 0; i < columns.Count; i++) {
                    //if row[i] would be NULL then c3 would result in true,
                    if (row[i].NodeType!=SqlNodeType.Null) {
                      SqlCase c3 = Sql.Case();
                      c3.Add(Sql.IsNull(row[i]), true);
                      c3.Else = row[i]==columns[i];

                      if (where==null)
                        where = c3;
                      else
                        where = where && c3;
                    }
                    if (node.Unique) {
                      SqlCase c4 = Sql.Case();
                      c4.Add(Sql.IsNull(row[i]), 0);
                      c4.Else = columns[i];
                      q1.GroupBy.Add(c4);
                    }
                  }
                  q1.Where = where;
                  if (node.Unique)
                    q1.Having = Sql.Count(Sql.Asterisk)==1;
                }
                c1.Else = Sql.Exists(q1);
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
          SqlMatch newNode = Sql.Match(Sql.Row(), Sql.SubQuery(finalQuery).Query, node.Unique, node.MatchType);
          node.ReplaceWith(newNode);
          return "EXISTS(SELECT '";
        }
        else {
          throw new InvalidOperationException("SqlMatch.Value must be an SqlRow instance.");
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
      SqlSelect q0 = Sql.Select();
      {
        SqlQueryRef originalQuery = Sql.QueryRef(query);
        SqlSelect q1 = Sql.Select(originalQuery);
        q1.Columns.Add(1);
        q1.Where = true; //initially true
        {
          int index = 0;
          foreach (SqlColumn col in originalQuery.Columns) {
            q1.Where = q1.Where && col==row[index];
            q1.GroupBy.Add(col);
            index++;
          }
          q1.Having = Sql.Count(Sql.Asterisk)==1;
        }
        q0.Columns.Add(Sql.Exists(q1));
      }
      return q0;
    }


    public override string Translate(SqlCompilerContext context, SqlFunctionCall node, FunctionCallSection section, int position)
    {
      switch (section) {
        case FunctionCallSection.Entry:
          // Call random() always without parameters

          if (node.FunctionType==SqlFunctionType.Rand)
            node.Arguments.Clear();

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

    public override string Translate(SqlCompilerContext context, SqlUserFunctionCall node, FunctionCallSection section, int position)
    {
      if (section == FunctionCallSection.Entry)
        switch (node.Name) {
          case RealExtractDays:
            return "extract (day from ";
          case RealExtractMilliseconds:
            return "extract (milliseconds from ";
          case RealExtractSeconds:
            return "extract (second from ";
        }

      return base.Translate(context, node, section, position);
    }

    public override string Translate(SqlCompilerContext context, SqlUnary node, NodeSection section)
    {
      //substitute UNIQUE predicate with a more complex EXISTS predicate,
      //because UNIQUE is not supported

      if (node.NodeType==SqlNodeType.Unique) {
        SqlSubQuery origSubselect = node.Operand as SqlSubQuery;
        if (origSubselect!=null) {
          SqlQueryRef origQuery = Sql.QueryRef(origSubselect.Query);
          SqlSelect existsOp = Sql.Select(origQuery);
          existsOp.Columns.Add(1);
          existsOp.Where = true;
          foreach (SqlColumn col in origQuery.Columns) {
            existsOp.Where = existsOp.Where && Sql.IsNotNull(col);
            existsOp.GroupBy.Add(col);
          }
          existsOp.Having = Sql.Count(Sql.Asterisk) > 1;
          existsOp.Top = 1;

          node.ReplaceWith(Sql.Not(Sql.Exists(existsOp)));
        }
      }
      return base.Translate(context, node, section);
    }

    public override string Translate(SqlCompilerContext context, SqlBinary node, NodeSection section)
    {
      if (section==NodeSection.Entry) {
        //substitute  x IN(array) with x IN(row)
        if (node.NodeType==SqlNodeType.In || node.NodeType==SqlNodeType.NotIn) {
          if (node.Right.NodeType==SqlNodeType.Array) {
            Type elemType = node.Right.GetType().GetGenericArguments()[0];

            SqlBinary subst = null;
            SqlRow newRow = Sql.Row();
            if (elemType.Equals(typeof (bool))) {
              SqlArray<bool> array = node.Right as SqlArray<bool>;
              for (int i = 0; i < array.Values.Length; i++) {
                newRow.Add(array.Values[i]);
              }
            }
            else if (elemType.Equals(typeof (char))) {
              SqlArray<char> array = node.Right as SqlArray<char>;
              for (int i = 0; i < array.Values.Length; i++) {
                newRow.Add(array.Values[i]);
              }
            }
            else if (elemType.Equals(typeof (string))) {
              SqlArray<string> array = node.Right as SqlArray<string>;
              for (int i = 0; i < array.Values.Length; i++) {
                newRow.Add(array.Values[i]);
              }
            }
            else if (elemType.Equals(typeof (byte))) {
              SqlArray<byte> array = node.Right as SqlArray<byte>;
              for (int i = 0; i < array.Values.Length; i++) {
                newRow.Add(array.Values[i]);
              }
            }
            else if (elemType.Equals(typeof (sbyte))) {
              SqlArray<sbyte> array = node.Right as SqlArray<sbyte>;
              for (int i = 0; i < array.Values.Length; i++) {
                newRow.Add(array.Values[i]);
              }
            }
            else if (elemType.Equals(typeof (short))) {
              SqlArray<short> array = node.Right as SqlArray<short>;
              for (int i = 0; i < array.Values.Length; i++) {
                newRow.Add(array.Values[i]);
              }
            }
            else if (elemType.Equals(typeof (ushort))) {
              SqlArray<ushort> array = node.Right as SqlArray<ushort>;
              for (int i = 0; i < array.Values.Length; i++) {
                newRow.Add(array.Values[i]);
              }
            }
            else if (elemType.Equals(typeof (int))) {
              SqlArray<int> array = node.Right as SqlArray<int>;
              for (int i = 0; i < array.Values.Length; i++) {
                newRow.Add(array.Values[i]);
              }
            }
            else if (elemType.Equals(typeof (uint))) {
              SqlArray<uint> array = node.Right as SqlArray<uint>;
              for (int i = 0; i < array.Values.Length; i++) {
                newRow.Add(array.Values[i]);
              }
            }
            else if (elemType.Equals(typeof (long))) {
              SqlArray<long> array = node.Right as SqlArray<long>;
              for (int i = 0; i < array.Values.Length; i++) {
                newRow.Add(array.Values[i]);
              }
            }
            else if (elemType.Equals(typeof (ulong))) {
              SqlArray<ulong> array = node.Right as SqlArray<ulong>;
              for (int i = 0; i < array.Values.Length; i++) {
                newRow.Add(array.Values[i]);
              }
            }
            else if (elemType.Equals(typeof (float))) {
              SqlArray<float> array = node.Right as SqlArray<float>;
              for (int i = 0; i < array.Values.Length; i++) {
                newRow.Add(array.Values[i]);
              }
            }
            else if (elemType.Equals(typeof (double))) {
              SqlArray<double> array = node.Right as SqlArray<double>;
              for (int i = 0; i < array.Values.Length; i++) {
                newRow.Add(array.Values[i]);
              }
            }
            else if (elemType.Equals(typeof (decimal))) {
              SqlArray<decimal> array = node.Right as SqlArray<decimal>;
              for (int i = 0; i < array.Values.Length; i++) {
                newRow.Add(array.Values[i]);
              }
            }
            else if (elemType.Equals(typeof (DateTime))) {
              SqlArray<DateTime> array = node.Right as SqlArray<DateTime>;
              for (int i = 0; i < array.Values.Length; i++) {
                newRow.Add(array.Values[i]);
              }
            }

            if (newRow.Count==0)
              subst = Sql.Equals(1, node.NodeType==SqlNodeType.In ? 2 : 1);
            else {
              if (node.NodeType==SqlNodeType.In)
                subst = Sql.In(node.Left, newRow);
              else
                subst = Sql.NotIn(node.Left, newRow);
            }
            node.ReplaceWith(subst);
          }
        }
      }
      return base.Translate(context, node, section);
    }

    public override string Translate(SqlCompilerContext context, SqlNextValue node, NodeSection section)
    {
      if (section==NodeSection.Entry) {
        return "nextval('";
      }
      else if (section==NodeSection.Exit) {
        return "')";
      }
      return string.Empty;
    }

    public override string Translate(SqlCompilerContext context, SqlOrder node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Exit:
          return (node.Ascending) ? "ASC NULLS FIRST" : "DESC NULLS LAST";
      }
      return string.Empty;
    }

    public override string Translate(SqlCompilerContext context, SqlCast node, NodeSection section)
    {
      //casting this way behaves differently: -32768::int2 is out of range ! We need (-32768)::int2
      switch (section) {
      case NodeSection.Entry:
        return "(";

      case NodeSection.Exit:
        return ")::" + Translate(node.Type);
      }
      return string.Empty;
    }

    public override string Translate(SqlCompilerContext context, Table node, SqlRenameAction action)
    {
      return "RENAME TO " + QuoteIdentifier(action.Name);
    }

    public override string Translate(SqlCompilerContext context, TableColumn node, SqlRenameAction action)
    {
      return string.Format("RENAME COLUMN {0} TO {1}", QuoteIdentifier(node.DbName), QuoteIdentifier(action.Name));
    }

    protected string TranslateLiteral(object obj)
    {
      if (obj==null || obj==DBNull.Value) {
        return "NULL";
      }
      else if (obj is bool) {
        return (bool) obj ? "TRUE" : "FALSE";
      }
      else if (obj is char) {
        return QuoteString(obj.ToString());
      }
      else if (obj is string) {
        return QuoteString(obj as string);
      }
      else if (obj is sbyte || obj is short || obj is int) {
        return Convert.ToInt32(obj).ToString(this);
      }
      else if (obj is long) {
        return ((long) obj).ToString(this);
      }
      else if (obj is byte || obj is ushort || obj is uint) {
        return Convert.ToUInt32(obj).ToString(this);
      }
      else if (obj is ulong) {
        return ((ulong) obj).ToString(this);
      }
      else if (obj is decimal) {
        return ((decimal) obj).ToString(this);
      }
      else if (obj is float) {
        return ((float) obj).ToString(this);
      }
      else if (obj is double) {
        return ((double) obj).ToString(this);
      }
      else if (obj is DateTime) {
        return "'" + ((DateTime) obj).ToString("yyyyMMdd HHmmss.ffffff") + "'::timestamp(6)";
      }
      else if (obj is SqlDateTimePart) {
        return TranslateDateTimePart((SqlDateTimePart) obj);
      } if (obj is SqlIntervalPart) {
        return ((SqlIntervalPart) obj).ToString().ToUpperInvariant(); //default names are acceptable
      }
      else if (obj is TimeSpan) {
        TimeSpan ts = (TimeSpan) obj;
        if (ts.Ticks==0)
          return "'0 day'::interval";

        StringBuilder sb = new StringBuilder("'");
        if (ts.Days!=0) {
          sb.AppendFormat("{0} day ", ts.Days);
        }
        if (ts.Hours!=0) {
          sb.AppendFormat("{0} hour ", ts.Hours);
        }
        if (ts.Minutes!=0) {
          sb.AppendFormat("{0} minute ", ts.Minutes);
        }
        if (ts.Seconds!=0) {
          sb.AppendFormat("{0} second ", ts.Seconds);
        }
        if (ts.Milliseconds!=0) {
          sb.AppendFormat("{0} millisecond ", ts.Milliseconds);
        }
        sb.Append("'::interval");
        return sb.ToString();
      }
      else if (obj is byte[]) {
        byte[] array = obj as byte[];
        if (array.Length==0)
          return "''::bytea";

        char[] chars = new char[1 + 5 * array.Length + 8];
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
        return new String(chars);
      }
      else {
        return obj.ToString();
      }
    }
    
    protected static string TranslateDateTimePart(SqlDateTimePart part)
    {
      switch (part) {
      case SqlDateTimePart.Year:
        return "YEAR";
      case SqlDateTimePart.Month:
        return "MONTH";
      case SqlDateTimePart.Day:
        return "DAY";
      case SqlDateTimePart.Hour:
        return "HOUR";
      case SqlDateTimePart.Minute:
        return "MINUTE";
      case SqlDateTimePart.Second:
        return "SECOND";
      case SqlDateTimePart.Millisecond:
        return "MILLISECONDS";
      case SqlDateTimePart.TimeZoneHour:
        return "TIMEZONE_HOUR";
      case SqlDateTimePart.TimeZoneMinute:
        return "TIMEZONE_MINUTE";
      case SqlDateTimePart.DayOfWeek:
        return "DOW";
      case SqlDateTimePart.DayOfYear:
        return "DOY";
      default:
        return "DAY";
      }
    }

    protected static string TranslateDotNetType<T>() // where T : IConvertible
    {
      switch (Type.GetTypeCode(typeof (T))) {
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
      default: {
        if (typeof (T).Equals(typeof (TimeSpan))) {
          return "interval";
        }
        return "text";
      }
      }
    }
  }
}