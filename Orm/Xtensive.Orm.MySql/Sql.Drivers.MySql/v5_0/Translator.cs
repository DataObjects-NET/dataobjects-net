// Copyright (C) 2011-2023 Xtensive LLC.
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
    public override string DateTimeFormatString => @"\'yyyy\-MM\-dd HH\:mm\:ss\.ffffff\'";

    public override string DateOnlyFormatString => @"\'yyyy\-MM\-dd\'";

    public override string TimeOnlyFormatString => @"\'HH\:mm\:ss\.ffffff\'";

    public override string TimeSpanFormatString => string.Empty;

    public override string DdlStatementDelimiter => ";";


    public override void Initialize()
    {
      base.Initialize();

      FloatNumberFormat.NumberDecimalSeparator = ".";
      FloatNumberFormat.NumberGroupSeparator = "";

      DoubleNumberFormat.NumberDecimalSeparator = ".";
      DoubleNumberFormat.NumberGroupSeparator = "";
    }

    public override SqlHelper.EscapeSetup EscapeSetup => SqlHelper.EscapeSetup.WithBackTick;

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

    public override void SelectHintsEntry(SqlCompilerContext context, SqlSelect node) { }

    public override void SelectHintsExit(SqlCompilerContext context, SqlSelect node)
    {
      if (node.Hints.Count != 0) {
        var hints = new List<string>(node.Hints.Count);
        foreach (var hint in node.Hints) {
          if (hint is SqlNativeHint sqlNativeHint) {
            hints.Add(QuoteIdentifier(sqlNativeHint.HintText));
          }
        }
        if (hints.Count > 0) {
          _ = context.Output.Append("USE INDEX (")
            .Append(string.Join(", ", hints))
            .Append(")");
        }
      }
    }


    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlFunctionType type)
    {
      switch (type) {
        case SqlFunctionType.LastAutoGeneratedId: _ = output.Append("LAST_INSERT_ID()"); break;
        case SqlFunctionType.SystemUser: _ = output.Append("SYSTEM_USER()"); break;
        case SqlFunctionType.User:
        case SqlFunctionType.CurrentUser:
          _ = output.Append("CURRENT_USER()"); break;
        case SqlFunctionType.SessionUser: _ = output.Append("SESSION_USER()"); break;
        //datetime/timespan
        case SqlFunctionType.DateTimeTruncate: _ = output.Append("DATE"); break;
        case SqlFunctionType.CurrentDate: _ = output.Append("CURDATE()"); break;
        case SqlFunctionType.CurrentTimeStamp: _ = output.Append("NOW()"); break;
        case SqlFunctionType.IntervalNegate: _ = output.Append("-"); break;
        case SqlFunctionType.DateTimeAddYears:
        case SqlFunctionType.DateTimeAddMonths:
        case SqlFunctionType.DateTimeConstruct:
        case SqlFunctionType.DateAddYears:
        case SqlFunctionType.DateAddMonths:
        case SqlFunctionType.DateAddDays:
        case SqlFunctionType.DateConstruct:
        case SqlFunctionType.TimeConstruct:
        case SqlFunctionType.IntervalToMilliseconds:
          return;
        //string
        case SqlFunctionType.CharLength: _ = output.Append("CHAR_LENGTH"); break;
        case SqlFunctionType.Lower: _ = output.Append("LCASE"); break;
        case SqlFunctionType.Position: _ = output.Append("LOCATE"); break;
        case SqlFunctionType.Upper: _ = output.Append("UCASE"); break;
        case SqlFunctionType.Concat: _ = output.Append("CONCAT()"); break;
        //math
        case SqlFunctionType.Ceiling: _ = output.Append("CEIL"); break;
        case SqlFunctionType.Log: _ = output.Append("LN"); break;
        case SqlFunctionType.Log10: _ = output.Append("LOG"); break;
        case SqlFunctionType.Truncate: _ = output.Append("TRUNCATE"); break;
        default: base.Translate(output, type); break;
      };
    }

    /// <inheritdoc/>
    public override string TranslateToString(SqlFunctionType type)
    {
      return type switch {
        SqlFunctionType.User or SqlFunctionType.CurrentUser => "CURRENT_USER()",
        SqlFunctionType.CharLength => "CHAR_LENGTH",
        _ => base.TranslateToString(type),
      };
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SequenceDescriptor descriptor,
      SequenceDescriptorSection section)
    {
      //throw new NotSupportedException(Strings.ExDoesNotSupportSequences);
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, ReferentialAction action)
    {
      _ = output.Append(action switch {
        ReferentialAction.Cascade => "CASCADE",
        ReferentialAction.NoAction => "NO ACTION",
        ReferentialAction.Restrict => "RESTRICT",
        ReferentialAction.SetDefault => "SET DEFAULT",
        ReferentialAction.SetNull => "SET NULL",
        _ => string.Empty,
      });
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlNodeType type)
    {
       switch (type) {
        case SqlNodeType.Concat: _ = output.Append(","); break;
        case SqlNodeType.DateTimePlusInterval:
        case SqlNodeType.TimePlusInterval:
          _ = output.Append("+");
          break;
        case SqlNodeType.DateTimeMinusInterval:
        case SqlNodeType.DateTimeMinusDateTime:
        case SqlNodeType.TimeMinusTime:
          _ = output.Append("-"); break;
        case SqlNodeType.Equals: _ = output.Append("="); break;
        case SqlNodeType.NotEquals: _ = output.Append("<>"); break;
        case SqlNodeType.Modulo: _ = output.Append("MOD"); break;
        case SqlNodeType.Intersect:
        case SqlNodeType.Except:
          throw SqlHelper.NotSupported(type.ToString());
        case SqlNodeType.BitAnd: _ = output.Append("&"); break;
        case SqlNodeType.BitOr: _ = output.Append("|"); break;
        case SqlNodeType.BitXor: _ = output.Append("^"); break;
        case SqlNodeType.Overlaps: throw SqlHelper.NotSupported(type.ToString());
        default: base.Translate(output, type); break;
      };
    }

    /// <inheritdoc/>
    public override string TranslateToString(SqlNodeType type)
    {
      return type switch {
        SqlNodeType.Modulo => "MOD",
        SqlNodeType.Intersect or SqlNodeType.Except => throw SqlHelper.NotSupported(type.ToString()),
        SqlNodeType.Overlaps => throw SqlHelper.NotSupported(type.ToString()),
        _ => base.TranslateToString(type),
      };
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

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlConcat node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          _ = context.Output.AppendOpeningPunctuation("CONCAT(");
          break;
        case NodeSection.Exit:
          _ = context.Output.Append(")");
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SchemaNode node) =>
      base.Translate(context, node);

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlCreateTable node, CreateTableSection section)
    {
      var output = context.Output;
      switch (section) {
        case CreateTableSection.Entry:
          _ = output.Append("CREATE ");
          if (node.Table is TemporaryTable temporaryTable) {
            _ = output.Append("TEMPORARY TABLE ");
            Translate(context, temporaryTable);
          }
          else {
            _ = output.Append("TABLE ");
            Translate(context, node.Table);
          }
          return;
        case CreateTableSection.Exit:
          return;
      }
      base.Translate(context, node, section);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, TableColumn column, TableColumnSection section)
    {
      switch (section) {
        case TableColumnSection.Exit:
          break;
        case TableColumnSection.GeneratedExit:
          _ = context.Output.Append("AUTO_INCREMENT"); //Workaround based on fake sequence.
          break;
        default:
          base.Translate(context, column, section);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlCreateIndex node, CreateIndexSection section)
    {
      var index = node.Index;
      switch (section) {
        case CreateIndexSection.Entry:
          var output = context.Output;
          if (index.IsUnique) {
            _ = output.Append("CREATE UNIQUE INDEX ");
          }
          else if (index.IsFullText) {
            _ = output.Append("CREATE FULLTEXT INDEX");
          }
          else {
            _ = output.Append("CREATE INDEX ");
          }
          TranslateIdentifier(output, index.Name);
          _ = output.Append(" USING BTREE ON ");
          Translate(context, index.DataTable);
          _ = output.AppendSpace();
          break;
        case CreateIndexSection.Exit:
          break;
        default:
          base.Translate(context, node, section);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlDropIndex node)
    {
      var output = context.Output;
      _ = output.Append("DROP INDEX ");
      TranslateIdentifier(output, node.Index.Name);
      _ = output.Append(" ON ");
      TranslateIdentifier(output, node.Index.DataTable.Name);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, Constraint constraint, ConstraintSection section)
    {
      switch (section) {
        case ConstraintSection.Entry:
          if (constraint is PrimaryKey) {
            return;
          }
          base.Translate(context, constraint, section);
          break;
        case ConstraintSection.Exit:
          var output = context.Output;
          _ = output.Append(")");
          if (constraint is ForeignKey fk) {
            if (fk.OnUpdate != ReferentialAction.NoAction) {
              _ = output.Append(" ON UPDATE ");
              Translate(output, fk.OnUpdate);
            }

            if (fk.OnDelete != ReferentialAction.NoAction) {
              _ = output.Append(" ON DELETE ");
              Translate(output, fk.OnDelete);
            }
          }
          break;
        default:
          base.Translate(context, constraint, section);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlAlterTable node, AlterTableSection section)
    {
      switch (section) {
        case AlterTableSection.DropBehavior:
          if (node.Action is not SqlCascadableAction cascadableAction || !cascadableAction.Cascade) {
            return;
          }
          if (cascadableAction is SqlDropConstraint) {
            return;
          }
          _ = context.Output.Append(cascadableAction.Cascade ? "CASCADE" : "RESTRICT");
          break;
        default:
          base.Translate(context, node, section);
          break;
      }
    }

    public override void InsertDefaultValues(SqlCompilerContext context) =>
      context.Output.AppendSpaceIfNecessary().AppendOpeningPunctuation("() VALUES ()");

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlBreak node)
    {
      //throw new NotSupportedException(Strings.ExCursorsOnlyForProcsAndFuncs);
    }

    /// <inheritdoc/>
    public override string Translate(SqlValueType type)
    {
      // we need to explicitly specify maximum interval precision
      return type.Type == SqlType.Interval
        ? "decimal(18, 18)"
        : base.Translate(type);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, object literalValue)
    {
      var output = context.Output;
      switch (literalValue) {
        case bool v:
          _ = output.Append(v ? '1' : '0');
          break;
        case ulong v:
          TranslateString(output, v.ToString());
          break;
        case byte[] values:
          var builder = output.StringBuilder;
          _ = builder.EnsureCapacity(builder.Length + (2 * (values.Length + 1)));
          _ = builder.Append("x'")
            .AppendHexArray(values)
            .Append('\'');
          break;
        case Guid guid:
          TranslateString(output, SqlHelper.GuidToString(guid));
          break;
        case TimeSpan timeSpan:
          _ = output.Append(timeSpan.Ticks * 100);
          break;
        default:
          base.Translate(context, literalValue);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlExtract node, ExtractSection section)
    {
      var isSecond = node.IsSecondExtraction;
      var isMillisecond = node.IsMillisecondExtraction;
      if (!(isSecond || isMillisecond)) {
        base.Translate(context, node, section);
        return;
      }
      switch (section) {
        case ExtractSection.Entry:
          _ = context.Output.AppendOpeningPunctuation("(extract(");
          break;
        case ExtractSection.Exit:
          _ = context.Output.Append(isMillisecond ? ") / 1000)" : "))");
          break;
        default:
          base.Translate(context, node, section);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlFunctionCall node,
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
              Translate(context.Output, node.FunctionType);
              return;
          }
          break;
        case FunctionCallSection.ArgumentEntry:
          return;
        case FunctionCallSection.ArgumentDelimiter:
          _ = context.Output.Append(ArgumentDelimiter);
          return;
      }
      base.Translate(context, node, section, position);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlNextValue node, NodeSection section) =>
      throw new NotSupportedException(Strings.ExDoesNotSupportSequences);

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlCast node, NodeSection section)
    {
      var output = context.Output;
      var sqlType = node.Type.Type;

      if (sqlType == SqlType.Binary ||
        sqlType == SqlType.Char ||
        sqlType == SqlType.Interval ||
        sqlType == SqlType.DateTime) {
        _ = section switch {
          NodeSection.Entry => output.AppendOpeningPunctuation("CAST("),
          NodeSection.Exit => output.Append("AS ").Append(Translate(node.Type)).Append(")"),
          _ => throw new ArgumentOutOfRangeException("section"),
        };
      }
      else if (sqlType == SqlType.Int16 || sqlType == SqlType.Int32) {
        _ = section switch {
          NodeSection.Entry => output.AppendOpeningPunctuation("CAST("),
          NodeSection.Exit => output.Append("AS SIGNED ").Append(Translate(node.Type)).Append(")"),
          _ => throw new ArgumentOutOfRangeException(nameof(section)),
        };
      }
      else if (sqlType == SqlType.Decimal) {
        _ = section switch {
          NodeSection.Entry => output.AppendOpeningPunctuation("CAST("),
          NodeSection.Exit => output.Append("AS ").Append(Translate(node.Type)).Append(")"),
          _ => throw new ArgumentOutOfRangeException(nameof(section)),
        };
      }
      else if (sqlType == SqlType.Decimal ||
        sqlType == SqlType.Double ||
        sqlType == SqlType.Float) {
        switch (section) {
          case NodeSection.Entry:
          case NodeSection.Exit:
            break;
          default:
            throw new ArgumentOutOfRangeException(nameof(section));
        }
      }
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlDateTimePart dateTimePart)
    {
      switch (dateTimePart) {
        case SqlDateTimePart.Millisecond: _ = output.Append("MICROSECOND"); break;
        case SqlDateTimePart.Day: _ = output.Append("DAY"); break;
        case SqlDateTimePart.Year: _ = output.Append("YEAR"); break;
        case SqlDateTimePart.Month: _ = output.Append("MONTH"); break;
        case SqlDateTimePart.Hour: _ = output.Append("HOUR"); break;
        case SqlDateTimePart.Minute: _ = output.Append("MINUTE"); break;
        default: base.Translate(output, dateTimePart); break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlDatePart datePart)
    {
      switch (datePart) {
        case SqlDatePart.Day: _ = output.Append("DAY"); break;
        case SqlDatePart.Year: _ = output.Append("YEAR"); break;
        case SqlDatePart.Month: _ = output.Append("MONTH"); break;
        default: base.Translate(output, datePart); break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlTimePart dateTimePart)
    {
      switch (dateTimePart) {
        case SqlTimePart.Millisecond: _ = output.Append("MICROSECOND"); break;
        case SqlTimePart.Hour: _ = output.Append("HOUR"); break;
        case SqlTimePart.Minute: _ = output.Append("MINUTE"); break;
        default: base.Translate(output, dateTimePart); break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlLockType lockType)
    {
      if (lockType.Supports(SqlLockType.Shared)) {
        _ = output.Append("LOCK IN SHARE MODE");
      }
      else if (lockType.Supports(SqlLockType.SkipLocked) || lockType.Supports(SqlLockType.ThrowIfLocked)) {
        base.Translate(output, lockType);
      }
      else {
        _ = output.Append("FOR UPDATE");
      }
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
          if (type==typeof (TimeSpan)) {
            return "numeric";
          }
          return "text";
      }
    }

    public virtual void Translate(SqlCompilerContext context, SqlRenameColumn action) //TODO: Work on this.
    {
      var column = action.Column;
      var tableName = column.Table.DbName;

      //alter table `actor` change column last_name1 last_name varchar(45)

      var output = context.Output;
      _ = output.Append("ALTER TABLE ");
      TranslateIdentifier(output, tableName);
      _ = output.Append(" CHANGE COLUMN ");
      TranslateIdentifier(output, column.DbName);
      _ = output.AppendSpace();
      TranslateIdentifier(output, action.NewName);
      _ = output.AppendSpace()
        .Append(Translate(column.DataType));
    }

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}