// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Oracle.v09
{
  internal class Translator : SqlTranslator
  {
    public override string DateTimeFormat { get { return @"'(TIMESTAMP '\'yyyy\-MM\-dd HH\:mm\:ss\.fff\'\)"; } }
    public override string TimeSpanFormat { get { return "(INTERVAL '{0}{1} {2}:{3}:{4}.{5:000}' DAY(6) TO SECOND(3))"; } }

    public override void Initialize()
    {
      base.Initialize();
      numberFormat.NumberDecimalSeparator = ".";
      numberFormat.NumberGroupSeparator = "";
      numberFormat.NaNSymbol = "BINARY_DOUBLE_NAN";
      numberFormat.PositiveInfinitySymbol = "+BINARY_DOUBLE_INFINITY";
      numberFormat.NegativeInfinitySymbol = "-BINARY_DOUBLE_INFINITY";
    }

    public override string QuoteIdentifier(params string[] names)
    {
      return SqlHelper.QuoteIdentifierWithQuotes(names);
    }

    public override string QuoteString(string str)
    {
      return "N" + base.QuoteString(str);
    }

    public override string Translate(SqlCompilerContext context, SqlSelect node, SelectSection section)
    {
      switch (section) {
      case SelectSection.HintsEntry:
        return "/*+";
      case SelectSection.HintsExit:
        return "*/";
      default:
        return base.Translate(context, node, section);
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

    public override string Translate(SqlCompilerContext context, SqlNextValue node, NodeSection section)
    {
      switch(section) {
      case NodeSection.Exit:
        return ".nextvalue";
      default:
        return string.Empty;
      }
    }

    public override string Translate(SqlCompilerContext context, SqlBinary node, NodeSection section)
    {
      if (node.NodeType==SqlNodeType.Modulo && section==NodeSection.Entry)
        return "MOD(";
      return base.Translate(context, node, section);
    }
    
    public override string Translate(SqlCompilerContext context, SqlExtract node, ExtractSection section)
    {
      if (node.DateTimePart==SqlDateTimePart.Second || node.IntervalPart==SqlIntervalPart.Second)
        switch (section) {
        case ExtractSection.Entry:
          return "TRUNC(EXTRACT(";
        case ExtractSection.Exit:
          return "))";
        default:
          return base.Translate(context, node, section);
        }

      if (node.DateTimePart==SqlDateTimePart.Millisecond || node.IntervalPart==SqlIntervalPart.Millisecond)
        switch (section) {
        case ExtractSection.Entry:
          return "MOD(EXTRACT(";
        case ExtractSection.Exit:
          return ")*1000,1000)";
        default:
          return base.Translate(context, node, section);
        }

      return base.Translate(context, node, section);
    }

    public override string Translate(SqlValueType type)
    {
      // we need to explicitly specify maximum interval precision
      if (type.Type==SqlType.Interval)
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
      default:
        return base.Translate(type);
      }
    }

    public override string Translate(SqlNodeType type)
    {
      switch (type) {
      case SqlNodeType.Modulo:
        return ",";
      case SqlNodeType.DateTimePlusInterval:
        return "+";
      case SqlNodeType.DateTimeMinusInterval:
      case SqlNodeType.DateTimeMinusDateTime:
        return "-";
      default:
        return base.Translate(type);
      }
    }

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}