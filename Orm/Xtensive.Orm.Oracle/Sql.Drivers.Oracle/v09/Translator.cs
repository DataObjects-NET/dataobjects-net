// Copyright (C) 2009-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using System.Text;
using Xtensive.Core;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Sql.Drivers.Oracle.Resources;
using Index = Xtensive.Sql.Model.Index;

namespace Xtensive.Sql.Drivers.Oracle.v09
{
  internal class Translator : SqlTranslator
  {
    /// <inheritdoc/>
    public override string NewLine => "\n";

    /// <inheritdoc/>
    public override string BatchBegin => "BEGIN\n";

    /// <inheritdoc/>
    public override string BatchEnd => "END;\n";

    /// <inheritdoc/>
    public override string DateTimeFormatString => @"'(TIMESTAMP '\'yyyy\-MM\-dd HH\:mm\:ss\.fff\'\)";

    /// <inheritdoc/>
    public override string DateOnlyFormatString => @"'(DATE '\'yyyy\-MM\-dd\'\)";

    /// <inheritdoc/>
    public override string TimeOnlyFormatString => @"'(INTERVAL '\'0 HH\:mm\:ss\.fffffff\'\ DAY(0) TO SECOND(7))";

    /// <inheritdoc/>
    public override string TimeSpanFormatString => "(INTERVAL '{0}{1} {2}:{3}:{4}.{5:000}' DAY(6) TO SECOND(3))";

    public string DateTimeOffsetFormatString => @"'(TIMESTAMP '\'yyyy\-MM\-dd HH\:mm\:ss\.fff\ zzz\'\)";

    public override void Initialize()
    {
      base.Initialize();
      FloatNumberFormat.NumberDecimalSeparator = ".";
      FloatNumberFormat.NumberGroupSeparator = "";
      FloatNumberFormat.NaNSymbol = "BINARY_FLOAT_NAN";
      FloatNumberFormat.PositiveInfinitySymbol = "+BINARY_FLOAT_INFINITY";
      FloatNumberFormat.NegativeInfinitySymbol = "-BINARY_FLOAT_INFINITY";

      DoubleNumberFormat.NumberDecimalSeparator = ".";
      DoubleNumberFormat.NumberGroupSeparator = "";
      DoubleNumberFormat.NaNSymbol = "BINARY_DOUBLE_NAN";
      DoubleNumberFormat.PositiveInfinitySymbol = "+BINARY_DOUBLE_INFINITY";
      DoubleNumberFormat.NegativeInfinitySymbol = "-BINARY_DOUBLE_INFINITY";
    }

    /// <inheritdoc/>
    public override SqlHelper.EscapeSetup EscapeSetup => SqlHelper.EscapeSetup.WithQuotes;

    /// <inheritdoc/>
    public override string QuoteString(string str) => "N" + base.QuoteString(str);

    /// <inheritdoc/>
    public override void TranslateString(IOutput output, string str)
    {
      _ = output.Append('N');
      base.TranslateString(output, str);
    }

    public override void SelectLimit(SqlCompilerContext context, SqlSelect node) => throw new NotSupportedException();
    public override void SelectOffset(SqlCompilerContext context, SqlSelect node) => throw new NotSupportedException();

    public override void SelectHintsEntry(SqlCompilerContext context, SqlSelect node) =>
      context.Output.AppendSpacePrefixed("/*+ ");

    public override void SelectHintsExit(SqlCompilerContext context, SqlSelect node) =>
      context.Output.AppendSpacePrefixed("*/ ");

    /// <inheritdoc/>
    public override string Translate(SqlJoinMethod method)
    {
      // TODO: add more hints
      return method switch {
        SqlJoinMethod.Loop => "use_nl",
        SqlJoinMethod.Merge => "use_merge",
        SqlJoinMethod.Hash => "use_hash",
        _ => string.Empty,
      };
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlNextValue node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Exit:
          _ = context.Output.Append(".nextval");
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlExtract node, ExtractSection section)
    {
      if (node.IsSecondExtraction) {
        switch (section) {
          case ExtractSection.Entry:
            _ = context.Output.Append("TRUNC(EXTRACT(");
            return;
          case ExtractSection.Exit:
            _ = context.Output.Append("))");
            return;
        }
      }

      if (node.IsMillisecondExtraction) {
        switch (section) {
          case ExtractSection.Entry:
            _ = context.Output.Append("MOD(EXTRACT(");
            return;
          case ExtractSection.Exit:
            _ = context.Output.Append(")*1000,1000)");
            return;
        }
      }

      base.Translate(context, node, section);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlDropTable node)
    {
      _ = context.Output.Append("DROP TABLE ");
      Translate(context, node.Table);
      if (node.Cascade) {
        _ = context.Output.Append(" CASCADE CONSTRAINTS");
      }
    }

