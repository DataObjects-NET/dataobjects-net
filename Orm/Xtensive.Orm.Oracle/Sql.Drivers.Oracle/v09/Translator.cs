// Copyright (C) 2009-2020 Xtensive LLC.
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
    public override string NewLine { get { return "\n"; } }

    public override string BatchBegin { get { return "BEGIN\n"; } }
    public override string BatchEnd { get { return "END;\n"; } }

    public override string DateTimeFormatString { get { return @"'(TIMESTAMP '\'yyyy\-MM\-dd HH\:mm\:ss\.fff\'\)"; } }
    public override string TimeSpanFormatString { get { return "(INTERVAL '{0}{1} {2}:{3}:{4}.{5:000}' DAY(6) TO SECOND(3))"; } }
    public string DateTimeOffsetFormatString { get { return @"'(TIMESTAMP '\'yyyy\-MM\-dd HH\:mm\:ss\.fff\ zzz\'\)"; } }

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

    public override SqlHelper.EscapeSetup EscapeSetup => SqlHelper.EscapeSetup.WithQuotes;

    public override string QuoteString(string str)
    {
      return "N" + base.QuoteString(str);
    }

    public override void TranslateString(IOutput output, string str)
    {
      output.Append('N');
      base.TranslateString(output, str);
    }

    public override void Translate(SqlCompilerContext context, SqlSelect node, SelectSection section)
    {
      switch (section) {
        case SelectSection.HintsEntry:
          context.Output.Append("/*+");
          break;
        case SelectSection.HintsExit:
          context.Output.Append("*/");
          break;
        case SelectSection.Limit:
        case SelectSection.Offset:
          throw new NotSupportedException();
        default:
          base.Translate(context, node, section);
          break;
      }
    }

    public override string Translate(SqlJoinMethod method)
    {
      // TODO: add more hints
      switch (method) {
        case SqlJoinMethod.Loop:
          return "use_nl";
        case SqlJoinMethod.Merge:
          return "use_merge";
        case SqlJoinMethod.Hash:
          return "use_hash";
        default:
          return string.Empty;
      }
    }

    public override void Translate(SqlCompilerContext context, SqlNextValue node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Exit:
          context.Output.Append(".nextval");
          break;
      }
    }

    public override void Translate(SqlCompilerContext context, SqlExtract node, ExtractSection section)
    {
      if (node.DateTimePart == SqlDateTimePart.Second || node.IntervalPart == SqlIntervalPart.Second)
        switch (section) {
          case ExtractSection.Entry:
            context.Output.Append("TRUNC(EXTRACT(");
            return;
          case ExtractSection.Exit:
            context.Output.Append("))");
            return;
        }

      if (node.DateTimePart == SqlDateTimePart.Millisecond || node.IntervalPart == SqlIntervalPart.Millisecond)
        switch (section) {
          case ExtractSection.Entry:
            context.Output.Append("MOD(EXTRACT(");
            return;
          case ExtractSection.Exit:
            context.Output.Append(")*1000,1000)");
            return;
        }

      base.Translate(context, node, section);
    }

    public override void Translate(SqlCompilerContext context, SqlDropTable node)
    {
      context.Output.Append("DROP TABLE ");
      Translate(context, node.Table);
      context.Output.Append(node.Cascade ? " CASCADE CONSTRAINTS" : string.Empty);
    }

    public override void Translate(SqlCompilerContext context, SqlDropSequence node)
    {
      context.Output.Append("DROP SEQUENCE ");
      Translate(context, node.Sequence);
    }

    public override void Translate(SqlCompilerContext context, SqlAlterTable node, AlterTableSection section)
    {
      var output = context.Output;
      switch (section) {
        case AlterTableSection.AddColumn:
          output.Append("ADD");
          break;
        case AlterTableSection.DropBehavior:
          var cascadableAction = node.Action as SqlCascadableAction;
          if (cascadableAction == null || !cascadableAction.Cascade)
            return;
          if (cascadableAction is SqlDropConstraint) {
            output.Append("CASCADE");
          }
          else if (cascadableAction is SqlDropColumn) {
            output.Append("CASCADE CONSTRAINTS");
          }
          else {
            throw new ArgumentOutOfRangeException("node.Action");
          }
          break;
        default:
          base.Translate(context, node, section);
          break;
      }
    }

    public override void Translate(SqlCompilerContext context, object literalValue)
    {
      var output = context.Output;
      switch (literalValue) {
        case bool v:
          output.Append(v ? "1" : "0");
          break;
        case byte[] values:
          var builder = output.StringBuilder;
          builder.EnsureCapacity(builder.Length + 2 * (values.Length + 1));
          builder.Append("'");
          builder.AppendHexArray(values);
          builder.Append("'");
          break;
        case Guid guid:
          TranslateString(output, SqlHelper.GuidToString(guid));
          break;
        case DateTimeOffset dt:
          output.Append(dt.ToString(DateTimeOffsetFormatString));
          break;
        default:
          base.Translate(context, literalValue);
          break;
      }
    }

    public override void Translate(SqlCompilerContext context, SqlDropIndex node)
    {
      context.Output.Append("DROP INDEX ");
      Translate(context.Output, node.Index);
    }

    public override void Translate(SqlCompilerContext context, SqlCreateIndex node, CreateIndexSection section)
    {
      var output = context.Output;
      var index = node.Index;
      switch (section) {
        case CreateIndexSection.Entry:
          output.Append("CREATE ");
          if (index.IsUnique)
            output.Append("UNIQUE ");
          else if (index.IsBitmap)
            output.Append("BITMAP ");
          output.Append("INDEX ");
          Translate(output, index);
          output.Append(" ON ");
          Translate(context, index.DataTable);
          return;
        case CreateIndexSection.Exit:
          break;
        case CreateIndexSection.ColumnsEnter:
          output.Append("(");
          break;
        case CreateIndexSection.ColumnsExit:
          output.Append(")");
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
          throw new ArgumentOutOfRangeException("section");
      }
    }

    public virtual void Translate(IOutput output, Index node)
    {
      if (node.DataTable.Schema != null) {
        TranslateIdentifier(output, node.DataTable.Schema.DbName, node.DbName);
      }
      else {
        TranslateIdentifier(output, node.DbName);
      }
    }

    public override void Translate(SqlCompilerContext context, SqlFunctionCall node, FunctionCallSection section, int position)
    {
      var output = context.Output;
      switch (section) {
        case FunctionCallSection.Exit when node.FunctionType == SqlFunctionType.Log10:
          output.Append(", 10)");
          break;
        case FunctionCallSection.ArgumentEntry:
          break;
        case FunctionCallSection.ArgumentDelimiter:
          output.Append(ArgumentDelimiter);
          break;
        default:
          base.Translate(context, node, section, position);
          break;
      }
    }

    public override void Translate(SqlCompilerContext context, SequenceDescriptor descriptor, SequenceDescriptorSection section)
    {
      switch (section) {
        case SequenceDescriptorSection.RestartValue when descriptor.StartValue.HasValue:
          throw new NotSupportedException(Strings.ExAlterSequenceRestartWithIsNotSupported);
        case SequenceDescriptorSection.AlterMaxValue when !descriptor.MaxValue.HasValue:
          context.Output.Append("NOMAXVALUE");
          break;
        case SequenceDescriptorSection.AlterMinValue when !descriptor.MinValue.HasValue:
          context.Output.Append("NOMINVALUE");
          break;
        default:
          base.Translate(context, descriptor, section);
          break;
      }
    }

    public override void Translate(SqlCompilerContext context, SqlCast node, NodeSection section)
    {
      var sqlType = node.Type.Type;

      if (sqlType == SqlType.Char ||
        sqlType == SqlType.VarChar ||
        sqlType == SqlType.VarCharMax) {
        switch (section) {
          case NodeSection.Entry:
            context.Output.Append("TO_CHAR(");
            break;
          case NodeSection.Exit:
            context.Output.Append(")");
            break;
          default:
            throw new ArgumentOutOfRangeException("section");
        }
      }
      else {
        base.Translate(context, node, section);
      }
    }

    public override string Translate(SqlValueType type)
    {
      // we need to explicitly specify maximum interval precision
      if (type.Type == SqlType.Interval)
        return "INTERVAL DAY(6) TO SECOND(3)";
      return base.Translate(type);
    }

    public override string Translate(SqlDateTimePart part)
    {
      switch (part) {
        case SqlDateTimePart.DayOfWeek:
        case SqlDateTimePart.DayOfYear:
          throw new NotSupportedException();
        case SqlDateTimePart.Millisecond:
          return "SECOND";
        default:
          return base.Translate(part);
      }
    }

    public override string Translate(SqlIntervalPart part)
    {
      switch (part) {
        case SqlIntervalPart.Millisecond:
          return "SECOND";
        default:
          return base.Translate(part);
      }
    }

    public override string Translate(SqlFunctionType type)
    {
      switch (type) {
        case SqlFunctionType.Truncate:
        case SqlFunctionType.DateTimeTruncate:
          return "TRUNC";
        case SqlFunctionType.IntervalNegate:
          return "-1*";
        case SqlFunctionType.Substring:
          return "SUBSTR";
        case SqlFunctionType.Log:
          return "LN";
        case SqlFunctionType.Log10:
          return "LOG";
        case SqlFunctionType.Ceiling:
          return "CEIL";
        case SqlFunctionType.CurrentDateTimeOffset:
          return "CURRENT_TIMESTAMP";
        default:
          return base.Translate(type);
      }
    }

    public override string Translate(SqlNodeType type)
    {
      switch (type) {
        case SqlNodeType.DateTimeOffsetPlusInterval:
        case SqlNodeType.DateTimePlusInterval:
          return "+";
        case SqlNodeType.DateTimeOffsetMinusDateTimeOffset:
        case SqlNodeType.DateTimeOffsetMinusInterval:
        case SqlNodeType.DateTimeMinusInterval:
        case SqlNodeType.DateTimeMinusDateTime:
          return "-";
        case SqlNodeType.Except:
          return "MINUS";
        default:
          return base.Translate(type);
      }
    }

    public override string Translate(SqlLockType lockType)
    {
      if (lockType.Supports(SqlLockType.Shared) || lockType.Supports(SqlLockType.SkipLocked))
        return base.Translate(lockType);
      return lockType.Supports(SqlLockType.ThrowIfLocked)
        ? "FOR UPDATE NOWAIT"
        : "FOR UPDATE";
    }

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}