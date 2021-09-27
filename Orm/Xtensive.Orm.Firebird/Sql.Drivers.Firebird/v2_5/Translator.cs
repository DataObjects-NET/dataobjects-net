// Copyright (C) 2011-2021 Xtensive LLC.
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
    public override string DateTimeFormatString
    {
      get { return Constants.DateTimeFormatString; }
    }

    public override string TimeSpanFormatString
    {
      get { return string.Empty; }
    }

    public override SqlHelper.EscapeSetup EscapeSetup => SqlHelper.EscapeSetup.WithQuotes;

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SequenceDescriptor descriptor,
      SequenceDescriptorSection section)
    {
      switch (section) {
        case SequenceDescriptorSection.RestartValue when descriptor.StartValue.HasValue:
          context.Output.Append("RESTART WITH ").Append(descriptor.StartValue.Value);
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
          output.Append("ADD");
          break;
        case AlterTableSection.AlterColumn:
          output.Append("ALTER");
          break;
        case AlterTableSection.DropColumn:
          output.Append("DROP");
          break;
        case AlterTableSection.RenameColumn:
          output.Append("ALTER");
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
      context.Output.Append("DROP TABLE ");
      Translate(context, node.Table);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, object literalValue)
    {
      var output = context.Output;
      switch (literalValue) {
        case bool v:
          output.Append(v ? '1' : '0');
          break;
        case UInt64 v:
          TranslateString(output, v.ToString());
          break;
        case byte[] values:
          var builder = output.StringBuilder;
          builder.EnsureCapacity(builder.Length + 2 * (values.Length + 1));
          builder.Append("x'");
          builder.AppendHexArray(values);
          builder.Append("'");
          break;
        case Guid guid:
          TranslateString(output, SqlHelper.GuidToString(guid));
          break;
        case TimeSpan timeSpan:
          output.Append(timeSpan.Ticks * 100);
          break;
        default:
          base.Translate(context, literalValue);
          break;
      }
    }

    /// <inheritdoc/>
    public override string Translate(SqlNodeType type)
    {
      switch (type) {
        case SqlNodeType.Equals:
          return "=";
        case SqlNodeType.NotEquals:
          return "<>";
        case SqlNodeType.Modulo:
          return "MOD";
        case SqlNodeType.DateTimeMinusDateTime:
          return "-";
        case SqlNodeType.Except:
          throw SqlHelper.NotSupported(type.ToString());
        case SqlNodeType.Intersect:
          throw SqlHelper.NotSupported(type.ToString());
        case SqlNodeType.BitAnd:
          return "BIN_AND";
        case SqlNodeType.BitOr:
          return "BIN_OR";
        case SqlNodeType.BitXor:
          return "BIN_XOR";
        case SqlNodeType.Overlaps:
          throw SqlHelper.NotSupported(type.ToString());
        default:
          return base.Translate(type);
      }
    }

    /// <inheritdoc/>
    public override string Translate(SqlFunctionType type)
    {
      switch (type) {
        case SqlFunctionType.CharLength:
          return "CHAR_LENGTH";
        case SqlFunctionType.BinaryLength:
          return "OCTET_LENGTH";
        case SqlFunctionType.Truncate:
          return "TRUNC";
        case SqlFunctionType.IntervalNegate:
          return "-";
        case SqlFunctionType.Log:
          return "LN";
        case SqlFunctionType.Log10:
          return "LOG10";
        case SqlFunctionType.Ceiling:
          return "CEIL";
        case SqlFunctionType.PadLeft:
          return "LPAD";
        case SqlFunctionType.PadRight:
          return "RPAD";
        case SqlFunctionType.Concat:
          return "||";
        case SqlFunctionType.SystemUser:
        case SqlFunctionType.SessionUser:
          return base.Translate(SqlFunctionType.CurrentUser);
        case SqlFunctionType.Degrees:
        case SqlFunctionType.Radians:
        case SqlFunctionType.Square:
          throw SqlHelper.NotSupported(type.ToString());
        case SqlFunctionType.IntervalAbs:
          return Translate(SqlFunctionType.Abs);
        default:
          return base.Translate(type);
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlNextValue node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          context.Output.Append("GEN_ID(");
          break;
        case NodeSection.Exit:
          context.Output.Append(",")
            .Append(node.Increment)
            .Append(")");
          break;
      }
    }

    /// <inheritdoc/>
    public override string Translate(SqlLockType lockType)
    {
      if (lockType.Supports(SqlLockType.Shared) || lockType.Supports(SqlLockType.SkipLocked))
        return base.Translate(lockType);
      return "WITH LOCK";
    }

    /// <inheritdoc/>
    public override string Translate(SqlDateTimePart dateTimePart)
    {
      switch (dateTimePart) {
        case SqlDateTimePart.DayOfYear:
          return "YEARDAY";
        case SqlDateTimePart.DayOfWeek:
          return "WEEKDAY";
      }
      return base.Translate(dateTimePart);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlSelect node, SelectSection section)
    {
      switch (section) {
        case SelectSection.Limit:
          context.Output.Append("FIRST");
          break;
        case SelectSection.Offset:
          context.Output.Append("SKIP");
          break;
        default:
          base.Translate(context, node, section);
          break;
      }
    }

    public override void Translate(SqlCompilerContext context, SqlUpdate node, UpdateSection section)
    {
      switch (section) {
        case UpdateSection.Limit:
          context.Output.Append("ROWS");
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
          context.Output.Append("ROWS");
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
          Index index = node.Index;
          output.Append("CREATE ");
          if (index.IsUnique)
            output.Append("UNIQUE ");
          //else if (!index.IsAscending)
          //    builder.Append("DESC ");
          output.Append("INDEX ")
            .Append(QuoteIdentifier(index.DbName))
            .Append(" ON ");
          Translate(context, index.DataTable);
          return;
        case CreateIndexSection.ColumnsEnter:
          if (node.Index.Columns[0].Expression != null) {
            if (node.Index.Columns.Count > 1)
              SqlHelper.NotSupported("expression index with multiple column");
            output.Append("COMPUTED BY (");
          }
          else
            output.Append("(");
          return;
      }
      base.Translate(context, node, section);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, Constraint constraint, ConstraintSection section)
    {
      switch (section) {
        case ConstraintSection.Exit:
          context.Output.Append(")");
          if (constraint is ForeignKey fk) {
            if (fk.OnUpdate != ReferentialAction.NoAction)
              context.Output.Append(" ON UPDATE ").Append(Translate(fk.OnUpdate));
            if (fk.OnDelete != ReferentialAction.NoAction)
              context.Output.Append(" ON DELETE ").Append(Translate(fk.OnDelete));
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
          output.Append("SET GENERATOR ");
          Translate(context, node.Sequence);
          break;
        case NodeSection.Exit:
          output.Append("TO ")
            .Append(node.SequenceDescriptor.LastValue ?? 0);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlDropIndex node)
    {
      if (!node.Index.IsFullText)
        context.Output.Append("DROP INDEX ").Append(QuoteIdentifier(node.Index.DbName));
      else {
        context.Output.Append("DROP FULLTEXT INDEX ON ");
        Translate(context, node.Index.DataTable);
      }
    }

    public override void Translate(SqlCompilerContext context, SqlDropSequence node)
    {
      context.Output.Append("DROP SEQUENCE ");
      Translate(context, node.Sequence);
    }

    public override void Translate(SqlCompilerContext context, SqlQueryRef node, TableSection section)
    {
      if (!context.GetTraversalPath().Any(n => n.NodeType == SqlNodeType.Insert))
        base.Translate(context, node, section);
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
