// Copyright (C) 2011-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Csaba Beer
// Created:    2011.01.17

using System.Linq;
using Xtensive.Sql.Compiler;
using System;
using System.Text;
using Xtensive.Core;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Model;
using Index = Xtensive.Sql.Model.Index;


namespace Xtensive.Sql.Drivers.Firebird.v2_5
{
  internal class Translator : SqlTranslator
  {
    public override string DateTimeFormatString => Constants.DateTimeFormatString;

    public override string DateOnlyFormatString => Constants.DateFormatString;

    public override string TimeOnlyFormatString => Constants.TimeFormatString;

    public override string TimeSpanFormatString => string.Empty;

    public override SqlHelper.EscapeSetup EscapeSetup => SqlHelper.EscapeSetup.WithQuotes;

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SequenceDescriptor descriptor,
      SequenceDescriptorSection section)
    {
      switch (section) {
        case SequenceDescriptorSection.RestartValue when descriptor.StartValue.HasValue:
          _ = context.Output.Append("RESTART WITH ").Append(descriptor.StartValue.Value);
          break;
        case SequenceDescriptorSection.StartValue:
        case SequenceDescriptorSection.Increment:
        case SequenceDescriptorSection.MaxValue:
        case SequenceDescriptorSection.MinValue:
        case SequenceDescriptorSection.AlterMaxValue:
        case SequenceDescriptorSection.AlterMinValue:
        case SequenceDescriptorSection.IsCyclic:
        default:
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlAlterTable node, AlterTableSection section)
    {
      var output = context.Output;
      switch (section) {
        case AlterTableSection.AddColumn:
          _ = output.Append("ADD");
          break;
        case AlterTableSection.AlterColumn:
          _ = output.Append("ALTER");
          break;
        case AlterTableSection.DropColumn:
          _ = output.Append("DROP");
          break;
        case AlterTableSection.RenameColumn:
          _ = output.Append("ALTER");
          break;
        case AlterTableSection.DropBehavior:
          break;
        default:
          base.Translate(context, node, section);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlDropTable node)
    {
      _ = context.Output.Append("DROP TABLE ");
      Translate(context, node.Table);
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
          _ = builder.EnsureCapacity(builder.Length + 2 * (values.Length + 1));
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
    public override void Translate(IOutput output, SqlNodeType type)
    {
      switch (type) {
        case SqlNodeType.Equals: _ = output.Append("="); break;
        case SqlNodeType.NotEquals:
          _ = output.Append("<>"); break;
        case SqlNodeType.Modulo:
          _ = output.Append("MOD"); break;
        case SqlNodeType.DateTimeMinusDateTime:
        case SqlNodeType.TimeMinusTime:
          _ = output.Append("-"); break;
        case SqlNodeType.Except:
        case SqlNodeType.Intersect:
        case SqlNodeType.Overlaps:
          throw SqlHelper.NotSupported(type.ToString());
        case SqlNodeType.BitAnd:
          _ = output.Append("BIN_AND"); break;
        case SqlNodeType.BitOr:
          _ = output.Append("BIN_OR"); break;
        case SqlNodeType.BitXor:
          _ = output.Append("BIN_XOR"); break;
        default: base.Translate(output, type); break;
      };
    }

    /// <inheritdoc/>
    public override string TranslateToString(SqlNodeType type)
    {
      return type switch {
        SqlNodeType.Modulo => "MOD",
        SqlNodeType.Except => throw SqlHelper.NotSupported(type.ToString()),
        SqlNodeType.Intersect => throw SqlHelper.NotSupported(type.ToString()),
        SqlNodeType.Overlaps => throw SqlHelper.NotSupported(type.ToString()),
        _ => base.TranslateToString(type),
      };
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlFunctionType type)
    {
      switch (type) {
        case SqlFunctionType.CharLength: _ = output.Append("CHAR_LENGTH"); break;
        case SqlFunctionType.BinaryLength: _ = output.Append("OCTET_LENGTH"); break;
        case SqlFunctionType.Truncate: _ = output.Append("TRUNC"); break;
        case SqlFunctionType.IntervalNegate: _ = output.Append("-"); break;
        case SqlFunctionType.Log: _ = output.Append("LN"); break;
        case SqlFunctionType.Log10: _ = output.Append("LOG10"); break;
        case SqlFunctionType.Ceiling: _ = output.Append("CEIL"); break;
        case SqlFunctionType.PadLeft: _ = output.Append("LPAD"); break;
        case SqlFunctionType.PadRight: _ = output.Append("RPAD"); break;
        case SqlFunctionType.Concat: _ = output.Append("||"); break;
        case SqlFunctionType.SystemUser:
        case SqlFunctionType.SessionUser:
          base.Translate(output, SqlFunctionType.CurrentUser); break;
        case SqlFunctionType.Degrees:
        case SqlFunctionType.Radians:
        case SqlFunctionType.Square:
          throw SqlHelper.NotSupported(type.ToString());
        case SqlFunctionType.IntervalAbs: base.Translate(output, SqlFunctionType.Abs); break;
        default: base.Translate(output, type); break;
      };
    }

    /// <inheritdoc/>
    public override string TranslateToString(SqlFunctionType type)
    {
      return type switch {
        SqlFunctionType.CharLength => "CHAR_LENGTH",
        SqlFunctionType.SystemUser or SqlFunctionType.SessionUser => base.TranslateToString(SqlFunctionType.CurrentUser),
        _ => base.TranslateToString(type),
      };
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlNextValue node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          _ = context.Output.Append("GEN_ID(");
          break;
        case NodeSection.Exit:
          _ = context.Output.Append(",")
            .Append(node.Increment)
            .Append(")");
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlLockType lockType)
    {
      if (lockType.Supports(SqlLockType.Shared) || lockType.Supports(SqlLockType.SkipLocked)) {
        base.Translate(output, lockType);
      }
      else {
        _ = output.Append("WITH LOCK");
      }
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlDateTimePart dateTimePart)
    {
      switch (dateTimePart) {
        case SqlDateTimePart.DayOfYear: _ = output.Append("YEARDAY"); break;
        case SqlDateTimePart.DayOfWeek: _ = output.Append("WEEKDAY"); break;
        default: base.Translate(output, dateTimePart); break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlDatePart datePart)
    {
      switch (datePart) {
        case SqlDatePart.DayOfYear: _ = output.Append("YEARDAY"); break;
        case SqlDatePart.DayOfWeek: _ = output.Append("WEEKDAY"); break;
        default: base.Translate(output, datePart); break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlSelect node, SelectSection section)
    {
      switch (section) {
        case SelectSection.Limit:
          _ = context.Output.Append("FIRST");
          break;
        case SelectSection.Offset:
          _ = context.Output.Append("SKIP");
          break;
        default:
          base.Translate(context, node, section);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlUpdate node, UpdateSection section)
    {
      switch (section) {
        case UpdateSection.Limit:
          _ = context.Output.Append("ROWS");
          break;
        default:
          base.Translate(context, node, section);
          break;
      }
    }

    /// <inheritdoc />
    public override void Translate(SqlCompilerContext context, SqlDelete node, DeleteSection section)
    {
      switch (section) {
        case DeleteSection.Limit:
          _ = context.Output.Append("ROWS");
          return;
      }
      base.Translate(context, node, section);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlCreateIndex node, CreateIndexSection section)
    {
      var output = context.Output;
      switch (section) {
        case CreateIndexSection.Entry:
          var index = node.Index;
          _ = output.Append("CREATE ");
          if (index.IsUnique) {
            _ = output.Append("UNIQUE ");
          }
          //else if (!index.IsAscending)
          //    builder.Append("DESC ");
          _ = output.Append("INDEX ");
          TranslateIdentifier(output, index.DbName);
          _ = output.Append(" ON ");
          Translate(context, index.DataTable);
          return;
        case CreateIndexSection.ColumnsEnter:
          if (node.Index.Columns[0].Expression != null) {
            if (node.Index.Columns.Count > 1) {
              _ = SqlHelper.NotSupported("expression index with multiple column");
            }
            _ = output.Append("COMPUTED BY (");
          }
          else
            _ = output.Append("(");
          return;
      }
      base.Translate(context, node, section);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, Constraint constraint, ConstraintSection section)
    {
      switch (section) {
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
    public override void Translate(SqlCompilerContext context, SqlAlterSequence node, NodeSection section)
    {
      var output = context.Output;
      switch (section) {
        case NodeSection.Entry:
          _ = output.Append("SET GENERATOR ");
          Translate(context, node.Sequence);
          break;
        case NodeSection.Exit:
          _ = output.Append("TO ")
            .Append(node.SequenceDescriptor.LastValue ?? 0);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlDropIndex node)
    {
      if (!node.Index.IsFullText) {
        _ = context.Output.Append("DROP INDEX ");
        TranslateIdentifier(context.Output, node.Index.DbName);
      }
      else {
        _ = context.Output.Append("DROP FULLTEXT INDEX ON ");
        Translate(context, node.Index.DataTable);
      }
    }

    public override void Translate(SqlCompilerContext context, SqlDropSequence node)
    {
      _ = context.Output.Append("DROP SEQUENCE ");
      Translate(context, node.Sequence);
    }

    public override void Translate(SqlCompilerContext context, SqlQueryRef node, TableSection section)
    {
      if (!context.GetTraversalPath().Any(n => n.NodeType == SqlNodeType.Insert)) {
        base.Translate(context, node, section);
      }
    }

    // Constructors

    /// <inheritdoc/>
    public Translator(SqlDriver driver)
      : base(driver)
    {
      FloatFormatString = $"{base.FloatFormatString}e0";
      DoubleFormatString = $"{base.DoubleFormatString}e0";
    }
  }
}
