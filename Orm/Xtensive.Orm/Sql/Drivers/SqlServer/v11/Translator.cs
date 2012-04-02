// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.04.02

using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.SqlServer.v11
{
  internal class Translator : v10.Translator
  {
    public override string Translate(SqlCompilerContext context, SqlSelect node, SelectSection section)
    {
      switch (section) {
        case SelectSection.Limit:
          return "FETCH NEXT";
        case SelectSection.LimitEnd:
          return "ROWS ONLY";
        case SelectSection.Offset:
          return "OFFSET";
        case SelectSection.OffsetEnd:
          return "ROWS";
        default:
          return base.Translate(context, node, section);
      }
    }

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}