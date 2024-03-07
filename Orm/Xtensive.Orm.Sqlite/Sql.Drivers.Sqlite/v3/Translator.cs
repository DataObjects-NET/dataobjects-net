// Copyright (C) 2011-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Malisa Ncube
// Created:    2011.04.29

using System.Text;
using Xtensive.Core;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Index = Xtensive.Sql.Model.Index;

namespace Xtensive.Sql.Drivers.Sqlite.v3
{
  internal class Translator : SqlTranslator
  {
    private const string DateTimeCastFormat = "%Y-%m-%d %H:%M:%f";

    /// <inheritdoc/>
    public override string DateTimeFormatString => @"\'yyyy\-MM\-dd HH\:mm\:ss.fff\'";

    /// <inheritdoc/>
    public override string DateOnlyFormatString => @"\'yyyy\-MM\-dd\'";

    /// <inheritdoc/>
    public override string TimeOnlyFormatString => @"\'HH\:mm\:ss.fffffff\'";

    public virtual string DateTimeOffsetFormatString => @"\'yyyy\-MM\-dd HH\:mm\:ss.fffK\'";

    /// <inheritdoc/>
    public override string TimeSpanFormatString => @"{0}{1}";

    /// <inheritdoc/>
    public override string DdlStatementDelimiter => ";";

    /// <inheritdoc/>
    public override void Initialize()
    {
      base.Initialize();
      FloatNumberFormat.NumberDecimalSeparator = ".";
      DoubleNumberFormat.NumberDecimalSeparator = ".";
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SchemaNode node) =>
      TranslateIdentifier(context.Output, node.DbName);

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlFunctionType functionType)
    {
      switch (functionType) {
        case SqlFunctionType.Acos:
        case SqlFunctionType.Asin:
        case SqlFunctionType.Atan:
        case SqlFunctionType.Atan2:
        case SqlFunctionType.Sin:
        case SqlFunctionType.SessionUser:
        case SqlFunctionType.Sqrt:
        case SqlFunctionType.Square:
        case SqlFunctionType.Tan:
        case SqlFunctionType.Position:
        case SqlFunctionType.Power:
          throw SqlHelper.NotSupported(functionType.ToString());
        case SqlFunctionType.Concat: _ = output.Append("||"); break;
        case SqlFunctionType.IntervalAbs: _ = output.Append("ABS"); break;
        case SqlFunctionType.Substring: _ = output.Append("SUBSTR"); break;
        case SqlFunctionType.IntervalNegate: _ = output.Append("-"); break;
        case SqlFunctionType.CurrentDate: _ = output.Append("DATE()"); break;
        case SqlFunctionType.BinaryLength: _ = output.Append("LENGTH"); break;
        case SqlFunctionType.LastAutoGeneratedId: _ = output.Append("LAST_INSERT_ROWID()"); break;
        case SqlFunctionType.DateTimeAddMonths: _ = output.Append("DATE"); break;
        case SqlFunctionType.DateTimeConstruct: _ = output.Append("DATETIME"); break;
        default: base.Translate(output, functionType); break;
      };
    }

    /// <inheritdoc/>
    public override string TranslateToString(SqlFunctionType functionType)
    {
      return functionType switch {
        SqlFunctionType.Acos or
          SqlFunctionType.Asin or
          SqlFunctionType.Atan or
          SqlFunctionType.Atan2 or
          SqlFunctionType.Sin or
          SqlFunctionType.SessionUser or
          SqlFunctionType.Sqrt or
          SqlFunctionType.Square or
          SqlFunctionType.Tan or
          SqlFunctionType.Position or
          SqlFunctionType.Power => throw SqlHelper.NotSupported(functionType.ToString()),
        _ => base.TranslateToString(functionType),
      };
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, object literalValue)
    {
      var output = context.Output;
      switch (literalValue) {
        case bool v:
          _ = output.Append(v ? '1' : '0');
          break;
        case byte[] values:
          TranslateByteArray(output.StringBuilder, values);
          break;
        case Guid guid:
          TranslateByteArray(output.StringBuilder, guid.ToByteArray());
          break;
        case TimeSpan timeSpan:
          _ = output.Append(timeSpan.Ticks * 100);
          break;
        case DateTimeOffset dt:
          _ = output.Append(dt.ToString(DateTimeOffsetFormatString, DateTimeFormat));
          break;
        default:
          base.Translate(context, literalValue);
          break;
      }
    }

