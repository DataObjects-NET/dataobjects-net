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
    public override string QuoteIdentifier(params string[] names)
    {
      return SqlHelper.QuoteIdentifierWithQuotes(names);
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

    public override string Translate(SqlValueType type)
    {
      // we need to explicitly specify maximum interval precision
      if (type.Type==SqlType.Interval)
        return "interval day(6) to second(9)";
      return base.Translate(type);
    }
    
    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}