    public override void Translate(SqlCompilerContext context, SqlDropView node)
    {
      _ = context.Output.Append("DROP VIEW ");
      Translate(context, node.View);
      if (node.Cascade) {
        _ = context.Output.Append(" CASCADE CONSTRAINTS");
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlDropSequence node)
    {
      _ = context.Output.Append("DROP SEQUENCE ");
      Translate(context, node.Sequence);
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlAlterTable node, AlterTableSection section)
    {
      var output = context.Output;
      switch (section) {
        case AlterTableSection.AddColumn:
          _ = output.Append("ADD");
          break;
        case AlterTableSection.DropBehavior:
          if (node.Action is not SqlCascadableAction cascadableAction || !cascadableAction.Cascade)
            return;
          if (cascadableAction is SqlDropConstraint) {
            _ = output.Append("CASCADE");
          }
          else if (cascadableAction is SqlDropColumn) {
            _ = output.Append("CASCADE CONSTRAINTS");
          }
          else {
            throw new ArgumentOutOfRangeException(nameof(node.Action));
          }
          break;
        default:
          base.Translate(context, node, section);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, object literalValue)
    {
      var output = context.Output;
      switch (literalValue) {
        case bool v:
          _ = output.Append(v ? "1" : "0");
          break;
        case byte[] values:
          var builder = output.StringBuilder;
          _ = builder.EnsureCapacity(builder.Length + 2 * (values.Length + 1));
          _ = builder.Append('\'')
            .AppendHexArray(values)
            .Append('\'');
          break;
        case Guid guid:
          TranslateString(output, SqlHelper.GuidToString(guid));
          break;
        case DateTimeOffset dt:
          _ = output.Append(dt.ToString(DateTimeOffsetFormatString));
          break;
        default:
          base.Translate(context, literalValue);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlDropIndex node)
    {
      _ = context.Output.Append("DROP INDEX ");
      Translate(context.Output, node.Index);
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
            : index.IsBitmap
              ? output.Append("CREATE BITMAP INDEX ")
              : output.Append("CREATE INDEX ");
          Translate(context.Output, index);
          _ = output.Append(" ON ");
          Translate(context, index.DataTable);
          return;
        case CreateIndexSection.Exit:
          break;
        case CreateIndexSection.ColumnsEnter:
          _ = output.Append("(");
          break;
        case CreateIndexSection.ColumnsExit:
          _ = output.Append(")");
          break;
        case CreateIndexSection.NonkeyColumnsEnter:
          break;
        case CreateIndexSection.NonkeyColumnsExit:
          break;
        case CreateIndexSection.StorageOptions:
          break;
        case CreateIndexSection.Where:
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(section));
      }
    }

    /// <inheritdoc/>
    public virtual void Translate(IOutput output, Index node)
    {
      if (node.DataTable.Schema != null) {
        TranslateIdentifier(output, node.DataTable.Schema.DbName, node.DbName);
      }
      else {
        TranslateIdentifier(output, node.DbName);
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlFunctionCall node, FunctionCallSection section, int position)
    {
      var output = context.Output;
      switch (section) {
        case FunctionCallSection.Exit when node.FunctionType == SqlFunctionType.Log10:
          _ = output.Append(", 10)");
          break;
        case FunctionCallSection.ArgumentEntry:
          break;
        case FunctionCallSection.ArgumentDelimiter:
          _ = output.Append(ArgumentDelimiter);
          break;
        default:
          base.Translate(context, node, section, position);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SequenceDescriptor descriptor, SequenceDescriptorSection section)
    {
      switch (section) {
        case SequenceDescriptorSection.RestartValue when descriptor.StartValue.HasValue:
          throw new NotSupportedException(Strings.ExAlterSequenceRestartWithIsNotSupported);
        case SequenceDescriptorSection.AlterMaxValue when !descriptor.MaxValue.HasValue:
          _ = context.Output.Append("NOMAXVALUE");
          break;
        case SequenceDescriptorSection.AlterMinValue when !descriptor.MinValue.HasValue:
          _ = context.Output.Append("NOMINVALUE");
          break;
        default:
          base.Translate(context, descriptor, section);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(SqlCompilerContext context, SqlCast node, NodeSection section)
    {
      var sqlType = node.Type.Type;

      if (sqlType == SqlType.Char ||
        sqlType == SqlType.VarChar ||
        sqlType == SqlType.VarCharMax) {
        _ = section switch {
          NodeSection.Entry => context.Output.Append("TO_CHAR("),
          NodeSection.Exit => context.Output.Append(")"),
          _ => throw new ArgumentOutOfRangeException("section"),
        };
      }
      else {
        base.Translate(context, node, section);
      }
    }

    /// <inheritdoc/>
    public override string Translate(SqlValueType type)
    {
      // we need to explicitly specify maximum interval precision
      return type.Type == SqlType.Interval
        ? "INTERVAL DAY(6) TO SECOND(3)"
        : type.Type == SqlType.Time
          ? "INTERVAL DAY(0) TO SECOND(7)"
          : base.Translate(type);
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlDateTimePart dateTimePart)
    {
      switch (dateTimePart) {
        case SqlDateTimePart.DayOfWeek:
        case SqlDateTimePart.DayOfYear:
          throw new NotSupportedException();
        case SqlDateTimePart.Millisecond:
          _ = output.Append("SECOND");
          break;
        default:
          base.Translate(output, dateTimePart);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlDatePart datePart)
    {
      switch (datePart) {
        case SqlDatePart.DayOfWeek:
        case SqlDatePart.DayOfYear:
          throw new NotSupportedException();
        default:
          base.Translate(output, datePart);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlTimePart timePart)
    {
      if (timePart== SqlTimePart.Millisecond) {
        _ = output.Append("SECOND");
      }
      else {
        base.Translate(output, timePart);
      }
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlIntervalPart part)
    {
      if (part == SqlIntervalPart.Millisecond) {
        _ = output.Append("SECOND");
      }
      else {
        base.Translate(output, part);
      }
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlFunctionType type)
    {
      switch (type) {
        case SqlFunctionType.Truncate:
        case SqlFunctionType.DateTimeTruncate:
          _ = output.Append("TRUNC");
          break;
        case SqlFunctionType.IntervalNegate: _ = output.Append("-1*"); break;
        case SqlFunctionType.Substring: _ = output.Append("SUBSTR"); break;
        case SqlFunctionType.Log: _ = output.Append("LN"); break;
        case SqlFunctionType.Log10: _ = output.Append("LOG"); break;
        case SqlFunctionType.Ceiling: _ = output.Append("CEIL"); break;
        case SqlFunctionType.CurrentDateTimeOffset: _ = output.Append("CURRENT_TIMESTAMP"); break;
        default: base.Translate(output, type); break;
      }
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlNodeType type)
    {
      switch (type) {
        case SqlNodeType.DateTimeOffsetPlusInterval:
        case SqlNodeType.DateTimePlusInterval:
        case SqlNodeType.TimePlusInterval:
          _ = output.Append("+"); break;
        case SqlNodeType.DateTimeOffsetMinusDateTimeOffset:
        case SqlNodeType.DateTimeOffsetMinusInterval:
        case SqlNodeType.DateTimeMinusInterval:
        case SqlNodeType.DateTimeMinusDateTime:
          _ = output.Append("-"); break;
        case SqlNodeType.Except:
          _ = output.Append("MINUS"); break;
        default: base.Translate(output, type); break;
      };
    }

    /// <inheritdoc/>
    public override void Translate(IOutput output, SqlLockType lockType)
    {
      if (lockType.Supports(SqlLockType.Shared) || lockType.Supports(SqlLockType.SkipLocked)) {
        base.Translate(output, lockType);
      }
      else if (lockType.Supports(SqlLockType.ThrowIfLocked)) {
        _ = output.Append("FOR UPDATE NOWAIT");
      }
      else {
        _ = output.Append("FOR UPDATE");
      }
    }

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}