    private void TranslateByteArray(StringBuilder sb, byte[] bytes)
    {
      _ = sb.EnsureCapacity(sb.Length + bytes.Length * 2 + 3);
      _ = sb.Append("x'")
        .AppendHexArray(bytes)
        .Append('\'');
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlAlterTable node, AlterTableSection section)
    {
      switch (section) {
        case AlterTableSection.Entry:
          _ = context.Output.Append("ALTER TABLE ");
          Translate(context, node.Table);
          break;
        case AlterTableSection.AddColumn:
          _ = context.Output.Append("ADD");
          break;
        case AlterTableSection.Exit:
          break;
        default:
          throw SqlHelper.NotSupported(node.Action.GetType().Name);
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, Constraint constraint, ConstraintSection section)
    {
      switch (section) {
        case ConstraintSection.Exit:
          if (constraint is ForeignKey fk) {
            if (fk.OnUpdate == ReferentialAction.Cascade) {
              _ = context.Output.Append(") ON UPDATE CASCADE");
              return;
            }
            if (fk.OnDelete == ReferentialAction.Cascade) {
              _ = context.Output.Append(") ON DELETE CASCADE");
              return;
            }
          }
          _ = context.Output.Append(")");
          break;
        default:
          base.Translate(context, constraint, section);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SequenceDescriptor descriptor, SequenceDescriptorSection section)
    {
      //switch (section) {
      //  case SequenceDescriptorSection.Increment:
      //    if (descriptor.Increment.HasValue)
      //      return "AUTOINCREMENT";
      //    return string.Empty;
      //}
    }

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
    public override void Translate(SqlCompilerContext context, SqlCreateView node, NodeSection section)
    {
      var output = context.Output;
      switch (section) {
        case NodeSection.Entry:
          if (node.View.ViewColumns.Count > 0) {
            _ = output.Append(" (");
            var first = true;
            foreach (DataTableColumn c in node.View.ViewColumns) {
              if (first) {
                first = false;
              }
              else {
                _ = output.Append(ColumnDelimiter);
              }

              _ = output.Append(c.DbName);
            }
            _ = output.Append(")");
          }
          break;
        case NodeSection.Exit:
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlDropSchema node) =>
      throw SqlHelper.NotSupported(node.GetType().Name);

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlDropTable node)
    {
      _ = context.Output.Append("DROP TABLE IF EXISTS ");
      Translate(context, node.Table);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlDropView node)
    {
      _ = context.Output.Append("DROP VIEW IF EXISTS ");
      Translate(context, node.View);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlFunctionCall node, FunctionCallSection section, int position)
    {
      if (node.FunctionType == SqlFunctionType.LastAutoGeneratedId) {
        if (section == FunctionCallSection.Entry) {
          Translate(context.Output, node.FunctionType);
          return;
        }
        if (section == FunctionCallSection.Exit) {
          return;
        }
      }
      switch (section) {
        case FunctionCallSection.ArgumentEntry:
          break;
        case FunctionCallSection.ArgumentDelimiter:
          _ = context.Output.Append(ArgumentDelimiter);
          break;
        default:
          base.Translate(context, node, section, position);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlUpdate node, UpdateSection section)
    {
      _ = context.Output.Append(section switch {
        UpdateSection.Entry => "UPDATE",
        UpdateSection.Set => "SET",
        UpdateSection.From => "FROM",
        UpdateSection.Where => "WHERE",
        _ => string.Empty
      });
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlCreateIndex node, CreateIndexSection section)
    {
      var index = node.Index;
      switch (section) {
        case CreateIndexSection.Entry:
          var output = context.Output;
          _ = index.IsUnique
            ? output.Append("CREATE UNIQUE INDEX ")
            : output.Append("CREATE INDEX ");
          TranslateIdentifier(output, index.Name);
          _ = output.Append(" ON ");
          TranslateIdentifier(output, index.DataTable.Name);
          _ = output.AppendSpace();
          return;
        case CreateIndexSection.Exit:
          return;
        default:
          base.Translate(context, node, section);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlDropIndex node)
    {
      var index = node.Index;
      _ = context.Output.Append("DROP INDEX ");
      TranslateIdentifier(context.Output, index.DataTable.Schema.DbName, index.DbName);
    }

    /// <inheritdoc/>
    public override string Translate(SqlJoinMethod method)
    {
      return method switch {
        SqlJoinMethod.Hash => "HASH",
        SqlJoinMethod.Merge => "MERGE",
        SqlJoinMethod.Loop => "LOOP",
        SqlJoinMethod.Remote => "REMOTE",
        _ => string.Empty,
      };
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlDateTimePart dateTimePart)
    {
      switch (dateTimePart) {
        case SqlDateTimePart.Year: _ = output.Append("'%Y'"); break;
        case SqlDateTimePart.Month: _ = output.Append("'%m'"); break;
        case SqlDateTimePart.Day: _ = output.Append("'%d'"); break;
        case SqlDateTimePart.DayOfWeek: _ = output.Append("'%w'"); break;
        case SqlDateTimePart.DayOfYear: _ = output.Append("'%j'"); break;
        case SqlDateTimePart.Hour: _ = output.Append("'%H'"); break;
        case SqlDateTimePart.Minute: _ = output.Append("'%M'"); break;
        case SqlDateTimePart.Second: _ = output.Append("'%S'"); break;
        default: base.Translate(output, dateTimePart); break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlDatePart dateTimePart)
    {
      switch (dateTimePart) {
        case SqlDatePart.Year: _ = output.Append("'%Y'"); break;
        case SqlDatePart.Month: _ = output.Append("'%m'"); break;
        case SqlDatePart.Day: _ = output.Append("'%d'"); break;
        case SqlDatePart.DayOfWeek: _ = output.Append("'%w'"); break;
        case SqlDatePart.DayOfYear: _ = output.Append("'%j'"); break;
        default: base.Translate(output, dateTimePart); break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlTimePart dateTimePart)
    {
      switch (dateTimePart) {
        case SqlTimePart.Hour: _ = output.Append("'%H'"); break;
        case SqlTimePart.Minute: _ = output.Append("'%M'"); break;
        case SqlTimePart.Second: _ = output.Append("'%S'"); break;
        default: base.Translate(output, dateTimePart); break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlIntervalPart intervalPart) => throw SqlHelper.NotSupported(intervalPart.ToString());

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlExtract extract, ExtractSection section)
    {
      _ = context.Output.Append(section switch {
        ExtractSection.Entry => "CAST(STRFTIME(",
        ExtractSection.From => ", ",
        ExtractSection.Exit => ") as INTEGER)",
        _ => string.Empty
      });
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlCast node, NodeSection section)
    {
      var output = context.Output;
      //http://www.sqlite.org/lang_expr.html
      var sqlType = node.Type.Type;

      if (sqlType == SqlType.DateTime ||
          sqlType == SqlType.DateTimeOffset) {
        _ = section switch {
          NodeSection.Entry => output.Append($"STRFTIME('{DateTimeCastFormat}', "),
          NodeSection.Exit => output.Append(")"),
          _ => throw new ArgumentOutOfRangeException(nameof(section)),
        };
      }
      else if (sqlType == SqlType.Binary ||
          sqlType == SqlType.Char ||
          sqlType == SqlType.Interval ||
          sqlType == SqlType.Int16 ||
          sqlType == SqlType.Int32 ||
          sqlType == SqlType.Int64) {
        _ = section switch {
          NodeSection.Entry => output.Append("CAST("),
          NodeSection.Exit => output.Append("AS ").Append(Translate(node.Type)).Append(")"),
          _ => throw new ArgumentOutOfRangeException(nameof(section)),
        };
      }
      else if (sqlType == SqlType.Decimal ||
          sqlType == SqlType.Double ||
          sqlType == SqlType.Float) {
        switch (section) {
          case NodeSection.Entry:
            break;
          case NodeSection.Exit:
            _ = output.Append("+ 0.0");
            break;
          default:
            throw new ArgumentOutOfRangeException(nameof(section));
        }
      }
    }

    /// <inheritdoc/>
    public virtual string Translate(SqlCompilerContext context, SqlRenameColumn action) =>
      throw SqlHelper.NotSupported(action.GetType().Name);

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlTrim node, TrimSection section)
    {
      var output = context.Output;
      _ = section switch {
        TrimSection.Entry => node.TrimType switch {
          SqlTrimType.Leading => output.AppendOpeningPunctuation("LTRIM("),
          SqlTrimType.Trailing => output.AppendOpeningPunctuation("RTRIM("),
          SqlTrimType.Both => output.AppendOpeningPunctuation("TRIM("),
          _ => throw new ArgumentOutOfRangeException(nameof(node.TrimType)),
        },
        TrimSection.Exit => node.TrimType switch {
          SqlTrimType.Leading or
            SqlTrimType.Trailing or
            SqlTrimType.Both => output.Append(")"),
          _ => throw new ArgumentOutOfRangeException(nameof(node.TrimType)),
        },
        _ => throw new ArgumentOutOfRangeException(nameof(section)),
      };
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, TableColumn column, TableColumnSection section)
    {
      switch (section) {
        case TableColumnSection.Type:
          if (column.SequenceDescriptor == null) {
            base.Translate(context, column, section);
          }
          else {
            _ = context.Output.Append("integer");   // SQLite requires autoincrement columns to have exactly 'integer' type.
          }
          break;
        case TableColumnSection.Exit:
          if (column.SequenceDescriptor == null) {
            return;
          }
          var primaryKey = column.Table.TableConstraints.OfType<PrimaryKey>().FirstOrDefault();
          if (primaryKey == null) {
            return;
          }
          _ = context.Output.Append("CONSTRAINT ");
          TranslateIdentifier(context.Output, primaryKey.Name);
          _ = context.Output.Append(" PRIMARY KEY AUTOINCREMENT");
          break;
        case TableColumnSection.GeneratedExit:
          break;
        default:
          base.Translate(context, column, section);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, Collation collation) => context.Output.Append(collation.DbName);

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlTrimType type) { }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlLockType lockType)
    {
      if (lockType.Supports(SqlLockType.Shared)) {
        _ = output.Append("SHARED");
        return;
      }
      if (lockType.Supports(SqlLockType.Exclusive)) {
        _ = output.Append("EXCLUSIVE");
        return;
      }
      if (lockType.Supports(SqlLockType.SkipLocked) || lockType.Supports(SqlLockType.ThrowIfLocked)) {
        base.Translate(output, lockType);
        return;
      }
      _ = output.Append("PENDING"); //http://www.sqlite.org/lockingv3.html Not sure whether this is the best alternative.
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlNodeType type)
    {
      switch (type) {
        case SqlNodeType.DateTimePlusInterval:
        case SqlNodeType.DateTimeOffsetPlusInterval:
        case SqlNodeType.TimePlusInterval:
          _ = output.Append("+");
          break;
        case SqlNodeType.DateTimeMinusInterval:
        case SqlNodeType.DateTimeMinusDateTime:
        case SqlNodeType.DateTimeOffsetMinusInterval:
        case SqlNodeType.DateTimeOffsetMinusDateTimeOffset:
        case SqlNodeType.TimeMinusTime:
          _ = output.Append("-");
          break;
        case SqlNodeType.Overlaps:
          throw SqlHelper.NotSupported(type.ToString());
        default:
          base.Translate(output, type);
          break;
      }
    }

    /// <inheritdoc/>
    public override string TranslateToString(SqlNodeType type)
    {
      return type switch {
        SqlNodeType.Overlaps => throw SqlHelper.NotSupported(type.ToString()),
        _ => base.TranslateToString(type),
      };
    }

    protected virtual string TranslateClrType(Type type)
    {
      switch (Type.GetTypeCode(type)) {
        case TypeCode.Boolean:
          return "bit";
        case TypeCode.Byte:
        case TypeCode.SByte:
        case TypeCode.Int16:
        case TypeCode.UInt16:
        case TypeCode.Int32:
        case TypeCode.UInt32:
          return "int";
        case TypeCode.Int64:
        case TypeCode.UInt64:
          return "bigint";
        case TypeCode.Decimal:
        case TypeCode.Single:
        case TypeCode.Double:
          return "numeric";
        case TypeCode.Char:
        case TypeCode.String:
          return "text";
        case TypeCode.DateTime:
          return "timestamp";
        default:
          if (type == typeof(TimeSpan)) {
            return "bigint";
          }
          if (type == typeof(Guid)) {
            return "guid";
          }
          return "text";
      }
    }

    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Translator"/> class.
    /// </summary>
    /// <param name="driver">The driver.</param>
    protected internal Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
