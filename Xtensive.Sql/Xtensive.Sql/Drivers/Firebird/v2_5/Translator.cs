// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.17

using Xtensive.Sql.Compiler;
using System;
using System.Text;
using Xtensive.Core;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Model;


namespace Xtensive.Sql.Firebird.v2_5
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

    /// <inheritdoc/>
    public override string QuoteIdentifier(params string[] names)
    {
      return SqlHelper.QuoteIdentifierWithQuotes(names);
    }

    /// <inheritdoc/>
    public override string Translate(SqlCompilerContext context, SequenceDescriptor descriptor,
                                     SequenceDescriptorSection section)
    {
      switch (section) {
        case SequenceDescriptorSection.StartValue:
          return string.Empty;
        case SequenceDescriptorSection.RestartValue:
          if (descriptor.StartValue.HasValue)
            return "RESTART WITH " + descriptor.StartValue.Value;
          return string.Empty;
        case SequenceDescriptorSection.Increment:
          return string.Empty;
        case SequenceDescriptorSection.MaxValue:
          return string.Empty;
        case SequenceDescriptorSection.MinValue:
          return string.Empty;
        case SequenceDescriptorSection.AlterMaxValue:
          return string.Empty;
        case SequenceDescriptorSection.AlterMinValue:
          return string.Empty;
        case SequenceDescriptorSection.IsCyclic:
          return string.Empty;
        default:
          return string.Empty;
      }
    }

    /// <inheritdoc/>
    public override string Translate(SqlCompilerContext context, SqlAlterTable node, AlterTableSection section)
    {
      switch (section) {
        case AlterTableSection.RenameColumn:
          return "ALTER COLUMN";
      case AlterTableSection.DropBehavior:
        return string.Empty;
      }
      return base.Translate(context, node, section);
    }

    /// <inheritdoc/>
    public override string Translate(SqlCompilerContext context, SqlDropTable node)
    {
      return "DROP TABLE " + Translate(node.Table);
    }

    /// <inheritdoc/>
    public override string Translate(SqlCompilerContext context, object literalValue)
    {
      var literalType = literalValue.GetType();
      switch (Type.GetTypeCode(literalType)) {
        case TypeCode.Boolean:
          return (bool) literalValue ? "1" : "0";
        case TypeCode.UInt64:
          return QuoteString(((UInt64) literalValue).ToString());
      }
      if (literalType == typeof (byte[])) {
        var values = (byte[]) literalValue;
        var builder = new StringBuilder(2*(values.Length + 1));
        builder.Append("'");
        builder.AppendHexArray(values);
        builder.Append("'");
        return builder.ToString();
      }
      if (literalType == typeof (Guid))
        return QuoteString(SqlHelper.GuidToString((Guid) literalValue));
      if (literalType == typeof (TimeSpan))
        return Convert.ToString((long) ((TimeSpan) literalValue).Ticks*100);
      return base.Translate(context, literalValue);
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
        case SqlNodeType.BitAnd:
          return "BIT_AND";
        case SqlNodeType.BitOr:
          return "BIT_OR";
        case SqlNodeType.BitXor:
          return "BIT_XOR";
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
    public override string Translate(SqlCompilerContext context, SqlNextValue node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          return "GEN_ID(";
        case NodeSection.Exit:
          return "," + node.Increment + ")";
      }
      return string.Empty;
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
    public override string Translate(SqlCompilerContext context, SqlSelect node, SelectSection section)
    {
      switch (section) {
        case SelectSection.Limit:
          return "ROWS";
        case SelectSection.Offset:
          return "TO";
      }
      return base.Translate(context, node, section);
    }

    /// <inheritdoc/>
    public override string Translate(SqlCompilerContext context, SqlCreateIndex node, CreateIndexSection section)
    {
      switch (section) {
        case CreateIndexSection.Entry:
          Index index = node.Index;
          var builder = new StringBuilder();
          builder.Append("CREATE ");
          if (index.IsUnique)
            builder.Append("UNIQUE ");
          //else if (!index.IsAscending)
          //    builder.Append("DESC ");
          builder.Append("INDEX " + QuoteIdentifier(index.DbName));
          builder.Append(" ON " + Translate(index.DataTable));
          return builder.ToString();
        case CreateIndexSection.ColumnsEnter:
          if (node.Index.Columns[0].Expression != null) {
            if (node.Index.Columns.Count > 1)
              SqlHelper.NotSupported("expression index with multiple column");
            return "COMPUTED BY (";
          }
          else
            return "(";
      }
      return base.Translate(context, node, section);
    }

    /// <inheritdoc/>
    public override string Translate(SqlCompilerContext context, Constraint constraint, ConstraintSection section)
    {
      switch (section) {
        case ConstraintSection.Exit:
          ForeignKey fk = constraint as ForeignKey;
          StringBuilder sb = new StringBuilder();
          sb.Append(")");
          if (fk != null) {
            if (fk.OnUpdate != ReferentialAction.NoAction)
              sb.Append(" ON UPDATE " + Translate(fk.OnUpdate));
            if (fk.OnDelete != ReferentialAction.NoAction)
              sb.Append(" ON DELETE " + Translate(fk.OnDelete));
          }
          return sb.ToString();
      }
      return base.Translate(context, constraint, section);
    }

    public override string Translate(SqlCompilerContext context, SqlAlterSequence node, NodeSection section)
    {
      switch (section) {
        case NodeSection.Entry:
          return "SET GENERATOR " + Translate(node.Sequence);
        default:
          return "TO " + (node.SequenceDescriptor.LastValue.HasValue ? node.SequenceDescriptor.LastValue : 0);
      }

    }

    public override string Translate(SqlCompilerContext context, SqlDropSequence node)
    {
      return "DROP SEQUENCE " + Translate(node.Sequence);
    }

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}