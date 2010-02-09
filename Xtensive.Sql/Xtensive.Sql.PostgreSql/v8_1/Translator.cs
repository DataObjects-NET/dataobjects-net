// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.PostgreSql.v8_1
{
  internal class Translator : v8_0.Translator
  {
    public override string Translate(SqlCompilerContext context, SqlContinue node)
    {
      return "CONTINUE";
    }

    public override string Translate(SqlCompilerContext context, SqlExtract node, ExtractSection section)
    {
      switch (node.IntervalPart) {
      case SqlIntervalPart.Day:
      case SqlIntervalPart.Hour:
        break;
      default:
        return base.Translate(context, node, section);
      }
      switch (section) {
      case ExtractSection.From:
        return "from justify_hours(";
      case ExtractSection.Exit:
        return "))";
      default:
        return base.Translate(context, node, section);
      }
    }

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }

  }
}