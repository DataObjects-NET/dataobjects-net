﻿// Copyright (C) 2011-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Malisa Ncube
// Created:    2011.02.25

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xtensive.Core;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Drivers.MySql.Resources;
using Xtensive.Sql.Model;
using Index = Xtensive.Sql.Model.Index;

namespace Xtensive.Sql.Drivers.MySql.v5_0
{
  internal class Translator : SqlTranslator
  {
    public override string DateTimeFormatString
    {
      get { return @"\'yyyy\-MM\-dd HH\:mm\:ss\.ffffff\'"; }
    }

    public override string TimeSpanFormatString
    {
      get { return string.Empty; }
    }

    public override string DdlStatementDelimiter
    {
      get { return ";"; }
    }

    public override string BatchItemDelimiter
    {
      get { return ";\r\n"; }
    }

    public override void Initialize()
    {
      base.Initialize();

      FloatNumberFormat.NumberDecimalSeparator = ".";
      FloatNumberFormat.NumberGroupSeparator = "";

      DoubleNumberFormat.NumberDecimalSeparator = ".";
      DoubleNumberFormat.NumberGroupSeparator = "";
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override string QuoteIdentifier(params string[] names)
    {
      return SqlHelper.QuoteIdentifierWithBackTick(names);
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override string QuoteString(string str)
    {
      return "'" + str.Replace("'", "''").Replace(@"\", @"\\").Replace("\0", string.Empty) + "'";
    }

    public override string Translate(SqlCompilerContext context, SqlSelect node, SelectSection section)
    {
      switch (section) {
        case SelectSection.HintsEntry:
          return string.Empty;
        case SelectSection.HintsExit:
          if (node.Hints.Count==0)
            return string.Empty;
          var hints = new List<string>(node.Hints.Count);
          foreach (SqlHint hint in node.Hints) {
            if (hint is SqlNativeHint)
              hints.Add(QuoteIdentifier((hint as SqlNativeHint).HintText));
          }
          return hints.Count > 0 ? "USE INDEX (" + string.Join(", ", hints.ToArray()) + ")" : string.Empty;
        default:
          return base.Translate(context, node, section);
      }
    }

    /// <inheritdoc/>
    public override string Translate(SqlFunctionType type)
    {
      switch (type) {
        case SqlFunctionType.LastAutoGeneratedId:
          return "LAST_INSERT_ID()";
        case SqlFunctionType.SystemUser:
          return "SYSTEM_USER()";
        case SqlFunctionType.User:
        case SqlFunctionType.CurrentUser:
          return "CURRENT_USER()";
        case SqlFunctionType.SessionUser:
          return "SESSION_USER()";
        case SqlFunctionType.NullIf:
          return "IFNULL";
        case SqlFunctionType.Coalesce:
          return "COALESCE";
        case SqlFunctionType.BinaryLength:
          return "LENGTH";

          //datetime/timespan

        case SqlFunctionType.DateTimeTruncate:
          return "DATE";
        case SqlFunctionType.CurrentDate:
          return "CURDATE()";
        case SqlFunctionType.CurrentTimeStamp:
          return "NOW()";
        case SqlFunctionType.IntervalNegate:
          return "-";
        case SqlFunctionType.DateTimeAddYears:
        case SqlFunctionType.DateTimeAddMonths:
        case SqlFunctionType.DateTimeConstruct:
        case SqlFunctionType.IntervalToMilliseconds:
          return string.Empty;
          //string

        case SqlFunctionType.CharLength:
          return "CHAR_LENGTH";
        case SqlFunctionType.Lower:
          return "LCASE";
        case SqlFunctionType.Position:
          return "LOCATE";
        case SqlFunctionType.Substring:
          return "SUBSTRING";
        case SqlFunctionType.Upper:
          return "UCASE";
        case SqlFunctionType.Concat:
          return "CONCAT()";
          //math

        case SqlFunctionType.Abs:
          return "ABS";
        case SqlFunctionType.Acos:
          return "ACOS";
        case SqlFunctionType.Asin:
          return "ASIN";
        case SqlFunctionType.Atan:
          return "ATAN";
        case SqlFunctionType.Atan2:
          return "ATAN2";
        case SqlFunctionType.Ceiling:
          return "CEIL";
        case SqlFunctionType.Cos:
          return "COS";
        case SqlFunctionType.Cot:
          return "COT";
        case SqlFunctionType.Degrees:
          return "DEGREES";
        case SqlFunctionType.Exp:
          return "EXP";
        case SqlFunctionType.Floor:
          return "FLOOR";
        case SqlFunctionType.Log:
          return "LN";
        case SqlFunctionType.Log10:
          return "LOG";
        case SqlFunctionType.Pi:
          return "PI";
        case SqlFunctionType.Power:
          return "POWER";
        case SqlFunctionType.Radians:
          return "RADIANS";
        case SqlFunctionType.Rand:
          return "RAND";
        case SqlFunctionType.Round:
          return "ROUND";
        case SqlFunctionType.Truncate:
          return "TRUNCATE";
        case SqlFunctionType.Sign:
          return "SIGN";
        case SqlFunctionType.Sqrt:
          return "SQRT";
        case SqlFunctionType.Tan:
          return "TAN";
        default:
          return base.Translate(type);
      }
    }

    /// <inheritdoc/>
    public override string Translate(SqlCompilerContext context, SequenceDescriptor descriptor,
      SequenceDescriptorSection section)
    {
      return string.Empty;
      //throw new NotSupportedException(Strings.ExDoesNotSupportSequences);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override string Translate(SqlNodeType type)
    {
      switch (type) {
        case SqlNodeType.Concat:
          return ",";
        case SqlNodeType.DateTimePlusInterval:
          return "+";
        case SqlNodeType.DateTimeMinusInterval:
        case SqlNodeType.DateTimeMinusDateTime:
          return "-";
        case SqlNodeType.Equals:
          return "=";
        case SqlNodeType.NotEquals:
          return "<>";
        case SqlNodeType.Modulo:
          return "MOD";
        case SqlNodeType.Intersect:
        case SqlNodeType.Except:
          throw SqlHelper.NotSupported(type.ToString());
        case SqlNodeType.BitAnd:
          return "&";
        case SqlNodeType.BitOr:
          return "|";
        case SqlNodeType.BitXor:
          return "^";
        case SqlNodeType.Overlaps:
          throw SqlHelper.NotSupported(type.ToString());
        default:
          return base.Translate(type);
      }
    }

    //TODO: Concat Fix and not introduce problems with numerics. a+b. Translate 'a' + 'b' => concat('a', 'b')
    //public override string Translate(SqlCompilerContext context, SqlBinary node, NodeSection section)
    //{
    //    switch (section)
    //    {
    //        case NodeSection.Entry:
    //            return (node.NodeType == SqlNodeType.RawConcat) ? string.Empty : "CONCAT(";
    //        case NodeSection.Exit:
    //            return (node.NodeType == SqlNodeType.RawConcat) ? string.Empty : ClosingParenthesis;
    //    }
    //    return string.Empty;
    //}

    public override string Translate(SqlCompilerContext context, SqlConcat node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          return "CONCAT(";
        case NodeSection.Exit:
          return ")";
        default:
          return string.Empty;
      }
    }

    public override string Translate(SqlCompilerContext context, SchemaNode node)
    {
      return QuoteIdentifier(new[] {node.Name});
    }


    public override string Translate(SqlCompilerContext context, SqlCreateTable node, CreateTableSection section)
    {
      switch (section) {
        case CreateTableSection.Entry:
          var builder = new StringBuilder();
          builder.Append("CREATE ");
          var temporaryTable = node.Table as TemporaryTable;
          if (temporaryTable!=null) {
            builder.Append("TEMPORARY TABLE " + Translate(context, temporaryTable));
          }
          else {
            builder.Append("TABLE " + Translate(context, node.Table));
          }
          return builder.ToString();
        case CreateTableSection.Exit:
          return string.Empty;
      }
      return base.Translate(context, node, section);
    }


    public override string Translate(SqlCompilerContext context, TableColumn column, TableColumnSection section)
    {
      switch (section) {
        case TableColumnSection.Exit:
          return string.Empty;
        case TableColumnSection.GeneratedExit:
          return "AUTO_INCREMENT"; //Workaround based on fake sequence.
        default:
          return base.Translate(context, column, section);
      }
    }

    public override string Translate(SqlCompilerContext context, SqlCreateIndex node, CreateIndexSection section)
    {
      Index index = node.Index;
      switch (section) {
        case CreateIndexSection.Entry:
          return string.Format("CREATE {0}INDEX {1} USING BTREE ON {2} "
            , index.IsUnique ? "UNIQUE " : (index.IsFullText ? "FULLTEXT " : String.Empty)
            , QuoteIdentifier(index.Name)
            , Translate(context, index.DataTable));

        case CreateIndexSection.Exit:
          return string.Empty;
        default:
          return base.Translate(context, node, section);
      }
    }

    public override string Translate(SqlCompilerContext context, SqlDropIndex node)
    {
      return string.Format("DROP INDEX {0} ON {1}", QuoteIdentifier(node.Index.Name),
        QuoteIdentifier(node.Index.DataTable.Name));
    }

    public override string Translate(SqlCompilerContext context, Constraint constraint, ConstraintSection section)
    {
      switch (section) {
        case ConstraintSection.Entry:
          if (constraint is PrimaryKey)
            return string.Empty;
          return base.Translate(context, constraint, section);
        case ConstraintSection.Exit: {
          var fk = constraint as ForeignKey;
          var sb = new StringBuilder();
          sb.Append(")");
          if (fk!=null) {
            if (fk.OnUpdate!=ReferentialAction.NoAction)
              sb.Append(" ON UPDATE " + Translate(fk.OnUpdate));
            if (fk.OnDelete!=ReferentialAction.NoAction)
              sb.Append(" ON DELETE " + Translate(fk.OnDelete));
          }
          return sb.ToString();
        }
        default:
          return base.Translate(context, constraint, section);
      }
    }

    public override string Translate(SqlCompilerContext context, SqlAlterTable node, AlterTableSection section)
    {
      switch (section) {
        case AlterTableSection.DropBehavior:
          var cascadableAction = node.Action as SqlCascadableAction;
          if (cascadableAction==null || !cascadableAction.Cascade)
            return string.Empty;
          if (cascadableAction is SqlDropConstraint)
            return string.Empty;
          return cascadableAction.Cascade ? "CASCADE" : "RESTRICT";
        default:
          return base.Translate(context, node, section);
      }
    }

    public override string Translate(SqlCompilerContext context, SqlInsert node, InsertSection section)
    {
      if (section==InsertSection.DefaultValues)
        return "() VALUES ()";
      return base.Translate(context, node, section);
    }

    public override string Translate(SqlCompilerContext context, SqlBreak node)
    {
      return string.Empty;
      //throw new NotSupportedException(Strings.ExCursorsOnlyForProcsAndFuncs);
    }

    public override string Translate(SqlValueType type)
    {
      // we need to explicitly specify maximum interval precision
      if (type.Type==SqlType.Interval)
        return "decimal(18, 18)";
      return base.Translate(type);
    }

    /// <inheritdoc/>
    public override string Translate(SqlCompilerContext context, object literalValue)
    {
      Type literalType = literalValue.GetType();
      switch (Type.GetTypeCode(literalType)) {
        case TypeCode.Boolean:
          return (bool) literalValue ? "1" : "0";
        case TypeCode.UInt64:
          return QuoteString(((UInt64) literalValue).ToString());
      }
      if (literalType==typeof (byte[])) {
        var values = (byte[]) literalValue;
        var builder = new StringBuilder(2 * (values.Length + 1));
        builder.Append("x'");
        builder.AppendHexArray(values);
        builder.Append("'");
        return builder.ToString();
      }
      if (literalType==typeof (Guid))
        return QuoteString(SqlHelper.GuidToString((Guid) literalValue));
      if (literalType==typeof (TimeSpan))
        return Convert.ToString(((TimeSpan) literalValue).Ticks * 100);
      return base.Translate(context, literalValue);
    }

    /// <inheritdoc/>
    public override string Translate(SqlCompilerContext context, SqlExtract node, ExtractSection section)
    {
      bool isSecond = node.DateTimePart==SqlDateTimePart.Second || node.IntervalPart==SqlIntervalPart.Second;
      bool isMillisecond = node.DateTimePart==SqlDateTimePart.Millisecond ||
        node.IntervalPart==SqlIntervalPart.Millisecond;
      if (!(isSecond || isMillisecond))
        return base.Translate(context, node, section);
      switch (section) {
        case ExtractSection.Entry:
          return "(extract(";
        case ExtractSection.Exit:
          return isMillisecond ? ") % 1000)" : "))";
        default:
          return base.Translate(context, node, section);
      }
    }

    /// <inheritdoc/>
    public override string Translate(SqlCompilerContext context, SqlFunctionCall node,
      FunctionCallSection section, int position)
    {
      switch (section) {
        case FunctionCallSection.Entry:
          switch (node.FunctionType) {
            case SqlFunctionType.LastAutoGeneratedId:
            case SqlFunctionType.CurrentUser:
            case SqlFunctionType.SessionUser:
            case SqlFunctionType.SystemUser:
            case SqlFunctionType.User:
            case SqlFunctionType.CurrentDate:
            case SqlFunctionType.CurrentTimeStamp:
              return Translate(node.FunctionType);
          }
          break;
        case FunctionCallSection.ArgumentEntry:
          return string.Empty;
        case FunctionCallSection.ArgumentDelimiter:
          return ArgumentDelimiter;
      }

      return base.Translate(context, node, section, position);
    }

    /// <inheritdoc/>
    public override string Translate(SqlCompilerContext context, SqlNextValue node, NodeSection section)
    {
      throw new NotSupportedException(Strings.ExDoesNotSupportSequences);
    }

    /// <inheritdoc/>
    public override string Translate(SqlCompilerContext context, SqlCast node, NodeSection section)
    {
      var sqlType = node.Type.Type;

      if (sqlType==SqlType.Binary ||
        sqlType==SqlType.Char ||
        sqlType==SqlType.Interval ||
        sqlType==SqlType.DateTime)
        switch (section) {
        case NodeSection.Entry:
          return "CAST(";
        case NodeSection.Exit:
          return "AS " + Translate(node.Type) + ")";
        default:
          throw new ArgumentOutOfRangeException("section");
        }
      if (sqlType==SqlType.Int16 ||
        sqlType==SqlType.Int32)
        switch (section) {
        case NodeSection.Entry:
          return "CAST(";
        case NodeSection.Exit:
          return "AS SIGNED " + Translate(node.Type) + ")";
        default:
          throw new ArgumentOutOfRangeException("section");
        }
      if (sqlType==SqlType.Decimal) {
        switch (section) {
          case NodeSection.Entry:
            return "CAST(";
          case NodeSection.Exit:
            return "AS " + Translate(node.Type) + ")";
          default:
            throw new ArgumentOutOfRangeException("section");
        }
      }
      if (sqlType==SqlType.Decimal ||
        sqlType==SqlType.Double ||
        sqlType==SqlType.Float)
        switch (section) {
        case NodeSection.Entry:
          return String.Empty;
        case NodeSection.Exit:
          return String.Empty;
        default:
          throw new ArgumentOutOfRangeException("section");
        }
      return string.Empty;
    }

    /// <inheritdoc/>
    public override string Translate(SqlDateTimePart part)
    {
      switch (part) {
        case SqlDateTimePart.Millisecond:
          return "MICROSECOND";
        case SqlDateTimePart.Day:
          return "DAY";
        case SqlDateTimePart.Year:
          return "YEAR";
        case SqlDateTimePart.Month:
          return "MONTH";
        case SqlDateTimePart.Hour:
          return "HOUR";
        case SqlDateTimePart.Minute:
          return "MINUTE";
      }

      return base.Translate(part);
    }

    /// <inheritdoc/>
    public override string Translate(SqlLockType lockType)
    {
      if (lockType.Supports(SqlLockType.Shared))
        return "LOCK IN SHARE MODE";
      if (lockType.Supports(SqlLockType.SkipLocked) || lockType.Supports(SqlLockType.ThrowIfLocked))
        return base.Translate(lockType);
      return "FOR UPDATE";
    }

    //----------------------- Other Vendor Type MySQL Type
    //BOOL TINYINT
    //BOOLEAN TINYINT
    //CHARACTER VARYING(M) VARCHAR(M)
    //FIXED DECIMAL
    //FLOAT4 FLOAT
    //FLOAT8 DOUBLE
    //INT1 TINYINT
    //INT2 SMALLINT
    //INT3 MEDIUMINT
    //INT4 INT
    //INT8 BIGINT
    //LONG VARBINARY MEDIUMBLOB
    //LONG VARCHAR MEDIUMTEXT
    //LONG MEDIUMTEXT
    //MIDDLEINT MEDIUMINT
    //NUMERIC DECIMAL
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
            return "numeric";
          if (type==typeof (Guid))
            return "text";
          return "text";
      }
    }

    public virtual string Translate(SqlCompilerContext context, SqlRenameColumn action) //TODO: Work on this.
    {
      string schemaName = action.Column.Table.Schema.DbName;
      string tableName = action.Column.Table.DbName;
      string columnName = action.Column.DbName;

      //alter table `actor` change column last_name1 last_name varchar(45)

      var builder = new StringBuilder();
      builder.Append("ALTER TABLE ");
      builder.Append(QuoteIdentifier(tableName));
      builder.Append(" CHANGE COLUMN ");
      builder.Append(QuoteIdentifier(columnName));
      builder.Append(" ");
      builder.Append(QuoteIdentifier(action.NewName));
      builder.Append(" ");
      builder.Append(Translate(action.Column.DataType));

      return builder.ToString();
    }

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}